using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class PlayerPassInfo
    {
        public ObscuredInt Id;

        public ObscuredDouble RecvFreeRewardCount = 0;
        public ObscuredDouble RecvPaidRewardCount = 0;

        public ObscuredInt IsBuy = 0;
        public ObscuredInt IsEnable = 0;
    }

    public static class PassContainer
    {
        public static Dictionary<int, PlayerPassInfo> m_passInfos = new Dictionary<int, PlayerPassInfo>();

        public static ObscuredInt AccumMonster = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_passInfos)
            {
                SerializeString.Append(pair.Value.Id - 151010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.RecvFreeRewardCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.RecvPaidRewardCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.IsBuy);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.IsEnable);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                PlayerPassInfo playerPassInfo = new PlayerPassInfo();
                playerPassInfo.Id = arrcontent[0].ToInt() + 151010000;
                playerPassInfo.RecvFreeRewardCount = arrcontent[1].ToDouble();
                playerPassInfo.RecvPaidRewardCount = arrcontent[2].ToDouble();
                playerPassInfo.IsBuy = arrcontent[3].ToInt();
                playerPassInfo.IsEnable = arrcontent[4].ToInt();

                m_passInfos.Add(playerPassInfo.Id, playerPassInfo);
            }
        }
    }

    public static class PassOperator
    {
        private static PassLocalTable m_passLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_passLocalTable = Managers.TableManager.Instance.GetTableClass<PassLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static PassData GetPassData(int index)
        {
            return m_passLocalTable.GetPassData(index);
        }
        //------------------------------------------------------------------------------------
        public static PassData GetPassData_RewardIndex(int rewardIndex)
        {
            return m_passLocalTable.GetPassData_RewardIndex(rewardIndex);
        }
        //------------------------------------------------------------------------------------
        public static List<PassData> GetPassDatas(V2Enum_PassType v2Enum_PassType)
        {
            return m_passLocalTable.GetPassDatas(v2Enum_PassType);
        }
        //------------------------------------------------------------------------------------
        public static int GetBerserkerKillCountMax()
        {
            return m_passLocalTable.berserkerKillPass.GetDecrypted();
        }
        //------------------------------------------------------------------------------------
        public static PlayerPassInfo CreatePlayerShopInfo(PassData passData)
        {
            if (passData == null)
                return null;

            if (PassContainer.m_passInfos.ContainsKey(passData.Index) == true)
                return PassContainer.m_passInfos[passData.Index];

            PlayerPassInfo playerPassInfo = new PlayerPassInfo();
            playerPassInfo.Id = passData.Index;
            playerPassInfo.RecvFreeRewardCount = 0;
            playerPassInfo.RecvPaidRewardCount = 0;
            playerPassInfo.IsBuy = 0;

            PassContainer.m_passInfos.Add(playerPassInfo.Id, playerPassInfo);

            return playerPassInfo;
        }
        //------------------------------------------------------------------------------------
    }
}