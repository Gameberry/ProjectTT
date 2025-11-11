using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class TimeManager : MonoSingleton<TimeManager>
    {
        public ObscuredDouble Current_TimeStamp = 0.0;
        public ObscuredDouble DailyInit_TimeStamp = 0.0;
        public ObscuredDouble WeekInit_TimeStamp = 0.0;
        public ObscuredDouble MonthInit_TimeStamp = 0.0;

        public event OnCallBack_Double OnInitDailyContent;
        public event OnCallBack_Double RemainInitDailyContent;
        public string RemainInitDailyContent_String = string.Empty;
        public event OnCallBack_String RemainInitDailyContent_Text;

        public event OnCallBack_Double OnInitWeekContent;
        public event OnCallBack_Double RemainInitWeekContent;
        public string RemainInitWeekContent_String = string.Empty;
        public event OnCallBack_String RemainInitWeekContent_Text;

        public event OnCallBack_Double OnInitMonthContent;
        public event OnCallBack_Double RemainInitMonthContent;
        public string RemainInitMonthContent_String = string.Empty;
        public event OnCallBack_String RemainInitMonthContent_Text;

        private WaitForSecondsRealtime m_addTimeWaitForSecondsRealtime = new WaitForSecondsRealtime(0.1f);
        //private WaitForSeconds m_addTimeWaitForSecondsRealtime = new WaitForSeconds(0.1f);

        private Event.ReadyStageCooltimeRewardMsg m_readyStageCooltimeRewardMsg = new Event.ReadyStageCooltimeRewardMsg();
        private Event.SetRemainLuckyRouletteMsg m_setRemainLuckyRouletteMsg = new Event.SetRemainLuckyRouletteMsg();
        

        private bool m_checkCheckStageCoolTimeReward = false;
        private bool m_checkCheckLuckyRoulette = false;

        private int m_rewardMinute = 0;

        private List<RewardData> m_stageCoolTimeRewards = new List<RewardData>();

        private List<string> m_changeInfoUpdate = new List<string>();

        public System.Action ShowRouletteEventNotice;
        public System.Action ShowDungeonEventNotice;
        public System.Action ShowDungeonGoddessEventNotice;
        public System.Action ShowRedBullEventNotice;
        public System.Action ShowUrsulaEventNotice;
        public System.Action ShowDigEventNotice;
        public System.Action ShowMathRpgEventNotice;
        public System.Action ShowDungeonKingSlimeEventNotice;

        private bool m_needShowRouletteEventTimeCheck = false;
        private bool m_needShowDungeonEventTimeCheck = false;
        private bool m_needShowDungeonGoddessEventTimeCheck = false;
        private bool m_needShowRedBullEventTimeCheck = false;
        private bool m_needShowUrsulaEventTimeCheck = false;
        private bool m_needShowDigEventTimeCheck = false;
        private bool m_needShowMathRpgEventTimeCheck = false;
        private bool m_needShowDungeonKingSlimeEventTimeCheck = false;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerTimeInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);
        }
        //------------------------------------------------------------------------------------
        public IEnumerator InitTimeManager()
        {
            bool recvtiem = false;
            TheBackEnd.TheBackEndManager.Instance.GetServerTime(x =>
            {
                recvtiem = true;
                ConvertServerTime(x);
            });

            while (recvtiem == false)
            {
                yield return null;
            }
        }
        //------------------------------------------------------------------------------------
        public void InitTimeContent()
        {
            StartCoroutine(AddTimeValue());

            if (DailyInit_TimeStamp > TimeContainer.DailyInitTimeStamp)
            {
                TimeContainer.AccumLoginCount++;
                TimeContainer.DailyInitTimeStamp = DailyInit_TimeStamp;

                Debug.LogWarning(string.Format("{0}ÀÏ", TimeContainer.AccumLoginCount.GetDecrypted()));

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshServerTime()
        {
            TheBackEnd.TheBackEndManager.Instance.GetServerTime(x =>
            {
                DateTime dateTime = x;
                string checktime = dateTime.ToString();
                Debug.LogWarning(string.Format("Current Time : {0}", dateTime.ToString()));

                ObscuredDouble refreshTime = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
                Debug.Log(string.Format("RefreshTime : {0}  CurrentTime : {1}", refreshTime.GetDecrypted(), Current_TimeStamp.GetDecrypted()));

                //if (Current_TimeStamp > refreshTime + 300)
                //{
                //    TheBackEnd.TheBackEndManager.Instance.OnCheatingDetected();
                //    return;
                //}

                Current_TimeStamp = refreshTime.GetDecrypted();
            });
        }
        //------------------------------------------------------------------------------------
        public void PlayCheckStageCoolTimeReward()
        {
            if (TimeContainer.LastRecvStageCoolTimeReward == 0)
            {
                TimeContainer.LastRecvStageCoolTimeReward = Current_TimeStamp;
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
            }

            m_checkCheckStageCoolTimeReward = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayCheckLuckyRoulette()
        {
            m_checkCheckLuckyRoulette = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayRouletteEventStartTimeTrigger()
        {
            m_needShowRouletteEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayDungeonEventStartTimeTrigger()
        {
            m_needShowDungeonEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayDungeonGoddessEventStartTimeTrigger()
        {
            m_needShowDungeonGoddessEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayRedBullEventStartTimeTrigger()
        {
            m_needShowRedBullEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayUrsulaEventStartTimeTrigger()
        {
            m_needShowUrsulaEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayDigEventStartTimeTrigger()
        {
            m_needShowDigEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayMathRpgEventStartTimeTrigger()
        {
            m_needShowMathRpgEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        public void PlayDungeonKingSlimeEventStartTimeTrigger()
        {
            m_needShowDungeonKingSlimeEventTimeCheck = true;
        }
        //------------------------------------------------------------------------------------
        private void ConvertServerTime(DateTime dateTime)
        {
            string checktime = dateTime.ToString();
            Debug.LogWarning(string.Format("Current Time : {0}", dateTime.ToString()));

            Current_TimeStamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
            SetDaily_Init_TimeStamp(Current_TimeStamp);
            SetWeek_Init_TimeStamp(Current_TimeStamp);
            SetMonth_Init_TimeStamp(Current_TimeStamp);
        }
        //------------------------------------------------------------------------------------
        private void SetDaily_Init_TimeStamp(double serverTimeStamp)
        {
            double timestamp = serverTimeStamp;
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(timestamp);
            DateTime initdt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            initdt = initdt.AddDays(1.0);
            Debug.LogWarning(string.Format("DailyInit : {0}", initdt.ToString()));

            DailyInit_TimeStamp = ((DateTimeOffset)initdt).ToUnixTimeSeconds();
        }
        //------------------------------------------------------------------------------------
        private void SetWeek_Init_TimeStamp(double serverTimeStamp)
        {
            double timestamp = serverTimeStamp;
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(timestamp);
            DateTime initdt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            double addday = 7 - (initdt.DayOfWeek.Enum32ToInt() - DayOfWeek.Monday.Enum32ToInt());
            if (addday > 7)
                addday -= 7;

            initdt = initdt.AddDays(addday);
            Debug.LogWarning(string.Format("WeekInit : {0}", initdt.ToString()));

            WeekInit_TimeStamp = ((DateTimeOffset)initdt).ToUnixTimeSeconds();
        }
        //------------------------------------------------------------------------------------
        private void SetMonth_Init_TimeStamp(double serverTimeStamp)
        {
            double timestamp = serverTimeStamp;
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(timestamp);
            DateTime initdt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            initdt = initdt.AddDays(1 - initdt.Day);
            initdt = initdt.AddMonths(1);
            Debug.LogWarning(string.Format("MonthInit : {0}", initdt.ToString()));

            MonthInit_TimeStamp = ((DateTimeOffset)initdt).ToUnixTimeSeconds();
        }
        //------------------------------------------------------------------------------------
        public ObscuredDouble CheatTime = 1.0;
        private ObscuredDouble AddQeustTimeCheck = 0.0;
        //------------------------------------------------------------------------------------
        private IEnumerator AddTimeValue()
        {
            while (isAlive)
            {
                yield return m_addTimeWaitForSecondsRealtime;

                double AddSecond = 0.1;

#if DEV_DEFINE
                if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                    || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
                {
                    AddSecond = 0.1 * CheatTime;
                }
#endif

                Current_TimeStamp += AddSecond;
                TimeContainer.AccumLoginTime += AddSecond;

                AddQeustTimeCheck += AddSecond;
                if (AddQeustTimeCheck > 60)
                {
                    AddQeustTimeCheck -= 60;
                    Managers.QuestManager.Instance.AddMissionCount(V2Enum_QuestGoalType.LoginTime, 1);
                }

                if (Current_TimeStamp >= DailyInit_TimeStamp)
                { // ÇÏ·ç ÃÊ±âÈ­ ÄÁÅÙÃ÷
                    SetDaily_Init_TimeStamp(Current_TimeStamp);

                    if (OnInitDailyContent != null)
                        OnInitDailyContent(DailyInit_TimeStamp);

                    if (DailyInit_TimeStamp > TimeContainer.DailyInitTimeStamp)
                    {
                        TimeContainer.AccumLoginCount++;
                        TimeContainer.DailyInitTimeStamp = DailyInit_TimeStamp;

                        Debug.LogWarning(string.Format("{0}ÀÏ", TimeContainer.AccumLoginCount.GetDecrypted()));

                        TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

                        Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.StackLogin);
                    }
                }

                if (Current_TimeStamp >= WeekInit_TimeStamp)
                { // ÀÏÁÖÀÏ(¿ù¿äÀÏ) ÃÊ±âÈ­ ÄÁÅÙÃ÷
                    SetWeek_Init_TimeStamp(Current_TimeStamp);

                    if (OnInitWeekContent != null)
                        OnInitWeekContent(WeekInit_TimeStamp);
                }

                if (Current_TimeStamp >= MonthInit_TimeStamp)
                { // ÇÑ´Þ(¿ùÃÊ) ÃÊ±âÈ­ ÄÁÅÙÃ÷
                    SetMonth_Init_TimeStamp(Current_TimeStamp);

                    if (OnInitMonthContent != null)
                        OnInitMonthContent(MonthInit_TimeStamp);
                }

                RemainInitDailyContent?.Invoke(DailyInit_TimeStamp - Current_TimeStamp);
                
                RemainInitWeekContent?.Invoke(WeekInit_TimeStamp - Current_TimeStamp);

                RemainInitMonthContent?.Invoke(MonthInit_TimeStamp - Current_TimeStamp);

                {
                    TimeSpan dateTime = new TimeSpan(0, 0, (int)(DailyInit_TimeStamp - Current_TimeStamp));

                    RemainInitDailyContent_String = string.Format("{0} {1} {2}"
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds)
                        );

                    RemainInitDailyContent_Text?.Invoke(RemainInitDailyContent_String);
                }

                {
                    TimeSpan dateTime = new TimeSpan(0, 0, (int)(WeekInit_TimeStamp - Current_TimeStamp));

                    RemainInitWeekContent_String = string.Format("{0} {1} {2} {3}"
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), dateTime.Days)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds)
                        );

                    RemainInitWeekContent_Text?.Invoke(RemainInitWeekContent_String);
                }


                {
                    TimeSpan dateTime = new TimeSpan(0, 0, (int)(MonthInit_TimeStamp - Current_TimeStamp));

                    RemainInitMonthContent_String = string.Format("{0} {1} {2} {3}"
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), dateTime.Days)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds)
                        );

                    RemainInitMonthContent_Text?.Invoke(RemainInitMonthContent_String);
                }


                if (m_checkCheckStageCoolTimeReward == true)
                {
                    if (Current_TimeStamp - TimeContainer.LastRecvStageCoolTimeReward >= Define.StageCoolTimeRewardTimeGab)
                    {
                        m_checkCheckStageCoolTimeReward = false;
                        Message.Send(m_readyStageCooltimeRewardMsg);
                    }
                }


                if (m_checkCheckLuckyRoulette == true)
                {
                    if (Current_TimeStamp >= PlayerDataContainer.LastRouletteActionTime + Define.LuckyRouletteSpinInterval)
                    {
                        m_checkCheckLuckyRoulette = false;
                        Managers.PlayerDataManager.Instance.SendReadyLuckyRouletteMsg();
                    }
                    else
                    {
                        Message.Send(m_setRemainLuckyRouletteMsg);
                    }
                }


                if (m_needShowRouletteEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventRouletteStartTime)
                    {
                        ShowRouletteEventNotice?.Invoke();
                        m_needShowRouletteEventTimeCheck = false;
                    }
                }

                if (m_needShowDungeonEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventDungeonStartTime)
                    {
                        ShowDungeonEventNotice?.Invoke();
                        m_needShowDungeonEventTimeCheck = false;
                    }
                }

                if (m_needShowDungeonGoddessEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventHealDungeonStartTime)
                    {
                        ShowDungeonGoddessEventNotice?.Invoke();
                        m_needShowDungeonGoddessEventTimeCheck = false;
                    }
                }

                if (m_needShowRedBullEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventRedBullStartTime)
                    {
                        ShowRedBullEventNotice?.Invoke();
                        m_needShowRedBullEventTimeCheck = false;
                    }
                }

                if (m_needShowUrsulaEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventUrsulaStartTime)
                    {
                        ShowUrsulaEventNotice?.Invoke();
                        m_needShowUrsulaEventTimeCheck = false;
                    }
                }

                if (m_needShowDigEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventDigStartTime)
                    {
                        ShowDigEventNotice?.Invoke();
                        m_needShowDigEventTimeCheck = false;
                    }
                }

                if (m_needShowMathRpgEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventMathRpgStartTime)
                    {
                        ShowMathRpgEventNotice?.Invoke();
                        m_needShowMathRpgEventTimeCheck = false;
                    }
                }

                if (m_needShowDungeonKingSlimeEventTimeCheck == true)
                {
                    if (Current_TimeStamp > Define.EventHealDungeonStartTime)
                    {
                        ShowDungeonKingSlimeEventNotice?.Invoke();
                        m_needShowDungeonKingSlimeEventTimeCheck = false;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public double GetIdleRewardTime_Second()
        {
            double currentIdleTime = Current_TimeStamp - TimeContainer.LastRecvStageCoolTimeReward;

            if (currentIdleTime > Define.StageCooltimeRewardMaxSecond)
                currentIdleTime = Define.StageCooltimeRewardMaxSecond;

            return currentIdleTime;
        }
        //------------------------------------------------------------------------------------
        public int GetIdleRewardTime_Minute()
        {
            double currentIdleTime = Current_TimeStamp - TimeContainer.LastRecvStageCoolTimeReward;

            if (currentIdleTime > Define.StageCooltimeRewardMaxSecond)
                currentIdleTime = Define.StageCooltimeRewardMaxSecond;

            return (int)Math.Truncate(currentIdleTime / 60);
        }
        //------------------------------------------------------------------------------------
        public void DoIdleReward()
        {
        //    double currentIdleTime = Current_TimeStamp - TimeContainer.LastRecvStageCoolTimeReward;

        //    if (currentIdleTime < Define.StageCoolTimeRewardTimeGab)
        //        return;

        //    if (currentIdleTime > Define.StageCooltimeRewardMaxSecond)
        //        currentIdleTime = Define.StageCooltimeRewardMaxSecond;

        //    m_rewardMinute = (int)Math.Truncate(currentIdleTime / 60);

        //    while (m_stageCoolTimeRewards.Count > 0)
        //    {
        //        RewardData rewardData = m_stageCoolTimeRewards[0];
        //        RewardManager.Instance.PoolRewardData(rewardData);
        //        m_stageCoolTimeRewards.Remove(rewardData);
        //    }

        //    int rewardloopcount = (int)Math.Truncate(currentIdleTime / Define.StageCoolTimeRewardTimeGab);


        //    MapRewardData mapRewardData = MapManager.Instance.GetMaxClearMapRewardData();

        //    List<int> reward_type = new List<int>();
        //    List<double> before_quan = new List<double>();
        //    List<double> reward_quan = new List<double>();
        //    List<double> after_quan = new List<double>();

        //    Dictionary<int, RewardData> coolreward = new Dictionary<int, RewardData>();

        //    if (mapRewardData == null)
        //    {
        //        RewardData rewardData = null;
        //        if (coolreward.ContainsKey(V2Enum_Point.Gold.Enum32ToInt()) == true)
        //            rewardData = coolreward[V2Enum_Point.Gold.Enum32ToInt()];
        //        else
        //        {
        //            rewardData = RewardManager.Instance.GetRewardData();
        //            rewardData.V2Enum_Goods = V2Enum_Goods.Point;
        //            rewardData.Index = V2Enum_Point.Dia.Enum32ToInt();
        //            rewardData.Amount = 0;

        //            coolreward.Add(rewardData.Index, rewardData);
        //        }

        //        rewardData.Amount += Define.DefaultGainGold* rewardloopcount;
        //    }
        //    else
        //    {
        //        for (int i = 0; i < rewardloopcount; ++i)
        //        {
        //            RewardData pickRewardData = mapRewardData.WeightedRandomPicker.Pick();

        //            RewardData rewardData = null;
        //            if (coolreward.ContainsKey(pickRewardData.Index) == true)
        //                rewardData = coolreward[pickRewardData.Index];
        //            else
        //            {
        //                rewardData = RewardManager.Instance.GetRewardData();
        //                rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(pickRewardData.Index);
        //                rewardData.Index = pickRewardData.Index;
        //                rewardData.Amount = 0;

        //                coolreward.Add(rewardData.Index, rewardData);
        //            }

        //            rewardData.Amount += pickRewardData.Amount;
        //        }
        //    }

        //    foreach (var pair in coolreward)
        //    {
        //        RewardData rewardData = pair.Value;

        //        reward_type.Add(rewardData.Index);
        //        before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
        //        reward_quan.Add(rewardData.Amount);

        //        GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

        //        after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

        //        m_stageCoolTimeRewards.Add(rewardData);
        //    }


















        //    //int currentstage = DungeonDataManager.Instance.GetMaxClearFarmStageStep();

        //    //StageCooltimeRewardData stageCooltimeRewardData = DungeonDataOperator.GetStageCooltimeRewardData(currentstage);

        //    //if (stageCooltimeRewardData == null)
        //    //    return;



            
        //    //for (int i = 0; i < stageCooltimeRewardData.StageCooltimeRewardElementDatas.Count; ++i)
        //    //{
        //    //    StageCooltimeRewardElementData stageCooltimeRewardElementData = stageCooltimeRewardData.StageCooltimeRewardElementDatas[i];

        //    //    int selectcount = 0;

        //    //    for (int j = 0; j < rewardloopcount; ++j)
        //    //    {
        //    //        int selectweight = UnityEngine.Random.Range(0, 10000);
        //    //        if (selectweight < stageCooltimeRewardElementData.Chance)
        //    //            selectcount++;
        //    //    }

        //    //    if (selectcount > 0)
        //    //    {
        //    //        double rewardamount = stageCooltimeRewardElementData.Amount;
        //    //        rewardamount = rewardamount * selectcount;
        //    //        rewardamount += rewardamount;
        //    //        rewardamount = Math.Floor(rewardamount);

        //    //        RewardData rewardData = RewardManager.Instance.GetRewardData();
        //    //        rewardData.V2Enum_Goods = stageCooltimeRewardElementData.GoodsType;
        //    //        rewardData.Index = stageCooltimeRewardElementData.GoodsIndex;
        //    //        rewardData.Amount = rewardamount;

        //    //        reward_type.Add(rewardData.Index);
        //    //        before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
        //    //        reward_quan.Add(rewardData.Amount);

        //    //        GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

        //    //        after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

        //    //        m_stageCoolTimeRewards.Add(rewardData);
        //    //    }
        //    //}

        //    //GuideQuestManager.Instance.AddEventCount(V2Enum_EventType.CooltimeRewardClaim, 1);

        //    TimeContainer.LastRecvStageCoolTimeReward = Current_TimeStamp;

        //    TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

        //    ThirdPartyLog.Instance.SendLog_OfflineEvent(m_rewardMinute,
        //                    reward_type, before_quan, reward_quan, after_quan);

        //    ThirdPartyLog.Instance.SendLog_Acc_Reward(MapContainer.MapLastEnter);

        //    UI.IDialog.RequestDialogEnter<UI.StageCooltimeRewardDialog>();

        //    m_checkCheckStageCoolTimeReward = true;
        }
        //------------------------------------------------------------------------------------
        public List<RewardData> GetStageCoolTimeRewardDatas()
        {
            return m_stageCoolTimeRewards;
        }
        //------------------------------------------------------------------------------------
        public int GetStageCoolTimeRewardMinute()
        {
            return m_rewardMinute;
        }
        //------------------------------------------------------------------------------------
        public void DoAdStageCoolTimeReward()
        {
            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            while (m_stageCoolTimeRewards.Count > 0)
            {
                RewardData rewardData = m_stageCoolTimeRewards[0];

                reward_type.Add(rewardData.Index);
                before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                reward_quan.Add(rewardData.Amount);

                GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                RewardManager.Instance.PoolRewardData(rewardData);
                m_stageCoolTimeRewards.Remove(rewardData);
            }

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerPointTable();

            ThirdPartyLog.Instance.SendLog_Offline_AdEvent(m_rewardMinute,
                                        reward_type, before_quan, reward_quan, after_quan,
                                        Define.IsAdFree == true ? 1 : 2);
        }
        //------------------------------------------------------------------------------------
        public void ReleaseStageCoolTimeReward()
        {
            while (m_stageCoolTimeRewards.Count > 0)
            {
                RewardData rewardData = m_stageCoolTimeRewards[0];

                RewardManager.Instance.PoolRewardData(rewardData);
                m_stageCoolTimeRewards.Remove(rewardData);
            }
        }
        //------------------------------------------------------------------------------------
        public double GetInitTime(V2Enum_IntervalType v2Enum_IntervalType)
        {
            switch (v2Enum_IntervalType)
            {
                case V2Enum_IntervalType.Quarter:
                    {
                        return Current_TimeStamp + (60.0 * 15.0);
                    }
                case V2Enum_IntervalType.Hour:
                    {
                        return Current_TimeStamp + (60.0 * 60.0);
                    }
                case V2Enum_IntervalType.Day:
                    {
                        return DailyInit_TimeStamp;
                    }
                case V2Enum_IntervalType.Week:
                    {
                        return WeekInit_TimeStamp;
                    }
                case V2Enum_IntervalType.Month:
                    {
                        return MonthInit_TimeStamp;
                    }
                default:
                    {
                        return 0.0;
                    }
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public double GetInitAddTime(V2Enum_IntervalType v2Enum_IntervalType, int cycle = 1)
        {
            switch (v2Enum_IntervalType)
            {
                case V2Enum_IntervalType.Quarter:
                    {
                        return 60.0 * (15.0 * cycle);
                    }
                case V2Enum_IntervalType.Hour:
                    {
                        return 60.0 * (60.0 * cycle);
                    }
                case V2Enum_IntervalType.Day:
                    {
                        return 86400.0 * cycle;
                    }
                case V2Enum_IntervalType.Week:
                    {
                        return 86400.0 * (7.0 * cycle);
                    }
                case V2Enum_IntervalType.Month:
                    {
                        return 86400.0 * 7.0 * (30.0 * cycle);
                    }
                default:
                    {
                        Debug.LogError(string.Format("{0} Is Not Support Type!", v2Enum_IntervalType));
                        return 0.0;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        public void AddInitEvent(V2Enum_IntervalType v2Enum_IntervalType, OnCallBack_Double action)
        {
            switch (v2Enum_IntervalType)
            {
                case V2Enum_IntervalType.Day:
                    {
                        OnInitDailyContent += action;
                        break;
                    }
                case V2Enum_IntervalType.Week:
                    {
                        OnInitWeekContent += action;
                        break;
                    }
                case V2Enum_IntervalType.Month:
                    {
                        OnInitMonthContent += action;
                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetCheat_FullStageCoolTimeReward()
        {
            TimeContainer.LastRecvStageCoolTimeReward = Current_TimeStamp - Define.StageCooltimeRewardMaxSecond;
        }
        //------------------------------------------------------------------------------------
        public string GetSecendToDayString_HMS(int rawSecond)
        {
            int remainSecond = rawSecond % 60;

            int rawMinute = rawSecond / 60;

            int remainMinute = rawMinute % 60;

            int remainHour = rawMinute / 60;

            return string.Format("{0} {1} {2}"
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/hour"), remainHour)
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/minute"), remainMinute)
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/second"), remainSecond)
                );
        }
        //------------------------------------------------------------------------------------
        public string GetSecendToDayString_MS(int rawSecond)
        {
            int remainSecond = rawSecond % 60;

            int rawMinute = rawSecond / 60;

            int remainMinute = rawMinute;

            return string.Format("{0} {1}"
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/minute"), remainMinute)
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/second"), remainSecond)
                );
        }
        //------------------------------------------------------------------------------------
        public int GetDayCount()
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(Current_TimeStamp);

            //Debug.Log(string.Format("JoinTime {0}   CurrentTime {1}", TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted(), dt));

            //TimeSpan timeSpan = dt - TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted();

            //int dayCount = dt.Day - TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Day;

            //Debug.Log(string.Format("Count : {0}, {1} - {2}", dayCount, dt.Day, TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Day));

            TimeSpan dayCount = dt.Date - TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Date;

            //Debug.Log(string.Format("Count : {0}, {1} - {2}", dayCount.Days, dt.Date, TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Date));

            return dayCount.Days;
        }
        //------------------------------------------------------------------------------------
    }
}