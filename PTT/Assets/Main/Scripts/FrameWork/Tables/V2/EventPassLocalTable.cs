using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using Gpm.Ui;

namespace GameBerry
{
    public class EventPassGroupData
    {
        public ObscuredInt EventPassIndex;
        public ObscuredInt EventVersion;

        public string EventPassName = string.Empty;
        public string EventPassBanner = string.Empty;

        public EventPassScheduler EventPassScheduler;

        public List<ObscuredInt> ConditionRewardGroupIndexs = new List<ObscuredInt>();

        public List<EventPassData> EventPassDatas = new List<EventPassData>();

        public Dictionary<ObscuredInt, List<EventPassConditionRewardData>> EventPassConditionRewardDatas = new Dictionary<ObscuredInt, List<EventPassConditionRewardData>>();

        public Dictionary<ObscuredInt, EventPassConditionRewardDataLevelGroup> EventPassConditionRewardDataLevelGroups = new Dictionary<ObscuredInt, EventPassConditionRewardDataLevelGroup>();

        public List<EventPassMissionData> EventPassMissionDatas = new List<EventPassMissionData>();

        public List<EventPassMissionLevelData> EventPassMissionLevelDatas = new List<EventPassMissionLevelData>();
    }

    public class EventPassScheduler
    {
        public ObscuredInt Index;

        public ObscuredInt EventPassIndex;
        public ObscuredInt EventVersion;

        public ObscuredDouble EventStartTimeStamp;
        public ObscuredDouble EventEndTimeStamp;
        public ObscuredDouble EventDisplayTimeStamp;
    }

    public class EventPassData : ShopDataBase
    {
        public ObscuredInt EventPassIndex;
        public ObscuredInt EventVersion;

        public ObscuredInt Step;

        public ObscuredInt ReturnEventMissionExp;

        public ObscuredInt ConditionRewardGroupIndex;

        public ObscuredInt DisplayRewardIndex;
    }

    public class EventPassConditionRewardData
    {
        public ObscuredInt Index;

        public ObscuredInt EventPassIndex;

        public ObscuredInt ConditionRewardGroupIndex;
        public ObscuredInt DisplayOrder;
        public ObscuredInt EventPassMissionLevel;

        public RewardData RewardData;
    }

    public class EventPassConditionRewardDataLevelGroup : InfiniteScrollData
    {
        public ObscuredInt EventPassIndex;

        public ObscuredInt Level;

        public EventPassConditionRewardData[] eventPassConditionRewardDatas;
    }

    public class EventPassMissionData
    {
        public ObscuredInt Index;

        public ObscuredInt EventPassIndex;

        public V2Enum_IntervalType MissionIntervalType;
        public ObscuredInt MissionIntervalParam;


        public V2Enum_QuestGoalType MissionType;
        public ObscuredInt MissionParam;

        public ObscuredInt ReturnEventMissionExp;
        public ObscuredBool IsPaidMission;

        public string NameLocalStringKey;
    }

    public class EventPassMissionLevelData
    {
        public ObscuredInt Index;

        public ObscuredInt EventPassIndex;

        public ObscuredInt TargetMissionLevel;
        public ObscuredInt RequiredMissionExp;
    }

    public class EventPassLocalTable : LocalTableBase
    {
        public Dictionary<ObscuredInt, EventPassGroupData> EventPassGroupDatas_Dic = new Dictionary<ObscuredInt, EventPassGroupData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;


            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventPassScheduler", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                System.DateTime startdateTime = System.DateTime.Parse(rows[i]["EventStartTime"].ToString()).ToUniversalTime();
                double startTime = ((System.DateTimeOffset)startdateTime).ToUnixTimeSeconds();

                System.DateTime enddateTime = System.DateTime.Parse(rows[i]["EventEndTime"].ToString()).ToUniversalTime();
                double endTime = ((System.DateTimeOffset)enddateTime).ToUnixTimeSeconds();

                double displayTime = endTime + rows[i]["DisplayDelaytime"].ToString().ToLong();

                if (displayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                {
                    EventPassScheduler eventPassScheduler = new EventPassScheduler();

                    eventPassScheduler.Index = rows[i]["Index"].ToString().ToInt();
                    eventPassScheduler.EventPassIndex = rows[i]["EventPassIndex"].ToString().ToInt();

                    eventPassScheduler.EventVersion = rows[i]["EventVersion"].ToString().ToInt();

                    eventPassScheduler.EventStartTimeStamp = startTime;
                    eventPassScheduler.EventEndTimeStamp = endTime;
                    eventPassScheduler.EventDisplayTimeStamp = displayTime;

                    if (EventPassGroupDatas_Dic.ContainsKey(eventPassScheduler.EventPassIndex) == false)
                    {
                        EventPassGroupData eventPassGroupData = new EventPassGroupData();

                        eventPassGroupData.EventPassIndex = eventPassScheduler.EventPassIndex;
                        eventPassGroupData.EventVersion = eventPassScheduler.EventVersion;
                        eventPassGroupData.EventPassScheduler = eventPassScheduler;

                        eventPassGroupData.EventPassName = string.Format("eventPass/{0}/groupTitle", eventPassScheduler.EventPassIndex);
                        eventPassGroupData.EventPassBanner = string.Format("eventPass/{0}/banner", eventPassScheduler.EventPassIndex);

                        EventPassGroupDatas_Dic.Add(eventPassScheduler.EventPassIndex, eventPassGroupData);
                    }
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventPass", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt EventPassIndex = rows[i]["EventPassIndex"].ToString().ToInt();

                if (EventPassGroupDatas_Dic.ContainsKey(EventPassIndex) == false)
                    continue;

                EventPassGroupData eventPassGroupData = EventPassGroupDatas_Dic[EventPassIndex];

                ObscuredInt EventVersion = rows[i]["EventVersion"].ToString().ToInt();

                if (eventPassGroupData.EventVersion != EventVersion)
                    continue;

                EventPassData eventPassData = new EventPassData();

                eventPassData.Index = rows[i]["Index"].ToString().ToInt();

                eventPassData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                eventPassData.EventPassIndex = EventPassIndex;
                eventPassData.EventVersion = EventVersion;
                
                eventPassData.Step = rows[i]["Step"].ToString().ToInt();

                eventPassData.PID = rows[i]["PID"].ToString();

                eventPassData.ReturnEventMissionExp = rows[i]["ReturnEventMissionExp"].ToString().ToInt();
                eventPassData.ConditionRewardGroupIndex = rows[i]["ConditionRewardGroupIndex"].ToString().ToInt();

                eventPassData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

                eventPassData.Description = rows[i]["Description"].ToString();

                RewardData rewardData = new RewardData();
                rewardData.V2Enum_Goods = rows[i]["ReturnGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                rewardData.Index = rows[i]["ReturnGoodsParam1"].ToString().ToInt();
                rewardData.Amount = rows[i]["ReturnGoodsParam2"].ToString().ToDouble();
                eventPassData.ShopRewardData.Add(rewardData);

                eventPassData.DisplayRewardIndex = rows[i]["DisplayRewardIndex"].ToString().ToInt();

                eventPassData.TitleLocalStringKey = string.Format("eventPass/{0}/title", eventPassData.ResourceIndex);
                eventPassData.SubTitleLocalStringKey = string.Format("eventPass/{0}/subTitle", eventPassData.ResourceIndex);
                eventPassData.MailTitleLocalStringKey = string.Format("eventPass/{0}/mailTitle", eventPassData.ResourceIndex);
                eventPassData.MailDescLocalStringKey = string.Format("eventPass/{0}/mailDesc", eventPassData.ResourceIndex);

                if (eventPassData.PID != "-1")
                    ShopOperator.AddStoreDataBase(eventPassData);

                eventPassGroupData.ConditionRewardGroupIndexs.Add(eventPassData.ConditionRewardGroupIndex);
                eventPassGroupData.EventPassDatas.Add(eventPassData);
            }

            foreach (var pair in EventPassGroupDatas_Dic)
            {
                pair.Value.ConditionRewardGroupIndexs.Reverse();
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventPassConditionReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt EventPassIndex = rows[i]["EventPassIndex"].ToString().ToInt();

                if (EventPassGroupDatas_Dic.ContainsKey(EventPassIndex) == false)
                    continue;

                EventPassGroupData eventPassGroupData = EventPassGroupDatas_Dic[EventPassIndex];

                EventPassConditionRewardData eventPassConditionRewardData = new EventPassConditionRewardData();

                eventPassConditionRewardData.Index = rows[i]["Index"].ToString().ToInt();

                eventPassConditionRewardData.EventPassIndex = EventPassIndex;

                eventPassConditionRewardData.ConditionRewardGroupIndex = rows[i]["ConditionRewardGroupIndex"].ToString().ToInt();

                eventPassConditionRewardData.DisplayOrder = rows[i]["DisplayOrder"].ToString().ToInt();
                eventPassConditionRewardData.EventPassMissionLevel = rows[i]["EventPassMissionLevel"].ToString().ToInt();

                RewardData rewardData = new RewardData();
                rewardData.V2Enum_Goods = rows[i]["RewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                rewardData.Index = rows[i]["RewardGoodsParam1"].ToString().ToInt();
                rewardData.Amount = rows[i]["RewardGoodsParam2"].ToString().ToDouble();

                eventPassConditionRewardData.RewardData = rewardData;

                if (eventPassGroupData.EventPassConditionRewardDatas.ContainsKey(eventPassConditionRewardData.ConditionRewardGroupIndex) == false)
                {
                    eventPassGroupData.EventPassConditionRewardDatas.Add(eventPassConditionRewardData.ConditionRewardGroupIndex, new List<EventPassConditionRewardData>());
                }

                eventPassGroupData.EventPassConditionRewardDatas[eventPassConditionRewardData.ConditionRewardGroupIndex].Add(eventPassConditionRewardData);

                if (eventPassGroupData.EventPassConditionRewardDataLevelGroups.ContainsKey(eventPassConditionRewardData.EventPassMissionLevel) == false)
                {
                    EventPassConditionRewardDataLevelGroup newEventPassConditionRewardDataLevelGroup = new EventPassConditionRewardDataLevelGroup();
                    newEventPassConditionRewardDataLevelGroup.EventPassIndex = eventPassConditionRewardData.EventPassIndex;
                    newEventPassConditionRewardDataLevelGroup.Level = eventPassConditionRewardData.EventPassMissionLevel;

                    newEventPassConditionRewardDataLevelGroup.eventPassConditionRewardDatas = new EventPassConditionRewardData[eventPassGroupData.ConditionRewardGroupIndexs.Count];

                    eventPassGroupData.EventPassConditionRewardDataLevelGroups.Add(eventPassConditionRewardData.EventPassMissionLevel, newEventPassConditionRewardDataLevelGroup);
                }

                EventPassConditionRewardDataLevelGroup eventPassConditionRewardDataLevelGroup = eventPassGroupData.EventPassConditionRewardDataLevelGroups[eventPassConditionRewardData.EventPassMissionLevel];
                eventPassConditionRewardDataLevelGroup.eventPassConditionRewardDatas[eventPassGroupData.ConditionRewardGroupIndexs.IndexOf(eventPassConditionRewardData.ConditionRewardGroupIndex)] = eventPassConditionRewardData;
                //eventPassConditionRewardDataLevelGroup.eventPassConditionRewardDatas.Add(eventPassConditionRewardData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventPassMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt EventPassIndex = rows[i]["EventPassIndex"].ToString().ToInt();

                if (EventPassGroupDatas_Dic.ContainsKey(EventPassIndex) == false)
                    continue;

                EventPassGroupData eventPassGroupData = EventPassGroupDatas_Dic[EventPassIndex];

                EventPassMissionData eventPassMissionData = new EventPassMissionData();

                eventPassMissionData.Index = rows[i]["Index"].ToString().ToInt();

                eventPassMissionData.EventPassIndex = EventPassIndex;

                eventPassMissionData.MissionIntervalType = rows[i]["MissionIntervalType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                eventPassMissionData.MissionIntervalParam = rows[i]["MissionIntervalParam"].ToString().ToInt();

                eventPassMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                eventPassMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                eventPassMissionData.ReturnEventMissionExp = rows[i]["ReturnEventMissionExp"].ToString().ToInt();
                eventPassMissionData.IsPaidMission = rows[i]["IsPaidMission"].ToString().ToInt() == 1;

                eventPassMissionData.NameLocalStringKey = string.Format("mission/{0}/name", eventPassMissionData.MissionType.Enum32ToInt());

                eventPassGroupData.EventPassMissionDatas.Add(eventPassMissionData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventPassMissionLevel", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt EventPassIndex = rows[i]["EventPassIndex"].ToString().ToInt();

                if (EventPassGroupDatas_Dic.ContainsKey(EventPassIndex) == false)
                    continue;

                EventPassGroupData eventPassGroupData = EventPassGroupDatas_Dic[EventPassIndex];

                EventPassMissionLevelData eventPassMissionLevelData = new EventPassMissionLevelData();

                eventPassMissionLevelData.Index = rows[i]["Index"].ToString().ToInt();

                eventPassMissionLevelData.EventPassIndex = EventPassIndex;

                eventPassMissionLevelData.TargetMissionLevel = rows[i]["TargetMissionLevel"].ToString().ToInt();
                eventPassMissionLevelData.RequiredMissionExp = rows[i]["RequiredMissionExp"].ToString().ToInt();

                eventPassGroupData.EventPassMissionLevelDatas.Add(eventPassMissionLevelData);
            }
        }
    }
}