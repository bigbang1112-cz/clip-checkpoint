using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using System.Globalization;

namespace GbxToolAPI.Converters.Yml;

internal class CultureInfoConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(CultureInfo);

    public object? ReadYaml(IParser parser, Type type)
    {
        return CultureInfo.GetCultureInfo(parser.Consume<Scalar>().Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar("~"));
            return;
        }

        var val = (CultureInfo)value!;

        emitter.Emit(new Scalar(val.Name));
    }
}
