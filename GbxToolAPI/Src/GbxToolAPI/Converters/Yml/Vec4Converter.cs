using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using GBX.NET;
using System.Globalization;

namespace GbxToolAPI.Converters.Yml;

internal class Vec4Converter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vec4) || type == typeof(Vec4?);

    public object? ReadYaml(IParser parser, Type type)
    {
        _ = parser.Consume<SequenceStart>();

        var x = float.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture);
        var y = float.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture);
        var z = float.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture);
        var w = float.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture);

        _ = parser.Consume<SequenceEnd>();

        return new Vec4(x, y, z, w);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar("~"));
            return;
        }

        var val = (Vec4)value!;

        emitter.Emit(new SequenceStart(default, default, isImplicit: true, SequenceStyle.Flow));
        emitter.Emit(new Scalar(val.X.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new Scalar(val.Y.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new Scalar(val.Z.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new Scalar(val.W.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new SequenceEnd());
    }
}
