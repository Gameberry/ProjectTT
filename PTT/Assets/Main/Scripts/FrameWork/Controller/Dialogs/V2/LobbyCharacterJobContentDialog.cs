using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gpm.Ui;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class LobbyCharacterJobContentDialog : IDialog
    {
        [Header("------------GuideJob------------")]
        [SerializeField]
        private Transform _guideJob3BG;

        [SerializeField]
        private List<Transform> _noJobHideUI = new List<Transform>();

        [SerializeField]
        private Transform _noJobLockUI;

        [Header("-------JobDo-------")]
        [SerializeField]
        private Button _changeJob;

        [SerializeField]
        private TMP_Text _upGradeJobCondition_Level;

        [SerializeField]
        private TMP_Text _upGradeJobCondition_Other;

        [SerializeField]
        private Button _doUpGradeJob;

        [SerializeField]
        private LobbyCharacterJobChangeDialog _lobbyCharacterJobChangeDialog;

        [SerializeField]
        private LobbyCharacterJobUpgradeCompleteDialog _lobbyCharacterJobUpgradeCompleteDialog;


        [Header("-------JobDetail-------")]
        [SerializeField]
        private Button _jobTierArrow_Prev;

        [SerializeField]
        private Button _jobTierArrow_Next;

        [SerializeField]
        private UIJobEffectElement _uIJobEffectElement;


        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup;

        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup_NextLevel;


        [SerializeField]
        private Transform _synergyEffect_Enhance_Max;


        [SerializeField]
        private Button _synergyEffect_Enhance;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceText;

        [SerializeField]
        private Image _synergyEffect_EnhanceCountIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceCountText;


        [SerializeField]
        private Transform _synergyEffect_Enhance2Group;

        [SerializeField]
        private Image _synergyEffect_Enhance2CountIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_Enhance2CountText;



        [SerializeField]
        private Color _buttonTextEnableColor;

        [SerializeField]
        private Material _buttonTextEnableMaterial;

        [SerializeField]
        private Color _buttonTextDisaEnableColor;

        [SerializeField]
        private Material _buttonTextDisableMaterial;

        private int _currentTier = 0;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_jobTierArrow_Prev != null)
                _jobTierArrow_Prev.onClick.AddListener(OnClick_PrevTier);

            if (_jobTierArrow_Next != null)
                _jobTierArrow_Next.onClick.AddListener(OnClick_NextTier);

            if (_synergyEffect_Enhance != null)
                _synergyEffect_Enhance.onClick.AddListener(OnClick_EnhanceSynergy);


            if (_changeJob != null)
                _changeJob.onClick.AddListener(OnClick_ChangeJob);

            if (_doUpGradeJob != null)
                _doUpGradeJob.onClick.AddListener(OnClick_UpGradeJob);

            if (_lobbyCharacterJobChangeDialog != null)
            { 
                _lobbyCharacterJobChangeDialog.Load_Element();
                _lobbyCharacterJobChangeDialog.SetLobbyCharacterJobContentDialog(this);
            }

            if (_lobbyCharacterJobUpgradeCompleteDialog != null)
                _lobbyCharacterJobUpgradeCompleteDialog.Load_Element();

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.JobEnhance.Enum32ToInt(), RefreshJobEnhance);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            SetJobDoBtn(JobContainer.JobTier);
            SetLevelUpStat(JobContainer.JobTier);

            if (Managers.GuideInteractorManager.Instance.PlayJobChangeTutorial == true)
            {
                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/job2"));
                
                if (_guideJob3BG != null)
                    _guideJob3BG.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshJobEnhance(double amount)
        {
            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Job) == false)
            {
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyCharacterJob_LevelUp);
                return;
            }

            if (Managers.JobManager.Instance.ReadySynergyEnhance(JobContainer.JobTier) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyCharacterJob_LevelUp);
            }
            else
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyCharacterJob_LevelUp);
        }
        //------------------------------------------------------------------------------------
        private void SetJobDoBtn(int tier)
        {
            if (_doUpGradeJob != null)
                _doUpGradeJob.gameObject.SetActive(true);

            if (tier <= 0)
            {
                for (int i = 0; i < _noJobHideUI.Count; ++i)
                {
                    if (_noJobHideUI[i] != null)
                        _noJobHideUI[i].gameObject.SetActive(false);
                }

                if (_noJobLockUI != null)
                    _noJobLockUI.gameObject.SetActive(true);

                if (_changeJob != null)
                    _changeJob.gameObject.SetActive(false);

                if (_upGradeJobCondition_Level != null)
                    _upGradeJobCondition_Level.gameObject.SetActive(false);

                if (_upGradeJobCondition_Other != null)
                {
                    _upGradeJobCondition_Other.gameObject.SetActive(true);
                    _upGradeJobCondition_Other.SetText(Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(V2Enum_ContentType.Job));
                    _upGradeJobCondition_Other.color = Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Job) == false ? Color.red : Color.white;
                }

                if (_doUpGradeJob != null)
                    _doUpGradeJob.interactable = true;
            }
            else
            {
                if (_noJobLockUI != null)
                    _noJobLockUI.gameObject.SetActive(false);

                for (int i = 0; i < _noJobHideUI.Count; ++i)
                {
                    if (_noJobHideUI[i] != null)
                        _noJobHideUI[i].gameObject.SetActive(true);
                }

                if (_changeJob != null)
                    _changeJob.gameObject.SetActive(true);

                if (_doUpGradeJob != null)
                    _doUpGradeJob.interactable = Managers.JobManager.Instance.CanUpGradeJob();

                Enum_SynergyType Enum_SynergyType = JobContainer.JobType.GetDecrypted().IntToEnum32<Enum_SynergyType>();

                JobData jobData = Managers.JobManager.Instance.GetJobData(Enum_SynergyType, tier);

                JobTierUpgradeConditionData jobTierUpgradeConditionData = Managers.JobManager.Instance.GetJobTierUpgradeConditionData(tier);

                if (jobTierUpgradeConditionData == null)
                {
                    if (_upGradeJobCondition_Level != null)
                    {
                        _upGradeJobCondition_Level.gameObject.SetActive(false);
                    }

                    if (_upGradeJobCondition_Other != null)
                    {
                        _upGradeJobCondition_Other.gameObject.SetActive(false);
                    }

                    if (_doUpGradeJob != null)
                        _doUpGradeJob.gameObject.SetActive(false);

                    return;
                }

                int level = 0;

                SkillInfo skillInfo = Managers.JobManager.Instance.GetSynergyEffectSkillInfo(tier);
                if (skillInfo != null)
                    level = skillInfo.Level;

                if (_upGradeJobCondition_Level != null)
                {
                    _upGradeJobCondition_Level.gameObject.SetActive(true);
                    _upGradeJobCondition_Level.SetText(string.Format("{0} : {1}", Managers.LocalStringManager.Instance.GetLocalString(jobData.NameKey), jobTierUpgradeConditionData.RequiredLevel));
                    _upGradeJobCondition_Level.color = jobTierUpgradeConditionData.RequiredLevel > level ? Color.red : Color.white;
                }

                if (_upGradeJobCondition_Other != null)
                {
                    _upGradeJobCondition_Other.gameObject.SetActive(true);
                    _upGradeJobCondition_Other.SetText(Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(jobTierUpgradeConditionData.OpenConditionType, jobTierUpgradeConditionData.OpenConditionValue));
                    _upGradeJobCondition_Other.color = Managers.ContentOpenConditionManager.Instance.IsOpen(jobTierUpgradeConditionData.OpenConditionType, jobTierUpgradeConditionData.OpenConditionValue) == false ? Color.red : Color.white;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetLevelUpStat(int tier, bool refreshCharacter = true)
        {
            _currentTier = tier;

            for (int i = 0; i < _noJobHideUI.Count; ++i)
            {
                if (_noJobHideUI[i] != null)
                    _noJobHideUI[i].gameObject.SetActive(tier > 0);
            }

            if (_noJobLockUI != null)
                _noJobLockUI.gameObject.SetActive(tier <= 0);

            if (refreshCharacter == true)
            {
                if (_uIJobEffectElement != null)
                    _uIJobEffectElement.SetJobEffectElement(JobContainer.JobType.GetDecrypted().IntToEnum32<Enum_SynergyType>(), tier);
            }
            

            if (tier <= 0)
                return;

            if (_jobTierArrow_Prev != null)
                _jobTierArrow_Prev.gameObject.SetActive(
                    Managers.JobManager.Instance.GetJobLevelUpCostData(tier - 1) != null);

            if (_jobTierArrow_Next != null)
                _jobTierArrow_Next.gameObject.SetActive(
                    Managers.JobManager.Instance.GetJobLevelUpCostData(tier + 1) != null);

            if (_synergyEffect_Enhance2Group != null)
                _synergyEffect_Enhance2Group.gameObject.SetActive(false);

            SkillInfo skillInfo = Managers.JobManager.Instance.GetSynergyEffectSkillInfo(tier);
            if (skillInfo == null)
            {
                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetJobStat(tier, Define.PlayerJobDefaultLevel);

                if (uIARRRSkillDescGroup_NextLevel != null)
                    uIARRRSkillDescGroup_NextLevel.SetJobStat(tier, Define.PlayerJobDefaultLevel + 1);

                if (_synergyEffect_Enhance != null)
                    _synergyEffect_Enhance.gameObject.SetActive(false);

                if (_synergyEffect_Enhance_Max != null)
                    _synergyEffect_Enhance_Max.gameObject.SetActive(false);
            }
            else
            {
                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetJobStat(tier, skillInfo.Level);

                if (uIARRRSkillDescGroup_NextLevel != null)
                {
                    if (Managers.JobManager.Instance.IsMaxLevelSynergy(tier) == false)
                    {
                        uIARRRSkillDescGroup_NextLevel.gameObject.SetActive(true);
                        uIARRRSkillDescGroup_NextLevel.SetJobStat(tier, skillInfo.Level + 1);
                    }
                    else
                        uIARRRSkillDescGroup_NextLevel.gameObject.SetActive(false);
                }


                {
                    bool isMax = Managers.JobManager.Instance.IsMaxLevelSynergy(tier);

                    if (_synergyEffect_Enhance != null)
                        _synergyEffect_Enhance.gameObject.SetActive(isMax == false);

                    if (_synergyEffect_Enhance_Max != null)
                        _synergyEffect_Enhance_Max.gameObject.SetActive(isMax == true);

                    if (isMax == true)
                    {
                        if (_synergyEffect_EnhanceText != null)
                        {
                            _synergyEffect_EnhanceText.color = _buttonTextEnableColor;
                            _synergyEffect_EnhanceText.fontMaterial = _buttonTextEnableMaterial;
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EnhanceText, "Max");
                        }

                        if (_synergyEffect_EnhanceCountText != null)
                        {
                            _synergyEffect_EnhanceCountText.color = Color.white;
                            _synergyEffect_EnhanceCountText.SetText("-");
                        }

                        if (_synergyEffect_EnhanceCountIcon != null)
                            _synergyEffect_EnhanceCountIcon.gameObject.SetActive(false);
                    }
                    else
                    {
                        int costIndex = Managers.JobManager.Instance.GetSynergyEnhanceCostGoodsIndex1(tier);

                        int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        int needCount = Managers.JobManager.Instance.GetSynergyEnhance_NeedCount1(tier);

                        bool readyEnhance = currentCount >= needCount;

                        Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);

                        if (_synergyEffect_EnhanceText != null)
                        {
                            _synergyEffect_EnhanceText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                            _synergyEffect_EnhanceText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EnhanceText, "ui/reinforce");
                        }

                        if (_synergyEffect_EnhanceCountText != null)
                        {
                            _synergyEffect_EnhanceCountText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                            _synergyEffect_EnhanceCountText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                            _synergyEffect_EnhanceCountText.SetText("{0}", needCount);
                        }

                        if (_synergyEffect_EnhanceCountIcon != null)
                        {
                            _synergyEffect_EnhanceCountIcon.sprite = pointsprite;
                            _synergyEffect_EnhanceCountIcon.gameObject.SetActive(true);
                        }




                        costIndex = Managers.JobManager.Instance.GetSynergyEnhanceCostGoodsIndex2(tier);

                        currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        needCount = Managers.JobManager.Instance.GetSynergyEnhance_NeedCount2(tier);

                        readyEnhance = currentCount >= needCount;

                        pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);


                        if (costIndex != -1)
                        {
                            if (_synergyEffect_Enhance2Group != null)
                                _synergyEffect_Enhance2Group.gameObject.SetActive(true);

                            if (_synergyEffect_Enhance2CountText != null)
                            {
                                _synergyEffect_Enhance2CountText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                                _synergyEffect_Enhance2CountText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                                _synergyEffect_Enhance2CountText.SetText("{0}", needCount);
                            }

                            if (_synergyEffect_Enhance2CountIcon != null)
                            {
                                _synergyEffect_Enhance2CountIcon.sprite = pointsprite;
                                _synergyEffect_Enhance2CountIcon.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }

            Managers.JobManager.Instance.RefreshJobUpgradeReddot();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PrevTier()
        {
            SetLevelUpStat(_currentTier - 1);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_NextTier()
        {
            SetLevelUpStat(_currentTier + 1);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnhanceSynergy()
        {
            if (Managers.JobManager.Instance.DoLevelUpJob() == true)
            {
                SetJobDoBtn(JobContainer.JobTier);
                SetLevelUpStat(_currentTier, false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ChangeJob()
        {
            if (_lobbyCharacterJobChangeDialog != null)
            {
                Enum_SynergyType Enum_SynergyType = Enum_SynergyType.Max;

                if (JobContainer.JobTier <= 0)
                    return;

                _lobbyCharacterJobChangeDialog.InitJobEffectElement(Enum_SynergyType, JobContainer.JobTier, false);
                _lobbyCharacterJobChangeDialog.ElementEnter();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_UpGradeJob()
        {
            if (_lobbyCharacterJobChangeDialog != null)
            {
                Enum_SynergyType Enum_SynergyType = Enum_SynergyType.Max;

                if (JobContainer.JobTier > 0)
                    Enum_SynergyType = JobContainer.JobType.GetDecrypted().IntToEnum32<Enum_SynergyType>();

                if (Managers.GuideInteractorManager.Instance.PlayJobChangeTutorial == true)
                {
                    if (_guideJob3BG != null)
                        _guideJob3BG.gameObject.SetActive(false);
                }

                _lobbyCharacterJobChangeDialog.InitJobEffectElement(Enum_SynergyType, JobContainer.JobTier + 1, true);
                _lobbyCharacterJobChangeDialog.ElementEnter();
            }
        }
        //------------------------------------------------------------------------------------
        public void CompleteChangeJobState(bool showCompleteDialog)
        {
            if (showCompleteDialog == true)
            {
                if (_lobbyCharacterJobUpgradeCompleteDialog != null)
                    _lobbyCharacterJobUpgradeCompleteDialog.ElementEnter();
            }

            SetJobDoBtn(JobContainer.JobTier);
            SetLevelUpStat(JobContainer.JobTier);
        }
        //------------------------------------------------------------------------------------
    }
}