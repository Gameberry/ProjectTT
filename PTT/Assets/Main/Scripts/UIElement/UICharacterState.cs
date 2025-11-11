using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry
{
    public class UICharacterState : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer m_charHPBer;

        [SerializeField]
        private SpriteRenderer m_charHPShadowBer;

        [SerializeField]
        private SpriteRenderer m_charShieldBer;

        [SerializeField]
        private Transform m_charCoolTime_Group;

        [SerializeField]
        private SpriteRenderer m_charCoolTime;

        private float m_hpDefaultWidth;
        private float m_mpDefaultWidth;

        private float m_decreaseRatio = 5.0f;
        //private Coroutine m_hpDecreaseDirection = null;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            Managers.UnityUpdateManager.Instance.UpdateFunc += UpdateFunc;

            if (m_charHPBer != null)
                m_hpDefaultWidth = m_charHPBer.size.x;

            if (m_charCoolTime != null)
                m_mpDefaultWidth = m_charCoolTime.size.x;
        }
        //------------------------------------------------------------------------------------
        public void UpdateFunc()
        {
            if (m_charHPBer != null && m_charHPShadowBer != null)
            {
                if (m_charHPBer.size.x >= m_charHPShadowBer.size.x)
                {
                    Vector2 size = m_charHPShadowBer.size;
                    size.x = m_charHPBer.size.x;

                    m_charHPShadowBer.size = size;
                }
                else
                {
                    Vector2 size = m_charHPShadowBer.size - m_charHPBer.size;
                    if (size.x < 0.04)
                    {
                        m_charHPShadowBer.size = m_charHPBer.size;
                    }
                    else
                    {
                        size *= m_decreaseRatio * Time.deltaTime;

                        m_charHPShadowBer.size -= size;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetHPBar(double ratio)
        {
            if (m_charHPBer == null)
                return;

            Vector2 size = m_charHPBer.size;

            if (ratio <= 0.0f)
            {
                size.x = 0.0f;
                m_charHPBer.size = size;
            }
            else if(ratio >= 1.0f)
            {
                size.x = m_hpDefaultWidth;

                m_charHPBer.size = size;

                if (m_charHPShadowBer != null)
                    m_charHPShadowBer.size = size;
            }
            else
            {
                size.x = m_hpDefaultWidth * (float)ratio;
                m_charHPBer.size = size;
            }
        }
        //------------------------------------------------------------------------------------
        public void SetShieldBar(double ratio)
        {
            if (m_charShieldBer == null)
                return;

            Vector2 size = m_charShieldBer.size;

            if (ratio <= 0.0f)
            {
                size.x = 0.0f;
                m_charShieldBer.size = size;
            }
            else if (ratio >= 1.0f)
            {
                size.x = m_hpDefaultWidth;

                m_charShieldBer.size = size;

                if (m_charHPShadowBer != null)
                    m_charHPShadowBer.size = size;
            }
            else
            {
                size.x = m_hpDefaultWidth * (float)ratio;
                m_charShieldBer.size = size;
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableCoolTimeBar(bool enable)
        {
            if (m_charCoolTime_Group != null)
                m_charCoolTime_Group.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void SetCoolTimeBar(double ratio)
        {
            if (m_charCoolTime == null)
                return;

            Vector2 size = m_charCoolTime.size;

            if (ratio <= 0.0f)
            {
                size.x = 0.0f;
                m_charCoolTime.size = size;
            }
            else if (ratio >= 1.0f)
            {
                size.x = m_mpDefaultWidth;

                m_charCoolTime.size = size;
            }
            else
            {
                size.x = m_mpDefaultWidth * (float)ratio;
                m_charCoolTime.size = size;
            }
        }
        //------------------------------------------------------------------------------------
        public void SetBarColor(Color color)
        {
            if (m_charHPBer != null)
                m_charHPBer.color = color;
        }
        //------------------------------------------------------------------------------------
    }
}
