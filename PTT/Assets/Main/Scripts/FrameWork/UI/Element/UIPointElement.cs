using TMPro;
using UnityEngine;

namespace GameBerry.UI
{
    public class UIPointElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _value;

        public void Set(string name, long amount)
        {
            if (_label != null) _label.text = name;
            if (_value != null) _value.text = amount.ToString("N0");
        }
    }
}
