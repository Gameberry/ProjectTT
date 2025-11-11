using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class LobbyTimeAttackMissionDialog : IDialog
    {
        [SerializeField]
        private CButton _recvBtn;

        [SerializeField]
        private TMP_Text m_missionTitle;

        [SerializeField]
        private TMP_Text _timeAttackMission_RemainTime;

        [SerializeField]
        private Color _timeAttackMission_RemainTime_1HUnder;

        [SerializeField]
        private Color _timeAttackMission_RemainTime_1HOver;

        [SerializeField]
        private Transform _rewardRoot;

        [SerializeField]
        private Transform _isRecved;

        private List<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new List<UIGlobalGoodsRewardIconElement>();



        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_recvBtn != null)
                _recvBtn.onClick.AddListener(OnClick_RecvTimeMission);

            Managers.TimeAttackMissionManager.Instance.OnEndEventTime += RefreshTimeAttackMissionRemainTime;
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshFocusMission();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[0];

                if (Managers.RewardManager.isAlive == true)
                    Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
                m_uIGlobalGoodsRewardIconElements.Remove(uIGlobalGoodsRewardIconElement);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshFocusMission()
        {
            TimeAttackMissionData timeAttackMissionData = Managers.TimeAttackMissionManager.Instance.GetFocusTimeMission();

            if (timeAttackMissionData == null)
                return;

            if (m_missionTitle != null)
                m_missionTitle.text = Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(timeAttackMissionData.ClearConditionType, timeAttackMissionData.ClearConditionParam);


            List<RewardData> rewardDatas = timeAttackMissionData.ReturnGoods;

            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[0];

                if (Managers.RewardManager.isAlive == true)
                    Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
                m_uIGlobalGoodsRewardIconElements.Remove(uIGlobalGoodsRewardIconElement);
            }

            for (int i = 0; i < rewardDatas.Count; ++i)
            {
                RewardData rewardData = rewardDatas[i];
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon_NoneParticle();
                if (uIGlobalGoodsRewardIconElement == null)
                    break;

                uIGlobalGoodsRewardIconElement.transform.SetParent(_rewardRoot);
                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);

                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);

                m_uIGlobalGoodsRewardIconElements.Add(uIGlobalGoodsRewardIconElement);
            }

            bool isrecved = Managers.TimeAttackMissionManager.Instance.IsRecved(timeAttackMissionData);
            if (_isRecved != null)
            { 
                _isRecved.gameObject.SetActive(isrecved);
                _isRecved.transform.SetAsLastSibling();
            }

            if (_recvBtn != null)
            {
                if (isrecved == false)
                    _recvBtn.SetInteractable(Managers.TimeAttackMissionManager.Instance.IsRecvReady(timeAttackMissionData));
                else
                    _recvBtn.SetInteractable(false);
            }

            if (timeAttackMissionData != null)
                RefreshTimeAttackMissionRemainTime(Managers.TimeAttackMissionManager.Instance.GetRemainTime());
        }
        //------------------------------------------------------------------------------------
        private void RefreshTimeAttackMissionRemainTime(double timestamp)
        {
            if (_timeAttackMission_RemainTime != null)
            {
                System.TimeSpan dateTime = new System.TimeSpan(0, 0, (int)timestamp);
                string day = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), dateTime.Days);
                string hour = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours);
                string minute = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes);
                string second = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds);

                _timeAttackMission_RemainTime.text = string.Format("{0} {1} {2} {3}", day, hour, minute, second);

            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RecvTimeMission()
        {
            TimeAttackMissionData timeAttackMissionData = Managers.TimeAttackMissionManager.Instance.GetFocusTimeMission();

            Managers.TimeAttackMissionManager.Instance.DoClearTimeMission(timeAttackMissionData);

            RefreshFocusMission();
        }
        //------------------------------------------------------------------------------------
    }
}