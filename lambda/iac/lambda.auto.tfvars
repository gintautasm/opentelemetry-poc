lambda_env_vars = {
      DYNATRACE_ENV_ID="itq..."
      DYNATRACE_API_KEY="dt0c01.R..."
      HONEYCOMB_API_KEY = "mJ8k..."
      DD_SITE = "datadoghq.eu"
      DD_API_KEY = "59f8..."
      DD_LOGS_ENABLED = true
      DD_OTLP_CONFIG_LOGS_ENABLED = true
      DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT = "localhost:43177"
      DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT = "localhost:43188"
      DD_TRACE_ENABLED = true
      DD_TRACE_OTEL_ENABLED = true
      DD_LOG_LEVEL = "debug"
      DD_APM_ENABLED = true
      DD_PROCESS_CONFIG_PROCESS_COLLECTION_ENABLED = true
      
      OTEL_EXPORTER_OTLP_TIMEOUT=30000
      OTEL_EXPORTER_OTLP_HEADERS="x-honeycomb-team=mJ8..."
    }

    #OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318/"