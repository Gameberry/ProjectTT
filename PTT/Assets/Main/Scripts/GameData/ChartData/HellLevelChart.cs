using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class HellLevelInfo
    {
        public int HellLevel;
        public int Exp;
        public long LevelUpTimeSec;
    }

    public class HellLevelChart : ChartBase
    {
        public HellLevelInfo this[int index] => rows[index];
        public HellLevelInfo[] rows;
        private Dictionary<int, HellLevelInfo> _infoByLevel;
        private int _maxLevel = 1;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _infoByLevel = new Dictionary<int, HellLevelInfo>();
            _maxLevel = 1;

            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                HellLevelInfo info = rows[i];
                _infoByLevel[info.HellLevel] = info;
                if (info.HellLevel > _maxLevel)
                    _maxLevel = info.HellLevel;
            }
        }

        public bool TryGetInfo(int hellLevel, out HellLevelInfo info)
        {
            info = default;
            return _infoByLevel != null && _infoByLevel.TryGetValue(hellLevel, out info);
        }

        public int GetMaxLevel()
        {
            return _maxLevel < 1 ? 1 : _maxLevel;
        }
    }

}
