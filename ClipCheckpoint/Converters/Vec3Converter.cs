using GBX.NET;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace BigBang1112.ClipCheckpoint.Converters;

public class Vec3Converter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vec3);

    public object? ReadYaml(IParser parser, Type type)
    {
        parser.Consume<SequenceStart>();

        var x = (float)double.Parse(parser.Consume<Scalar>().Value);
        var y = (float)double.Parse(parser.Consume<Scalar>().Value);
        var z = (float)double.Parse(parser.Consume<Scalar>().Value);

        parser.Consume<SequenceEnd>();

        return new Vec3(x, y, z);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var val = (Vec3)value!;

        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Flow));
        emitter.Emit(new Scalar(val.X.ToString()));
        emitter.Emit(new Scalar(val.Y.ToString()));
        emitter.Emit(new Scalar(val.Z.ToString()));
        emitter.Emit(new SequenceEnd());
    }
}
