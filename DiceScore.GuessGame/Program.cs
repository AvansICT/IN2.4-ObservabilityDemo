using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http.Json;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

// ──────────────────────────────────────────────
// Service identity
// ──────────────────────────────────────────────
const string ServiceName    = "DiceScore.GuessGame";
const string ServiceVersion = "1.0.0";
const string ApiUrl         = "http://localhost:5000/";

// ──────────────────────────────────────────────
// OpenTelemetry instrumentation objects
// Studenten gebruiken deze objecten in de TODOs hieronder.
// ──────────────────────────────────────────────
var activitySource = new ActivitySource(ServiceName);

var meter = new Meter(ServiceName, ServiceVersion);

// Tellers — studenten verhogen deze in de TODOs
var gamesPlayedCounter = meter.CreateCounter<long>("game.played",      description: "Aantal gespeelde ronden");
var gamesWonCounter    = meter.CreateCounter<long>("game.won",         description: "Aantal gewonnen ronden");
var wrongInputCounter  = meter.CreateCounter<long>("game.wrong_input", description: "Aantal ongeldige invoeringen");

// ──────────────────────────────────────────────
// OpenTelemetry provider setup (volledig werkend)
// ──────────────────────────────────────────────
var otlpEndpoint = new Uri("http://localhost:4317");

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
    .AddSource(ServiceName)
    .AddHttpClientInstrumentation()   // injecteert automatisch W3C traceparent-header op HttpClient-aanroepen
    .AddOtlpExporter(o => o.Endpoint = otlpEndpoint)
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
    .AddMeter(ServiceName)
    .AddOtlpExporter(o => o.Endpoint = otlpEndpoint)
    .Build();

// ──────────────────────────────────────────────
// HttpClient voor aanroepen naar DiceScore.API
// ──────────────────────────────────────────────
using var httpClient = new HttpClient();

// ──────────────────────────────────────────────
// Spellogica
// ──────────────────────────────────────────────
Console.WriteLine("=== DiceScore Raadspel ===");
Console.WriteLine("Raad de uitkomst van een dobbelsteenworp (1-6).");
Console.WriteLine("Typ 'stop' om te beëindigen.\n");

while (true)
{
    // TODO (stap 1): Start hier een nieuwe Activity/span voor de hele ronde.
    //   Gebruik: using var roundActivity = activitySource.StartActivity("game.round");
    //   De span omvat de volledige ronde: invoer, API-aanroep en uitkomst.

    // ── Invoer vragen ──────────────────────────────────────────────────────
    Console.Write("Jouw gok (1-6): ");
    var input = Console.ReadLine()?.Trim();

    if (string.Equals(input, "stop", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Tot ziens!");
        break;
    }

    if (!int.TryParse(input, out var guess) || guess < 1 || guess > 6)
    {
        Console.WriteLine("Ongeldige invoer. Voer een getal tussen 1 en 6 in.\n");

        // TODO (stap 2): Verhoog hier de wrongInputCounter.
        //   Gebruik: wrongInputCounter.Add(1);

        continue;
    }

    // TODO (stap 3): Sla de gok op als tag op de roundActivity span.
    //   Gebruik: roundActivity?.SetTag("game.guess", guess);

    // ── API aanroepen ──────────────────────────────────────────────────────
    int apiRoll;
    try
    {
        // TODO (stap 4): Optioneel — start hier een child-span voor de HTTP-aanroep.
        //   Gebruik: using var httpActivity = activitySource.StartActivity("game.api_call");
        //   AddHttpClientInstrumentation zorgt al voor W3C-propagatie,
        //   maar een eigen child-span maakt de relatie explicieter zichtbaar in Tempo.

        var response = await httpClient.GetFromJsonAsync<DiceRollResult>(ApiUrl);

        if (response is null)
        {
            Console.WriteLine("Onverwacht antwoord van de API. Probeer opnieuw.\n");
            continue;
        }

        apiRoll = response.Roll;

        // TODO (stap 5): Sla het API-resultaat op als tag op de roundActivity span.
        //   Gebruik: roundActivity?.SetTag("game.api_roll", apiRoll);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Kon de API niet bereiken: {ex.Message}");
        Console.WriteLine("Zorg dat DiceScore.API draait op http://localhost:5000\n");

        // TODO (stap 6): Markeer de roundActivity span als fout.
        //   Gebruik:
        //     roundActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        //     roundActivity?.RecordException(ex);

        continue;
    }

    // ── Uitkomst bepalen ───────────────────────────────────────────────────
    Console.WriteLine($"De dobbelsteenworp: {apiRoll}");

    var won = guess == apiRoll;

    if (won)
    {
        Console.WriteLine("Goed geraden! Je hebt gewonnen!\n");

        // TODO (stap 7): Verhoog de gamesWonCounter.
        //   Gebruik: gamesWonCounter.Add(1);

        // TODO (stap 8): Voeg een span-event toe voor de overwinning.
        //   Gebruik:
        //     roundActivity?.AddEvent(new ActivityEvent("game.win",
        //         tags: new ActivityTagsCollection { { "game.guess", guess }, { "game.roll", apiRoll } }));
    }
    else
    {
        Console.WriteLine($"Helaas, het was {apiRoll}. Jij raadde {guess}.\n");

        // TODO (stap 9): Voeg een span-event toe voor het verlies.
        //   Gebruik:
        //     roundActivity?.AddEvent(new ActivityEvent("game.loss",
        //         tags: new ActivityTagsCollection { { "game.guess", guess }, { "game.roll", apiRoll } }));
    }

    // TODO (stap 10): Verhoog de gamesPlayedCounter (altijd, win of verlies).
    //   Gebruik: gamesPlayedCounter.Add(1, new TagList { { "game.won", won } });
    //   De tag "game.won" maakt het mogelijk om in Prometheus te filteren op uitkomst.

    // TODO (stap 11): Sla het eindresultaat op als tag op de roundActivity span.
    //   Gebruik: roundActivity?.SetTag("game.won", won);
}

// ──────────────────────────────────────────────
// Model voor de API-respons
// ──────────────────────────────────────────────
record DiceRollResult(int Roll, DateTime Timestamp, int DelayMs);
