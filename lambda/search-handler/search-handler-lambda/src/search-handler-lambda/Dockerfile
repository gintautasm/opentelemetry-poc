FROM public.ecr.aws/lambda/dotnet:8

#WORKDIR /var/task
WORKDIR ${LAMBDA_TASK_ROOT}

# This COPY command copies the .NET Lambda project's build artifacts from the host machine into the image. 
# The source of the COPY should match where the .NET Lambda project publishes its build artifacts. If the Lambda function is being built 
# with the AWS .NET Lambda Tooling, the `--docker-host-build-output-dir` switch controls where the .NET Lambda project
# will be built. The .NET Lambda project templates default to having `--docker-host-build-output-dir`
# set in the aws-lambda-tools-defaults.json file to "bin/Release/lambda-publish".
#
# Alternatively Docker multi-stage build could be used to build the .NET Lambda project inside the image.
# For more information on this approach checkout the project's README.md file.

COPY "bin/Release/lambda-publish"  .

COPY "otel-colletor-layer/collector-config/config.yaml"  ./collector-config/config.yaml

## https://docs.datadoghq.com/serverless/aws_lambda/installation/dotnet/?tab=containerimage
COPY --from=public.ecr.aws/datadog/lambda-extension:latest /opt/. /opt/
COPY "datadog/datadog.yml" ${LAMBDA_TASK_ROOT}/.

#COPY --from=public.ecr.aws/aws-observability/aws-otel-collector:latest /awscollector /opt/extensions/.
COPY "otel-colletor-layer/extensions/collector" /opt/extensions/.

#COPY "otel-colletor-layer/extensions/aws-otel-collector.deb" .
# RUN yum -i aws-otel-collector.deb

#WORKDIR /opt/extensions

# COPY "otel-colletor-layer/extensions/collector"  /opt/extensions/.
# RUN chmod +x /opt/extensions/collector

ENV OPENTELEMETRY_COLLECTOR_CONFIG_FILE=/var/task/collector-config/config.yaml

CMD ["search-handler-lambda::search_handler_lambda.Function::FunctionHandler"]