using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.UI
{
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

        protected override void OnLoad()
        {
            _uISkillSlotGroup?.OnConnect_SlotClicked(OnSlotClicked);

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
                    UISkillJobGroup element = Instantiate(_skillJobGroupPrefab, _skillListContent);
                    element.SetJobSkill(pair.Key, pair.Value, OnSkillElementClicked);
                    _spawnSkillJobGroups.Add(element);
                }
            }
        }

        protected override void OnEnter()
        {
            SkillManager.Instance.OnSkillDataChanged += RefreshSkill;

            ClearSelectedSkillInfo();
            RefreshAll();
        }

        protected override void OnExit()
        {
            if (SkillManager.Instance != null)
                SkillManager.Instance.OnSkillDataChanged -= RefreshSkill;
        }

        private void RefreshSkill(int skillId)
        {
            RefreshSkillList();

            if (_selectedSkillId == skillId)
                ShowSkillInfo(_selectedSkillId);
        }

        private void RefreshAll()
        {
            RefreshSkillList();

            if (_selectedSkillId > 0)
                ShowSkillInfo(_selectedSkillId);
        }

        private void RefreshSkillList()
        {
            for (int i = 0; i < _spawnSkillJobGroups.Count; ++i)
                _spawnSkillJobGroups[i].Refresh();
        }

        private void OnSlotClicked(int slotIndex)
        {
            int skillId = SkillManager.Instance.GetEquippedSkillId(slotIndex);
            if (skillId > 0)
                ShowSkillInfo(skillId);
        }

        private void OnSkillElementClicked(int skillId)
        {
            ShowSkillInfo(skillId);
        }

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

            if (_selectedSkillIcon != null)
                _selectedSkillIcon.sprite = SkillManager.Instance.GetIcon(skillInfo.SkillId);

            if (_selectedSkillName != null)
                _selectedSkillName.SetText(SkillManager.Instance.GetSkillNameText(skillId));

            if (_selectedSkillType != null)
                _selectedSkillType.SetText(skillInfo.SkillType.ToString());

            bool hasSkill = SkillManager.Instance.HasSkill(skillId);
            int level = hasSkill ? Mathf.Max(1, SkillManager.Instance.GetSkillLevel(skillId)) : 1;

            if (_selectedSkillLevel != null)
            {
                if (hasSkill)
                {
                    _selectedSkillLevel.SetText($"Lv.{level}");
                    _selectedSkillLevel.gameObject.SetActive(true);
                }
                else
                {
                    _selectedSkillLevel.gameObject.SetActive(false);
                }
            }

            if (_selectedSkillConditionDataDescription != null)
                _selectedSkillConditionDataDescription.SetText(SkillManager.Instance.GetSkillConditionDescription(skillInfo));

            if (_selectedSkillDescription != null)
                _selectedSkillDescription.SetText(SkillManager.Instance.GetSkillDescriptionText(skillInfo, level));

            if (skillInfo.SkillType == Enum_SkillType.Active)
            {
                ShowActiveCoolTimeInfo(skillInfo);
            }
            else
            {
                if (_activeSkillCoolTimeGroup != null)
                    _activeSkillCoolTimeGroup.gameObject.SetActive(false);
            }

            RefreshButtons(skillInfo, hasSkill);
        }

        private void ShowActiveCoolTimeInfo(SkillInfo activeInfo)
        {
            if (_activeSkillCoolTimeGroup != null)
                _activeSkillCoolTimeGroup.gameObject.SetActive(true);

            if (_cooldownTypeText != null)
                _cooldownTypeText.SetText(SkillManager.Instance.GetCooldownTypeText(activeInfo));

            if (_cooldownValueText != null)
                _cooldownValueText.SetText(SkillManager.Instance.GetCooldownValueText(activeInfo));
        }

        private void RefreshButtons(SkillInfo skillInfo, bool hasSkill)
        {
            if (!hasSkill)
            {
                if (_equipButton != null) _equipButton.gameObject.SetActive(false);
                if (_unequipButton != null) _unequipButton.gameObject.SetActive(false);
                if (_levelUpButton != null) _levelUpButton.gameObject.SetActive(false);
                return;
            }

            if (_levelUpButton != null)
                _levelUpButton.gameObject.SetActive(true);

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

        private void ClearSelectedSkillInfo()
        {
            _selectedSkillId = 0;

            if (_selectedSkillInfoGroup != null)
                _selectedSkillInfoGroup.gameObject.SetActive(false);

            if (_activeSkillCoolTimeGroup != null)
                _activeSkillCoolTimeGroup.gameObject.SetActive(false);
        }

        private void OnEquipButtonClicked()
        {
            if (_selectedSkillId <= 0)
                return;

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
                RefreshAll();
        }

        private void OnUnequipButtonClicked()
        {
            if (_selectedSkillId <= 0)
                return;

            int slotIndex = UserTable.Get<SkillTable>().FindSlotIndexBySkillId(_selectedSkillId);
            if (slotIndex < 0)
                return;

            if (SkillManager.Instance.UnequipSkillFromSlot(slotIndex))
                RefreshAll();
        }

        private void OnLevelUpButtonClicked()
        {
            if (_selectedSkillId <= 0)
                return;

            if (SkillManager.Instance.LevelUpSkill(_selectedSkillId))
                ShowSkillInfo(_selectedSkillId);
        }
    }
}
