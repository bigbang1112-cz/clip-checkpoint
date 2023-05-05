using System.Text.Json.Serialization;
using System.Text.Json;
using GBX.NET;

namespace GbxToolAPI.Converters.Json;

public class Vec4Converter : JsonConverter<Vec4>
{
    public override Vec4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        var x = reader.GetSingle();
        reader.Read();
        var y = reader.GetSingle();
        reader.Read();
        var z = reader.GetSingle();
        reader.Read();
        var w = reader.GetSingle();
        reader.Read();
        
        return new(x, y, z, w);
    }

    public override void Write(Utf8JsonWriter writer, Vec4 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteNumberValue(value.W);
        writer.WriteEndArray();
    }
}
