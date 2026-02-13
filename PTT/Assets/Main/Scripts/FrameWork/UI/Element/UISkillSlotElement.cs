using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;

namespace GameBerry.UI
{
    /// <summary>
    /// 스킬 슬롯 UI 요소 (통합 버전)
    /// CharacterControllerBase에 통합된 스킬 시스템 참조
    /// </summary>
    public class UISkillSlotElement : MonoBehaviour
    {
        [SerializeField] private int _slotIndex = 0;

        [SerializeField] private Image _slotBackground;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private Image _cooldownOverlay; // 쿨타임 진행 표시용
        [SerializeField] private TMP_Text _cooldownText;  // 남은 시간/횟수 표시
        
        [SerializeField] private Transform _emptySlotIndicator; // 빈 슬롯 표시
        [SerializeField] private Image _selectedFrame;
        [SerializeField] private Button _button;

        public event System.Action<int> OnSlotClicked; // slotIndex 전달

        private int _currentSkillId = 0;
        private bool _isSelected = false;

        // 플레이어 참조 (CharacterControllerBase에서 직접 스킬 메서드 호출)
        private CharacterControllerBase _player;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            // 쿨타임 업데이트
            if (_currentSkillId > 0)
                UpdateCooldown();
        }
        //------------------------------------------------------------------------------------
        public void Init(int slotIndex)
        {
            _slotIndex = slotIndex;
            SetSelected(false);
            RefreshSlot();
        }
        //------------------------------------------------------------------------------------
        public void RefreshSlot()
        {
            _player = Managers.BattleSceneManager.Instance?.GetPlayer();
            int skillId = SkillManager.Instance.GetEquippedSkillId(_slotIndex);
            _currentSkillId = skillId;
            
            if (skillId <= 0)
            {
                ShowEmptySlot();
                return;
            }

            SkillInfo skillInfo = Chart.GameChart.Get<SkillChart>()?.GetActive(skillId);
            if (skillInfo == null)
            {
                ShowEmptySlot();
                return;
            }

            ShowSkill(skillInfo);
        }
        //------------------------------------------------------------------------------------
        private void ShowEmptySlot()
        {
            if (_emptySlotIndicator != null)
                _emptySlotIndicator.gameObject.SetActive(true);

            if (_skillIcon != null)
                _skillIcon.gameObject.SetActive(false);

            if (_cooldownOverlay != null)
                _cooldownOverlay.gameObject.SetActive(false);

            if (_cooldownText != null)
                _cooldownText.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void ShowSkill(SkillInfo skillInfo)
        {
            if (_emptySlotIndicator != null)
                _emptySlotIndicator.gameObject.SetActive(false);

            if (_skillIcon != null)
            {
                _skillIcon.gameObject.SetActive(true);
                _skillIcon.sprite = SkillManager.Instance.GetIcon(skillInfo.SkillId);
            }

            UpdateCooldown();
        }
        //------------------------------------------------------------------------------------
        private void UpdateCooldown()
        {
            if (_currentSkillId <= 0 || _player == null)
                return;

            // CharacterControllerBase의 메서드 직접 호출!
            bool canUse = _player.CanUseSkill(_currentSkillId);

            if (canUse)
            {
                // 쿨타임 완료
                if (_cooldownOverlay != null)
                    _cooldownOverlay.gameObject.SetActive(false);

                if (_cooldownText != null)
                    _cooldownText.gameObject.SetActive(false);
            }
            else
            {
                // 쿨타임 진행 중
                SkillInfo skillInfo = Chart.GameChart.Get<SkillChart>()?.GetActive(_currentSkillId);
                if (skillInfo == null)
                    return;

                if (_cooldownOverlay != null)
                    _cooldownOverlay.gameObject.SetActive(true);

                if (_cooldownText != null)
                {
                    _cooldownText.gameObject.SetActive(true);

                    if (skillInfo.CooldownType == Enum_SkillCooldownType.Time)
                    {
                        // CharacterControllerBase의 메서드 호출
                        float remaining = _player.GetRemainingSkillCooldownTime(_currentSkillId);
                        _cooldownText.SetText(string.Format("{0:0.0}s", remaining));

                        // 쿨타임 진행도 (fillAmount)
                        if (_cooldownOverlay != null && _cooldownOverlay.type == Image.Type.Filled)
                        {
                            float progress = (remaining / skillInfo.CooldownValue);
                            _cooldownOverlay.fillAmount = Mathf.Clamp01(progress);
                        }
                    }
                    else if (skillInfo.CooldownType == Enum_SkillCooldownType.AttackCount)
                    {
                        // CharacterControllerBase의 메서드 호출
                        int remaining = _player.GetRemainingSkillCooldownAttackCount(_currentSkillId);
                        _cooldownText.SetText("{0}", remaining);

                        // 쿨타임 진행도
                        if (_cooldownOverlay != null && _cooldownOverlay.type == Image.Type.Filled)
                        {
                            float progress = ((float)remaining / skillInfo.CooldownValue);
                            _cooldownOverlay.fillAmount = Mathf.Clamp01(progress);
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (_selectedFrame != null)
                _selectedFrame.gameObject.SetActive(selected);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            OnSlotClicked?.Invoke(_slotIndex);
        }
        //------------------------------------------------------------------------------------
        public int SlotIndex => _slotIndex;
        public int CurrentSkillId => _currentSkillId;
        //------------------------------------------------------------------------------------
    }
}
