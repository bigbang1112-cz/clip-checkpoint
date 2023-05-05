using System.Text.Json.Serialization;
using System.Text.Json;
using TmEssentials;

namespace GbxToolAPI.Converters.Json;

public class TimeSingleConverter : JsonConverter<TimeSingle>
{
    public override TimeSingle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(reader.GetSingle());
    }

    public override void Write(Utf8JsonWriter writer, TimeSingle value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.TotalSeconds);
    }
}
