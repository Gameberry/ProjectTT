using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class PlayerAdBuffInfo
    {
        public ObscuredInt Index;
        public ObscuredDouble BuffActiveTime = -1.0;
    }

    public static class AdBuffContainer
    {
        public static Dictionary<int, PlayerAdBuffInfo> AdBuffInfo = new Dictionary<int, PlayerAdBuffInfo>();

        public static ObscuredDouble DailyInitTimeStemp = 0;

        public static ObscuredInt BuffActiveCount = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in AdBuffInfo)
            {
                SerializeString.Append(pair.Value.Index - 117010000);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.BuffActiveTime);
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

                PlayerAdBuffInfo playerAdBuffInfo = new PlayerAdBuffInfo();
                playerAdBuffInfo.Index = arrcontent[0].ToInt() + 117010000;
                playerAdBuffInfo.BuffActiveTime = arrcontent[1].ToDouble();

                AdBuffInfo.Add(playerAdBuffInfo.Index, playerAdBuffInfo);
            }
        }
    }

    public static class AdBuffOperator
    {
        public static AdBuffLocalTable m_adBuffLocalTable;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_adBuffLocalTable = Managers.TableManager.Instance.GetTableClass<AdBuffLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<int, AdBuffActiveData> GetAllBuffActivaData()
        {
            return m_adBuffLocalTable.GetAllBuffActivaData();
        }
        //------------------------------------------------------------------------------------
        public static AdBuffActiveData GetBuffActiveData(int index)
        {
            return m_adBuffLocalTable.GetBuffActiveData(index);
        }
        //------------------------------------------------------------------------------------
        public static PlayerAdBuffInfo GetBuffActiveInfo(int index)
        {
            if (AdBuffContainer.AdBuffInfo.ContainsKey(index) == true)
                return AdBuffContainer.AdBuffInfo[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}