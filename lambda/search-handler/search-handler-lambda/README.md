cp ~/dev/opentelemetry-lambda/collector/build/extensions/collector ~/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda/otel-colletor-layer/extensions/collector

dotnet publish "/home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda" \
     --output "/home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda/./bin/Release/lambda-publish" \
     --configuration "Release" /p:GenerateRuntimeConfigurationFiles=true \
     --runtime linux-x64 \
     --self-contained False

chmod +rx /home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda/bin/Release/lambda-publish/

cd ~/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda

docker build  -f "/home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda/Dockerfile" -t search-handler-lambda:latest .
docker run -it -p 9000:8080 \
     -e DD_SITE="datadoghq.eu" \
     -e DD_API_KEY="d601...." \
     -e DD_TRACE_OTEL_ENABLED=true \
     -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT="0.0.0.0:43177" \
     -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT="0.0.0.0:43188" \
     --network="main-backend" search-handler-lambda search-handler-lambda::search_handler_lambda.Function::FunctionHandler
curl -XPOST "http://localhost:9000/2015-03-31/functions/function/invocations" -d '"hello world"'

docker tag search-handler-lambda:latest gintautasm/opentelemetry-poc:latest
docker push gintautasm/opentelemetry-poc:latest

docker tag search-handler-lambda:latest 506........dkr.ecr.eu-west-1.amazonaws.com/opentelemetry-poc:latest
aws ecr get-login-password --region eu-west-1 | docker login --username AWS --password-stdin 506........dkr.ecr.eu-west-1.amazonaws.com
docker push 506........dkr.ecr.eu-west-1.amazonaws.com/opentelemetry-poc:latest

docker run -d --name dd-agent \
-e DD_API_KEY=d601.... \
-e DD_SITE="datadoghq.eu" \
-v /var/run/docker.sock:/var/run/docker.sock:ro \
-v /proc/:/host/proc/:ro \
-v /sys/fs/cgroup/:/host/sys/fs/cgroup:ro \
-v /var/lib/docker/containers:/var/lib/docker/containers:ro \
gcr.io/datadoghq/agent:7


terraform apply -auto-approve
terraform destroy -auto-approve

