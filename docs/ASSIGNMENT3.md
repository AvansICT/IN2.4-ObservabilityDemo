# Opdracht 3 - Grafana dashboard genereren via een LLM

Grafana dashboard JSON is omvangrijk en complex om met de hand te schrijven. Gebruik een LLM (Claude Code, GitHub Copilot, ChatGPT) om snel een bruikbaar dashboard te genereren en importeer het vervolgens in Grafana.

## Stap 1 — Genereer de JSON

Genereer via een LLM een grafana dashboard. Bedenk vooraf welke metrics je zou willen zien en neem deze mee in je prompt. Belangrijk is dat je ook meegeeft voor welke tech stack je een prompt wilt (in dit geval ASP.NET)

## Stap 2 — Importeer in Grafana

1. Open http://localhost:3000 en log in (`admin` / `admin`)
2. Ga naar **Dashboards** → **New** → **Import**
3. Plak de gegenereerde JSON in het tekstveld
4. Klik **Load** en vervolgens **Import**

> **Tip:** werkt het dashboard niet direct? Controleer of de datasource UIDs overeenkomen met de namen in **Connections** → **Data Sources**.