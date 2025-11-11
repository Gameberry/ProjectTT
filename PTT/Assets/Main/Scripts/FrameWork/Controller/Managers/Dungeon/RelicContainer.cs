using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class RelicContainer
    {
        // 플레이어가 소유한 시너지
        public static Dictionary<ObscuredInt, SkillInfo> SynergyInfo = new Dictionary<ObscuredInt, SkillInfo>();

        // 플레이어 신규 시너지 <SynergyIndex>
        public static Dictionary<RelicData, int> NewSynergys = new Dictionary<RelicData, int>();

        public static ObscuredInt SynergyAccumLevel = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetRelicInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyInfo)
            {
                SerializeString.Append(pair.Value.Id - 112010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Count);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetRelicInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                SkillInfo skillInfo = new SkillInfo();
                skillInfo.Id = arrcontent[0].ToInt() + 112010000;
                skillInfo.Count = arrcontent[1].ToInt();
                skillInfo.Level = arrcontent[2].ToInt();

                SynergyInfo.Add(skillInfo.Id, skillInfo);
            }
        }


        public static string GetNewSynergySerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in NewSynergys)
            {
                if (pair.Key == null)
                    continue;

                SerializeString.Append(pair.Key.Index - 112010000);
                SerializeString.Append(',');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }
    }

    public static class RelicOperator
    {
        private static RelicLocalTable _relicLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _relicLocalTable = Managers.TableManager.Instance.GetTableClass<RelicLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, RelicData> GetAllRelicData()
        {
            return _relicLocalTable.GetAllRelicData();
        }
        //------------------------------------------------------------------------------------
        public static RelicData GetRelicData(ObscuredInt index)
        {
            return _relicLocalTable.GetRelicData(index);
        }
        //------------------------------------------------------------------------------------
        public static RelicLevelUpCostData GetRelicLevelUpCostData(ObscuredInt level)
        {
            return _relicLocalTable.GetRelicLevelUpCostData(level);
        }
        //------------------------------------------------------------------------------------
    }
}