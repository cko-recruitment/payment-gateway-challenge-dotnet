# Payment Gateway Challenge - .NET

A payment gateway API built in .NET 8 as part of the Checkout.com engineering assessment. The gateway allows merchants to process card payments and retrieve payment details, integrating with a simulated acquiring bank.

> The full challenge README is in the Checkout-challenge-README.md file of the project
---
## Running the project

To build the solution
```
dotnet build
```

To run the solution
```
dotnet run --project src/PaymentGateway.Api/PaymentGateway.Api.csproj
```

To run all the tests
```
dotnet test
```

To run docker the banking simulator
```
docker-compose up
```

### cURL

Update the last card number to see the change in statuses.

1: Authorized
2: Declined
0: Service Unavailable

To make requests in the terminal. The IdempotencyKey is optional.
```
bash
curl -X 'POST' \
  'https://localhost:7092/api/Payments' \
  -H 'accept: text/plain' \
  -H 'Idempotency-Key: 53CA5A76-87D9-4530-91A4-3CAF8ACB3648' \
  -H 'Content-Type: application/json' \
  -d '{
  "cardNumber": "123456789000000001",
  "expiryMonth": 0,
  "expiryYear": 2030,
  "currency": "usd",
  "amount": 1050,
  "cvv": "456"
}'
```

---
## Architecture

The solution uses the Repository pattern to separate payment storage concerns from business logic while keeping the project simple. An in-memory repository fulfils the storage requirements of this exercise without introducing unnecessary infrastructure complexity.

iDesign, Domain-Driven Design and N-tier architectures were considered but ruled out as the brief explicitly asks to avoid over-engineering and focus on the functional requirements.

---
## Project Structure

The project follows a layered architecture with clear separation of concerns. Each directory has a well-defined responsibility, making the codebase straightforward to navigate and extend.

- **Common**: Project-wide utilities including extension methods, abstractions (e.g. `IGuidGenerator` for testability), and mapping helpers used across layers.
- **Controllers**: Exposes the Payment Gateway's HTTP endpoints, handling request routing and delegating processing to the service layer.
- **Enums**: Centralised enumeration types (`PaymentStatus`, `PaymentErrorType`) shared across the application.
- **Infrastructure**: Encapsulates all external and internal data access concerns.
    - **Clients**: Adapters for third-party integrations, in this case, the Bank Simulator client.
    - **Repositories**: Internal data stores.
- **Models**: Strongly-typed data structures scoped by their role in the request lifecycle.
    - **Domain**: Core business entities (`Payment`, `PaymentError`) that represent the application's internal model.
    - **Requests**: Input shapes for incoming API and downstream service calls.
    - **Responses**: Output shapes returned to API consumers and received from external services.
- **Services**: Contains the business logic layer (`PaymentService`), orchestrating between repositories, clients, and domain models. Consumed directly by the controller.
- **Validation**: Dedicated validation logic for incoming requests, keeping validation rules decoupled from controllers and services.

```
// Payment Gateway

.
├── Common
│   ├── Extensions
│   │   ├── BankResultExtension.cs
│   │   ├── PostPaymentRequestExtension.cs
│   │   └── ResultExtensions.cs
│   ├── GuidGenerator
│   │   └── IGuidGenerator.cs
│   └── Mapping
│       └── PaymentMappingExtensions.cs
├── Controllers
│   └── PaymentsController.cs
├── Enums
│   ├── PaymentErrorType.cs
│   └── PaymentStatus.cs
├── Infrastructure
│   ├── Clients
│   │   └── BankSimulator
│   │       ├── BankSimulator.cs
│   │       └── IBankSimulator.cs
│   └── Repositories
│       └── Payment
│           ├── IPaymentRepository.cs
│           └── PaymentsRepository.cs
├── Models
│   ├── Domain
│   │   ├── Payment.cs
│   │   └── PaymentError.cs
│   ├── Requests
│   │   ├── BankSimulatorRequest.cs
│   │   └── PostPaymentRequest.cs
│   └── Responses
│       ├── BankSimulatorResponse.cs
│       └── PaymentResponse.cs
├── Properties
│   └── launchSettings.json
├── Services
│   └── PaymentService.cs
├── Validation
│   └── PostPaymentRequestValidator.cs
├── appsettings.Development.json
├── appsettings.json
├── PaymentGateway.Api.csproj
└── Program.cs

// Tests

.
├── Helpers
│   └── PaymentTestDataBuilder.cs
├── Integration
│   └── PaymentsControllerTests.cs
├── Unit
│   ├── PaymentServiceTests.cs
│   └── PostPaymentRequestValidatorTests.cs
├── PaymentGateway.Api.Tests.csproj
└── Usings.cs
```

---
## Extra Mile Features

- **Idempotency**: an optional `Idempotency-Key` header prevents duplicate payments from being processed. If a request is received with a key that matches an existing payment, a `409 Conflict` is returned immediately without calling the bank. Keys expire after 24 hours, matching the behaviour of Checkout.com's own API.
- **Authorization code storage**: the `authorization_code` returned by the bank on successful payments is persisted alongside the payment record. This could be used as a means of reconciliation and dispute resolution by banks and merchants.
- **Correlation IDs**: each valid payment is assigned a `CorrelationID` when being processed by the `PaymentService`. It is included in all related log entries and stored with the `CorrelationId` as the Id for a payment. This makes it straightforward to trace a payment end-to-end across both logs and the in-memory store.
---
## Out of Scope / Future Enhancements

These were excluded to stay within the scope of the brief but would be required in a production system:

- **Authentication / Authorisation**: signed JWT tokens validated independently per service, enabling a stateless and scalable auth model.
- **Persistent storage**: a relational DB (Postgres, SQL Server) or NoSQL store (MongoDB) to ensure payments survive restarts.
- **Input hardening**
	- Rate limiting: prevent a merchant (or attacker) hammering the endpoint with thousands of requests. .NET has `AspNetCoreRateLimit` for this.
	- Request size limits: reject payloads over a certain size before they even call the controller.
- **Security**
	- Card number should be tokenised: store a token that maps to the real card number held by a PCI-compliant vault, never the raw number.
	- Scoped permissions: a merchant can only retrieve their own payments, not another merchant's.
- **Observability**
	- Alerting on unusual patterns: spike in declined payments from one merchant could indicate card testing fraud.
	- Audit log: immutable record of every payment attempt, who made it, and what happened.
	- More robust observability with tools like OpenTelemetry for structured logs, distributed traces, and metrics, replacing basic console logging.
- **Resilience / retries**: exponential backoff for transient bank failures (e.g. `503` responses) rather than immediately surfacing errors to the caller.
- **Multiple acquiring banks**: the gateway currently supports a single bank; production systems route to multiple providers.
- **Merchant / payer identification**: associating payments with a specific merchant or customer context.

---
## Assumptions

- Expiry year must be a 4-figure integer (e.g. `2030`). Shortened values (e.g. `30`) are rejected.

---
## HTTP status code scenarios

Success - HTTP 200
- Retrieving an existing payment successfully.
- Successfully processing a payment request (this includes both authorized and declined statuses from the bank).

Bad Request - HTTP 400
- Validation errors in the payment request (e.g., invalid card number, expiry date, currency, amount, or CVV).

Not Found - HTTP 404
- Attempting to retrieve a payment using an ID that does not exist in the system.

Conflict - HTTP 409
- Attempting to process a payment using an idempotency key that has already been used for a previous request.

Internal Server Error - HTTP 500
- Unexpected system errors.

Service Unavailable - HTTP 503
- Bank simulator service is unavailable or a connection error occurs.

---
## Error Handling

FluentResults is used to keep error handling explicit and consistent. Both validation failures and business logic errors are surfaced in the same format, making API responses predictable for clients.

---
## Logging

Structured logging is used to provide consistent, queryable log output without exposing sensitive data.

- **Correlation IDs**: each valid payment is assigned a correlation ID, which is included in all related log entries, making it straightforward to trace a payment end-to-end.
- **Sensitive data**: sensitive data is not logged. IDs and statuses are referenced where appropriate.
- **Log levels**: `Information` for normal payment flows, `Warning` for duplicate idempotency key attempts and validation, and `Error` for bank simulator failures.

---
## Testing

The project has two levels of test coverage:

- **Integration tests** on the controller: verifying end-to-end request/response behaviour including routing, status codes, and response shapes for authorised, declined, and rejected scenarios.
- **Unit tests** on the service and validation layers: covering individual validation rules and payment processing logic in isolation.

---
## Dependencies

The following libraries were added beyond the project's defaults:

|Library|Purpose|
|---|---|
|[FluentResults](https://github.com/altmann/FluentResults)|Result-based error handling without exceptions|
|[FluentValidation](https://docs.fluentvalidation.net/en/latest/)|Declarative validation rules for payment requests|
|[FluentAssertions](https://fluentassertions.com/)|Readable test assertions|
