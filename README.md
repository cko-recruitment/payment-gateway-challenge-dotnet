# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

The API targets .NET 10 and C# 14. Running it without Docker, or running the tests, requires the .NET 10 SDK.

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.

## Bank simulator (optional)

Start the supplied bank simulator from the repository root when you want to process payments locally:

```sh
docker compose up -d
```

It is available at `http://localhost:8080`. Stop it with `docker compose down`. Compose only starts the simulator; the API can run separately either on the host or in a container.

## Run the API with the .NET SDK

With the .NET 10 SDK installed, run:

```sh
dotnet run --project src/PaymentGateway.Api
```

The API uses `http://localhost:8080/` as the bank URL by default. The launch profile serves Swagger UI at `https://localhost:7092/swagger`.

## Run the API in a container

Build the image from the repository root, then run it with Docker Desktop:

```sh
docker build -t payment-gateway-api .
docker run --rm --name payment-gateway-api -p 8081:8080 -e ASPNETCORE_ENVIRONMENT=Development -e AcquiringBank__BaseUrl=http://host.docker.internal:8080/ payment-gateway-api
```

The API is available at `http://localhost:8081`, with Swagger UI at `http://localhost:8081/swagger` and health at `http://localhost:8081/health`. Stop it with Ctrl+C. `host.docker.internal` lets the API container reach the simulator on the host's published port. On Linux Docker Engine, add `--add-host=host.docker.internal:host-gateway` to the `docker run` command.

## Run the tests

With the .NET 10 SDK installed, keep the bank simulator running and run:

```sh
dotnet test PaymentGateway.sln
```
