using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class ResearchData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;

        public ObscuredInt LocationY;
        public ObscuredInt LocationX;

        public ObscuredInt NextResearchIndex1;
        public ObscuredInt NextResearchIndex2;
        public ObscuredInt NextResearchIndex3;
        public ObscuredInt NextResearchIndex4;

        public List<ObscuredInt> NextResearchIndex = new List<ObscuredInt>();

        public V2Enum_ResearchType ResearchEffectType;
        public ObscuredDouble ResearchEffectTypeLevelUpValue;

        public ObscuredInt ResearchMaxLevel;

        public ObscuredInt LevelUpCostGoodsParam1;
        public ObscuredDouble LevelUpCostGoodsParam2;
        public ObscuredDouble LevelUpCostGoodsParam3;

        public ObscuredDouble BaseResearchTimeValue;
        public ObscuredDouble IncreaseResearchTimeValue;

        public List<ResearchData> MyRootData = new List<ResearchData>();
    }


    public class ResearchOpenConditionData
    {
        public ObscuredInt ResearchIndex;

        public Dictionary<ObscuredInt, ObscuredInt> Preceeds = new Dictionary<ObscuredInt, ObscuredInt>();
    }

    public class ResearchLocalTable : LocalTableBase
    {
        private List<List<ResearchData>> m_masteryNormalAllData = new List<List<ResearchData>>();
        private Dictionary<int, ResearchData> m_masteryDatas_Dic = new Dictionary<int, ResearchData>();

        private List<ResearchData> m_noneRootMastery = new List<ResearchData>();

        private Dictionary<int, ResearchOpenConditionData> _researchOpenConditionDatas_Dic = new Dictionary<int, ResearchOpenConditionData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            //await SetMasteryData("MasteryNormal", m_masteryNormalAllData, V2Enum_MasteryType.Normal);

            //await SetMasteryData("MasteryPrime", m_masteryPrimeAllData, V2Enum_MasteryType.Prime);


            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Research", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ResearchData masteryData = new ResearchData();

                masteryData.Index = rows[i]["Index"].ToString().ToInt();

                masteryData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                masteryData.LocationY = rows[i]["LocationY"].ToString().ToInt();
                masteryData.LocationX = rows[i]["LocationX"].ToString().ToInt();

                int nextvalue = rows[i]["NextResearchIndex1"].ToString().ToInt();
                if (nextvalue > 0)
                    masteryData.NextResearchIndex.Add(nextvalue);

                nextvalue = rows[i]["NextResearchIndex2"].ToString().ToInt();
                if (nextvalue > 0)
                    masteryData.NextResearchIndex.Add(nextvalue);

                nextvalue = rows[i]["NextResearchIndex3"].ToString().ToInt();
                if (nextvalue > 0)
                    masteryData.NextResearchIndex.Add(nextvalue);

                nextvalue = rows[i]["NextResearchIndex4"].ToString().ToInt();
                if (nextvalue > 0)
                    masteryData.NextResearchIndex.Add(nextvalue);


                masteryData.ResearchEffectType = rows[i]["ResearchEffectType"].ToString().ToInt().IntToEnum32<V2Enum_ResearchType>();
                masteryData.ResearchEffectTypeLevelUpValue = rows[i]["ResearchEffectTypeLevelUpValue"].ToString().ToDouble();

                masteryData.ResearchMaxLevel = rows[i]["ResearchMaxLevel"].ToString().ToInt();

                masteryData.LevelUpCostGoodsParam1 = rows[i]["LevelUpCostGoodsParam1"].ToString().ToInt();
                masteryData.LevelUpCostGoodsParam2 = rows[i]["LevelUpCostGoodsParam2"].ToString().ToDouble();
                masteryData.LevelUpCostGoodsParam3 = rows[i]["LevelUpCostGoodsParam3"].ToString().ToDouble();

                masteryData.BaseResearchTimeValue = rows[i]["BaseResearchTimeValue"].ToString().ToDouble();
                masteryData.IncreaseResearchTimeValue = rows[i]["IncreaseResearchTimeValue"].ToString().ToDouble();

                while (m_masteryNormalAllData.Count < masteryData.LocationY)
                    m_masteryNormalAllData.Add(new List<ResearchData>());

                List<ResearchData> mymasteryDatas = m_masteryNormalAllData[masteryData.LocationY - 1];
                mymasteryDatas.Add(masteryData);

                m_masteryDatas_Dic.Add(masteryData.Index, masteryData);
            }

            foreach (KeyValuePair<int, ResearchData> pair in m_masteryDatas_Dic)
            {
                for (int i = 0; i < pair.Value.NextResearchIndex.Count; ++i)
                {
                    if (m_masteryDatas_Dic.ContainsKey(pair.Value.NextResearchIndex[i]) == true)
                    {
                        ResearchData masteryData = m_masteryDatas_Dic[pair.Value.NextResearchIndex[i]];
                        masteryData.MyRootData.Add(pair.Value);
                    }
                }
            }

            foreach (KeyValuePair<int, ResearchData> pair in m_masteryDatas_Dic)
            {
                if (pair.Value.MyRootData.Count == 0)
                    m_noneRootMastery.Add(pair.Value);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ResearchOpenCondition", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                ResearchOpenConditionData masteryData = new ResearchOpenConditionData();
                masteryData.ResearchIndex = rows[i]["ResearchIndex"].ToString().ToInt();

                for (int j = 1; j <= 4; ++j)
                {
                    try
                    {
                        string SynergyType = string.Format("PreceedIndex{0}", j);
                        int SynergyTypeData = rows[i][SynergyType].ToString().ToInt();
                        if (SynergyTypeData == -1 || SynergyTypeData == 0)
                            continue;

                        if (masteryData.Preceeds.ContainsKey(SynergyTypeData) == true)
                            continue;

                        string RequiredSynergyCount = string.Format("PreceedIndexLevel{0}", j);
                        int RequiredSynergyCountData = rows[i][RequiredSynergyCount].ToString().ToInt();
                        if (RequiredSynergyCountData == -1 || RequiredSynergyCountData == 0)
                            continue;

                        masteryData.Preceeds.Add(SynergyTypeData, RequiredSynergyCountData);
                    }
                    catch
                    {

                    }
                }

                _researchOpenConditionDatas_Dic.Add(masteryData.ResearchIndex, masteryData);
            }
        }
        //------------------------------------------------------------------------------------
        public List<List<ResearchData>> GetNormalAllData()
        {
            return m_masteryNormalAllData;
        }
        //------------------------------------------------------------------------------------
        public List<ResearchData> GetNoneRootData()
        {
            return m_noneRootMastery;
        }
        //------------------------------------------------------------------------------------
        public ResearchData GetData(int index)
        {
            if (m_masteryDatas_Dic.ContainsKey(index) == true)
                return m_masteryDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public ResearchOpenConditionData GetResearchOpenConditionData(int index)
        {
            if (_researchOpenConditionDatas_Dic.ContainsKey(index) == true)
                return _researchOpenConditionDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}