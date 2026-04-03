# Opdracht 1 - Start de LGTM Docker container

Start de alles-in-één observability stack met het `grafana/otel-lgtm` image:

```powershell
docker run --detach --name lgtm -p 3000:3000 -p 4317:4317 -p 4318:4318 --rm grafana/otel-lgtm:latest
```

Deze enkele container bevat:
- **Grafana** op http://localhost:3000 (dashboard 👉 admin/admin)
- **Loki** (logaggregatie)
- **Tempo** (gedistribueerde tracing)
- **Mimir** (metrics, Prometheus-compatibel)
- **OpenTelemetry Collector** op poort 4317 (gRPC) en 4318 (HTTP)

**Controleer of de container actief is:**

```powershell
docker ps --filter name=lgtm
```

### 2. Start de ASP.NET Core WebAPI

Open een nieuw terminal en start de API vanuit de `src` map:

```powershell
dotnet run --project src
```

Of met een specifieke URL:

```powershell
dotnet run --project src --urls "http://localhost:5000"
```

De API start en luistert op de ingestelde poorten. Je ziet uitvoer die aangeeft:
- Applicatie gestart
- OpenTelemetry exporters geconfigureerd
- Luisterende URLs

**Test het endpoint handmatig:**

```powershell
curl http://localhost:5000
```

Verwacht antwoord:
```json
{
  "roll": 4,
  "timestamp": "2026-04-03T10:30:45.123Z",
  "delayMs": 342
}
```

### 3. Genereer verkeer

Start in een apart terminal het verkeergeneratiescript:

```powershell
.\scripts\generate-traffic.ps1
```

> **Script wordt geblokkeerd?** Gebruik dan:
> ```powershell
> powershell -ExecutionPolicy Bypass -File .\scripts\generate-traffic.ps1
> ```

**Opties:**

```powershell
# Genereer verkeer gedurende 60 seconden op 2 verzoeken/seconde (standaard)
.\scripts\generate-traffic.ps1

# Aangepaste URL en verzoeksnelheid
.\scripts\generate-traffic.ps1 -Url "http://localhost:5000" -RequestsPerSecond 5

# Uitvoeren voor een bepaalde duur
.\scripts\generate-traffic.ps1 -DurationSeconds 120

# Oneindig uitvoeren (druk Ctrl+C om te stoppen)
.\scripts\generate-traffic.ps1 -Infinite

# Opties combineren
.\scripts\generate-traffic.ps1 -RequestsPerSecond 10 -DurationSeconds 300
```

Het script toont:
- ✅ Geslaagde verzoeken met dobbelsteenEmoji en vertraging
- ❌ Mislukte verzoeken (gesimuleerde fouten)
- 📊 Samenvattingsstatistieken aan het einde
