using System.Collections.Generic;

namespace GameBerry.Chart
{
    [System.Serializable]
    public class PointInfo
    {
        public int ItemId; // ItemId == Point key
        public GameBerry.Enum_PointType Type;
        public string Name;
        public bool ShowInWallet = true;
        public int SortOrder = 0;
    }

    public class PointChart : ChartBase
    {
        public PointInfo[] rows;
        private Dictionary<int, PointInfo> _idToInfo;
        private Dictionary<GameBerry.Enum_PointType, PointInfo> _typeToInfo;

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _idToInfo = new Dictionary<int, PointInfo>(rows.Length);
            _typeToInfo = new Dictionary<GameBerry.Enum_PointType, PointInfo>();

            foreach (var r in rows)
            {
                if (r == null) continue;
                _idToInfo[r.ItemId] = r;
                _typeToInfo[r.Type] = r;
            }
        }

        public PointInfo Get(int itemId)
            => _idToInfo != null && _idToInfo.TryGetValue(itemId, out var v) ? v : null;

        public PointInfo GetByType(GameBerry.Enum_PointType type)
            => _typeToInfo != null && _typeToInfo.TryGetValue(type, out var v) ? v : null;
    }
}
