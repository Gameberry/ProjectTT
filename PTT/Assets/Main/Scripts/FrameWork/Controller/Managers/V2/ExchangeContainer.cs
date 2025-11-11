using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class ExchangeInfo
    {
        public ObscuredInt Index;
        public ObscuredInt ToDayExchangeCount = 0;
        public ObscuredDouble InitTimeStemp = 0;
    }

    public static class ExchangeContainer
    {
        public static Dictionary<int, ExchangeInfo> ExchangeInfos = new Dictionary<int, ExchangeInfo>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in ExchangeInfos)
            {
                SerializeString.Append(pair.Value.Index - 152010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.ToDayExchangeCount);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.InitTimeStemp);
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

                ExchangeInfo missionDailyInfo = new ExchangeInfo();
                missionDailyInfo.Index = arrcontent[0].ToInt() + 152010000;
                missionDailyInfo.ToDayExchangeCount = arrcontent[1].ToInt();
                missionDailyInfo.InitTimeStemp = arrcontent[2].ToDouble();

                ExchangeInfos.Add(missionDailyInfo.Index, missionDailyInfo);
            }
        }

    }
}