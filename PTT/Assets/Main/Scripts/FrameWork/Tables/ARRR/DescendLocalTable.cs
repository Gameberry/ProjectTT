using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;

namespace GameBerry
{
    public class DescendData
    {
        public ObscuredInt Index;

        public V2Enum_Grade Grade;

        public Enum_DescendType DescendType;

        public ObscuredInt DescendParam1;
        public ObscuredInt DescendIconIndex;

        public MainSkillData SynergySkillData;
        public PetData PetData;

        public Dictionary<ObscuredInt, ObscuredInt> DescendPhaseIndexList = new Dictionary<ObscuredInt, ObscuredInt>();

        public HashSet<Enum_SynergyType> SynergyType = new HashSet<Enum_SynergyType>();

        //public Dictionary<Enum_SynergyType, ObscuredInt> NeedSynergyCount = new Dictionary<Enum_SynergyType, ObscuredInt>();

        public ObscuredDouble OriginNeedRequiredExp;
        public ObscuredDouble NeedRequiredExp;

        public List<DescendBreakthroughData> SynergyRuneList = new List<DescendBreakthroughData>();

        public DescendOpenCostData DescendOpenCostData;

        public string NameLocalKey;
        public string DescLocalKey;
    }

    public class DescendLevelUpCostData
    {
        public ObscuredInt Index;

        public ObscuredInt DescendLevel;

        public ObscuredInt DescendLevelUpCostGoodsIndex;
        public ObscuredInt DescendLevelUpCostGoodParam1;

        public ObscuredInt DescendLevelUpCostGoodsIndex2;
        public ObscuredInt DescendLevelUpCostGoodParam2;
    }

    public class DescendBreakthroughData
    {
        public ObscuredInt Index;

        public ObscuredInt DecendSkillIndex;
        public ObscuredInt Procedure;

        public ObscuredInt MainSkillIndex;

        public MainSkillData SynergySkillData;

        public ObscuredInt IconIndex;

        public string NameLocalKey;
        public string DescLocalKey;
    }

    public class DescendBreakthroughCostData
    {
        public ObscuredInt Index;

        public ObscuredInt Procedure;

        public ObscuredInt LimitBreakCostGoodsIndex;
        public ObscuredInt LimitBreakCostGoodsValue;
    }

    public class DescendDuplicationData
    {
        public ObscuredInt Index;

        public ObscuredInt DescendIndex;

        public ObscuredInt DuplicationGoodsIndex;
        public ObscuredDouble DuplicationGoodsValue;
    }

    public class DescendOpenCostData
    {
        public ObscuredInt Index;

        public ObscuredInt DescendIndex;

        public ObscuredInt OpenCostGoodsIndex;
        public ObscuredDouble OpenCostGoodsValue;
    }

    public class DescendLevelUpStatData
    {
        public ObscuredInt DescendLevel;
        public List<CreatureBaseStatElement> EffectParam_Stat = new List<CreatureBaseStatElement>();

        public Dictionary<V2Enum_Stat, ObscuredDouble> AccEffectStat = new Dictionary<V2Enum_Stat, ObscuredDouble>();
    }

    public class DescendIngameEnforceData
    {
        public ObscuredDouble DmgBoost;

        public ObscuredInt EnforceCostGoodsIndex;
        public ObscuredDouble EnforceCostBaseValue;
        public ObscuredDouble EnforceCostLevelValue;
    }

    public class DescendSlotConditionData
    {
        public ObscuredInt SlotNumber;

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionValue;
    }

    public class DescendLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, ObscuredDouble> _descendPhaseValue_Dic = new Dictionary<ObscuredInt, ObscuredDouble>();

        private Dictionary<ObscuredInt, DescendData> _descendEffectDatas_Dic = new Dictionary<ObscuredInt, DescendData>();
        private Dictionary<ObscuredInt, DescendLevelUpCostData> _descendLevelUpCost_Dic = new Dictionary<ObscuredInt, DescendLevelUpCostData>();

        private Dictionary<ObscuredInt, DescendBreakthroughData> _synergyBreakthroughList_Dic = new Dictionary<ObscuredInt, DescendBreakthroughData>();
        private Dictionary<ObscuredInt, DescendBreakthroughCostData> _descendLevelUpLimit_Dic = new Dictionary<ObscuredInt, DescendBreakthroughCostData>();
        private Dictionary<ObscuredInt, DescendDuplicationData> _descendDuplicationData_Dic = new Dictionary<ObscuredInt, DescendDuplicationData>();

        private Dictionary<ObscuredInt, DescendLevelUpStatData> _synergyReinforceStatData_Dic = new Dictionary<ObscuredInt, DescendLevelUpStatData>();
        private DescendLevelUpStatData _maxSynergyReinforceStatData = null;

        private Dictionary<ObscuredInt, DescendSlotConditionData> _descendSlotConditionData_Dic = new Dictionary<ObscuredInt, DescendSlotConditionData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Descend", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int index = rows[i]["Index"].ToString().ToInt();
                if (index == -1)
                    continue;

                DescendData descendData = new DescendData();

                descendData.Index = index;

                descendData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                descendData.DescendType = rows[i]["DescendType"].ToString().ToInt().IntToEnum32<Enum_DescendType>();
                descendData.DescendParam1 = rows[i]["DescendParam1"].ToString().ToInt();
                descendData.DescendIconIndex = rows[i]["DescendIconIndex"].ToString().ToInt();

                descendData.NameLocalKey = string.Format("descendname/{0}", descendData.Index);
                descendData.DescLocalKey = string.Format("descenddesc/{0}", descendData.Index);

                for (int j = 1; j <= 5; ++j)
                {
                    try
                    {
                        string SynergyType = string.Format("DescendPhaseIndex{0}", j);
                        if (rows[i].ContainsKey(SynergyType) == false)
                            continue;

                        int SynergyTypeData = rows[i][SynergyType].ToString().ToInt();

                        descendData.DescendPhaseIndexList.Add(j, SynergyTypeData);
                    }
                    catch
                    {

                    }
                }

                //for (int j = 1; j <= 4; ++j)
                //{
                //    try
                //    {
                //        string SynergyType = string.Format("SynergyType{0}", j);
                //        int SynergyTypeData = rows[i][SynergyType].ToString().ToInt();
                //        if (SynergyTypeData == -1 || SynergyTypeData == 0)
                //            continue;

                //        Enum_SynergyType Enum_GambleSynergyType = SynergyTypeData.IntToEnum32<Enum_SynergyType>();

                //        if (descendData.SynergyType.Contains(Enum_GambleSynergyType) == false)
                //            descendData.SynergyType.Add(Enum_GambleSynergyType);
                //    }
                //    catch
                //    {

                //    }
                //}

                //descendData.OriginNeedRequiredExp = rows[i]["RequiredExp"].ToString().ToInt();

                if (_descendEffectDatas_Dic.ContainsKey(descendData.Index) == false)
                    _descendEffectDatas_Dic.Add(descendData.Index, descendData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendLevelUpCostData _descendLevelUpCostData = new DescendLevelUpCostData();

                _descendLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                _descendLevelUpCostData.DescendLevel = rows[i]["DescendLevel"].ToString().ToInt();

                _descendLevelUpCostData.DescendLevelUpCostGoodsIndex = rows[i]["DescendLevelUpCostGoodsIndex"].ToString().ToInt();
                _descendLevelUpCostData.DescendLevelUpCostGoodParam1 = rows[i]["DescendLevelUpCostGoodParam1"].ToString().ToInt();


                _descendLevelUpCostData.DescendLevelUpCostGoodsIndex2 = rows[i]["DescendLevelUpCostGoodsIndex2"].ToString().ToInt();
                _descendLevelUpCostData.DescendLevelUpCostGoodParam2 = rows[i]["DescendLevelUpCostGoodParam2"].ToString().ToInt();


                if (_descendLevelUpCost_Dic.ContainsKey(_descendLevelUpCostData.DescendLevel) == false)
                    _descendLevelUpCost_Dic.Add(_descendLevelUpCostData.DescendLevel, _descendLevelUpCostData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendBreakthrough", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendBreakthroughData synergyLevelEffectData = new DescendBreakthroughData();

                synergyLevelEffectData.Index = rows[i]["Index"].ToString().ToInt();

                synergyLevelEffectData.DecendSkillIndex = rows[i]["DecendSkillIndex"].ToString().ToInt();
                synergyLevelEffectData.Procedure = rows[i]["Procedure"].ToString().ToInt();
                synergyLevelEffectData.MainSkillIndex = rows[i]["MainSkillIndex"].ToString().ToInt();

                synergyLevelEffectData.IconIndex = rows[i]["IconIndex"].ToString().ToInt();

                synergyLevelEffectData.NameLocalKey = string.Format("skillname/{0}", synergyLevelEffectData.Index);
                synergyLevelEffectData.DescLocalKey = string.Format("skilldesc/{0}", synergyLevelEffectData.Index);

                if (_descendEffectDatas_Dic.ContainsKey(synergyLevelEffectData.DecendSkillIndex) == true)
                {
                    DescendData synergyEffectData = _descendEffectDatas_Dic[synergyLevelEffectData.DecendSkillIndex];
                    if (synergyEffectData.SynergyRuneList == null)
                        synergyEffectData.SynergyRuneList = new List<DescendBreakthroughData>();

                    synergyEffectData.SynergyRuneList.Add(synergyLevelEffectData);
                }

                _synergyBreakthroughList_Dic.Add(synergyLevelEffectData.Index, synergyLevelEffectData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendBreakthroughCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendBreakthroughCostData synergyLevelUpLimitData = new DescendBreakthroughCostData();

                synergyLevelUpLimitData.Index = rows[i]["Index"].ToString().ToInt();

                synergyLevelUpLimitData.Procedure = rows[i]["Procedure"].ToString().ToInt();

                synergyLevelUpLimitData.LimitBreakCostGoodsIndex = rows[i]["LimitBreakCostGoodsIndex"].ToString().ToInt();
                synergyLevelUpLimitData.LimitBreakCostGoodsValue = rows[i]["LimitBreakCostGoodsValue"].ToString().ToInt();


                if (_descendLevelUpLimit_Dic.ContainsKey(synergyLevelUpLimitData.Procedure) == false)
                    _descendLevelUpLimit_Dic.Add(synergyLevelUpLimitData.Procedure, synergyLevelUpLimitData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendDuplication", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendDuplicationData synergyDuplicationData = new DescendDuplicationData();

                synergyDuplicationData.Index = rows[i]["Index"].ToString().ToInt();
                synergyDuplicationData.DescendIndex = rows[i]["DescendIndex"].ToString().ToInt();

                synergyDuplicationData.DuplicationGoodsIndex = rows[i]["DuplicationGoodsIndex"].ToString().ToInt();
                synergyDuplicationData.DuplicationGoodsValue = rows[i]["DuplicationGoodsValue"].ToString().ToDouble();

                if (_descendDuplicationData_Dic.ContainsKey(synergyDuplicationData.DescendIndex) == false)
                    _descendDuplicationData_Dic.Add(synergyDuplicationData.DescendIndex, synergyDuplicationData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendOpenCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendOpenCostData descendOpenCostData = new DescendOpenCostData();

                descendOpenCostData.Index = rows[i]["Index"].ToString().ToInt();
                descendOpenCostData.DescendIndex = rows[i]["DescendIndex"].ToString().ToInt();

                descendOpenCostData.OpenCostGoodsIndex = rows[i]["OpenCostGoodsIndex"].ToString().ToInt();
                descendOpenCostData.OpenCostGoodsValue = rows[i]["OpenCostGoodsValue"].ToString().ToDouble();

                if (_descendEffectDatas_Dic.ContainsKey(descendOpenCostData.DescendIndex) == true)
                {
                    DescendData synergyEffectData = _descendEffectDatas_Dic[descendOpenCostData.DescendIndex];
                    synergyEffectData.DescendOpenCostData = descendOpenCostData;
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendTotalLevelCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendContainer.SynergyTotalLevelCostData.Index = rows[i]["Index"].ToString().ToInt();

                DescendContainer.SynergyTotalLevelCostData.CostGoodsIndex = rows[i]["CostGoodsIndex"].ToString().ToInt();
                DescendContainer.SynergyTotalLevelCostData.GainExp = rows[i]["GainExp"].ToString().ToInt();

                break;
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendLevelUpStat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            string IncreaseAtt = "IncreaseAtt";
            string IncreaseHP = "IncreaseHP";
            string IncreaseDef = "IncreaseDef";

            DescendLevelUpStatData prevStat = null;

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendLevelUpStatData synergyDuplicationData = new DescendLevelUpStatData();

                synergyDuplicationData.DescendLevel = rows[i]["DescendLevel"].ToString().ToInt();

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

                if (_synergyReinforceStatData_Dic.ContainsKey(synergyDuplicationData.DescendLevel) == false)
                    _synergyReinforceStatData_Dic.Add(synergyDuplicationData.DescendLevel, synergyDuplicationData);

                if (_maxSynergyReinforceStatData == null)
                    _maxSynergyReinforceStatData = synergyDuplicationData;
                else
                {
                    if (_maxSynergyReinforceStatData.DescendLevel < synergyDuplicationData.DescendLevel)
                        _maxSynergyReinforceStatData = synergyDuplicationData;
                }

                prevStat = synergyDuplicationData;
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendIngameEnforce", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendContainer.DescendIngameEnforceData.DmgBoost = rows[i]["DmgBoost"].ToString().ToDouble();

                DescendContainer.DescendIngameEnforceData.EnforceCostGoodsIndex = rows[i]["EnforceCostGoodsIndex"].ToString().ToInt();
                DescendContainer.DescendIngameEnforceData.EnforceCostBaseValue = rows[i]["EnforceCostBaseValue"].ToString().ToDouble();
                DescendContainer.DescendIngameEnforceData.EnforceCostLevelValue = rows[i]["EnforceCostLevelValue"].ToString().ToDouble();

                break;
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("DescendSlotCondition", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DescendSlotConditionData descendOpenCostData = new DescendSlotConditionData();
                descendOpenCostData.SlotNumber = rows[i]["SlotNumber"].ToString().ToInt();

                descendOpenCostData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                descendOpenCostData.OpenConditionValue = rows[i]["OpenConditionValue"].ToString().ToInt();

                if (_descendSlotConditionData_Dic.ContainsKey(descendOpenCostData.SlotNumber) == false)
                    _descendSlotConditionData_Dic.Add(descendOpenCostData.SlotNumber, descendOpenCostData);
            }

        }
        //------------------------------------------------------------------------------------
        public double GetDescendPhaseValue(ObscuredInt level)
        {
            if (_descendPhaseValue_Dic.ContainsKey(level) == false)
                return 0.0;

            return _descendPhaseValue_Dic[level];
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, DescendData> GetAllSynergyEffectDatas()
        {
            return _descendEffectDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public DescendData GetSynergyEffectData(ObscuredInt index)
        {
            if (_descendEffectDatas_Dic.ContainsKey(index) == true)
                return _descendEffectDatas_Dic[index];
            return null;
        }
        //------------------------------------------------------------------------------------
        public DescendLevelUpCostData GetSynergyLevelUpCostData(ObscuredInt level)
        {
            if (_descendLevelUpCost_Dic.ContainsKey(level) == false)
                return null;

            return _descendLevelUpCost_Dic[level];
        }
        //------------------------------------------------------------------------------------
        public DescendBreakthroughCostData GetSynergyLevelUpLimitData(ObscuredInt level)
        {
            if (_descendLevelUpLimit_Dic.ContainsKey(level) == false)
                return null;

            return _descendLevelUpLimit_Dic[level];
        }
        //------------------------------------------------------------------------------------
        public DescendBreakthroughData GetSynergyRuneData(ObscuredInt index)
        {
            if (_synergyBreakthroughList_Dic.ContainsKey(index) == true)
                return _synergyBreakthroughList_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public DescendDuplicationData GetSynergyDuplicationData(ObscuredInt index)
        {
            if (_descendDuplicationData_Dic.ContainsKey(index) == false)
                return null;

            return _descendDuplicationData_Dic[index];
        }
        //------------------------------------------------------------------------------------
        public DescendLevelUpStatData GetSynergyReinforceStatData(ObscuredInt level)
        {
            if (_maxSynergyReinforceStatData != null)
            {
                if (_maxSynergyReinforceStatData.DescendLevel < level)
                    return _maxSynergyReinforceStatData;
            }

            if (_synergyReinforceStatData_Dic.ContainsKey(level) == false)
                return null;

            return _synergyReinforceStatData_Dic[level];
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, DescendSlotConditionData> GetAllDescendSlotConditionDatas()
        {
            return _descendSlotConditionData_Dic;
        }
        //------------------------------------------------------------------------------------
        public DescendSlotConditionData GetDescendSlotConditionData(ObscuredInt slot)
        {
            if (_descendSlotConditionData_Dic.ContainsKey(slot) == true)
                return _descendSlotConditionData_Dic[slot];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}