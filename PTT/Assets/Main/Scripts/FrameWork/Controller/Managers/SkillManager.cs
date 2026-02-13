using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class SkillCooldownTracker
    {
        public int skillId;
        public Enum_SkillCooldownType cooldownType;
        public float nextAvailableTime;      // Time 기반 쿨타임용
        public int remainingAttackCount;     // AttackCount 기반 쿨타임용

        public bool IsReady()
        {
            if (cooldownType == Enum_SkillCooldownType.Time)
                return Time.time >= nextAvailableTime;
            else if (cooldownType == Enum_SkillCooldownType.AttackCount)
                return remainingAttackCount <= 0;

            return true;
        }

        public void StartCooldown(float cooldownValue)
        {
            if (cooldownType == Enum_SkillCooldownType.Time)
                nextAvailableTime = Time.time + cooldownValue;
            else if (cooldownType == Enum_SkillCooldownType.AttackCount)
                remainingAttackCount = Mathf.CeilToInt(cooldownValue);
        }

        public void OnAttack()
        {
            if (cooldownType == Enum_SkillCooldownType.AttackCount && remainingAttackCount > 0)
                remainingAttackCount--;
        }
    }

    public class SkillManager : Singleton<SkillManager>
    {
        private List<Table.TableBase> SkillTables = new List<Table.TableBase>()
        {
            Table.UserTable.Get<Table.SkillTable>()
        };

        public event Action<int> OnSkillDataChanged;    // 스킬 해금, 레벨업 등
        public event Action OnSkillSlotChanged;    // 스킬 슬롯 변경

        private SkillTable _skillTable;
        private SkillChart _skillChart;

        private const string _iconPath = "Icon/skill/{0}";
        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        // 쿨타임 추적용
        private Dictionary<int, SkillCooldownTracker> _cooldownTrackers = new Dictionary<int, SkillCooldownTracker>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _skillTable = UserTable.Get<SkillTable>();
            _skillChart = GameChart.Get<SkillChart>();
        }
        //------------------------------------------------------------------------------------
        public Sprite GetIcon(int itemId)
        {
            Sprite sp = null;

            if (itemId <= 0)
                return null;

            if (_skillIcons.ContainsKey(itemId) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(_iconPath, itemId), o =>
                {
                    sp = o as Sprite;
                    _skillIcons.Add(itemId, sp);
                });
            }
            else
                sp = _skillIcons[itemId];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public void RefreshSkillSlot()
        {
            OnSkillSlotChanged?.Invoke();
        }
        //------------------------------------------------------------------------------------
        #region Skill Unlock & Level
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 해금 가능 여부 체크
        /// </summary>
        public bool CanUnlockSkill(int skillId, int currentJobLevel, int currentCharLevel)
        {
            if (_skillTable.HasSkill(skillId))
                return false; // 이미 보유

            SkillInfo skillInfo = _skillChart.Get(skillId);
            if (skillInfo == null)
                return false;

            // 전직 레벨 체크
            if (skillInfo.RequireJobLevel > currentJobLevel)
                return false;

            // 캐릭터 레벨 체크 (액티브 스킬만)
            if (skillInfo.SkillType == Enum_SkillType.Active)
            {
                if (skillInfo.RequireCharLevel > currentCharLevel)
                    return false;
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 해금
        /// </summary>
        public bool UnlockSkill(int skillId, int currentJobLevel, int currentCharLevel, bool immediateServerUpdate = true)
        {
            if (!CanUnlockSkill(skillId, currentJobLevel, currentCharLevel))
                return false;

            if (!_skillTable.UnlockSkill(skillId))
                return false;

            // 쿨타임 트래커 초기화 (액티브 스킬만)
            SkillInfo skillInfo = _skillChart.Get(skillId);
            if (skillInfo != null && skillInfo.SkillType == Enum_SkillType.Active)
            {
                InitializeCooldownTracker(skillId);
            }

            UserTable.TransactionUpdate_WaitSecond(SkillTables);

            OnSkillDataChanged?.Invoke(skillId);

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 레벨업
        /// </summary>
        public bool LevelUpSkill(int skillId, bool immediateServerUpdate = true)
        {
            if (!_skillTable.HasSkill(skillId))
                return false;

            if (!_skillTable.LevelUpSkill(skillId))
                return false;

            UserTable.TransactionUpdate_WaitSecond(SkillTables);

            OnSkillDataChanged?.Invoke(skillId);

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 레벨 가져오기
        /// </summary>
        public int GetSkillLevel(int skillId)
        {
            return _skillTable.GetSkillLevel(skillId);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 보유 여부
        /// </summary>
        public bool HasSkill(int skillId)
        {
            return _skillTable.HasSkill(skillId);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Skill Slot Management
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 슬롯에 스킬 장착
        /// </summary>
        public bool EquipSkillToSlot(int slotIndex, int skillId, bool immediateServerUpdate = true)
        {
            // 액티브 스킬만 장착 가능
            SkillInfo activeInfo = _skillChart.GetActive(skillId);
            if (activeInfo == null)
                return false;

            if (!_skillTable.EquipSkillToSlot(slotIndex, skillId))
                return false;

            UserTable.TransactionUpdate_WaitSecond(SkillTables);

            OnSkillSlotChanged?.Invoke();

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 슬롯에서 스킬 해제
        /// </summary>
        public bool UnequipSkillFromSlot(int slotIndex, bool immediateServerUpdate = true)
        {
            if (!_skillTable.UnequipSkillFromSlot(slotIndex))
                return false;

            UserTable.TransactionUpdate_WaitSecond(SkillTables);

            OnSkillSlotChanged?.Invoke();

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 슬롯에 장착된 스킬 ID 가져오기
        /// </summary>
        public int GetEquippedSkillId(int slotIndex)
        {
            return _skillTable.GetEquippedSkillId(slotIndex);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬이 장착되어 있는지 확인
        /// </summary>
        public bool IsSkillEquipped(int skillId)
        {
            return _skillTable.IsSkillEquipped(skillId);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 모든 장착된 액티브 스킬 목록 (skillId만)
        /// </summary>
        public List<int> GetEquippedSkillIds()
        {
            List<int> result = new List<int>();
            var slots = _skillTable.GetAllSlots();

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].skillId > 0)
                    result.Add(slots[i].skillId);
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Cooldown Management
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 쿨타임 트래커 초기화
        /// </summary>
        private void InitializeCooldownTracker(int skillId)
        {
            SkillInfo activeInfo = _skillChart.GetActive(skillId);
            if (activeInfo == null)
                return;

            if (!_cooldownTrackers.ContainsKey(skillId))
            {
                _cooldownTrackers[skillId] = new SkillCooldownTracker
                {
                    skillId = skillId,
                    cooldownType = activeInfo.CooldownType,
                    nextAvailableTime = 0f,
                    remainingAttackCount = 0
                };
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 사용 가능 여부 체크
        /// </summary>
        public bool CanUseSkill(int skillId)
        {
            if (!_skillTable.HasSkill(skillId))
                return false;

            SkillInfo activeInfo = _skillChart.GetActive(skillId);
            if (activeInfo == null)
                return false; // 패시브는 사용 불가

            if (!_cooldownTrackers.TryGetValue(skillId, out var tracker))
            {
                InitializeCooldownTracker(skillId);
                return true; // 첫 사용
            }

            return tracker.IsReady();
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 사용 (쿨타임 시작)
        /// </summary>
        public bool UseSkill(int skillId)
        {
            if (!CanUseSkill(skillId))
                return false;

            SkillInfo activeInfo = _skillChart.GetActive(skillId);
            if (activeInfo == null)
                return false;

            if (!_cooldownTrackers.TryGetValue(skillId, out var tracker))
            {
                InitializeCooldownTracker(skillId);
                tracker = _cooldownTrackers[skillId];
            }

            tracker.StartCooldown(activeInfo.CooldownValue);

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 공격 횟수 기반 쿨타임 감소 (플레이어가 공격할 때마다 호출)
        /// </summary>
        public void OnPlayerAttack()
        {
            foreach (var tracker in _cooldownTrackers.Values)
            {
                tracker.OnAttack();
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 남은 쿨타임 시간 가져오기 (초 단위, Time 기반만)
        /// </summary>
        public float GetRemainingCooldownTime(int skillId)
        {
            if (!_cooldownTrackers.TryGetValue(skillId, out var tracker))
                return 0f;

            if (tracker.cooldownType != Enum_SkillCooldownType.Time)
                return 0f;

            float remaining = tracker.nextAvailableTime - Time.time;
            return Mathf.Max(0f, remaining);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 남은 쿨타임 공격 횟수 가져오기 (AttackCount 기반만)
        /// </summary>
        public int GetRemainingCooldownAttackCount(int skillId)
        {
            if (!_cooldownTrackers.TryGetValue(skillId, out var tracker))
                return 0;

            if (tracker.cooldownType != Enum_SkillCooldownType.AttackCount)
                return 0;

            return Mathf.Max(0, tracker.remainingAttackCount);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Skill Info & Stats
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 최종 공격 배율 계산
        /// </summary>
        public double GetSkillFinalAttackMultiplier(int skillId)
        {
            SkillInfo activeInfo = _skillChart.GetActive(skillId);
            if (activeInfo == null)
                return 0;

            int skillLevel = _skillTable.GetSkillLevel(skillId);
            return activeInfo.GetFinalAttackMultiplier(skillLevel);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 타격 횟수
        /// </summary>
        public int GetSkillHitCount(int skillId)
        {
            SkillInfo activeInfo = _skillChart.GetActive(skillId);
            if (activeInfo == null)
                return 0;

            return activeInfo.HitCount;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬의 ConditionData 인덱스 목록
        /// </summary>
        public IReadOnlyList<int> GetSkillConditionIndexes(int skillId)
        {
            SkillInfo skillInfo = _skillChart.Get(skillId);
            if (skillInfo == null)
                return new List<int>();

            return skillInfo.GetEnemyConditionIndexes();
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 보유한 모든 패시브 스킬 목록
        /// </summary>
        public List<SkillInfo> GetOwnedPassiveSkills()
        {
            List<SkillInfo> result = new List<SkillInfo>();

            var allSkills = _skillTable.GetAllSkills();
            foreach (var kvp in allSkills)
            {
                SkillInfo passiveInfo = _skillChart.GetPassive(kvp.Key);
                if (passiveInfo != null && passiveInfo.SkillType == Enum_SkillType.Passive)
                    result.Add(passiveInfo);
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 보유한 모든 액티브 스킬 목록
        /// </summary>
        public List<SkillInfo> GetOwnedActiveSkills()
        {
            List<SkillInfo> result = new List<SkillInfo>();

            var allSkills = _skillTable.GetAllSkills();
            foreach (var kvp in allSkills)
            {
                SkillInfo activeInfo = _skillChart.GetActive(kvp.Key);
                if (activeInfo != null)
                    result.Add(activeInfo);
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Passive Skill Effects
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 패시브 스킬 효과를 플레이어에게 적용
        /// (ConditionData 기반 스탯 버프 등은 별도 시스템에서 처리)
        /// </summary>
        public void ApplyPassiveSkillEffects(CharacterControllerBase character)
        {
            if (character == null)
                return;

            var passiveSkills = GetOwnedPassiveSkills();

            foreach (var passive in passiveSkills)
            {
                // ConditionData 인덱스를 통해 실제 효과 적용
                // 예: ConditionManager.ApplyConditions(character, passive.GetConditionIndexes());
                
                // TODO: ConditionData 시스템과 연동하여 실제 버프/디버프 적용
                // 지금은 예시로 주석 처리
                
                Debug.Log($"[SkillManager] Applied passive skill: {passive.SkillId}");
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
