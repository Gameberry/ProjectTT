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
    public class RotationEventData
    {
        public ObscuredInt Index;

        public ObscuredInt EventOrder;

        public ObscuredInt BossIndex;

        public V2Enum_ElementType BossElementType;
    }

    public class RotationEventBossStat
    {
        public ObscuredInt Index;
        public ObscuredInt PhaseNumber;


        public List<float> WaveMonsterScale;


        public double HP;

        public List<CreatureBaseStatElement> MasteryDungeonOverrideStats = new List<CreatureBaseStatElement>();

        public RotationEventBossStat PrevData;
        public RotationEventBossStat NextData;
    }

    public class RotationEventCountRewardOnce : CrackDungeonCountRewardOnceData
    {
        public ObscuredInt EventOrder;
    }

    public class RotationEventCountRewardRepeat : CrackDungeonCountRewardRepeatData
    {
        public ObscuredInt EventOrder;
    }

    public class RotationEventLocalTable : LocalTableBase
    {
        public List<RotationEventData> rotationEventData = new List<RotationEventData>();

        public List<RotationEventBossStat> rotationEventBossStats = new List<RotationEventBossStat>();

        public List<CrackDungeonClearRewardData> rotationEventClearRewards = new List<CrackDungeonClearRewardData>();

        public List<RankRewardData> rotationEventRankRewards = new List<RankRewardData>();

        public Dictionary<ObscuredInt, List<RotationEventCountRewardOnce>> rotationEventCountRewardOnces = new Dictionary<ObscuredInt, List<RotationEventCountRewardOnce>>();

        public Dictionary<ObscuredInt, List<RotationEventCountRewardRepeat>> rotationEventCountRewardRepeatDatas = new Dictionary<ObscuredInt, List<RotationEventCountRewardRepeat>>();

        public Dictionary<ObscuredInt, RotationEventCountRewardRepeat> rotationCountRewardRepeatFinalData = new Dictionary<ObscuredInt, RotationEventCountRewardRepeat>();
        public Dictionary<ObscuredInt, int> RepeatMaxAccumedCombatCount = new Dictionary<ObscuredInt, int>();

        public List<EventMissionData> eventRouletteMissionDatas = new List<EventMissionData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RotationEvent", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                RotationEventData rotationEventData = new RotationEventData();

                rotationEventData.Index = rows[i]["Index"].ToString().ToInt();
                rotationEventData.EventOrder = rows[i]["EventOrder"].ToString().ToInt();
                rotationEventData.BossIndex = rows[i]["BossIndex"].ToString().ToInt();

                rotationEventData.BossElementType = rows[i]["BossElementType"].ToString().FastStringToInt().IntToEnum32<V2Enum_ElementType>();

                this.rotationEventData.Add(rotationEventData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RotationEventBossStat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                RotationEventBossStat rotationEventBossStat = new RotationEventBossStat();

                rotationEventBossStat.Index = rows[i]["Index"].ToString().ToInt();
                rotationEventBossStat.PhaseNumber = rows[i]["PhaseNumber"].ToString().ToInt();

                rotationEventBossStat.WaveMonsterScale = JsonConvert.DeserializeObject<List<float>>(rows[i]["BossMonsterScale"].ToString());

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
                            rotationEventBossStat.HP = creatureBaseStatElement.BaseValue;

                        rotationEventBossStat.MasteryDungeonOverrideStats.Add(creatureBaseStatElement);
                    }
                    catch
                    {

                    }
                }


                rotationEventBossStats.Add(rotationEventBossStat);
            }

            for (int i = 0; i < rotationEventBossStats.Count; ++i)
            {
                RotationEventBossStat crackDungeonData = rotationEventBossStats[i];

                if (i != 0)
                {
                    crackDungeonData.PrevData = rotationEventBossStats[i - 1];
                }

                if (i != rotationEventBossStats.Count - 1)
                {
                    crackDungeonData.NextData = rotationEventBossStats[i + 1];
                }
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RotationEventClearReward", o =>
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


                rotationEventClearRewards.Add(crackDungeonClearRewardData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RotationEventRankReward", o =>
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

                rotationEventRankRewards.Add(eventRouletteRankRewardData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RotationEventCountRewardOnce", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                RotationEventCountRewardOnce crackDungeonCountRewardOnceData = new RotationEventCountRewardOnce();
                crackDungeonCountRewardOnceData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();

                crackDungeonCountRewardOnceData.EventOrder = rows[i]["EventOrder"].ToString().ToInt();

                crackDungeonCountRewardOnceData.AccumedCombatCount = rows[i]["AccumedCombatCount"].ToString().ToInt();

                crackDungeonCountRewardOnceData.CountRewardGoodsType = rows[i]["CountRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                crackDungeonCountRewardOnceData.CountRewardGoodsParam1 = rows[i]["CountRewardGoodsParam1"].ToString().ToInt();
                crackDungeonCountRewardOnceData.CountRewardGoodsParam2 = rows[i]["CountRewardGoodsParam2"].ToString().ToDouble();

                if (rotationEventCountRewardOnces.ContainsKey(crackDungeonCountRewardOnceData.EventOrder) == false)
                    rotationEventCountRewardOnces.Add(crackDungeonCountRewardOnceData.EventOrder, new List<RotationEventCountRewardOnce>());

                rotationEventCountRewardOnces[crackDungeonCountRewardOnceData.EventOrder].Add(crackDungeonCountRewardOnceData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RotationEventCountRewardRepeat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int groupcount = rows[i]["AccumedCombatCount"].ToString().ToInt();

                RotationEventCountRewardRepeat crackDungeonCountRewardRepeatData = new RotationEventCountRewardRepeat();

                crackDungeonCountRewardRepeatData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();

                crackDungeonCountRewardRepeatData.EventOrder = rows[i]["EventOrder"].ToString().ToInt();

                crackDungeonCountRewardRepeatData.AccumedCombatCount = rows[i]["AccumedCombatCount"].ToString().ToInt();

                crackDungeonCountRewardRepeatData.rewardData = new RewardData();
                crackDungeonCountRewardRepeatData.rewardData.V2Enum_Goods = rows[i]["CountRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                crackDungeonCountRewardRepeatData.rewardData.Index = rows[i]["CountRewardGoodsParam1"].ToString().ToInt();
                crackDungeonCountRewardRepeatData.rewardData.Amount = rows[i]["CountRewardGoodsParam2"].ToString().ToDouble();


                crackDungeonCountRewardRepeatData.IsFinalReward = rows[i]["IsFinalReward"].ToString().ToInt() == 1;

                if (RepeatMaxAccumedCombatCount.ContainsKey(crackDungeonCountRewardRepeatData.EventOrder) == false)
                    RepeatMaxAccumedCombatCount.Add(crackDungeonCountRewardRepeatData.EventOrder, 0);

                if (RepeatMaxAccumedCombatCount[crackDungeonCountRewardRepeatData.EventOrder] < crackDungeonCountRewardRepeatData.AccumedCombatCount)
                    RepeatMaxAccumedCombatCount[crackDungeonCountRewardRepeatData.EventOrder] = crackDungeonCountRewardRepeatData.AccumedCombatCount;

                if (crackDungeonCountRewardRepeatData.IsFinalReward == true)
                {
                    if (rotationCountRewardRepeatFinalData.ContainsKey(crackDungeonCountRewardRepeatData.EventOrder) == false)
                        rotationCountRewardRepeatFinalData.Add(crackDungeonCountRewardRepeatData.EventOrder, crackDungeonCountRewardRepeatData);
                }
                else
                {
                    if (rotationEventCountRewardRepeatDatas.ContainsKey(crackDungeonCountRewardRepeatData.EventOrder) == false)
                        rotationEventCountRewardRepeatDatas.Add(crackDungeonCountRewardRepeatData.EventOrder, new List<RotationEventCountRewardRepeat>());

                    rotationEventCountRewardRepeatDatas[crackDungeonCountRewardRepeatData.EventOrder].Add(crackDungeonCountRewardRepeatData);
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("RotationEventMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMissionData eventRouletteMissionData = new EventMissionData();
                eventRouletteMissionData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                eventRouletteMissionData.MissionType = rows[i]["MissionType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                eventRouletteMissionData.MissionParam = rows[i]["MissionParam"].ToString().ToInt();

                eventRouletteMissionData.ClearRewardType = V2Enum_Goods.Point;
                eventRouletteMissionData.ClearRewardParam1 = V2Enum_Point.RotationEventTicket.Enum32ToInt();
                eventRouletteMissionData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();

                eventRouletteMissionData.NameLocalStringKey = string.Format("mission/{0}/name", eventRouletteMissionData.MissionType.Enum32ToInt());

                eventRouletteMissionDatas.Add(eventRouletteMissionData);
            }
        }
        //------------------------------------------------------------------------------------
    }
}