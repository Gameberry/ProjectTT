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
    public class SevenDayGroup
    {
        public ObscuredInt Day;

        public SevenDayPassData sevenDayPassData;

        public List<SevenDayMissionData> sevenDayMissionDatas = new List<SevenDayMissionData>();
    }

    public class SevenDayPassData : ShopDataBase
    {
        public ObscuredInt Day;
    }

    public class SevenDayMissionData : InfiniteScrollData
    {
        public ObscuredInt Index;

        public ObscuredInt ResourceIndex;

        public ObscuredInt Day;

        public V2Enum_QuestGoalType MissionType;
        public ObscuredInt MissionParam;

        public RewardData FreeReward = new RewardData();

        public RewardData PaidReward = new RewardData();

        public string NameLocalStringKey;
    }

    public class SevenDayRewardOnceData
    {
        public ObscuredInt Index;

        public ObscuredInt MissionCount;

        public RewardData RewardData = new RewardData();
    }

    public class SevenDayLocalTable : LocalTableBase
    {
        public Dictionary<ObscuredInt, SevenDayGroup> EventPassGroupDatas_Dic = new Dictionary<ObscuredInt, SevenDayGroup>();
        public Dictionary<ObscuredInt, SevenDayPassData> SevenDayPassData_Dic = new Dictionary<ObscuredInt, SevenDayPassData>();
        public List<SevenDayMissionData> m_missionDatas = new List<SevenDayMissionData>();
        public List<SevenDayRewardOnceData> goddessDungeonCountRewardOnceDatas = new List<SevenDayRewardOnceData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SevenDayPass", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt day = rows[i]["Day"].ToString().ToInt();

                if (EventPassGroupDatas_Dic.ContainsKey(day) == false)
                {
                    SevenDayGroup newSevenDayGroup = new SevenDayGroup();
                    newSevenDayGroup.Day = day;
                    EventPassGroupDatas_Dic.Add(day, newSevenDayGroup);
                }

                SevenDayGroup sevenDayGroup = EventPassGroupDatas_Dic[day];

                SevenDayPassData sevenDayPassData = new SevenDayPassData();

                sevenDayPassData.Index = rows[i]["Index"].ToString().ToInt();

                sevenDayPassData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                sevenDayPassData.Day = day;

                sevenDayPassData.PID = rows[i]["PID"].ToString();

                sevenDayPassData.PriceKR = rows[i]["PriceKR"].ToString().ToInt();

                sevenDayPassData.Description = rows[i]["Description"].ToString();

                RewardData rewardData = new RewardData();
                rewardData.V2Enum_Goods = rows[i]["ReturnGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                rewardData.Index = rows[i]["ReturnGoodsParam1"].ToString().ToInt();
                rewardData.Amount = rows[i]["ReturnGoodsParam2"].ToString().ToDouble();
                sevenDayPassData.ShopRewardData.Add(rewardData);

                sevenDayPassData.TitleLocalStringKey = string.Format("sevenday/{0}/title", sevenDayPassData.ResourceIndex);
                sevenDayPassData.SubTitleLocalStringKey = string.Format("sevenday/{0}/subTitle", sevenDayPassData.ResourceIndex);
                sevenDayPassData.MailTitleLocalStringKey = string.Format("sevenday/{0}/mailTitle", sevenDayPassData.ResourceIndex);
                sevenDayPassData.MailDescLocalStringKey = string.Format("sevenday/{0}/mailDesc", sevenDayPassData.ResourceIndex);

                ShopOperator.AddStoreDataBase(sevenDayPassData);

                sevenDayGroup.sevenDayPassData = sevenDayPassData;

                if (SevenDayPassData_Dic.ContainsKey(sevenDayPassData.Index) == false)
                    SevenDayPassData_Dic.Add(sevenDayPassData.Index, sevenDayPassData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SevenDayMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt day = rows[i]["Day"].ToString().ToInt();

                if (EventPassGroupDatas_Dic.ContainsKey(day) == false)
                    continue;

                SevenDayGroup eventPassGroupData = EventPassGroupDatas_Dic[day];

                SevenDayMissionData sevenDayMissionData = new SevenDayMissionData();

                sevenDayMissionData.Index = rows[i]["Index"].ToString().ToInt();

                sevenDayMissionData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                sevenDayMissionData.Day = day;

                sevenDayMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                sevenDayMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                RewardData freeRewardData = new RewardData();
                freeRewardData.V2Enum_Goods = rows[i]["FreeRewardType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                freeRewardData.Index = rows[i]["FreeRewardParam1"].ToString().ToInt();
                freeRewardData.Amount = rows[i]["FreeRewardParam2"].ToString().ToDouble();
                sevenDayMissionData.FreeReward = freeRewardData;


                RewardData paidRewardData = new RewardData();
                paidRewardData.V2Enum_Goods = rows[i]["PaidRewardType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                paidRewardData.Index = rows[i]["PaidRewardParam1"].ToString().ToInt();
                paidRewardData.Amount = rows[i]["PaidRewardParam2"].ToString().ToDouble();
                sevenDayMissionData.PaidReward = paidRewardData;

                sevenDayMissionData.NameLocalStringKey = string.Format("mission/{0}/name", sevenDayMissionData.MissionType.Enum32ToInt());

                eventPassGroupData.sevenDayMissionDatas.Add(sevenDayMissionData);
                m_missionDatas.Add(sevenDayMissionData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SevenDayRewardOnce", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SevenDayRewardOnceData goddessDungeonCountRewardOnceData = new SevenDayRewardOnceData();
                goddessDungeonCountRewardOnceData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                goddessDungeonCountRewardOnceData.MissionCount = rows[i]["MissionCount"].ToString().ToInt();

                RewardData rewardData = new RewardData();
                rewardData.V2Enum_Goods = rows[i]["ClearRewardType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                rewardData.Index = rows[i]["ClearRewardParam1"].ToString().ToInt();
                rewardData.Amount = rows[i]["ClearRewardParam2"].ToString().ToDouble();
                goddessDungeonCountRewardOnceData.RewardData = rewardData;

                goddessDungeonCountRewardOnceDatas.Add(goddessDungeonCountRewardOnceData);
            }
        }
        //------------------------------------------------------------------------------------
        public SevenDayGroup GetSevenDayGroup(ObscuredInt day)
        {
            if (EventPassGroupDatas_Dic.ContainsKey(day) == false)
                return null;

            return EventPassGroupDatas_Dic[day];
        }
        //------------------------------------------------------------------------------------
        public SevenDayPassData GetSevenDayPassData(ObscuredInt index)
        {
            if (SevenDayPassData_Dic.ContainsKey(index) == false)
                return null;

            return SevenDayPassData_Dic[index];
        }
        //------------------------------------------------------------------------------------
        public List<SevenDayMissionData> GetMissionDatas()
        {
            return m_missionDatas;
        }
        //------------------------------------------------------------------------------------
    }
}