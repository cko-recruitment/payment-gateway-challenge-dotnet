using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Converters
{
    public class GuidJsonConverter : JsonConverter<Guid>
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
