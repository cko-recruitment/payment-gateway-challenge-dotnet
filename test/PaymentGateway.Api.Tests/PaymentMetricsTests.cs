using System.Diagnostics;
using System.Diagnostics.Metrics;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Observability;

namespace PaymentGateway.Api.Tests;

public class PaymentMetricsTests
{
    [Theory]
    [InlineData(PaymentStatus.Authorized, "authorized")]
    [InlineData(PaymentStatus.Declined, "declined")]
    public void Payment_authorization_metric_records_one_decision_with_its_outcome(
        PaymentStatus status, string expectedOutcome)
    {
        // Arrange
        using var activity = new Activity("payment-metric-test").Start();
        var measurements = new List<(long Value, KeyValuePair<string, object?>[] Tags)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == PaymentMetrics.MeterName && instrument.Name == "payment.authorizations")
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            // The meter is shared with API tests; capture only this test's operation.
            if (ReferenceEquals(Activity.Current, activity))
            {
                measurements.Add((value, [.. tags]));
            }
        });
        listener.Start();

        var payment = new PaymentModel
        {
            CardNumber = "1234567890121235",
            ExpiryMonth = 1,
            ExpiryYear = 2030,
            Currency = "GBP",
            Amount = 100,
            BankAuthorization = new BankAuthorization(status)
        };

        // Act
        payment.RecordMetrics();

        // Assert
        var measurement = Assert.Single(measurements);
        Assert.Equal(1, measurement.Value);
        var tag = Assert.Single(measurement.Tags);
        Assert.Equal("outcome", tag.Key);
        Assert.Equal(expectedOutcome, tag.Value);
    }
}