using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIHPBarElement : MonoBehaviour
    {
        [SerializeField]
        protected Image HPBar;

        [SerializeField]
        protected Image HPBarShadow;

        protected float m_decreaseHpShadowRatio = 5.0f;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            Managers.UnityUpdateManager.Instance.UpdateFunc += UpdateFunc;
        }
        //------------------------------------------------------------------------------------
        public virtual void ResetHPBar()
        { 

        }
        //------------------------------------------------------------------------------------
        public virtual void SetHPRatio(float ratio)
        {
            if (HPBar != null)
            { 
                HPBar.fillAmount = ratio;

                if (HPBarShadow != null)
                {
                    if (HPBar.fillAmount > HPBarShadow.fillAmount)
                        HPBarShadow.fillAmount = HPBar.fillAmount;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void UpdateFunc()
        {
            Updated();
        }
        //------------------------------------------------------------------------------------
        protected virtual void Updated()
        {
            if (HPBar != null && HPBarShadow != null)
            {
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