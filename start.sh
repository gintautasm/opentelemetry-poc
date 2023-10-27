docker build -t 'otel-poc' .

docker run --rm -it \
 -e ASPNETCORE_URLS='http://+:5000' \
 -e OTEL_SERVICE_NAME='otel-poc' \
 -e OTEL_DOTNET_AUTO_LOGS_INCLUDE_FORMATTED_MESSAGE \
 -e OTEL_TRACES_EXPORTER=none \
 -e OTEL_METRICS_EXPORTER=none \
 -e OTEL_LOGS_EXPORTER=none \
 -e OTEL_DOTNET_AUTO_TRACES_CONSOLE_EXPORTER_ENABLED=false \
 -e OTEL_DOTNET_AUTO_METRICS_CONSOLE_EXPORTER_ENABLED=false \
 -e OTEL_DOTNET_AUTO_LOGS_CONSOLE_EXPORTER_ENABLED=true \
 -p 8000:5000 \
 -v './appsettings.json:/app/appsettings.json' \
 otel-poc

 curl http://localhost:8000/weatherforecast
