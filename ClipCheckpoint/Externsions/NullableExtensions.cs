namespace ClipCheckpoint.Externsions;

internal static class NullableExtensions
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
