using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class UIReinforceViewElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _gambleReinforce;

        public void SetText(string text, Color color)
        {
            if (_gambleReinforce != null)
            {
                _gambleReinforce.SetText(text);
                _gambleReinforce.color = color;
            }
        }
    }
}