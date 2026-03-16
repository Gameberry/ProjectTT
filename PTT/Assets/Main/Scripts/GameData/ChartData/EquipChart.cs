using System;
using System.Collections.Generic;

namespace GameBerry.Chart
{
    [System.Serializable]
    public class EquipInfo
    {
        public int ItemId; // ItemId == Equip key (3000 + R*100 + T*10 + V)
        public Enum_EquipType EquipType;
        public Enum_ContentCategory ContentCategory;
        public Enum_Stat[] FixedRandomStat;
        public Enum_Rarity FixedRarity;
        public string BaseStats;

        [NonSerialized] private Dictionary<Enum_Stat, double> _baseStatDict;

        public IReadOnlyDictionary<Enum_Stat, double> GetBaseStats()
        {
            if (_baseStatDict == null)
                _baseStatDict = StatHelper.ParseStatsPacked(BaseStats);
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
    }
}
