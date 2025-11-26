using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UISpineSkinTabButtonElement : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private SpineEquipSlot _slot;
        [SerializeField] private SkinSelectDialog _manager;

        // 선택 상태 시 색 바꾸고 싶으면 여기서 처리
        [SerializeField] private GameObject _selectedIndicator;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (_manager == null)
                return;

            _manager.OnClickSlotTab(_slot);
        }

        public void SetSelected(bool selected)
        {
            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(selected);
        }
    }
}
