using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class SkillCooldownTracker
    {
        public int skillId;
        public Enum_SkillCooldownType cooldownType;
        public float nextAvailableTime;
        public int remainingAttackCount;

        public bool IsReady()
        {
            if (cooldownType == Enum_SkillCooldownType.Time)
                return Time.time >= nextAvailableTime;
            if (cooldownType == Enum_SkillCooldownType.AttackCount)
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

    public class SkillManager : MonoSingleton<SkillManager>
    {
        private readonly List<Table.TableBase> SkillTables = new List<Table.TableBase>()
        {
            Table.UserTable.Get<Table.SkillTable>()
        };

        public event Action<int> OnSkillDataChanged;
        public event Action OnSkillSlotChanged;

        private SkillTable _skillTable;
        private SkillChart _skillChart;

        private const string _iconPath = "Icon/skill/{0}";
        private readonly Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();
        private readonly Dictionary<int, SkillCooldownTracker> _cooldownTrackers = new Dictionary<int, SkillCooldownTracker>();
        private readonly StringBuilder _descBuilder = new StringBuilder(256);

public SkillInfo[] SkillInfoVIew = null;

        protected override void Init()
        {
            _skillTable = UserTable.Get<SkillTable>();
            _skillChart = GameChart.Get<SkillChart>();
            SkillInfoVIew = _skillChart.rows;
        }

        public Sprite GetIcon(int itemId)
        {
            if (itemId <= 0)
                return null;

            if (_skillIcons.TryGetValue(itemId, out Sprite sp))
                return sp;

            ResourceLoader.Instance.Load<Sprite>(string.Format(_iconPath, itemId), o =>
            {
                Sprite loaded = o as Sprite;
                _skillIcons[itemId] = loaded;
            });

            _skillIcons.TryGetValue(itemId, out sp);
            return sp;
        }

        public void RefreshSkillSlot()
        {
            OnSkillSlotChanged?.Invoke();
        }

        public bool CanUnlockSkill(int skillId, int currentJobLevel, int currentCharLevel, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            if (_skillTable.HasSkill(skillId))
                return false;

            SkillInfo skillInfo = _skillChart.Get(skillId);
            if (skillInfo == null)
                return false;
            if (skillInfo.ActorType != actorType)
                return false;
            if (skillInfo.RequireJobLevel > currentJobLevel)
                return false;

            if (skillInfo.SkillType == Enum_SkillType.Active && skillInfo.RequireCharLevel > currentCharLevel)
                return false;

            return true;
        }

        public bool UnlockSkill(int skillId, int currentJobLevel, int currentCharLevel, bool immediateServerUpdate = true, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            if (!CanUnlockSkill(skillId, currentJobLevel, currentCharLevel, actorType))
                return false;
            if (!_skillTable.UnlockSkill(skillId))
                return false;

            SkillInfo skillInfo = _skillChart.Get(skillId);
            if (skillInfo != null && skillInfo.SkillType == Enum_SkillType.Active)
                InitializeCooldownTracker(skillId, actorType);

            UserTable.TransactionUpdate_WaitSecond(SkillTables);
            OnSkillDataChanged?.Invoke(skillId);
            return true;
        }

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

        public int GetSkillLevel(int skillId) => _skillTable.GetSkillLevel(skillId);
        public bool HasSkill(int skillId) => _skillTable.HasSkill(skillId);

        public bool EquipSkillToSlot(int slotIndex, int skillId, bool immediateServerUpdate = true, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            SkillInfo activeInfo = _skillChart.GetActive(skillId, actorType);
            if (activeInfo == null)
                return false;
            if (!_skillTable.EquipSkillToSlot(slotIndex, skillId))
                return false;

            UserTable.TransactionUpdate_WaitSecond(SkillTables);
            OnSkillSlotChanged?.Invoke();
            return true;
        }

        public bool UnequipSkillFromSlot(int slotIndex, bool immediateServerUpdate = true)
        {
            if (!_skillTable.UnequipSkillFromSlot(slotIndex))
                return false;

            UserTable.TransactionUpdate_WaitSecond(SkillTables);
            OnSkillSlotChanged?.Invoke();
            return true;
        }

        public int GetEquippedSkillId(int slotIndex) => _skillTable.GetEquippedSkillId(slotIndex);
        public bool IsSkillEquipped(int skillId) => _skillTable.IsSkillEquipped(skillId);

        public List<int> GetEquippedSkillIds(Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            List<int> result = new List<int>();
            var slots = _skillTable.GetAllSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                int skillId = slots[i].skillId;
                if (skillId <= 0)
                    continue;

                if (_skillChart.GetActive(skillId, actorType) != null)
                    result.Add(skillId);
            }
            return result;
        }

        private void InitializeCooldownTracker(int skillId, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            SkillInfo activeInfo = _skillChart.GetActive(skillId, actorType);
            if (activeInfo == null)
                return;

            if (_cooldownTrackers.ContainsKey(skillId))
                return;

            _cooldownTrackers[skillId] = new SkillCooldownTracker
            {
                skillId = skillId,
                cooldownType = activeInfo.CooldownType,
                nextAvailableTime = 0f,
                remainingAttackCount = 0
            };
        }

        public bool CanUseSkill(int skillId, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            if (!_skillTable.HasSkill(skillId))
                return false;

            SkillInfo activeInfo = _skillChart.GetActive(skillId, actorType);
            if (activeInfo == null)
                return false;

            if (!_cooldownTrackers.TryGetValue(skillId, out SkillCooldownTracker tracker))
            {
                InitializeCooldownTracker(skillId, actorType);
                return true;
            }

            return tracker.IsReady();
        }

        public bool UseSkill(int skillId, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            if (!CanUseSkill(skillId, actorType))
                return false;

            SkillInfo activeInfo = _skillChart.GetActive(skillId, actorType);
            if (activeInfo == null)
                return false;

            if (!_cooldownTrackers.TryGetValue(skillId, out SkillCooldownTracker tracker))
            {
                InitializeCooldownTracker(skillId, actorType);
                tracker = _cooldownTrackers[skillId];
            }

            tracker.StartCooldown(activeInfo.CooldownValue);
            return true;
        }

        public void OnPlayerAttack()
        {
            foreach (var tracker in _cooldownTrackers.Values)
            {
                tracker.OnAttack();
            }
        }

        public float GetRemainingCooldownTime(int skillId)
        {
            if (!_cooldownTrackers.TryGetValue(skillId, out SkillCooldownTracker tracker))
                return 0f;
            if (tracker.cooldownType != Enum_SkillCooldownType.Time)
                return 0f;

            return Mathf.Max(0f, tracker.nextAvailableTime - Time.time);
        }

        public int GetRemainingCooldownAttackCount(int skillId)
        {
            if (!_cooldownTrackers.TryGetValue(skillId, out SkillCooldownTracker tracker))
                return 0;
            if (tracker.cooldownType != Enum_SkillCooldownType.AttackCount)
                return 0;

            return Mathf.Max(0, tracker.remainingAttackCount);
        }

        public double GetSkillFinalAttackMultiplier(int skillId, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            SkillInfo activeInfo = _skillChart.GetActive(skillId, actorType);
            if (activeInfo == null)
                return 0;

            int skillLevel = _skillTable.GetSkillLevel(skillId);
            return activeInfo.GetFinalAttackMultiplier(skillLevel);
        }

        public int GetSkillHitCount(int skillId, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            SkillInfo activeInfo = _skillChart.GetActive(skillId, actorType);
            if (activeInfo == null)
                return 0;

            return activeInfo.HitCount;
        }

        public IReadOnlyList<int> GetSkillConditionIndexes(int skillId)
        {
            SkillInfo skillInfo = _skillChart.Get(skillId);
            if (skillInfo == null)
                return new List<int>();

            return skillInfo.GetEnemyConditionIndexes();
        }

        public List<SkillInfo> GetOwnedPassiveSkills(Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            List<SkillInfo> result = new List<SkillInfo>();
            var allSkills = _skillTable.GetAllSkills();

            foreach (var kvp in allSkills)
            {
                SkillInfo passiveInfo = _skillChart.GetPassive(kvp.Key, actorType);
                if (passiveInfo != null)
                    result.Add(passiveInfo);
            }

            return result;
        }

        public List<SkillInfo> GetOwnedActiveSkills(Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            List<SkillInfo> result = new List<SkillInfo>();
            var allSkills = _skillTable.GetAllSkills();

            foreach (var kvp in allSkills)
            {
                SkillInfo activeInfo = _skillChart.GetActive(kvp.Key, actorType);
                if (activeInfo != null)
                    result.Add(activeInfo);
            }

            return result;
        }

        public void ApplyPassiveSkillEffects(CharacterControllerBase character, Enum_SkillActorType actorType = Enum_SkillActorType.Player)
        {
            if (character == null)
                return;

            var passiveSkills = GetOwnedPassiveSkills(actorType);
            foreach (var passive in passiveSkills)
            {
                Debug.Log($"[SkillManager] Applied passive skill: {passive.SkillId}");
            }
        }

        public string GetSkillNameText(int skillId)
        {
            if (skillId <= 0)
                return "-";

            return $"Skill {skillId}";
        }

        public string GetSkillConditionDescription(SkillInfo skillInfo)
        {
            if (skillInfo == null)
                return "-";

            _descBuilder.Clear();
            int setCount = 0;

            IReadOnlyList<int> conditions = skillInfo.GetMyConditionIndexes();
            for (int i = 0; i < conditions.Count; ++i)
            {
                if (setCount > 0)
                    _descBuilder.Append(", ");

                _descBuilder.Append(StaticResource.Instance.GetConditionData().GetConditionDataDesc(conditions[i]));
                setCount++;
            }

            conditions = skillInfo.GetEnemyConditionIndexes();
            for (int i = 0; i < conditions.Count; ++i)
            {
                if (setCount > 0)
                    _descBuilder.Append(", ");

                _descBuilder.Append(StaticResource.Instance.GetConditionData().GetConditionDataDesc(conditions[i]));
                setCount++;
            }

            return setCount > 0 ? _descBuilder.ToString() : "-";
        }

        public string GetSkillDescriptionText(SkillInfo skillInfo, int displayLevel = 1)
        {
            if (skillInfo == null)
                return "-";

            if (skillInfo.SkillType == Enum_SkillType.Active)
            {
                int level = Mathf.Max(1, displayLevel);
                double multiplier = skillInfo.GetFinalAttackMultiplier(level) * 100.0;
                return $"Deals <color=#E50008>{multiplier:0.#}%</color>damage to <color=#FFFFFF>{skillInfo.TargetCount}</color> nearby target(s) <color=#FFFFFF>{skillInfo.HitCount}</color> time(s).";
            }

            return string.Format("{0}Job Passive", skillInfo.RequireJobLevel);
        }

        public string GetCooldownTypeText(SkillInfo skillInfo)
        {
            if (skillInfo == null)
                return "-";

            return skillInfo.CooldownType.ToString();
        }

        public string GetCooldownValueText(SkillInfo skillInfo)
        {
            if (skillInfo == null)
                return "-";

            if (skillInfo.CooldownType == Enum_SkillCooldownType.Time)
                return $"{skillInfo.CooldownValue:F1}s";

            return $"{skillInfo.CooldownValue:F0} attacks";
        }
    }
}
