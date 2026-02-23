using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UILanternAppliedEffectElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _statName;
        [SerializeField] private TMP_Text _value;

        public void Bind(int itemId, string statName, string statValue)
        {
            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(itemId);

            if (_title != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_title, ItemManager.Instance.GetItemNameLocalKey(itemId));

            if (_statName != null)
                _statName.SetText(statName);

            if (_value != null)
                _value.SetText(statValue);
        }
    }
}
