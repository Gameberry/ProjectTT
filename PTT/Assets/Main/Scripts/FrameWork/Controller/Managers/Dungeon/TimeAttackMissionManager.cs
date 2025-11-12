using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class TimeAttackMissionManager : MonoSingleton<TimeAttackMissionManager>
    {
        private Event.RefreshTimeAttackMissionMsg _refreshTimeAttackMissionMsg = new Event.RefreshTimeAttackMissionMsg();
        private Event.HideTimeAttackMissionIconMsg _hideTimeAttackMissionIconMsg = new Event.HideTimeAttackMissionIconMsg();

        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private List<string> m_changeInfoUpdate = new List<string>();

        public event OnCallBack_Double OnEndEventTime;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerTimeAttackMissionInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            TimeAttackMissionOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitTimeAttackMissionContent()
        {
            if (TimeAttackMissionContainer.FocusMission == -1)
            {
                RefreshFocusTimeMission();

                if (TimeAttackMissionContainer.FocusMission != -1)
                    TheBackEnd.TheBackEndManager.Instance.UpdatePlayerTimeAttackMissionInfoTable();
            }
            else
            {
                TimeAttackMissionData timeAttackMissionData = GetTimeAttackMissionData(TimeAttackMissionContainer.FocusMission);
                ContentOpenConditionManager.Instance.AddOpenConditionEvent(timeAttackMissionData.ClearConditionType, RefreshOpenCondition);
            }

            UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += RefreshEventRemainTime;
        }
        //------------------------------------------------------------------------------------
        private void RefreshOpenCondition(V2Enum_OpenConditionType v2Enum_OpenConditionType, int conditionValue)
        {
            if (TimeAttackMissionContainer.FocusMission == -1)
            { 
                ContentOpenConditionManager.Instance.RemoveOpenConditionEvent(v2Enum_OpenConditionType, RefreshOpenCondition);
                return;
            }

            TimeAttackMissionData timeAttackMissionData = GetTimeAttackMissionData(TimeAttackMissionContainer.FocusMission);
            if (IsRecvReady(timeAttackMissionData) == true)
                Message.Send(_refreshTimeAttackMissionMsg);
        }
        //------------------------------------------------------------------------------------
        public void RefreshEventRemainTime()
        {
            OnEndEventTime?.Invoke(GetRemainTime());
        }
        //------------------------------------------------------------------------------------
        public double GetRemainTime()
        {
            if (TimeAttackMissionContainer.FocusMission != -1)
            {
                TimeAttackMissionData timeAttackMissionData = GetTimeAttackMissionData(TimeAttackMissionContainer.FocusMission);
                TimeAttackMissionInfo timeAttackMissionInfo = GetTimeAttackMissionInfo(timeAttackMissionData);

                if (timeAttackMissionInfo.RecvCount > 0)
                    return 0;

                if(timeAttackMissionInfo != null)
                    return timeAttackMissionInfo.FinishTimeStemp - TimeManager.Instance.Current_TimeStamp;
            }

            return -1;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, TimeAttackMissionData> GetAllTimeAttackMissionData()
        {
            return TimeAttackMissionOperator.GetAllTimeAttackMissionData();
        }
        //------------------------------------------------------------------------------------
        public TimeAttackMissionData GetTimeAttackMissionData(ObscuredInt index)
        {
            return TimeAttackMissionOperator.GetTimeAttackMissionData(index);
        }
        //------------------------------------------------------------------------------------
        public TimeAttackMissionInfo GetTimeAttackMissionInfo(TimeAttackMissionData timeAttackMissionData)
        {
            if (timeAttackMissionData == null)
                return null;

            if (TimeAttackMissionContainer.TimeAttackMissionInfos.ContainsKey(timeAttackMissionData.Index) == true)
                return TimeAttackMissionContainer.TimeAttackMissionInfos[timeAttackMissionData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public TimeAttackMissionInfo AddNewMissionInfo(TimeAttackMissionData missionData)
        {
            if (missionData == null)
                return null;

            TimeAttackMissionInfo missionInfo = GetTimeAttackMissionInfo(missionData);
            if (missionInfo != null)
                return null;

            TimeAttackMissionInfo missionDailyInfo = new TimeAttackMissionInfo();
            missionDailyInfo.Index = missionData.Index.GetDecrypted();
            missionDailyInfo.FinishTimeStemp = TimeManager.Instance.Current_TimeStamp + TimeManager.Instance.GetInitAddTime(missionData.DurationType, missionData.DurationParam);

            TimeAttackMissionContainer.TimeAttackMissionInfos.Add(missionDailyInfo.Index, missionDailyInfo);

            return missionDailyInfo;
        }
        //------------------------------------------------------------------------------------
        public TimeAttackMissionData GetFocusTimeMission()
        {
            if (TimeAttackMissionContainer.FocusMission == -1)
                return null;
                
            return GetTimeAttackMissionData(TimeAttackMissionContainer.FocusMission);
            }
        //------------------------------------------------------------------------------------
        public void RefreshFocusTimeMission()
        {
            double current = TimeManager.Instance.Current_TimeStamp;

            TimeAttackMissionData prevData = GetTimeAttackMissionData(TimeAttackMissionContainer.FocusMission);

            if (TimeAttackMissionContainer.FocusMission != -1)
            {
                TimeAttackMissionData timeAttackMissionData = GetTimeAttackMissionData(TimeAttackMissionContainer.FocusMission);
                TimeAttackMissionInfo timeAttackMissionInfo = GetTimeAttackMissionInfo(timeAttackMissionData);

                if (timeAttackMissionInfo.RecvCount <= 0 && timeAttackMissionInfo.FinishTimeStemp > current)
                    return;
            }
            
            {
                TimeAttackMissionContainer.FocusMission = -1;

                foreach (var pair in TimeAttackMissionContainer.TimeAttackMissionInfos)
                {
                    TimeAttackMissionInfo timeAttackMissionInfo = pair.Value;
                    if (timeAttackMissionInfo.RecvCount <= 0 && timeAttackMissionInfo.FinishTimeStemp > current)
                    {
                        TimeAttackMissionData timeAttackMissionData = GetTimeAttackMissionData(pair.Key);

                        if (prevData != null)
                            ContentOpenConditionManager.Instance.RemoveOpenConditionEvent(prevData.ClearConditionType, RefreshOpenCondition);

                        ContentOpenConditionManager.Instance.AddOpenConditionEvent(timeAttackMissionData.ClearConditionType, RefreshOpenCondition);
                        TimeAttackMissionContainer.FocusMission = timeAttackMissionInfo.Index;
                        return;
                    }
                }

                foreach (var pair in GetAllTimeAttackMissionData())
                {
                    TimeAttackMissionInfo timeAttackMissionInfo = GetTimeAttackMissionInfo(pair.Value);
                    if (timeAttackMissionInfo != null)
                        continue;

                    if (IsPlayReady(pair.Value) == true)
                    {
                        if (IsRecvReady(pair.Value) == false)
                        {
                            AddNewMissionInfo(pair.Value);
                            TimeAttackMissionContainer.FocusMission = pair.Value.Index;

                            if (prevData != null)
                                ContentOpenConditionManager.Instance.RemoveOpenConditionEvent(prevData.ClearConditionType, RefreshOpenCondition);
                            ContentOpenConditionManager.Instance.AddOpenConditionEvent(pair.Value.ClearConditionType, RefreshOpenCondition);

                            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

                            ThirdPartyLog.Instance.SendLog_log_timeattack_mission(pair.Value.Index, 0,
                    null, null, null, null);

                            UI.UIManager.DialogEnter<UI.LobbyTimeAttackMissionDialog>();
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public bool IsPlayReady(TimeAttackMissionData timeAttackMissionData)
        {
            return ContentOpenConditionManager.Instance.IsOpen(timeAttackMissionData.OpenConditionType, timeAttackMissionData.OpenConditionParam);
        }
        //------------------------------------------------------------------------------------
        public bool IsRecvReady(TimeAttackMissionData timeAttackMissionData)
        {
            return ContentOpenConditionManager.Instance.IsOpen(timeAttackMissionData.ClearConditionType, timeAttackMissionData.ClearConditionParam);
        }
        //------------------------------------------------------------------------------------
        public bool IsRecved(TimeAttackMissionData timeAttackMissionData)
        {
            if (timeAttackMissionData == null)
                return false;

            TimeAttackMissionInfo timeAttackMissionInfo = GetTimeAttackMissionInfo(timeAttackMissionData);
            if (timeAttackMissionInfo == null)
                return false;

            return timeAttackMissionInfo.RecvCount > 0;
        }
        //------------------------------------------------------------------------------------
        public bool DoClearTimeMission(TimeAttackMissionData timeAttackMissionData)
        {
            if (timeAttackMissionData == null)
                return false;

            if (IsPlayReady(timeAttackMissionData) == false)
                return false;

            if (IsRecvReady(timeAttackMissionData) == false)
                return false;

            if (IsRecved(timeAttackMissionData) == true)
                return false;

            TimeAttackMissionInfo timeAttackMissionInfo = GetTimeAttackMissionInfo(timeAttackMissionData);
            if (timeAttackMissionInfo == null)
                return false;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            timeAttackMissionInfo.RecvCount += 1;

            for (int i = 0; i < timeAttackMissionData.ReturnGoods.Count; ++i)
            {
                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == timeAttackMissionData.ReturnGoods[i].Index);
                if (rewardData == null)
                {
                    rewardData = RewardManager.Instance.GetRewardData();
                    rewardData.V2Enum_Goods = timeAttackMissionData.ReturnGoods[i].V2Enum_Goods;
                    rewardData.Index = timeAttackMissionData.ReturnGoods[i].Index;
                    rewardData.Amount = 0;
                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }

                rewardData.Amount += timeAttackMissionData.ReturnGoods[i].Amount.GetDecrypted();
            }

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
                {
                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);
                    after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                }


                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
                //GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.DailyMissionRewardGet);

                ThirdPartyLog.Instance.SendLog_log_timeattack_mission(timeAttackMissionData.Index, 1,
                    reward_type, before_quan, reward_quan, after_quan);

                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
            }

            Message.Send(_hideTimeAttackMissionIconMsg);

            return true;
        }
        //------------------------------------------------------------------------------------
    }
}
