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
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class ExceptionHandlingTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{\"error_message\":\"downstream-private-details\"}")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "{}")]
    public async Task Bank_http_error_returns_payment_error_500(HttpStatusCode bankStatus, string body)
    {
        await using var factory = CreateFactory(BankResponds(bankStatus, body));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred while processing the payment.");
    }

    [Fact]
    public async Task Bank_connection_failure_returns_generic_500()
    {
        await using var factory = CreateFactory(new BankHandler(_ => throw new HttpRequestException("downstream-private-details")));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Fact]
    public async Task Bank_timeout_returns_generic_500_without_storing_a_payment()
    {
        bool bankCallWasCancelled = false;
        await using var factory = CreateFactory(new BankHandler(async cancellationToken =>
        {
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            finally
            {
                bankCallWasCancelled = cancellationToken.IsCancellationRequested;
            }
        }), bankTimeout: TimeSpan.FromMilliseconds(100));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        Assert.True(bankCallWasCancelled);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"authorized\":true,\"authorization_code\":\" \"}")]
    public async Task Bank_incomplete_authorization_returns_payment_error_500(string body)
    {
        await using var factory = CreateFactory(BankResponds(HttpStatusCode.OK, body));

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred while processing the payment.");
    }

    [Fact]
    public async Task Bank_malformed_json_returns_generic_500()
    {
        await using var factory = CreateFactory(BankResponds(HttpStatusCode.OK, "not JSON"));
        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Fact]
    public async Task Unhandled_exception_returns_generic_500()
    {
        await using var factory = CreateFactory(
            new BankHandler(_ => throw new InvalidOperationException("downstream-private-details")), "Production");

        await AssertProblem(factory, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Fact]
    public async Task Validation_failure_keeps_its_400_response()
    {
        await using var factory = CreateFactory(new BankHandler(_ => throw new InvalidOperationException("Bank must not be called")));
        using var client = CreateClient(factory);
        var request = GetValidRequest();
        request.Amount = 0;

        using var response = await client.PostAsJsonAsync("/api/payments", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.Equal("Request is invalid.", problem.Title);
        Assert.Equal("Field 'Amount' is invalid: Amount is required and must be a positive integer in minor units.", problem.Detail);
    }

    private static async Task AssertProblem(WebApplicationFactory<PaymentsController> factory, HttpStatusCode status, string title)
    {
        using var client = CreateClient(factory);
        using var response = await client.PostAsJsonAsync("/api/payments", GetValidRequest());

        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)status, problem!.Status);
        Assert.Equal(title, problem.Title);
        Assert.Null(problem.Detail);
        Assert.DoesNotContain("downstream-private-details", body);
        Assert.DoesNotContain("Exception", body);
        Assert.Empty(factory.Services.GetRequiredService<PaymentsRepository>().Payments);
    }

    private static WebApplicationFactory<PaymentsController> CreateFactory(
        BankHandler handler, string environment = "Development", TimeSpan? bankTimeout = null) =>
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            // Tests verify responses and do not need machine-specific logging providers.
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<TimeProvider>(new FixedTimeProvider());
                services.AddHttpClient<IAcquiringBankClient, AcquiringBankClient>()
                    .ConfigureHttpClient(client => client.Timeout = bankTimeout ?? TimeSpan.FromSeconds(5))
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

    private static CreatePaymentRequest GetValidRequest() => new()
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