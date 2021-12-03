using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace BigBang1112.ClipCheckpoint.Converters;

public class TimeSpanConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(TimeSpan);

    public object? ReadYaml(IParser parser, Type type)
    {
        return TimeSpan.FromSeconds(double.Parse(parser.Consume<Scalar>().Value));
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var val = (TimeSpan)value!;

        emitter.Emit(new Scalar(val.TotalSeconds.ToString()));
    }
}
