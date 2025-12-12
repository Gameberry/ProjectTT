using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISpineSkinButtonElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        private SkinSlotType _slot;
        private string _skinName;
        private SkeletonAnimationHandler _handler;

        /// <summary>
        /// UI 초기화 (외부에서 셋업)
        /// </summary>
        public void Init(SkeletonAnimationHandler handler,
                         SkinSlotType slot,
                         string skinName,
                         string displayName)
        {
            _handler = handler;
            _slot = slot;
            _skinName = skinName;

            if (_label != null)
                _label.text = displayName;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (_handler == null)
                return;

            if (string.IsNullOrEmpty(_skinName))
            {
                // “없음” 버튼일 경우 → 슬롯 해제
                //_handler.UnequipSlotSkin(_slot);
                Managers.SkinManager.Instance.UnequipSlotSkin(_slot);
            }
            else
            {
                // 스킨 장착
                //_handler.EquipSlotSkin(_slot, _skinName);
                Managers.SkinManager.Instance.EquipSlotSkin(_slot, _skinName);
            }
        }
    }
}
