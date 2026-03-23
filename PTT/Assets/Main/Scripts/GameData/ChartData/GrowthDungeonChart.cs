using System.Collections.Generic;
using GameBerry;

namespace GameBerry.Chart
{
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

        public int RewardExp;
        public Enum_PointType RewardPointType1;
        public int RewardPointAmount1;
        public Enum_PointType RewardPointType2;
        public int RewardPointAmount2;
        public int[] RewardEquipmentItemIds;
        public int RewardEquipmentLevelMin;
        public int RewardEquipmentLevelMax;
        public int RewardEquipmentCount;
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
