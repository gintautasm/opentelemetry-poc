# opentelemetry-poc

![PoC architecture](/resources/architecture.png "PoC architecture")

## Usage

### Prerequisites

* Confluent cloud Kafka server trial version or any other [free trial here](https://login.confluent.io/)
* Datadog account [free trial here](https://www.datadoghq.com/free-datadog-trial/)
* Dynatrace account [free trial here](https://www.dynatrace.com/signup/)


### Setup Dynatrace
* Create API Keys for Dynatrace

### Setup Datadog
* Install Opentelemetry extension
* Create API Keys for Datadog

### Setup Confluent Cloud

* Create topics: `start-search` and `search-results`
* Create API keys for accessing Kafka

### Setup application

* Populate api-keys in `docker-compose.yml` file
* Ensure all build required containers are built
* Run `docker compose up` in root
* Set api-keys in `lambda/search-handler/src/search-handler-console/.vscode/launch.json`
* Build and start `search-handler-console`
  * Perform search: type any letter or number key in console, sends message to webapi container
  * Perform debug search: hit spacebar, reroutes message to self via kafka
  * Enter: to quit


