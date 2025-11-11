using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class GoddessDungeonData
    {
        public int Index;
        public int PhaseNumber;

        public V2Enum_ElementType PhaseElementType;

        public List<int> BossMonsterIndex = new List<int>();
        public List<float> BossMonsterScale;
        public int BossMonsterLevel;

        public List<int> WaveMonsterPosition;
        public List<int> WaveMonsterGroup;
        public int WaveMonsterLevel;

        public GoddessDungeonData PrevData;
        public GoddessDungeonData NextData;
    }

    public class GoddessDungeonClearRewardData
    {
        public int Index;

        public int EndPhase;

        public V2Enum_Goods EndRewardType;
        public int EndRewardParam1;
        public double EndRewardParam2;

        public int PhaseRewardItemSelectCount;

        public WeightedRandomPicker<EventDungeonWeightRewardData> GoodsTypeRandomPicker = new WeightedRandomPicker<EventDungeonWeightRewardData>();

        public V2Enum_Goods PhaseRewardPointType;
        public int PhaseRewardPointParam1;
        public double PhaseRewardPointParam2;
    }

    public class GoddessDungeonCountRewardOnceData
    {
        public ObscuredInt Index;
        public ObscuredInt AccumedSummonCount;

        public V2Enum_Goods CountRewardGoodsType;
        public ObscuredInt CountRewardGoodsParam1;
        public ObscuredDouble CountRewardGoodsParam2;
    }


    public class GoddessSummonData
    {
        public ObscuredInt Index;
        public V2Enum_Goods GoodsType;
        public ObscuredInt GoodsIndex;
        public ObscuredDouble GoodsAmount; 

        public ObscuredDouble SummonWeight;

        public ObscuredBool IsFinalReward;
    }

    public class GoddessPassData
    {
        public ObscuredInt Index;
        public ObscuredInt PassOrder;

        //public V2Enum_MissionType PassClearConditionType;
        public ObscuredInt PassClearConditionParam;

        public V2Enum_Goods FreeRewardGoodsType;
        public ObscuredInt FreeRewardGoodsParam1;
        public ObscuredDouble FreeRewardGoodsParam2;

        public ObscuredInt ShopPackageIndex;
    }

    public class EventGoddessDungeonLocalTable : LocalTableBase
    {
        public List<GoddessDungeonData> goddessDungeonDatas = new List<GoddessDungeonData>();

        public List<GoddessDungeonClearRewardData> goddessDungeonClearRewardDatas = new List<GoddessDungeonClearRewardData>();

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

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoddessDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessDungeonData goddessDungeonData = new GoddessDungeonData();

                goddessDungeonData.Index = rows[i]["Index"].ToString().ToInt();
                goddessDungeonData.PhaseNumber = rows[i]["PhaseNumber"].ToString().ToInt();

                goddessDungeonData.PhaseElementType = V2Enum_ElementType.Light;

                goddessDungeonData.BossMonsterIndex.Add(106010496);
                goddessDungeonData.BossMonsterScale = JsonConvert.DeserializeObject<List<float>>(rows[i]["BossMonsterScale"].ToString());
                goddessDungeonData.BossMonsterLevel = rows[i]["BossMonsterLevel"].ToString().ToInt();

                goddessDungeonData.WaveMonsterPosition = JsonConvert.DeserializeObject<List<int>>(rows[i]["WaveMonsterPosition"].ToString());
                goddessDungeonData.WaveMonsterGroup = JsonConvert.DeserializeObject<List<int>>(rows[i]["WaveMonsterGroup"].ToString());
                goddessDungeonData.WaveMonsterLevel = rows[i]["WaveMonsterLevel"].ToString().ToInt();


                goddessDungeonDatas.Add(goddessDungeonData);
            }

            for (int i = 0; i < goddessDungeonDatas.Count; ++i)
            {
                GoddessDungeonData crackDungeonData = goddessDungeonDatas[i];

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

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoddessDungeonClearReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoddessDungeonClearRewardData goddessDungeonClearRewardData = new GoddessDungeonClearRewardData();

                goddessDungeonClearRewardData.Index = rows[i]["Index"].ToString().ToInt();
                goddessDungeonClearRewardData.EndPhase = rows[i]["EndPhase"].ToString().ToInt();

                goddessDungeonClearRewardData.EndRewardType = V2Enum_Goods.Point;
                goddessDungeonClearRewardData.EndRewardParam1 = V2Enum_Point.GoddessPick.Enum32ToInt();
                goddessDungeonClearRewardData.EndRewardParam2 = rows[i]["EndRewardParam2"].ToString().ToDouble();

                goddessDungeonClearRewardData.PhaseRewardItemSelectCount = rows[i]["PhaseRewardItemSelectCount"].ToString().ToInt();

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

                            goddessDungeonClearRewardData.GoodsTypeRandomPicker.Add(crackDungeonWeightRewardData, crackDungeonWeightRewardData.PhaseRewardItemWeight);
                        }
                    }
                    catch
                    {

                    }
                }

                goddessDungeonClearRewardData.PhaseRewardPointType = rows[i]["PhaseRewardPointType"].ToString().FastStringToInt().IntToEnum32<V2Enum_Goods>();
                goddessDungeonClearRewardData.PhaseRewardPointParam1 = rows[i]["PhaseRewardPointParam1"].ToString().ToInt();
                goddessDungeonClearRewardData.PhaseRewardPointParam2 = rows[i]["PhaseRewardPointParam2"].ToString().ToDouble();

                goddessDungeonClearRewardDatas.Add(goddessDungeonClearRewardData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoddessDungeonRankReward", o =>
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

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoddessDungeonSummonRewardOnce", o =>
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

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoddessDungeonMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMissionData eventRouletteMissionData = new EventMissionData();
                eventRouletteMissionData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                eventRouletteMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                eventRouletteMissionData.ClearRewardType = V2Enum_Goods.Point;
                eventRouletteMissionData.ClearRewardParam1 = V2Enum_Point.GoddessPick.Enum32ToInt();
                eventRouletteMissionData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();

                eventRouletteMissionData.NameLocalStringKey = string.Format("mission/{0}/name", eventRouletteMissionData.MissionType.Enum32ToInt());

                eventGoddessMissionDatas.Add(eventRouletteMissionData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoddessDungeonSummon", o =>
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

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoddessDungeonPass", o =>
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