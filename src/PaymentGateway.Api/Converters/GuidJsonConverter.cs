using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Converters
{
    /// <summary>
    /// Custom converter to handle the empty guid response from the bank response
    /// </summary>
    public sealed class GuidJsonConverter : JsonConverter<Guid>
    {
        public override Guid Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        { 
            if (!Guid.TryParse(reader.GetString(), out Guid guid))
            {
                return Guid.Empty;
            }
            return guid;
        }


        public override void Write(
            Utf8JsonWriter writer,
            Guid guidValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(guidValue.ToString());
    }
  
}
