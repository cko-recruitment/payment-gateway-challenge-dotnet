using PaymentGateway.Api.Common.GuidGenerator;
using PaymentGateway.Api.Infrastructure.Clients.BankSimulator;
using PaymentGateway.Api.Repositories.Payment;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();
builder.Services.AddSingleton<IGuidGenerator, GuidGenerator>();
builder.Services.AddScoped<PaymentService>();

builder.Services.AddHttpClient<IBankSimulator, BankSimulator>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();