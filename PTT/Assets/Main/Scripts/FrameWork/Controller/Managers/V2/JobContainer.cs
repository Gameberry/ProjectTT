using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class JobContainer
    {
        // 플레이어가 소유한 룬
        public static Dictionary<ObscuredInt, SkillInfo> SynergyInfo = new Dictionary<ObscuredInt, SkillInfo>();

        public static ObscuredInt JobType = -1;
        public static ObscuredInt JobTier = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSynergyInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in SynergyInfo)
            {
                SerializeString.Append(pair.Value.Id);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.Level);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSynergyInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                SkillInfo skillInfo = new SkillInfo();
                skillInfo.Id = arrcontent[0].ToInt();
                skillInfo.Level = arrcontent[1].ToInt();

                SynergyInfo.Add(skillInfo.Id, skillInfo);
            }
        }


    }

    public static class JobOperator
    {
        private static JobLocalTable _synergyLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _synergyLocalTable = Managers.TableManager.Instance.GetTableClass<JobLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, JobData> GetJobDatas_Dic(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            return _synergyLocalTable.GetJobDatas_Dic(v2Enum_ARR_SynergyType);
        }
        //------------------------------------------------------------------------------------
        public static JobData GetJobData(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, ObscuredInt jobTier)
        {
            return _synergyLocalTable.GetJobData(v2Enum_ARR_SynergyType, jobTier);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, JobData> GetAllJobData()
        {
            return _synergyLocalTable.GetAllJobData();
        }
        //------------------------------------------------------------------------------------
        public static JobData GetJobData(ObscuredInt index)
        {
            return _synergyLocalTable.GetJobData(index);
        }
        //------------------------------------------------------------------------------------
        public static JobLevelUpCostData GetJobLevelUpCostData(ObscuredInt jobTier)
        {
            return _synergyLocalTable.GetJobLevelUpCostData(jobTier);
        }
        //------------------------------------------------------------------------------------
        public static JobLevelUpStatData GetJobLevelUpStatData(ObscuredInt jobTier)
        {
            return _synergyLocalTable.GetJobLevelUpStatData(jobTier);
        }
        //------------------------------------------------------------------------------------
        public static JobTierUpgradeConditionData GetJobTierUpgradeConditionData(ObscuredInt jobTier)
        {
            return _synergyLocalTable.GetJobTierUpgradeConditionData(jobTier);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, JobTierUpgradeConditionData> GetAllJobTierUpgradeConditionData()
        {
            return _synergyLocalTable.GetAllJobTierUpgradeConditionData();
        }
        //------------------------------------------------------------------------------------
    }
}
