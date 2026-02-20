using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class SummonPriceInfo
    {
        public Enum_SummonType SummonType;
        public int MainPoint;
        public int MainPointPrice;
        public int SubPoint;
        public int SubPointPrice;
        public int DailyAdViewCount;
    }

    public class SummonPriceChart : ChartBase
    {
        public SummonPriceInfo this[int index] => rows[index];
        public SummonPriceInfo[] rows;
        private Dictionary<Enum_SummonType, SummonPriceInfo> _typeToInfo;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _typeToInfo = new Dictionary<Enum_SummonType, SummonPriceInfo>();
            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                SummonPriceInfo info = rows[i];
                _typeToInfo[info.SummonType] = info;
            }
        }

        public bool TryGetInfo(Enum_SummonType summonType, out SummonPriceInfo info)
        {
            info = default;
            return _typeToInfo != null && _typeToInfo.TryGetValue(summonType, out info);
        }
    }

}
