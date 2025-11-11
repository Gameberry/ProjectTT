using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIDialogStatViewElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_effectText;

        [SerializeField]
        private TMP_Text m_effectStatTitle;

        [SerializeField]
        private TMP_Text m_effectStatValue;

        //------------------------------------------------------------------------------------
        public void SetStatViewElement(string equipText, string effectStatTitle, string effectStatValue)
        {
            if (m_effectText != null)
                m_effectText.text = equipText;

            if (m_effectStatTitle != null)
                m_effectStatTitle.text = effectStatTitle;

            if (m_effectStatValue != null)
                m_effectStatValue.text = effectStatValue;
        }
        //------------------------------------------------------------------------------------
        public void SetStatViewElement_Local(string equipText, string effectStatTitle, string effectStatValue)
        {
            if (m_effectText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_effectText, equipText);

            if (m_effectStatTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_effectStatTitle, effectStatTitle);

            if (m_effectStatValue != null)
                m_effectStatValue.text = effectStatValue;
        }
        //------------------------------------------------------------------------------------
    }
}