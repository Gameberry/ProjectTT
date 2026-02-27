using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameBerry
{
    /// <summary>
    /// 중첩 Pack일 때만 사용
    /// </summary>
    public static class PackSep
    {
        public const char Top = ';'; // 기본
        public const char L1 = '_';   // 1단 중첩
        public const char L2 = '~';   // 2단 중첩
        public const char L3 = '^';   // 3단 중첩
    }

    public static class PackUtil
    {
        private const char KeyValueSep = '|';
        private const char EscapeChar = '\\';

        [ThreadStatic] private static Stack<StringBuilder> _sbPool;

        private const int DefaultCapacity = 256;
        private const int MaxPooledCapacity = 4096;

        private static StringBuilder RentBuilder(int initialCapacity = DefaultCapacity)
        {
            var pool = _sbPool ??= new Stack<StringBuilder>(4);
            if (pool.Count > 0)
            {
                var sb = pool.Pop();
                sb.Clear();
                return sb;
            }
            return new StringBuilder(initialCapacity);
        }

        private static void ReturnBuilder(StringBuilder sb)
        {
            if (sb == null) return;
            if (sb.Capacity > MaxPooledCapacity) return;
            (_sbPool ??= new Stack<StringBuilder>(4)).Push(sb);
        }

        // ========================
        // ESCAPE / UNESCAPE (scanner)
        // ========================
        private static bool NeedsEscape(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == EscapeChar || c == PackSep.Top || c == KeyValueSep)
                    return true;
            }
            return false;
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (!NeedsEscape(s)) return s;

            var sb = RentBuilder(s.Length + 8);
            try
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == EscapeChar || c == PackSep.Top || c == KeyValueSep)
                        sb.Append(EscapeChar);
                    sb.Append(c);
                }
                return sb.ToString();
            }
            finally { ReturnBuilder(sb); }
        }

        private static string Unescape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.IndexOf(EscapeChar) < 0) return s;

            var sb = RentBuilder(s.Length);
            try
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == EscapeChar && i + 1 < s.Length)
                    {
                        sb.Append(s[i + 1]);
                        i++;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }
            finally { ReturnBuilder(sb); }
        }

        private static string EscapeSeg(string raw) => Escape(raw);
        private static string UnescapeSeg(string raw) => Unescape(raw);

        // ========================
        // PACK / UNPACK VALUE (primitive/enum/string)
        // ========================
        public static string PackValue<T>(T value)
        {
            if (value == null) return string.Empty;

            var t = typeof(T);

            if (t.IsEnum)
                return Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);

            if (t == typeof(bool))
                return ((bool)(object)value) ? "1" : "0";

            if (t == typeof(string))
                return Escape((string)(object)value);

            if (t == typeof(int)) return ((int)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(long)) return ((long)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(short)) return ((short)(object)value).ToString(CultureInfo.InvariantCulture);

            if (t == typeof(float)) return ((float)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(double)) return ((double)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(decimal)) return ((decimal)(object)value).ToString(CultureInfo.InvariantCulture);

            return Escape(value.ToString());
        }

        /// <summary>
        /// 포맷 에러가 나면 default(T)로 떨어짐(예외 안 터뜨림).
        /// </summary>
        public static T UnpackValue<T>(string str)
        {
            var t = typeof(T);
            if (string.IsNullOrEmpty(str)) return default;

            if (t.IsEnum)
            {
                if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ev))
                    return (T)Enum.ToObject(t, ev);
                return default;
            }

            if (t == typeof(int))
            {
                int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v);
                return (T)(object)v;
            }
            if (t == typeof(long))
            {
                long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long v);
                return (T)(object)v;
            }
            if (t == typeof(short))
            {
                short.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out short v);
                return (T)(object)v;
            }

            if (t == typeof(float))
            {
                float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float v);
                return (T)(object)v;
            }
            if (t == typeof(double))
            {
                double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double v);
                return (T)(object)v;
            }
            if (t == typeof(decimal))
            {
                decimal.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal v);
                return (T)(object)v;
            }

            if (t == typeof(bool))
                return (T)(object)(str == "1");

            if (t == typeof(string))
                return (T)(object)Unescape(str);

            return default;
        }

        private static string PackKey<TKey>(TKey key) => PackValue(key);
        private static TKey UnpackKey<TKey>(string s) => UnpackValue<TKey>(s);

        // ========================
        // LIST<IPackable>
        // ========================
        public static string PackList<TPack>(List<TPack> list, char customEntry = PackSep.Top)
            where TPack : IPackable
        {
            if (list == null || list.Count == 0) return string.Empty;

            var sb = RentBuilder();
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(customEntry);
                    string raw = list[i].Pack();
                    sb.Append(EscapeSeg(raw ?? string.Empty));
                }
                return sb.ToString();
            }
            finally { ReturnBuilder(sb); }
        }

        public static List<TPack> UnpackList<TPack>(string str, char customEntry = PackSep.Top)
            where TPack : IPackable, new()
        {
            var result = new List<TPack>();
            if (string.IsNullOrEmpty(str)) return result;

            int len = str.Length;
            int start = 0;

            for (int i = 0; i <= len; i++)
            {
                if (i == len || str[i] == customEntry)
                {
                    int segLen = i - start;
                    if (segLen > 0)
                    {
                        string seg = UnescapeSeg(str.Substring(start, segLen));
                        var item = new TPack();
                        item.Unpack(seg);
                        result.Add(item);
                    }
                    start = i + 1;
                }
            }

            return result;
        }

        // ========================
        // LIST<PRIMITIVE>
        // ========================
        public static string PackPrimitiveList<T>(List<T> list, char customEntry = PackSep.Top)
        {
            if (list == null || list.Count == 0) return string.Empty;

            var sb = RentBuilder();
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(customEntry);
                    sb.Append(PackValue(list[i]));
                }
                return sb.ToString();
            }
            finally { ReturnBuilder(sb); }
        }

        public static List<T> UnpackPrimitiveList<T>(string str, char customEntry = PackSep.Top)
        {
            var result = new List<T>();
            if (string.IsNullOrEmpty(str)) return result;

            int len = str.Length;
            int start = 0;

            for (int i = 0; i <= len; i++)
            {
                if (i == len || str[i] == customEntry)
                {
                    int segLen = i - start;
                    if (segLen > 0)
                        result.Add(UnpackValue<T>(str.Substring(start, segLen)));
                    start = i + 1;
                }
            }

            return result;
        }

        // ========================
        // DICT<KEY, IPackable>
        // ========================
        public static string PackDict<TKey, TPack>(Dictionary<TKey, TPack> dict, char customEntry = PackSep.Top)
            where TPack : IPackable
        {
            if (dict == null || dict.Count == 0) return string.Empty;

            var sb = RentBuilder();
            try
            {
                bool first = true;
                foreach (var kv in dict)
                {
                    if (!first) sb.Append(customEntry);
                    first = false;

                    sb.Append(PackKey(kv.Key));
                    sb.Append(KeyValueSep);

                    string raw = kv.Value.Pack();
                    sb.Append(EscapeSeg(raw ?? string.Empty));
                }

                return sb.ToString();
            }
            finally { ReturnBuilder(sb); }
        }

        public static Dictionary<TKey, TPack> UnpackDict<TKey, TPack>(string str, char customEntry = PackSep.Top)
            where TPack : IPackable, new()
        {
            var result = new Dictionary<TKey, TPack>();
            if (string.IsNullOrEmpty(str)) return result;

            int len = str.Length;
            int start = 0;

            for (int i = 0; i <= len; i++)
            {
                if (i == len || str[i] == customEntry)
                {
                    int segLen = i - start;
                    if (segLen > 0)
                    {
                        int segStart = start;
                        int segEnd = i;

                        int sepIdx = -1;
                        for (int j = segStart; j < segEnd; j++)
                        {
                            if (str[j] == KeyValueSep) { sepIdx = j; break; }
                        }

                        if (sepIdx > segStart)
                        {
                            string keyStr = str.Substring(segStart, sepIdx - segStart);
                            string valStr = str.Substring(sepIdx + 1, segEnd - (sepIdx + 1));

                            TKey key = UnpackKey<TKey>(keyStr);

                            valStr = UnescapeSeg(valStr);
                            var item = new TPack();
                            item.Unpack(valStr);

                            result[key] = item;
                        }
                    }

                    start = i + 1;
                }
            }

            return result;
        }

        // ========================
        // DICT<KEY, PRIMITIVE>
        // ========================
        public static string PackPrimitiveDict<TKey, TValue>(Dictionary<TKey, TValue> dict, char customEntry = PackSep.Top)
        {
            if (dict == null || dict.Count == 0) return string.Empty;

            var sb = RentBuilder();
            try
            {
                bool first = true;
                foreach (var kv in dict)
                {
                    if (!first) sb.Append(customEntry);
                    first = false;

                    sb.Append(PackKey(kv.Key));
                    sb.Append(KeyValueSep);
                    sb.Append(PackValue(kv.Value));
                }

                return sb.ToString();
            }
            finally { ReturnBuilder(sb); }
        }

        public static Dictionary<TKey, TValue> UnpackPrimitiveDict<TKey, TValue>(string str, char customEntry = PackSep.Top)
        {
            var result = new Dictionary<TKey, TValue>();
            if (string.IsNullOrEmpty(str)) return result;

            int len = str.Length;
            int start = 0;

            for (int i = 0; i <= len; i++)
            {
                if (i == len || str[i] == customEntry)
                {
                    int segLen = i - start;
                    if (segLen > 0)
                    {
                        int segStart = start;
                        int segEnd = i;

                        int sepIdx = -1;
                        for (int j = segStart; j < segEnd; j++)
                        {
                            if (str[j] == KeyValueSep) { sepIdx = j; break; }
                        }

                        if (sepIdx > segStart)
                        {
                            string keyStr = str.Substring(segStart, sepIdx - segStart);
                            string valStr = str.Substring(sepIdx + 1, segEnd - (sepIdx + 1));

                            TKey key = UnpackKey<TKey>(keyStr);
                            TValue val = UnpackValue<TValue>(valStr);

                            result[key] = val;
                        }
                    }

                    start = i + 1;
                }
            }

            return result;
        }
    }
}
