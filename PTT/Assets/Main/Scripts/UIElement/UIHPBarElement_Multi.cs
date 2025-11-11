using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIHPBarElement_Multi : UIHPBarElement
    {
        [SerializeField]
        private Image SubHPBar;

        [SerializeField]
        private List<Color> ColorHPBar;

        private int m_currentColorIndex = 0;

        private int m_currentHPBarCount = 0;
        private float m_currentHPRatio = 1.0f;

        private int m_targetHPBarCount = 0;
        private float m_targetHPRatio = 1.0f;

        private float m_countGabTime = 0.5f;

        private float m_gabStartRatio = 0.0f;
        private float m_gabStartTime = 0.0f;
        private float m_gabTime = 0.0f;
        private bool m_playGabDirection = false;

        //------------------------------------------------------------------------------------
        public override void ResetHPBar()
        {
            m_currentColorIndex = 0;

            m_currentHPBarCount = 0;
            m_currentHPRatio = 1.0f;

            m_targetHPBarCount = 0;
            m_targetHPRatio = 1.0f;

            if (HPBar != null)
            { 
                HPBar.fillAmount = m_currentHPRatio;
                HPBar.color = GetCurrentColor();
            }

            if (SubHPBar != null)
            {
                SubHPBar.color = GetNextColor();
            }

            if (HPBarShadow != null)
                HPBarShadow.fillAmount = m_currentHPRatio;
        }
        //------------------------------------------------------------------------------------
        public void SetHPRatio(int HpBarCount, float Ratio)
        {
            m_targetHPBarCount = HpBarCount;
            m_targetHPRatio = Ratio;
        }
        //------------------------------------------------------------------------------------
        private void SetCurrentColorIndex_Next()
        {
            int next = m_currentColorIndex + 1;

            if (ColorHPBar.Count <= next)
                next = 0;

            m_currentColorIndex = next;
        }
        //------------------------------------------------------------------------------------
        private int GetNextColorIndex()
        {
            int next = m_currentColorIndex + 1;

            if (ColorHPBar.Count <= next)
                next = 0;

            return next;
        }
        //------------------------------------------------------------------------------------
        private Color GetCurrentColor()
        {
            if (m_currentColorIndex >= 0
                && ColorHPBar.Count > m_currentColorIndex)
                return ColorHPBar[m_currentColorIndex];

            return Color.white;
        }
        //------------------------------------------------------------------------------------
        private Color GetNextColor()
        {
            if (GetNextColorIndex() >= 0
                && ColorHPBar.Count > GetNextColorIndex())
                return ColorHPBar[GetNextColorIndex()];

            return Color.white;
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
            if (m_currentHPBarCount < m_targetHPBarCount)
            {
                if (m_playGabDirection == false)
                {
                    m_playGabDirection = true;
                    int gabCount = m_targetHPBarCount - m_currentHPBarCount;

                    m_gabStartRatio = HPBarShadow.fillAmount;

                    m_gabTime = m_countGabTime / (float)gabCount;
                    m_gabTime *= m_gabStartRatio;

                    m_gabStartTime = m_gabTime + Time.time;

                    HPBar.fillAmount = 0.0f;
                }

                if (m_gabStartTime > Time.time)
                {
                    float ratio = (m_gabStartTime - Time.time) / m_gabTime;

                    HPBarShadow.fillAmount = m_gabStartRatio * ratio;

                    return;
                }
                else
                {
                    SetCurrentColorIndex_Next();

                    if (HPBar != null)
                        HPBar.color = GetCurrentColor();

                    if (SubHPBar != null)
                        SubHPBar.color = GetNextColor();

                    if (HPBarShadow != null)
                        HPBarShadow.fillAmount = 1.0f;

                    m_currentHPBarCount++;
                    if (m_currentHPBarCount == m_targetHPBarCount)
                    {
                        m_playGabDirection = false;

                        HPBarShadow.fillAmount = 1.0f;
                        HPBar.fillAmount = m_targetHPRatio;
                    }
                    else
                    {
                        int gabCount = m_targetHPBarCount - m_currentHPBarCount;

                        m_gabStartRatio = 1.0f;

                        m_gabTime = m_countGabTime / (float)gabCount;
                        m_gabTime *= m_gabStartRatio;

                        m_gabStartTime = m_gabTime + Time.time;

                        HPBar.fillAmount = 0.0f;

                        return;
                    }
                }
            }

            if (HPBar != null && HPBarShadow != null)
            {
                HPBar.fillAmount = m_targetHPRatio;

                if (HPBar.fillAmount >= HPBarShadow.fillAmount)
                {
                    float size = HPBarShadow.fillAmount;
                    size = HPBar.fillAmount;

                    HPBarShadow.fillAmount = size;
                }
                else
                {
                    float size = HPBarShadow.fillAmount - HPBar.fillAmount;
                    size *= m_decreaseHpShadowRatio * Time.deltaTime;

                    HPBarShadow.fillAmount -= size;
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}