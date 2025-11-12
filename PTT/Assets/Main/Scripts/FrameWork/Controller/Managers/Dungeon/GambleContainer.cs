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
        public static List<GambleSkillProbData> GetGambleSkillProbs(Enum_SynergyType Enum_Card, V2Enum_Grade V2Enum_Grade)
        {
            return _gambleLocalTable.GetGambleSkillProbs(Enum_Card, V2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<Enum_GambleSlotType, List<GambleSlotProbData>> GetGambleSlotProbData_Dic()
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
        public static GambleCostData GetGambleCostData(Enum_GambleType Enum_GambleType)
        {
            return _gambleLocalTable.GetGambleCostData(Enum_GambleType);
        }
        //------------------------------------------------------------------------------------
    }
}