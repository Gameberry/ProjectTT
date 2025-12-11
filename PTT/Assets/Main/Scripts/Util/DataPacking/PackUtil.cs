// File: PackUtil.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameBerry
{
    public static class PackUtil
    {
        // ========= 공통 상수 / 유틸 =========

        private const char EntrySep = ';';
        private const char KeyValueSep = '|';

        [ThreadStatic]
        private static StringBuilder _cachedBuilder;

        private static StringBuilder GetBuilder(int initialCapacity = 256)
        {
            var sb = _cachedBuilder;
            if (sb == null)
            {
                sb = new StringBuilder(initialCapacity);
                _cachedBuilder = sb;
            }

            sb.Clear();
            return sb;
        }

        private static string Escape(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\\", "\\\\")
                    .Replace(";", "\\;")
                    .Replace("|", "\\|");
        }

        private static string Unescape(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\\|", "|")
                    .Replace("\\;", ";")
                    .Replace("\\\\", "\\");
        }

        // ========= 단일 값 =========

        public static string PackValue<T>(T value)
        {
            if (value == null) return string.Empty;

            var t = typeof(T);

            // enum -> int
            if (t.IsEnum)
                return Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);

            // bool -> 1 / 0
            if (t == typeof(bool))
                return ((bool)(object)value) ? "1" : "0";

            // string
            if (t == typeof(string))
                return Escape((string)(object)value);

            // 정수 계열
            if (t == typeof(int))
                return ((int)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(long))
                return ((long)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(short))
                return ((short)(object)value).ToString(CultureInfo.InvariantCulture);

            // 실수 계열
            if (t == typeof(float))
                return ((float)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(double))
                return ((double)(object)value).ToString(CultureInfo.InvariantCulture);
            if (t == typeof(decimal))
                return ((decimal)(object)value).ToString(CultureInfo.InvariantCulture);

            return value.ToString();
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

        // ========= List<IPackable> =========

        public static string PackList<TPack>(List<TPack> list)
            where TPack : IPackable
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            var sb = GetBuilder();
            bool first = true;

            for (int i = 0; i < list.Count; i++)
            {
                if (!first)
                    sb.Append(EntrySep);
                first = false;

                sb.Append(list[i]?.Pack() ?? string.Empty);
            }

            return sb.ToString();
        }

        public static List<TPack> UnpackList<TPack>(string str)
            where TPack : IPackable, new()
        {
            var result = new List<TPack>();
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
                        var item = new TPack();
                        item.Unpack(segment);
                        result.Add(item);
                    }
                    start = i + 1;
                }
            }

            return result;
        }

        // ========= List<primitive> =========

        public static string PackPrimitiveList<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            var sb = GetBuilder();
            bool first = true;

            for (int i = 0; i < list.Count; i++)
            {
                if (!first)
                    sb.Append(EntrySep);
                first = false;

                sb.Append(PackValue(list[i]));
            }

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

        // ========= Dictionary<TKey, TPack> (IPackable) =========

        public static string PackDict<TKey, TPack>(Dictionary<TKey, TPack> dict)
            where TPack : IPackable
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;

            var sb = GetBuilder();
            bool first = true;

            foreach (var kv in dict)
            {
                if (!first)
                    sb.Append(EntrySep);
                first = false;

                string keyStr = PackKey(kv.Key);
                string valueStr = kv.Value?.Pack() ?? string.Empty;

                sb.Append(keyStr);
                sb.Append(KeyValueSep);
                sb.Append(valueStr);
            }

            return sb.ToString();
        }

        public static Dictionary<TKey, TPack> UnpackDict<TKey, TPack>(string str)
            where TPack : IPackable, new()
        {
            var result = new Dictionary<TKey, TPack>();
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
                        int kvStart = start;
                        int kvEnd = i;

                        int sepIdx = -1;
                        for (int j = kvStart; j < kvEnd; j++)
                        {
                            if (str[j] == KeyValueSep)
                            {
                                sepIdx = j;
                                break;
                            }
                        }

                        if (sepIdx > kvStart)
                        {
                            int keyLen = sepIdx - kvStart;
                            int valLen = kvEnd - (sepIdx + 1);

                            string keyStr = str.Substring(kvStart, keyLen);
                            string valueStr = valLen > 0
                                ? str.Substring(sepIdx + 1, valLen)
                                : string.Empty;

                            TKey key = UnpackKey<TKey>(keyStr);

                            var packObj = new TPack();
                            packObj.Unpack(valueStr);

                            result[key] = packObj;
                        }
                    }

                    start = i + 1;
                }
            }

            return result;
        }

        // ========= Dictionary<TKey, TValue> (primitive) =========

        public static string PackPrimitiveDict<TKey, TValue>(
            Dictionary<TKey, TValue> dict)
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;

            var sb = GetBuilder();
            bool first = true;

            foreach (var kv in dict)
            {
                if (!first)
                    sb.Append(EntrySep);
                first = false;

                sb.Append(PackKey(kv.Key));
                sb.Append(KeyValueSep);
                sb.Append(PackValue(kv.Value));
            }

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
                        int kvStart = start;
                        int kvEnd = i;

                        int sepIdx = -1;
                        for (int j = kvStart; j < kvEnd; j++)
                        {
                            if (str[j] == KeyValueSep)
                            {
                                sepIdx = j;
                                break;
                            }
                        }

                        if (sepIdx > kvStart)
                        {
                            int keyLen = sepIdx - kvStart;
                            int valLen = kvEnd - (sepIdx + 1);

                            string keyStr = str.Substring(kvStart, keyLen);
                            string valueStr = valLen > 0
                                ? str.Substring(sepIdx + 1, valLen)
                                : string.Empty;

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
