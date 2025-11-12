using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using CodeStage.AntiCheat.ObscuredTypes;
using Gpm.Ui;

namespace GameBerry
{
    public class CharacterLevelUpCostData
    {
        public ObscuredInt Index;

        public ObscuredInt MaximumLevel;

        public List<CharacterLevelUpCost> LevelUpCostGoods = new List<CharacterLevelUpCost>();
    }

    public class CharacterLevelUpCost
    {
        public ObscuredInt PrevMaxLevel = 0;

        public V2Enum_Goods LevelUpCostGoodsType;
        public ObscuredInt LevelUpCostGoodsParam1;
        // 소비량 = LevelUpCostGoodsParam2 + (LevelUpCostGoodsParam3*레벨)
        public ObscuredDouble LevelUpCostGoodsParam2;
        public ObscuredDouble LevelUpCostGoodsParam3;
    }


    public class CharacterLevelUpStatData
    {
        public ObscuredInt Index;

        public ObscuredInt PrevMaxLevel = 0;
        public ObscuredInt MaximumLevel;

        public Dictionary<V2Enum_Stat, CreatureBaseStatElement> StatValue = new Dictionary<V2Enum_Stat, CreatureBaseStatElement>();
    }

    public class CharacterBaseStatData
    {
        public ObscuredInt Index;

        public V2Enum_Stat BaseStat;

        public ObscuredDouble BaseValue;
        public ObscuredInt ResourceIndex;

        public V2Enum_PrintType VisibleType;

        public string NameLocalStringKey;
    }

    public class CharacterLevelUpLimitData
    {
        public ObscuredInt Index;

        public V2Enum_MonsterRoleType RoleType;
        public ObscuredInt LimitLevel;

        public List<CharacterLevelUpLimitCost> LimitCostGoods = new List<CharacterLevelUpLimitCost>();

        public ObscuredBool IsOpenPassiveSkill2;
        public ObscuredBool IsOpenPassiveSkill3;

        public Dictionary<V2Enum_Stat, CreatureBaseStatElement> LimitStatValue = new Dictionary<V2Enum_Stat, CreatureBaseStatElement>();
    }

    public class CharacterLevelUpLimitCost
    {
        public V2Enum_Goods LimitBreakCostGoodsType;
        public ObscuredInt LimitBreakCostGoodsParam1;
        // 소비량 = LimitBreakCostGoodsParam2
        public ObscuredDouble LimitBreakCostGoodsParam2;
    }

    public class ARRRSkillData
    { 
        public ObscuredInt Index;
        public ObscuredInt SkillIndex;

        public V2Enum_Grade Grade;
    }

    public class CombatPowerData
    {
        public ObscuredDouble BattlePowerConvertValue;
    }

    public class CharacterLocalTable : LocalTableBase
    {
        private List<CharacterLevelUpCostData> _characterLevelUpCostDatas = new List<CharacterLevelUpCostData>();

        private List<CharacterLevelUpStatData> _characterLevelUpStatDatas = new List<CharacterLevelUpStatData>();

        private List<CharacterBaseStatData> _characterBaseStatDatas = new List<CharacterBaseStatData>();
        private Dictionary<V2Enum_Stat, CharacterBaseStatData> _characterBaseStatDatas_Dic = new Dictionary<V2Enum_Stat, CharacterBaseStatData>();

        private Dictionary<ObscuredInt, CharacterLevelUpLimitData> _creatureLevelUpLimitDatas_Dic = new Dictionary<ObscuredInt, CharacterLevelUpLimitData>();

        private List<ARRRSkillData> _characterSkillDatas = new List<ARRRSkillData>();
        private Dictionary<ObscuredInt, ARRRSkillData> _characterSkillDataLinks = new Dictionary<ObscuredInt, ARRRSkillData>();
        private Dictionary<ObscuredInt, ARRRSkillData> _characterSkillDataLinks_SkillBaseDataIndex = new Dictionary<ObscuredInt, ARRRSkillData>();

        private Dictionary<V2Enum_Grade, LevelUpCostData> _skillLevelUpCostDatas_Dic = new Dictionary<V2Enum_Grade, LevelUpCostData>();

        private Dictionary<V2Enum_Stat, ObscuredDouble> _statPowerDatas_Dic = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        private Dictionary<Enum_SynergyPowerType, ObscuredDouble> _synergyPowerDatas_Dic = new Dictionary<Enum_SynergyPowerType, ObscuredDouble>();
        private Dictionary<Enum_RelicPowerType, ObscuredDouble> _relicPowerDatas_Dic = new Dictionary<Enum_RelicPowerType, ObscuredDouble>();
        private Dictionary<Enum_DescendPowerType, ObscuredDouble> _descendPowerDatas_Dic = new Dictionary<Enum_DescendPowerType, ObscuredDouble>();



        private ObscuredInt _characterMaxLevel = 0;

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CharacterLevelUpCostData characterLevelUpCostData = new CharacterLevelUpCostData();
                characterLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                characterLevelUpCostData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();

                for (int j = 1; j <= 2; ++j)
                {
                    try
                    {
                        string LevelUpCostGoodsParam1 = string.Format("LevelUpCostGoodsParam{0}1", j);
                        int starUpRequireditem = rows[i][LevelUpCostGoodsParam1].ToString().ToInt();
                        if (starUpRequireditem == -1 || starUpRequireditem == 0)
                            continue;

                        string LevelUpCostGoodsType = string.Format("LevelUpCostGoodsType{0}", j);
                        string LevelUpCostGoodsParam2 = string.Format("LevelUpCostGoodsParam{0}2", j);
                        string LevelUpCostGoodsParam3 = string.Format("LevelUpCostGoodsParam{0}3", j);

                        CharacterLevelUpCost characterLevelUpCost = new CharacterLevelUpCost();
                        characterLevelUpCost.LevelUpCostGoodsType = rows[i][LevelUpCostGoodsType].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                        characterLevelUpCost.LevelUpCostGoodsParam1 = rows[i][LevelUpCostGoodsParam1].ToString().ToInt();
                        characterLevelUpCost.LevelUpCostGoodsParam2 = rows[i][LevelUpCostGoodsParam2].ToString().ToDouble();
                        characterLevelUpCost.LevelUpCostGoodsParam3 = rows[i][LevelUpCostGoodsParam3].ToString().ToDouble();

                        characterLevelUpCostData.LevelUpCostGoods.Add(characterLevelUpCost);
                    }
                    catch
                    {

                    }
                }

                if (_characterMaxLevel < characterLevelUpCostData.MaximumLevel)
                    _characterMaxLevel = characterLevelUpCostData.MaximumLevel;

                _characterLevelUpCostDatas.Add(characterLevelUpCostData);
            }


            _characterLevelUpCostDatas.Sort((x, y) =>
            {
                if (x.MaximumLevel.GetDecrypted() > y.MaximumLevel.GetDecrypted())
                    return 1;
                else if (x.MaximumLevel.GetDecrypted() < y.MaximumLevel.GetDecrypted())
                    return -1;

                return 0;
            });

            int prevMaxLevel = 0;

            for (int i = 0; i < _characterLevelUpCostDatas.Count; ++i)
            {
                CharacterLevelUpCostData characterLevelUpCostData = _characterLevelUpCostDatas[i];

                for (int j = 0; j < characterLevelUpCostData.LevelUpCostGoods.Count; ++j)
                {
                    CharacterLevelUpCost characterLevelUpCost = characterLevelUpCostData.LevelUpCostGoods[j];

                    characterLevelUpCost.PrevMaxLevel = prevMaxLevel;
                }

                prevMaxLevel = characterLevelUpCostData.MaximumLevel;
            }

            List<V2Enum_Stat> searchStat = new List<V2Enum_Stat>();
            searchStat.Add(V2Enum_Stat.Attack);
            searchStat.Add(V2Enum_Stat.HP);
            searchStat.Add(V2Enum_Stat.Defence);
            searchStat.Add(V2Enum_Stat.MoveSpeed);
            searchStat.Add(V2Enum_Stat.CritChance);



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterLevelUpStat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CharacterLevelUpStatData creatureLevelUpStatData = new CharacterLevelUpStatData();

                creatureLevelUpStatData.Index = rows[i]["Index"].ToString().ToInt();

                creatureLevelUpStatData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();


                for (int statid = 0; statid < searchStat.Count; ++statid)
                {
                    V2Enum_Stat v2Enum_Stat = searchStat[statid];

                    try
                    {
                        double statvalue = rows[i][v2Enum_Stat.ToString()].ToString().ToDouble();
                        CreatureBaseStatElement allyBaseStatElement = new CreatureBaseStatElement();
                        allyBaseStatElement.BaseStat = v2Enum_Stat;
                        allyBaseStatElement.BaseValue = statvalue;

                        creatureLevelUpStatData.StatValue.Add(allyBaseStatElement.BaseStat, allyBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                _characterLevelUpStatDatas.Add(creatureLevelUpStatData);
            }

            _characterLevelUpStatDatas.Sort((x, y) =>
            {
                if (x.MaximumLevel.GetDecrypted() > y.MaximumLevel.GetDecrypted())
                    return 1;
                else if (x.MaximumLevel.GetDecrypted() < y.MaximumLevel.GetDecrypted())
                    return -1;

                return 0;
            });


            prevMaxLevel = 0;

            for (int i = 0; i < _characterLevelUpStatDatas.Count; ++i)
            {
                CharacterLevelUpStatData characterLevelUpStatData = _characterLevelUpStatDatas[i];
                characterLevelUpStatData.PrevMaxLevel = prevMaxLevel;

                prevMaxLevel = characterLevelUpStatData.MaximumLevel;
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterBaseStat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CharacterBaseStatData data = new CharacterBaseStatData();
                data.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                data.BaseStat = rows[i]["BaseStat"].ToString().ToInt().IntToEnum32<V2Enum_Stat>();
                data.BaseValue = rows[i]["BaseValue"].ToString().ToDouble();
                data.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                data.NameLocalStringKey = string.Format("baseStat/{0}/name", data.ResourceIndex);

                data.VisibleType = rows[i]["VisibleType"].ToString().ToInt().IntToEnum32<V2Enum_PrintType>();

                _characterBaseStatDatas.Add(data);
                _characterBaseStatDatas_Dic.Add(data.BaseStat, data);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterLevelUpLimit", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            searchStat.Clear();

            searchStat.Add(V2Enum_Stat.VampiricRate);

            searchStat.Add(V2Enum_Stat.ResistanceStat);
            searchStat.Add(V2Enum_Stat.ResistancePenetration);

            searchStat.Add(V2Enum_Stat.CritDmgIncrease);

            searchStat.Add(V2Enum_Stat.DmgBoost);


            for (int i = 0; i < rows.Count; ++i)
            {
                CharacterLevelUpLimitData characterLevelUpLimitData = new CharacterLevelUpLimitData();
                characterLevelUpLimitData.Index = rows[i]["Index"].ToString().ToInt();
                characterLevelUpLimitData.LimitLevel = rows[i]["LimitLevel"].ToString().ToInt();

                for (int j = 1; j <= 2; ++j)
                {
                    try
                    {
                        string LimitBreakCostGoodsParam1 = string.Format("LimitBreakCostGoodsParam{0}1", j);
                        int starUpRequireditem = rows[i][LimitBreakCostGoodsParam1].ToString().ToInt();
                        if (starUpRequireditem == -1 || starUpRequireditem == 0)
                            continue;

                        string LimitBreakCostGoodsType = string.Format("LimitBreakCostGoodsType{0}", j);
                        string LimitBreakCostGoodsParam2 = string.Format("LimitBreakCostGoodsParam{0}2", j);

                        CharacterLevelUpLimitCost characterLevelUpCost = new CharacterLevelUpLimitCost();
                        characterLevelUpCost.LimitBreakCostGoodsType = rows[i][LimitBreakCostGoodsType].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                        characterLevelUpCost.LimitBreakCostGoodsParam1 = rows[i][LimitBreakCostGoodsParam1].ToString().ToInt();
                        characterLevelUpCost.LimitBreakCostGoodsParam2 = rows[i][LimitBreakCostGoodsParam2].ToString().ToDouble();

                        characterLevelUpLimitData.LimitCostGoods.Add(characterLevelUpCost);
                    }
                    catch
                    {

                    }
                }

                for (int statid = 0; statid < searchStat.Count; ++statid)
                {
                    V2Enum_Stat v2Enum_Stat = searchStat[statid];

                    try
                    {
                        double statvalue = rows[i][v2Enum_Stat.ToString()].ToString().ToDouble();
                        CreatureBaseStatElement allyBaseStatElement = new CreatureBaseStatElement();
                        allyBaseStatElement.BaseStat = v2Enum_Stat;
                        allyBaseStatElement.BaseValue = statvalue;

                        characterLevelUpLimitData.LimitStatValue.Add(allyBaseStatElement.BaseStat, allyBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                _creatureLevelUpLimitDatas_Dic.Add(characterLevelUpLimitData.LimitLevel, characterLevelUpLimitData);
            }




            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterSkill", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ARRRSkillData data = new ARRRSkillData();
                data.Index = rows[i]["Index"].ToString().ToInt();
                data.SkillIndex = rows[i]["SkillIndex"].ToString().ToInt();
                data.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                _characterSkillDatas.Add(data);
                _characterSkillDataLinks.Add(data.Index, data);
                _characterSkillDataLinks_SkillBaseDataIndex.Add(data.SkillIndex, data);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterSkillLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);


            for (int i = 0; i < rows.Count; ++i)
            {
                LevelUpCostData petLevelUpCostData = new LevelUpCostData();

                petLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                petLevelUpCostData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                petLevelUpCostData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();

                petLevelUpCostData.LevelUpCostCount = rows[i]["SkillLevelUpCostCount"].ToString().ToInt();

                if (_skillLevelUpCostDatas_Dic.ContainsKey(petLevelUpCostData.Grade) == false)
                    _skillLevelUpCostDatas_Dic.Add(petLevelUpCostData.Grade, petLevelUpCostData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Combatpower", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                Enum_PowerType powerType = rows[i]["PowerType"].ToString().ToInt().IntToEnum32<Enum_PowerType>();

                double BattlePowerConvertValue = rows[i]["BattlePowerConvertValue"].ToString().ToDouble();

                int powerenum = rows[i]["PowerEnum"].ToString().ToInt();
                

                if (powerType == Enum_PowerType.Stat)
                {
                    V2Enum_Stat PowerEnum = powerenum.IntToEnum32<V2Enum_Stat>();
                    if (_statPowerDatas_Dic.ContainsKey(PowerEnum) == false)
                        _statPowerDatas_Dic.Add(PowerEnum, BattlePowerConvertValue);
                }
                else if (powerType == Enum_PowerType.Synergy)
                {
                    Enum_SynergyPowerType PowerEnum = powerenum.IntToEnum32<Enum_SynergyPowerType>();
                    if (_synergyPowerDatas_Dic.ContainsKey(PowerEnum) == false)
                        _synergyPowerDatas_Dic.Add(PowerEnum, BattlePowerConvertValue);
                }
                else if (powerType == Enum_PowerType.Relic)
                {
                    Enum_RelicPowerType PowerEnum = powerenum.IntToEnum32<Enum_RelicPowerType>();
                    if (_relicPowerDatas_Dic.ContainsKey(PowerEnum) == false)
                        _relicPowerDatas_Dic.Add(PowerEnum, BattlePowerConvertValue);
                }
                else if (powerType == Enum_PowerType.Descend)
                {
                    Enum_DescendPowerType PowerEnum = powerenum.IntToEnum32<Enum_DescendPowerType>();
                    if (_descendPowerDatas_Dic.ContainsKey(PowerEnum) == false)
                        _descendPowerDatas_Dic.Add(PowerEnum, BattlePowerConvertValue);
                }
            }

        }
        //------------------------------------------------------------------------------------
        public ObscuredInt GetMaxLevel()
        {
            return _characterMaxLevel;
        }
        //------------------------------------------------------------------------------------
        public CharacterLevelUpCostData GetCharacterLevelUpCostData(ObscuredInt nextlevel)
        {
            CharacterLevelUpCostData selectData = null;

            List<CharacterLevelUpCostData> characterLevelUpCostDatas = _characterLevelUpCostDatas;

            for (int i = characterLevelUpCostDatas.Count - 1; i >= 0; --i)
            {
                if (characterLevelUpCostDatas[i].MaximumLevel >= nextlevel)
                    selectData = characterLevelUpCostDatas[i];
                else
                    break;
            }

            return selectData;
        }
        //------------------------------------------------------------------------------------
        public CharacterLevelUpStatData GetCharacterLevelUpStatData(ObscuredInt level)
        {
            CharacterLevelUpStatData selectData = null;

            List<CharacterLevelUpStatData> characterLevelUpCostDatas = _characterLevelUpStatDatas;

            for (int i = characterLevelUpCostDatas.Count - 1; i >= 0; --i)
            {
                if (characterLevelUpCostDatas[i].MaximumLevel >= level)
                    selectData = characterLevelUpCostDatas[i];
                else
                    break;
            }

            return selectData;
        }
        //------------------------------------------------------------------------------------
        public List<CharacterLevelUpStatData> GetCharacterLevelUpStatDatas()
        {
            return _characterLevelUpStatDatas;
        }
        //------------------------------------------------------------------------------------
        public CharacterLevelUpLimitData GetCharacterLevelUpLimitData(ObscuredInt level)
        {
            if (_creatureLevelUpLimitDatas_Dic.ContainsKey(level) == false)
                return null;

            return _creatureLevelUpLimitDatas_Dic[level];
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, CharacterLevelUpLimitData> GetCharacterLevelUpLimitDatas()
        {
            return _creatureLevelUpLimitDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public List<CharacterBaseStatData> GetCharacterBaseStatDatas()
        {
            return _characterBaseStatDatas;
        }
        //------------------------------------------------------------------------------------
        public CharacterBaseStatData GetCharacterBaseStatData(V2Enum_Stat v2Enum_Stat)
        {
            if (_characterBaseStatDatas_Dic.ContainsKey(v2Enum_Stat) == true)
                return _characterBaseStatDatas_Dic[v2Enum_Stat];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<ARRRSkillData> GetARRRSkillDatas()
        {
            return _characterSkillDatas;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, ARRRSkillData> GetARRRSkillLinkDatas()
        {
            return _characterSkillDataLinks;
        }
        //------------------------------------------------------------------------------------
        public ARRRSkillData GetARRRSkillData(ObscuredInt index)
        {
            if (_characterSkillDataLinks.ContainsKey(index))
                return _characterSkillDataLinks[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public ARRRSkillData GetARRRSkillData_SkillBaseDataIndex(ObscuredInt index)
        {
            if (_characterSkillDataLinks_SkillBaseDataIndex.ContainsKey(index))
                return _characterSkillDataLinks_SkillBaseDataIndex[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public LevelUpCostData GetPetLevelUpCostData(V2Enum_Grade index)
        {
            if (_skillLevelUpCostDatas_Dic.ContainsKey(index) == true)
                return _skillLevelUpCostDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public double GetBattlePowerConvertValue(V2Enum_Stat v2Enum_Stat)
        {
            if (_statPowerDatas_Dic.ContainsKey(v2Enum_Stat) == true)
                return _statPowerDatas_Dic[v2Enum_Stat];

            return 0;
        }
        //------------------------------------------------------------------------------------
        public double GetBattlePowerConvertValue(Enum_SynergyPowerType v2Enum_Stat)
        {
            if (_synergyPowerDatas_Dic.ContainsKey(v2Enum_Stat) == true)
                return _synergyPowerDatas_Dic[v2Enum_Stat];

            return 0;
        }
        //------------------------------------------------------------------------------------
        public double GetBattlePowerConvertValue(Enum_RelicPowerType v2Enum_Stat)
        {
            if (_relicPowerDatas_Dic.ContainsKey(v2Enum_Stat) == true)
                return _relicPowerDatas_Dic[v2Enum_Stat];

            return 0;
        }
        //------------------------------------------------------------------------------------
        public double GetBattlePowerConvertValue(Enum_DescendPowerType v2Enum_Stat)
        {
            if (_descendPowerDatas_Dic.ContainsKey(v2Enum_Stat) == true)
                return _descendPowerDatas_Dic[v2Enum_Stat];

            return 0;
        }
        //------------------------------------------------------------------------------------
    }
}