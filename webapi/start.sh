docker build -t 'otel-poc' .

docker run --rm -it \
 -e ASPNETCORE_URLS='http://+:5000' \
 -e OTEL_SERVICE_NAME='otel-poc' \
 -e OTEL_DOTNET_AUTO_LOGS_INCLUDE_FORMATTED_MESSAGE=true \
 -e OTEL_TRACES_EXPORTER=otel \
 -e OTEL_METRICS_EXPORTER=otel \
 -e OTEL_LOGS_EXPORTER=otel \
 -e OTEL_DOTNET_AUTO_TRACES_CONSOLE_EXPORTER_ENABLED=false \
 -e OTEL_DOTNET_AUTO_METRICS_CONSOLE_EXPORTER_ENABLED=false \
 -e OTEL_DOTNET_AUTO_LOGS_CONSOLE_EXPORTER_ENABLED=false \
 -e OTEL_EXPORTER_OTLP_ENDPOINT="http://host.docker.internal:4318/" \
 -p 8000:5000 \
 -v './appsettings.json:/app/appsettings.json' \
 otel-poc

 curl http://localhost:8000/weatherforecast
