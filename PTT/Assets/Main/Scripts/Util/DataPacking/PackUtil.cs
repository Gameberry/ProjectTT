using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameBerry
{
    public static class PackSep
    { // 중첩Pack일 때만 사용
        public const char Dict = ';';
        public const char List = '_';
        public const char Sub = '~';
    }

    public static class PackUtil
    {
        private const char EntrySep = ';';
        private const char KeyValueSep = '|';

        [ThreadStatic]
        private static Stack<StringBuilder> _sbPool;

        private static StringBuilder RentBuilder(int initialCapacity = 256)
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
            if (sb.Capacity > 4096) // 과도하게 커진 sb는 버림(메모리 관리)
                return;

            (_sbPool ??= new Stack<StringBuilder>(4)).Push(sb);
        }

        // ========================
        // ESCAPE / UNESCAPE
        // ========================

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return s
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace("|", "\\|");
        }

        private static string Unescape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return s
                .Replace("\\|", "|")
                .Replace("\\;", ";")
                .Replace("\\\\", "\\");
        }

        private static string EscapeSeg(string raw) => Escape(raw);
        private static string UnescapeSeg(string raw) => Unescape(raw);

        // ========================
        // PACK VALUE (primitive)
        // ========================

        public static string PackValue<T>(T value)
        {
            if (value == null)
                return string.Empty;

            var t = typeof(T);

            // enum → int
            if (t.IsEnum)
                return Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);

            if (t == typeof(bool))
                return ((bool)(object)value) ? "1" : "0";

            if (t == typeof(string))
                return Escape((string)(object)value);

            if (t == typeof(int))
                return ((int)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(long))
                return ((long)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(short))
                return ((short)(object)value).ToString(CultureInfo.InvariantCulture);

            if (t == typeof(float))
                return ((float)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(double))
                return ((double)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(decimal))
                return ((decimal)(object)value).ToString(CultureInfo.InvariantCulture);

            return Escape(value.ToString());
        }

        public static T UnpackValue<T>(string str)
        {
            var t = typeof(T);

            if (t.IsEnum)
            {
                int v = int.Parse(str, CultureInfo.InvariantCulture);
                return (T)Enum.ToObject(t, v);
            }

            if (t == typeof(int))
                return (T)(object)int.Parse(str, CultureInfo.InvariantCulture);
            if (t == typeof(long))
                return (T)(object)long.Parse(str, CultureInfo.InvariantCulture);
            if (t == typeof(short))
                return (T)(object)short.Parse(str, CultureInfo.InvariantCulture);

            if (t == typeof(float))
                return (T)(object)float.Parse(str, CultureInfo.InvariantCulture);
            if (t == typeof(double))
                return (T)(object)double.Parse(str, CultureInfo.InvariantCulture);
            if (t == typeof(decimal))
                return (T)(object)decimal.Parse(str, CultureInfo.InvariantCulture);

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

        public static string PackList<TPack>(List<TPack> list, char customEntry = EntrySep)
            where TPack : IPackable
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            var sb = RentBuilder();
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(customEntry);

                    string raw = list[i].Pack();          // struct도 OK
                    sb.Append(EscapeSeg(raw ?? string.Empty));
                }
                return sb.ToString();
            }
            finally
            {
                ReturnBuilder(sb);
            }
        }

        public static List<TPack> UnpackList<TPack>(string str, char customEntry = EntrySep)
            where TPack : IPackable, new()
        {
            var result = new List<TPack>();
            if (string.IsNullOrEmpty(str))
                return result;

            int len = str.Length;
            int start = 0;

            for (int i = 0; i <= len; i++)
            {
                if (i == len || str[i] == customEntry)
                {
                    int segLen = i - start;
                    if (segLen > 0)
                    {
                        string segment = str.Substring(start, segLen);
                        segment = UnescapeSeg(segment);

                        var item = new TPack();
                        item.Unpack(segment);
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

        public static string PackPrimitiveList<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            var sb = RentBuilder();
            bool first = true;

            foreach (var v in list)
            {
                if (!first) sb.Append(EntrySep);
                first = false;

                sb.Append(PackValue(v));
            }

            ReturnBuilder(sb);

            return sb.ToString();
        }

        public static List<T> UnpackPrimitiveList<T>(string str)
        {
            var result = new List<T>();
            if (string.IsNullOrEmpty(str))
                return result;

            int len = str.Length;
            int start = 0;

            for (int i = 0; i <= len; i++)
            {
                if (i == len || str[i] == EntrySep)
                {
                    int segLen = i - start;
                    if (segLen > 0)
                    {
                        string segment = str.Substring(start, segLen);
                        var value = UnpackValue<T>(segment);
                        result.Add(value);
                    }
                    start = i + 1;
                }
            }

            return result;
        }

        // ========================
        // DICT<KEY, IPackable>
        // ========================

        public static string PackDict<TKey, TPack>(Dictionary<TKey, TPack> dict, char customEntry = EntrySep)
            where TPack : IPackable
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;

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

                    string raw = kv.Value.Pack();         // struct/class 모두 OK
                    sb.Append(EscapeSeg(raw ?? string.Empty));
                }

                return sb.ToString();
            }
            finally
            {
                ReturnBuilder(sb);
            }
        }

        public static Dictionary<TKey, TPack> UnpackDict<TKey, TPack>(string str, char customEntry = EntrySep)
            where TPack : IPackable, new()
        {
            var result = new Dictionary<TKey, TPack>();
            if (string.IsNullOrEmpty(str))
                return result;

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
                            if (str[j] == KeyValueSep)
                            {
                                sepIdx = j;
                                break;
                            }
                        }

                        if (sepIdx > segStart)
                        {
                            string keyStr = str.Substring(segStart, sepIdx - segStart);
                            string valueStr = str.Substring(sepIdx + 1, segEnd - (sepIdx + 1));

                            TKey key = UnpackKey<TKey>(keyStr);

                            valueStr = UnescapeSeg(valueStr);

                            var item = new TPack();
                            item.Unpack(valueStr);
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

        public static string PackPrimitiveDict<TKey, TValue>(
            Dictionary<TKey, TValue> dict)
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;

            var sb = RentBuilder();
            bool first = true;

            foreach (var kv in dict)
            {
                if (!first) sb.Append(EntrySep);
                first = false;

                sb.Append(PackKey(kv.Key));
                sb.Append(KeyValueSep);
                sb.Append(PackValue(kv.Value));
            }

            ReturnBuilder(sb);

            return sb.ToString();
        }

        public static Dictionary<TKey, TValue> UnpackPrimitiveDict<TKey, TValue>(
            string str)
        {
            var result = new Dictionary<TKey, TValue>();
            if (string.IsNullOrEmpty(str))
                return result;

            int len = str.Length;
            int start = 0;

            for (int i = 0; i <= len; i++)
            {
                if (i == len || str[i] == EntrySep)
                {
                    int segLen = i - start;
                    if (segLen > 0)
                    {
                        int segStart = start;
                        int segEnd = i;

                        int sepIdx = -1;
                        for (int j = segStart; j < segEnd; j++)
                        {
                            if (str[j] == KeyValueSep)
                            {
                                sepIdx = j;
                                break;
                            }
                        }

                        if (sepIdx > segStart)
                        {
                            string keyStr = str.Substring(segStart, sepIdx - segStart);
                            string valueStr = str.Substring(sepIdx + 1, segEnd - (sepIdx + 1));

                            TKey key = UnpackKey<TKey>(keyStr);
                            TValue value = UnpackValue<TValue>(valueStr);

                            result[key] = value;
                        }
                    }

                    start = i + 1;
                }
            }

            return result;
        }
    }
}
