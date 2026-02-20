using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UISummonTypeTabElement : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private GameObject _lockRoot;
        [SerializeField] private GameObject _selectedRoot;
        [SerializeField] private GameObject _redDotRoot;

        private Enum_SummonType _summonType;
        private Action<Enum_SummonType> _onClick;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        public void Bind(Enum_SummonType summonType, string title, Sprite icon, bool unlocked, bool selected, bool redDot, Action<Enum_SummonType> onClick)
        {
            _summonType = summonType;
            _onClick = onClick;

            if (_title != null)
                _title.SetText(title ?? summonType.ToString());

            if (_icon != null)
                _icon.sprite = icon;

            if (_lockRoot != null)
                _lockRoot.SetActive(unlocked == false);

            if (_button != null)
                _button.interactable = unlocked;

            SetSelected(selected);
            SetRedDot(redDot);
        }

        public void SetSelected(bool selected)
        {
            if (_selectedRoot != null)
                _selectedRoot.SetActive(selected);
        }

        public void SetRedDot(bool on)
        {
            if (_redDotRoot != null)
                _redDotRoot.SetActive(on);
        }

        private void OnClick()
        {
            _onClick?.Invoke(_summonType);
        }
    }
}
