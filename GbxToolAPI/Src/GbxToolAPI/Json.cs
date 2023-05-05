using GbxToolAPI.Converters.Json;
using System.Text.Json;

namespace GbxToolAPI;

public static class Json
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        Converters =
        {
            new CultureInfoConverter(),
            new TimeInt32Converter(),
            new TimeSingleConverter(),
            new Vec2Converter(),
            new Vec3Converter(),
            new Vec4Converter()
        },
    };
}
