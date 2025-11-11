using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class SkillInfo
    {
        public ObscuredInt Id;
        public ObscuredInt Level = Define.PlayerSkillDefaultLevel;
        public ObscuredInt Count = 0;
        public ObscuredInt LimitCompleteLevel = 0;

        public DescendData descend = null;
    }


    public static class ARRRSkillContainer
    {
        // 플레이어가 소유한 스킬
        public static Dictionary<ObscuredInt, SkillInfo> _skillInfo = new Dictionary<ObscuredInt, SkillInfo>();

        // 플레이어 스킬슬롯 <SlotID, ArrrSkillId>
        public static Dictionary<ObscuredInt, ARRRSkillData> _skillSlotData = new Dictionary<ObscuredInt, ARRRSkillData>();

        public static List<ObscuredInt> _skillTempSlotData = new List<ObscuredInt>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSkillInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in _skillInfo)
            {
                SerializeString.Append(pair.Value.Id - 104050000);
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

        public static void SetSkillInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                SkillInfo skillInfo = new SkillInfo();
                skillInfo.Id = arrcontent[0].ToInt() + 104050000;
                skillInfo.Count = arrcontent[1].ToInt();
                skillInfo.Level = arrcontent[2].ToInt();
                skillInfo.LimitCompleteLevel = arrcontent[3].ToInt();

                _skillInfo.Add(skillInfo.Id, skillInfo);
            }
        }

        public static string GetSkillSlotSerializeString()
        {
            SerializeString.Clear();


            foreach (var pair in _skillSlotData)
            { 
                if(pair.Value == null)
                    SerializeString.Append(-1);
                else
                    SerializeString.Append(pair.Value.Index - 104050000);

                SerializeString.Append(',');
            }


            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSkillSlotDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split(',');

            for (int i = 0; i < arr.Length; ++i)
            {
                _skillTempSlotData.Add(arr[i].ToInt() + 104050000);
            }
        }
    }
}