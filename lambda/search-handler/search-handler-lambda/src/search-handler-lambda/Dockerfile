FROM public.ecr.aws/lambda/dotnet:8

WORKDIR /var/task
#WORKDIR ${LAMBDA_TASK_ROOT}

COPY "bin/Release/lambda-publish"  .

COPY "otel-colletor-layer/collector-config/config.yaml"  ./collector-config/config.yaml

## https://docs.datadoghq.com/serverless/aws_lambda/installation/dotnet/?tab=containerimage
#COPY --from=public.ecr.aws/datadog/lambda-extension:latest /opt/. /opt/
# COPY "datadog/datadog.yml" ${LAMBDA_TASK_ROOT}/.

#COPY --from=public.ecr.aws/aws-observability/aws-otel-collector:latest /awscollector /opt/extensions/.
COPY "otel-colletor-layer/extensions/collector" /opt/extensions/.

ENV OPENTELEMETRY_COLLECTOR_CONFIG_FILE=/var/task/collector-config/config.yaml

CMD ["search-handler-lambda::search_handler_lambda.Function::FunctionHandler"]