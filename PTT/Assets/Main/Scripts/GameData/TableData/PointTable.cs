using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;

namespace GameBerry.Table
{
    /// <summary>
    /// 재화는 인벤과 분리 (Key-Value).
    /// 저장 포맷: "id:amount|id:amount|..."
    /// </summary>
    public class PointTable : TableBase
    {
        private const string pointKey = "Point";
        private Dictionary<int, long> points = new Dictionary<int, long>();

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == pointKey) points = Unpack(data[i][key].ToString());
                }
            }
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(pointKey, Pack(points));
            return p;
        }

        public long GetAmount(int pointId) => points.TryGetValue(pointId, out var v) ? v : 0;

        public void Add(int pointId, long amount)
        {
            if (amount == 0) return;
            long next = GetAmount(pointId) + amount;
            if (next < 0) next = 0;
            points[pointId] = next;
        }

        private static string Pack(Dictionary<int, long> dict)
        {
            if (dict == null || dict.Count == 0) return string.Empty;
            var parts = new List<string>(dict.Count);
            foreach (var kv in dict) parts.Add($"{kv.Key}:{kv.Value}");
            return string.Join("|", parts);
        }

        private static Dictionary<int, long> Unpack(string str)
        {
            var dict = new Dictionary<int, long>();
            if (string.IsNullOrEmpty(str)) return dict;

            var pairs = str.Split('|');
            foreach (var p in pairs)
            {
                if (string.IsNullOrEmpty(p)) continue;
                var kv = p.Split(':');
                if (kv.Length != 2) continue;

                if (!int.TryParse(kv[0], out int id)) continue;
                if (!long.TryParse(kv[1], out long amt)) amt = 0;
                dict[id] = Math.Max(0, amt);
            }
            return dict;
        }
    }
}
