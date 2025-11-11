using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UIGambleSlotElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _slotText;

        [SerializeField]
        private Image _slotIcon;

        public void SetText(string text)
        {
            if (_slotText != null)
                _slotText.SetText(text);
        }

        public void SetIcon(Sprite sprite)
        {
            if (_slotIcon != null)
                _slotIcon.sprite = sprite;
        }
    }
}