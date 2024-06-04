#region provider

terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "5.50.0"
    }
  }
}

provider "aws" {
  default_tags {
   tags = {
    "service-name" = "opentelemetry-poc" 
     Environment = "PoC"
     Owner       = "N/A"
     Project     = "opentelemetry_poc"
   }
 }
}

#endregion

#region Basic lambda

data "aws_iam_policy_document" "assume_role" {
  statement {
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

resource "aws_iam_role" "iam_for_lambda" {
  name               = "iam_for_lambda"
  assume_role_policy = data.aws_iam_policy_document.assume_role.json
}

resource "aws_lambda_function" "test_lambda" {
  
  function_name = "lambda_function_name"
  role          = aws_iam_role.iam_for_lambda.arn
  image_uri    = "${aws_ecr_repository.opentelemetry_poc.repository_url}:latest"
  timeout = 60

  package_type = "Image"
  publish = true

  #handler       = "search-handler-lambda::search_handler_lambda.Function::FunctionHandler"

  environment {
    variables = {
      foo = "bar"
      DD_SITE = "datadoghq.eu"
      DD_API_KEY = "d601..."
      DD_TRACE_OTEL_ENABLED = true
      DD_LOGS_ENABLED = true
      DD_OTLP_CONFIG_LOGS_ENABLED = true
      DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT = "0.0.0.0:43177"
      DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT = "0.0.0.0:43188"
      DD_SERVERLESS_LOGS_ENABLED = true
      DD_CAPTURE_LAMBDA_PAYLOAD = true
      DD_TRACE_ENABLED = true
      DD_LOG_LEVEL = "debug"
      DD_DOGSTATSD_NON_LOCAL_TRAFFIC = true
      DD_APM_ENABLED = true
    }
  }
}

#endregion

#region AWS ECR repository

resource "aws_ecr_repository" "opentelemetry_poc" {
  name                 = "opentelemetry-poc"
  image_tag_mutability = "MUTABLE"
  force_delete = true

  #image_scanning_configuration {
  #  scan_on_push = true
  #}
}

#endregion