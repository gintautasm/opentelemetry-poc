### AWS Lambda with OpenTelemetry sample app

## Building

### Prerequisites

* Assumption, you have golang installed, AWS CLI 2 installed and configured
* Create docker network 
```
docker network create 'main-backend'
```
* OTEL collector extension built from source repo [here](https://github.com/open-telemetry/opentelemetry-lambda)
* copy collector output
```
cp ~/dev/opentelemetry-lambda/collector/build/extensions/collector ~/dev/opentelemetry-poc/lambda/otel-colletor-layer/extensions/collector
```

### Publish Lambda container

```
dotnet publish "$(pwd)/lambda" \
     --output "$(pwd)/lambda/./bin/Release/lambda-publish" \
     --configuration "Release" /p:GenerateRuntimeConfigurationFiles=true \
     --runtime linux-x64 \
     --self-contained False
```

### Build Docker container

```
cd ~/dev/opentelemetry-poc/lambda

docker build . -f "$(pwd)/Dockerfile" \
     --build-arg AWS_ACCESS_KEY_ID=$AWS_ACCESS_KEY_ID \
     --build-arg AWS_SECRET_ACCESS_KEY=$AWS_SECRET_ACCESS_KEY \
     -t search-handler-lambda:latest \
     --no-cache \
     --progress plain
```

### Run Docker container locally and call the function

```
docker run --rm -it -p 9000:8080 \
     -e DD_SITE="datadoghq.eu" \
     -e DD_API_KEY="d601...." \
     -e DD_TRACE_OTEL_ENABLED=true \
     -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT="0.0.0.0:43177" \
     -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT="0.0.0.0:43188" \
     --network="main-backend" search-handler-lambda


curl -XPOST "http://localhost:9000/2015-03-31/functions/function/invocations" -d '"hello world"'
```

### Docker build cleanup

```
docker image prune
```

### Push docker container to AWS ECR

Push will only work if ECR repository exists.
```
export AWS_PROFILE=my-aws-profile
export AWS_ACCOUNT=$(aws sts get-caller-identity --query 'Account' --output text)
export AWS_REGION=eu-west-1

aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT.dkr.ecr.$AWS_REGION.amazonaws.com

docker tag search-handler-lambda:latest $AWS_ACCOUNT.dkr.ecr.$AWS_REGION.amazonaws.com/opentelemetry-poc:latest

docker push $AWS_ACCOUNT.dkr.ecr.$AWS_REGION.amazonaws.com/opentelemetry-poc:latest

aws lambda update-function-code \
  --function-name opentelemetry_poc_lambda_function \
  --image-uri $AWS_ACCOUNT.dkr.ecr.$AWS_REGION.amazonaws.com/opentelemetry-poc:latest
```

## Deploying to AWS

Use terraform for deploying AWS Lambda 
```bash
cd ~/dev/opentelemetry-poc/lambda/iac

terraform apply -auto-approve
terraform destroy -auto-approve
```

## Invoke deployed Lambda

```
echo -n \"Time is `date +%F_%H:%M:%S`\" > payload.json \
&& aws lambda invoke \
    --function-name opentelemetry_poc_lambda_function \
    --payload file://payload.json \
    --cli-binary-format raw-in-base64-out \
    output.txt \
    && cat output.txt
```