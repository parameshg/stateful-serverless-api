terraform {
  backend "s3" {
    bucket = "aspnetcore"
    key    = "terraform-state/stateful-serverless-api.tfstate"
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

variable "IMAGE_COMMAND" {
  type        = list(string)
  description = "Docker Image CMD Override"
  default     = ["Api::Api.LambdaEntryPoint::FunctionHandlerAsync"]
}

variable "SENTRY_ENDPOINT" {
  type    = string
  description = "Sentry Dsn"
  default = ""
}

variable "HTTP_TIMEOUT" {
  type        = number
  description = "Lambda Timeout"
  default     = 10
}

provider "aws" {
  region     = var.AWS_REGION
  access_key = var.AWS_ACCESS_KEY_ID
  secret_key = var.AWS_SECRET_ACCESS_KEY
}

# AWS IAM ROLE ####################################################################################################################################################

data "aws_iam_policy_document" "stateful-serverless-api-trust" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}

data "aws_iam_policy_document" "stateful-serverless-api-policy" {
  statement {
    actions   = ["dynamodb:*"]
    resources = ["arn:aws:dynamodb:${var.AWS_REGION}:${var.AWS_ACCOUNT}:table/*"]
  }
}

resource "aws_iam_role" "stateful-serverless-api" {
  name               = "stateful-serverless-api"
  managed_policy_arns = ["arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"]
  assume_role_policy = data.aws_iam_policy_document.stateful-serverless-api-trust.json
  inline_policy {
    name   = "inline-policy"
    policy = data.aws_iam_policy_document.stateful-serverless-api-policy.json
  }
  tags               = {
    provisioner      = "terraform"
    executioner      = "github-actions"
    project          = "stateful-serverless-api"
    url              = "https://github.com/parameshg/stateful-serverless-api"
  }
}

# AWS LAMBDA ######################################################################################################################################################

resource "aws_lambda_function" "stateful-serverless-api" {
  function_name  = "stateful-serverless-api"
  role           = "${aws_iam_role.stateful-serverless-api.arn}"
  package_type   = "Image"
  image_uri      = "${var.AWS_ACCOUNT}.dkr.ecr.${var.AWS_REGION}.amazonaws.com/stateful-serverless-api:latest"
  image_config    {
    command      = var.IMAGE_COMMAND
  }
  environment      {
    variables    = {
      SENTRY_DSN = var.SENTRY_ENDPOINT
    }
  }
  timeout        = var.HTTP_TIMEOUT
  tags           = {
    provisioner  = "terraform"
    executioner  = "github-actions"
    project      = "stateful-serverless-api"
    url          = "https://github.com/parameshg/stateful-serverless-api"
  }
}

# AWS API GATEWAY #################################################################################################################################################

resource "aws_api_gateway_rest_api" "stateful-serverless-api" {
  name                         = "stateful-serverless-api"
  description                  = "Stateful Serverless Api"
  disable_execute_api_endpoint = true
  endpoint_configuration {
    types                      = ["REGIONAL"]
  }
  tags                         = {
    provisioner                = "terraform"
    executioner                = "github-actions"
    project                    = "stateful-serverless-api"
    url                        = "https://github.com/parameshg/stateful-serverless-api"
  }
}

resource "aws_api_gateway_resource" "stateful-serverless-api" {
  rest_api_id             = "${aws_api_gateway_rest_api.stateful-serverless-api.id}"
  parent_id               = "${aws_api_gateway_rest_api.stateful-serverless-api.root_resource_id}"
  path_part               = "{proxy+}"
}

resource "aws_api_gateway_method" "stateful-serverless-api" {
  rest_api_id             = "${aws_api_gateway_rest_api.stateful-serverless-api.id}"
  resource_id             = "${aws_api_gateway_resource.stateful-serverless-api.id}"
  http_method             = "ANY"
  authorization           = "NONE"
  api_key_required        = true
}

resource "aws_api_gateway_method_response" "stateful-serverless-api" {
    rest_api_id           = "${aws_api_gateway_rest_api.stateful-serverless-api.id}"
    resource_id           = "${aws_api_gateway_resource.stateful-serverless-api.id}"
    http_method           = "${aws_api_gateway_method.stateful-serverless-api.http_method}"
    status_code           = "200"
    response_models       = {
      "application/json"  = "Empty"
    }
}

resource "aws_api_gateway_integration" "stateful-serverless-api" {
  rest_api_id             = "${aws_api_gateway_rest_api.stateful-serverless-api.id}"
  resource_id             = "${aws_api_gateway_method.stateful-serverless-api.resource_id}"
  http_method             = "${aws_api_gateway_method.stateful-serverless-api.http_method}"
  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = "${aws_lambda_function.stateful-serverless-api.invoke_arn}"
}

resource "aws_api_gateway_method" "stateful-serverless-api-root" {
  rest_api_id             = "${aws_api_gateway_rest_api.stateful-serverless-api.id}"
  resource_id             = "${aws_api_gateway_rest_api.stateful-serverless-api.root_resource_id}"
  http_method             = "ANY"
  authorization           = "NONE"
  api_key_required        = true
}

resource "aws_api_gateway_integration" "stateful-serverless-api-root" {
  rest_api_id             = "${aws_api_gateway_rest_api.stateful-serverless-api.id}"
  resource_id             = "${aws_api_gateway_method.stateful-serverless-api-root.resource_id}"
  http_method             = "${aws_api_gateway_method.stateful-serverless-api-root.http_method}"
  integration_http_method = "POST"
  type                    = "AWS_PROXY"
  uri                     = "${aws_lambda_function.stateful-serverless-api.invoke_arn}"
}

resource "aws_api_gateway_deployment" "stateful-serverless-api" {
  depends_on = [
    aws_api_gateway_integration.stateful-serverless-api,
    aws_api_gateway_integration.stateful-serverless-api-root,
  ]
  rest_api_id = "${aws_api_gateway_rest_api.stateful-serverless-api.id}"
}

resource "aws_api_gateway_stage" "stateful-serverless-api" {
  deployment_id = aws_api_gateway_deployment.stateful-serverless-api.id
  rest_api_id   = aws_api_gateway_rest_api.stateful-serverless-api.id
  stage_name    = "prod"
}

resource "aws_lambda_permission" "stateful-serverless-api" {
  statement_id  = "AllowExecutionFromApiGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.stateful-serverless-api.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "arn:aws:execute-api:${var.AWS_REGION}:${var.AWS_ACCOUNT}:${aws_api_gateway_rest_api.stateful-serverless-api.id}/*"
  # http://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-control-access-using-iam-policies-to-invoke-api.html
}

# AWS API GATEWAY USAGE PLAN & API KEY ############################################################################################################################

resource "aws_api_gateway_usage_plan" "stateful-serverless-api" {
  name              = "stateful-serverless-api"
  description       = "Stateful Serverless Api Usage Plan"
  api_stages {
    api_id          = aws_api_gateway_rest_api.stateful-serverless-api.id
    stage           = aws_api_gateway_stage.stateful-serverless-api.stage_name
  }
  quota_settings {
    limit           = 100
    offset          = 0
    period          = "DAY"
  }
  throttle_settings {
    burst_limit     = 2
    rate_limit      = 1
  }
}

resource "aws_api_gateway_api_key" "stateful-serverless-api" {
  name = "stateful-serverless-api"
}

resource "aws_api_gateway_usage_plan_key" "stateful-serverless-api" {
  key_id        = aws_api_gateway_api_key.stateful-serverless-api.id
  key_type      = "API_KEY"
  usage_plan_id = aws_api_gateway_usage_plan.stateful-serverless-api.id
}

# AWS DYNAMODB ############################################################################################################################

resource "aws_dynamodb_table" "stateful-serverless-api" {
  name              = "stateful-serverless-api"
  billing_mode      = "PAY_PER_REQUEST"
  hash_key          = "name"
  attribute {
    name            = "name"
    type            = "S"
  }
  global_secondary_index {
    name            = "name-index"
    hash_key        = "name"
    projection_type = "ALL"
  }
  tags              = {
    provisioner     = "terraform"
    executioner     = "github-actions"
    project         = "stateful-serverless-api"
    url             = "https://github.com/parameshg/stateful-serverless-api"
  }
}