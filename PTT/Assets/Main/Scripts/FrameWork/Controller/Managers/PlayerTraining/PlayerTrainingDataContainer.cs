using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class PlayerTrainingDataContainer
    {
        // 플레이어 스텟 강화 현황
        public static Dictionary<V2Enum_Stat, ObscuredInt> m_trainingLevel = new Dictionary<V2Enum_Stat, ObscuredInt>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_trainingLevel)
            {
                SerializeString.Append(pair.Key.Enum32ToInt());
                SerializeString.Append(',');
                SerializeString.Append(pair.Value);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                m_trainingLevel.Add(arrcontent[0].ToInt().IntToEnum32<V2Enum_Stat>(), arrcontent[1].ToInt());
            }
        }
    }

    public static class PlayerTrainingDataOperator
    {
        private static CharacterBaseTrainingLocalTable m_playerTrainingLocalChart = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_playerTrainingLocalChart = Managers.TableManager.Instance.GetTableClass<CharacterBaseTrainingLocalTable>();

        }
        //------------------------------------------------------------------------------------
        public static double GetTrainingStatPrice(V2Enum_Stat type, int traininglevel)
        {
            if (m_playerTrainingLocalChart == null)
                return double.MaxValue;

            CharacterBaseTrainingData data = m_playerTrainingLocalChart.GetData(type);

            if (data == null)
                return double.MaxValue;

            //double price = data.CostDefCount + ((traininglevel - 1) * data.CostAddCount);

            double price = (data.LevelUpCostGoodsParam2 * data.LevelUpCostGoodsParam3)
                + (traininglevel * data.LevelUpCostGoodsParam4);

            return price;
        }
        //------------------------------------------------------------------------------------
        public static double GetCurrentTrainingStatValue(V2Enum_Stat type, int TrainingLevel)
        {
            if (m_playerTrainingLocalChart == null)
                return 0.0;

            CharacterBaseTrainingData data = m_playerTrainingLocalChart.GetData(type);

            if (data == null)
                return 0.0;

            return data.TrainingValuePerLevel * (TrainingLevel);
        }
        //------------------------------------------------------------------------------------
        public static bool IsMaxTrainingStat(V2Enum_Stat type, int TrainingLevel)
        {
            if (m_playerTrainingLocalChart == null)
                return true;

            CharacterBaseTrainingData data = m_playerTrainingLocalChart.GetData(type);

            if (data == null)
                return true;

            if (data.TrainingMaxLevel <= TrainingLevel)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public static CharacterBaseTrainingData GetTrainingData(V2Enum_Stat type)
        {
            return m_playerTrainingLocalChart.GetData(type);
        }
        //------------------------------------------------------------------------------------
        public static CharacterBaseTrainingData GetTrainingData(int id)
        {
            return m_playerTrainingLocalChart.GetData(id);
        }
        //------------------------------------------------------------------------------------
    }
}

