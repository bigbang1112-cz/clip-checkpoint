using TmEssentials;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GbxToolAPI.Converters.Yml;

public sealed class TimeInt32Converter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(TimeInt32);

    public object? ReadYaml(IParser parser, Type type)
    {
        return TimeInt32.FromMilliseconds(int.Parse(parser.Consume<Scalar>().Value));
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var val = (TimeInt32)value!;

        emitter.Emit(new Scalar(val.TotalMilliseconds.ToString()));
    }
}
