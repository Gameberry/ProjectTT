using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class GambleContainer
    {
        
    }

    public static class GambleOperator
    {
        private static GambleLocalTable _gambleLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _gambleLocalTable = Managers.TableManager.Instance.GetTableClass<GambleLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static List<GambleCardData> GetGambleCardProbDatas()
        {
            return _gambleLocalTable.GetGambleCardProbDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<GambleGradeProbData> GetGambleProbDatas()
        {
            return _gambleLocalTable.GetGambleProbDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<GambleGradeProbData> GetGambleProbDatas(ObscuredInt level)
        {
            return _gambleLocalTable.GetGambleProbDatas(level);
        }
        //------------------------------------------------------------------------------------
        public static WeightedRandomPicker<GambleGradeProbData> GetGambleGradePicker(ObscuredInt level)
        {
            return _gambleLocalTable.GetGambleGradePicker(level);
        }
        //------------------------------------------------------------------------------------
        public static List<GambleSkillProbData> GetGambleSkillProbs(V2Enum_Grade V2Enum_Grade)
        {
            return _gambleLocalTable.GetGambleSkillProbs(V2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static List<GambleSkillProbData> GetGambleSkillProbs(V2Enum_ARR_SynergyType v2Enum_ARR_Card, V2Enum_Grade V2Enum_Grade)
        {
            return _gambleLocalTable.GetGambleSkillProbs(v2Enum_ARR_Card, V2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<V2Enum_ARR_GambleSlotType, List<GambleSlotProbData>> GetGambleSlotProbData_Dic()
        {
            return _gambleLocalTable.GetGambleSlotProbData_Dic();
        }
        //------------------------------------------------------------------------------------
        public static List<GambleSlotIncreaseValueData> GetGambleSlotIncreaseValueDatas(ObscuredInt index)
        {
            return _gambleLocalTable.GetGambleSlotIncreaseValueDatas(index);
        }
        //------------------------------------------------------------------------------------
        public static List<V2Enum_Stat> GetDisplayStatList()
        {
            return _gambleLocalTable.GetDisplayStatList();
        }
        //------------------------------------------------------------------------------------
        public static GambleProbChanceData GetGambleProbChanceData()
        {
            return _gambleLocalTable.GetGambleProbChanceData();
        }
        //------------------------------------------------------------------------------------
        public static GambleCostData GetGambleCostData(V2Enum_ARR_GambleType v2Enum_ARR_GambleType)
        {
            return _gambleLocalTable.GetGambleCostData(v2Enum_ARR_GambleType);
        }
        //------------------------------------------------------------------------------------
    }
}