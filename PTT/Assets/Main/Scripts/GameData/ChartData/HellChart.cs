using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class HellInfo
    {
        public int HellLevel;
        public Enum_Rarity Rarity;
        public double Prob;
    }

    public class HellChart : ChartBase
    {
        public HellInfo this[int index] => rows[index];
        public HellInfo[] rows;
        private Dictionary<int, List<HellInfo>> _infosByLevel;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _infosByLevel = new Dictionary<int, List<HellInfo>>();
            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                HellInfo info = rows[i];
                if (_infosByLevel.TryGetValue(info.HellLevel, out List<HellInfo> list) == false)
                {
                    list = new List<HellInfo>();
                    _infosByLevel.Add(info.HellLevel, list);
                }

                list.Add(info);
            }
        }

        public IReadOnlyList<HellInfo> GetRows(int hellLevel)
        {
            if (_infosByLevel == null)
                return null;

            if (_infosByLevel.TryGetValue(hellLevel, out List<HellInfo> list))
                return list;

            return null;
        }
    }

}
