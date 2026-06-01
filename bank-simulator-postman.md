# Using the Bank Simulator With Postman

This guide shows how to use the provided bank simulator through the Payment Gateway API.

The important flow is:

```text
Postman -> PaymentGateway.Api -> IBankSimulatorClient -> Bank simulator
```

The simulator itself is defined in `imposters/bank_simulator.ejs`. The C# `BankSimulatorClient` does not replace it; it is only the HTTP client used by the gateway to call the simulator.

## 1. Start the Bank Simulator

From the repository root, run:

```powershell
docker-compose up
```

The simulator will listen on:

```text
http://localhost:8080/payments
```

You normally do not call this URL directly from Postman when testing the gateway. Instead, call the gateway API, and the gateway will call the simulator through `IBankSimulatorClient`.

## 2. Start the Payment Gateway API

From the repository root, run:

```powershell
dotnet run --project src\PaymentGateway.Api\PaymentGateway.Api.csproj
```

The launch settings expose the API on:

```text
https://localhost:7092
http://localhost:5067
```

If HTTPS certificate trust causes issues in Postman, use the HTTP URL:

```text
http://localhost:5067
```

## 3. Create a Payment in Postman

Create a new Postman request:

```text
POST http://localhost:5067/api/payments
```

Headers:

```text
Content-Type: application/json
```

Body type:

```text
raw JSON
```

## 4. Authorized Payment

Use a card number ending in an odd number: `1`, `3`, `5`, `7`, or `9`.

Example:

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

Expected gateway response:

```text
201 Created
```

Response body will include:

```json
{
  "status": "Authorized",
  "cardNumberLastFour": "1111"
}
```

The full card number and CVV are not returned.

## 5. Declined Payment

Use a card number ending in an even number: `2`, `4`, `6`, or `8`.

Example:

```json
{
  "cardNumber": "4111111111111112",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

Expected gateway response:

```text
201 Created
```

Response body will include:

```json
{
  "status": "Declined",
  "cardNumberLastFour": "1112"
}
```

## 6. Bank Simulator Unavailable/Error Response

Use a card number ending in `0`.

Example:

```json
{
  "cardNumber": "4111111111111110",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

The bank simulator returns `503 Service Unavailable`.

Expected gateway response:

```text
503 Service Unavailable
```

No payment is stored for this request.

## 7. Rejected Payment

Send invalid data to verify that gateway validation happens before the bank simulator is called.

Example:

```json
{
  "cardNumber": "123",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

Expected gateway response:

```text
400 Bad Request
```

Response body will include:

```json
{
  "status": "Rejected",
  "errors": [
    "Card number must be between 14 and 19 characters long."
  ]
}
```

Because the request is rejected by FluentValidation, the bank simulator is not called.

## 8. Retrieve a Payment

Copy the `id` returned from an authorized or declined payment response.

Create a new Postman request:

```text
GET http://localhost:5067/api/payments/{id}
```

Example:

```text
GET http://localhost:5067/api/payments/9d46b8e0-bf73-4077-975f-d5addce16d45
```

Expected response:

```text
200 OK
```

The response includes the payment status and last four card digits, but never the full card number.

## Optional: Call the Simulator Directly

You can call the simulator directly to verify it is running:

```text
POST http://localhost:8080/payments
```

Body:

```json
{
  "card_number": "2222405343248877",
  "expiry_date": "04/2027",
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```

Expected direct simulator response:

```json
{
  "authorized": true,
  "authorization_code": "generated-guid-value"
}
```

This direct call bypasses the Payment Gateway API, so it does not test gateway validation, storage, masking, or response mapping.
