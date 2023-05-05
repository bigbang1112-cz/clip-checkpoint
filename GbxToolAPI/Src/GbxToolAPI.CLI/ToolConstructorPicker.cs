using System.Diagnostics.CodeAnalysis;

namespace GbxToolAPI.CLI;

static class ToolConstructorPicker
{
    internal static IEnumerable<(T, object[])> CreateInstances<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(Dictionary<Type, ICollection<object>> inputByType, bool singleOutput) where T : class, ITool
    {
        foreach (var ctor in typeof(T).GetConstructors())
        {
            var ctorParams = ctor.GetParameters();

            var invalidCtor = false;
            var bulkParamIndex = default(int?);
            var ctorParamValuesToServe = new object[ctorParams.Length];
            var bulkParamList = new List<object>();

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var param = ctorParams[i];

                if (singleOutput && param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var elementType = param.ParameterType.GetGenericArguments()[0];

                    if (!inputByType.TryGetValue(elementType, out var coll))
                    {
                        invalidCtor = true;
                        break;
                    }

                    if (coll.Count > 1)
                    {
                        var castMethod = typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(elementType);

                        ctorParamValuesToServe[i] = castMethod.Invoke(null, new object[] { coll })!;

                        continue;
                    }
                }

                if (!inputByType.TryGetValue(param.ParameterType, out var inputList))
                {
                    invalidCtor = true;
                    break;
                }

                switch (inputList.Count)
                {
                    case <= 0:
                        throw new Exception("No input for parameter " + param.Name + " of type " + param.ParameterType.Name);
                    case 1:
                        ctorParamValuesToServe[i] = inputList.First();
                        break;
                    default:

                        if (singleOutput)
                        {
                            invalidCtor = true;
                            break;
                        }

                        if (bulkParamIndex is not null)
                        {
                            throw new Exception("Bulk input is supported with only one type of input.");
                        }

                        bulkParamIndex = i;
                        bulkParamList.AddRange(inputList);
                        break;
                }
            }

            if (invalidCtor)
            {
                continue;
            }

            if (bulkParamIndex is null)
            {
                yield return (ctor.Invoke(ctorParamValuesToServe) as T ?? throw new Exception("Invalid constructor"), ctorParamValuesToServe);
                continue;
            }

            foreach (var val in bulkParamList)
            {
                ctorParamValuesToServe[bulkParamIndex.Value] = val;
                yield return (ctor.Invoke(ctorParamValuesToServe) as T ?? throw new Exception("Invalid constructor"), ctorParamValuesToServe);
            }
        }
    }
}