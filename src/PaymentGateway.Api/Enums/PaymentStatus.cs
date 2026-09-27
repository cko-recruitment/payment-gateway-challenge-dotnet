using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PaymentStatus>))]

public enum PaymentStatus
{
    Pending, //I'd have this additional status to represent when Payment is created but not yet processed
    Authorized,
    Declined,
    //Rejected I remove this because this is semantically unrealistic. We would normally not create a payment when the request is invalid.
}