using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;

namespace GameBerry.UI
{
    /// <summary>
    /// 각인 스테이지 패널 Element
    /// 스테이지 번호, 잠금 상태, 슬롯 표시, 각인/확률 버튼
    /// </summary>
    public class UIEngravingStageElement : MonoBehaviour
    {
        [Header("Stage Info")]
        [SerializeField] private TMP_Text _stageNumberText;
        [SerializeField] private Image _stageLockIcon;

        [Header("Slot Display")]
        [SerializeField] private UIEngravingSlotElement _slotElement;

        [Header("Buttons")]
        [SerializeField] private Button _engraveButton;
        [SerializeField] private TMP_Text _engraveButtonText;
        [SerializeField] private Button _probabilityButton;

        private int _stageNumber;
        private System.Action<int> _onEngraveCallback;
        private System.Action<int> _onProbabilityCallback;

        //------------------------------------------------------------------------------------
        public void Init(int stage, System.Action<int> onEngrave, System.Action<int> onProbability)
        {
            _stageNumber = stage;
            _onEngraveCallback = onEngrave;
            _onProbabilityCallback = onProbability;

            if (_stageNumberText != null)
                _stageNumberText.SetText("{0}", stage);

            if (_engraveButton != null)
                _engraveButton.onClick.AddListener(OnEngraveClicked);

            if (_probabilityButton != null)
                _probabilityButton.onClick.AddListener(OnProbabilityClicked);
        }
        //------------------------------------------------------------------------------------
        public void UpdatePanel(EngravingStageData stageData)
        {
            if (stageData == null)
                return;

            bool isUnlocked = stageData.isUnlocked;

            // 슬롯 업데이트
            if (_slotElement != null && isUnlocked)
                _slotElement.UpdateSlots(stageData);

            // 잠금 아이콘
            if (_stageLockIcon != null)
                _stageLockIcon.gameObject.SetActive(!isUnlocked);

            // 각인 버튼
            if (_engraveButton != null)
            {
                _engraveButton.interactable = isUnlocked;

                if (_engraveButtonText != null)
                {
                    string textKey = isUnlocked ? "btn_engrave" : "btn_locked";
                    Managers.LocalStringManager.Instance.SetLocalizeText(_engraveButtonText, textKey);
                }
            }

            // 확률 버튼
            if (_probabilityButton != null)
                _probabilityButton.gameObject.SetActive(isUnlocked);
        }
        //------------------------------------------------------------------------------------
        private void OnEngraveClicked()
        {
            _onEngraveCallback?.Invoke(_stageNumber);
        }
        //------------------------------------------------------------------------------------
        private void OnProbabilityClicked()
        {
            _onProbabilityCallback?.Invoke(_stageNumber);
        }
        //------------------------------------------------------------------------------------
    }
}
