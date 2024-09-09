using PaymentGateway.Api.Constants;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddSingleton<IPaymentRepository>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var logger = serviceProvider.GetRequiredService<ILogger<PaymentRepository>>();
    var bankUrl = builder.Configuration.GetValue<string>(Constants.BankEndpointKey);

    return string.IsNullOrEmpty(bankUrl)
        ? throw new InvalidOperationException("BankUrl is not configured.")
        : new PaymentRepository(logger, httpClientFactory, bankUrl);
});

builder.Services.AddSingleton<IPaymentService, PaymentService>();

var outputTemplateString = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Message:lj}{NewLine}{Exception}";
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt", outputTemplate: outputTemplateString)
    .WriteTo.Console(outputTemplate: outputTemplateString)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting payment gateway API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}