using System.Collections.Generic;

namespace GameBerry.Chart
{
    [System.Serializable]
    public class EquipInfo
    {
        public int ItemId; // ItemId == Equip key
        public GameBerry.Enum_EquipType EquipType;
        public string Name;

        public int BaseAtk;
        public int BaseDef;
        public int BaseHp;

        public float EnhanceStatRatePerLevel = 0.05f;
    }

    public class EquipChart : ChartBase
    {
        public EquipInfo[] rows;
        private Dictionary<int, EquipInfo> _indexToInfo;

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _indexToInfo = new Dictionary<int, EquipInfo>(rows.Length);
            foreach (var r in rows)
                if (r != null) _indexToInfo[r.ItemId] = r;
        }

        public EquipInfo Get(int itemId)
            => _indexToInfo != null && _indexToInfo.TryGetValue(itemId, out var v) ? v : null;
    }
}
