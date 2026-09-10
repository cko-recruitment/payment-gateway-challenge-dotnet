using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Authorized,
    Declined,
    Rejected
}