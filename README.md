# opentelemetry-poc

## Usage

* Install OTEL Collector
* send API keys
```
export DYNATRACE_ENV_ID=mrw77986
export DYNATRACE_API_KEY=dt0c01....
export DATADOG_API_KEY=781d2...
```
* Run `/usr/bin/otelcol-contrib --config=otel-config.yml`
* run `docker compose up` in root
