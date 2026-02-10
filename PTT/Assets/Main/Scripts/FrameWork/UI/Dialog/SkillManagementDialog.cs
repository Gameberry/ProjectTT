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
        [SerializeField] private List<UISkillSlotElement> _skillSlotElements = new List<UISkillSlotElement>();

        [Header("Skill List")]
        [SerializeField] private Transform _skillListContent;
        [SerializeField] private UISkillElement _skillElementPrefab;
        [SerializeField] private Button _activeTabButton;
        [SerializeField] private Button _passiveTabButton;

        private readonly List<UISkillElement> _spawnedSkillElements = new List<UISkillElement>();
        private Enum_SkillType _currentTab = Enum_SkillType.Active;

        [Header("Selected Skill Info")]
        [SerializeField] private Transform _selectedSkillInfoGroup;
        [SerializeField] private Image _selectedSkillIcon;
        [SerializeField] private TMP_Text _selectedSkillName;
        [SerializeField] private TMP_Text _selectedSkillType;
        [SerializeField] private TMP_Text _selectedSkillLevel;
        [SerializeField] private TMP_Text _selectedSkillDescription;

        [Header("Active Skill Info")]
        [SerializeField] private Transform _activeSkillInfoGroup;
        [SerializeField] private TMP_Text _cooldownTypeText;
        [SerializeField] private TMP_Text _cooldownValueText;
        [SerializeField] private TMP_Text _attackMultiplierText;
        [SerializeField] private TMP_Text _hitCountText;

        [Header("Buttons")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Button _levelUpButton;

        private int _selectedSkillId = 0;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            // 스킬 슬롯 초기화
            for (int i = 0; i < _skillSlotElements.Count; i++)
            {
                _skillSlotElements[i].Init(i);
                _skillSlotElements[i].OnSlotClicked += OnSlotClicked;
            }

            // 탭 버튼
            if (_activeTabButton != null)
                _activeTabButton.onClick.AddListener(() => SetTab(Enum_SkillType.Active));

            if (_passiveTabButton != null)
                _passiveTabButton.onClick.AddListener(() => SetTab(Enum_SkillType.Passive));

            // 액션 버튼
            if (_equipButton != null)
                _equipButton.onClick.AddListener(OnEquipButtonClicked);

            if (_unequipButton != null)
                _unequipButton.onClick.AddListener(OnUnequipButtonClicked);

            if (_levelUpButton != null)
                _levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            SkillManager.Instance.OnSkillDataChanged += RefreshAll;
            SkillManager.Instance.OnSkillSlotChanged += RefreshSlots;

            SetTab(Enum_SkillType.Active);
            RefreshSlots();
            ClearSelectedSkillInfo();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.OnSkillDataChanged -= RefreshAll;
                SkillManager.Instance.OnSkillSlotChanged -= RefreshSlots;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetTab(Enum_SkillType tab)
        {
            _currentTab = tab;
            RefreshSkillList();
        }
        //------------------------------------------------------------------------------------
        private void RefreshAll()
        {
            RefreshSlots();
            RefreshSkillList();

            if (_selectedSkillId > 0)
                ShowSkillInfo(_selectedSkillId);
        }
        //------------------------------------------------------------------------------------
        private void RefreshSlots()
        {
            for (int i = 0; i < _skillSlotElements.Count; i++)
            {
                _skillSlotElements[i].RefreshSlot();
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshSkillList()
        {
            // 기존 리스트 제거
            for (int i = 0; i < _spawnedSkillElements.Count; i++)
            {
                if (_spawnedSkillElements[i] != null)
                    Destroy(_spawnedSkillElements[i].gameObject);
            }
            _spawnedSkillElements.Clear();

            if (_skillElementPrefab == null || _skillListContent == null)
                return;

            SkillChart skillChart = Chart.GameChart.Get<SkillChart>();
            if (skillChart == null)
                return;

            // 보유한 스킬 목록
            var ownedSkills = SkillManager.Instance.HasSkill(0) 
                ? UserTable.Get<SkillTable>().GetAllSkills() 
                : new Dictionary<int, SkillData>();

            // 탭에 따라 필터링
            foreach (var kvp in ownedSkills)
            {
                SkillInfo skillInfo = skillChart.Get(kvp.Key);
                if (skillInfo == null || skillInfo.SkillType != _currentTab)
                    continue;

                CreateSkillElement(kvp.Key);
            }

            // 미해금 스킬도 표시 (선택사항)
            // TODO: 해금 가능한 스킬 목록 추가
        }
        //------------------------------------------------------------------------------------
        private void CreateSkillElement(int skillId)
        {
            var element = Instantiate(_skillElementPrefab, _skillListContent);
            element.Bind(skillId, OnSkillElementClicked);
            _spawnedSkillElements.Add(element);
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
            }

            if (_selectedSkillName != null)
            {
                // TODO: 로컬라이제이션
                _selectedSkillName.SetText($"Skill {skillId}");
            }

            if (_selectedSkillType != null)
                _selectedSkillType.SetText(skillInfo.SkillType.ToString());

            bool hasSkill = SkillManager.Instance.HasSkill(skillId);

            if (_selectedSkillLevel != null)
            {
                if (hasSkill)
                {
                    int level = SkillManager.Instance.GetSkillLevel(skillId);
                    _selectedSkillLevel.SetText($"Lv.{level}");
                    _selectedSkillLevel.gameObject.SetActive(true);
                }
                else
                {
                    _selectedSkillLevel.gameObject.SetActive(false);
                }
            }

            if (_selectedSkillDescription != null)
            {
                // TODO: 스킬 설명 (ConditionData 기반)
                _selectedSkillDescription.SetText("Skill description here");
            }

            // 액티브 스킬 추가 정보
            if (skillInfo.SkillType == Enum_SkillType.Active)
            {
                SkillInfo activeInfo = skillInfo;
                ShowActiveSkillInfo(activeInfo, hasSkill);
            }
            else
            {
                if (_activeSkillInfoGroup != null)
                    _activeSkillInfoGroup.gameObject.SetActive(false);
            }

            // 버튼 상태 갱신
            RefreshButtons(skillInfo, hasSkill);
        }
        //------------------------------------------------------------------------------------
        private void ShowActiveSkillInfo(SkillInfo activeInfo, bool hasSkill)
        {
            if (_activeSkillInfoGroup != null)
                _activeSkillInfoGroup.gameObject.SetActive(true);

            if (_cooldownTypeText != null)
                _cooldownTypeText.SetText(activeInfo.CooldownType.ToString());

            if (_cooldownValueText != null)
            {
                if (activeInfo.CooldownType == Enum_SkillCooldownType.Time)
                    _cooldownValueText.SetText($"{activeInfo.CooldownValue:F1}s");
                else
                    _cooldownValueText.SetText($"{activeInfo.CooldownValue:F0} attacks");
            }

            if (_attackMultiplierText != null)
            {
                if (hasSkill)
                {
                    double finalMultiplier = SkillManager.Instance.GetSkillFinalAttackMultiplier(activeInfo.SkillId);
                    _attackMultiplierText.SetText($"{finalMultiplier:P0}");
                }
                else
                {
                    _attackMultiplierText.SetText($"{activeInfo.BaseAttackMultiplier:P0}");
                }
            }

            if (_hitCountText != null)
                _hitCountText.SetText($"{activeInfo.HitCount} hits");
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

            if (_activeSkillInfoGroup != null)
                _activeSkillInfoGroup.gameObject.SetActive(false);
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
