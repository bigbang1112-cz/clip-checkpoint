using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using GBX.NET;
using System.Globalization;

namespace GbxToolAPI.Converters.Yml;

internal class Vec2Converter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vec2) || type == typeof(Vec2?);

    public object? ReadYaml(IParser parser, Type type)
    {
        _ = parser.Consume<SequenceStart>();

        var x = float.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture);
        var y = float.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture);

        _ = parser.Consume<SequenceEnd>();

        return new Vec2(x, y);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar("~"));
            return;
        }

        var val = (Vec2)value!;

        emitter.Emit(new SequenceStart(default, default, isImplicit: true, SequenceStyle.Flow));
        emitter.Emit(new Scalar(val.X.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new Scalar(val.Y.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new SequenceEnd());
    }
}
