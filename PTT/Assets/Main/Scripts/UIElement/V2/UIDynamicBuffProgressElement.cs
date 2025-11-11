using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIDynamicBuffProgressElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private TMP_Text m_bufftime;

        [SerializeField]
        private Image m_buffApplyGauge;


        [SerializeField]
        private TMP_Text m_buffstack;



        //------------------------------------------------------------------------------------
        public void SetBuffProgressElement(Sprite icon, float remainTime, float applyTime, int stack)
        {
            if (m_icon != null)
                m_icon.sprite = icon;

            if (applyTime > 0.0f)
            {
                if (m_bufftime != null)
                {
                    m_bufftime.gameObject.SetActive(true);
                    m_bufftime.text = string.Format("{0}", (int)remainTime);
                }

                if (m_buffApplyGauge != null)
                {
                    m_buffApplyGauge.gameObject.SetActive(true);

                    float ratio = remainTime / applyTime;
                    m_buffApplyGauge.fillAmount = 1.0f - ratio;
                }
            }
            else
            {
                if (m_bufftime != null)
                    m_bufftime.gameObject.SetActive(false);

                if (m_buffApplyGauge != null)
                    m_buffApplyGauge.gameObject.SetActive(false);
            }

            if (stack > 0)
            {
                if (m_buffstack != null)
                {
                    m_buffstack.gameObject.SetActive(true);
                    m_buffstack.text = string.Format("x{0}", stack);
                }
            }
            else
            {
                if (m_buffstack != null)
                    m_buffstack.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
    }
}