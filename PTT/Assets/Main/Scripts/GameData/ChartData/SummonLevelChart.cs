using System;
using System.Collections.Generic;

namespace GameBerry.Chart
{
    public struct SummonLevelInfo
    {
        public Enum_SummonType SummonType;
        public int SummonLevel;
        public int Exp;
        public string Reward;

        public ItemHandle _RewardItemHandle;
    }

    public class SummonLevelChart : ChartBase
    {
        public SummonLevelInfo this[int index] => rows[index];
        public SummonLevelInfo[] rows;

        public Dictionary<(Enum_SummonType, int), SummonLevelInfo> _summonTypeLevelToInfo;
        private Dictionary<Enum_SummonType, int> _maxLevelByType;
        private Dictionary<Enum_SummonType, List<SummonLevelInfo>> _infosByType;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _summonTypeLevelToInfo = new Dictionary<(Enum_SummonType, int), SummonLevelInfo>();
            _maxLevelByType = new Dictionary<Enum_SummonType, int>();
            _infosByType = new Dictionary<Enum_SummonType, List<SummonLevelInfo>>();

            for(int i = 0; i < rows.Length; ++i)
            {
                var r = rows[i];
                string rewardStr = r.Reward;
                string[] rewardSplit = string.IsNullOrEmpty(rewardStr) ? null : rewardStr.Split('=');

                if (rewardSplit != null && rewardSplit.Length == 2)
                {
                    int rewardItemId = PackUtil.UnpackValue<int>(rewardSplit[0]);
                    long rewardAmount = PackUtil.UnpackValue<long>(rewardSplit[1]);
                    r._RewardItemHandle = ItemHandle.ForMeta(rewardItemId, rewardAmount);
                }
                else
                {
                    r._RewardItemHandle = default;
                }

                _summonTypeLevelToInfo.Add((r.SummonType, r.SummonLevel), r);
                if (_infosByType.TryGetValue(r.SummonType, out List<SummonLevelInfo> list) == false)
                {
                    list = new List<SummonLevelInfo>();
                    _infosByType.Add(r.SummonType, list);
                }
                list.Add(r);

                if (_maxLevelByType.TryGetValue(r.SummonType, out int maxLevel) == false || r.SummonLevel > maxLevel)
                    _maxLevelByType[r.SummonType] = r.SummonLevel;
            }

            foreach (var pair in _infosByType)
            {
                pair.Value.Sort((a, b) => a.SummonLevel.CompareTo(b.SummonLevel));
            }
        }

        public bool TryGetSummonLevelInfo(Enum_SummonType summonType, int summonLevel, out SummonLevelInfo info)
        {
            info = default;
            return _summonTypeLevelToInfo != null && _summonTypeLevelToInfo.TryGetValue((summonType, summonLevel), out info);
        }

        public int GetMaxLevel(Enum_SummonType summonType)
        {
            if (_maxLevelByType != null && _maxLevelByType.TryGetValue(summonType, out int maxLevel))
                return maxLevel;

            return 1;
        }

        public IReadOnlyList<SummonLevelInfo> GetInfos(Enum_SummonType summonType)
        {
            if (_infosByType != null && _infosByType.TryGetValue(summonType, out List<SummonLevelInfo> infos))
                return infos;

            return null;
        }
    }

}
