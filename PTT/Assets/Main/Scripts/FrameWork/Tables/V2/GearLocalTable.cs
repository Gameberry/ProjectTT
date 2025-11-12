using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;


namespace GameBerry
{
    public class GearData : Gpm.Ui.InfiniteScrollData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;

        public V2Enum_GearType GearType;
        public Enum_SynergyType SynergyType;

        public V2Enum_Grade Grade;

        public V2Enum_Stat OwnEffectType1;
        public ObscuredDouble OwnEffectBaseValue1;
        public ObscuredDouble OwnEffectLevelupValue1;

        public V2Enum_Stat OwnEffectType2;
        public ObscuredDouble OwnEffectBaseValue2;
        public ObscuredDouble OwnEffectLevelupValue2;

        public List<OperatorOverrideStat> OwnEffect = new List<OperatorOverrideStat>();

        public string NameLocalKey;
        public string DescLocalKey;

        public GearData NextGearData = null;
    }

    public class GearLevelUpCostData
    {
        public ObscuredInt Index;

        public V2Enum_GearType GearType;

        public ObscuredInt MaxLevel;

        public ObscuredInt LevelUpCostGoodsParam11;
        public ObscuredInt LevelUpCostGoodsParam12;
        public ObscuredInt LevelUpCostGoodsParam13;

        public ObscuredInt LevelUpCostGoodsParam21;
        public ObscuredInt LevelUpCostGoodsParam22;
        public ObscuredInt LevelUpCostGoodsParam23;
    }

    public class GearCombineData
    {
        public ObscuredInt Index;

        public V2Enum_Grade Grade;

        public ObscuredInt RequiredCount;
        public ObscuredInt SuccessProb;
    }

    public class GearOptionData
    {
        public ObscuredInt Index;

        public V2Enum_GearType GearType;

        public Enum_SynergyType SynergyType;

        public Dictionary<V2Enum_Grade, ObscuredInt> GearSkills = new Dictionary<V2Enum_Grade, ObscuredInt>();
    }

    public class GearLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, GearData> _synergyRuneDatas_Dic = new Dictionary<ObscuredInt, GearData>();
        private Dictionary<V2Enum_Grade, List<GearData>> _synergyRuneList_Dic = new Dictionary<V2Enum_Grade, List<GearData>>();
        private Dictionary<V2Enum_GearType, List<GearData>> _synergyRuneList_GearType_Dic = new Dictionary<V2Enum_GearType, List<GearData>>();
        private List<GearData> _synergyRuneAllData = new List<GearData>();

        private Dictionary<V2Enum_GearType, GearLevelUpCostData> _gearLevelUpCost_Dic = new Dictionary<V2Enum_GearType, GearLevelUpCostData>();
        
        private Dictionary<V2Enum_Grade, GearCombineData> _synergyRuneCombineDatas_Dic = new Dictionary<V2Enum_Grade, GearCombineData>();

        private Dictionary<V2Enum_GearType, Dictionary<Enum_SynergyType, GearOptionData>> _gearOptionData_Dic = new Dictionary<V2Enum_GearType, Dictionary<Enum_SynergyType, GearOptionData>>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterGear", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GearData gearData = new GearData();

                gearData.Index = rows[i]["Index"].ToString().ToInt();

                gearData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                gearData.GearType = rows[i]["GearType"].ToString().ToInt().IntToEnum32<V2Enum_GearType>();
                gearData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<Enum_SynergyType>();

                gearData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();


                int stattype = rows[i]["OwnEffectType1"].ToString().ToInt();

                if (stattype != -1)
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = stattype.IntToEnum32<V2Enum_Stat>();
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OwnEffectBaseValue1"].ToString().ToDouble();
                    operatorOverrideStat.OverrideStatAddValue = rows[i]["OwnEffectLevelupValue1"].ToString().ToDouble();
                    gearData.OwnEffect.Add(operatorOverrideStat);
                }


                stattype = rows[i]["OwnEffectType2"].ToString().ToInt();

                if (stattype != -1)
                {
                    OperatorOverrideStat operatorOverrideStat = new OperatorOverrideStat();
                    operatorOverrideStat.BaseStat = stattype.IntToEnum32<V2Enum_Stat>();
                    operatorOverrideStat.OverrideStatBaseValue = rows[i]["OwnEffectBaseValue2"].ToString().ToDouble();
                    operatorOverrideStat.OverrideStatAddValue = rows[i]["OwnEffectLevelupValue2"].ToString().ToDouble();
                    gearData.OwnEffect.Add(operatorOverrideStat);
                }

                gearData.NameLocalKey = string.Format("gearname/{0}", gearData.Index);
                gearData.DescLocalKey = string.Format("geardesc/{0}", gearData.Index);


                if (_synergyRuneDatas_Dic.ContainsKey(gearData.Index) == false)
                    _synergyRuneDatas_Dic.Add(gearData.Index, gearData);

                if (_synergyRuneList_Dic.ContainsKey(gearData.Grade) == false)
                    _synergyRuneList_Dic.Add(gearData.Grade, new List<GearData>());

                _synergyRuneList_Dic[gearData.Grade].Add(gearData);


                if (_synergyRuneList_GearType_Dic.ContainsKey(gearData.GearType) == false)
                    _synergyRuneList_GearType_Dic.Add(gearData.GearType, new List<GearData>());

                _synergyRuneList_GearType_Dic[gearData.GearType].Add(gearData);
                

                _synergyRuneAllData.Add(gearData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterGearLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GearLevelUpCostData gearLevelUpCostData = new GearLevelUpCostData();

                gearLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                gearLevelUpCostData.GearType = rows[i]["GearType"].ToString().ToInt().IntToEnum32<V2Enum_GearType>();

                gearLevelUpCostData.MaxLevel = rows[i]["MaxLevel"].ToString().ToInt();

                gearLevelUpCostData.LevelUpCostGoodsParam11 = rows[i]["LevelUpCostGoodsParam11"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam12 = rows[i]["LevelUpCostGoodsParam12"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam13 = rows[i]["LevelUpCostGoodsParam13"].ToString().ToInt();

                gearLevelUpCostData.LevelUpCostGoodsParam21 = rows[i]["LevelUpCostGoodsParam21"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam22 = rows[i]["LevelUpCostGoodsParam22"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam23 = rows[i]["LevelUpCostGoodsParam23"].ToString().ToInt();

                if (_gearLevelUpCost_Dic.ContainsKey(gearLevelUpCostData.GearType) == false)
                    _gearLevelUpCost_Dic.Add(gearLevelUpCostData.GearType, gearLevelUpCostData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterGearCombine", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GearCombineData gearCombineData = new GearCombineData();

                gearCombineData.Index = rows[i]["Index"].ToString().ToInt();

                gearCombineData.Grade = rows[i]["Grade"].ToString().ToInt().IntToEnum32<V2Enum_Grade>();

                gearCombineData.RequiredCount = rows[i]["RequiredCount"].ToString().ToInt();
                gearCombineData.SuccessProb = rows[i]["SuccessProb"].ToString().ToInt();

                if (_synergyRuneCombineDatas_Dic.ContainsKey(gearCombineData.Grade) == false)
                    _synergyRuneCombineDatas_Dic.Add(gearCombineData.Grade, gearCombineData);
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterGearOption", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GearOptionData gearOptionData = new GearOptionData();

                gearOptionData.Index = rows[i]["Index"].ToString().ToInt();

                gearOptionData.GearType = rows[i]["GearType"].ToString().ToInt().IntToEnum32<V2Enum_GearType>();
                gearOptionData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<Enum_SynergyType>();

                for (int grade = V2Enum_Grade.C.Enum32ToInt(); grade <= V2Enum_Grade.SS.Enum32ToInt(); ++grade)
                {
                    V2Enum_Grade v2Enum_Grade = grade.IntToEnum32<V2Enum_Grade>();

                    try
                    {
                        string str = string.Format("GearSkill{0}", v2Enum_Grade.ToString());
                        int skillindex = rows[i][str].ToString().ToInt();
                        gearOptionData.GearSkills.Add(v2Enum_Grade, skillindex);
                    }
                    catch (System.Exception e)
                    { 

                    }
                }

                if (_gearOptionData_Dic.ContainsKey(gearOptionData.GearType) == false)
                    _gearOptionData_Dic.Add(gearOptionData.GearType, new Dictionary<Enum_SynergyType, GearOptionData>());

                if (_gearOptionData_Dic[gearOptionData.GearType].ContainsKey(gearOptionData.SynergyType) == false)
                    _gearOptionData_Dic[gearOptionData.GearType][gearOptionData.SynergyType] = gearOptionData;
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, GearData> GetAllGearData_Dic()
        {
            return _synergyRuneDatas_Dic;
        }
        //------------------------------------------------------------------------------------
        public List<GearData> GetAllGearData()
        {
            return _synergyRuneAllData;
        }
        //------------------------------------------------------------------------------------
        public List<GearData> GetAllGearData(V2Enum_Grade v2Enum_Grade)
        {
            if (_synergyRuneList_Dic.ContainsKey(v2Enum_Grade) == true)
                return _synergyRuneList_Dic[v2Enum_Grade];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<GearData> GetAllGearData(V2Enum_GearType v2Enum_Grade)
        {
            if (_synergyRuneList_GearType_Dic.ContainsKey(v2Enum_Grade) == true)
                return _synergyRuneList_GearType_Dic[v2Enum_Grade];

            return null;
        }
        //------------------------------------------------------------------------------------
        public GearData GetGearData(ObscuredInt index)
        {
            if (_synergyRuneDatas_Dic.ContainsKey(index) == true)
                return _synergyRuneDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public GearLevelUpCostData GetGearLevelUpCostData(V2Enum_GearType grade)
        {
            if (_gearLevelUpCost_Dic.ContainsKey(grade) == true)
                return _gearLevelUpCost_Dic[grade];

            return null;
        }
        //------------------------------------------------------------------------------------
        public GearCombineData GetGearCombineData(V2Enum_Grade grade)
        {
            if (_synergyRuneCombineDatas_Dic.ContainsKey(grade) == true)
                return _synergyRuneCombineDatas_Dic[grade];

            return null;
        }
        //------------------------------------------------------------------------------------
        public GearOptionData GetGearOptionData(V2Enum_GearType grade, Enum_SynergyType gearNumber)
        {
            if (_gearOptionData_Dic.ContainsKey(grade) == false)
                return null;

            if (_gearOptionData_Dic[grade].ContainsKey(gearNumber) == false)
                return null;

            return _gearOptionData_Dic[grade][gearNumber];
        }
        //------------------------------------------------------------------------------------
    }
}