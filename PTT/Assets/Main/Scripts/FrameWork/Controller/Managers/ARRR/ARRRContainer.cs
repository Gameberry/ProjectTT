using System;
using System.Collections.Generic;
using Gpm.Ui;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class PlayerCharacterInfo : CreatureInfo
    {
        public ObscuredInt CharacterIndex;

        public ObscuredInt Count = 0;
        public ObscuredInt Enable = 0;

        public ObscuredInt Star = 0;
        public ObscuredInt Level = Define.CreatureDefaultLevel;

        public string myGamerInData = string.Empty; // 길드 전용
    }

    public class ARRRInfo
    {
        public int SkinNumber = 0;

        public Dictionary<V2Enum_Stat, ObscuredDouble> DefaultStatValue = new Dictionary<V2Enum_Stat, ObscuredDouble>();

        public List<SkillInfo> EquipSkillData = new List<SkillInfo>();

        public List<PetInfo> EquipPetInfo = new List<PetInfo>();
    }

    public static class ARRRContainer
    {
        public static ObscuredInt ARRRLevel = 1;
        public static ObscuredInt ARRRLimitCompleteLevel = 0;

        public static ObscuredInt RecvDefaultGoods = 0;
        public static Dictionary<ObscuredInt, ObscuredDouble> DefaultGoods = new Dictionary<ObscuredInt, ObscuredDouble>();

        public static List<PlayerCharacterInfo> PlayerCharacterInfos = new List<PlayerCharacterInfo>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetAllyInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in PlayerCharacterInfos)
            {
                if (pair.Enable == 0)
                    continue;

                SerializeString.Append(pair.CreatureIndex/* - 104010000*/);
                SerializeString.Append(',');
                SerializeString.Append(pair.CharacterIndex);
                SerializeString.Append(',');
                SerializeString.Append(pair.Count);
                SerializeString.Append(',');
                SerializeString.Append(pair.Star);
                SerializeString.Append(',');
                SerializeString.Append(pair.Level);
                SerializeString.Append(',');
                SerializeString.Append(pair.Enable);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetAllyInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                PlayerCharacterInfo playerV3AllyInfo = new PlayerCharacterInfo();
                playerV3AllyInfo.CreatureIndex = arrcontent[0].ToInt()/* + 104010000*/;
                playerV3AllyInfo.CharacterIndex = arrcontent[1].ToInt();
                playerV3AllyInfo.Count = arrcontent[2].ToInt();
                playerV3AllyInfo.Star = arrcontent[3].ToInt();
                if (playerV3AllyInfo.Star < 0)
                    playerV3AllyInfo.Star = 0;
                playerV3AllyInfo.Level = arrcontent[4].ToInt();
                playerV3AllyInfo.Enable = arrcontent[5].ToInt();

                PlayerCharacterInfos.Add(playerV3AllyInfo);
            }
        }
    }

    public static class ARRRStatOperator
    {
        //------------------------------------------------------------------------------------
        public static V2Enum_Stat ConvertCrowdControlTypeToStat(V2Enum_SkillEffectType v2Enum_CrowdControlType)
        {
            switch (v2Enum_CrowdControlType)
            {
                case V2Enum_SkillEffectType.IncreaseAtt:
                case V2Enum_SkillEffectType.DecreaseAtt:
                    return V2Enum_Stat.Attack;
                case V2Enum_SkillEffectType.IncreaseArmor:
                case V2Enum_SkillEffectType.DecreaseArmor:
                    return V2Enum_Stat.Defence;
                case V2Enum_SkillEffectType.IncreaseMoveSpeed:
                    return V2Enum_Stat.MoveSpeed;
                case V2Enum_SkillEffectType.SkillCooltimeReduce:
                    return V2Enum_Stat.SkillCoolTimeDecrease;
                case V2Enum_SkillEffectType.IncreaseHp:
                    return V2Enum_Stat.HP;
            }

            return V2Enum_Stat.Max;
        }
        //------------------------------------------------------------------------------------
    }
}