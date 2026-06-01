# Gateway API design

## Endpoints

The API exposes two main endpoints:
```
POST /api/payments
GET /api/payments/{id}
```

### POST `/api/payments`

Processes a payment request.

Request body:

```json
{
  "cardNumber": "4111111111111111",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

Successful bank response:

- `201 Created` when the bank simulator returns an authorization decision.
- `status` is `Authorized` when the bank authorizes the payment.
- `status` is `Declined` when the bank declines the payment.

Example response:

```json
{
  "id": "598a6965-6c41-4654-b54e-fd6373826206",
  "status": "Authorized",
  "cardNumberLastFour": "1111",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 100,
  "errors": []
}
```

Validation failure:

- `400 Bad Request`
- `status` is `Rejected`.
- Validation happens before the bank simulator is called.

Example response:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "status": "Rejected",
  "cardNumberLastFour": "",
  "expiryMonth": 0,
  "expiryYear": 0,
  "currency": "",
  "amount": 0,
  "errors": [
    "Card number must be between 14 and 19 characters long."
  ]
}
```

Bank simulator unavailable:

- `503 Service Unavailable`
- No payment is stored.

### GET `/api/payments/{id}`

Retrieves a stored payment.

Responses:

- `200 OK` with payment details when found.
- `404 Not Found` when no payment exists for the id.

Example response:

```json
{
  "id": "598a6965-6c41-4654-b54e-fd6373826206",
  "status": "Authorized",
  "cardNumberLastFour": "1111",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 100
}
```

## Validation

Validation is implemented with FluentValidation in `PostPaymentRequestValidator`.

Rules are taken from instructions.mdand are following:

- Card number is required, numeric only, and 14 to 19 characters.
- Expiry month is 1 to 12.
- Expiry year is required, and the expiry month/year combination must be in the future.
- Currency is required, exactly 3 characters. My submission must validate against no more than 3 currency codes. I limited to `GBP`, `USD`, or `EUR`.
- Amount must be an integer greater than 0 in the minor currency unit.
- CVV is required, numeric only, and 3 to 4 characters.

## Architecture

The flow is:

```text
Postman request -> PaymentGateway.Api -> IBankSimulatorClient -> Bank simulator
```

- External bank simulator calls are behind `IBankSimulatorClient`.
- Payment storage is behind `IPaymentsRepository`.
- Payment orchestration is handled by `IPaymentService`+`PaymentService`.
- Validators run in `PaymentService` before the bank simulator is called, so if validation fails, no call reaches simulator.

## Testing
The testing requests in Postman were following:
```
POST http://localhost:5067/api/payments
```

The body raw JSON:
```
{
  "cardNumber": "4111111111111111",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

Headers:

```
Content-Type: application/json
```

### 1. Start the Bank Simulator

From the repository root, run:

```
docker-compose up
```

The simulator will listen on:

```text
http://localhost:8080/payments
```

### 2. Start the Payment Gateway API

From the repository root, run:

```powershell
dotnet run --project src\PaymentGateway.Api\PaymentGateway.Api.csproj
```

The launch settings expose the API on:

```text
https://localhost:7092
http://localhost:5067
```

## Observability and logging

I considered full production-grade logging  asout of scope for this assessment but the implementation includes lightweight logging around:
- incoming controller calls;
- outgoing calls from the payment gateway to the bank simulator;
- bank simulator availability/failure cases.

The intention was to make the payment flow easier to trace during local testing while keeping the implementation simple and focused on the functional requirements.
