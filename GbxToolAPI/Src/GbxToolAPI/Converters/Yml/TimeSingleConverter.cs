using System.Globalization;
using TmEssentials;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GbxToolAPI.Converters.Yml;

public sealed class TimeSingleConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(TimeSingle);

    public object? ReadYaml(IParser parser, Type type)
    {
        return TimeSingle.FromSeconds(float.Parse(parser.Consume<Scalar>().Value, CultureInfo.InvariantCulture));
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var val = (TimeSingle)value!;

        emitter.Emit(new Scalar(val.TotalSeconds.ToString(CultureInfo.InvariantCulture)));
    }
}
