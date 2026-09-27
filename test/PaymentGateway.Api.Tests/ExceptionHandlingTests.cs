using System.Net;
using System.Net.Http.Json;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class ExceptionHandlingTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task Bank_unsuccessful_status_returns_payment_error_500(HttpStatusCode bankStatus)
    {
        await using var factory = CreateFactory(BankResponds(bankStatus, "{\"error_message\":\"downstream-private-details\"}"));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred while processing the payment.", instance: null);
    }

    [Fact]
    public async Task Bank_unavailable_returns_payment_error_500()
    {
        await using var factory = CreateFactory(BankResponds(HttpStatusCode.ServiceUnavailable, "{}"));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred while processing the payment.", instance: null);
    }

    [Fact]
    public async Task Bank_connection_failure_returns_generic_500()
    {
        await using var factory = CreateFactory(new BankHandler(_ => throw new HttpRequestException("downstream-private-details")));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Fact]
    public async Task Bank_request_timeout_returns_generic_500()
    {
        await using var factory = CreateFactory(new BankHandler(_ => throw new TaskCanceledException("downstream-private-details")));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Fact]
    public async Task Bank_gateway_timeout_returns_payment_error_500()
    {
        await using var factory = CreateFactory(BankResponds(HttpStatusCode.GatewayTimeout, "{}"));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred while processing the payment.", instance: null);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"authorized\":true}")]
    [InlineData("{\"authorized\":true,\"authorization_code\":\"\"}")]
    [InlineData("{\"authorized\":true,\"authorization_code\":\" \"}")]
    public async Task Bank_missing_authorization_details_returns_payment_error_500(string body)
    {
        using var factory = CreateFactory(BankResponds(HttpStatusCode.OK, body));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred while processing the payment.", instance: null);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("{\"authorized\":\"invalid\"}")]
    public async Task Bank_malformed_json_returns_generic_500(string body)
    {
        await using var factory = CreateFactory(BankResponds(HttpStatusCode.OK, body));
        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Theory]
    [InlineData("error_message")]
    [InlineData("errorMessage")]
    public async Task Bank_bad_request_message_is_not_exposed(string field)
    {
        var body = System.Text.Json.JsonSerializer.Serialize(
            new Dictionary<string, string> { [field] = "downstream-private-details" });
        await using var factory = CreateFactory(BankResponds(HttpStatusCode.BadRequest, body));
        await AssertProblem(factory, HttpStatusCode.InternalServerError,
            "An unexpected error occurred while processing the payment.", instance: null);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Unhandled_exception_returns_generic_500(string environment)
    {
        await using var factory = CreateFactory(
            new BankHandler(_ => throw new InvalidOperationException("downstream-private-details")), environment);

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Theory]
    [InlineData(true, "bank-authorization-123", PaymentStatus.Authorized)]
    [InlineData(false, "", PaymentStatus.Declined)]
    public async Task Bank_outcome_returns_created_payment(bool authorized, string code, PaymentStatus status)
    {
        await using var factory = CreateFactory(new BankHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { authorized, authorization_code = code })
        })));
        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync("/api/payments", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        Assert.NotNull(payment);
        Assert.Equal(status, payment!.Status);
        Assert.Equal("1235", payment.CardNumberLastFour);
        Assert.Equal("GBP", payment.Currency);
        Assert.Equal(100, payment.Amount);
        Assert.NotNull(response.Headers.Location);

        var stored = Assert.Single(factory.Services.GetRequiredService<PaymentsRepository>().Payments);
        Assert.Equal(authorized ? code : null, stored.BankAuthorization.AuthorizationCode);

        using var retrieved = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, retrieved.StatusCode);
        Assert.Equal(await response.Content.ReadAsStringAsync(), await retrieved.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Validation_failure_keeps_its_400_response()
    {
        await using var factory = CreateFactory(new BankHandler(_ => throw new InvalidOperationException("Bank must not be called")));
        using var client = CreateClient(factory);
        var request = ValidRequest();
        request.Amount = 0;

        using var response = await client.PostAsJsonAsync("/api/payments", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.Equal("Payment request is invalid.", problem.Title);
        Assert.Equal("Field 'Amount' is invalid: Amount is required and must be a positive integer in minor units.", problem.Detail);
    }

    [Fact]
    public async Task Caller_cancellation_is_not_converted_to_bank_timeout()
    {
        using var httpClient = new HttpClient(new BankHandler(token => Task.FromCanceled<HttpResponseMessage>(token)))
        {
            BaseAddress = new Uri("http://bank.test/")
        };
        var bank = new AcquiringBankClient(httpClient);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            bank.AuthorizeAsync(ValidRequest().ToPayment(), cancellation.Token));
    }

    private static async Task AssertProblem(WebApplicationFactory<PaymentsController> factory, HttpStatusCode status, string title, string? instance = "/api/payments")
    {
        using var client = CreateClient(factory);
        using var response = await client.PostAsJsonAsync("/api/payments", ValidRequest());

        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)status, problem!.Status);
        Assert.Equal(title, problem.Title);
        Assert.Equal(instance, problem.Instance);
        Assert.Null(problem.Detail);
        // The controller filter omits optional problem metadata; the global handler supplies it.
        if (instance is null)
            Assert.Null(problem.Type);
        else
            Assert.False(string.IsNullOrWhiteSpace(problem.Type));
        Assert.DoesNotContain("downstream-private-details", body);
        Assert.DoesNotContain("Exception", body);
        Assert.Empty(factory.Services.GetRequiredService<PaymentsRepository>().Payments);
    }

    private static WebApplicationFactory<PaymentsController> CreateFactory(BankHandler handler, string environment = "Development") =>
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            // Tests verify responses and do not need machine-specific logging providers.
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<TimeProvider>(new FixedTimeProvider());
                services.AddHttpClient<IAcquiringBankClient, AcquiringBankClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler);
            });
        });

    private static HttpClient CreateClient(WebApplicationFactory<PaymentsController> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

    private static BankHandler BankResponds(HttpStatusCode status, string body) => new(_ =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        }));

    private static CreatePaymentRequest ValidRequest() => new()
    {
        CardNumber = "1234567890121235",
        ExpiryMonth = 10,
        ExpiryYear = 2026,
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };

    private sealed class BankHandler(Func<CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            respond(cancellationToken);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);
    }
}
