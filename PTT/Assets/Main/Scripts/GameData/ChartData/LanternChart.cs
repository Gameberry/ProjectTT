using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class LanternInfo
    {
        public int ItemId;
        public int SpineResourceId;
        public string EquipStat;
        public string OwnStat;
        public int Skill;

        [System.NonSerialized] private Dictionary<Enum_Stat, double> _equipStatDict;
        [System.NonSerialized] private Dictionary<Enum_Stat, double> _ownStatDict;

        public IReadOnlyDictionary<Enum_Stat, double> GetEquipStats()
        {
            if (_equipStatDict == null)
                _equipStatDict = StatHelper.ParseStatsPacked(EquipStat);
            return _equipStatDict;
        }

        public IReadOnlyDictionary<Enum_Stat, double> GetOwnStats()
        {
            if (_ownStatDict == null)
                _ownStatDict = StatHelper.ParseStatsPacked(OwnStat);
            return _ownStatDict;
        }
    }

    public class LanternChart : ChartBase
    {
        public LanternInfo this[int index] => rows[index];
        public LanternInfo[] rows;
        private Dictionary<int, LanternInfo> _itemIdToInfo;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _itemIdToInfo = new Dictionary<int, LanternInfo>(rows.Length);
            for (int i = 0; i < rows.Length; ++i)
            {
                LanternInfo info = rows[i];
                if (info == null)
                    continue;

                _itemIdToInfo[info.ItemId] = info;
            }
        }

        public LanternInfo Get(int itemId)
            => _itemIdToInfo != null && _itemIdToInfo.TryGetValue(itemId, out var v) ? v : null;
    }

}
