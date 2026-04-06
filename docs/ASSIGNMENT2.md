# Opdracht 2 - Toegang tot de observability stack

### Grafana dashboard

1. Open http://localhost:3000 in je browser
2. Log in met gebruikersnaam `admin` en wachtwoord `admin`
3. Navigeer naar **Connections** → **Data Sources** om te zien:
   - Tempo (traces)
   - Loki (logs)
   - Prometheus (metrics)

### Traces bekijken

1. Ga in Grafana naar **Explore**
2. Selecteer **Tempo** als databron
3. Zoek naar traces van de `DiceScore.API` service
4. Je ziet:
   - Verzoekduur
   - Dobbelsteenresultaten
   - Vertragingstijden
   - Foutoptredens
   - Aangepaste tags (dice.result, dice.delay_ms, dice.error)

### Metrics bekijken

1. Ga in Grafana naar **Explore**
2. Selecteer **Prometheus** als databron
3. Zoek naar metrics zoals:
   - `http_server_request_duration_seconds`
   - `http_server_active_requests`
   - Ingebouwde ASP.NET Core metrics

### Logs bekijken

1. Ga in Grafana naar **Explore**
2. Selecteer **Loki** als databron
3. Filter logs op servicenaam of logniveau
4. Zoek naar specifieke dobbelsteen-events of fouten