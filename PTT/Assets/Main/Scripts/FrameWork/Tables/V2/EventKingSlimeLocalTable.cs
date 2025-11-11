using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class EventKingSlimeLocalTable : LocalTableBase
    {
        public List<GoddessDungeonData> kingSlimeDungeonDatas = new List<GoddessDungeonData>();

        public List<GoddessDungeonClearRewardData> kingSlimeDungeonClearRewardDatas = new List<GoddessDungeonClearRewardData>();

        public List<RankRewardData> eventKingSlimeRankRewardDatas = new List<RankRewardData>();

        public List<GoddessDungeonCountRewardOnceData> kingSlimeDungeonCountRewardOnceDatas = new List<GoddessDungeonCountRewardOnceData>();

        public List<EventMissionData> eventKingSlimeMissionDatas = new List<EventMissionData>();

        public List<GoddessSummonData> kingSlimeSummonDatas = new List<GoddessSummonData>();

        public WeightedRandomPicker<GoddessSummonData> WeightedRandomPicker_Summon = new WeightedRandomPicker<GoddessSummonData>();

        public GoddessSummonData SummonPickData;

        public double totalSummonWeight = 0.0;

        public List<GoddessPassData> kingSlimePassDatas = new List<GoddessPassData>();

        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("KingSlimeDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessDungeonData kingSlimeDungeonData = new GoddessDungeonData();

                kingSlimeDungeonData.Index = rows[i]["Index"].ToString().ToInt();
                kingSlimeDungeonData.PhaseNumber = rows[i]["PhaseNumber"].ToString().ToInt();

                kingSlimeDungeonData.PhaseElementType = V2Enum_ElementType.Light;

                kingSlimeDungeonData.BossMonsterIndex.Add(106010501);
                kingSlimeDungeonData.BossMonsterScale = JsonConvert.DeserializeObject<List<float>>(rows[i]["BossMonsterScale"].ToString());
                kingSlimeDungeonData.BossMonsterLevel = rows[i]["BossMonsterLevel"].ToString().ToInt();

                kingSlimeDungeonData.WaveMonsterPosition = JsonConvert.DeserializeObject<List<int>>(rows[i]["WaveMonsterPosition"].ToString());
                kingSlimeDungeonData.WaveMonsterGroup = JsonConvert.DeserializeObject<List<int>>(rows[i]["WaveMonsterGroup"].ToString());
                kingSlimeDungeonData.WaveMonsterLevel = rows[i]["WaveMonsterLevel"].ToString().ToInt();


                kingSlimeDungeonDatas.Add(kingSlimeDungeonData);
            }

            for (int i = 0; i < kingSlimeDungeonDatas.Count; ++i)
            {
                GoddessDungeonData crackDungeonData = kingSlimeDungeonDatas[i];

                if (i != 0)
                {
                    crackDungeonData.PrevData = kingSlimeDungeonDatas[i - 1];
                }

                if (i != kingSlimeDungeonDatas.Count - 1)
                {
                    crackDungeonData.NextData = kingSlimeDungeonDatas[i + 1];
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("KingSlimeDungeonClearReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessDungeonClearRewardData kingSlimeDungeonClearRewardData = new GoddessDungeonClearRewardData();

                kingSlimeDungeonClearRewardData.Index = rows[i]["Index"].ToString().ToInt();
                kingSlimeDungeonClearRewardData.EndPhase = rows[i]["EndPhase"].ToString().ToInt();

                kingSlimeDungeonClearRewardData.EndRewardType = V2Enum_Goods.Point;
                kingSlimeDungeonClearRewardData.EndRewardParam1 = V2Enum_Point.KingSlimePick.Enum32ToInt();
                kingSlimeDungeonClearRewardData.EndRewardParam2 = rows[i]["EndRewardParam2"].ToString().ToDouble();

                kingSlimeDungeonClearRewardData.PhaseRewardItemSelectCount = rows[i]["PhaseRewardItemSelectCount"].ToString().ToInt();

                for (int j = 1; j <= 6; ++j)
                {
                    EventDungeonWeightRewardData crackDungeonWeightRewardData = new EventDungeonWeightRewardData();

                    try
                    {
                        int goodstype = rows[i][string.Format("PhaseRewardItemType{0}", j)].ToString().ToInt();
                        if (goodstype != -1 && goodstype != 0)
                        {
                            crackDungeonWeightRewardData.PhaseRewardItemType = goodstype.IntToEnum32<V2Enum_Goods>();

                            crackDungeonWeightRewardData.PhaseRewardItemGrade = rows[i][string.Format("PhaseRewardItemGrade{0}", j)].ToString().ToInt().IntToEnum32<V2Enum_Grade>();
                            crackDungeonWeightRewardData.PhaseRewardItemWeight = rows[i][string.Format("PhaseRewardItemWeight{0}", j)].ToString().ToDouble();

                            kingSlimeDungeonClearRewardData.GoodsTypeRandomPicker.Add(crackDungeonWeightRewardData, crackDungeonWeightRewardData.PhaseRewardItemWeight);
                        }
                    }
                    catch
                    {

                    }
                }

                kingSlimeDungeonClearRewardData.PhaseRewardPointType = rows[i]["PhaseRewardPointType"].ToString().FastStringToInt().IntToEnum32<V2Enum_Goods>();
                kingSlimeDungeonClearRewardData.PhaseRewardPointParam1 = rows[i]["PhaseRewardPointParam1"].ToString().ToInt();
                kingSlimeDungeonClearRewardData.PhaseRewardPointParam2 = rows[i]["PhaseRewardPointParam2"].ToString().ToDouble();

                kingSlimeDungeonClearRewardDatas.Add(kingSlimeDungeonClearRewardData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("KingSlimeDungeonRankReward", o =>
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

                eventKingSlimeRankRewardDatas.Add(eventRouletteRankRewardData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("KingSlimeDungeonSummonReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessDungeonCountRewardOnceData kingSlimeDungeonCountRewardOnceData = new GoddessDungeonCountRewardOnceData();
                kingSlimeDungeonCountRewardOnceData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                kingSlimeDungeonCountRewardOnceData.AccumedSummonCount = rows[i]["AccumedSummonCount"].ToString().ToInt();

                kingSlimeDungeonCountRewardOnceData.CountRewardGoodsType = rows[i]["CountRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                kingSlimeDungeonCountRewardOnceData.CountRewardGoodsParam1 = rows[i]["CountRewardGoodsParam1"].ToString().ToInt();
                kingSlimeDungeonCountRewardOnceData.CountRewardGoodsParam2 = rows[i]["CountRewardGoodsParam2"].ToString().ToDouble();

                kingSlimeDungeonCountRewardOnceDatas.Add(kingSlimeDungeonCountRewardOnceData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("KingSlimeDungeonMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMissionData eventRouletteMissionData = new EventMissionData();
                eventRouletteMissionData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                eventRouletteMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                eventRouletteMissionData.ClearRewardType = V2Enum_Goods.Point;
                eventRouletteMissionData.ClearRewardParam1 = V2Enum_Point.KingSlimePick.Enum32ToInt();
                eventRouletteMissionData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();

                eventRouletteMissionData.NameLocalStringKey = string.Format("mission/{0}/name", eventRouletteMissionData.MissionType.Enum32ToInt());

                eventKingSlimeMissionDatas.Add(eventRouletteMissionData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("KingSlimeDungeonSummon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessSummonData allyJewelryData = new GoddessSummonData();
                allyJewelryData.Index = rows[i]["Index"].ToString().FastStringToInt();
                allyJewelryData.GoodsType = rows[i]["GoodsType"].ToString().FastStringToInt().IntToEnum32<V2Enum_Goods>();
                allyJewelryData.GoodsIndex = rows[i]["GoodsIndex"].ToString().FastStringToInt();
                allyJewelryData.GoodsAmount = rows[i]["GoodsAmount"].ToString().ToDouble();

                allyJewelryData.SummonWeight = rows[i]["SummonWeight"].ToString().ToDouble();
                allyJewelryData.IsFinalReward = rows[i]["IsFinalReward"].ToString().ToInt() == 1;
                if (allyJewelryData.IsFinalReward == true)
                    SummonPickData = allyJewelryData;

                totalSummonWeight += allyJewelryData.SummonWeight;

                WeightedRandomPicker_Summon.Add(allyJewelryData, allyJewelryData.SummonWeight);
                kingSlimeSummonDatas.Add(allyJewelryData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("KingSlimeDungeonPass", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessPassData kingSlimePassData = new GoddessPassData();
                kingSlimePassData.Index = rows[i]["Index"].ToString().ToInt();
                kingSlimePassData.PassOrder = rows[i]["PassOrder"].ToString().ToInt();

                //kingSlimePassData.PassClearConditionType = rows[i]["PassClearConditionType"].ToString().ToInt().IntToEnum32<V2Enum_MissionType>();
                kingSlimePassData.PassClearConditionParam = rows[i]["PassClearConditionParam"].ToString().ToInt();

                kingSlimePassData.FreeRewardGoodsType = rows[i]["FreeRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                kingSlimePassData.FreeRewardGoodsParam1 = rows[i]["FreeRewardGoodsParam1"].ToString().ToInt();
                kingSlimePassData.FreeRewardGoodsParam2 = rows[i]["FreeRewardGoodsParam2"].ToString().ToDouble();

                kingSlimePassData.ShopPackageIndex = rows[i]["ShopPackageIndex"].ToString().ToInt();

                kingSlimePassDatas.Add(kingSlimePassData);
            }
        }
    }
}