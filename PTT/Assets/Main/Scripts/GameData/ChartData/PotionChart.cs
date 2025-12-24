using System.Collections.Generic;

namespace GameBerry.Chart
{
    public enum Enum_PotionType { None=0, HP=1, MP=2 }
    public enum Enum_PotionGrade { None=0, Low=1, Mid=2, High=3 }

    [System.Serializable]
    public class PotionInfo
    {
        public int ItemId; // ItemId == Potion key
        public Enum_PotionType Type;
        public Enum_PotionGrade Grade;
        public string Name;
        public int HealAmount;
    }

    public class PotionChart : ChartBase
    {
        public PotionInfo[] rows;
        private Dictionary<int, PotionInfo> _indexToInfo;

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _indexToInfo = new Dictionary<int, PotionInfo>(rows.Length);
            foreach (var r in rows)
                if (r != null) _indexToInfo[r.ItemId] = r;
        }

        public PotionInfo Get(int itemId)
            => _indexToInfo != null && _indexToInfo.TryGetValue(itemId, out var v) ? v : null;
    }
}
