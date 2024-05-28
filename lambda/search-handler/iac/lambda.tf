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
  image_uri    = "docker.io/gintautasm/opentelemetry-poc:latest"

  package_type = "Image"
  publish = true

  environment {
    variables = {
      foo = "bar"
    }
  }
}

#endregion
