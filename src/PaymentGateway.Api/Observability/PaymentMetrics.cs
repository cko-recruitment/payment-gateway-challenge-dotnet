using System.Diagnostics;
using System.Diagnostics.Metrics;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Observability;

public static class PaymentMetrics
{
    public const string MeterName = "PaymentGateway.Api";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> Authorizations = Meter.CreateCounter<long>(
        "payment.authorizations",
        unit: "{payment}",
        description: "Payments processed by the acquiring bank.");

    public static void RecordMetrics(this PaymentModel paymentModel)
    {
        TagList tags = new() { { "outcome", paymentModel.BankAuthorization.Status.ToString().ToLowerInvariant() } };
        Authorizations.Add(1, tags);
    }
}