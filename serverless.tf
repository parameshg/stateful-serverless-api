terraform {
  backend "s3" {
    bucket = "aspnetcore"
    key    = "terraform-state/statefull-serverless-api.tfstate"
  }
}

variable "AWS_ACCOUNT" {
  type = string
  description = "AWS_ACCOUNT"
}

variable "AWS_REGION" {
  type = string
  description = "AWS_REGION"
}

variable "AWS_ACCESS_KEY_ID" {
  type = string
  description = "AWS_ACCESS_KEY_ID"
}

variable "AWS_SECRET_ACCESS_KEY" {
  type = string
  description = "AWS_SECRET_ACCESS_KEY"
}

provider "aws" {
  region     = var.AWS_REGION
  access_key = var.AWS_ACCESS_KEY_ID
  secret_key = var.AWS_SECRET_ACCESS_KEY
}

resource "aws_iam_role" "statefull-serverless-api" {
  name               = "statefull-serverless-api-role"
  tags               = {
    provisioner      = "terraform"
    executioner      = "github-actions"
    project          = "statefull-serverless-api"
    url              = "https://github.com/parameshg/statefull-serverless-api"
  }
  assume_role_policy = <<EOF
  {
    "Version": "2012-10-17",
    "Statement": [
      {
        "Action": "sts:AssumeRole",
        "Principal": {
          "Service": "lambda.amazonaws.com"
        },
        "Effect": "Allow",
        "Sid": ""
      }
    ]
  }
EOF
}

resource "aws_lambda_function" "statefull-serverless-api" {
  function_name = "statefull-serverless-api"
  package_type  = "Image"
  image_uri     = "${var.AWS_ACCOUNT}.dkr.ecr.${var.AWS_REGION}.amazonaws.com/statefull-serverless-api:latest"
  role          = "${aws_iam_role.statefull-serverless-api.arn}"
  tags          = {
    provisioner = "terraform"
    executioner = "github-actions"
    project     = "statefull-serverless-api"
    url         = "https://github.com/parameshg/statefull-serverless-api"
  }
}

resource "aws_api_gateway_rest_api" "statefull-serverless-api" {
  name          = "statefull-serverless-api-gateway"
  description   = "Statefull Serverless Api"
  tags          = {
    provisioner = "terraform"
    executioner = "github-actions"
    project     = "statefull-serverless-api"
    url         = "https://github.com/parameshg/statefull-serverless-api"
  }
}

resource "aws_api_gateway_resource" "statefull-serverless-api" {
  rest_api_id = "${aws_api_gateway_rest_api.statefull-serverless-api.id}"
  parent_id   = "${aws_api_gateway_rest_api.statefull-serverless-api.root_resource_id}"
  path_part   = "{proxy+}"
}

resource "aws_api_gateway_method" "statefull-serverless-api" {
  rest_api_id   = "${aws_api_gateway_rest_api.statefull-serverless-api.id}"
  resource_id   = "${aws_api_gateway_resource.statefull-serverless-api.id}"
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "statefull-serverless-api" {
  rest_api_id = "${aws_api_gateway_rest_api.statefull-serverless-api.id}"
  resource_id = "${aws_api_gateway_method.statefull-serverless-api.resource_id}"
  http_method = "${aws_api_gateway_method.statefull-serverless-api.http_method}"

  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = "${aws_lambda_function.statefull-serverless-api.invoke_arn}"
}

resource "aws_api_gateway_method" "statefull-serverless-api-root" {
  rest_api_id   = "${aws_api_gateway_rest_api.statefull-serverless-api.id}"
  resource_id   = "${aws_api_gateway_rest_api.statefull-serverless-api.root_resource_id}"
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "statefull-serverless-api-root" {
  rest_api_id = "${aws_api_gateway_rest_api.statefull-serverless-api.id}"
  resource_id = "${aws_api_gateway_method.statefull-serverless-api-root.resource_id}"
  http_method = "${aws_api_gateway_method.statefull-serverless-api-root.http_method}"

  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = "${aws_lambda_function.statefull-serverless-api.invoke_arn}"
}

resource "aws_api_gateway_deployment" "statefull-serverless-api" {
  depends_on = [
    aws_api_gateway_integration.statefull-serverless-api,
    aws_api_gateway_integration.statefull-serverless-api-root,
  ]

  rest_api_id = "${aws_api_gateway_rest_api.statefull-serverless-api.id}"
  stage_name  = "test"
}