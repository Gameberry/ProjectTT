using System;
using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class MonsterInfo
    {
        public int Index;
        public string Stat;

        
        [NonSerialized] private Dictionary<Enum_Stat, double> MonsterStat;

        public IReadOnlyDictionary<Enum_Stat, double> GetBaseStats()
        {
            if (MonsterStat == null)
                MonsterStat = StatHelper.ParseStatsPacked(Stat);
            return MonsterStat;
        }
    }

    public class MonsterChart : ChartBase
    {
        public MonsterInfo this[int index] => rows[index];
        public MonsterInfo[] rows;
        private Dictionary<int, MonsterInfo> _infoByIndex;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _infoByIndex = new Dictionary<int, MonsterInfo>();

            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                MonsterInfo info = rows[i];
                if (info == null)
                    continue;

                _infoByIndex[info.Index] = info;
            }
        }

        public bool TryGetInfo(int index, out MonsterInfo info)
        {
            info = null;
            return _infoByIndex != null && _infoByIndex.TryGetValue(index, out info);
        }
    }

}
