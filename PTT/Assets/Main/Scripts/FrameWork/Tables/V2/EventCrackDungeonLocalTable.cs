using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class CrackDungeonData
    {
        public int Index;
        public int PhaseNumber;

        public V2Enum_ElementType PhaseElementType;

        public List<int> WaveMonsterGroup = new List<int>();
        public List<float> WaveMonsterScale;

        public int BossMonsterAuraStep;

        public double HP;

        public List<CreatureBaseStatElement> MasteryDungeonOverrideStats = new List<CreatureBaseStatElement>();

        public Enum_TargetConditionType TargetSearchType;

        public CrackDungeonData PrevData;
        public CrackDungeonData NextData;
    }

    public class EventDungeonWeightRewardData
    {
        public V2Enum_Goods PhaseRewardItemType;
        public V2Enum_Grade PhaseRewardItemGrade;
        public double PhaseRewardItemWeight;
    }

    public class CrackDungeonClearRewardData
    {
        public int Index;

        public double AccumedDamageMin;
        public double AccumedDamageMax;

        public V2Enum_Goods EndRewardType;
        public int EndRewardParam1;
        public double EndRewardParam2;

        public int PhaseRewardItemSelectCount;

        public WeightedRandomPicker<EventDungeonWeightRewardData> GoodsTypeRandomPicker = new WeightedRandomPicker<EventDungeonWeightRewardData>();

        public V2Enum_Goods PhaseRewardPointType;
        public int PhaseRewardPointParam1;
        public double PhaseRewardPointParam2;

        public ObscuredInt HeartCount;
    }


    public class CrackDungeonCountRewardOnceData
    {
        public ObscuredInt Index;
        public ObscuredInt AccumedCombatCount;

        public V2Enum_Goods CountRewardGoodsType;
        public ObscuredInt CountRewardGoodsParam1;
        public ObscuredDouble CountRewardGoodsParam2;
    }


    public class CrackDungeonCountRewardRepeatData
    {
        public ObscuredInt Index;
        public ObscuredInt AccumedCombatCount;

        public RewardData rewardData = new RewardData();

        public bool IsFinalReward = false;
    }


    public class EventCrackDungeonLocalTable : LocalTableBase
    {
        public List<CrackDungeonData> crackDungeonDatas = new List<CrackDungeonData>();

        public List<CrackDungeonClearRewardData> crackDungeonClearRewardDatas = new List<CrackDungeonClearRewardData>();

        public List<RankRewardData> eventCrackRankRewardDatas = new List<RankRewardData>();

        public List<CrackDungeonCountRewardOnceData> crackDungeonCountRewardOnceDatas = new List<CrackDungeonCountRewardOnceData>();

        public List<CrackDungeonCountRewardRepeatData> crackDungeonCountRewardRepeatDatas = new List<CrackDungeonCountRewardRepeatData>();

        public CrackDungeonCountRewardRepeatData CrackDungeonCountRewardRepeatFinalData = new CrackDungeonCountRewardRepeatData();
        public int RepeatMaxAccumedCombatCount = 0;

        public List<EventMissionData> eventRouletteMissionDatas = new List<EventMissionData>();


        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CrackDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CrackDungeonData crackDungeonData = new CrackDungeonData();

                crackDungeonData.Index = rows[i]["Index"].ToString().ToInt();
                crackDungeonData.PhaseNumber = rows[i]["PhaseNumber"].ToString().ToInt();

                crackDungeonData.PhaseElementType = rows[i]["PhaseElementType"].ToString().FastStringToInt().IntToEnum32<V2Enum_ElementType>();

                crackDungeonData.WaveMonsterGroup.Add(106010495);
                crackDungeonData.WaveMonsterScale = JsonConvert.DeserializeObject<List<float>>(rows[i]["BossMonsterScale"].ToString());

                crackDungeonData.BossMonsterAuraStep = rows[i]["BossMonsterAuraStep"].ToString().FastStringToInt();

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

                crackDungeonData.TargetSearchType = rows[i]["TargetSearchType"].ToString().FastStringToInt().IntToEnum32<Enum_TargetConditionType>();

                crackDungeonDatas.Add(crackDungeonData);
            }

            for (int i = 0; i < crackDungeonDatas.Count; ++i)
            {
                CrackDungeonData crackDungeonData = crackDungeonDatas[i];

                if (i != 0)
                {
                    crackDungeonData.PrevData = crackDungeonDatas[i - 1];
                }

                if (i != crackDungeonDatas.Count - 1)
                {
                    crackDungeonData.NextData = crackDungeonDatas[i + 1];
                }
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CrackDungeonClearReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CrackDungeonClearRewardData crackDungeonClearRewardData = new CrackDungeonClearRewardData();

                crackDungeonClearRewardData.Index = rows[i]["Index"].ToString().ToInt();
                crackDungeonClearRewardData.AccumedDamageMin = rows[i]["AccumedDamageMin"].ToString().ToDouble();
                crackDungeonClearRewardData.AccumedDamageMax = rows[i]["AccumedDamageMax"].ToString().ToDouble();

                crackDungeonClearRewardData.EndRewardType = rows[i]["EndRewardType"].ToString().FastStringToInt().IntToEnum32<V2Enum_Goods>();
                crackDungeonClearRewardData.EndRewardParam1 = rows[i]["EndRewardParam1"].ToString().ToInt();
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

                crackDungeonClearRewardData.HeartCount = rows[i]["HeartCount"].ToString().ToInt();
                
                crackDungeonClearRewardDatas.Add(crackDungeonClearRewardData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CrackDungeonRankReward", o =>
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

                eventCrackRankRewardDatas.Add(eventRouletteRankRewardData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CrackDungeonCountRewardOnce", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CrackDungeonCountRewardOnceData crackDungeonCountRewardOnceData = new CrackDungeonCountRewardOnceData();
                crackDungeonCountRewardOnceData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                crackDungeonCountRewardOnceData.AccumedCombatCount = rows[i]["AccumedCombatCount"].ToString().ToInt();

                crackDungeonCountRewardOnceData.CountRewardGoodsType = rows[i]["CountRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                crackDungeonCountRewardOnceData.CountRewardGoodsParam1 = rows[i]["CountRewardGoodsParam1"].ToString().ToInt();
                crackDungeonCountRewardOnceData.CountRewardGoodsParam2 = rows[i]["CountRewardGoodsParam2"].ToString().ToDouble();

                crackDungeonCountRewardOnceDatas.Add(crackDungeonCountRewardOnceData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CrackDungeonCountRewardRepeat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int groupcount = rows[i]["AccumedCombatCount"].ToString().ToInt();

                CrackDungeonCountRewardRepeatData crackDungeonCountRewardRepeatData = new CrackDungeonCountRewardRepeatData();

                crackDungeonCountRewardRepeatData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                crackDungeonCountRewardRepeatData.AccumedCombatCount = rows[i]["AccumedCombatCount"].ToString().ToInt();

                crackDungeonCountRewardRepeatData.rewardData = new RewardData();
                crackDungeonCountRewardRepeatData.rewardData.V2Enum_Goods = rows[i]["CountRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                crackDungeonCountRewardRepeatData.rewardData.Index = rows[i]["CountRewardGoodsParam1"].ToString().ToInt();
                crackDungeonCountRewardRepeatData.rewardData.Amount = rows[i]["CountRewardGoodsParam2"].ToString().ToDouble();


                crackDungeonCountRewardRepeatData.IsFinalReward = rows[i]["IsFinalReward"].ToString().ToInt() == 1;

                if (RepeatMaxAccumedCombatCount < crackDungeonCountRewardRepeatData.AccumedCombatCount)
                    RepeatMaxAccumedCombatCount = crackDungeonCountRewardRepeatData.AccumedCombatCount;

                if (crackDungeonCountRewardRepeatData.IsFinalReward == true)
                    CrackDungeonCountRewardRepeatFinalData = crackDungeonCountRewardRepeatData;
                else
                    crackDungeonCountRewardRepeatDatas.Add(crackDungeonCountRewardRepeatData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CrackDungeonMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMissionData eventRouletteMissionData = new EventMissionData();
                eventRouletteMissionData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                eventRouletteMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                eventRouletteMissionData.ClearRewardType = V2Enum_Goods.Point;
                eventRouletteMissionData.ClearRewardParam1 = V2Enum_Point.EventDungeonTicket.Enum32ToInt();
                eventRouletteMissionData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();

                eventRouletteMissionData.NameLocalStringKey = string.Format("mission/{0}/name", eventRouletteMissionData.MissionType.Enum32ToInt());

                eventRouletteMissionDatas.Add(eventRouletteMissionData);
            }
        }
        //------------------------------------------------------------------------------------
    }
}