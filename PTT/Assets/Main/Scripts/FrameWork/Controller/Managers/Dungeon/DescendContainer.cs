using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class DescendContainer
    {
        // 플레이어가 소유한 시너지
        public static Dictionary<ObscuredInt, SkillInfo> SynergyInfo = new Dictionary<ObscuredInt, SkillInfo>();

        // 플레이어 장착 시너지 <SlotID, ArrrSkillId>
        public static Dictionary<ObscuredInt, ObscuredInt> SynergyEquip_Dic = new Dictionary<ObscuredInt, ObscuredInt>();

        // 플레이어 신규 시너지 <SynergyIndex>
        public static Dictionary<DescendData, int> NewSynergys = new Dictionary<DescendData, int>();




        public static HashSet<ObscuredInt> Runes = new HashSet<ObscuredInt>();

        // 플레이어 신규 룬 <RuneIndex>
        public static Dictionary<DescendBreakthroughData, int> NewRunes = new Dictionary<DescendBreakthroughData, int>();




        public static ObscuredInt SynergyAccumLevel = 0;


        public static ObscuredInt SynergyServerRecvExp = 0;
        public static ObscuredInt SynergyContentExp = 0;

        public static SynergyTotalLevelCostData SynergyTotalLevelCostData = new SynergyTotalLevelCostData();

        public static DescendIngameEnforceData DescendIngameEnforceData = new DescendIngameEnforceData();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();


        public static string GetSynergyInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyInfo)
            {
                SerializeString.Append(pair.Value.Id - 110010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Count);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.LimitCompleteLevel);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSynergyInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                SkillInfo skillInfo = new SkillInfo();
                skillInfo.Id = arrcontent[0].ToInt() + 110010000;
                skillInfo.Count = arrcontent[1].ToInt();
                skillInfo.Level = arrcontent[2].ToInt();
                skillInfo.LimitCompleteLevel = arrcontent[3].ToInt();

                SynergyInfo.Add(skillInfo.Id, skillInfo);
            }
        }




        public static string GetSynergyInfoEquipSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyEquip_Dic)
            {
                SerializeString.Append(pair.Key);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value - 110010000);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }


        public static string GetLogEquipInfo()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyEquip_Dic)
            {
                SerializeString.Append(pair.Value);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSynergyInfoEquipSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                int slotid = arrcontent[0].ToInt();

                if (SynergyEquip_Dic.ContainsKey(slotid) == false)
                    SynergyEquip_Dic.Add(slotid, arrcontent[1].ToInt() + 110010000);
            }
        }

        public static string GetRuneInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in Runes)
            {
                SerializeString.Append(pair - 110031000);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetRuneInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                Runes.Add(arr[i].ToInt() + 110031000);
            }
        }

        public static string GetNewSynergySerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in NewSynergys)
            {
                if (pair.Key == null)
                    continue;

                SerializeString.Append(pair.Key.Index - 110010000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static string GetNewRuneSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in NewRunes)
            {
                if (pair.Key == null)
                    continue;

                SerializeString.Append(pair.Key.Index - 110031000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }
    }

    public static class DescendOperator
    {
        private static DescendLocalTable _synergyLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _synergyLocalTable = Managers.TableManager.Instance.GetTableClass<DescendLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static double GetDescendPhaseValue(ObscuredInt level)
        {
            return _synergyLocalTable.GetDescendPhaseValue(level);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, DescendData> GetAllSynergyEffectDatas()
        {
            return _synergyLocalTable.GetAllSynergyEffectDatas();
        }
        //------------------------------------------------------------------------------------
        public static DescendData GetSynergyEffectData(ObscuredInt index)
        {
            return _synergyLocalTable.GetSynergyEffectData(index);
        }
        //------------------------------------------------------------------------------------
        public static DescendLevelUpCostData GetSynergyLevelUpCostData(ObscuredInt level)
        {
            return _synergyLocalTable.GetSynergyLevelUpCostData(level);
        }
        //------------------------------------------------------------------------------------
        public static DescendBreakthroughData GetSynergyRuneData(ObscuredInt index)
        {
            return _synergyLocalTable.GetSynergyRuneData(index);
        }
        //------------------------------------------------------------------------------------
        public static DescendBreakthroughCostData GetSynergyLevelUpLimitData(ObscuredInt level)
        {
            return _synergyLocalTable.GetSynergyLevelUpLimitData(level + 1);
        }
        //------------------------------------------------------------------------------------
        public static DescendDuplicationData GetSynergyDuplicationData(ObscuredInt index)
        {
            return _synergyLocalTable.GetSynergyDuplicationData(index);
        }
        //------------------------------------------------------------------------------------
        public static DescendLevelUpStatData GetSynergyReinforceStatData(ObscuredInt level)
        {
            return _synergyLocalTable.GetSynergyReinforceStatData(level);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, DescendSlotConditionData> GetAllDescendSlotConditionDatas()
        {
            return _synergyLocalTable.GetAllDescendSlotConditionDatas();
        }
        //------------------------------------------------------------------------------------
        public static DescendSlotConditionData GetDescendSlotConditionData(ObscuredInt slot)
        {
            return _synergyLocalTable.GetDescendSlotConditionData(slot);
        }
        //------------------------------------------------------------------------------------
    }
}