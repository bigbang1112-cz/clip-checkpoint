using GBX.NET;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace BigBang1112.ClipCheckpoint.Converters;

public class Vec2Converter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vec2);

    public object? ReadYaml(IParser parser, Type type)
    {
        parser.Consume<SequenceStart>();

        var x = float.Parse(parser.Consume<Scalar>().Value);
        var y = float.Parse(parser.Consume<Scalar>().Value);

        parser.Consume<SequenceEnd>();

        return new Vec2(x, y);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var val = (Vec2)value!;

        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Flow));
        emitter.Emit(new Scalar(val.X.ToString()));
        emitter.Emit(new Scalar(val.Y.ToString()));
        emitter.Emit(new SequenceEnd());
    }
}
