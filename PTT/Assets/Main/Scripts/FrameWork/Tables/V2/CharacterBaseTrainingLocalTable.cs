using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class CharacterBaseTrainingData
    {
        public int Index;
        public int ResourceIndex;

        public int OpenConditionIndex;
        public int OpenConditionLevel;

        public V2Enum_Stat TrainingType;

        public int TrainingValuePerLevel;
        public int TrainingMaxLevel;

        public int LevelUpCostGoodsType;
        public int LevelUpCostGoodsParam1;
        public int LevelUpCostGoodsParam2;
        public int LevelUpCostGoodsParam3;
        public int LevelUpCostGoodsParam4;

        public string NameLocalStringKey;
        public string IconStringKey;
    }

    public class CharacterBaseTrainingLocalTable : LocalTableBase
    {
        private List<CharacterBaseTrainingData> m_characterBaseTrainingDatas = new List<CharacterBaseTrainingData>();
        private Dictionary<int, CharacterBaseTrainingData> m_characterBaseTrainingDatas_Dic = null;
        private Dictionary<V2Enum_Stat, CharacterBaseTrainingData> m_characterBaseTrainingDatas_Enum_Dic = null;

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CharacterBaseTraining", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            m_characterBaseTrainingDatas = JsonConvert.DeserializeObject<List<CharacterBaseTrainingData>>(rows.ToJson());
            m_characterBaseTrainingDatas_Dic = new Dictionary<int, CharacterBaseTrainingData>(m_characterBaseTrainingDatas.Count);
            m_characterBaseTrainingDatas_Enum_Dic = new Dictionary<V2Enum_Stat, CharacterBaseTrainingData>(m_characterBaseTrainingDatas.Count);

            for (int i = 0; i < m_characterBaseTrainingDatas.Count; ++i)
            {
                m_characterBaseTrainingDatas[i].NameLocalStringKey = string.Format("baseTraining/{0}/name", m_characterBaseTrainingDatas[i].ResourceIndex);
                m_characterBaseTrainingDatas[i].IconStringKey = string.Format("baseTraining/{0}/icon", m_characterBaseTrainingDatas[i].ResourceIndex);

                m_characterBaseTrainingDatas_Dic.Add(m_characterBaseTrainingDatas[i].Index, m_characterBaseTrainingDatas[i]);
                m_characterBaseTrainingDatas_Enum_Dic.Add(m_characterBaseTrainingDatas[i].TrainingType, m_characterBaseTrainingDatas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        public List<CharacterBaseTrainingData> GetAllData()
        {
            return m_characterBaseTrainingDatas;
        }
        //------------------------------------------------------------------------------------
        public CharacterBaseTrainingData GetData(int id)
        {
            if (m_characterBaseTrainingDatas_Dic.ContainsKey(id) == true)
                return m_characterBaseTrainingDatas_Dic[id];

            return null;
        }
        //------------------------------------------------------------------------------------
        public CharacterBaseTrainingData GetData(V2Enum_Stat v2Enum_Stat)
        {
            if (m_characterBaseTrainingDatas_Enum_Dic.ContainsKey(v2Enum_Stat) == true)
                return m_characterBaseTrainingDatas_Enum_Dic[v2Enum_Stat];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}