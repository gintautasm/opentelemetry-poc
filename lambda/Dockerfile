## https://docs.datadoghq.com/serverless/aws_lambda/installation/dotnet/?tab=containerimage
FROM public.ecr.aws/datadog/lambda-extension:latest as datadog
FROM public.ecr.aws/aws-observability/aws-otel-collector:latest as adot
FROM public.ecr.aws/lambda/dotnet:8 as base

FROM alpine:latest as adot-extension

ARG AWS_DEFAULT_REGION=${AWS_DEFAULT_REGION:-"eu-west-1"}
ARG AWS_ACCESS_KEY_ID=${AWS_ACCESS_KEY_ID:-""}
ARG AWS_SECRET_ACCESS_KEY=${AWS_SECRET_ACCESS_KEY:-""}
ENV AWS_DEFAULT_REGION=${AWS_DEFAULT_REGION}
ENV AWS_ACCESS_KEY_ID=${AWS_ACCESS_KEY_ID}
ENV AWS_SECRET_ACCESS_KEY=${AWS_SECRET_ACCESS_KEY}

RUN apk add aws-cli curl unzip

RUN mkdir -p /opt

ENV LAYER_VER=0-98-0:5
RUN curl $(aws lambda get-layer-version-by-arn --arn arn:aws:lambda:$AWS_DEFAULT_REGION:901920570463:layer:aws-otel-collector-amd64-ver-$LAYER_VER --query 'Content.Location' --output text) --output layer.zip
RUN unzip layer.zip -d /opt
RUN rm layer.zip

FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /src
COPY ["search-handler-lambda.csproj", "l/"]
RUN dotnet restore "l/search-handler-lambda.csproj"

WORKDIR "/src/l"
COPY . .
RUN dotnet build "search-handler-lambda.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "search-handler-lambda.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .

# add datadog extension layer
COPY --from=datadog /opt/. /opt/
# COPY "datadog/datadog.yml" /var/task/datadog.yml

# add datadog extension layer
#COPY "otel-colletor-layer/extensions/collector" /opt/extensions/.
COPY --from=adot-extension /opt/extensions/. /opt/extensions/.
RUN chmod -R +x /opt/extensions/
COPY "otel-colletor-layer/collector-config/config.yaml"  ./collector-config/config.yaml

ENV OPENTELEMETRY_COLLECTOR_CONFIG_FILE=/var/task/collector-config/config.yaml

CMD ["search-handler-lambda::search_handler_lambda.Function::FunctionHandler"]