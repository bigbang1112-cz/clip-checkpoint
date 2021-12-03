using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBang1112.ClipCheckpoint.Extensions
{
    public static class NullableExtensions
    {
        public static bool TryValue<T>(this T? source, out T value) where T : struct
        {
            if (source.HasValue)
            {
                value = source.Value;
                return true;
            }

            value = default;
            return false;
        }
    }
}
