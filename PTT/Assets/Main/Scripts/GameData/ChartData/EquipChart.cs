using System;
using System.Collections.Generic;

namespace GameBerry.Chart
{
    [System.Serializable]
    public class EquipInfo
    {
        public int ItemId; // ItemId == Equip key (3000 + R*100 + T*10 + V)
        public GameBerry.Enum_EquipType EquipType;
        public string Name;

        // Packed base stats string. Example: "Attack=10|HP=50|Defence=5"
        public string BaseStats;

        [NonSerialized] private Dictionary<V2Enum_Stat, double> _baseStatDict;

        public IReadOnlyDictionary<V2Enum_Stat, double> GetBaseStats()
        {
            if (_baseStatDict == null)
                _baseStatDict = EquipChart.ParseStatsPacked(BaseStats);
            return _baseStatDict;
        }
    }

    public class EquipChart : ChartBase
    {
        public EquipInfo[] rows;
        private Dictionary<int, EquipInfo> _itemIdToInfo;

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _itemIdToInfo = new Dictionary<int, EquipInfo>(rows.Length);
            foreach (var r in rows)
            {
                if (r == null) continue;
                _itemIdToInfo[r.ItemId] = r;
            }
        }

        public EquipInfo Get(int itemId)
            => _itemIdToInfo != null && _itemIdToInfo.TryGetValue(itemId, out var v) ? v : null;

        // ---- helpers ----
        public static Dictionary<V2Enum_Stat, double> ParseStatsPacked(string packed)
        {
            var dict = new Dictionary<V2Enum_Stat, double>();
            if (string.IsNullOrEmpty(packed)) return dict;

            var parts = packed.Split('|');
            for (int i = 0; i < parts.Length; ++i)
            {
                var kv = parts[i].Split('=');
                if (kv.Length != 2) continue;

                if (Enum.TryParse(kv[0], out V2Enum_Stat stat) == false) continue;
                if (double.TryParse(kv[1], out var val) == false) continue;

                dict[stat] = val;
            }
            return dict;
        }

        // itemId rule: 3000 + (rarity*100) + (equipType*10) + variant
        public static int GetRarityIndexFromItemId(int itemId) => (itemId - 3000) / 100;
        public static int GetEquipTypeIndexFromItemId(int itemId) => ((itemId - 3000) / 10) % 10;
        public static int GetVariantFromItemId(int itemId) => (itemId - 3000) % 10;
    }
}
