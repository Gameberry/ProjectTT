using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UICheckInElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_dayText;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_checkInReward;

        [SerializeField]
        private Transform m_isFocusParticle;

        [SerializeField]
        private Image m_adIcon;

        [SerializeField]
        private Transform m_isAlreadyReward;

        private CheckInRewardData m_currentCheckInRewardData;

        //-----------------------------------------------------------------------------------
        public void SetCheckInElement(CheckInRewardData checkInRewardData)
        {
            if (m_dayText != null)
                m_dayText.text = string.Format("Day{0}", checkInRewardData.CheckInCount.GetDecrypted());

            m_checkInReward.SetRewardElement(
                checkInRewardData.CheckInRewardGoodsType,
                checkInRewardData.CheckInRewardParam1.GetDecrypted(),
                Managers.GoodsManager.Instance.GetGoodsSprite(checkInRewardData.CheckInRewardGoodsType.Enum32ToInt(), checkInRewardData.CheckInRewardParam1.GetDecrypted()),
                Managers.GoodsManager.Instance.GetGoodsGrade(checkInRewardData.CheckInRewardGoodsType.Enum32ToInt(), checkInRewardData.CheckInRewardParam1.GetDecrypted()),
                checkInRewardData.CheckInRewardParam2.GetDecrypted());


            m_currentCheckInRewardData = checkInRewardData;
        }
        //-----------------------------------------------------------------------------------
        public void RefreshCheckInElement()
        {
            if (m_currentCheckInRewardData == null)
                return;

            bool isAlready = Managers.CheckInManager.Instance.IsAlreadyCheckInReward(m_currentCheckInRewardData);
            if (isAlready == true)
            {
                if (m_isFocusParticle != null)
                    m_isFocusParticle.gameObject.SetActive(false);

                if (m_adIcon != null)
                    m_adIcon.gameObject.SetActive(false);

                if (m_isAlreadyReward != null)
                    m_isAlreadyReward.gameObject.SetActive(true);
            }
            else
            {
                if (m_isAlreadyReward != null)
                    m_isAlreadyReward.gameObject.SetActive(false);

                bool isfocus = Managers.CheckInManager.Instance.IsFocusCheckInReward(m_currentCheckInRewardData);

                if (isfocus == false)
                {
                    if (m_isFocusParticle != null)
                        m_isFocusParticle.gameObject.SetActive(false);

                    if (m_adIcon != null)
                        m_adIcon.gameObject.SetActive(false);
                }
                else
                {
                    bool isReady = Managers.CheckInManager.Instance.IsReadyCheckInReward(m_currentCheckInRewardData);
                    bool isAdReady = Managers.CheckInManager.Instance.IsReadyAdCheckInReward(m_currentCheckInRewardData);

                    if (isReady == false && isAdReady == false)
                    {
                        if (m_isFocusParticle != null)
                            m_isFocusParticle.gameObject.SetActive(false);

                        if (m_adIcon != null)
                            m_adIcon.gameObject.SetActive(false);

                        if (m_isAlreadyReward != null)
                            m_isAlreadyReward.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (m_isFocusParticle != null)
                        {
                            bool showparticle = isReady == true || isAdReady == true;

                            m_isFocusParticle.gameObject.SetActive(showparticle);
                        }

                        if (m_adIcon != null)
                            m_adIcon.gameObject.SetActive(isReady == false && isAdReady == true);
                    }
                }
            }
        }
        //-----------------------------------------------------------------------------------
    }
}