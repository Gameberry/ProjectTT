using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIAdBuffBackGroundIcon : MonoBehaviour
    {
        [SerializeField]
        private Image m_buffIcon;

        [SerializeField]
        private TMP_Text m_buffRemainTime;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += RefreshAdBuffState;
        }
        //------------------------------------------------------------------------------------
        private void SetBuffIcon(AdBuffActiveData adbuffstatedata)
        {
            if (m_buffIcon != null)
            {
                m_buffIcon.sprite = Managers.AdBuffManager.Instance.GetSprite(adbuffstatedata);
            }

            if (m_buffRemainTime != null)
            {
                m_buffRemainTime.gameObject.SetActive(true);
                int remainSecond = (int)(Managers.AdBuffManager.Instance.GetBuffEndTime(adbuffstatedata) - Managers.TimeManager.Instance.Current_TimeStamp);

                int remainMinute = remainSecond / 60;

                if (remainMinute < 1)
                    m_buffRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), remainSecond);
                else
                    m_buffRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), remainMinute);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAdBuffState()
        {
            AdBuffActiveData adBuffActiveData = Managers.AdBuffManager.Instance.GetCurrentActiveBuffData();

            if (adBuffActiveData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            SetBuffIcon(adBuffActiveData);
            gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
    }
}