# opentelemetry-poc

## Usage

### Setup OpenTelemetry collector

* Install OTEL Collector
* export API keys in bash before running
```
export DYNATRACE_ENV_ID=mrw77986
export DYNATRACE_API_KEY=dt0c01....
export DATADOG_API_KEY=781d2...
```
* Run `/usr/bin/otelcol-contrib --config=otel-config.yml`

### Setup application

* run `docker compose up` in root

### Setup Confluent Cloud

* create topics: `start-search` and `search-results`
