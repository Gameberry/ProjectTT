using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;

namespace GameBerry.Managers
{
    public class QuestManager : MonoSingleton<QuestManager>
    {
        private Dictionary<V2Enum_QuestGoalType, List<QuestData>> m_missionDatas = new Dictionary<V2Enum_QuestGoalType, List<QuestData>>();

        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private Event.RefreshQuestDataMsg _refreshQuestDataMsg = new Event.RefreshQuestDataMsg();
        private Event.RefreshQuestGaugeDataMsg _refreshQuestGaugeDataMsg = new Event.RefreshQuestGaugeDataMsg();

        private List<string> m_changeInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerQuestInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            QuestOperator.Init();

            for (int qtype = V2Enum_QuestType.Daily.Enum32ToInt(); qtype < V2Enum_QuestType.Max.Enum32ToInt(); ++qtype)
            {
                List<QuestData> missionDatas = GetQuestDatas(qtype.IntToEnum32<V2Enum_QuestType>());

                for (int i = 0; i < missionDatas.Count; ++i)
                {
                    QuestData missionData = missionDatas[i];

                    if (m_missionDatas.ContainsKey(missionData.QuestGoalType) == false)
                        m_missionDatas.Add(missionData.QuestGoalType, new List<QuestData>());

                    m_missionDatas[missionData.QuestGoalType].Add(missionData);
                }

            }
        }
        //------------------------------------------------------------------------------------
        public void InitQuestContent()
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;

            foreach (var pair in QuestContainer.QuestInfos)
            {
                QuestInfo questInfo = pair.Value;

                QuestData questData = GetMissionData(questInfo.Index);

                if (questData == null)
                    continue;
                if (questData.QuestType == V2Enum_QuestType.Achievement)
                    continue;

                if (questInfo.InitTimeStemp < currentTime)
                    ResetQuestData(questData);
            }

            foreach (var pair in QuestContainer.QuestGaugeInfos)
            {
                if (pair.Key == V2Enum_QuestType.Achievement)
                    continue;

                QuestGaugeInfo questGaugeInfo = pair.Value;

                if (questGaugeInfo.InitTimeStemp < currentTime)
                {
                    questGaugeInfo.RecvRequiredQuestCount = 0;
                    questGaugeInfo.InitTimeStemp = GetInitTime(pair.Key);
                }
            }

            if (TimeManager.Instance.Current_TimeStamp > QuestContainer.DaliyInitTimeStemp)
            {
                QuestContainer.DaliyInitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;
                AddMissionCount(V2Enum_QuestGoalType.LoginOnce, 1);
                TheBackEnd.TheBackEndManager.Instance.UpdatePlayerQuestInfoTable();
            }
            
            TimeManager.Instance.OnInitDailyContent += OnInitDailyContent;
            TimeManager.Instance.OnInitWeekContent += OnInitWeeklyContent;
            TimeManager.Instance.OnInitMonthContent += OnInitMonthlyContent;
        }
        //------------------------------------------------------------------------------------
        private double GetInitTime(V2Enum_QuestType v2Enum_QuestType)
        {
            if (v2Enum_QuestType == V2Enum_QuestType.Daily)
                return TimeManager.Instance.DailyInit_TimeStamp;
            else if (v2Enum_QuestType == V2Enum_QuestType.Weekly)
                return TimeManager.Instance.WeekInit_TimeStamp;
            else if (v2Enum_QuestType == V2Enum_QuestType.Monthly)
                return TimeManager.Instance.MonthInit_TimeStamp;

            return -1;
        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            _refreshQuestDataMsg.missionDatas.Clear();

            List<QuestData> questDatas = GetQuestDatas(V2Enum_QuestType.Daily);

            for (int i = 0; i < questDatas.Count; ++i)
            {
                QuestData questData = questDatas[i];

                if (ResetQuestData(questData))
                    _refreshQuestDataMsg.missionDatas.Add(questData.Index.GetDecrypted());
            }

            Message.Send(_refreshQuestDataMsg);

            if (QuestContainer.QuestGaugeInfos.ContainsKey(V2Enum_QuestType.Daily))
            {
                QuestGaugeInfo questGaugeInfo = QuestContainer.QuestGaugeInfos[V2Enum_QuestType.Daily];
                questGaugeInfo.RecvRequiredQuestCount = 0;
                questGaugeInfo.InitTimeStemp = nextinittimestamp;
            }

            _refreshQuestGaugeDataMsg.v2Enum_QuestType = V2Enum_QuestType.Daily;

            QuestContainer.DaliyInitTimeStemp = nextinittimestamp;

            AddMissionCount(V2Enum_QuestGoalType.LoginOnce, 1);
            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerQuestInfoTable();

            Message.Send(_refreshQuestGaugeDataMsg);
        }
        //------------------------------------------------------------------------------------
        public void OnInitWeeklyContent(double nextinittimestamp)
        {
            _refreshQuestDataMsg.missionDatas.Clear();

            List<QuestData> questDatas = GetQuestDatas(V2Enum_QuestType.Weekly);

            for (int i = 0; i < questDatas.Count; ++i)
            {
                QuestData questData = questDatas[i];

                if (ResetQuestData(questData))
                    _refreshQuestDataMsg.missionDatas.Add(questData.Index.GetDecrypted());
            }

            Message.Send(_refreshQuestDataMsg);

            if (QuestContainer.QuestGaugeInfos.ContainsKey(V2Enum_QuestType.Weekly))
            {
                QuestGaugeInfo questGaugeInfo = QuestContainer.QuestGaugeInfos[V2Enum_QuestType.Weekly];
                questGaugeInfo.RecvRequiredQuestCount = 0;
                questGaugeInfo.InitTimeStemp = nextinittimestamp;
            }

            _refreshQuestGaugeDataMsg.v2Enum_QuestType = V2Enum_QuestType.Weekly;
            Message.Send(_refreshQuestGaugeDataMsg);
        }
        //------------------------------------------------------------------------------------
        public void OnInitMonthlyContent(double nextinittimestamp)
        {
            _refreshQuestDataMsg.missionDatas.Clear();

            List<QuestData> questDatas = GetQuestDatas(V2Enum_QuestType.Monthly);

            for (int i = 0; i < questDatas.Count; ++i)
            {
                QuestData questData = questDatas[i];

                if (ResetQuestData(questData))
                    _refreshQuestDataMsg.missionDatas.Add(questData.Index.GetDecrypted());
            }

            Message.Send(_refreshQuestDataMsg);

            if (QuestContainer.QuestGaugeInfos.ContainsKey(V2Enum_QuestType.Monthly))
            {
                QuestGaugeInfo questGaugeInfo = QuestContainer.QuestGaugeInfos[V2Enum_QuestType.Monthly];
                questGaugeInfo.RecvRequiredQuestCount = 0;
                questGaugeInfo.InitTimeStemp = nextinittimestamp;
            }

            _refreshQuestGaugeDataMsg.v2Enum_QuestType = V2Enum_QuestType.Monthly;
            Message.Send(_refreshQuestGaugeDataMsg);
        }
        //------------------------------------------------------------------------------------
        private bool ResetQuestData(QuestData questData)
        {
            QuestInfo questInfo = GetMissionInfo(questData);

            if (questInfo == null)
                return false;

            questInfo.DoActionCount = 0;

            if (questData.QuestGoalType == V2Enum_QuestGoalType.LoginOnce)
                questInfo.DoActionCount = 1;

            questInfo.RecvCount = 0;
            questInfo.InitTimeStemp = GetInitTime(questData.QuestType);

            return true;
        }
        //------------------------------------------------------------------------------------
        public void RefreshSynergyRedDot()
        {
            int count = 0;

            for (int qtype = V2Enum_QuestType.Daily.Enum32ToInt(); qtype < V2Enum_QuestType.Max.Enum32ToInt(); ++qtype)
            {
                V2Enum_QuestType v2Enum_QuestType = qtype.IntToEnum32<V2Enum_QuestType>();

                if (ReadyCount(v2Enum_QuestType) > 0)
                {
                    Managers.RedDotManager.Instance.ShowRedDot(GetRedDotEnum(v2Enum_QuestType));
                    count++;
                }
            }

            if(count > 0)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Quest);
        }
        //------------------------------------------------------------------------------------
        public List<QuestData> GetQuestDatas(V2Enum_QuestType v2Enum_QuestType)
        {
            return QuestOperator.GetQuestDatas(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        public List<QuestGaugeData> GetQuestGaugeDatas(V2Enum_QuestType v2Enum_QuestType)
        {
            return QuestOperator.GetQuestGaugeDatas(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        public QuestData GetMissionData(int index)
        {
            return QuestOperator.GetMissionData(index);
        }
        //------------------------------------------------------------------------------------
        public QuestInfo GetMissionInfo(QuestData missionData)
        {
            if (missionData == null)
                return null;

            return QuestOperator.MissionDailyInfo(missionData.Index.GetDecrypted());
        }
        //------------------------------------------------------------------------------------
        public QuestGaugeInfo GetQuestGaugeInfo(V2Enum_QuestType v2Enum_QuestType)
        {
            if (QuestContainer.QuestGaugeInfos.ContainsKey(v2Enum_QuestType) == true)
                return QuestContainer.QuestGaugeInfos[v2Enum_QuestType];

            return null;
        }
        //------------------------------------------------------------------------------------
        #region Quest
        //------------------------------------------------------------------------------------
        public void AddMissionCount(V2Enum_QuestGoalType v2Enum_MissionType, int addValue)
        {
            //EventRouletteManager.Instance.AddEventRouletteMissionCount(v2Enum_MissionType, addValue);
            //EventDungeonManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);
            //AllyArenaManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);
            //EventDungeonGoddessManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);
            //EventRedBullManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);
            //EventUrsulaManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);
            //EventDungeonKingSlimeManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);
            //EventPassManager.Instance.AddEventPassMissionCount(v2Enum_MissionType, addValue);
            //RotationEventManager.Instance.AddRotationEventMissionCount(v2Enum_MissionType, addValue);
            //GuildManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);
            //SevenDayManager.Instance.AddEventDungeonMissionCount(v2Enum_MissionType, addValue);

            if (m_missionDatas.ContainsKey(v2Enum_MissionType) == false)
                return;

            List<QuestData> missionDatas = m_missionDatas[v2Enum_MissionType];

            _refreshQuestDataMsg.missionDatas.Clear();

            for (int i = 0; i < missionDatas.Count; ++i)
            {
                QuestData missionData = missionDatas[i];

                if (IsAlReadyRecvRewardMission(missionData) == true)
                    continue;

                QuestInfo missionInfo = GetMissionInfo(missionData);
                if (missionInfo == null)
                { 
                    missionInfo = AddNewMissionInfo(missionData);
                    if (missionInfo == null)
                        continue;
                }

                missionInfo.DoActionCount += addValue;

                if (missionData.QuestGoalValue.GetDecrypted() < missionInfo.DoActionCount)
                    missionInfo.DoActionCount = missionData.QuestGoalValue.GetDecrypted();

                // ·¹µå´å¿ë
                IsReadyRecvRewardMission(missionDatas[i]);

                _refreshQuestDataMsg.missionDatas.Add(missionDatas[i].Index.GetDecrypted());
            }

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerQuestInfoTable);

            Message.Send(_refreshQuestDataMsg);
        }
        //------------------------------------------------------------------------------------
        public QuestInfo AddNewMissionInfo(QuestData missionData)
        {
            if (missionData == null)
                return null;

            QuestInfo missionInfo = GetMissionInfo(missionData);
            if (missionInfo != null)
                return null;

            QuestInfo missionDailyInfo = new QuestInfo();
            missionDailyInfo.Index = missionData.Index.GetDecrypted();
            missionDailyInfo.InitTimeStemp = GetInitTime(missionData.QuestType);

            QuestContainer.QuestInfos.Add(missionDailyInfo.Index, missionDailyInfo);

            return missionDailyInfo;
        }

        //------------------------------------------------------------------------------------
        public bool CanShortcut(V2Enum_QuestGoalType v2Enum_MissionType)
        {
            //switch (v2Enum_MissionType)
            //{
            //    case V2Enum_QuestGoalType.StageChallenge:
            //    case V2Enum_QuestGoalType.SummonCount:
            //    case V2Enum_QuestGoalType.TraitChangeCount:

            //    case V2Enum_QuestGoalType.GuildJoin:
            //    case V2Enum_QuestGoalType.GuildRaidNormal:
            //    case V2Enum_QuestGoalType.GuildRaidRank:

            //    case V2Enum_QuestGoalType.GuildCheckIn:
            //    case V2Enum_QuestGoalType.GuildShopBuy:

            //    case V2Enum_QuestGoalType.LuckyRoulette:
            //    case V2Enum_QuestGoalType.TrialTower:
            //    case V2Enum_QuestGoalType.DevilCastle:

            //    case V2Enum_QuestGoalType.AllyArena:
            //    case V2Enum_QuestGoalType.ClanMission:
            //    case V2Enum_QuestGoalType.BerserkerMode:
            //        {
            //            return true;
            //        }
            //}

            return false;
        }
        //------------------------------------------------------------------------------------
        public void DoShortcutRecvMission(V2Enum_QuestGoalType v2Enum_MissionType)
        {
            //switch (v2Enum_MissionType)
            //{
            //    case V2Enum_QuestGoalType.GuildJoin:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.Guild);
            //            break;
            //        }
            //    case V2Enum_QuestGoalType.GuildRaidNormal:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.Guild_RaidEntrance_Normal);
            //            break;
            //        }
            //    case V2Enum_QuestGoalType.GuildRaidRank:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.Guild_RaidEntrance_Rank);
            //            break;
            //        }
            //    case V2Enum_QuestGoalType.GuildCheckIn:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.Guild_CheckIn);
            //            break;
            //        }
            //    case V2Enum_QuestGoalType.GuildShopBuy:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.Guild_CoinShop);
            //            break;
            //        }
            //    case V2Enum_QuestGoalType.TrialTower:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.StageMap_TrialTowerPopup);
            //            break;
            //        }
            //    case V2Enum_QuestGoalType.DevilCastle:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.StageMap_DevilCastlePopup);
            //            break;
            //        }

            //    case V2Enum_QuestGoalType.AllyArena:
            //        {
            //            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ClanAllyArena);
            //            break;
            //        }
            //}
        }
        //------------------------------------------------------------------------------------
        public string GetMissionNameLocalString(QuestData missionData)
        {
            string localstring = string.Empty;

            //switch (missionData.QuestGoalType)
            //{
            //    case V2Enum_QuestGoalType.AllDailyMissionClear:
            //        {
            //            localstring = LocalStringManager.Instance.GetLocalString(missionData.NameLocalStringKey);
            //            break;
            //        }
            //    default:
            //        {
            //            localstring = string.Format(LocalStringManager.Instance.GetLocalString(missionData.NameLocalStringKey), missionData.QuestGoalValue.GetDecrypted());
            //            break;
            //        }
            //}

            localstring = string.Format(LocalStringManager.Instance.GetLocalString(missionData.NameLocalStringKey), missionData.QuestGoalValue.GetDecrypted());

            return localstring;
        }
        //------------------------------------------------------------------------------------
        public bool CanShortcut(QuestData missionData)
        {
            return CanShortcut(missionData.QuestGoalType);
        }
        //------------------------------------------------------------------------------------
        public void DoShortcutRecvMission(QuestData missionData)
        {
            DoShortcutRecvMission(missionData.QuestGoalType);
        }
        //------------------------------------------------------------------------------------
        public int SortGambleSynergyCombineDatas(QuestData x, QuestData y)
        {
            if (IsAlReadyRecvRewardMission(x) == false && IsAlReadyRecvRewardMission(y) == true)
                return -1;
            else if (IsAlReadyRecvRewardMission(x) == true && IsAlReadyRecvRewardMission(y) == false)
                return 1;

            if (IsReadyRecvRewardMission(x) == false && IsReadyRecvRewardMission(y) == true)
                return 1;
            else if (IsReadyRecvRewardMission(x) == true && IsReadyRecvRewardMission(y) == false)
                return -1;

            if (x.Index < y.Index)
                return -1;
            else if (x.Index > y.Index)
                return 1;

            return 0;
        }
        //------------------------------------------------------------------------------------
        public ContentDetailList GetRedDotEnum(V2Enum_QuestType v2Enum_QuestType)
        {
            if (v2Enum_QuestType == V2Enum_QuestType.Daily)
            {
                return ContentDetailList.Quest_Daily;
            }
            else if (v2Enum_QuestType == V2Enum_QuestType.Weekly)
            {
                return ContentDetailList.Quest_Weekly;
            }
            else if (v2Enum_QuestType == V2Enum_QuestType.Monthly)
            {
                return ContentDetailList.Quest_Monthly;
            }
            else if (v2Enum_QuestType == V2Enum_QuestType.Achievement)
            {
                return ContentDetailList.Quest_Achievement;
            }

            return ContentDetailList.Quest;
        }
        //------------------------------------------------------------------------------------
        public int GetActionCount(QuestData missionData)
        {
            if (missionData == null)
                return 0;

            QuestInfo missionInfo = GetMissionInfo(missionData);
            if (missionInfo == null)
                return 0;

            return missionInfo.DoActionCount.GetDecrypted().ToInt();
        }
        //------------------------------------------------------------------------------------
        public bool IsAlReadyRecvRewardMission(QuestData missionData)
        {
            if (missionData == null)
                return false;

            QuestInfo missionDailyInfo = GetMissionInfo(missionData);
            if (missionDailyInfo == null)
                return false;

            return missionDailyInfo.RecvCount > 0;
        }
        //------------------------------------------------------------------------------------
        public bool IsReadyRecvRewardMission(QuestData missionData)
        {
            if (missionData == null)
                return false;

            QuestInfo missionInfo = GetMissionInfo(missionData);
            if (missionInfo == null)
                return false;

            bool ready = missionInfo.DoActionCount >= missionData.QuestGoalValue.GetDecrypted();

            if (ready == true)
            {
                QuestInfo missionDailyInfo = missionInfo;

                if (missionDailyInfo != null)
                {
                    if (missionDailyInfo.RecvCount == 0)
                    {
                        if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Quest) == true)
                        { 
                            RedDotManager.Instance.ShowRedDot(GetRedDotEnum(missionData.QuestType));
                            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Quest);
                        }
                    }
                }
            }

            return ready;
        }
        //------------------------------------------------------------------------------------
        public double GetDisplayRewardAmount(QuestData missionData)
        {
            double rewardAmount = 0.0;

            rewardAmount = missionData.ClearRewardValue.GetDecrypted();

            return rewardAmount;
        }
        //------------------------------------------------------------------------------------
        public void DoRecvMission(QuestData missionData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (IsReadyRecvRewardMission(missionData) == false)
                return;

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();


            double rewardAmount = 0.0;
            rewardAmount = missionData.ClearRewardValue.GetDecrypted();

            QuestInfo missionDailyInfo = GetMissionInfo(missionData);

            if (missionDailyInfo == null)
                return;

            missionDailyInfo.InitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;

            if (missionDailyInfo.RecvCount < 1)
            {
                missionDailyInfo.RecvCount = 1;

                if (missionData.QuestType == V2Enum_QuestType.Daily)
                {
                    AddMissionCount(V2Enum_QuestGoalType.DailyMissionClearCount, 1);
                }
                else if (missionData.QuestType == V2Enum_QuestType.Weekly)
                {
                    AddMissionCount(V2Enum_QuestGoalType.WeeklyMissionClearCount, 1);
                }

            }

            rewardAmount = missionData.ClearRewardValue.GetDecrypted();

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.V2Enum_Goods = missionData.ClearRewardType;
            rewardData.Index = missionData.ClearRewardIndex.GetDecrypted();
            
            rewardData.Amount = rewardAmount;

            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);


            reward_type.Add(rewardData.Index);
            before_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
            reward_quan.Add(rewardData.Amount);

            GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

            after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.Index));


            Message.Send(m_setInGameRewardPopupMsg);
            UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerQuestInfoTable();
            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerPointTable();

            _refreshQuestDataMsg.missionDatas.Clear();
            _refreshQuestDataMsg.missionDatas.Add(missionData.Index.GetDecrypted());

            //if (missionData.QuestType == V2Enum_QuestType.Daily)
            //    GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.DailyMissionRewardGet);

            RedDotManager.Instance.HideRedDot(GetRedDotEnum(missionData.QuestType));

            Message.Send(_refreshQuestDataMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_Quest(missionData.Index);

            ThirdPartyLog.Instance.SendLog_QuestEvent(missionData.QuestType, new List<int>() { missionData.Index.GetDecrypted() },
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public bool DoRecvAllMission(V2Enum_QuestType contentDetailList)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            List<QuestData> questDatas = GetQuestDatas(contentDetailList);
            if (questDatas == null)
                return false;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();
            List<int> idx = new List<int>();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            for (int i = 0; i < questDatas.Count; ++i)
            {
                QuestData questData = questDatas[i];

                if (IsReadyRecvRewardMission(questData) == false)
                    continue;

                QuestInfo questInfo = GetMissionInfo(questData);

                if (questInfo.RecvCount < 1)
                {
                    questInfo.RecvCount = 1;

                    if (questData.QuestType == V2Enum_QuestType.Daily)
                    {
                        AddMissionCount(V2Enum_QuestGoalType.DailyMissionClearCount, 1);
                    }
                    else if (questData.QuestType == V2Enum_QuestType.Weekly)
                    {
                        AddMissionCount(V2Enum_QuestGoalType.WeeklyMissionClearCount, 1);
                    }

                    idx.Add(questData.Index.GetDecrypted());

                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == questData.ClearRewardIndex.GetDecrypted());
                    if (rewardData == null)
                    {
                        rewardData = RewardManager.Instance.GetRewardData();
                        rewardData.V2Enum_Goods = questData.ClearRewardType;
                        rewardData.Index = questData.ClearRewardIndex.GetDecrypted();
                        rewardData.Amount = 0;
                        m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                    }

                    rewardData.Amount += questData.ClearRewardValue.GetDecrypted();

                    //rewardData.Amount += missionData.ClearRewardParam2;
                }

                ThirdPartyLog.Instance.SendLog_Quest(questData.Index);
            }

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
                {
                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);
                    after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
                }


                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
                //GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.DailyMissionRewardGet);


                ThirdPartyLog.Instance.SendLog_QuestEvent(contentDetailList, idx,
                    reward_type, before_quan, reward_quan, after_quan);

                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

                RedDotManager.Instance.HideRedDot(GetRedDotEnum(contentDetailList));
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public int ReadyCount(V2Enum_QuestType v2Enum_QuestType)
        {
            List<QuestData> questDatas = GetQuestDatas(v2Enum_QuestType);

            int readyCount = 0;

            for (int i = 0; i < questDatas.Count; ++i)
            {
                if (IsAlReadyRecvRewardMission(questDatas[i]))
                    continue;

                if (IsReadyRecvRewardMission(questDatas[i]))
                    readyCount++;
            }

            return readyCount;
        }

        //------------------------------------------------------------------------------------
        public int AlReadyCount(V2Enum_QuestType v2Enum_QuestType)
        {
            List<QuestData> questDatas = GetQuestDatas(v2Enum_QuestType);

            int alreadyCount = 0;

            for (int i = 0; i < questDatas.Count; ++i)
            {
                if (IsAlReadyRecvRewardMission(questDatas[i]))
                    alreadyCount++;
            }

            return alreadyCount;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region QuestGauge
        //------------------------------------------------------------------------------------
        public QuestGaugeInfo AddNewQuestGaugeInfo(V2Enum_QuestType v2Enum_QuestType)
        {
            QuestGaugeInfo missionInfo = GetQuestGaugeInfo(v2Enum_QuestType);
            if (missionInfo != null)
                return missionInfo;

            missionInfo = new QuestGaugeInfo();
            missionInfo.QuestType = v2Enum_QuestType;
            missionInfo.InitTimeStemp = GetInitTime(v2Enum_QuestType);

            QuestContainer.QuestGaugeInfos.Add(missionInfo.QuestType, missionInfo);

            return missionInfo;
        }
        //------------------------------------------------------------------------------------
        public int GetEventRouletteAccumCount(V2Enum_QuestType v2Enum_QuestType)
        {
            return AlReadyCount(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        public int GetRecvedOnceReward(V2Enum_QuestType v2Enum_QuestType)
        {
            QuestGaugeInfo questGaugeInfo = GetQuestGaugeInfo(v2Enum_QuestType);
            if (questGaugeInfo == null)
                return 0;

            return questGaugeInfo.RecvRequiredQuestCount;
        }
        //------------------------------------------------------------------------------------
        public void GetRecvQuestGauge(V2Enum_QuestType v2Enum_QuestType)
        {
            QuestGaugeInfo questGaugeInfo = GetQuestGaugeInfo(v2Enum_QuestType);

            if (questGaugeInfo == null)
                questGaugeInfo = AddNewQuestGaugeInfo(v2Enum_QuestType);

            _refreshQuestGaugeDataMsg.v2Enum_QuestType = v2Enum_QuestType;

            List<QuestGaugeData> eventRouletteDrawRewardDatas = GetQuestGaugeDatas(v2Enum_QuestType)
                .FindAll(x => x.RequiredQuestCount > GetRecvedOnceReward(v2Enum_QuestType)
            && x.RequiredQuestCount <= GetEventRouletteAccumCount(v2Enum_QuestType));

            if (eventRouletteDrawRewardDatas.Count <= 0)
                return;

            List<int> idx = new List<int>();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();
            List<int> count = new List<int>();

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            for (int i = 0; i < eventRouletteDrawRewardDatas.Count; ++i)
            {
                QuestGaugeData missionData = eventRouletteDrawRewardDatas[i];

                idx.Add(missionData.Index.GetDecrypted());

                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == missionData.RewardData.Index.GetDecrypted());
                if (rewardData == null)
                {
                    rewardData = RewardManager.Instance.GetRewardData();
                    rewardData.V2Enum_Goods = missionData.RewardData.V2Enum_Goods;
                    rewardData.Index = missionData.RewardData.Index.GetDecrypted();
                    rewardData.Amount = 0;
                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }

                rewardData.Amount += missionData.RewardData.Amount.GetDecrypted();

                if (questGaugeInfo.RecvRequiredQuestCount < missionData.RequiredQuestCount)
                    questGaugeInfo.RecvRequiredQuestCount = missionData.RequiredQuestCount;
            }

            ThirdPartyLog.Instance.SendLog_QuestGauge(v2Enum_QuestType, questGaugeInfo.RecvRequiredQuestCount);

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
                {
                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);
                    after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
                }

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

                ThirdPartyLog.Instance.SendLog_Quest_upperbarEvent(v2Enum_QuestType, idx,
                    reward_type, before_quan, reward_quan, after_quan);

                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
            }

            Message.Send(_refreshQuestGaugeDataMsg);
        }
        //------------------------------------------------------------------------------------
        public int GetQuestGaugeMaxCount(V2Enum_QuestType v2Enum_QuestType)
        {
            return QuestOperator.GetQuestGaugeMaxCount(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        public bool ReadyRecvDrawReward(V2Enum_QuestType v2Enum_QuestType)
        {
            List<QuestGaugeData> eventRouletteDrawRewardDatas = GetQuestGaugeDatas(v2Enum_QuestType)
                .FindAll(x => x.RequiredQuestCount > GetRecvedOnceReward(v2Enum_QuestType)
            && x.RequiredQuestCount <= GetEventRouletteAccumCount(v2Enum_QuestType));

            return eventRouletteDrawRewardDatas.Count > 0;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}