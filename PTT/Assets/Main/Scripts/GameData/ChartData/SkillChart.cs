using System;
using System.Collections.Generic;

namespace GameBerry.Chart
{
    public enum Enum_SkillType
    {
        Active = 0,
        Passive = 1,
        Max
    }

    public enum Enum_SkillCooldownType
    {
        Time = 0,        // 시간 기반 쿨타임
        AttackCount = 1, // 공격 횟수 기반 쿨타임
        Max
    }

    /// <summary>
    /// 스킬 기본 정보 (공통)
    /// </summary>
    [System.Serializable]
    public class SkillInfo
    {
        public int SkillId;              // 스킬 고유 ID
        public Enum_SkillType SkillType; // Active or Passive

        public string AnimationName;     // 애니메이션 이름
        [NonSerialized]
        public string CustomParam;     // 애니메이션 이름
        public int ResourceIndex;

        public int RequireJobLevel;      // 해금에 필요한 전직 차수 (1~5차)
        public int RequireCharLevel;     // 해금에 필요한 캐릭터 레벨 (액티브만 사용)
        
        // ConditionData indexes (comma-separated in Excel)
        // "1,2,3" -> [1,2,3]
        public string EnemyCondition;

        public List<int> _enemyConditionIndexList;

        public IReadOnlyList<int> GetEnemyConditionIndexes()
        {
            if (_enemyConditionIndexList == null)
            {
                _enemyConditionIndexList = new List<int>();
                if (!string.IsNullOrEmpty(EnemyCondition))
                {
                    var splits = EnemyCondition.Split(',');
                    foreach (var s in splits)
                    {
                        if (int.TryParse(s.Trim(), out int idx))
                            _enemyConditionIndexList.Add(idx);
                    }
                }
            }
            return _enemyConditionIndexList;
        }

        public string MyCondition;

        public List<int> _myConditionIndexList;

        public IReadOnlyList<int> GetMyConditionIndexes()
        {
            if (_myConditionIndexList == null)
            {
                _myConditionIndexList = new List<int>();
                if (!string.IsNullOrEmpty(MyCondition))
                {
                    var splits = MyCondition.Split(',');
                    foreach (var s in splits)
                    {
                        if (int.TryParse(s.Trim(), out int idx))
                            _myConditionIndexList.Add(idx);
                    }
                }
            }
            return _myConditionIndexList;
        }


        // ActiveSKill
        public Enum_SkillCooldownType CooldownType; // 쿨타임 타입
        public float CooldownValue;                 // 쿨타임 값 (초 or 횟수)

        public double BaseAttackMultiplier;         // 스킬 공격력 기본 배율
        public double LevelAttackMultiplier;        // 스킬 공격력 레벨당 추가 배율

        public int HitCount;                        // 스킬 타격 횟수
        public Enum_AttackRangeType AttackRangeType;
        public float AttackAngle;                 // Sector일 때 각도
        public float AttackRange;                 // 때릴려는 범위
        public int TargetCount;                        // 스킬 타격 횟수
        public float HitRange;                 // 타격 범위

        /// <summary>
        /// 스킬 최종 공격배율 계산
        /// 최종 배율 = 기본 배율 + (레벨 추가 배율 * (스킬 레벨 - 1))
        /// </summary>
        public double GetFinalAttackMultiplier(int skillLevel)
        {
            if (skillLevel <= 0)
                skillLevel = 1;

            return BaseAttackMultiplier + (LevelAttackMultiplier * (skillLevel - 1));
        }
        // ActiveSKill

        public AttackStruct GetAttackStruct(CharacterControllerBase hitter, int level = 0)
        {
            AttackStruct attackStruct = new AttackStruct();
            attackStruct.Hitter = hitter;
            attackStruct.AttackLevel = level;
            attackStruct.SkillInfo = this;

            return attackStruct;
        }
    }

    public class SkillChart : ChartBase
    {
        public SkillInfo[] rows;
        
        private Dictionary<int, SkillInfo> _skillIdToInfo;
        private Dictionary<int, List<SkillInfo>> _jobLevelToSkills; // 전직 레벨별 스킬 목록

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _skillIdToInfo = new Dictionary<int, SkillInfo>(rows.Length);
            _jobLevelToSkills = new Dictionary<int, List<SkillInfo>>();

            foreach (var skill in rows)
            {
                if (skill == null) continue;
                
                _skillIdToInfo[skill.SkillId] = skill;

                // 전직 레벨별로 그룹화
                if (!_jobLevelToSkills.ContainsKey(skill.RequireJobLevel))
                    _jobLevelToSkills[skill.RequireJobLevel] = new List<SkillInfo>();

                _jobLevelToSkills[skill.RequireJobLevel].Add(skill);
            }
        }

        public SkillInfo Get(int skillId)
            => _skillIdToInfo != null && _skillIdToInfo.TryGetValue(skillId, out var v) ? v : null;

        public SkillInfo GetActive(int skillId)
        {
            var info = Get(skillId);
            if (info != null && info.SkillType == Enum_SkillType.Active)
                return info;
            return null;
        }

        public SkillInfo GetPassive(int skillId)
        {
            var info = Get(skillId);
            if (info != null && info.SkillType == Enum_SkillType.Passive)
                return info;
            return null;
        }

        /// <summary>
        /// 전직별로 정리한 스킬 목록 반환
        /// </summary> Dictionary<int, List<SkillInfo>> _jobLevelToSkills; // 전직 레벨별 스킬 목록
        public Dictionary<int, List<SkillInfo>> GetJobLevelToSkills()
        {
            return _jobLevelToSkills;
        }

        /// <summary>
        /// 특정 전직 레벨에 해당하는 스킬 목록 반환
        /// </summary>
        public List<SkillInfo> GetSkillsByJobLevel(int jobLevel)
        {
            if (_jobLevelToSkills != null && _jobLevelToSkills.TryGetValue(jobLevel, out var list))
                return list;

            return new List<SkillInfo>();
        }

        /// <summary>
        /// 해금 가능한 스킬 목록 반환
        /// </summary>
        public List<SkillInfo> GetUnlockableSkills(int currentJobLevel, int currentCharLevel)
        {
            var result = new List<SkillInfo>();

            foreach (var skill in rows)
            {
                if (skill == null) continue;

                // 전직 레벨 체크
                if (skill.RequireJobLevel > currentJobLevel)
                    continue;

                // 캐릭터 레벨 체크 (액티브만)
                if (skill.SkillType == Enum_SkillType.Active)
                {
                    if (skill.RequireCharLevel > currentCharLevel)
                        continue;
                }

                result.Add(skill);
            }

            return result;
        }
    }
}
