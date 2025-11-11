using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;

namespace GameBerry
{
    public class SynergyData
    {
        public ObscuredInt Index;

        public ObscuredInt ResourceIndex;

        public V2Enum_ARR_SynergyType SynergyType;

        public Dictionary<ObscuredInt, List<SynergyEffectData>> TierDatas = new Dictionary<ObscuredInt, List<SynergyEffectData>>();
    }

    public class SynergyEffectData
    {
        public ObscuredInt Index;

        public V2Enum_ARR_SynergyType SynergyType;

        public ObscuredInt SynergyTier;
        public ObscuredInt SynergyOriginCount;
        public ObscuredInt SynergyCount;
        public ObscuredInt MainSkillIndex;

        public MainSkillData SynergySkillData;
        public SynergyConditionData SynergyConditionData;

        public V2Enum_Grade V2Enum_Grade;
        public SynergyEffectData NextEffectData = null;

        public List<SynergyBreakthroughData> SynergyRuneList = new List<SynergyBreakthroughData>();
    }

    public class SynergyBreakthroughData
    {
        public ObscuredInt Index;

        public ObscuredInt SynergySkillIndex;
        public ObscuredInt Procedure;

        public ObscuredInt MainSkillIndex;
        public MainSkillData SynergySkillData;

        public ObscuredInt IconIndex;
        public V2Enum_Grade Grade;

        public string NameLocalKey;
        public string DescLocalKey;
    }

    public class SynergyLevelEffectData
    {
        public ObscuredInt Index;

        public ObscuredInt SynergySkillIndex;
        public ObscuredInt RequiredLevel;

        public ObscuredInt MainSkillIndex;

        public MainSkillData SynergySkillData;

        public string DescLocalKey;
    }

    public class SynergyLevelUpCostData
    {
        public ObscuredInt Index;

        public V2Enum_Grade Grade;
        public ObscuredInt MaximumLevel;

        public ObscuredInt SkillLevelUpCostGoodsIndex1;
        public ObscuredInt SkillLevelUpCostGoodsParam11;
        public ObscuredInt SkillLevelUpCostGoodsParam12;


        public ObscuredInt SkillLevelUpCostGoodsIndex2;
        public ObscuredInt SkillLevelUpCostGoodsParam21;
        public ObscuredInt SkillLevelUpCostGoodsParam22;
    }

    public class SynergyCombineData
    {
        public ObscuredInt Index;

        public ObscuredInt MainSkillIndex;

        public Dictionary<V2Enum_ARR_SynergyType, ObscuredInt> NeedSynergyCount = new Dictionary<V2Enum_ARR_SynergyType, ObscuredInt>();

        public MainSkillData SynergySkillData;
    }

    public class SynergyConditionData
    {
        public ObscuredInt Index;

        public ObscuredInt SynergySkillIndex;

        public ObscuredInt RequiredTier1Reinforce;
        public ObscuredInt RequiredTier2Reinforce;
        public ObscuredInt RequiredTier3Reinforce;
        public ObscuredInt RequiredTier4Reinforce;
    }

    public class SynergyBreakthroughCostData
    {
        public ObscuredInt Index;

        public V2Enum_ARR_SynergyType SynergyType;

        public ObscuredInt Procedure;

        public ObscuredInt LimitBreakCostGoodsIndex;
        public ObscuredInt LimitBreakCostGoodsValue;
    }

    public class SynergyDuplicationData
    {
        public ObscuredInt Index;

        public V2Enum_ARR_SynergyType SynergyType;

        public V2Enum_Grade Grade;

        public ObscuredInt DuplicationGoodsIndex;
        public ObscuredDouble DuplicationGoodsValue;
    }

    public class SynergyTotalLevelCostData
    {
        public ObscuredInt Index;

        public ObscuredInt CostGoodsIndex;
        public ObscuredInt GainExp;
    }

    public class SynergyReinforceStatData
    {
        public ObscuredInt SynergyReinforce;
        public List<CreatureBaseStatElement> EffectParam_Stat = new List<CreatureBaseStatElement>();

        public Dictionary<V2Enum_Stat, ObscuredDouble> AccEffectStat = new Dictionary<V2Enum_Stat, ObscuredDouble>();
    }

    public class SynergyLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_ARR_SynergyType, SynergyData> _gambleSynergyData_Dic = new Dictionary<V2Enum_ARR_SynergyType, SynergyData>();

        private Dictionary<V2Enum_ARR_SynergyType, List<SynergyEffectData>> _gambleSynergyEffectDataList_Dic = new Dictionary<V2Enum_ARR_SynergyType, List<SynergyEffectData>>();

        private Dictionary<ObscuredInt, SynergyEffectData> _synergyEffectDatas_Dic = new Dictionary<ObscuredInt, SynergyEffectData>();

        private Dictionary<V2Enum_Grade, SynergyLevelUpCostData> _synergyLevelUpCost_Dic = new Dictionary<V2Enum_Grade, SynergyLevelUpCostData>();

        private Dictionary<V2Enum_ARR_SynergyType, Dictionary<ObscuredInt, SynergyBreakthroughCostData>> _synergyLevelUpLimit_Dic = new Dictionary<V2Enum_ARR_SynergyType, Dictionary<ObscuredInt, SynergyBreakthroughCostData>>();

        private List<SynergyCombineData> _gambleSynergyCombineDatas = new List<SynergyCombineData>();

        private Dictionary<V2Enum_ARR_SynergyType, Dictionary<V2Enum_Grade, SynergyDuplicationData>> _synergyDuplicationData_Dic = new Dictionary<V2Enum_ARR_SynergyType, Dictionary<V2Enum_Grade, SynergyDuplicationData>>();

        private Dictionary<ObscuredInt, SynergyBreakthroughData> _synergyBreakthroughList_Dic = new Dictionary<ObscuredInt, SynergyBreakthroughData>();

        Dictionary<V2Enum_Grade, SynergyDuplicationData> _synergyRuneDuplicationData_Dic = new Dictionary<V2Enum_Grade, SynergyDuplicationData>();

        private Dictionary<ObscuredInt, SynergyReinforceStatData> _synergyReinforceStatData_Dic = new Dictionary<ObscuredInt, SynergyReinforceStatData>();
        private SynergyReinforceStatData _maxSynergyReinforceStatData = null;

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Synergy", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int index = rows[i]["Index"].ToString().ToInt();
                if (index == -1)
                    continue;

                SynergyData gambleCardData = new SynergyData();

                gambleCardData.Index = index;

                gambleCardData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                gambleCardData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_SynergyType>();

                if (_gambleSynergyData_Dic.ContainsKey(gambleCardData.SynergyType) == false)
                    _gambleSynergyData_Dic.Add(gambleCardData.SynergyType, gambleCardData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyEffect", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int index = rows[i]["Index"].ToString().ToInt();
                if (index == -1)
                    continue;

                SynergyEffectData synergyEffectData = new SynergyEffectData();

                synergyEffectData.Index = index;
                synergyEffectData.MainSkillIndex = rows[i]["MainSkillIndex"].ToString().ToInt();

                synergyEffectData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_SynergyType>();
                synergyEffectData.SynergyTier = rows[i]["SynergyTier"].ToString().ToInt();
                synergyEffectData.SynergyOriginCount = rows[i]["SynergyCount"].ToString().ToInt();
                synergyEffectData.SynergyCount = synergyEffectData.SynergyOriginCount;

                if (_gambleSynergyEffectDataList_Dic.ContainsKey(synergyEffectData.SynergyType) == false)
                    _gambleSynergyEffectDataList_Dic.Add(synergyEffectData.SynergyType, new List<SynergyEffectData>());

                _gambleSynergyEffectDataList_Dic[synergyEffectData.SynergyType].Add(synergyEffectData);

                if (_gambleSynergyData_Dic.ContainsKey(synergyEffectData.SynergyType) == true)
                {
                    SynergyData synergyData = _gambleSynergyData_Dic[synergyEffectData.SynergyType];
                    if (synergyData.TierDatas.ContainsKey(synergyEffectData.SynergyTier) == false)
                        synergyData.TierDatas.Add(synergyEffectData.SynergyTier, new List<SynergyEffectData>());

                    synergyData.TierDatas[synergyEffectData.SynergyTier].Add(synergyEffectData);
                }

                if (_synergyEffectDatas_Dic.ContainsKey(synergyEffectData.Index) == false)
                    _synergyEffectDatas_Dic.Add(synergyEffectData.Index, synergyEffectData);
            }

            foreach (var pair in _gambleSynergyEffectDataList_Dic)
            {
                pair.Value.Sort((x, y) =>
                {
                    if (x.SynergyTier < y.SynergyTier)
                        return -1;
                    else if (x.SynergyTier > y.SynergyTier)
                        return 1;
                    else
                    {
                        if (x.Index < y.Index)
                            return -1;
                        else if (x.Index > y.Index)
                            return 1;
                    }

                    return 0;
                });

                SynergyEffectData prevEffect = null;

                for (int i = 0; i < pair.Value.Count; ++i)
                {
                    SynergyEffectData gambleSynergyEffectData = pair.Value[i];
                    if (prevEffect != null)
                        prevEffect.NextEffectData = gambleSynergyEffectData;

                    prevEffect = gambleSynergyEffectData;
                }
            }

            foreach (var pair2 in _gambleSynergyData_Dic)
            {
                foreach (var pair in pair2.Value.TierDatas)
                {
                    pair.Value.Sort((x, y) =>
                    {
                        if (x.SynergyTier < y.SynergyTier)
                            return -1;
                        else if (x.SynergyTier > y.SynergyTier)
                            return 1;
                        else
                        {
                            if (x.Index < y.Index)
                                return -1;
                            else if (x.Index > y.Index)
                                return 1;
                        }

                        return 0;
                    });
                }
            }







            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyBreakthrough", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int synergySkillIndex = rows[i]["SynergySkillIndex"].ToString().ToInt();

                SynergyBreakthroughData synergyRuneData = new SynergyBreakthroughData();
                synergyRuneData.Index = rows[i]["Index"].ToString().ToInt();

                synergyRuneData.SynergySkillIndex = synergySkillIndex;
                synergyRuneData.Procedure = rows[i]["Procedure"].ToString().ToInt();

                synergyRuneData.MainSkillIndex = rows[i]["MainSkillIndex"].ToString().ToInt();

                synergyRuneData.IconIndex = rows[i]["IconIndex"].ToString().ToInt();
                //synergyRuneData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();
                synergyRuneData.Grade = V2Enum_Grade.Max;

                synergyRuneData.NameLocalKey = string.Format("skillname/{0}", synergyRuneData.Index);
                synergyRuneData.DescLocalKey = string.Format("skilldesc/{0}", synergyRuneData.Index);

                if (_synergyEffectDatas_Dic.ContainsKey(synergySkillIndex) == true)
                {
                    SynergyEffectData synergyEffectData = _synergyEffectDatas_Dic[synergySkillIndex];
                    synergyEffectData.SynergyRuneList.Add(synergyRuneData);
                }

                _synergyBreakthroughList_Dic.Add(synergyRuneData.Index, synergyRuneData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyLevelUpCostData synergyLevelEffectData = new SynergyLevelUpCostData();

                synergyLevelEffectData.Index = rows[i]["Index"].ToString().ToInt();

                synergyLevelEffectData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();
                synergyLevelEffectData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();

                synergyLevelEffectData.SkillLevelUpCostGoodsIndex1 = rows[i]["SkillLevelUpCostGoodsIndex1"].ToString().ToInt();

                synergyLevelEffectData.SkillLevelUpCostGoodsParam11 = rows[i]["SkillLevelUpCostGoodsParam11"].ToString().ToInt();
                synergyLevelEffectData.SkillLevelUpCostGoodsParam12 = rows[i]["SkillLevelUpCostGoodsParam12"].ToString().ToInt();


                synergyLevelEffectData.SkillLevelUpCostGoodsIndex2 = rows[i]["SkillLevelUpCostGoodsIndex2"].ToString().ToInt();

                synergyLevelEffectData.SkillLevelUpCostGoodsParam21 = rows[i]["SkillLevelUpCostGoodsParam21"].ToString().ToInt();
                synergyLevelEffectData.SkillLevelUpCostGoodsParam22 = rows[i]["SkillLevelUpCostGoodsParam22"].ToString().ToInt();


                if (_synergyLevelUpCost_Dic.ContainsKey(synergyLevelEffectData.Grade) == false)
                    _synergyLevelUpCost_Dic.Add(synergyLevelEffectData.Grade, synergyLevelEffectData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyCombine", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int index = rows[i]["Index"].ToString().ToInt();
                if (index == -1)
                    continue;

                SynergyCombineData gambleCardData = new SynergyCombineData();

                gambleCardData.Index = index;
                gambleCardData.MainSkillIndex = rows[i]["MainSkillIndex"].ToString().ToInt();

                for (int j = 1; j <= 4; ++j)
                {
                    try
                    {
                        string SynergyType = string.Format("SynergyType{0}", j);
                        int SynergyTypeData = rows[i][SynergyType].ToString().ToInt();
                        if (SynergyTypeData == -1 || SynergyTypeData == 0)
                            continue;

                        string RequiredSynergyCount = string.Format("RequiredSynergyCount{0}", j);
                        int RequiredSynergyCountData = rows[i][RequiredSynergyCount].ToString().ToInt();
                        if (RequiredSynergyCountData == -1 || RequiredSynergyCountData == 0)
                            continue;

                        V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType = SynergyTypeData.IntToEnum32<V2Enum_ARR_SynergyType>();

                        if (gambleCardData.NeedSynergyCount.ContainsKey(v2Enum_ARR_GambleSynergyType) == false)
                            gambleCardData.NeedSynergyCount.Add(v2Enum_ARR_GambleSynergyType, RequiredSynergyCountData);
                    }
                    catch
                    {

                    }
                }

                _gambleSynergyCombineDatas.Add(gambleCardData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyCondition", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyConditionData synergyConditionData = new SynergyConditionData();

                synergyConditionData.Index = rows[i]["Index"].ToString().ToInt();

                synergyConditionData.SynergySkillIndex = rows[i]["SynergySkillIndex"].ToString().ToInt();

                synergyConditionData.RequiredTier1Reinforce = rows[i]["RequiredTier1Reinforce"].ToString().ToInt();
                synergyConditionData.RequiredTier2Reinforce = rows[i]["RequiredTier2Reinforce"].ToString().ToInt();
                synergyConditionData.RequiredTier3Reinforce = rows[i]["RequiredTier3Reinforce"].ToString().ToInt();
                synergyConditionData.RequiredTier4Reinforce = rows[i]["RequiredTier4Reinforce"].ToString().ToInt();

                if (_synergyEffectDatas_Dic.ContainsKey(synergyConditionData.SynergySkillIndex) == true)
                {
                    SynergyEffectData synergyEffectData = _synergyEffectDatas_Dic[synergyConditionData.SynergySkillIndex];
                    synergyEffectData.SynergyConditionData = synergyConditionData;
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyBreakthroughCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyBreakthroughCostData synergyLevelUpLimitData = new SynergyBreakthroughCostData();

                synergyLevelUpLimitData.Index = rows[i]["Index"].ToString().ToInt();

                synergyLevelUpLimitData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_SynergyType>();
                
                synergyLevelUpLimitData.Procedure = rows[i]["Procedure"].ToString().ToInt();

                synergyLevelUpLimitData.LimitBreakCostGoodsIndex = rows[i]["LimitBreakCostGoodsIndex"].ToString().ToInt();
                synergyLevelUpLimitData.LimitBreakCostGoodsValue = rows[i]["LimitBreakCostGoodsValue"].ToString().ToInt();

                if (_synergyLevelUpLimit_Dic.ContainsKey(synergyLevelUpLimitData.SynergyType) == false)
                    _synergyLevelUpLimit_Dic.Add(synergyLevelUpLimitData.SynergyType, new Dictionary<ObscuredInt, SynergyBreakthroughCostData>());

                if (_synergyLevelUpLimit_Dic[synergyLevelUpLimitData.SynergyType].ContainsKey(synergyLevelUpLimitData.Procedure) == false)
                    _synergyLevelUpLimit_Dic[synergyLevelUpLimitData.SynergyType].Add(synergyLevelUpLimitData.Procedure, synergyLevelUpLimitData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyDuplication", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyDuplicationData synergyDuplicationData = new SynergyDuplicationData();

                synergyDuplicationData.Index = rows[i]["Index"].ToString().ToInt();

                synergyDuplicationData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_SynergyType>();
                
                synergyDuplicationData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                synergyDuplicationData.DuplicationGoodsIndex = rows[i]["DuplicationGoodsIndex"].ToString().ToInt();
                synergyDuplicationData.DuplicationGoodsValue = rows[i]["DuplicationGoodsValue"].ToString().ToDouble();

                if (_synergyDuplicationData_Dic.ContainsKey(synergyDuplicationData.SynergyType) == false)
                    _synergyDuplicationData_Dic.Add(synergyDuplicationData.SynergyType, new Dictionary<V2Enum_Grade, SynergyDuplicationData>());

                if (_synergyDuplicationData_Dic[synergyDuplicationData.SynergyType].ContainsKey(synergyDuplicationData.Grade) == false)
                    _synergyDuplicationData_Dic[synergyDuplicationData.SynergyType].Add(synergyDuplicationData.Grade, synergyDuplicationData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyTotalLevelCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyContainer.SynergyTotalLevelCostData.Index = rows[i]["Index"].ToString().ToInt();

                SynergyContainer.SynergyTotalLevelCostData.CostGoodsIndex = rows[i]["CostGoodsIndex"].ToString().ToInt();
                SynergyContainer.SynergyTotalLevelCostData.GainExp = rows[i]["GainExp"].ToString().ToInt();

                break;
            }







            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyRuneDuplication", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyDuplicationData synergyDuplicationData = new SynergyDuplicationData();

                synergyDuplicationData.Index = rows[i]["Index"].ToString().ToInt();

                synergyDuplicationData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                synergyDuplicationData.DuplicationGoodsIndex = rows[i]["DuplicationGoodsIndex"].ToString().ToInt();
                synergyDuplicationData.DuplicationGoodsValue = rows[i]["DuplicationGoodsValue"].ToString().ToDouble();

                if (_synergyRuneDuplicationData_Dic.ContainsKey(synergyDuplicationData.Grade) == false)
                    _synergyRuneDuplicationData_Dic.Add(synergyDuplicationData.Grade, synergyDuplicationData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SynergyReinforceStat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            string IncreaseAtt = "IncreaseAtt";
            string IncreaseHP = "IncreaseHP";
            string IncreaseDef = "IncreaseDef";

            SynergyReinforceStatData prevStat = null;

            for (int i = 0; i < rows.Count; ++i)
            {
                SynergyReinforceStatData synergyDuplicationData = new SynergyReinforceStatData();

                synergyDuplicationData.SynergyReinforce = rows[i]["SynergyReinforce"].ToString().ToInt();

                if (rows[i].ContainsKey(IncreaseAtt) == true)
                {
                    CreatureBaseStatElement creatureBaseStatElement = new CreatureBaseStatElement();
                    creatureBaseStatElement.BaseStat = V2Enum_Stat.Attack;
                    creatureBaseStatElement.BaseValue = rows[i][IncreaseAtt].ToString().ToDouble();

                    synergyDuplicationData.EffectParam_Stat.Add(creatureBaseStatElement);

                }

                if (rows[i].ContainsKey(IncreaseHP) == true)
                {
                    CreatureBaseStatElement creatureBaseStatElement = new CreatureBaseStatElement();
                    creatureBaseStatElement.BaseStat = V2Enum_Stat.HP;
                    creatureBaseStatElement.BaseValue = rows[i][IncreaseHP].ToString().ToDouble();

                    synergyDuplicationData.EffectParam_Stat.Add(creatureBaseStatElement);
                }

                if (rows[i].ContainsKey(IncreaseDef) == true)
                {
                    CreatureBaseStatElement creatureBaseStatElement = new CreatureBaseStatElement();
                    creatureBaseStatElement.BaseStat = V2Enum_Stat.Defence;
                    creatureBaseStatElement.BaseValue = rows[i][IncreaseDef].ToString().ToDouble();

                    synergyDuplicationData.EffectParam_Stat.Add(creatureBaseStatElement);
                }

                for (int stat = 0; stat < synergyDuplicationData.EffectParam_Stat.Count; ++stat)
                {
                    CreatureBaseStatElement creatureBaseStatElement = synergyDuplicationData.EffectParam_Stat[stat];

                    double accvalue = 0;
                    if (prevStat != null)
                    {
                        if (prevStat.AccEffectStat.ContainsKey(creatureBaseStatElement.BaseStat) == true)
                            accvalue = prevStat.AccEffectStat[creatureBaseStatElement.BaseStat];
                    }

                    synergyDuplicationData.AccEffectStat.Add(creatureBaseStatElement.BaseStat, accvalue + creatureBaseStatElement.BaseValue);
                }

                if (_synergyReinforceStatData_Dic.ContainsKey(synergyDuplicationData.SynergyReinforce) == false)
                    _synergyReinforceStatData_Dic.Add(synergyDuplicationData.SynergyReinforce, synergyDuplicationData);

                if (_maxSynergyReinforceStatData == null)
                    _maxSynergyReinforceStatData = synergyDuplicationData;
                else
                {
                    if (_maxSynergyReinforceStatData.SynergyReinforce < synergyDuplicationData.SynergyReinforce)
                        _maxSynergyReinforceStatData = synergyDuplicationData;
                }

                prevStat = synergyDuplicationData;
            }
        }
        //------------------------------------------------------------------------------------
        private CreatureBaseStatElement ConvertStringToBaseStat(List<string> EffectParam)
        {
            if (EffectParam.Count >= 2)
            {
                CreatureBaseStatElement creatureBaseStatElement = new CreatureBaseStatElement();
                creatureBaseStatElement.BaseStat = EffectParam[0].ToInt().IntToEnum32<V2Enum_Stat>();
                creatureBaseStatElement.BaseValue = EffectParam[1].ToDouble();

                return creatureBaseStatElement;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<V2Enum_ARR_SynergyType, SynergyData> GetAllGambleSynergyData()
        {
            return _gambleSynergyData_Dic;
        }
        //------------------------------------------------------------------------------------
        public SynergyData GetGambleSynergyData(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType)
        {
            if (_gambleSynergyData_Dic.ContainsKey(v2Enum_ARR_GambleSynergyType) == true)
                return _gambleSynergyData_Dic[v2Enum_ARR_GambleSynergyType];
            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SynergyEffectData> GetAllSynergyEffectDatas()
        {
            return _synergyEffectDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public SynergyEffectData GetSynergyEffectData(ObscuredInt index)
        {
            if (_synergyEffectDatas_Dic.ContainsKey(index) == true)
                return _synergyEffectDatas_Dic[index];
            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<V2Enum_ARR_SynergyType, List<SynergyEffectData>> GetAllGambleSynergyEffectDataList()
        {
            return _gambleSynergyEffectDataList_Dic;
        }
        //------------------------------------------------------------------------------------
        public List<SynergyEffectData> GetGambleSynergyEffectDataList(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType)
        {
            if (_gambleSynergyEffectDataList_Dic.ContainsKey(v2Enum_ARR_GambleSynergyType) == true)
                return _gambleSynergyEffectDataList_Dic[v2Enum_ARR_GambleSynergyType];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SynergyLevelUpCostData GetSynergyLevelUpCostData(V2Enum_Grade v2Enum_Grade)
        {
            if (_synergyLevelUpCost_Dic.ContainsKey(v2Enum_Grade) == true)
                return _synergyLevelUpCost_Dic[v2Enum_Grade];
            return null;
        }
        //------------------------------------------------------------------------------------
        public List<SynergyCombineData> GetAllGambleSynergyCombineData()
        {
            return _gambleSynergyCombineDatas;
        }
        //------------------------------------------------------------------------------------
        public SynergyBreakthroughCostData GetSynergyLevelUpLimitData(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType, ObscuredInt level)
        {
            if (_synergyLevelUpLimit_Dic.ContainsKey(v2Enum_ARR_GambleSynergyType) == false)
                return null;

            if (_synergyLevelUpLimit_Dic[v2Enum_ARR_GambleSynergyType].ContainsKey(level) == false)
                return null;

            return _synergyLevelUpLimit_Dic[v2Enum_ARR_GambleSynergyType][level];
        }
        //------------------------------------------------------------------------------------
        //public Dictionary<ObscuredInt, SynergyLevelUpLimitData> GetAllSynergyGradeLevelUpLimitData(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType, V2Enum_Grade v2Enum_Grade)
        //{
        //    if (_synergyLevelUpLimit_Dic.ContainsKey(v2Enum_ARR_GambleSynergyType) == false)
        //        return null;

        //    if (_synergyLevelUpLimit_Dic[v2Enum_ARR_GambleSynergyType].ContainsKey(v2Enum_Grade) == true)
        //        return _synergyLevelUpLimit_Dic[v2Enum_ARR_GambleSynergyType][v2Enum_Grade];

        //    return null;
        //}
        //------------------------------------------------------------------------------------
        public SynergyDuplicationData GetSynergyDuplicationData(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, V2Enum_Grade v2Enum_Grade)
        {
            if (_synergyDuplicationData_Dic.ContainsKey(v2Enum_ARR_SynergyType) == false)
                return null;

            if (_synergyDuplicationData_Dic[v2Enum_ARR_SynergyType].ContainsKey(v2Enum_Grade) == false)
                return null;

            return _synergyDuplicationData_Dic[v2Enum_ARR_SynergyType][v2Enum_Grade];
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SynergyBreakthroughData> GetAllSynergyRuneData()
        {
            return _synergyBreakthroughList_Dic;
        }
        //------------------------------------------------------------------------------------
        public SynergyBreakthroughData GetSynergyRuneData(ObscuredInt index)
        {
            if (_synergyBreakthroughList_Dic.ContainsKey(index) == true)
                return _synergyBreakthroughList_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SynergyDuplicationData GetSynergyRuneDuplicationData(V2Enum_Grade v2Enum_Grade)
        {
            if (_synergyRuneDuplicationData_Dic.ContainsKey(v2Enum_Grade) == false)
                return null;

            return _synergyRuneDuplicationData_Dic[v2Enum_Grade];
        }
        //------------------------------------------------------------------------------------
        public SynergyReinforceStatData GetSynergyReinforceStatData(ObscuredInt level)
        {
            if (_maxSynergyReinforceStatData != null)
            {
                if (_maxSynergyReinforceStatData.SynergyReinforce < level)
                    return _maxSynergyReinforceStatData;
            }

            if (_synergyReinforceStatData_Dic.ContainsKey(level) == false)
                return null;

            return _synergyReinforceStatData_Dic[level];
        }
        //------------------------------------------------------------------------------------
    }
}