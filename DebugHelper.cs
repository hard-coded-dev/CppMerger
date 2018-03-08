using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMerger
{
    /// <summary>
    /// Helper Functions
    /// </summary>
    public static class DebugHelper
    {
        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value)) + "}";
        }

        public static string ToDebugString<TValue>(this IList<TValue> list)
        {
            return "{" + string.Join(",", list) + "}";
        }
    }
}
