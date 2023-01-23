global using static Helpers.DictionaryHelper;
using System.Diagnostics.CodeAnalysis;

namespace Helpers;

public static class DictionaryHelper
{
    public static bool TryGetValue<TKey, TValue, TTypedValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, [NotNullWhen(true)] out TTypedValue value)
    where TTypedValue : TValue
    {
        if (dict.TryGetValue(key, out var val) && val is TTypedValue typedVal)
        {
            value = typedVal;
            return true;
        }
        value = default!;
        return false;
    }
}
