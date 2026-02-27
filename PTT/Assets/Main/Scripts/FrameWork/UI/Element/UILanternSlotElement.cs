using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;

namespace GameBerry.UI
{
    public class UILanternSlotElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _level;
        [SerializeField] private GameObject _occupiedRoot;
        [SerializeField] private GameObject _emptyRoot;
        [SerializeField] private GameObject _lockRoot;
        [SerializeField] private TMP_Text _lockText;
        [SerializeField] private GameObject _focusRoot;
        [SerializeField] private Button _button;

        private Enum_LanternSlotType _slotType = Enum_LanternSlotType.Main;
        private Action<Enum_LanternSlotType> _onClick;

        public Enum_LanternSlotType SlotType => _slotType;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        public void Init(Enum_LanternSlotType slotType, Action<Enum_LanternSlotType> onClick)
        {
            _slotType = slotType;
            _onClick = onClick;
        }

        public void Bind(int itemId, bool unlocked, string lockText)
        {
            if (_button != null)
                _button.interactable = unlocked;

            if (_lockRoot != null)
                _lockRoot.SetActive(unlocked == false);

            if (_lockText != null)
                _lockText.SetText(lockText ?? string.Empty);

            bool hasItem = unlocked && itemId > 0;

            if (_occupiedRoot != null)
                _occupiedRoot.SetActive(hasItem);

            if (_emptyRoot != null)
                _emptyRoot.SetActive(unlocked && hasItem == false);

            if (hasItem == false)
            {
                if (_icon != null)
                    _icon.sprite = null;
                if (_level != null)
                    _level.SetText("Lv.1");
                return;
            }

            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);
            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(itemId);

            if (_level != null)
                _level.SetText($"Lv.{LanternManager.Instance.GetLanternLevel(itemId)}");
        }

        public void SetFocus(bool focused)
        {
            if (_focusRoot != null)
                _focusRoot.SetActive(focused);
        }

        private void OnClick()
        {
            _onClick?.Invoke(_slotType);
        }
    }
}
