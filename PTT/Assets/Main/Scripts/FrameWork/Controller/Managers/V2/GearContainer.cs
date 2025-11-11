using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class GearContainer
    {
        // 플레이어가 소유한 룬
        public static Dictionary<ObscuredInt, SkillInfo> SynergyInfo = new Dictionary<ObscuredInt, SkillInfo>();

        // 플레이어가 착용한 룬
        public static Dictionary<V2Enum_GearType, ObscuredInt> SynergyEquip_Dic = new Dictionary<V2Enum_GearType, ObscuredInt>();

        // 슬롯 레벨
        public static Dictionary<V2Enum_GearType, ObscuredInt> SlotLevel_Dic = new Dictionary<V2Enum_GearType, ObscuredInt>();

        // 플레이어 신규 룬 <SynergyIndex>
        public static Dictionary<GearData, int> NewSynergys = new Dictionary<GearData, int>();

        public static ObscuredInt AccumCombineCount = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();


        public static string GetSynergyInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyInfo)
            {
                SerializeString.Append(pair.Value.Id - 111010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Count);
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
                skillInfo.Id = arrcontent[0].ToInt() + 111010000;
                skillInfo.Count = arrcontent[1].ToInt();

                SynergyInfo.Add(skillInfo.Id, skillInfo);
            }
        }




        public static string GetSynergyInfoEquipSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyEquip_Dic)
            {
                SerializeString.Append(pair.Key.Enum32ToInt());
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.GetDecrypted());
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
                SerializeString.Append('/');
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

                SynergyEquip_Dic.Add(arrcontent[0].ToInt().IntToEnum32<V2Enum_GearType>(), arrcontent[1].ToInt());
            }
        }



        public static string GetSynergyInfoSlotSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SlotLevel_Dic)
            {
                SerializeString.Append(pair.Key.Enum32ToInt());
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.GetDecrypted());
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }



        public static void SetSynergyInfoSlotSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                SlotLevel_Dic.Add(arrcontent[0].ToInt().IntToEnum32<V2Enum_GearType>(), arrcontent[1].ToInt());
            }
        }



        public static string GetNewSynergySerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in NewSynergys)
            {
                if (pair.Key == null)
                    continue;

                SerializeString.Append(pair.Key.Index - 111010000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }
    }

    public static class CharacterGearOperator
    {
        private static GearLocalTable _synergyLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _synergyLocalTable = Managers.TableManager.Instance.GetTableClass<GearLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, GearData> GetAllGearData_Dic()
        {
            return _synergyLocalTable.GetAllGearData_Dic();
        }
        //------------------------------------------------------------------------------------
        public static List<GearData> GetAllGearData()
        {
            return _synergyLocalTable.GetAllGearData();
        }
        //------------------------------------------------------------------------------------
        public static List<GearData> GetAllGearData(V2Enum_Grade v2Enum_Grade)
        {
            return _synergyLocalTable.GetAllGearData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static List<GearData> GetAllGearData(V2Enum_GearType v2Enum_Grade)
        {
            return _synergyLocalTable.GetAllGearData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static GearData GetGearData(ObscuredInt index)
        {
            return _synergyLocalTable.GetGearData(index);
        }
        //------------------------------------------------------------------------------------
        public static GearLevelUpCostData GetGearLevelUpCostData(V2Enum_GearType v2Enum_Grade)
        {
            return _synergyLocalTable.GetGearLevelUpCostData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static GearCombineData GetGearCombineData(V2Enum_Grade grade)
        {
            return _synergyLocalTable.GetGearCombineData(grade);
        }
        //------------------------------------------------------------------------------------
        public static GearOptionData GetGearOptionData(V2Enum_GearType v2Enum_Grade, V2Enum_ARR_SynergyType gearNumber)
        {
            return _synergyLocalTable.GetGearOptionData(v2Enum_Grade, gearNumber);
        }
        //------------------------------------------------------------------------------------
    }
}