using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class EventRedBullLocalTable : LocalTableBase
    {
        public List<CrackDungeonData> goddessDungeonDatas = new List<CrackDungeonData>();

        public List<CrackDungeonClearRewardData> goddessDungeonClearRewardDatas = new List<CrackDungeonClearRewardData>();

        public List<RankRewardData> eventGoddessRankRewardDatas = new List<RankRewardData>();

        public List<GoddessDungeonCountRewardOnceData> goddessDungeonCountRewardOnceDatas = new List<GoddessDungeonCountRewardOnceData>();

        public List<EventMissionData> eventGoddessMissionDatas = new List<EventMissionData>();

        public List<GoddessSummonData> goddessSummonDatas = new List<GoddessSummonData>();

        public WeightedRandomPicker<GoddessSummonData> WeightedRandomPicker_Summon = new WeightedRandomPicker<GoddessSummonData>();

        public GoddessSummonData SummonPickData;

        public double totalSummonWeight = 0.0;

        public List<GoddessPassData> goddessPassDatas = new List<GoddessPassData>();


        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RedBullDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CrackDungeonData crackDungeonData = new CrackDungeonData();

                crackDungeonData.Index = rows[i]["Index"].ToString().ToInt();
                crackDungeonData.PhaseNumber = rows[i]["PhaseNumber"].ToString().ToInt();

                crackDungeonData.PhaseElementType = V2Enum_ElementType.Fire;

                crackDungeonData.WaveMonsterGroup.Add(106010499);
                crackDungeonData.WaveMonsterScale = JsonConvert.DeserializeObject<List<float>>(rows[i]["BossMonsterScale"].ToString());

                for (int j = 1; j <= 5; ++j)
                {
                    try
                    {
                        int overridestat = rows[i][string.Format("BossOverrideStatType{0}", j)].ToString().ToInt();

                        if (overridestat == -1 || overridestat == 0)
                            continue;

                        CreatureBaseStatElement creatureBaseStatElement = new CreatureBaseStatElement();
                        creatureBaseStatElement.BaseStat = overridestat.IntToEnum32<V2Enum_Stat>();
                        creatureBaseStatElement.BaseValue = rows[i][string.Format("BossOverrideStatValue{0}", j)].ToString().ToDouble();

                        if (creatureBaseStatElement.BaseStat == V2Enum_Stat.HP)
                            crackDungeonData.HP = creatureBaseStatElement.BaseValue;

                        crackDungeonData.MasteryDungeonOverrideStats.Add(creatureBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                goddessDungeonDatas.Add(crackDungeonData);
            }

            for (int i = 0; i < goddessDungeonDatas.Count; ++i)
            {
                CrackDungeonData crackDungeonData = goddessDungeonDatas[i];

                if (i != 0)
                {
                    crackDungeonData.PrevData = goddessDungeonDatas[i - 1];
                }

                if (i != goddessDungeonDatas.Count - 1)
                {
                    crackDungeonData.NextData = goddessDungeonDatas[i + 1];
                }
            }





            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RedBullDungeonClearReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CrackDungeonClearRewardData crackDungeonClearRewardData = new CrackDungeonClearRewardData();

                crackDungeonClearRewardData.Index = rows[i]["Index"].ToString().ToInt();
                crackDungeonClearRewardData.AccumedDamageMin = rows[i]["AccumedDamageMin"].ToString().ToDouble();
                crackDungeonClearRewardData.AccumedDamageMax = rows[i]["AccumedDamageMax"].ToString().ToDouble();

                crackDungeonClearRewardData.EndRewardType = V2Enum_Goods.Point;
                crackDungeonClearRewardData.EndRewardParam1 = V2Enum_Point.InGameGold.Enum32ToInt();
                crackDungeonClearRewardData.EndRewardParam2 = rows[i]["EndRewardParam2"].ToString().ToDouble();

                crackDungeonClearRewardData.PhaseRewardItemSelectCount = rows[i]["PhaseRewardItemSelectCount"].ToString().ToInt();

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

                            crackDungeonClearRewardData.GoodsTypeRandomPicker.Add(crackDungeonWeightRewardData, crackDungeonWeightRewardData.PhaseRewardItemWeight);
                        }
                    }
                    catch
                    {

                    }
                }

                crackDungeonClearRewardData.PhaseRewardPointType = rows[i]["PhaseRewardPointType"].ToString().FastStringToInt().IntToEnum32<V2Enum_Goods>();
                crackDungeonClearRewardData.PhaseRewardPointParam1 = rows[i]["PhaseRewardPointParam1"].ToString().ToInt();
                crackDungeonClearRewardData.PhaseRewardPointParam2 = rows[i]["PhaseRewardPointParam2"].ToString().ToDouble();

                goddessDungeonClearRewardDatas.Add(crackDungeonClearRewardData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RedBullDungeonRankReward", o =>
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

                eventGoddessRankRewardDatas.Add(eventRouletteRankRewardData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RedBullDungeonSummonRewardOnce", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessDungeonCountRewardOnceData goddessDungeonCountRewardOnceData = new GoddessDungeonCountRewardOnceData();
                goddessDungeonCountRewardOnceData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                goddessDungeonCountRewardOnceData.AccumedSummonCount = rows[i]["AccumedSummonCount"].ToString().ToInt();

                goddessDungeonCountRewardOnceData.CountRewardGoodsType = rows[i]["CountRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                goddessDungeonCountRewardOnceData.CountRewardGoodsParam1 = rows[i]["CountRewardGoodsParam1"].ToString().ToInt();
                goddessDungeonCountRewardOnceData.CountRewardGoodsParam2 = rows[i]["CountRewardGoodsParam2"].ToString().ToDouble();

                goddessDungeonCountRewardOnceDatas.Add(goddessDungeonCountRewardOnceData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RedBullDungeonMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMissionData eventRouletteMissionData = new EventMissionData();
                eventRouletteMissionData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                eventRouletteMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                eventRouletteMissionData.ClearRewardType = V2Enum_Goods.Point;
                eventRouletteMissionData.ClearRewardParam1 = V2Enum_Point.RedBullPick.Enum32ToInt();
                eventRouletteMissionData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();

                eventRouletteMissionData.NameLocalStringKey = string.Format("mission/{0}/name", eventRouletteMissionData.MissionType.Enum32ToInt());

                eventGoddessMissionDatas.Add(eventRouletteMissionData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RedBullDungeonSummon", o =>
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
                goddessSummonDatas.Add(allyJewelryData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RedBullDungeonPass", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessPassData goddessPassData = new GoddessPassData();
                goddessPassData.Index = rows[i]["Index"].ToString().ToInt();
                goddessPassData.PassOrder = rows[i]["PassOrder"].ToString().ToInt();

                //goddessPassData.PassClearConditionType = rows[i]["PassClearConditionType"].ToString().ToInt().IntToEnum32<V2Enum_MissionType>();
                goddessPassData.PassClearConditionParam = rows[i]["PassClearConditionParam"].ToString().ToInt();

                goddessPassData.FreeRewardGoodsType = rows[i]["FreeRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                goddessPassData.FreeRewardGoodsParam1 = rows[i]["FreeRewardGoodsParam1"].ToString().ToInt();
                goddessPassData.FreeRewardGoodsParam2 = rows[i]["FreeRewardGoodsParam2"].ToString().ToDouble();

                goddessPassData.ShopPackageIndex = rows[i]["ShopPackageIndex"].ToString().ToInt();

                goddessPassDatas.Add(goddessPassData);
            }
        }
    }
}