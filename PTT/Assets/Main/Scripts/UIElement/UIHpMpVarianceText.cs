using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace GameBerry.UI
{
    public class UIHpMpVarianceText : MonoBehaviour
    {
        [SerializeField]
        private Animator m_animator;

        [SerializeField]
        private TMP_Text m_varianceText;

        [SerializeField]
        private Image m_varianceText_Element;

        [SerializeField]
        private Transform m_criticalImg;

        [SerializeField]
        private Transform m_superCriticalImg;

        public bool isAlive = false;

        //------------------------------------------------------------------------------------
        public void ShowVarianceText(string text, VarianceColor varianceColor)
        {
            if (varianceColor == null)
            {
                if (m_varianceText != null)
                {
                    m_varianceText.text = text;

                    if (m_criticalImg != null)
                        m_criticalImg.gameObject.SetActive(false);

                    if (m_superCriticalImg != null)
                        m_superCriticalImg.gameObject.SetActive(false);
                }

                return;
            }

            if (m_varianceText != null)
            {
                m_varianceText.text = text;
                m_varianceText.fontMaterial = varianceColor.VarianceMaterial;
                m_varianceText.colorGradientPreset = varianceColor.VarianceColorGradient;
            }

            //if (m_varianceText_Element != null)
            //{
            //    if (elementicon == null)
            //        m_varianceText_Element.gameObject.SetActive(false);
            //    else
            //    {
            //        m_varianceText_Element.gameObject.SetActive(true);
            //        m_varianceText_Element.sprite = elementicon;
            //    }
            //}

            if (varianceColor.VarianceType == HpMpVarianceType.CriticalChance)
            {
                if (m_criticalImg != null)
                    m_criticalImg.gameObject.SetActive(true);

                if (m_superCriticalImg != null)
                    m_superCriticalImg.gameObject.SetActive(false);
            }
            else
            {
                if (m_criticalImg != null)
                    m_criticalImg.gameObject.SetActive(false);

                if (m_superCriticalImg != null)
                    m_superCriticalImg.gameObject.SetActive(false);
            }

            m_animator?.Rebind();

            isAlive = true;
        }
        //------------------------------------------------------------------------------------
        public void ForcePoolText()
        {
            isAlive = false;

            Managers.HPMPVarianceManager.Instance.PoolVarianceText(this, true);
        }
        //------------------------------------------------------------------------------------
        public void PoolText()
        {
            isAlive = false;

            Managers.HPMPVarianceManager.Instance.PoolVarianceText(this);
        }
        //------------------------------------------------------------------------------------
    }
}