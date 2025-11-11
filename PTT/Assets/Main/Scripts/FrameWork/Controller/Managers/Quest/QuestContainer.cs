using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class QuestInfo
    {
        public ObscuredInt Index;

        public ObscuredDouble DoActionCount = 0.0;
        public ObscuredInt RecvCount = 0;
        public ObscuredDouble InitTimeStemp = 0;
    }

    public class QuestGaugeInfo
    {
        public V2Enum_QuestType QuestType;
        public ObscuredInt RecvRequiredQuestCount = 0;
        public ObscuredDouble InitTimeStemp = 0;
    }

    public static class QuestContainer
    {
        public static Dictionary<int, QuestInfo> QuestInfos = new Dictionary<int, QuestInfo>();

        public static Dictionary<V2Enum_QuestType, QuestGaugeInfo> QuestGaugeInfos = new Dictionary<V2Enum_QuestType, QuestGaugeInfo>();

        public static ObscuredDouble DaliyInitTimeStemp = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetQuestInfosSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in QuestInfos)
            {
                SerializeString.Append(pair.Value.Index);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.DoActionCount);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.RecvCount);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.InitTimeStemp);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetQuestInfosDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                QuestInfo questInfo = new QuestInfo();
                questInfo.Index = arrcontent[0].ToInt();
                questInfo.DoActionCount = arrcontent[1].ToInt();
                questInfo.RecvCount = arrcontent[2].ToInt();
                questInfo.InitTimeStemp = arrcontent[3].ToDouble();

                QuestInfos.Add(questInfo.Index, questInfo);
            }
        }


        public static string GetQuestGaugeInfosSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in QuestGaugeInfos)
            {
                SerializeString.Append(pair.Value.QuestType.Enum32ToInt());
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.RecvRequiredQuestCount);
                SerializeString.Append(',');
                SerializeString.Append((long)pair.Value.InitTimeStemp);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetQuestGaugeInfosDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                QuestGaugeInfo missionDailyInfo = new QuestGaugeInfo();
                missionDailyInfo.QuestType = arrcontent[0].ToInt().IntToEnum32<V2Enum_QuestType>();
                missionDailyInfo.RecvRequiredQuestCount = arrcontent[1].ToInt();
                missionDailyInfo.InitTimeStemp = arrcontent[2].ToDouble();

                QuestGaugeInfos.Add(missionDailyInfo.QuestType, missionDailyInfo);
            }
        }
    }

    public static class QuestOperator
    {
        private static QuestLocalTable m_missionLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            m_missionLocalTable = Managers.TableManager.Instance.GetTableClass<QuestLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static List<QuestData> GetQuestDatas(V2Enum_QuestType v2Enum_QuestType)
        {
            return m_missionLocalTable.GetQuestDatas(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        public static List<QuestGaugeData> GetQuestGaugeDatas(V2Enum_QuestType v2Enum_QuestType)
        {
            return m_missionLocalTable.GetQuestGaugeDatas(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        public static ObscuredInt GetQuestGaugeMaxCount(V2Enum_QuestType v2Enum_QuestType)
        {
            return m_missionLocalTable.GetQuestGaugeMaxCount(v2Enum_QuestType);
        }
        //------------------------------------------------------------------------------------
        public static QuestData GetMissionData(int index)
        {
            return m_missionLocalTable.GetQuestData(index);
        }
        //------------------------------------------------------------------------------------
        public static QuestInfo MissionDailyInfo(int index)
        {
            if (QuestContainer.QuestInfos.ContainsKey(index) == true)
                return QuestContainer.QuestInfos[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}