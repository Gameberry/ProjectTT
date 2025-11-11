using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class EventRouletteData
    {
        public ObscuredInt Index;
        public ObscuredInt SlotNumber;

        public ObscuredBool IsSlotLimited;
        public ObscuredInt SlotSelectionMaxCount;

        public ObscuredDouble SlotSelectionWeight;

        //public V2Enum_Goods SlotRewardGoodsType;
        //public ObscuredInt SlotRewardGoodsParam1;
        //public ObscuredDouble SlotRewardGoodsParam2;

        public RewardData rewardData = new RewardData();

        public ObscuredBool IsResetTrigger;
    }

    public class EventRouletteDrawRewardData
    {
        public ObscuredInt Index;
        public ObscuredInt AccumedDrawCount;

        public V2Enum_Goods DrawRewardGoodsType;
        public ObscuredInt DrawRewardGoodsParam1;
        public ObscuredDouble DrawRewardGoodsParam2;
    }

    public class EventMissionData
    {
        public ObscuredInt Index;
        public V2Enum_QuestGoalType MissionType;
        public ObscuredInt MissionParam;

        public V2Enum_Goods ClearRewardType;
        public ObscuredInt ClearRewardParam1;
        public ObscuredDouble ClearRewardParam2;

        public string NameLocalStringKey;
    }

    public class RankRewardData
    {
        public ObscuredInt Index;
        public ObscuredInt RankMin;
        public ObscuredInt RankMax;

        public V2Enum_Goods RankRewardType1;
        public ObscuredInt RankRewardParam11;
        public ObscuredDouble RankRewardParam12;
    }

    public class EventRouletteLocalTable : LocalTableBase
    {
        public List<EventRouletteData> eventRouletteDatas = new List<EventRouletteData>();
        public List<EventRouletteDrawRewardData> eventRouletteDrawRewardDatas = new List<EventRouletteDrawRewardData>();
        public List<EventMissionData> eventRouletteMissionDatas = new List<EventMissionData>();
        public List<RankRewardData> eventRouletteRankRewardDatas = new List<RankRewardData>();

        public List<EventRouletteData> limitReward = new List<EventRouletteData>();
        public WeightedRandomPicker<EventRouletteData> normalRewardWeightPicker = new WeightedRandomPicker<EventRouletteData>();
        public ObscuredDouble normalRewardTotalWeight = 0;
        public ObscuredDouble rewardTotalWeight = 0;

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventRoulette", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventRouletteData eventRouletteData = new EventRouletteData();
                eventRouletteData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteData.SlotNumber = rows[i]["SlotNumber"].ToString().ToInt();

                eventRouletteData.IsSlotLimited = rows[i]["IsSlotLimited"].ToString().ToInt() == 1;
                eventRouletteData.SlotSelectionMaxCount = rows[i]["SlotSelectionMaxCount"].ToString().ToInt();

                eventRouletteData.SlotSelectionWeight = rows[i]["SlotSelectionWeight"].ToString().ToDouble();

                //eventRouletteData.SlotRewardGoodsType = rows[i]["SlotRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                //eventRouletteData.SlotRewardGoodsParam1 = rows[i]["SlotRewardGoodsParam1"].ToString().ToInt();
                //eventRouletteData.SlotRewardGoodsParam2 = rows[i]["SlotRewardGoodsParam2"].ToString().ToDouble();

                eventRouletteData.rewardData.V2Enum_Goods = rows[i]["SlotRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                eventRouletteData.rewardData.Index = rows[i]["SlotRewardGoodsParam1"].ToString().ToInt();
                eventRouletteData.rewardData.Amount = rows[i]["SlotRewardGoodsParam2"].ToString().ToDouble();

                eventRouletteData.IsResetTrigger = rows[i]["IsResetTrigger"].ToString().ToInt() == 1;

                rewardTotalWeight += eventRouletteData.SlotSelectionWeight;

                if (eventRouletteData.IsSlotLimited == false)
                {
                    normalRewardWeightPicker.Add(eventRouletteData, eventRouletteData.SlotSelectionWeight);
                    normalRewardTotalWeight += eventRouletteData.SlotSelectionWeight;
                }
                else
                    limitReward.Add(eventRouletteData);

                eventRouletteDatas.Add(eventRouletteData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventRouletteDrawReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventRouletteDrawRewardData eventRouletteDrawRewardData = new EventRouletteDrawRewardData();
                eventRouletteDrawRewardData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteDrawRewardData.AccumedDrawCount = rows[i]["AccumedDrawCount"].ToString().ToInt();

                eventRouletteDrawRewardData.DrawRewardGoodsType = rows[i]["DrawRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                eventRouletteDrawRewardData.DrawRewardGoodsParam1 = rows[i]["DrawRewardGoodsParam1"].ToString().ToInt();
                eventRouletteDrawRewardData.DrawRewardGoodsParam2 = rows[i]["DrawRewardGoodsParam2"].ToString().ToDouble();

                eventRouletteDrawRewardDatas.Add(eventRouletteDrawRewardData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventRouletteMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMissionData eventRouletteMissionData = new EventMissionData();
                eventRouletteMissionData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                eventRouletteMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                eventRouletteMissionData.ClearRewardType = V2Enum_Goods.Point;
                eventRouletteMissionData.ClearRewardParam1 = V2Enum_Point.LuckyCoin.Enum32ToInt();
                eventRouletteMissionData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();

                eventRouletteMissionData.NameLocalStringKey = string.Format("mission/{0}/name", eventRouletteMissionData.MissionType.Enum32ToInt());

                eventRouletteMissionDatas.Add(eventRouletteMissionData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventRouletteRankReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                RankRewardData eventRouletteRankRewardData = new RankRewardData();
                eventRouletteRankRewardData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteRankRewardData.RankMin = rows[i]["RankMin"].ToString().ToInt();
                eventRouletteRankRewardData.RankMax = rows[i]["RankMax"].ToString().ToInt();

                eventRouletteRankRewardData.RankRewardType1 = rows[i]["RankRewardType1"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                eventRouletteRankRewardData.RankRewardParam11 = rows[i]["RankRewardParam11"].ToString().ToInt();
                eventRouletteRankRewardData.RankRewardParam12 = rows[i]["RankRewardParam12"].ToString().ToDouble();

                eventRouletteRankRewardDatas.Add(eventRouletteRankRewardData);
            }
        }
        //------------------------------------------------------------------------------------
        public EventMissionData GetEventRouletteMissionData(int index)
        {
            if (eventRouletteMissionDatas == null)
                return null;

            return eventRouletteMissionDatas.Find(x => x.Index.GetDecrypted() == index);
        }
        //------------------------------------------------------------------------------------
    }
}