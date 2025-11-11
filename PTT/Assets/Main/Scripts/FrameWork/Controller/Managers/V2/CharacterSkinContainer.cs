using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class PlayerSkinInfo
    {
        public ObscuredInt Id;
        public ObscuredInt Count = 0; // 기본값
        public ObscuredInt Level = Define.PlayerSkillDefaultLevel; // 기본값
        public ObscuredInt Star = 0;
    }

    public static class CharacterSkinContainer
    {
        public static Dictionary<int, PlayerSkinInfo> m_skinInfo = new Dictionary<int, PlayerSkinInfo>();
        public static string m_fakeSkinWeaponName = string.Empty;
        public static string m_currentSkinWeaponName = string.Empty;

        public static int m_fakeSkinBodyNumber = -1;
        public static int m_currentSkinBodyNumber = -1;

        public static Dictionary<V2Enum_Skin, ObscuredInt> m_skinEquipId = new Dictionary<V2Enum_Skin, ObscuredInt>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSkinInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_skinInfo)
            {
                SerializeString.Append(pair.Value.Id - 141010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Count);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Star);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSkinInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                PlayerSkinInfo SkinInfo = new PlayerSkinInfo();
                SkinInfo.Id = arrcontent[0].ToInt() + 141010000;
                SkinInfo.Count = arrcontent[1].ToInt();
                SkinInfo.Level = arrcontent[2].ToInt();

                if (arrcontent.Length >= 4)
                    SkinInfo.Star = arrcontent[3].ToInt();
                else
                    SkinInfo.Star = 0;

                m_skinInfo.Add(SkinInfo.Id, SkinInfo);
            }
        }


        public static string GetSkinEquipSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_skinEquipId)
            {
                SerializeString.Append(pair.Key.Enum32ToInt());
                SerializeString.Append(',');
                if (pair.Value == -1)
                    SerializeString.Append(pair.Value);
                else
                    SerializeString.Append(pair.Value - 141010000);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSkinEquipDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');
                int value = arrcontent[1].ToInt();
                if (value != -1)
                    value += 141010000;

                m_skinEquipId.Add(arrcontent[0].ToInt().IntToEnum32<V2Enum_Skin>(), value);
            }
        }
    }

    public static class CharacterSkinOperator
    {
        private static CharacterSkinLocalTable m_characterSkinLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_characterSkinLocalTable = Managers.TableManager.Instance.GetTableClass<CharacterSkinLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static CharacterSkinData GetCharacterSkinData(int index)
        {
            return m_characterSkinLocalTable.GetData(index);
        }
        //------------------------------------------------------------------------------------
        public static List<CharacterSkinData> GetCharacterSkinAllData(V2Enum_Skin v2Enum_Skin)
        {
            return m_characterSkinLocalTable.GeAlltData(v2Enum_Skin);
        }
        //------------------------------------------------------------------------------------
        public static PlayerSkinInfo GetPlayerSkinInfo(CharacterSkinData characterSkinData)
        {
            PlayerSkinInfo platerSkinInfo = null;

            CharacterSkinContainer.m_skinInfo.TryGetValue(characterSkinData.Index, out platerSkinInfo);

            return platerSkinInfo;
        }
        //------------------------------------------------------------------------------------
        public static PlayerSkinInfo AddNewPlayerSkinInfo(CharacterSkinData characterSkinData)
        {
            if (CharacterSkinContainer.m_skinInfo.ContainsKey(characterSkinData.Index) == true)
                return CharacterSkinContainer.m_skinInfo[characterSkinData.Index];
            else
            {
                PlayerSkinInfo platerSkinInfo = new PlayerSkinInfo();
                platerSkinInfo.Id = characterSkinData.Index;
                platerSkinInfo.Count = 0;
                platerSkinInfo.Level = Define.PlayerSkillDefaultLevel;

                CharacterSkinContainer.m_skinInfo.Add(platerSkinInfo.Id, platerSkinInfo);

                return platerSkinInfo;
            }
        }
        //------------------------------------------------------------------------------------
        public static double GetNeedEnhanceSkinPrice(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return 0;

            int mylevel = Define.PlayerSkillDefaultLevel;
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo != null)
                mylevel = playerSkinInfo.Level;

            return (characterSkinData.LevelUpCostGoodsParam2)
                 + (mylevel * characterSkinData.LevelUpCostGoodsParam3);
        }
        //------------------------------------------------------------------------------------
        public static int GetNeedStarUpSkinPrice(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return 0;

            int star = 0;
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo != null)
                star = playerSkinInfo.Star;

            return playerSkinInfo.Star + 2;
        }
        //------------------------------------------------------------------------------------
        public static double GetSkinOwnEffectValue(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return 0.0;

            int mylevel = Define.PlayerSkillDefaultLevel;
            int mystar = 0;
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo != null)
            { 
                mylevel = playerSkinInfo.Level;
                mystar = playerSkinInfo.Star;
            }

            return (characterSkinData.OwnEffectBaseValue + (characterSkinData.OwnEffectAddValue * mylevel)) + (characterSkinData.SurpassAddValue * mystar);
        }
        //------------------------------------------------------------------------------------
        public static CharacterSkinData GetEquipSkin(V2Enum_Skin v2Enum_Skin)
        {
            if (CharacterSkinContainer.m_skinEquipId.ContainsKey(v2Enum_Skin) == false)
                return null;
            else
                return GetCharacterSkinData(CharacterSkinContainer.m_skinEquipId[v2Enum_Skin]);
        }
        //------------------------------------------------------------------------------------
        public static bool IsEquipSkin(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return false;

            if (CharacterSkinContainer.m_skinEquipId.ContainsKey(characterSkinData.SkinType) == false)
                return false;
            else
                return CharacterSkinContainer.m_skinEquipId[characterSkinData.SkinType] == characterSkinData.Index;
        }
        //------------------------------------------------------------------------------------
        public static bool DoEquipSkin(CharacterSkinData characterSkinData)
        {
            if (GetPlayerSkinInfo(characterSkinData) == null)
                return false;

            if (CharacterSkinContainer.m_skinEquipId.ContainsKey(characterSkinData.SkinType) == false)
                CharacterSkinContainer.m_skinEquipId.Add(characterSkinData.SkinType, characterSkinData.Index);
            else
                CharacterSkinContainer.m_skinEquipId[characterSkinData.SkinType] = characterSkinData.Index;

            return true;
        }
        //------------------------------------------------------------------------------------
        public static void UhEquipSkin(V2Enum_Skin SkinType)
        {
            if (CharacterSkinContainer.m_skinEquipId.ContainsKey(SkinType) == true)
                CharacterSkinContainer.m_skinEquipId[SkinType] = -1;
        }
        //------------------------------------------------------------------------------------
    }
}