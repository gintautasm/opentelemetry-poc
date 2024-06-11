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
    sid       = "AllowAssumeRole"
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }

    actions = ["sts:AssumeRole"]
  }
}

resource "aws_iam_role" "iam_for_lambda" {
  name               = "opentelemetry_poc_iam_for_lambda"
  assume_role_policy = data.aws_iam_policy_document.assume_role.json
}

resource "aws_lambda_function" "test_lambda" {
  
  function_name = "opentelemetry_poc_lambda_function"
  role          = aws_iam_role.iam_for_lambda.arn
  image_uri    = "${aws_ecr_repository.opentelemetry_poc.repository_url}:latest"
  timeout = 60

  package_type = "Image"
  publish = true
  memory_size = 256

  #handler       = "search-handler-lambda::search_handler_lambda.Function::FunctionHandler"

  environment {
    variables = var.lambda_env_vars
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

#region CloudWatch

resource "aws_cloudwatch_log_group" "opentelemetry_poc_cw_group" {
  name              = "/aws/lambda/${aws_lambda_function.test_lambda.function_name}"
  retention_in_days = 1
}

data "aws_iam_policy_document" "lambda_logging" {
  statement {
    effect = "Allow"

    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents",
    ]

    resources = ["arn:aws:logs:*:*:*"]
  }
}

resource "aws_iam_policy" "lambda_logging" {
  name        = "lambda_logging"
  path        = "/"
  description = "IAM policy for logging from a lambda"
  policy      = data.aws_iam_policy_document.lambda_logging.json
}

resource "aws_iam_role_policy_attachment" "lambda_logs" {
  role       = aws_iam_role.iam_for_lambda.name
  policy_arn = aws_iam_policy.lambda_logging.arn
}

#endregion

#region variables

variable "lambda_env_vars" { }

#endregion