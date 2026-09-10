using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Invalid requests are reported as a Rejected payment response instead of the default 400 ValidationProblemDetails.
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<IPaymentsService, PaymentsService>();

var bankSimulatorBaseUrl = builder.Configuration["BankSimulator:BaseUrl"] ?? "http://localhost:8080";
builder.Services.AddHttpClient<IBankClient, BankClient>(client =>
{
    client.BaseAddress = new Uri(bankSimulatorBaseUrl);
});

var app = builder.Build();

// Swagger is enabled in all environments so reviewers can explore the API easily.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
