using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UIGuideQuestClicker : MonoBehaviour
    {
        [SerializeField]
        private Image m_hand;

        [SerializeField]
        private Image m_effect;

        //------------------------------------------------------------------------------------
        public void SetSprite(Sprite hand, Sprite effect)
        {
            if (hand != null)
            {
                if (m_hand != null)
                { 
                    m_hand.sprite = hand;
                    m_hand.gameObject.SetActive(true);
                }
            }

            if (effect != null)
            {
                if (m_effect != null)
                { 
                    m_effect.sprite = effect;
                    m_effect.gameObject.SetActive(true);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetHandAngle(float angle)
        {
            Vector3 rotate = transform.localEulerAngles;
            rotate.z = angle;

            transform.localEulerAngles = rotate;
        }
        //------------------------------------------------------------------------------------
    }
}
