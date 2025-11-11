using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class TimeAttackMissionInfo
    {
        public ObscuredInt Index;

        public int RecvCount = 0;
        public ObscuredDouble FinishTimeStemp = 0;
    }

    public static class TimeAttackMissionContainer
    {
        public static Dictionary<int, TimeAttackMissionInfo> TimeAttackMissionInfos = new Dictionary<int, TimeAttackMissionInfo>();

        public static ObscuredInt FocusMission = -1;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetTimeAttackMissionInfosSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in TimeAttackMissionInfos)
            {
                SerializeString.Append(pair.Value.Index);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.RecvCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.FinishTimeStemp);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetTimeAttackMissionInfosDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                TimeAttackMissionInfo questInfo = new TimeAttackMissionInfo();
                questInfo.Index = arrcontent[0].ToInt();
                questInfo.RecvCount = arrcontent[1].ToInt();
                questInfo.FinishTimeStemp = arrcontent[2].ToDouble();

                TimeAttackMissionInfos.Add(questInfo.Index, questInfo);
            }
        }
    }

    public static class TimeAttackMissionOperator
    {
        private static TimeAttackMissionLocalTable m_missionLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_missionLocalTable = Managers.TableManager.Instance.GetTableClass<TimeAttackMissionLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, TimeAttackMissionData> GetAllTimeAttackMissionData()
        {
            return m_missionLocalTable.GetAllTimeAttackMissionData();
        }
        //------------------------------------------------------------------------------------
        public static TimeAttackMissionData GetTimeAttackMissionData(ObscuredInt index)
        {
            return m_missionLocalTable.GetTimeAttackMissionData(index);
        }
        //------------------------------------------------------------------------------------

    }
}