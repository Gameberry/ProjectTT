using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class CheckInManager : MonoSingleton<CheckInManager>
    {
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private Event.RefreshCheckInRewardMsg m_refreshCheckInRewardMsg = new Event.RefreshCheckInRewardMsg();

        private List<string> m_changeInfoUpdate = new List<string>();

        public V2Enum_CheckInType canCheckInType = V2Enum_CheckInType.Max;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerCheckInInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);
            CheckInOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitCheckInContent()
        {
            TimeManager.Instance.OnInitDailyContent += OnInitDailyContent;

            if (CheckInContainer.m_checkInInfo.ContainsKey(V2Enum_CheckInType.Once) == false)
            {
                CheckInContainer.m_checkInInfo.Add(V2Enum_CheckInType.Once, new CheckInInfo());
            }

            if (CheckInContainer.m_checkInInfo.ContainsKey(V2Enum_CheckInType.Repeat) == false)
            {
                CheckInContainer.m_checkInInfo.Add(V2Enum_CheckInType.Repeat, new CheckInInfo());
            }

            SetCheckInRewardCount();
        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            SetCheckInRewardCount();
        }
        //------------------------------------------------------------------------------------
        private void SetCheckInRewardCount()
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;

            foreach (KeyValuePair<V2Enum_CheckInType, CheckInInfo> pair in CheckInContainer.m_checkInInfo)
            {
                CheckInInfo checkInInfo = pair.Value;

                if (checkInInfo.NextCheckRewardTime != 0.0)
                {
                    if (checkInInfo.NextCheckRewardTime <= TimeManager.Instance.Current_TimeStamp)
                    {
                        checkInInfo.CheckInRewardCount++;
                        checkInInfo.NextCheckRewardTime = 0.0;
                        checkInInfo.NextAdCheckRewardTime = 0.0;

                        m_refreshCheckInRewardMsg.v2Enum_CheckInType = pair.Key;

                        Message.Send(m_refreshCheckInRewardMsg);
                    }
                }

                if (ReadyAdView(pair.Key) == true)
                {
                    if (canCheckInType != V2Enum_CheckInType.Once)
                        canCheckInType = pair.Key;

                    RedDotManager.Instance.ShowRedDot(ContentDetailList.CheckIn);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public CheckInInfo GetCheckInInfo(V2Enum_CheckInType v2Enum_CheckInType)
        {
            if (CheckInContainer.m_checkInInfo.ContainsKey(v2Enum_CheckInType) == false)
            {
                CheckInContainer.m_checkInInfo.Add(v2Enum_CheckInType, new CheckInInfo());
            }

            return CheckInContainer.m_checkInInfo[v2Enum_CheckInType];
        }
        //------------------------------------------------------------------------------------
        public List<CheckInRewardData> GetCheckInRewardDatas(V2Enum_CheckInType v2Enum_CheckInType)
        {
            if (v2Enum_CheckInType == V2Enum_CheckInType.Once)
                return CheckInOperator.GetCheckInRewardOnceDatas();
            else if (v2Enum_CheckInType == V2Enum_CheckInType.Repeat)
                return CheckInOperator.GetCheckInRewardRepeatDatas();

            return new List<CheckInRewardData>();
        }
        //------------------------------------------------------------------------------------
        public int GetCheckInRewardCount(V2Enum_CheckInType v2Enum_CheckInType)
        {
            if (CheckInContainer.m_checkInInfo.ContainsKey(v2Enum_CheckInType) == true)
            {
                int checkincount = CheckInContainer.m_checkInInfo[v2Enum_CheckInType].CheckInRewardCount;
                if (v2Enum_CheckInType == V2Enum_CheckInType.Repeat)
                {
                    checkincount = checkincount % GetCheckInRewardDatas(v2Enum_CheckInType).Count;

                    if (checkincount == 0)
                        checkincount = GetCheckInRewardDatas(v2Enum_CheckInType).Count;

                    return checkincount;
                }
                else
                    return checkincount;
            }

            return 0;
        }
        //------------------------------------------------------------------------------------
        public bool ReadyAdView(V2Enum_CheckInType v2Enum_CheckInType)
        {
            CheckInRewardData checkInRewardData = GetFocusCheckInReward(v2Enum_CheckInType);
            if (checkInRewardData == null)
                return false;

            CheckInInfo checkInInfo = GetCheckInInfo(v2Enum_CheckInType);

            return checkInInfo.NextAdCheckRewardTime <= TimeManager.Instance.Current_TimeStamp;
        }
        //------------------------------------------------------------------------------------
        public bool AllAlReady()
        {
            foreach (KeyValuePair<V2Enum_CheckInType, CheckInInfo> pair in CheckInContainer.m_checkInInfo)
            {
                if (ReadyAdView(pair.Key) == true)
                    return false;
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public CheckInRewardData GetFocusCheckInReward(V2Enum_CheckInType v2Enum_CheckInType)
        {
            List<CheckInRewardData> checkInRewardDatas = GetCheckInRewardDatas(v2Enum_CheckInType);
            if (checkInRewardDatas == null)
                return null;

            return checkInRewardDatas.Find(x => x.CheckInCount.GetDecrypted() == GetCheckInRewardCount(v2Enum_CheckInType));
        }
        //------------------------------------------------------------------------------------
        public bool IsFocusCheckInReward(CheckInRewardData checkInRewardData)
        {
            if (checkInRewardData == null)
                return false;

            return GetCheckInRewardCount(checkInRewardData.CheckInType) == checkInRewardData.CheckInCount.GetDecrypted();
        }
        //------------------------------------------------------------------------------------
        public bool IsAlreadyCheckInReward(CheckInRewardData checkInRewardData)
        {
            if (checkInRewardData == null)
                return true;

            return GetCheckInRewardCount(checkInRewardData.CheckInType) > checkInRewardData.CheckInCount.GetDecrypted();
        }
        //------------------------------------------------------------------------------------
        public bool IsReadyCheckInReward(CheckInRewardData checkInRewardData)
        {
            if (checkInRewardData == null)
                return false;

            if (GetCheckInRewardCount(checkInRewardData.CheckInType) == checkInRewardData.CheckInCount.GetDecrypted())
            {
                CheckInInfo checkInInfo = GetCheckInInfo(checkInRewardData.CheckInType);

                if (checkInInfo.NextCheckRewardTime == 0.0)
                {
                    return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool IsReadyAdCheckInReward(CheckInRewardData checkInRewardData)
        {
            if (checkInRewardData == null)
                return false;

            if (GetCheckInRewardCount(checkInRewardData.CheckInType) == checkInRewardData.CheckInCount.GetDecrypted())
            {
                CheckInInfo checkInInfo = GetCheckInInfo(checkInRewardData.CheckInType);

                if (checkInInfo.NextAdCheckRewardTime == 0.0)
                {
                    return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public void DoCheckInReward(CheckInRewardData checkInRewardData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (checkInRewardData == null)
                return;

            if (IsReadyCheckInReward(checkInRewardData) == false)
            { 
                if (IsReadyAdCheckInReward(checkInRewardData) == true)
                {
                    //UnityPlugins.appLovin.ShowRewardedAd(() =>
                    //{
                    //    ThirdPartyLog.Instance.SendLog_AD_ViewEvent("checkin", checkInRewardData.Index.GetDecrypted(), GameBerry.Define.IsAdFree == true ? 1 : 2);
                    //    GetAdCheckInReward(checkInRewardData);
                    //});

                    
                }
                return;
            }

            GetCheckInReward(checkInRewardData);
        }
        //------------------------------------------------------------------------------------
        private void GetCheckInReward(CheckInRewardData checkInRewardData)
        {
            if (checkInRewardData == null)
                return;

            CheckInInfo checkInInfo = GetCheckInInfo(checkInRewardData.CheckInType);
            checkInInfo.NextCheckRewardTime = TimeManager.Instance.DailyInit_TimeStamp;

            GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.CheckInRewardGet);

            PaymentsReward(checkInRewardData, false);
        }
        //------------------------------------------------------------------------------------
        private void GetAdCheckInReward(CheckInRewardData checkInRewardData)
        {
            if (checkInRewardData == null)
                return;

            CheckInInfo checkInInfo = GetCheckInInfo(checkInRewardData.CheckInType);
            checkInInfo.NextAdCheckRewardTime = TimeManager.Instance.DailyInit_TimeStamp;

            GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.CheckInRewardGet);

            PaymentsReward(checkInRewardData, true);
        }
        //------------------------------------------------------------------------------------
        private void PaymentsReward(CheckInRewardData checkInRewardData, bool isAd)
        {
            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.V2Enum_Goods = checkInRewardData.CheckInRewardGoodsType;
            rewardData.Index = checkInRewardData.CheckInRewardParam1.GetDecrypted();
            rewardData.Amount = checkInRewardData.CheckInRewardParam2.GetDecrypted();

            reward_type.Add(rewardData.Index);
            before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
            reward_quan.Add(rewardData.Amount);

            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);

            GoodsManager.Instance.AddGoodsAmount(checkInRewardData.CheckInRewardGoodsType.Enum32ToInt(), checkInRewardData.CheckInRewardParam1.GetDecrypted(), checkInRewardData.CheckInRewardParam2.GetDecrypted());

            after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

            Message.Send(m_setInGameRewardPopupMsg);
            UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

            m_refreshCheckInRewardMsg.v2Enum_CheckInType = checkInRewardData.CheckInType;
            Message.Send(m_refreshCheckInRewardMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

            CheckInInfo checkInInfo = GetCheckInInfo(checkInRewardData.CheckInType);
            if (checkInInfo != null)
            {
                checkInInfo.NextCheckRewardTime = TimeManager.Instance.DailyInit_TimeStamp;
                ThirdPartyLog.Instance.SendLog_AttendEvent(checkInRewardData.CheckInType, checkInInfo.CheckInRewardCount,
                            reward_type, before_quan, reward_quan, after_quan,
                            isAd == true ? (Define.IsAdFree == true ? 1 : 2) : 0);
            }

            if (AllAlReady() == true)
                RedDotManager.Instance.HideRedDot(ContentDetailList.CheckIn);
        }
        //------------------------------------------------------------------------------------
    }
}