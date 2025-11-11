using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;


namespace GameBerry.Managers
{
    public class PlayerTrainingDataManager : MonoSingleton<PlayerTrainingDataManager>
    {
        private Dictionary<V2Enum_Stat, StatElementValue> m_addStatValue = new Dictionary<V2Enum_Stat, StatElementValue>();

        private Event.RefreshTrainingMsg m_refreshTrainingMsg = new Event.RefreshTrainingMsg();

        private List<string> m_changeInfoUpdate = new List<string>();

        public int CheatAddLevelUP = 0;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerTrainingTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);
            PlayerTrainingDataOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitStatElementValue()
        {
            foreach (KeyValuePair<V2Enum_Stat, ObscuredInt> pair in PlayerTrainingDataContainer.m_trainingLevel)
            {
                if (m_addStatValue.ContainsKey(pair.Key) == false)
                    m_addStatValue.Add(pair.Key, new StatElementValue());

                m_addStatValue[pair.Key].StatValue += GetCurrentTrainingStatValue(pair.Key);

                CharacterStatManager.Instance.AddStatElementValue(pair.Key, m_addStatValue[pair.Key]);

                CharacterStatManager.Instance.SetOutPutStatValue(pair.Key);
            }
        }
        //------------------------------------------------------------------------------------
        public CharacterBaseTrainingData GetCharTrainingData(int id)
        {
            return PlayerTrainingDataOperator.GetTrainingData(id);
        }
        //------------------------------------------------------------------------------------
        public CharacterBaseTrainingData GetCharTrainingData(V2Enum_Stat type)
        {
            return PlayerTrainingDataOperator.GetTrainingData(type);
        }
        //------------------------------------------------------------------------------------
        public void AddTrainingStatLevel(V2Enum_Stat type, int level)
        { 
            PlayerTrainingDataContainer.m_trainingLevel.Add(type, level);
        }
        //------------------------------------------------------------------------------------
        public void SetTrainingStatLevel(V2Enum_Stat type, int level)
        {
            if (PlayerTrainingDataContainer.m_trainingLevel.ContainsKey(type) == false)
            {
                PlayerTrainingDataContainer.m_trainingLevel.Add(type, level);
            }
            else
                PlayerTrainingDataContainer.m_trainingLevel[type] = level;
        }
        //------------------------------------------------------------------------------------
        public int GetCurrentTrainingStatLevel(V2Enum_Stat type)
        {
            if (PlayerTrainingDataContainer.m_trainingLevel.ContainsKey(type) == false)
            {
                return 0;
            }
            return PlayerTrainingDataContainer.m_trainingLevel[type];
        }
        //------------------------------------------------------------------------------------
        public double GetTrainingStatPrice(V2Enum_Stat type)
        {
            double price = PlayerTrainingDataOperator.GetTrainingStatPrice(type, GetCurrentTrainingStatLevel(type));

            price *= CharacterStatManager.Instance.GetOutPutAddUpgradeCost();

            price = System.Math.Round(price);

            return price;
        }
        //------------------------------------------------------------------------------------
        public double GetCurrentTrainingStatValue(V2Enum_Stat type)
        {
            return PlayerTrainingDataOperator.GetCurrentTrainingStatValue(type, GetCurrentTrainingStatLevel(type));
        }
        //------------------------------------------------------------------------------------
        public double GetNextTrainingStatValue(V2Enum_Stat type)
        {
            return PlayerTrainingDataOperator.GetCurrentTrainingStatValue(type, GetCurrentTrainingStatLevel(type) + 1);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxTrainingStat(V2Enum_Stat type)
        {
            return PlayerTrainingDataOperator.IsMaxTrainingStat(type, GetCurrentTrainingStatLevel(type));
        }
        //------------------------------------------------------------------------------------
        public bool InCreaseTrainingStatLevel_Click(V2Enum_Stat type)
        {
            CharacterBaseTrainingData data = PlayerTrainingDataOperator.GetTrainingData(type);

            if (data == null)
                return false;

            double price = GetTrainingStatPrice(type);

            if (GoodsManager.Instance.GetGoodsAmount(data.LevelUpCostGoodsType, data.LevelUpCostGoodsParam1) >= price)
            {
                if (PlayerTrainingDataContainer.m_trainingLevel.ContainsKey(data.TrainingType) == false)
                    PlayerTrainingDataContainer.m_trainingLevel.Add(data.TrainingType, 0);

                PlayerTrainingDataContainer.m_trainingLevel[data.TrainingType]++;

#if DEV_DEFINE
                PlayerTrainingDataContainer.m_trainingLevel[data.TrainingType] += CheatAddLevelUP;
#endif

                if (PlayerTrainingDataContainer.m_trainingLevel[data.TrainingType] > data.TrainingMaxLevel)
                    PlayerTrainingDataContainer.m_trainingLevel[data.TrainingType] = data.TrainingMaxLevel;

                if (m_addStatValue.ContainsKey(type) == false)
                { 
                    m_addStatValue.Add(type, new StatElementValue());
                    CharacterStatManager.Instance.AddStatElementValue(type, m_addStatValue[type]);
                }

                m_addStatValue[type].StatValue = GetCurrentTrainingStatValue(type);
                CharacterStatManager.Instance.SetOutPutStatValue(type);

                GoodsManager.Instance.UseGoodsAmount(data.LevelUpCostGoodsType, data.LevelUpCostGoodsParam1, price);

                m_refreshTrainingMsg.TrainingId = -1;
                    Message.Send(m_refreshTrainingMsg);


                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);
                return true;
            }
            else
                return false;
        }
        //------------------------------------------------------------------------------------
        public bool CheckLockTraining(V2Enum_Stat type)
        {
            if (CheckLockTraining_Training(type) == true)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool CheckLockTraining_Training(V2Enum_Stat type)
        {
            CharacterBaseTrainingData data = PlayerTrainingDataOperator.GetTrainingData(type);
            if (data == null)
                return true;

            if (data.OpenConditionIndex == -1)
                return false;

            CharacterBaseTrainingData checkdata = GetCharTrainingData(data.OpenConditionIndex);
            if (checkdata == null)
                return true;

            if (GetCurrentTrainingStatLevel(checkdata.TrainingType) >= data.OpenConditionLevel)
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
    }
}