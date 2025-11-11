using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class SynergyRuneContainer
    {
        // 플레이어가 소유한 룬
        public static Dictionary<ObscuredInt, SkillInfo> SynergyInfo = new Dictionary<ObscuredInt, SkillInfo>();

        // 플레이어가 착용한 룬
        public static Dictionary<ObscuredInt, ObscuredInt> SynergyEquip_Dic = new Dictionary<ObscuredInt, ObscuredInt>();

        // 플레이어 신규 룬 <SynergyIndex>
        public static Dictionary<SynergyRuneData, int> NewSynergys = new Dictionary<SynergyRuneData, int>();

        public static ObscuredInt AccumCombineCount = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();


        public static string GetSynergyInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyInfo)
            {
                SerializeString.Append(pair.Value.Id - 118010000);
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
                skillInfo.Id = arrcontent[0].ToInt() + 118010000;
                skillInfo.Count = arrcontent[1].ToInt();

                SynergyInfo.Add(skillInfo.Id, skillInfo);
            }
        }




        public static string GetSynergyInfoEquipSerializeString()
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

            string[] arr = data.Split(',');

            for (int i = 0; i < arr.Length; ++i)
            {
                SynergyEquip_Dic.Add(i + 1, arr[i].ToInt());
            }
        }


        public static string GetNewSynergySerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in NewSynergys)
            {
                if (pair.Key == null)
                    continue;

                SerializeString.Append(pair.Key.Index - 118010000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

    }

    public static class SynergyRuneOperator
    {
        private static SynergyRuneLocalTable _synergyLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _synergyLocalTable = Managers.TableManager.Instance.GetTableClass<SynergyRuneLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, SynergyRuneData> GetAllSynergyRuneData_Dic()
        {
            return _synergyLocalTable.GetAllSynergyRuneData_Dic();
        }
        //------------------------------------------------------------------------------------
        public static List<SynergyRuneData> GetAllSynergyRuneData()
        {
            return _synergyLocalTable.GetAllSynergyRuneData();
        }
        //------------------------------------------------------------------------------------
        public static List<SynergyRuneData> GetAllSynergyRuneData(V2Enum_Grade v2Enum_Grade)
        {
            return _synergyLocalTable.GetAllSynergyRuneData(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public static SynergyRuneData GetSynergyRuneData(ObscuredInt index)
        {
            return _synergyLocalTable.GetSynergyRuneData(index);
        }
        //------------------------------------------------------------------------------------
        public static SynergyRuneOpenconditionData GetSynergyRuneOpenconditionData(ObscuredInt slotNumber)
        {
            return _synergyLocalTable.GetSynergyRuneOpenconditionData(slotNumber);
        }
        //------------------------------------------------------------------------------------
        public static SynergyRuneCombineData GetSynergyRuneCombineData(V2Enum_Grade grade)
        {
            return _synergyLocalTable.GetSynergyRuneCombineData(grade);
        }
        //------------------------------------------------------------------------------------
    }
}