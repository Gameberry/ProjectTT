using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIIconAmountElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private TMP_Text m_amount;

        //------------------------------------------------------------------------------------
        public void SetIconAmount(Sprite sprite, double amount)
        {
            if (m_icon != null)
                m_icon.sprite = sprite;

            if (m_amount != null)
            {
                if (amount < 1.0)
                {
                    m_amount.gameObject.SetActive(false);
                }
                else
                {
                    m_amount.gameObject.SetActive(true);
                    m_amount.text = Util.GetAlphabetNumber(amount);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}