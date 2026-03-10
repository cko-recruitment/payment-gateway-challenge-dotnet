# Checkout.com Engineering Assessment

Building world class, high performance, payments infrastructure requires a passionate, thriving and talented engineering community, this is where your journey begins.

We are passionate about giving candidates the opportunity to showcase varied, job relevant skills, across our assessment process - including our coding/technical assessment. To help you prepare for this, we have created this repository to provide you with some guidance on what to expect and how to prepare for the assessment.

- [What languages we support](#what-languages-we-support)
- [Assessment Overview](#assessment-overview)
  - [Live coding interview preparation](#live-coding-interview-preparation)
  - [Offline coding interview praparation](#offline-coding-interview-preparation)
- [Assessment](#assessment)  
  - [Building a payment gateway](#building-a-payment-gateway)
  - [Requirements](#requirements)
    - [Processing a payment](#processing-a-payment)
    - [Retrieving a payment’s details](#retrieving-a-payments-details)
  - [Documentation](#documentation)
  - [Implementation considerations](#implementation-considerations)
  - [Bank simulator](#bank-simulator)
    - [Starting the simulator](#starting-the-simulator)
    - [Calling the simulator](#calling-the-simulator)
    - [Test cards](#test-cards)
    - [Implementation detail](#implementation-detail)
- [Get help](#get-help)

## What languages we support

A variety of languages are supported for the assessment, please select the language you are most comfortable with from the list below and follow the instructions to complete the assessment.

* [.NET](https://github.com/cko-recruitment/payment-gateway-challenge-dotnet)
* [Go](https://github.com/cko-recruitment/payment-gateway-challenge-go)
* [Java](https://github.com/cko-recruitment/payment-gateway-challenge-java)
* [Python](https://github.com/cko-recruitment/payment-gateway-challenge-python)

If you would like to use a different language, please contact the recruiter who is responsible for managing your individual process.

Best of luck with any current or future assessment you undertake with Checkout.com.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Assessment Overview
There are two variations of this challenge: 
1. A live coding variation where you will pair with a Checkout Senior Engineer to solve the challenge live
2. A take home variation where you will complete the challenge ahead of the interview, and then demonstrate and review the solution with a Checkout Senior Engineer.

You should have already indicated to the Checkout talent team which variation you have chosen.

In either case, please **do take a look at the details of this project** and familiarize yourself with the requirements and the documentation.

### Live coding interview preparation
If you have opted for the live coding variation please prepare your development environment so that you are comfortable and ready to start writing code (IDE setup, all libraries and dependencies ready).

### Offline coding interview preparation
If you have opted for the offline coding variation please ensure that you have shared your solution (instructions on how to do this are given to you by the Checkout talent team), and that you are ready to demo/review your solution.

Please do not create pull requests against our cko-recruitment repositories.

We look forward to working through this challenge together. 

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Assessment
### Building a payment gateway
E-Commerce is experiencing exponential growth and merchants who sell their goods or services online need a way to easily collect money from their customers.

We would like to build a payment gateway, an API based application that will allow a merchant to offer a way for their shoppers to pay for their product.

Processing a card payment online involves multiple steps and entities:

<div align="center">
  <br>
  <br>
  <img src="https://github.com/cko-recruitment/.github/blob/main/images/card_payment_overview.png" alt="Card payment overview">
  <br>
  <br>
</div>


**Shopper:** Individual who is buying the product online.

**Merchant:** The seller of the product. For example, Apple or Amazon.

**Payment Gateway:** Responsible for validating requests, storing card information and forwarding payment requests and accepting payment responses to and from the acquiring bank.

**Acquiring Bank:** Allows us to do the actual retrieval of money from the shopper’s card and pay out to the merchant. It also performs some validation of the card information and
then sends the payment details to the appropriate 3rd party organization for processing.

We will be building the payment gateway only and simulating the acquiring bank component in order to allow us to fully test the payment flow.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Requirements
The product requirements for this initial phase are the following:

- A merchant should be able to process a payment through the payment gateway and receive one of the following types of response:
    - Authorized - the payment was authorized by the call to the acquiring bank
    - Declined - the payment was declined by the call to the acquiring bank
    - Rejected - No payment could be created as invalid information was supplied to the payment gateway and therefore it has rejected the request without calling the acquiring bank
-  A merchant should be able to retrieve the details of a previously made payment

<p align="right">(<a href="#readme-top">back to top</a>)</p>

#### Processing a payment
The payment gateway will need to provide merchants with a way to process a card payment. To do this, the merchant should be able to submit a request to the payment gateway. A payment request must include the following fields:

| Field        | Validation rules                     | Notes                                                                                                                                                                               |
|--------------|--------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Card number  | Required                             |                                                                                                                                                                                     |
|              | Between 14-19 characters long        |                                                                                                                                                                                     |
|              | Must only contain numeric characters |                                                                                                                                                                                     |
| Expiry month | Required                             |                                                                                                                                                                                     |
|              | Value must be between 1-12           |                                                                                                                                                                                     |
| Expiry year  | Required                             |                                                                                                                                                                                     |
|              | Value must be in the future          | Ensure the combination of expiry month + year is in the future                                                                                                                      |
| Currency     | Required                             | Refer to the list of [ISO currency codes](https://www.xe.com/iso4217.php). Ensure your submission validates against no more than 3 currency codes                                   |
|              | Must be 3 characters                 |                                                                                                                                                                                     |
| Amount       | Required                             | Represents the amount in the minor currency unit. For example, if the currency was USD then <ul><li>$0.01 would be supplied as 1</li><li>$10.50 would be supplied as 1050</li></ul> |
|              | Must be an integer                   |                                                                                                                                                                                     |
| CVV          | Required                             |                                                                                                                                                                                     |
|              | Must be 3-4 characters long          |                                                                                                                                                                                     |
|              | Must only contain numeric characters |                                                                                                                                                                                     |

Responses for payments that were sucessfully sent to the acquiring bank must include the following fields:

| Field                 | Notes                                                                                                                                                                               |
|-----------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Id                    | This is the payment id which will be used to retrieve the payment details. Feel free to choose whatever format you think makes most sense e.g. a GUID is fine                       |
| Status                | Must be one of the following values `Authorized`, `Declined`                                                                                                                        |
| Last four card digits | Payment gateways cannot return a full card number as this is a serious compliance risk. However, it is fine to return the last four digits of a card                                |
| Expiry month          |                                                                                                                                                                                     |
| Expiry year           |                                                                                                                                                                                     |
| Currency              | Refer to the list of [ISO currency codes](https://www.xe.com/iso4217.php). Ensure your submission validates against no more than 3 currency codes                                   |
|                       |                                                                                                                                                                                     |
| Amount                | Represents the amount in the minor currency unit. For example, if the currency was USD then <ul><li>$0.01 would be supplied as 1</li><li>$10.50 would be supplied as 1050</li></ul> |

<p align="right">(<a href="#readme-top">back to top</a>)</p>


#### Retrieving a payment’s details
The second requirement for the payment gateway is to allow a merchant to retrieve details of a previously made payment using its identifier. Doing this will help the merchant with their reconciliation and reporting needs. The response should include a masked card number and card details along with a status code which indicates the result of the payment.

| Field                 | Notes                                                                                                                                                                               |
|-----------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Id                    | This is the payment id which will be used to retrieve the payment details. Feel free to choose whatever format you think makes most sense e.g. a GUID is fine                       |
| Status                | Must be one of the following values `Authorized`, `Declined`                                                                                                                        |
| Last four card digits | Payment gateways cannot return a full card number as this is a serious compliance risk. However, it is fine to return the last four digits of a card                                |
| Expiry month          |                                                                                                                                                                                     |
| Expiry year           |                                                                                                                                                                                     |
| Currency              | Refer to the list of [ISO currency codes](https://www.xe.com/iso4217.php). Ensure your submission validates against no more than 3 currency codes                                   |
|                       |                                                                                                                                                                                     |
| Amount                | Represents the amount in the minor currency unit. For example, if the currency was USD then <ul><li>$0.01 would be supplied as 1</li><li>$10.50 would be supplied as 1050</li></ul> |

**Note: Payment Storage**

You do not need to integrate with a real storage engine or database. It is fine to use the test double repository provided in the sample code to represent this.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Documentation
Please document your key design considerations and assumptions made when the test is performed as an offline take-home exercise.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Implementation considerations
We expect the following with each submission:
- Code must compile
- Your code is covered by automated tests. It is your choice which type of tests and the number of them you want to implement.
- The code to be simple and maintainable. We do not want to encourage over-engineering.
- Your API design and architecture should be focused on meeting the functional requirements outlined above.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Bank simulator
A bank simulator is provided. The simulator provides responses based on the request:

- If any of the required fields is missing from the request the simulator returns a `400 Bad Request` status code with an error message.
- If all fields are present, then the response will be dependent on the provided card number:
  - If the card number ends on an odd number (1, 3, 5, 7, 9) then the simulator returns an `200 Ok` authorized response with a new random `authorization_code`
  - If the card number ends on an even number (2, 4, 6, 8) then the simulator returns an `200 Ok` unauthorized response
  - If the card number ends on a zero (0) then the simulator returns an error in the form of a `503 Service Unavailable` response

<p align="right">(<a href="#readme-top">back to top</a>)</p>

#### Starting the simulator
To start the simulator, run `docker-compose up`

<p align="right">(<a href="#readme-top">back to top</a>)</p>

#### Calling the simulator

The simulator supports a single route which is a HTTP POST to the following URI:
```
http://localhost:8080/payments
```

The JSON snippet below shows an example of the body that is expected to be submitted:

```json
{
  "card_number": "2222405343248877",
  "expiry_date": "04/2025",
  "currency": "GBP",
  "amount": 100,
  "cvv": "123"
}
```
A response will be provided with the following structure:

```json
{
  "authorized": true,
  "authorization_code": "0bb07405-6d44-4b50-a14f-7ae0beff13ad"
}
```
<p align="right">(<a href="#readme-top">back to top</a>)</p>



#### Implementation detail
*This section isn't required reading. It is just additional context on how the simulator is implemented*

The simulator is implemented using [Mountebank](https://github.com/mountebank-testing/mountebank) which provides a programmable test double.

The configuration is stored in the `imposters` directory of this repo as an [ejs template](https://ejs.co/). Typically
engineers would not use an EJS template, however for this test it works well. The preferred way to use Mountebank or
similar products (e.g. WireMock) is to call it's API during your test setup via a client library.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Get help
If you have any questions or queries on any of the above, please contact the recruiter who is responsible for managing your individual process or careers@checkout.com.

<p align="right">(<a href="#readme-top">back to top</a>)</p>