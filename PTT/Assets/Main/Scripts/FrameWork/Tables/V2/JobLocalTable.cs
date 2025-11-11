using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;

namespace GameBerry
{
    public class JobData
    {
        public ObscuredInt Index;

        public V2Enum_ARR_SynergyType SynergyType;
        public ObscuredInt JobTier;

        public ObscuredInt MainSkill;

        public MainSkillData SynergySkillData;

        public ObscuredDouble GearEffect;
        public ObscuredInt RuneEffect;

        public string WeaponSkinResource;

        public string NameKey;
        public string JobGearEffectKey;
        public string JobRuneEffectKey;
    }

    public class JobLevelUpCostData
    {
        public ObscuredInt Index;

        public ObscuredInt JobTier;

        public ObscuredInt MaximumLevel;

        public ObscuredInt LevelUpCostGoodsParam11;
        public ObscuredInt LevelUpCostGoodsParam12;
        public ObscuredInt LevelUpCostGoodsParam13;

        public ObscuredInt LevelUpCostGoodsParam21;
        public ObscuredInt LevelUpCostGoodsParam22;
        public ObscuredInt LevelUpCostGoodsParam23;
    }

    public class JobLevelUpStatData
    {
        public ObscuredInt Index;

        public ObscuredInt JobTier;

        public V2Enum_Stat LevelUpStatType;
        public ObscuredDouble LevelUpStatParam1;
        public ObscuredDouble LevelUpStatParam2;
    }

    public class JobTierUpgradeConditionData
    {
        public ObscuredInt Index;

        public ObscuredInt JobTier;

        public ObscuredInt RequiredLevel;

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionValue;
    }

    public class JobLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_ARR_SynergyType, Dictionary<ObscuredInt, JobData>> _jobDatas_Dic = new Dictionary<V2Enum_ARR_SynergyType, Dictionary<ObscuredInt, JobData>>();
        private Dictionary<ObscuredInt, JobData> _jobDatas_Index_Dic = new Dictionary<ObscuredInt, JobData>();

        private Dictionary<ObscuredInt, JobLevelUpCostData> _jobLevelUpCostDatas_Dic = new Dictionary<ObscuredInt, JobLevelUpCostData>();

        private Dictionary<ObscuredInt, JobLevelUpStatData> _jobLevelUpStatDatas_Dic = new Dictionary<ObscuredInt, JobLevelUpStatData>();

        private Dictionary<ObscuredInt, JobTierUpgradeConditionData> _jobTierUpgradeConditionDatas_Dic = new Dictionary<ObscuredInt, JobTierUpgradeConditionData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Job", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                JobData jobData = new JobData();

                jobData.Index = rows[i]["Index"].ToString().ToInt();

                jobData.SynergyType = rows[i]["SynergyType"].ToString().ToInt().IntToEnum32<V2Enum_ARR_SynergyType>();
                jobData.JobTier = rows[i]["JobTier"].ToString().ToInt();

                jobData.MainSkill = rows[i]["MainSkill"].ToString().ToInt();
                jobData.GearEffect = rows[i]["GearEffect"].ToString().ToDouble();
                jobData.RuneEffect = rows[i]["RuneEffect"].ToString().ToInt();

                jobData.WeaponSkinResource = rows[i]["WeaponSkinResource"].ToString();

                jobData.NameKey = string.Format("job/{0}/{1}", jobData.SynergyType.Enum32ToInt(), jobData.JobTier);
                jobData.JobGearEffectKey = string.Format("jobgear/{0}", jobData.SynergyType.Enum32ToInt());
                jobData.JobRuneEffectKey = string.Format("jobrune/{0}", jobData.SynergyType.Enum32ToInt());

                if (_jobDatas_Dic.ContainsKey(jobData.SynergyType) == false)
                    _jobDatas_Dic.Add(jobData.SynergyType, new Dictionary<ObscuredInt, JobData>());

                if (_jobDatas_Dic[jobData.SynergyType].ContainsKey(jobData.JobTier) == false)
                    _jobDatas_Dic[jobData.SynergyType].Add(jobData.JobTier, jobData);

                _jobDatas_Index_Dic.Add(jobData.Index, jobData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("JobLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                JobLevelUpCostData gearLevelUpCostData = new JobLevelUpCostData();

                gearLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                gearLevelUpCostData.JobTier = rows[i]["JobTier"].ToString().ToInt();

                gearLevelUpCostData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();

                gearLevelUpCostData.LevelUpCostGoodsParam11 = rows[i]["LevelUpCostGoodsParam11"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam12 = rows[i]["LevelUpCostGoodsParam12"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam13 = rows[i]["LevelUpCostGoodsParam13"].ToString().ToInt();

                gearLevelUpCostData.LevelUpCostGoodsParam21 = rows[i]["LevelUpCostGoodsParam21"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam22 = rows[i]["LevelUpCostGoodsParam22"].ToString().ToInt();
                gearLevelUpCostData.LevelUpCostGoodsParam23 = rows[i]["LevelUpCostGoodsParam23"].ToString().ToInt();

                if (_jobLevelUpCostDatas_Dic.ContainsKey(gearLevelUpCostData.JobTier) == false)
                    _jobLevelUpCostDatas_Dic.Add(gearLevelUpCostData.JobTier, gearLevelUpCostData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("JobLevelUpCost", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                JobLevelUpCostData jobLevelUpCostData = new JobLevelUpCostData();

                jobLevelUpCostData.Index = rows[i]["Index"].ToString().ToInt();

                jobLevelUpCostData.JobTier = rows[i]["JobTier"].ToString().ToInt();

                jobLevelUpCostData.MaximumLevel = rows[i]["MaximumLevel"].ToString().ToInt();

                jobLevelUpCostData.LevelUpCostGoodsParam11 = rows[i]["LevelUpCostGoodsParam11"].ToString().ToInt();
                jobLevelUpCostData.LevelUpCostGoodsParam12 = rows[i]["LevelUpCostGoodsParam12"].ToString().ToInt();
                jobLevelUpCostData.LevelUpCostGoodsParam13 = rows[i]["LevelUpCostGoodsParam13"].ToString().ToInt();

                jobLevelUpCostData.LevelUpCostGoodsParam21 = rows[i]["LevelUpCostGoodsParam21"].ToString().ToInt();
                jobLevelUpCostData.LevelUpCostGoodsParam22 = rows[i]["LevelUpCostGoodsParam22"].ToString().ToInt();
                jobLevelUpCostData.LevelUpCostGoodsParam23 = rows[i]["LevelUpCostGoodsParam23"].ToString().ToInt();

                if (_jobLevelUpCostDatas_Dic.ContainsKey(jobLevelUpCostData.JobTier) == false)
                    _jobLevelUpCostDatas_Dic.Add(jobLevelUpCostData.JobTier, jobLevelUpCostData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("JobLevelUpStat", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                JobLevelUpStatData jobLevelUpStatData = new JobLevelUpStatData();

                jobLevelUpStatData.Index = rows[i]["Index"].ToString().ToInt();

                jobLevelUpStatData.JobTier = rows[i]["JobTier"].ToString().ToInt();

                jobLevelUpStatData.LevelUpStatType = rows[i]["LevelUpStatType"].ToString().ToInt().IntToEnum32<V2Enum_Stat>();
                jobLevelUpStatData.LevelUpStatParam1 = rows[i]["LevelUpStatParam1"].ToString().ToInt();
                jobLevelUpStatData.LevelUpStatParam2 = rows[i]["LevelUpStatParam2"].ToString().ToInt();

                if (_jobLevelUpStatDatas_Dic.ContainsKey(jobLevelUpStatData.JobTier) == false)
                    _jobLevelUpStatDatas_Dic.Add(jobLevelUpStatData.JobTier, jobLevelUpStatData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("JobTierUpgradeCondition", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                JobTierUpgradeConditionData jobLevelUpStatData = new JobTierUpgradeConditionData();

                jobLevelUpStatData.Index = rows[i]["Index"].ToString().ToInt();

                jobLevelUpStatData.JobTier = rows[i]["JobTier"].ToString().ToInt();

                jobLevelUpStatData.RequiredLevel = rows[i]["RequiredLevel"].ToString().ToInt();

                jobLevelUpStatData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                jobLevelUpStatData.OpenConditionValue = rows[i]["OpenConditionValue"].ToString().ToInt();

                if (_jobTierUpgradeConditionDatas_Dic.ContainsKey(jobLevelUpStatData.JobTier) == false)
                    _jobTierUpgradeConditionDatas_Dic.Add(jobLevelUpStatData.JobTier, jobLevelUpStatData);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, JobData> GetJobDatas_Dic(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            if (_jobDatas_Dic.ContainsKey(v2Enum_ARR_SynergyType) == false)
                return null;

            return _jobDatas_Dic[v2Enum_ARR_SynergyType];
        }
        //------------------------------------------------------------------------------------
        public JobData GetJobData(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, ObscuredInt jobTier)
        {
            if (_jobDatas_Dic.ContainsKey(v2Enum_ARR_SynergyType) == false)
                return null;

            if (_jobDatas_Dic[v2Enum_ARR_SynergyType].ContainsKey(jobTier) == false)
                return null;

            return _jobDatas_Dic[v2Enum_ARR_SynergyType][jobTier];
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, JobData> GetAllJobData()
        {
            return _jobDatas_Index_Dic;
        }
        //------------------------------------------------------------------------------------
        public JobData GetJobData(ObscuredInt index)
        {
            if (_jobDatas_Index_Dic.ContainsKey(index) == false)
                return null;

            return _jobDatas_Index_Dic[index];
        }
        //------------------------------------------------------------------------------------
        public JobLevelUpCostData GetJobLevelUpCostData(ObscuredInt jobTier)
        {
            if (_jobLevelUpCostDatas_Dic.ContainsKey(jobTier) == true)
                return _jobLevelUpCostDatas_Dic[jobTier];

            return null;
        }
        //------------------------------------------------------------------------------------
        public JobLevelUpStatData GetJobLevelUpStatData(ObscuredInt jobTier)
        {
            if (_jobLevelUpStatDatas_Dic.ContainsKey(jobTier) == true)
                return _jobLevelUpStatDatas_Dic[jobTier];

            return null;
        }
        //------------------------------------------------------------------------------------
        public JobTierUpgradeConditionData GetJobTierUpgradeConditionData(ObscuredInt jobTier)
        {
            if (_jobTierUpgradeConditionDatas_Dic.ContainsKey(jobTier) == true)
                return _jobTierUpgradeConditionDatas_Dic[jobTier];

            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, JobTierUpgradeConditionData> GetAllJobTierUpgradeConditionData()
        {
            return _jobTierUpgradeConditionDatas_Dic;
        }
        //------------------------------------------------------------------------------------
    }
}