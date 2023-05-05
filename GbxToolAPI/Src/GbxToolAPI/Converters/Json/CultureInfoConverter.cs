using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace GbxToolAPI.Converters.Json;

public class CultureInfoConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return CultureInfo.GetCultureInfo(reader.GetString() ?? "");
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}
