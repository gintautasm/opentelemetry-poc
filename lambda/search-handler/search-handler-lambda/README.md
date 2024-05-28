dotnet publish "/home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda" \
     --output "/home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda/./bin/Release/lambda-publish" \
     --configuration "Release" /p:GenerateRuntimeConfigurationFiles=true \
     --runtime linux-x64 \
     --self-contained False

chmod +rx /home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda/bin/Release/lambda-publish/

cd ~/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda

docker build  -f "/home/gintautas/dev/opentelemetry-poc/lambda/search-handler/search-handler-lambda/src/search-handler-lambda/Dockerfile" -t search-handler-lambda:latest .
docker run -it -p 9000:8080 --network="main-backend" search-handler-lambda search-handler-lambda::search_handler_lambda.Function::FunctionHandler
curl -XPOST "http://localhost:9000/2015-03-31/functions/function/invocations" -d '"hello world"'

docker tag search-handler-lambda:latest gintautasm/opentelemetry-poc:latest

docker push gintautasm/opentelemetry-poc:latest
