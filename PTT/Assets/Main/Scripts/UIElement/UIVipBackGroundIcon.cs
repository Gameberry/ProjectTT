using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIVipBackGroundIcon : MonoBehaviour
    {
        [SerializeField]
        private Image m_buffIcon;

        [SerializeField]
        private TMP_Text m_buffRemainTime;

        [SerializeField]
        private int _vipIndex;

        [SerializeField]
        private Button _showVipInfo;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += RefreshAdBuffState;

            if (_showVipInfo != null)
                _showVipInfo.onClick.AddListener(() =>
                {
                    V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(_vipIndex);
                    if (v2Enum_Goods != V2Enum_Goods.Max)
                        Contents.GlobalContent.ShowGoodsDescPopup(v2Enum_Goods, _vipIndex);
                });
        }
        //------------------------------------------------------------------------------------
        private void SetBuffIcon(VipPackageData adbuffstatedata)
        {
            if (m_buffIcon != null)
            {
                m_buffIcon.sprite = Managers.VipPackageManager.Instance.GetRelicIcon(adbuffstatedata);
            }

            if (m_buffRemainTime != null)
            {
                if (adbuffstatedata.DurationType == V2Enum_IntervalType.Account)
                { 
                    m_buffRemainTime.gameObject.SetActive(false);
                    return;
                }

                m_buffRemainTime.gameObject.SetActive(true);
                int remainSecond = (int)(Managers.VipPackageManager.Instance.GetPackageEndTime(adbuffstatedata) - Managers.TimeManager.Instance.Current_TimeStamp);

                int remainMinute = remainSecond / 60;

                if (remainMinute < 1)
                    m_buffRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), remainSecond);
                else
                {
                    int remainHour = remainMinute / 60;
                    if (remainHour < 1)
                        m_buffRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), remainMinute);
                    else
                    {
                        int remainDay = remainHour / 24;
                        if (remainDay < 1)
                            m_buffRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), remainHour);
                        else
                            m_buffRemainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), remainDay);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAdBuffState()
        {
            VipPackageData adBuffActiveData = Managers.VipPackageManager.Instance.GetVipPackageData(_vipIndex);

            if (adBuffActiveData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (Managers.VipPackageManager.Instance.IsActivePackage(adBuffActiveData) == false)
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