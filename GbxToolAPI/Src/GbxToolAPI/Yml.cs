using GbxToolAPI.Converters.Yml;
using YamlDotNet.Serialization;

namespace GbxToolAPI;

public static class Yml
{
    public static IReadOnlyCollection<IYamlTypeConverter> TypeConverters { get; } = new List<IYamlTypeConverter>
    {
        new TimeSingleConverter(),
        new TimeInt32Converter(),
        new Vec2Converter(),
        new Vec3Converter(),
        new Vec4Converter(),
        new CultureInfoConverter()
    };

    public static ISerializer Serializer { get; } = CreateSerializerBuilder().Build();
    public static IDeserializer Deserializer { get; } = CreateDeserializerBuilder().Build();

    private static SerializerBuilder CreateSerializerBuilder()
    {
        var builder = new SerializerBuilder();

        foreach (var typeConverter in TypeConverters)
        {
            builder = builder.WithTypeConverter(typeConverter);
        }

        return builder;
    }

    private static DeserializerBuilder CreateDeserializerBuilder()
    {
        var builder = new DeserializerBuilder()
            .IgnoreUnmatchedProperties();

        foreach (var typeConverter in TypeConverters)
        {
            builder = builder.WithTypeConverter(typeConverter);
        }

        return builder;
    }
}
