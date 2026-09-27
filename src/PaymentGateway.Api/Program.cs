using System.Text.Json.Serialization;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using PaymentGateway.Api.Middleware;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("PaymentGateway.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());
// Metrics support can be enabled as below.
//  .WithMetrics(metrics => metrics
//      .AddAspNetCoreInstrumentation()
//      .AddMeter("PaymentGateway.Api")
//      .AddConsoleExporter());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpClient<IAcquiringBankClient, AcquiringBankClient>(client =>
{
    string baseUrl = builder.Configuration["AcquiringBank:BaseUrl"] ?? "http://localhost:8080/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("AcquiringBank:TimeoutSeconds", 5));
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Production health check may probe the external dependency e.g. DB, messaging,...
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapControllers();

app.Run();