using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public class BackendChartValueConverter : JsonConverter
{
    public BackendChartValueConverter()
    {


    }

    private bool isDebug = false;

    public BackendChartValueConverter(bool debug)
    {
        isDebug = debug;
    }

    private readonly Type[] _types = {
            typeof(string),
            typeof(string[]),
            typeof(int),
            typeof(int[]),
            typeof(int[][]),
            typeof(float),
            typeof(float[]),
            typeof(float[][]),
            typeof(double),
            typeof(double[]),
            typeof(bool),
            typeof(bool[]),
            typeof(long),
            typeof(System.DateTime),
            // typeof(Enum),
            
            // typeof(Chart.Tier),
            // // typeof(Chart.Tier[]),
            // typeof(Chart.StatType),
            // typeof(Chart.ItemType),
            // // typeof(Chart.ItemType[]),
            // typeof(Chart.EquipmentType),
            // // typeof(Chart.EquipmentType[]),
            // typeof(Character.BuffType),
            // // typeof(Character.BuffType[]),
            // typeof(Chart.CraftingType),
            // typeof(Chart.CraftingType[]),
            // typeof(Chart.GachaType),
            // typeof(Chart.GachaType[]),
            // typeof(Chart.ShopCategory),
            // typeof(Chart.ShopCategory[]),
            // typeof(Chart.ShopLimitType),
            // typeof(Chart.ShopLimitType[]),
            // typeof(Chart.IAPShopCategory),
            // typeof(Chart.IAPShopSubCategory),
            // typeof(Chart.ShopCostType),
            // typeof(Chart.ShopUnlockType),
            // typeof(Chart.ScrollType),
            // typeof(Chart.TimePackageType),
            // typeof(Chart.TowerEntryConditionType),
            // typeof(Chart.WitchBoxCategory),
            // typeof(Chart.GoodsType),
            // typeof(Chart.GoodsType[]),
            // typeof(ContentObj.Category),
            // typeof(ContentObj.UnlockCondition),
            // typeof(Chart.MonsterType),
            // typeof(Chart.ItemCollectionCategory),
            // typeof(Chart.QuestType),
            // typeof(Chart.QuestCategory),
            // typeof(Chart.PassType),
            // typeof(Chart.AchievementCategory),
            // typeof(Chart.WraithType),
            // typeof(Chart.EventAttendanceType),
            // typeof(Chart.ChaliceType),
        };

    #region implemented abstract members of JsonConverter

    private void WriteObject(JsonWriter writer, object value)
    {
        writer.WriteStartObject();
        var obj = value as IDictionary<object, object>;
        foreach (var kvp in obj)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            this.WriteValue(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }

    private void WriteArray(JsonWriter writer, object value)
    {
        writer.WriteStartArray();
        var array = value as IEnumerable<object>;
        foreach (var o in array)
        {
            this.WriteValue(writer, o);
        }
        writer.WriteEndArray();
    }

    private void WriteValue(JsonWriter writer, object value)
    {
        var t = JToken.FromObject(value);
        switch (t.Type)
        {
            case JTokenType.Object:
                this.WriteObject(writer, value);
                break;
            case JTokenType.Array:
                this.WriteArray(writer, value);
                break;
            default:
                writer.WriteValue(value);
                break;
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is double)
        {
            writer.WriteValue((double)value);
        }
        else
        {
            writer.WriteValue(value.ToString());
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // var token = JToken.Load(reader);
        var str = reader.Value?.ToString();
        if (isDebug) Debug.Log($"{str} | {objectType}");
        if (str == null)
        {
            return null;
        }

        if (objectType == typeof(string))
        {
            return str;
        }

        if (objectType == typeof(string[]))
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<string>();
            if (str[^1] == ';') str = str[..^1];
            return str.Split(';').Select(x => x.Trim()).ToArray();
            var arr = str.Split(';').Select(x => x.Trim()).ToList();
            if (string.IsNullOrEmpty(arr[^1]))
            {
                return arr.GetRange(0, arr.Count - 1).ToArray();
            }
            return arr.ToArray();
        } 
        if (objectType == typeof(int))
        {
            if (str.Contains("None")) return 0;
            if (string.IsNullOrEmpty(str)) return 0;
            try
            {
                return int.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        if (objectType == typeof(int[]))
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<int>();
            if(str[^1] == ';') str = str[..^1];
            return str.Split(';').Select(x => string.IsNullOrEmpty(x) ? 0 : int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        }
        
        // if (objectType == typeof(int[][]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //
        //     string[] strs = data.S.Split("],").ToArray();
        //     int[][] val = new int[strs.Length][];
        //     for (int i = 0; i < val.Length; i++)
        //     {
        //         val[i] = strs[i].Replace("[", string.Empty).Replace("]", string.Empty).Split(';').Select(x => int.Parse(x)).ToArray();
        //     }
        //     return val;
        // }
        
        if (objectType == typeof(float))
        {
            if (string.IsNullOrEmpty(str)) return 0f;
            return float.Parse(str, CultureInfo.InvariantCulture);
        }
        
        if (objectType == typeof(float[]))
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<float>();
            if(str[^1] == ';') str = str[..^1];
            return str.Split(';').Select(x => string.IsNullOrEmpty(x) ? 0f : float.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        }
        // else if (objectType == typeof(float[][]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //
        //     string[] strs = data.S.Split("],").ToArray();
        //     float[][] val = new float[strs.Length][];
        //     for (int i = 0; i < val.Length; i++)
        //     {
        //         val[i] = strs[i].Replace("[", string.Empty).Replace("]", string.Empty).Split(';').Select(x => float.Parse(x)).ToArray();
        //     }
        //     return val;
        // }
         if (objectType == typeof(double))
        {
            if (string.IsNullOrEmpty(str)) return 0f;
            return double.Parse(str, CultureInfo.InvariantCulture);
        }
         
        if (objectType == typeof(double[]))
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<double>();
            if(str[^1] == ';') str = str[..^1];
            return str.Split(';').Select(x => string.IsNullOrEmpty(x) ? 0D : double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        }
        
        if (objectType == typeof(long))
        {
            if (string.IsNullOrEmpty(str)) return 0L;
            return long.Parse(str, CultureInfo.InvariantCulture);
        }
        
        if (objectType == typeof(long[]))
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<long>();
            if(str[^1] == ';') str = str[..^1];
            return str.Split(';').Select(x => string.IsNullOrEmpty(x) ? 0L : long.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        }
        
        if (objectType == typeof(bool))
        {
            if (bool.TryParse(str, out var result)) return result;
            if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var resultInt)) return resultInt > 0; 
            return result;
        }
        if (objectType == typeof(bool[]))
        {
            if (string.IsNullOrEmpty(str)) return Array.Empty<bool>();
            if(str[^1] == ';') str = str[..^1];
            return str.Split(';').Select(x =>
            {
                if (string.IsNullOrEmpty(x)) return false;
                if (bool.TryParse(x, out var result)) return result;
                if (int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var resultInt)) return resultInt > 0;
                return result;
            }).ToArray();
        }
        
        if (objectType == typeof(System.DateTime))
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (System.DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
            return null;
        }

        if (objectType.IsEnum)
        {
            if (string.IsNullOrEmpty(str)) return Enum.GetValues(objectType).GetValue(0);
            try
            {
                return Enum.Parse(objectType, str, true);
            }
            catch
            {
#if UNITY_EDITOR
                Debug.Log($"enum is not defined: {objectType}/{str}");
#endif
                return Enum.GetValues(objectType).GetValue(0);
            }
        }
        
        //else if(objectType == typeof(Enum))
        //{
        //    BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //    return Util.EnumParse<Enum>(data.S);
        //}
        //else if (objectType == typeof(Enum[]))
        //{
        //    BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //    if (data.S[^1] == ';')
        //    {
        //        data.S = data.S[..^1];
        //    }
        //    return data.S.ToEnumArray<Enum>();
        //}
        // else if (objectType == typeof(Chart.Tier))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.Tier>(data.S);
        // }
        // else if (objectType == typeof(Chart.Tier[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.Tier>();
        // }
        // else if (objectType == typeof(Chart.StatType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.StatType>(data.S);
        // }
        // else if (objectType == typeof(Chart.StatType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.StatType>();
        // }
        // else if (objectType == typeof(Chart.ItemType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ItemType>(data.S);
        // }
        // else if (objectType == typeof(Chart.ItemType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.ItemType>();
        // }
        // else if (objectType == typeof(Chart.EquipmentType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.EquipmentType>(data.S);
        // }
        // else if (objectType == typeof(Chart.EquipmentType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.EquipmentType>();
        // }
        // else if (objectType == typeof(Character.BuffType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Character.BuffType>(data.S);
        // }
        // else if (objectType == typeof(Character.BuffType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Character.BuffType>();
        // }
        // else if (objectType == typeof(Chart.CraftingType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.CraftingType>(data.S);
        // }
        // else if (objectType == typeof(Chart.CraftingType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.CraftingType>();
        // }
        // else if (objectType == typeof(Chart.GachaType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.GachaType>(data.S);
        // }
        // else if (objectType == typeof(Chart.GachaType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.GachaType>();
        // }
        // else if (objectType == typeof(Chart.ShopCategory))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ShopCategory>(data.S);
        // }
        // else if (objectType == typeof(Chart.ShopUnlockType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ShopUnlockType>(data.S);
        // }
        // else if (objectType == typeof(Chart.ShopCategory[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.ShopCategory>();
        // }
        // else if (objectType == typeof(Chart.ShopLimitType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ShopLimitType>(data.S);
        // }
        // else if (objectType == typeof(Chart.ShopLimitType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.ShopLimitType>();
        // }
        // else if (objectType == typeof(Chart.IAPShopCategory))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.IAPShopCategory>(data.S);
        // }
        // else if (objectType == typeof(Chart.IAPShopSubCategory))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.IAPShopSubCategory>(data.S);
        // }
        // else if (objectType == typeof(Chart.ShopCostType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ShopCostType>(data.S);
        // }
        // else if (objectType == typeof(Chart.ScrollType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ScrollType>(data.S);
        // }
        // else if (objectType == typeof(Chart.TimePackageType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.TimePackageType>(data.S);
        // }
        // else if (objectType == typeof(Chart.TowerEntryConditionType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.TowerEntryConditionType>(data.S);
        // }
        //     
        // // Old ==================================================
        //     
        // else if (objectType == typeof(Chart.TimeType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.TimeType>(data.S);
        // }
        // else if (objectType == typeof(Chart.TimeType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.TimeType>();
        // }
        // else if (objectType == typeof(Chart.WitchBoxCategory))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.WitchBoxCategory>(data.S);
        // }
        // else if (objectType == typeof(Chart.GoodsType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.GoodsType>(data.S);
        // }
        // else if (objectType == typeof(Chart.GoodsType[]))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     if (data.S[^1] == ';')
        //     {
        //         data.S = data.S[..^1];
        //     }
        //     return data.S.ToEnumArray<Chart.GoodsType>();
        // }
        // else if (objectType == typeof(ContentObj.Category))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<ContentObj.Category>(data.S);
        // }
        // else if (objectType == typeof(ContentObj.UnlockCondition))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<ContentObj.UnlockCondition>(data.S);
        // }
        // else if (objectType == typeof(Chart.MonsterType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.MonsterType>(data.S);
        // }
        // else if (objectType == typeof(Chart.ItemCollectionCategory))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ItemCollectionCategory>(data.S);
        // }
        // else if (objectType == typeof(Chart.QuestType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.QuestType>(data.S);
        // }
        // else if (objectType == typeof(Chart.QuestCategory))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.QuestCategory>(data.S);
        // }
        // else if (objectType == typeof(Chart.PassType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.PassType>(data.S);
        // }
        // else if (objectType == typeof(Chart.AchievementCategory))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.AchievementCategory>(data.S);
        // }
        // else if (objectType == typeof(Chart.WraithType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.WraithType>(data.S);
        // }
        // else if (objectType == typeof(Chart.EventAttendanceType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.EventAttendanceType>(data.S);
        // }
        // else if (objectType == typeof(Chart.ChaliceType))
        // {
        //     BackendCustom.Data data = JsonConvert.DeserializeObject<BackendCustom.Data>(token.ToString());
        //     return Util.EnumParse<Chart.ChaliceType>(data.S);
        // }
        
        Debug.LogError("Code not implemented yet!");
        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsEnum || _types.Any(t => t == objectType);
    }

    #endregion
}