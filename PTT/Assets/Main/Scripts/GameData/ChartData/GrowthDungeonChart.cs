using System;
using System.Collections.Generic;
using GameBerry;

namespace GameBerry.Chart
{
    [System.Serializable]
    public class DungeonRewardPointInfo
    {
        public Enum_PointType PointType;
        public int Amount;
    }

    public class DungeonRuntimeInfo
    {
        public int Stage;
        public Enum_GrowthDungeonRuleType RuleType;
        public string Name;

        public int FieldMonsterKey;
        public int[] FieldMonsterModel;
        public int FieldMonsterCount;
        public float SpawnInterval;

        public int SupportMonsterKey;
        public int SupportMonsterModel;
        public int SupportMonsterCount;
        public float SupportSpawnInterval;

        public int BossMonsterKey;
        public int BossMonsterModel;

        public float TimeLimit;
        public int TargetKillCount;
        public int BossSpawnKillCount;
        public float ExtraTimeOnSupportKill;
        public float PlayerBuffAttackInc;
        public float PlayerBuffMoveSpeedInc;
        public float StunDuration;
        public float StunInterval;
        public float BossInvincibleDuration;
        public float BossWeakDuration;
        public float EffectRadius;

        public string RewardPoints;

        [NonSerialized] private DungeonRewardPointInfo[] _rewardPointInfos;

        public IReadOnlyList<DungeonRewardPointInfo> GetRewardPoints()
        {
            if (_rewardPointInfos == null)
                _rewardPointInfos = DungeonRewardPointParser.Parse(RewardPoints);

            return _rewardPointInfos;
        }
    }

    internal static class DungeonRewardPointParser
    {
        public static DungeonRewardPointInfo[] Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<DungeonRewardPointInfo>();

            string[] entries = raw.Split(';', StringSplitOptions.RemoveEmptyEntries);
            List<DungeonRewardPointInfo> rewards = new List<DungeonRewardPointInfo>(entries.Length);

            for (int i = 0; i < entries.Length; ++i)
            {
                string[] split = entries[i].Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2)
                    continue;

                string pointTypeRaw = split[0].Trim();
                string amountRaw = split[1].Trim();

                if (Enum.TryParse(pointTypeRaw, true, out Enum_PointType pointType) == false)
                    continue;

                if (int.TryParse(amountRaw, out int amount) == false || amount <= 0)
                    continue;

                rewards.Add(new DungeonRewardPointInfo
                {
                    PointType = pointType,
                    Amount = amount
                });
            }

            return rewards.ToArray();
        }
    }

    public class DungeonWeaponInfo : DungeonRuntimeInfo { }
    public class DungeonExperienceInfo : DungeonRuntimeInfo { }
    public class DungeonEquipmentInfo : DungeonRuntimeInfo { }
    public class DungeonTrainingInfo : DungeonRuntimeInfo { }
    public class DungeonEnhanceInfo : DungeonRuntimeInfo { }

    public abstract class DungeonStageChart<TInfo> : ChartBase where TInfo : DungeonRuntimeInfo
    {
        public TInfo this[int index] => rows[index];
        public TInfo[] rows;

        private Dictionary<int, TInfo> _infoByStage;
        private List<TInfo> _sortedRows;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _infoByStage = new Dictionary<int, TInfo>();
            _sortedRows = new List<TInfo>();

            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                TInfo info = rows[i];
                if (info == null || info.Stage <= 0)
                    continue;

                _infoByStage[info.Stage] = info;
                _sortedRows.Add(info);
            }

            _sortedRows.Sort((lhs, rhs) => lhs.Stage.CompareTo(rhs.Stage));
        }

        public bool TryGetInfo(int stage, out TInfo info)
        {
            info = null;
            return _infoByStage != null && _infoByStage.TryGetValue(stage, out info);
        }

        public IReadOnlyList<TInfo> GetRows()
        {
            return _sortedRows;
        }

        public int GetMaxStage()
        {
            if (_sortedRows == null || _sortedRows.Count <= 0)
                return 0;

            return _sortedRows[_sortedRows.Count - 1].Stage;
        }

    }

    public class DungeonWeaponChart : DungeonStageChart<DungeonWeaponInfo> { }
    public class DungeonExperienceChart : DungeonStageChart<DungeonExperienceInfo> { }
    public class DungeonEquipmentChart : DungeonStageChart<DungeonEquipmentInfo> { }
    public class DungeonTrainingChart : DungeonStageChart<DungeonTrainingInfo> { }
    public class DungeonEnhanceChart : DungeonStageChart<DungeonEnhanceInfo> { }
}
