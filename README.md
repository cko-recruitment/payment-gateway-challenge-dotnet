# Payment Gateway API

An ASP.NET Core API for the [Checkout.com payment gateway challenge](https://github.com/cko-recruitment/). It validates card payments, sends valid requests to the supplied bank simulator, and lets a merchant retrieve a processed payment by ID. The API targets .NET 10 and C# 14.

## Prerequisites

- Docker with Compose to run the bank simulator or build the API image.
- The .NET 10 SDK to run the API on the host or run tests.

The simulator is optional for starting the API and using `/health`, but it must be running to process payments and run the simulator-backed tests.

## Start the bank simulator

Start the supplied bank simulator from the repository root when you want to process payments locally:

```sh
docker compose up -d
```

The simulator listens at `http://localhost:8080`. Compose starts only the simulator. Stop it with `docker compose down`.

## Run the API with the .NET SDK

With the .NET 10 SDK installed, run:

```sh
dotnet run --project src/PaymentGateway.Api
```

The launch profile serves Swagger UI at `https://localhost:7092/swagger`. The API uses `http://localhost:8080/` as the bank URL by default. The bank URL and timeout can be overridden with `AcquiringBank:BaseUrl` and `AcquiringBank:TimeoutSeconds`.

## Run the API in a container

Build the image from the repository root, then run it with Docker Desktop:

```sh
docker build -t payment-gateway-api .
docker run --rm --name payment-gateway-api -p 8081:8080 -e ASPNETCORE_ENVIRONMENT=Development -e AcquiringBank__BaseUrl=http://host.docker.internal:8080/ payment-gateway-api
```

Swagger UI is at `http://localhost:8081/swagger` and the health endpoint is at `http://localhost:8081/health`. Stop the API container with Ctrl+C. The container uses HTTP locally because no HTTPS certificate is configured in the image. `host.docker.internal` lets it reach the simulator through the host's published port. On Linux Docker Engine, add `--add-host=host.docker.internal:host-gateway` to the `docker run` command.

## Use the API

Send `POST /api/payments` with a JSON body such as:

```json
{
  "cardNumber": "1234567890121235",
  "expiryMonth": 12,
  "expiryYear": 2030,
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

`amount` is in minor currency units, so `100` GBP means GBP 1.00. With the supplied simulator, a card number ending in an odd digit is authorized and one ending in an even non-zero digit is declined. In either case the API responds with `201 Created`. For the sample request, the response has this shape (with a generated ID):

```json
{
  "id": "6b9d4c67-426f-4dc5-8cf3-8e032ad47dd4",
  "status": "Authorized",
  "cardNumberLastFour": "1235",
  "expiryMonth": 12,
  "expiryYear": 2030,
  "currency": "GBP",
  "amount": 100
}
```

Use `GET /api/payments/{id}` to retrieve the same fields; an unknown ID returns `404`.

Invalid requests return `400` without calling the bank. Bank failures, including the simulator's `503` response for card numbers ending in zero, return a generic `500` response and do not create a payment. The response does not expose the bank's error message.

## Run the tests

With the bank simulator running, use:

```sh
dotnet test PaymentGateway.sln
```

Validation rules are covered by fast unit tests. API tests use `WebApplicationFactory`; bank error cases use a hand-written HTTP handler, while several payment-flow tests call the supplied simulator at `localhost:8080`. Those tests do not call an API container.

## Design decisions and trade-offs

- **Validation:** `PaymentValidator` contains the rules without HTTP or bank dependencies, and `TimeProvider` makes expiry behavior testable. Only GBP, USD and EUR are supported, as the assignment limits validation to at most three currencies. Amounts must be positive integers in minor units; rejecting zero and negative amounts is an explicit choice beyond the assignment's integer requirement.
- **Expiry:** The month and year must be strictly later than the current UTC month and year. This interprets the assignment's word “future” literally, so a card expiring in the current month is rejected. A production card policy might instead accept it through month-end; that would require changing the validator and its boundary tests.
- **Payment outcomes:** Rejected requests are not stored, so `Rejected` is an API validation outcome rather than a persisted payment status. There is no `Pending` status because a payment is stored only after the bank returns an authorized or declined result. The bank client uses a nullable authorization code for that two-outcome response; exceptions represent bank or transport failures.
- **Storage:** Payments are kept in an in-memory repository, as allowed by the assignment. Data is lost when the API restarts. The outbound bank request carries the CVV for authorization, but the stored payment does not retain it. Responses expose only the last four card digits. The stored model still keeps the full card number; a real gateway would need a deliberate card-data retention design.
- **Failures and observability:** Bank-response exceptions produce a generic payment error and retain the downstream status in logs; unexpected exceptions use a generic API error. Console logging and OpenTelemetry console traces cover inbound requests and outbound HTTP calls. `/health` reports that the API is running; it does not probe bank availability. Custom metrics are not configured.
