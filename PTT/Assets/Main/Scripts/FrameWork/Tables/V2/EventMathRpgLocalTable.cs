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
    public class MathRpgBrickData
    {
        public List<int> brickPos = new List<int>();
        public MathRpgObjectData brickObj = null;
    }

    public class MathRpgBrickLineData
    {
        public ObscuredInt Index;
        public ObscuredInt GroupIndex;
        public ObscuredInt Order;

        public List<MathRpgBrickData> mathRpgBrickDatas = new List<MathRpgBrickData>();
    }

    public class MathRpgBrickContainerData
    {
        public ObscuredInt GroupIndex;

        public List<MathRpgBrickLineData> mathRpgBrickLineDatas = new List<MathRpgBrickLineData>();
    }

    public class MathRpgObjectData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;

        public V2Enum_ObjectType ObjectType;
        public ObscuredDouble Param1;
        public ObscuredDouble Param2;
        public ObscuredDouble Param3;
    }

    public class ScenarioLine : InfiniteScrollData
    { 
        public ObscuredInt Step;
        public MathRpgBrickLineData LineData;

        public EventMathRpgScenarioData eventMathRpgScenarioData;

        public ScenarioLine PrevLine = null;
    }

    public class EventMathRpgScenarioData
    {
        public ObscuredInt Phase;

        public ObscuredInt MinStep;
        public ObscuredInt MaxStep;

        public ObscuredInt BossStep;
        public ObscuredDouble BossCombatPower;

        public List<MathRpgBrickContainerData> mathRpgBrickContainerDatas = new List<MathRpgBrickContainerData>();

        public List<RewardData> RewardGoods = new List<RewardData>();

        // List<라인데이터>
        public Dictionary<ObscuredInt, ScenarioLine> mathLineDatas = new Dictionary<ObscuredInt, ScenarioLine>();
    }

    public class EventMathRpgRouletteData
    {
        public ObscuredInt Index;

        public ObscuredInt SlotNumber;
        public ObscuredInt SlotGroupNumber;

        //public MathRpgObjectData mathRpgObject;

        public ObscuredDouble IncreasePower = 0;

        public ObscuredDouble SelectionWeight;

        public ObscuredInt RouletteLevelGroup;
    }

    public class EventMathRpgRouletteCountRewardData
    {
        public ObscuredInt SlotGroupNumber;
        public ObscuredInt AccumedSelectCount;
        public RewardData RewardGoods = new RewardData();
    }

    public class EventMathRpgCharacterLevelData
    {
        public ObscuredInt Index;
        public ObscuredInt CharacterLevel;
        public ObscuredLong RequiredExp;

        public ObscuredDouble CoinMaxRechargeAmount;
        public ObscuredDouble RouletteSpinCost;
        public ObscuredDouble ObjectRewardAddValue;

        public ObscuredDouble BoostIncreasePower;

        public List<EventMathRpgRouletteData> RouletteDatas = new List<EventMathRpgRouletteData>();
        public WeightedRandomPicker<EventMathRpgRouletteData> RouletteDataPicker = new WeightedRandomPicker<EventMathRpgRouletteData>();
    }

    public class EventMathRpgLocalTable : LocalTableBase
    {
        public Dictionary<ObscuredInt, EventMathRpgScenarioData> eventMathRpgScenarioDatas = new Dictionary<ObscuredInt, EventMathRpgScenarioData>();

        public List<EventDigPassData> eventMathRpgPassDatas = new List<EventDigPassData>();
        public List<EventDigPassLevelData> eventMathRpgPassLevelDatas = new List<EventDigPassLevelData>();

        public List<RankRewardData> eventMathRpgRankRewardDatas = new List<RankRewardData>();

        public Dictionary<ObscuredInt, List<EventMathRpgRouletteCountRewardData>> eventMathRpgRouletteCountRewardDatas_Dic = new Dictionary<ObscuredInt, List<EventMathRpgRouletteCountRewardData>>();

        public Dictionary<ObscuredInt, EventMathRpgCharacterLevelData> eventMathRpgCharacterLevelDatas_Dic = new Dictionary<ObscuredInt, EventMathRpgCharacterLevelData>();

        public ObscuredInt StartPhase = -1, EndPhase = -1, EndStep = -1;



        public override async UniTask InitData_Async()
        {
            Dictionary<ObscuredInt, MathRpgObjectData> mathRpgObjectDatas = new Dictionary<ObscuredInt, MathRpgObjectData>();
            Dictionary<ObscuredInt, MathRpgBrickContainerData> mathRpgBrickContainerDatas = new Dictionary<ObscuredInt, MathRpgBrickContainerData>();

            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgObject", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                MathRpgObjectData mathRpgObjectData = new MathRpgObjectData();

                mathRpgObjectData.Index = rows[i]["Index"].ToString().ToInt();
                mathRpgObjectData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                mathRpgObjectData.ObjectType = rows[i]["ObjectType"].ToString().ToInt().IntToEnum32<V2Enum_ObjectType>();

                mathRpgObjectData.Param1 = rows[i]["Param1"].ToString().ToDouble();
                mathRpgObjectData.Param2 = rows[i]["Param2"].ToString().ToDouble();
                mathRpgObjectData.Param3 = rows[i]["Param3"].ToString().ToDouble();

                mathRpgObjectDatas.Add(mathRpgObjectData.Index, mathRpgObjectData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgBrick", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                MathRpgBrickLineData MathRpgBrickLineData = new MathRpgBrickLineData();

                MathRpgBrickLineData.Index = rows[i]["Index"].ToString().ToInt();
                MathRpgBrickLineData.GroupIndex = rows[i]["GroupIndex"].ToString().ToInt();
                MathRpgBrickLineData.Order = rows[i]["Order"].ToString().ToInt();

                for (int idx = 1; idx <= 3; ++idx)
                {
                    string pos = rows[i][string.Format("Brick{0}", idx)].ToString();
                    if (pos.Contains("-1"))
                        continue;

                    MathRpgBrickData mathRpgBrickData = new MathRpgBrickData();
                    mathRpgBrickData.brickPos = JsonConvert.DeserializeObject<List<int>>(pos);
                    //string[] posarr = pos.sp

                    ObscuredInt objIdx = rows[i][string.Format("Object{0}", idx)].ToString().ToInt();

                    if (mathRpgObjectDatas.ContainsKey(objIdx) == true)
                    {
                        mathRpgBrickData.brickObj = mathRpgObjectDatas[objIdx];
                    }

                    MathRpgBrickLineData.mathRpgBrickDatas.Add(mathRpgBrickData);
                }

                MathRpgBrickContainerData mathRpgBrickContainerData = null;

                if (mathRpgBrickContainerDatas.ContainsKey(MathRpgBrickLineData.GroupIndex) == true)
                    mathRpgBrickContainerData = mathRpgBrickContainerDatas[MathRpgBrickLineData.GroupIndex];
                else
                {
                    mathRpgBrickContainerData = new MathRpgBrickContainerData();
                    mathRpgBrickContainerData.GroupIndex = MathRpgBrickLineData.GroupIndex;
                    mathRpgBrickContainerDatas.Add(mathRpgBrickContainerData.GroupIndex, mathRpgBrickContainerData);
                }

                mathRpgBrickContainerData.mathRpgBrickLineDatas.Add(MathRpgBrickLineData);
            }

            foreach (var pair in mathRpgBrickContainerDatas)
            {
                pair.Value.mathRpgBrickLineDatas.Sort((x, y) =>
                {
                    if (x.Order > y.Order)
                        return 1;
                    else
                        return -1;
                });
            }





            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgScenario", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt phase = rows[i]["Phase"].ToString().ToInt();

                EventMathRpgScenarioData eventMathRpgScenarioData = new EventMathRpgScenarioData();
                eventMathRpgScenarioData.Phase = phase;

                List<int> GroupIndexList = JsonConvert.DeserializeObject<List<int>>(rows[i]["GroupIndex"].ToString());

                for (int groupidx = 0; groupidx < GroupIndexList.Count; ++groupidx)
                {
                    ObscuredInt groupIndex = GroupIndexList[groupidx];

                    if (mathRpgBrickContainerDatas.ContainsKey(groupIndex) == true)
                    {
                        MathRpgBrickContainerData mathRpgBrickContainerData = mathRpgBrickContainerDatas[groupIndex];
                        eventMathRpgScenarioData.mathRpgBrickContainerDatas.Add(mathRpgBrickContainerData);
                    }
                }

                for (int idx = 1; idx <= 4; ++idx)
                {
                    int rewardtype = rows[i][string.Format("RewardGoodsType{0}", idx)].ToString().ToInt();
                    if (rewardtype == -1)
                        continue;

                    RewardData rewardData = new RewardData();
                    rewardData.V2Enum_Goods = rewardtype.IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = rows[i][string.Format("RewardGoodsParam{0}1", idx)].ToString().ToInt();
                    rewardData.Amount = rows[i][string.Format("RewardGoodsParam{0}2", idx)].ToString().ToDouble();

                    eventMathRpgScenarioData.RewardGoods.Add(rewardData);
                }

                eventMathRpgScenarioDatas.Add(eventMathRpgScenarioData.Phase, eventMathRpgScenarioData);

                if (StartPhase == -1)
                    StartPhase = eventMathRpgScenarioData.Phase;
                else
                {
                    if (StartPhase > eventMathRpgScenarioData.Phase)
                        StartPhase = eventMathRpgScenarioData.Phase;
                }

                if (EndPhase == -1)
                    EndPhase = eventMathRpgScenarioData.Phase;
                else
                {
                    if (EndPhase < eventMathRpgScenarioData.Phase)
                        EndPhase = eventMathRpgScenarioData.Phase;
                }
            }


            ObscuredInt currStep = 1;

            MathRpgBrickData garaBrick = new MathRpgBrickData();
            garaBrick.brickPos.Add(1);
            garaBrick.brickPos.Add(2);
            garaBrick.brickPos.Add(3);
            garaBrick.brickPos.Add(4);
            garaBrick.brickPos.Add(5);
            garaBrick.brickPos.Add(6);

            MathRpgBrickLineData garaLineData = new MathRpgBrickLineData();
            garaLineData.mathRpgBrickDatas.Add(garaBrick);


            foreach (var pair in eventMathRpgScenarioDatas)
            {
                EventMathRpgScenarioData eventMathRpgScenarioData = pair.Value;
                eventMathRpgScenarioData.MinStep = currStep;

                // 가라 최저층 데이터 생성
                ScenarioLine garaLine = new ScenarioLine();
                garaLine.Step = currStep - 1;
                garaLine.LineData = garaLineData;
                garaLine.eventMathRpgScenarioData = eventMathRpgScenarioData;
                eventMathRpgScenarioData.mathLineDatas.Add(garaLine.Step, garaLine);
                // 가라 최저층 데이터 생성

                ScenarioLine prevLine = garaLine;

                for (int i = 0; i < eventMathRpgScenarioData.mathRpgBrickContainerDatas.Count; ++i)
                {
                    MathRpgBrickContainerData mathRpgBrickContainerData = pair.Value.mathRpgBrickContainerDatas[i];

                    for (int line = 0; line < mathRpgBrickContainerData.mathRpgBrickLineDatas.Count; ++line)
                    {
                        MathRpgBrickLineData mathRpgBrickLineData = mathRpgBrickContainerData.mathRpgBrickLineDatas[line];

                        ScenarioLine scenarioLine = new ScenarioLine();

                        scenarioLine.Step = currStep;
                        scenarioLine.LineData = mathRpgBrickLineData;
                        scenarioLine.PrevLine = prevLine;
                        scenarioLine.eventMathRpgScenarioData = eventMathRpgScenarioData;

                        prevLine = scenarioLine;

                        eventMathRpgScenarioData.mathLineDatas.Add(scenarioLine.Step, scenarioLine);
                        eventMathRpgScenarioData.MaxStep = currStep;

                        for (int obj = 0; obj < mathRpgBrickLineData.mathRpgBrickDatas.Count; ++obj)
                        {
                            MathRpgBrickData mathRpgBrickData = mathRpgBrickLineData.mathRpgBrickDatas[obj];

                            if (mathRpgBrickData.brickObj.ObjectType == V2Enum_ObjectType.ObjectCombat)
                            {
                                eventMathRpgScenarioData.BossCombatPower = mathRpgBrickData.brickObj.Param1;
                                eventMathRpgScenarioData.BossStep = currStep;
                            }
                        }

                        currStep++;
                    }
                }

                if (EndStep < eventMathRpgScenarioData.MaxStep)
                    EndStep = eventMathRpgScenarioData.MaxStep;
            }





            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgPass", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventDigPassData eventDigPassData = new EventDigPassData();

                eventDigPassData.Index = rows[i]["Index"].ToString().ToInt();

                eventDigPassData.DisplayOrder = rows[i]["DisplayOrder"].ToString().ToInt();
                eventDigPassData.DigLevel = rows[i]["DrawLevel"].ToString().ToInt();

                {
                    RewardData rewardData = new RewardData();
                    rewardData.V2Enum_Goods = rows[i]["FreeRewardType1"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = rows[i]["FreeRewardParam11"].ToString().ToInt();
                    rewardData.Amount = rows[i]["FreeRewardParam12"].ToString().ToDouble();

                    eventDigPassData.rewardDatas.Add(rewardData);
                }

                {
                    RewardData rewardData = new RewardData();
                    rewardData.V2Enum_Goods = rows[i]["FreeRewardType2"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = rows[i]["FreeRewardParam21"].ToString().ToInt();
                    rewardData.Amount = rows[i]["FreeRewardParam22"].ToString().ToDouble();

                    eventDigPassData.rewardDatas.Add(rewardData);
                }

                eventDigPassData.IsPaidProduct = rows[i]["IsPaidProduct"].ToString().ToInt() == 1;
                eventDigPassData.PaidProductIndex = rows[i]["PaidProductIndex"].ToString().ToInt();

                eventMathRpgPassDatas.Add(eventDigPassData);
            }


            for (int i = 0; i < eventMathRpgPassDatas.Count; ++i)
            {
                EventDigPassData eventDigPassData = eventMathRpgPassDatas[i];

                if (i != 0)
                {
                    eventDigPassData.PrevData = eventMathRpgPassDatas[i - 1];
                }

                if (i != eventMathRpgPassDatas.Count - 1)
                {
                    eventDigPassData.NextData = eventMathRpgPassDatas[i + 1];
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgPassLevel", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventDigPassLevelData eventDigPassLevelData = new EventDigPassLevelData();

                eventDigPassLevelData.Index = rows[i]["Index"].ToString().ToInt();

                eventDigPassLevelData.TargetLevel = rows[i]["TargetLevel"].ToString().ToInt();
                eventDigPassLevelData.RequiredDigCount = rows[i]["RequiredDrawCount"].ToString().ToInt();

                eventMathRpgPassLevelDatas.Add(eventDigPassLevelData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgRankReward", o =>
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

                eventMathRpgRankRewardDatas.Add(eventRouletteRankRewardData);
            }




            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgCharacterLevel", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMathRpgCharacterLevelData eventMathRpgCharacterLevelData = new EventMathRpgCharacterLevelData();
                eventMathRpgCharacterLevelData.Index = rows[i]["Index"].ToString().ToInt();

                eventMathRpgCharacterLevelData.CharacterLevel = rows[i]["CharacterLevel"].ToString().ToInt();
                eventMathRpgCharacterLevelData.RequiredExp = rows[i]["RequiredExp"].ToString().ToLong();


                eventMathRpgCharacterLevelData.CoinMaxRechargeAmount = rows[i]["CoinMaxRechargeAmount"].ToString().ToDouble();
                eventMathRpgCharacterLevelData.RouletteSpinCost = rows[i]["RouletteSpinCost"].ToString().ToDouble();
                eventMathRpgCharacterLevelData.ObjectRewardAddValue = rows[i]["ObjectRewardAddValue"].ToString().ToDouble();

                eventMathRpgCharacterLevelData.BoostIncreasePower = rows[i]["BoostIncreasePower"].ToString().ToDouble();
                
                eventMathRpgCharacterLevelDatas_Dic.Add(eventMathRpgCharacterLevelData.CharacterLevel, eventMathRpgCharacterLevelData);
            }




            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgRoulette", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventMathRpgRouletteData eventMathRpgRouletteData = new EventMathRpgRouletteData();
                eventMathRpgRouletteData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();

                eventMathRpgRouletteData.SlotNumber = rows[i]["SlotNumber"].ToString().ToInt();
                eventMathRpgRouletteData.SlotGroupNumber = rows[i]["SlotGroupNumber"].ToString().ToInt();

                eventMathRpgRouletteData.IncreasePower = rows[i]["IncreasePower"].ToString().ToDouble();
                eventMathRpgRouletteData.SelectionWeight = rows[i]["SelectionWeight"].ToString().ToDouble();

                eventMathRpgRouletteData.RouletteLevelGroup = rows[i]["RouletteLevelGroup"].ToString().ToInt();

                if (eventMathRpgCharacterLevelDatas_Dic.ContainsKey(eventMathRpgRouletteData.RouletteLevelGroup) == true)
                {
                    EventMathRpgCharacterLevelData eventMathRpgCharacterLevelData = eventMathRpgCharacterLevelDatas_Dic[eventMathRpgRouletteData.RouletteLevelGroup];

                    eventMathRpgCharacterLevelData.RouletteDataPicker.Add(eventMathRpgRouletteData, eventMathRpgRouletteData.SelectionWeight);
                    eventMathRpgCharacterLevelData.RouletteDatas.Add(eventMathRpgRouletteData);
                }
            }


            foreach (var pair in eventMathRpgCharacterLevelDatas_Dic)
            {
                pair.Value.RouletteDatas.Sort((x, y) =>
                {
                    if (x.SlotNumber < y.SlotNumber)
                        return -1;
                    else if (x.SlotNumber > y.SlotNumber)
                        return 1;

                    return 0;
                });
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventMathRpgRouletteCountReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ObscuredInt slotNum = rows[i]["SlotGroupNumber"].ToString().ToInt();

                if (eventMathRpgRouletteCountRewardDatas_Dic.ContainsKey(slotNum) == false)
                {
                    eventMathRpgRouletteCountRewardDatas_Dic.Add(slotNum, new List<EventMathRpgRouletteCountRewardData>());
                }

                EventMathRpgRouletteCountRewardData eventMathRpgRouletteCountRewardData = new EventMathRpgRouletteCountRewardData();

                eventMathRpgRouletteCountRewardData.SlotGroupNumber = slotNum;
                eventMathRpgRouletteCountRewardData.AccumedSelectCount = rows[i]["AccumedSelectCount"].ToString().ToInt();

                eventMathRpgRouletteCountRewardData.RewardGoods.V2Enum_Goods = rows[i]["RewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                eventMathRpgRouletteCountRewardData.RewardGoods.Index = rows[i]["RewardGoodsParam1"].ToString().ToInt();
                eventMathRpgRouletteCountRewardData.RewardGoods.Amount = rows[i]["RewardGoodsParam2"].ToString().ToDouble();

                eventMathRpgRouletteCountRewardDatas_Dic[slotNum].Add(eventMathRpgRouletteCountRewardData);
            }



        }
    }
}