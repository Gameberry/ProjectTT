using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{

    public class CheckInInfo
    {
        public ObscuredInt CheckInRewardCount = 1;
        public ObscuredDouble NextCheckRewardTime = 0.0;
        public ObscuredDouble NextAdCheckRewardTime = 0.0;
    }

    public static class CheckInContainer
    {
        public static Dictionary<V2Enum_CheckInType, CheckInInfo> m_checkInInfo = new Dictionary<V2Enum_CheckInType, CheckInInfo>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_checkInInfo)
            {
                SerializeString.Append(pair.Key.Enum32ToInt());
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.CheckInRewardCount);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.NextCheckRewardTime);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.NextAdCheckRewardTime);
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

                CheckInInfo checkInInfo = new CheckInInfo();
                checkInInfo.CheckInRewardCount = arrcontent[1].ToInt();
                checkInInfo.NextCheckRewardTime = arrcontent[2].ToDouble();
                checkInInfo.NextAdCheckRewardTime = arrcontent[3].ToDouble();

                m_checkInInfo.Add(arrcontent[0].ToInt().IntToEnum32<V2Enum_CheckInType>(), checkInInfo);
            }
        }
    }

    public static class CheckInOperator
    {
        private static CheckInLocalTable m_checkInLocalTable;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_checkInLocalTable = Managers.TableManager.Instance.GetTableClass<CheckInLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static List<CheckInRewardData> GetCheckInRewardOnceDatas()
        {
            return m_checkInLocalTable.GetCheckInRewardOnceDatas();
        }
        //------------------------------------------------------------------------------------
        public static List<CheckInRewardData> GetCheckInRewardRepeatDatas()
        {
            return m_checkInLocalTable.GetCheckInRewardRepeatDatas();
        }
        //------------------------------------------------------------------------------------
    }
}