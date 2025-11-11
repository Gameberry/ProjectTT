using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class ShopPostInfo
    {
        public ObscuredString InData; // key°¡ µÈ´Ù.
        public ObscuredInt ShopIndex;
        public ObscuredLong Make_TimeStamp;
        public List<RewardData> rewardDatas = new List<RewardData>();

        public bool IsRecv;
    }

    public static class ShopPostContainer
    {
        public static Dictionary<string, PostInfo> m_myShopPostInfo = new Dictionary<string, PostInfo>();

        public static List<ShopPostInfo> m_shopPostInfos = new List<ShopPostInfo>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_shopPostInfos)
            {
                SerializeString.Append(pair.InData);
                SerializeString.Append(',');
                SerializeString.Append(pair.ShopIndex);
                SerializeString.Append(',');
                SerializeString.Append(pair.Make_TimeStamp);
                SerializeString.Append(',');

                for (int i = 0; i < pair.rewardDatas.Count; ++i)
                {
                    RewardData rewardData = pair.rewardDatas[i];
                    SerializeString.Append(rewardData.V2Enum_Goods.Enum32ToInt());
                    SerializeString.Append('_');
                    SerializeString.Append(rewardData.Index);
                    SerializeString.Append('_');
                    SerializeString.Append(rewardData.Amount);
                    SerializeString.Append(':');
                }

                if (SerializeString.Length > 0)
                    SerializeString.Remove(SerializeString.Length - 1, 1);

                SerializeString.Append(',');
                SerializeString.Append(pair.IsRecv == true ? 1 : 0);

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

                ShopPostInfo playerShopInfo = new ShopPostInfo();
                playerShopInfo.InData = arrcontent[0];
                playerShopInfo.ShopIndex = arrcontent[1].ToInt();
                playerShopInfo.Make_TimeStamp = arrcontent[2].ToLong();


                string[] rewardDatas = arrcontent[3].Split(':');

                for (int j = 0; j < rewardDatas.Length; ++j)
                {
                    RewardData rewardData = new RewardData();

                    string[] rewardContent = rewardDatas[j].Split('_');

                    rewardData.IsPoolData = false;
                    rewardData.V2Enum_Goods = rewardContent[0].ToInt().IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = rewardContent[1].ToInt();
                    rewardData.Amount = rewardContent[2].ToDouble();

                    playerShopInfo.rewardDatas.Add(rewardData);
                }

                playerShopInfo.IsRecv = arrcontent[4].ToInt() == 1 ? true : false;

                m_shopPostInfos.Add(playerShopInfo);
            }
        }
    }
}