using System.Collections.Generic;

namespace GameBerry.Chart
{
    public struct SummonInfo
    {
        public Enum_SummonType SummonType;
        public int SummonLevel;
        public int Item;
        public double Prob;
    }

    public class SummonChart : ChartBase
    {
        public SummonInfo this[int index] => rows[index];
        public SummonInfo[] rows;
        private Dictionary<(Enum_SummonType, int), List<SummonInfo>> _summonTypeLevelToInfos;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _summonTypeLevelToInfos = new Dictionary<(Enum_SummonType, int), List<SummonInfo>>();
            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                SummonInfo info = rows[i];
                var key = (info.SummonType, info.SummonLevel);

                if (_summonTypeLevelToInfos.TryGetValue(key, out var list) == false)
                {
                    list = new List<SummonInfo>();
                    _summonTypeLevelToInfos.Add(key, list);
                }

                list.Add(info);
            }
        }

        public IReadOnlyList<SummonInfo> GetRows(Enum_SummonType summonType, int summonLevel)
        {
            if (_summonTypeLevelToInfos == null)
                return null;

            if (_summonTypeLevelToInfos.TryGetValue((summonType, summonLevel), out var list))
                return list;

            return null;
        }
    }

}
