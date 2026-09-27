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

- **Validation policy:** GBP, USD and EUR stay within the assignment's three-currency limit. Amounts must be greater than zero, a deliberate restriction beyond its integer requirement. “Future” expiry means later than the current UTC month; accepting a card through its expiry month would be another reasonable policy.
- **Payment outcomes:** Invalid requests return `400` and are not stored, although the assignment calls them rejected payments. A bank decline is stored and returns `201` because it produces a retrievable payment. The bank client uses an authorization code or `null` for those two outcomes; a result type would express them more clearly if the contract grew.
- **Card data and persistence:** CVV goes to the bank but is excluded from the stored payment. The in-memory repository keeps the full card number and loses records on restart. A production design would need durable storage and a deliberate way to protect or avoid retaining the full number.
- **Bank failures:** Downstream errors produce a generic `500`, rather than exposing the bank's status or message or treating unavailability as a decline. The downstream status is logged; bank error text would need sanitizing before logging in production.
- **Operational scope:** Console traces and a basic `/health` endpoint are enough to inspect this local service, but health does not establish bank availability. Compose runs only the simulator so the API can be started either with `dotnet run` or in its own container.
- **Production safeguards:** A production API must authenticate merchants and authorize both payment creation and retrieval. Payment creation also needs durable, merchant-scoped idempotency keys. Preventing a second charge after a timeout requires the bank interaction to honor the same key or provide a way to reconcile an uncertain outcome.
