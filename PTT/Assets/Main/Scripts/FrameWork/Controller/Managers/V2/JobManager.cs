using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class JobManager : MonoSingleton<JobManager>
    {
        private GameBerry.Event.RefreshCharacterInfo_StatMsg _refreshCharacterInfo_StatMsg = new Event.RefreshCharacterInfo_StatMsg();
        private GameBerry.Event.RefreshCharacterSkin_StatMsg _refreshCharacterSkin_StatMsg = new Event.RefreshCharacterSkin_StatMsg();

        // totalLevel
        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrSynergyTotalStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        public Dictionary<V2Enum_Stat, ObscuredDouble> ArrrSynergyTotalStatValues { get { return _arrrSynergyTotalStatValues; } }

        private string weaponSkinPath = "Weapon/{0}";
        private string weaponSkinDefault = "000";

        private List<string> m_changeInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerJobInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            JobOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitJobContent()
        {
            foreach (var pair in GetAllJobData())
            {
                JobData jobData = pair.Value;

                jobData.SynergySkillData = SkillManager.Instance.GetMainSkillData(jobData.MainSkill);
            }

            foreach (var pair in JobContainer.SynergyInfo)
            {
                SkillInfo skillInfo = pair.Value;

                IncreaseStat(skillInfo.Id);
            }
        }
        //------------------------------------------------------------------------------------
#if DEV_DEFINE
        [ContextMenu("InitJob")]
        public void InitJob()
        {
            JobContainer.SynergyInfo.Clear();
            JobContainer.JobTier = 0;
            JobContainer.JobType = -1;

            Managers.GearManager.Instance.RefreshJobUpGrade();

            _arrrSynergyTotalStatValues.Clear();

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();
        }
#endif
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, JobData> GetJobDatas_Dic(Enum_SynergyType Enum_SynergyType)
        {
            return JobOperator.GetJobDatas_Dic(Enum_SynergyType);
        }
        //------------------------------------------------------------------------------------
        public JobData GetJobData(Enum_SynergyType Enum_SynergyType, ObscuredInt jobTier)
        {
            return JobOperator.GetJobData(Enum_SynergyType, jobTier);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, JobData> GetAllJobData()
        {
            return JobOperator.GetAllJobData();
        }
        //------------------------------------------------------------------------------------
        public string GetWeaponSkin()
        {
            if (JobContainer.JobTier <= 0)
                return string.Format(weaponSkinPath, weaponSkinDefault);

            Enum_SynergyType Enum_SynergyType = JobContainer.JobType.GetDecrypted().IntToEnum32<Enum_SynergyType>();

            return GetWeaponSkinName(Enum_SynergyType, JobContainer.JobTier);
        }
        //------------------------------------------------------------------------------------
        public string GetWeaponSkinName(Enum_SynergyType Enum_SynergyType, ObscuredInt jobTier)
        {
            JobData jobData = GetJobData(Enum_SynergyType, jobTier);
            if (jobData == null)
                return string.Format(weaponSkinPath, weaponSkinDefault);

            return string.Format(weaponSkinPath, jobData.WeaponSkinResource);
            //return string.Format(weaponSkinPath, weaponSkinDefault);
        }
        //------------------------------------------------------------------------------------
        public JobData GetJobData(ObscuredInt index)
        {
            return JobOperator.GetJobData(index);
        }
        //------------------------------------------------------------------------------------
        public JobLevelUpCostData GetJobLevelUpCostData(ObscuredInt jobTier)
        {
            return JobOperator.GetJobLevelUpCostData(jobTier);
        }
        //------------------------------------------------------------------------------------
        public JobLevelUpStatData GetJobLevelUpStatData(ObscuredInt jobTier)
        {
            return JobOperator.GetJobLevelUpStatData(jobTier);
        }
        //------------------------------------------------------------------------------------
        public JobTierUpgradeConditionData GetJobTierUpgradeConditionData(ObscuredInt jobTier)
        {
            return JobOperator.GetJobTierUpgradeConditionData(jobTier);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, JobTierUpgradeConditionData> GetAllJobTierUpgradeConditionData()
        {
            return JobOperator.GetAllJobTierUpgradeConditionData();
        }
        //------------------------------------------------------------------------------------
        public SkillInfo GetSynergyEffectSkillInfo(ObscuredInt jobTier)
        {
            if (JobContainer.SynergyInfo.ContainsKey(jobTier) == true)
                return JobContainer.SynergyInfo[jobTier];

            return null;
        }
        //------------------------------------------------------------------------------------
        public SkillInfo AddNewSkillInfo(ObscuredInt jobTier)
        {
            if (JobContainer.SynergyInfo.ContainsKey(jobTier) == true)
                return JobContainer.SynergyInfo[jobTier];

            SkillInfo skillInfo = new SkillInfo();
            skillInfo.Id = jobTier;
            skillInfo.Level = Define.PlayerJobDefaultLevel;
            skillInfo.Count = 0;

            JobContainer.SynergyInfo.Add(skillInfo.Id, skillInfo);

            return skillInfo;
        }
        //------------------------------------------------------------------------------------
        public double GetStatValue(ObscuredInt jobTier)
        {
            SkillInfo skillInfo = GetSynergyEffectSkillInfo(jobTier);
            if (skillInfo == null)
                return 0;

            int level = skillInfo.Level;

            return GetStatValue(jobTier, level);
        }
        //------------------------------------------------------------------------------------
        public double GetStatValue(ObscuredInt jobTier, int level)
        {
            JobLevelUpStatData jobLevelUpStatData = GetJobLevelUpStatData(jobTier);

            return jobLevelUpStatData.LevelUpStatParam1 + (level * jobLevelUpStatData.LevelUpStatParam2);
        }
        //------------------------------------------------------------------------------------
        public void IncreaseStat(ObscuredInt jobTier)
        {
            JobLevelUpStatData jobLevelUpStatData = GetJobLevelUpStatData(jobTier);

            double statvalue = GetStatValue(jobTier);

            if (_arrrSynergyTotalStatValues.ContainsKey(jobLevelUpStatData.LevelUpStatType) == false)
                _arrrSynergyTotalStatValues.Add(jobLevelUpStatData.LevelUpStatType, 0);

            _arrrSynergyTotalStatValues[jobLevelUpStatData.LevelUpStatType] += statvalue;
        }
        //------------------------------------------------------------------------------------
        public void DecreaseStat(ObscuredInt jobTier)
        {
                JobLevelUpStatData jobLevelUpStatData = GetJobLevelUpStatData(jobTier);

                double statvalue = GetStatValue(jobTier);

                _arrrSynergyTotalStatValues[jobLevelUpStatData.LevelUpStatType] -= statvalue;

                if (_arrrSynergyTotalStatValues[jobLevelUpStatData.LevelUpStatType] < 0)
                    _arrrSynergyTotalStatValues[jobLevelUpStatData.LevelUpStatType] = 0;
        }
        //------------------------------------------------------------------------------------
        #region UpGrade
        //------------------------------------------------------------------------------------
        public bool CanUpGradeJob(ObscuredInt JobTier)
        {
            JobTierUpgradeConditionData jobTierUpgradeConditionData = GetJobTierUpgradeConditionData(JobTier);
            if (jobTierUpgradeConditionData == null)
                return false;

            if (ContentOpenConditionManager.Instance.IsOpen(jobTierUpgradeConditionData.OpenConditionType, jobTierUpgradeConditionData.OpenConditionValue) == false)
                return false;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(JobTier);
            if (skillInfo == null)
                return false;

            if (jobTierUpgradeConditionData.RequiredLevel > skillInfo.Level)
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool CanUpGradeJob()
        {
            if (JobContainer.JobTier == 0)
                return true;

            return CanUpGradeJob(JobContainer.JobTier);
        }
        //------------------------------------------------------------------------------------
        public bool ChangeJobType(Enum_SynergyType Enum_SynergyType)
        {
            if (JobContainer.JobTier == 0)
                return false;

            JobContainer.JobType = Enum_SynergyType.Enum32ToInt();

            Managers.GearManager.Instance.RefreshJobUpGrade();

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate);

            Message.Send(_refreshCharacterSkin_StatMsg);

            ThirdPartyLog.Instance.SendLog_log_job_change(JobContainer.JobType, JobContainer.JobTier);

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoUpGradeJob(Enum_SynergyType Enum_SynergyType)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Job) == false)
            { 
                ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Job);
                return false;
            }

            if (JobContainer.JobTier == 0)
            {
                JobContainer.JobType = Enum_SynergyType.Enum32ToInt();
                JobContainer.JobTier = 1;

                SkillInfo skillInfo = AddNewSkillInfo(JobContainer.JobTier);
                skillInfo.Level = Define.PlayerJobDefaultLevel;
            }
            else
            {
                if (CanUpGradeJob(JobContainer.JobTier) == false)
                    return false;

                JobContainer.JobTier += 1;

                SkillInfo skillInfo = AddNewSkillInfo(JobContainer.JobTier);
                skillInfo.Level = Define.PlayerJobDefaultLevel;
            }

            IncreaseStat(JobContainer.JobTier);

            Managers.GearManager.Instance.RefreshJobUpGrade();

            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate);

            Message.Send(_refreshCharacterSkin_StatMsg);

            ThirdPartyLog.Instance.SendLog_log_job_enforce(JobContainer.JobType, JobContainer.JobTier);

            return true;
        }
        //------------------------------------------------------------------------------------
        public void RefreshJobUpgradeReddot()
        {
            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Job) == false)
            {
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyCharacterJob_Upgrade);
                return;
            }

            if (CanUpGradeJob() == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyCharacterJob_Upgrade);
            }
            else
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyCharacterJob_Upgrade);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region LevelUp
        //------------------------------------------------------------------------------------
        public bool IsMaxLevelSynergy(ObscuredInt JobTier)
        {
            JobLevelUpCostData synergyLevelUpCostData = GetJobLevelUpCostData(JobTier);
            if (synergyLevelUpCostData == null)
                return true;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(JobTier);

            int level = skillInfo.Level;

            return level >= synergyLevelUpCostData.MaximumLevel;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount1(ObscuredInt JobTier)
        {
            JobLevelUpCostData synergyLevelUpCostData = GetJobLevelUpCostData(JobTier);

            if (synergyLevelUpCostData == null)
                return 99999;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(JobTier);

            int level = skillInfo.Level;

            return synergyLevelUpCostData.LevelUpCostGoodsParam12 + (level * synergyLevelUpCostData.LevelUpCostGoodsParam13);
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex1(ObscuredInt JobTier)
        {
            JobLevelUpCostData synergyLevelUpCostData = GetJobLevelUpCostData(JobTier);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.LevelUpCostGoodsParam11;
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhance_NeedCount2(ObscuredInt JobTier)
        {
            JobLevelUpCostData synergyLevelUpCostData = GetJobLevelUpCostData(JobTier);

            if (synergyLevelUpCostData == null)
                return 99999;

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(JobTier);

            int level = skillInfo.Level;

            return synergyLevelUpCostData.LevelUpCostGoodsParam22 + (level * synergyLevelUpCostData.LevelUpCostGoodsParam23);
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyEnhanceCostGoodsIndex2(ObscuredInt JobTier)
        {
            JobLevelUpCostData synergyLevelUpCostData = GetJobLevelUpCostData(JobTier);

            if (synergyLevelUpCostData == null)
                return -1;

            return synergyLevelUpCostData.LevelUpCostGoodsParam21;
        }
        //------------------------------------------------------------------------------------
        public bool ReadySynergyEnhance(ObscuredInt JobTier)
        {
            int costIndex = GetSynergyEnhanceCostGoodsIndex1(JobTier);
            int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

            if (currentCount < GetSynergyEnhance_NeedCount1(JobTier))
                return false;

            costIndex = GetSynergyEnhanceCostGoodsIndex2(JobTier);
            currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

            if (currentCount < GetSynergyEnhance_NeedCount2(JobTier))
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoLevelUpJob()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (IsMaxLevelSynergy(JobContainer.JobTier) == true)
                return false;

            if (ReadySynergyEnhance(JobContainer.JobTier) == false)
            {
                return false;
            }

            List<int> used_type = new List<int>();
            List<double> former_quan = new List<double>();
            List<double> used_quan = new List<double>();
            List<double> keep_quan = new List<double>();

            int costIndex = GetSynergyEnhanceCostGoodsIndex1(JobContainer.JobTier);
            used_type.Add(costIndex);

            int useCost = GetSynergyEnhance_NeedCount1(JobContainer.JobTier);

            former_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));
            used_quan.Add(useCost);
            Managers.GoodsManager.Instance.UseGoodsAmount(costIndex, useCost);
            keep_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));

            costIndex = GetSynergyEnhanceCostGoodsIndex2(JobContainer.JobTier);
            used_type.Add(costIndex);

            useCost = GetSynergyEnhance_NeedCount2(JobContainer.JobTier);

            former_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));
            used_quan.Add(useCost);
            Managers.GoodsManager.Instance.UseGoodsAmount(costIndex, useCost);
            keep_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(costIndex));

            DecreaseStat(JobContainer.JobTier);

            SkillInfo skillInfo = GetSynergyEffectSkillInfo(JobContainer.JobTier);
            skillInfo.Level += 1;

            IncreaseStat(JobContainer.JobTier);


            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_job_levelup(JobContainer.JobType, skillInfo.Level,
                 used_type, former_quan, used_quan, keep_quan);

            //PassManager.Instance.CheckPassType(V2Enum_PassType.SkillLevel);
            //Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.SynergySkillLevelStack);


            BattleSceneManager.Instance.RefreshMyARRRStat();
            Message.Send(_refreshCharacterInfo_StatMsg);
            ARRRStatManager.Instance.RefreshBattlePower();

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate);

            return true;
        }
        //------------------------------------------------------------------------------------
        public Enum_SynergyType GetCurrentJobType()
        {
            if (JobContainer.JobTier <= 0)
                return Enum_SynergyType.Max;

            return JobContainer.JobType.GetDecrypted().IntToEnum32<Enum_SynergyType>();
        }
        //------------------------------------------------------------------------------------
        public double GetJobAddBuff_Gear(Enum_SynergyType Enum_SynergyType)
        {
            if (GetCurrentJobType() != Enum_SynergyType)
                return 0;

            double buffvalue = 0;

            foreach (var equippair in JobContainer.SynergyInfo)
            {
                JobData jobData = GetJobData(Enum_SynergyType, equippair.Value.Id);

                if (jobData != null)
                {
                    buffvalue += jobData.GearEffect;
                }
            }

            return buffvalue;
        }
        //------------------------------------------------------------------------------------
        public int GetJobAddBuff_Rune(Enum_SynergyType Enum_SynergyType)
        {
            if (GetCurrentJobType() != Enum_SynergyType)
                return 0;

            int buffvalue = 0;

            foreach (var equippair in JobContainer.SynergyInfo)
            {
                JobData jobData = GetJobData(Enum_SynergyType, equippair.Value.Id);

                if (jobData != null)
                {
                    buffvalue += jobData.RuneEffect;
                }
            }

            return buffvalue;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region InGameJob
        //------------------------------------------------------------------------------------
        public void SetInGameJobData()
        {
            Enum_SynergyType Enum_SynergyType = GetCurrentJobType();

            foreach (var equippair in JobContainer.SynergyInfo)
            {
                JobData jobData = GetJobData(Enum_SynergyType, equippair.Value.Id);

                if (jobData != null)
                {
                    MainSkillData mainSkillData = jobData.SynergySkillData;
                    if (mainSkillData != null)
                        Managers.BattleSceneManager.Instance.AddGambleSkill(mainSkillData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
    }
}