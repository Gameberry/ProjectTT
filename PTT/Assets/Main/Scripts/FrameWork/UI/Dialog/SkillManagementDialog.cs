using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.UI
{
    /// <summary>
    /// 스킬 관리 다이얼로그
    /// - 스킬 슬롯 표시 및 장착/해제
    /// - 보유한 스킬 목록 표시
    /// - 스킬 상세 정보
    /// </summary>
    public class SkillManagementDialog : IDialog
    {
        [Header("Skill Slots")]
        [SerializeField] private UISkillSlotGroup _uISkillSlotGroup;

        [Header("Skill List")]
        [SerializeField] private Transform _skillListContent;
        [SerializeField] private UISkillJobGroup _skillJobGroupPrefab;
        private List<UISkillJobGroup> _spawnSkillJobGroups = new List<UISkillJobGroup>();

        [Header("Selected Skill Info")]
        [SerializeField] private Transform _selectedSkillInfoGroup;
        [SerializeField] private Image _selectedSkillIcon;
        [SerializeField] private TMP_Text _selectedSkillName;
        [SerializeField] private TMP_Text _selectedSkillType;
        [SerializeField] private TMP_Text _selectedSkillLevel;
        [SerializeField] private TMP_Text _selectedSkillConditionDataDescription;
        [SerializeField] private TMP_Text _selectedSkillDescription;

        [Header("Active CoolTime Info")]
        [SerializeField] private Transform _activeSkillCoolTimeGroup;
        [SerializeField] private TMP_Text _cooldownTypeText;
        [SerializeField] private TMP_Text _cooldownValueText;

        [Header("Buttons")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Button _levelUpButton;

        private int _selectedSkillId = 0;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            _uISkillSlotGroup?.OnConnect_SlotClicked(OnSlotClicked);

            // 액션 버튼
            if (_equipButton != null)
                _equipButton.onClick.AddListener(OnEquipButtonClicked);

            if (_unequipButton != null)
                _unequipButton.onClick.AddListener(OnUnequipButtonClicked);

            if (_levelUpButton != null)
                _levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);

            SkillChart skillChart = Chart.GameChart.Get<SkillChart>();
            if (skillChart != null)
            {
                foreach (var pair in skillChart.GetJobLevelToSkills(Enum_SkillActorType.Player))
                {
                    var element = Instantiate(_skillJobGroupPrefab, _skillListContent);
                    element.SetJobSkill(pair.Key, pair.Value, OnSkillElementClicked);
                    _spawnSkillJobGroups.Add(element);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            SkillManager.Instance.OnSkillDataChanged += RefreshSkill;

            ClearSelectedSkillInfo();

            RefreshAll();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.OnSkillDataChanged -= RefreshSkill;
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshSkill(int skillId)
        {
            RefreshSkillList();

            if (_selectedSkillId == skillId)
                ShowSkillInfo(_selectedSkillId);
        }
        //------------------------------------------------------------------------------------
        private void RefreshAll()
        {
            RefreshSkillList();

            if (_selectedSkillId > 0)
                ShowSkillInfo(_selectedSkillId);
        }
        //------------------------------------------------------------------------------------
        private void RefreshSkillList()
        {
            for (int i = 0; i < _spawnSkillJobGroups.Count; ++i)
            {
                _spawnSkillJobGroups[i].Refresh();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnSlotClicked(int slotIndex)
        {
            int skillId = SkillManager.Instance.GetEquippedSkillId(slotIndex);
            if (skillId > 0)
            {
                ShowSkillInfo(skillId);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnSkillElementClicked(int skillId)
        {
            ShowSkillInfo(skillId);
        }
        //------------------------------------------------------------------------------------
        private void ShowSkillInfo(int skillId)
        {
            _selectedSkillId = skillId;

            SkillChart skillChart = Chart.GameChart.Get<SkillChart>();
            SkillInfo skillInfo = skillChart?.Get(skillId);

            if (skillInfo == null)
            {
                ClearSelectedSkillInfo();
                return;
            }

            if (_selectedSkillInfoGroup != null)
                _selectedSkillInfoGroup.gameObject.SetActive(true);

            // 기본 정보
            if (_selectedSkillIcon != null)
            {
                // TODO: 스킬 아이콘 로드
                _selectedSkillIcon.sprite = SkillManager.Instance.GetIcon(skillInfo.SkillId);
            }

            if (_selectedSkillName != null)
            {
                // TODO: 로컬라이제이션
                _selectedSkillName.SetText($"Skill {skillId}");
            }

            if (_selectedSkillType != null)
                _selectedSkillType.SetText(skillInfo.SkillType.ToString());

            bool hasSkill = SkillManager.Instance.HasSkill(skillId);
            int level = 0;

            if (_selectedSkillLevel != null)
            {
                if (hasSkill)
                {
                    level = SkillManager.Instance.GetSkillLevel(skillId);
                    _selectedSkillLevel.SetText($"Lv.{level}");
                    _selectedSkillLevel.gameObject.SetActive(true);
                }
                else
                {
                    _selectedSkillLevel.gameObject.SetActive(false);
                }
            }

            if (_selectedSkillConditionDataDescription != null)
            {
                string desc = "-";

                int setCount = 0;

                IReadOnlyList<int> conditions = skillInfo.GetMyConditionIndexes();

                for (int i = 0; i < conditions.Count; ++i)
                {
                    if(setCount == 0)
                        desc = StaticResource.Instance.GetConditionData().GetConditionDataDesc(conditions[i]);
                    else
                        desc += string.Format(", {0}", StaticResource.Instance.GetConditionData().GetConditionDataDesc(conditions[i]));

                    setCount++;
                }

                conditions = skillInfo.GetEnemyConditionIndexes();

                for (int i = 0; i < conditions.Count; ++i)
                {
                    if (setCount == 0)
                        desc = StaticResource.Instance.GetConditionData().GetConditionDataDesc(conditions[i]);
                    else
                        desc += string.Format(", {0}", StaticResource.Instance.GetConditionData().GetConditionDataDesc(conditions[i]));

                    setCount++;
                }
                _selectedSkillConditionDataDescription.SetText(desc);
            }


            // 액티브 스킬 추가 정보
            if (skillInfo.SkillType == Enum_SkillType.Active)
            {
                if (_selectedSkillDescription != null)
                {
                    string desc = $"Deals <color=#E50008>{SkillManager.Instance.GetSkillFinalAttackMultiplier(skillInfo.SkillId) * 100}%</color>" +
                        $"damage to <color=#FFFFFF>{skillInfo.TargetCount}</color> nearby target(s) " +
                        $"<color=#FFFFFF>{skillInfo.HitCount}</color> time(s).";
                    _selectedSkillDescription.SetText(desc);
                }

                ShowActiveCoolTimeInfo(skillInfo);
            }
            else
            {
                if (_selectedSkillDescription != null)
                    _selectedSkillDescription.SetText("{0}Job Passive", skillInfo.RequireJobLevel);

                if (_activeSkillCoolTimeGroup != null)
                    _activeSkillCoolTimeGroup.gameObject.SetActive(false);
            }

            // 버튼 상태 갱신
            RefreshButtons(skillInfo, hasSkill);
        }
        //------------------------------------------------------------------------------------
        private void ShowActiveCoolTimeInfo(SkillInfo activeInfo)
        {
            if (_activeSkillCoolTimeGroup != null)
                _activeSkillCoolTimeGroup.gameObject.SetActive(true);

            if (_cooldownTypeText != null)
                _cooldownTypeText.SetText(activeInfo.CooldownType.ToString());

            if (_cooldownValueText != null)
            {
                if (activeInfo.CooldownType == Enum_SkillCooldownType.Time)
                    _cooldownValueText.SetText($"{activeInfo.CooldownValue:F1}s");
                else
                    _cooldownValueText.SetText($"{activeInfo.CooldownValue:F0} attacks");
            }

        }
        //------------------------------------------------------------------------------------
        private void RefreshButtons(SkillInfo skillInfo, bool hasSkill)
        {
            if (!hasSkill)
            {
                // 미해금 스킬
                if (_equipButton != null) _equipButton.gameObject.SetActive(false);
                if (_unequipButton != null) _unequipButton.gameObject.SetActive(false);
                if (_levelUpButton != null) _levelUpButton.gameObject.SetActive(false);
                return;
            }

            // 레벨업 버튼은 항상 표시
            if (_levelUpButton != null)
                _levelUpButton.gameObject.SetActive(true);

            // 액티브 스킬만 장착/해제 가능
            if (skillInfo.SkillType == Enum_SkillType.Active)
            {
                bool isEquipped = SkillManager.Instance.IsSkillEquipped(_selectedSkillId);

                if (_equipButton != null)
                    _equipButton.gameObject.SetActive(!isEquipped);

                if (_unequipButton != null)
                    _unequipButton.gameObject.SetActive(isEquipped);
            }
            else
            {
                if (_equipButton != null) _equipButton.gameObject.SetActive(false);
                if (_unequipButton != null) _unequipButton.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void ClearSelectedSkillInfo()
        {
            _selectedSkillId = 0;

            if (_selectedSkillInfoGroup != null)
                _selectedSkillInfoGroup.gameObject.SetActive(false);

            if (_activeSkillCoolTimeGroup != null)
                _activeSkillCoolTimeGroup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void OnEquipButtonClicked()
        {
            if (_selectedSkillId <= 0)
                return;

            // 빈 슬롯 찾기
            int emptySlot = -1;
            for (int i = 0; i < SkillTable.MaxSlotCount; i++)
            {
                if (SkillManager.Instance.GetEquippedSkillId(i) <= 0)
                {
                    emptySlot = i;
                    break;
                }
            }

            if (emptySlot < 0)
            {
                Debug.LogWarning("[SkillManagementDialog] No empty slot available");
                return;
            }

            if (SkillManager.Instance.EquipSkillToSlot(emptySlot, _selectedSkillId))
            {
                RefreshAll();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnUnequipButtonClicked()
        {
            if (_selectedSkillId <= 0)
                return;

            // 장착된 슬롯 찾기
            int slotIndex = UserTable.Get<SkillTable>().FindSlotIndexBySkillId(_selectedSkillId);
            if (slotIndex < 0)
                return;

            if (SkillManager.Instance.UnequipSkillFromSlot(slotIndex))
            {
                RefreshAll();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnLevelUpButtonClicked()
        {
            if (_selectedSkillId <= 0)
                return;

            // TODO: 레벨업 비용 체크 (골드, 재화 등)

            if (SkillManager.Instance.LevelUpSkill(_selectedSkillId))
            {
                ShowSkillInfo(_selectedSkillId); // 정보 갱신
            }
        }
        //------------------------------------------------------------------------------------
    }
}
