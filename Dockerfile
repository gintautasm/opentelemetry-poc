# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# install OpenTelemetry .NET Automatic Instrumentation
ARG OTEL_VERSION=1.1.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh


RUN apk add unzip && \
    OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh
RUN chmod +x '/otel-dotnet-auto/instrument.sh'

ARG TARGETARCH
WORKDIR /source

# optimize
RUN --mount=type=bind,source='opentelemetry-poc.csproj',target='opentelemetry-poc.csproj' \
    --mount=type=cache,target=/root/.nuget/ \
    dotnet restore -a $TARGETARCH

# copy and publish app and libraries
COPY . .
RUN dotnet publish -a $TARGETARCH -o /app
COPY ./run.sh /app
RUN chmod +x '/app/run.sh'


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ENV OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto"

COPY --from=build /otel-dotnet-auto /otel-dotnet-auto
WORKDIR /app
COPY --from=build /app .
#ENTRYPOINT [ "sh" ]
#ENTRYPOINT [ "./opentelemetry-poc"]
ENTRYPOINT [ "sh", "./run.sh" ]
