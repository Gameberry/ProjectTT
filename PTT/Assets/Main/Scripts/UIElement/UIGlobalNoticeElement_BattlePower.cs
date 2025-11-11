using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GameBerry.UI
{
    public class UIGlobalNoticeElement_BattlePower : UIGlobalNoticeElement
    {

        [SerializeField]
        private Color m_InCreaseColor;

        [SerializeField]
        private Color m_DeCreaseColor;

        [SerializeField]
        private TMP_Text m_ChangeValue;

        [SerializeField]
        private WaitForSeconds m_turm = new WaitForSeconds(1.0f);

        //------------------------------------------------------------------------------------
        public void ShowNoticeElement_BattlePower(double battlePower, double changeValue)
        {
            if (m_noticeMessage != null)
                m_noticeMessage.text = Util.GetAlphabetNumber(battlePower);

            if (m_ChangeValue != null)
            {
                if (changeValue < 0)
                {
                    m_ChangeValue.color = m_DeCreaseColor;
                    m_ChangeValue.text = string.Format("({0})", Util.GetAlphabetNumber(changeValue));
                }
                else
                {
                    m_ChangeValue.color = m_InCreaseColor;
                    m_ChangeValue.text = string.Format("(+{0})", Util.GetAlphabetNumber(changeValue));
                }
            }

            if (m_messageCanvasGroup != null)
                m_messageCanvasGroup.alpha = 1.0f;

            gameObject.SetActive(true);

            if (HideDirectionCoroutine != null)
                StopCoroutine(HideDirectionCoroutine);

            HideDirectionCoroutine = StartCoroutine(HideDirection(m_turm));
        }
        //------------------------------------------------------------------------------------
    }
}