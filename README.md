# ObservabilityDemo - ASP.NET Core WebAPI met LGTM Stack

Een demo ASP.NET Core WebAPI die willekeurige dobbelsteenworpen (1-6) retourneert met instelbare vertragingen en foutpercentages, volledig geïnstrumenteerd met OpenTelemetry en geïntegreerd met de LGTM observability stack (Loki, Grafana, Tempo, Mimir/Prometheus).

## Functies

- 🎲 Willekeurig dobbelsteenwerp endpoint (geeft 1-6 terug)
- ⏱️ Instelbare willekeurige vertraging (standaard: 0-1000ms)
- ❌ Instelbaar foutpercentage (standaard: 5%)
- 📊 Volledige OpenTelemetry instrumentatie (traces, metrics, logs)
- 🔍 Integratie met LGTM observability stack
- 🚀 Verkeergeneratiescript inbegrepen

## Vereisten

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- PowerShell (voor het verkeergeneratiescript)


## Opdrachten

| Opdracht | Omschrijving |
|----------|--------------|
| [Opdracht 1](docs/ASSIGNMENT1.md) | Start de LGTM Docker container |
| [Opdracht 2](docs/ASSIGNMENT2.md) | Toegang tot de observability stack |
| [Opdracht 3](docs/ASSIGNMENT3.md) | Grafana dashboard genereren via een LLM |


## Configuratie

### API-instellingen

Bewerk [appsettings.json](src/appsettings.json) om het gedrag aan te passen:

```json
{
  "ApiSettings": {
    "MinDelayMs": 0,        // Minimale vertraging in milliseconden
    "MaxDelayMs": 1000,     // Maximale vertraging in milliseconden
    "ErrorRate": 0.05       // Foutpercentage (0.05 = 5%)
  },
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"  // OTLP gRPC endpoint
  }
}
```

**Voorbeelden:**

- **Geen vertraging:** Stel `MinDelayMs` en `MaxDelayMs` beiden in op `0`
- **Vaste vertraging:** Stel beiden in op dezelfde waarde (bijv. `500`)
- **Hoger foutpercentage:** Verander `ErrorRate` naar `0.20` (20%)
- **Geen fouten:** Stel `ErrorRate` in op `0`

### Omgevingsvariabelen

Je kunt instellingen ook overschrijven via omgevingsvariabelen:

```powershell
$env:ApiSettings__MinDelayMs = "100"
$env:ApiSettings__MaxDelayMs = "500"
$env:ApiSettings__ErrorRate = "0.10"
dotnet run --project src
```

## Services stoppen

### Stop de ASP.NET Core API

Druk op `Ctrl+C` in de terminal waar de API actief is.

### Stop de LGTM container

```powershell
docker stop lgtm
```

De `--rm` vlag verwijdert de container automatisch na het stoppen.



## Hoe het werkt

### Flow

1. **Client** stuurt HTTP GET-verzoek naar `/`
2. **ASP.NET Core** ontvangt het verzoek en maakt een OpenTelemetry span aan
3. **Applicatie** past willekeurige vertraging toe (standaard 0-1000ms)
4. **Applicatie** controleert het foutpercentage en kan een 500-fout retourneren (5% kans)
5. **Applicatie** genereert willekeurig getal 1-6 als er geen fout is
6. **OpenTelemetry** legt traces, metrics en logs vast
7. **OTLP Exporter** stuurt telemetrie naar de OpenTelemetry Collector
8. **Collector** stuurt data door naar:
   - Tempo (traces)
   - Prometheus (metrics)
   - Loki (logs)
9. **Grafana** visualiseert alle telemetriedata

### Aangepaste telemetrietags

Elk verzoek bevat aangepaste OpenTelemetry-tags:
- `dice.result` - Het gegooid getal (1-6)
- `dice.delay_ms` - Toegepaste vertraging
- `dice.error` - Of er een fout werd gesimuleerd
- `dice.min_delay` - Ingestelde minimale vertraging
- `dice.max_delay` - Ingestelde maximale vertraging
- `dice.error_rate` - Ingesteld foutpercentage

## Probleemoplossing

### API start niet

- Controleer of poort 5000 niet in gebruik is: `netstat -ano | findstr :5000`
- Controleer of .NET SDK is geïnstalleerd: `dotnet --version`
- Herstel pakketten: `dotnet restore`

### Geen traces in Grafana

- Controleer of de container actief is: `docker ps --filter name=lgtm`
- Bekijk de containerlogs: `docker logs lgtm`
- Zorg dat de API is geconfigureerd met het juiste OTLP endpoint (`http://localhost:4317`)
- Controleer of Grafana bereikbaar is: http://localhost:3000

### Docker container start niet

- Zorg dat Docker Desktop actief is
- Controleer op poortconflicten: `netstat -ano | findstr :3000`
- Herstart Docker Desktop

### Hoog foutpercentage

- Controleer `ApiSettings:ErrorRate` in `src/appsettings.json`
- Zorg dat het is ingesteld op `0.05` (5%) en niet `0.5` (50%)

## Geavanceerd gebruik

### Aangepaste query's in Grafana

**Prometheus query-voorbeelden:**
```promql
# Verzoeksnelheid
rate(http_server_request_duration_seconds_count[1m])

# Gemiddelde verzoekduur
rate(http_server_request_duration_seconds_sum[1m]) / rate(http_server_request_duration_seconds_count[1m])

# Foutpercentage
rate(http_server_request_duration_seconds_count{http_response_status_code="500"}[1m])
```

**Tempo query:**
- Zoek op servicenaam: `ObservabilityDemo`
- Filter op duur: `> 500ms`
- Filter op status: `status=error`

### Uitvoeren in productie

Voor productie-implementaties:
1. Gebruik HTTPS-endpoints
2. Configureer juiste authenticatie voor Grafana
3. Gebruik permanente opslag voor alle services
4. Stel geschikte retentiebeleid in
5. Beveilig OpenTelemetry-endpoints
6. Gebruik omgevingsspecifieke configuratie


## Licentie

Dit is een demoproject voor onderwijsdoeleinden.

## Bronnen

- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Grafana Documentatie](https://grafana.com/docs/)
- [Tempo Documentatie](https://grafana.com/docs/tempo/latest/)
- [Loki Documentatie](https://grafana.com/docs/loki/latest/)
- [Prometheus Documentatie](https://prometheus.io/docs/)
