# Opdracht 4 - Gedistribueerde tracing met twee services

In deze opdracht voeg je OpenTelemetry-instrumentatie toe aan de `DiceScore.GuessGame` console-applicatie. De infrastructuur is al opgezet — jouw taak is om de ontbrekende spans, tags, events en teller-verhogingen te implementeren.

## Scenario

Je werkt nu met **twee services**:

| Service | Type | Poort |
|---------|------|-------|
| `DiceScore.API` | ASP.NET Core WebAPI | http://localhost:5000 |
| `DiceScore.GuessGame` | .NET Console-app | — (client) |

De GuessGame roept de API aan via HTTP. Doordat beide services OpenTelemetry gebruiken met W3C Trace Context-propagation, verschijnen de calls als één doorlopende gedistribueerde trace in Tempo.

## Wat is al geregeld

- De `ActivitySource` en `Meter` zijn aangemaakt met de servicenaam `DiceScore.GuessGame`
- De OTLP-exporters zijn geconfigureerd (zelfde endpoint als de API: `http://localhost:4317`)
- `AddHttpClientInstrumentation()` zorgt automatisch voor het meesturen van de `traceparent`-header bij elke HTTP-aanroep naar de API
- Drie tellers zijn gedefinieerd: `game.played`, `game.won`, `game.wrong_input`
- De spellogica werkt volledig — je kunt het spel nu al speln

## Wat jij moet implementeren

Open `DiceScore.GuessGame/Program.cs` en zoek naar de genummerde `// TODO`-commentaren. Implementeer de volgende stappen:

### Stap 1 — Start een span per ronde

Voeg bovenaan de `while`-lus een Activity toe die de hele spelronde omvat:

```csharp
using var roundActivity = activitySource.StartActivity("game.round");
```

### Stap 2 — Tel ongeldige invoer

Verhoog de `wrongInputCounter` wanneer een speler een ongeldige waarde invoert:

```csharp
wrongInputCounter.Add(1);
```

### Stap 3 — Sla de gok op als span-tag

Voeg de geraden waarde toe aan de actieve span:

```csharp
roundActivity?.SetTag("game.guess", guess);
```

### Stap 4 — Optioneel: child-span voor de API-aanroep

Start een child-span rondom de `httpClient`-aanroep. Dit maakt de relatie tussen GuessGame en API zichtbaar als geneste spans in Tempo:

```csharp
using var httpActivity = activitySource.StartActivity("game.api_call");
```

### Stap 5 — Sla het API-resultaat op als span-tag

```csharp
roundActivity?.SetTag("game.api_roll", apiRoll);
```

### Stap 6 — Markeer fouten correct

Als de HTTP-aanroep mislukt, markeer de span dan als fout:

```csharp
roundActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
roundActivity?.RecordException(ex);
```

### Stap 7 & 8 — Gewonnen ronde

Verhoog de winteller en voeg een span-event toe:

```csharp
gamesWonCounter.Add(1);
roundActivity?.AddEvent(new ActivityEvent("game.win",
    tags: new ActivityTagsCollection { { "game.guess", guess }, { "game.roll", apiRoll } }));
```

### Stap 9 — Verloren ronde

Voeg een span-event toe voor het verlies:

```csharp
roundActivity?.AddEvent(new ActivityEvent("game.loss",
    tags: new ActivityTagsCollection { { "game.guess", guess }, { "game.roll", apiRoll } }));
```

### Stap 10 & 11 — Tel gespeelde rondes en sla resultaat op

```csharp
gamesPlayedCounter.Add(1, new TagList { { "game.won", won } });
roundActivity?.SetTag("game.won", won);
```

## De applicatie starten

Zorg dat de LGTM-container en de API al draaien (zie Opdracht 1). Start dan het raadspel in een aparte terminal:

```powershell
dotnet run --project DiceScore.GuessGame
```

Speel een paar ronden en schakel over naar Grafana.

## Controleren in Grafana

### Traces in Tempo

1. Ga naar **Explore** → selecteer **Tempo**
2. Zoek op servicenaam `DiceScore.GuessGame`
3. Open een trace en bekijk de spans:
   - Je ziet een `game.round`-span van de GuessGame
   - Daarbinnen een HTTP-span (automatisch aangemaakt door `AddHttpClientInstrumentation`)
   - Die HTTP-span is verbonden met de `GET /`-span van `DiceScore.API`
4. Dit is **gedistribueerde tracing**: één trace die twee services beslaat

> **Tip:** Klik op de HTTP-span en zoek naar het veld **Linked Traces** om de verbinding met de API-span te zien.

### Service Graph

1. Ga in Tempo naar het tabblad **Service Graph**
2. Je ziet twee knooppunten: `DiceScore.GuessGame` → `DiceScore.API`
3. De pijl geeft aan dat GuessGame de API aanroept

### Metrics in Prometheus

1. Ga naar **Explore** → selecteer **Prometheus**
2. Zoek naar de volgende metrics:
   - `game_played_total` — totaal aantal gespeelde ronden
   - `game_won_total` — totaal aantal gewonnen ronden
   - `game_wrong_input_total` — totaal aantal ongeldige invoeringen
3. Bereken het winstpercentage met PromQL:

```promql
rate(game_won_total[1m]) / rate(game_played_total[1m])
```

## Bonusopdrachten

### Bonus 1 — Filter traces op uitkomst

In Tempo kun je traces filteren op span-tags. Probeer:
- Zoek op `game.won = true` om alleen winnende rondes te zien
- Zoek op `game.won = false` om verliezende rondes te zien

### Bonus 2 — Voeg een ronde-teller toe

Houd bij in welke ronde de speler zit en voeg dit toe als tag op de span:

```csharp
roundActivity?.SetTag("game.round_number", roundNumber);
```

### Bonus 3 — Maak een Grafana-dashboard

Gebruik een LLM (zoals in Opdracht 3) om een dashboard te genereren met de volgende panels:
- Winstpercentage over tijd (lijndiagram)
- Totaal gespeelde rondes (statpanel)
- Verdeling van geraden waarden vs. echte worpen (bargraph)
