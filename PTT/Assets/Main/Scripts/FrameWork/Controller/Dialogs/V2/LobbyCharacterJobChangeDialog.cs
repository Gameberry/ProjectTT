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
    public class LobbyCharacterJobChangeDialog : IDialog
    {
        [Header("------------GuideOpen------------")]
        // 잠긴 아이콘 눌러보기
        [SerializeField]
        private Transform _guideJob4BG;

        [SerializeField]
        private Button _guideJob4Btn;

        // 해금 시점 확인하기
        [SerializeField]
        private Transform _guideJob5BG;

        [SerializeField]
        private Button _guideJob5Btn;

        [Header("------------Filter------------")]
        [SerializeField]
        private Transform _filterGroup;

        [SerializeField]
        private List<UICallBackBtnElement> m_elementFilter;

        [Header("-------JobDetail-------")]
        [SerializeField]
        private UIJobEffectElement _uIJobEffectElement;

        [SerializeField]
        private Button _uIUpGradeJob;

        private ObscuredInt _jobTier = 0;
        private V2Enum_ARR_SynergyType _v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;

        private bool _isUpGrade = false;

        private LobbyCharacterJobContentDialog _lobbyCharacterJobContentDialog;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_guideJob4Btn != null)
                _guideJob4Btn.onClick.AddListener(() =>
                {
                    if (_guideJob4BG != null)
                        _guideJob4BG.gameObject.SetActive(false);

                    if (_guideJob4Btn != null)
                        _guideJob4Btn.gameObject.SetActive(false);

                    if (_guideJob5BG != null)
                        _guideJob5BG.gameObject.SetActive(true);

                    if (_guideJob5Btn != null)
                        _guideJob5Btn.gameObject.SetActive(true);

                    Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/job4"));
                    Managers.GuideInteractorManager.Instance.SetGuideStep(5);
                });

            if (_guideJob5Btn != null)
                _guideJob5Btn.onClick.AddListener(() =>
                {
                    if (_guideJob5BG != null)
                        _guideJob5BG.gameObject.SetActive(false);

                    if (_guideJob5Btn != null)
                        _guideJob5Btn.gameObject.SetActive(false);

                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                });

            if (_uIUpGradeJob != null)
                _uIUpGradeJob.onClick.AddListener(OnClick_UpGradeJob);

            for (int i = 0; i < m_elementFilter.Count; ++i)
            {
                m_elementFilter[i].SetCallBack(OnClick_ElementFilter);
                m_elementFilter[i].SetEnable(false);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshJobUI(_v2Enum_ARR_SynergyType, _jobTier);

            if (Managers.GuideInteractorManager.Instance.PlayJobChangeTutorial == true)
            {
                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/job3"));

                Managers.GuideInteractorManager.Instance.SetGuideStep(4);

                if (_guideJob4BG != null)
                    _guideJob4BG.gameObject.SetActive(true);

                if (_guideJob4Btn != null)
                    _guideJob4Btn.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {

        }
        //------------------------------------------------------------------------------------
        public void SetLobbyCharacterJobContentDialog(LobbyCharacterJobContentDialog lobbyCharacterJobContentDialog)
        {
            _lobbyCharacterJobContentDialog = lobbyCharacterJobContentDialog;
        }
        //------------------------------------------------------------------------------------
        private void EnableFilter(bool enable)
        {
            if (_filterGroup != null)
                _filterGroup.gameObject.SetActive(enable);

            if (enable == true)
            {
                int synergytype = _v2Enum_ARR_SynergyType.Enum32ToInt();

                for (int i = 0; i < m_elementFilter.Count; ++i)
                {
                    JobData jobData = Managers.JobManager.Instance.GetJobData(m_elementFilter[i].m_myID.IntToEnum32<V2Enum_ARR_SynergyType>(), _jobTier);

                    m_elementFilter[i].SetEnable(m_elementFilter[i].m_myID == synergytype);

                    if (jobData != null)
                        m_elementFilter[i].SetText(Managers.LocalStringManager.Instance.GetLocalString(jobData.NameKey));
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void InitJobEffectElement(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, ObscuredInt tier, bool isupgrade)
        {
            _isUpGrade = isupgrade;

            _v2Enum_ARR_SynergyType = v2Enum_ARR_SynergyType;
            _jobTier = tier;

        }
        //------------------------------------------------------------------------------------
        public void RefreshJobUI(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, ObscuredInt tier)
        {
            _v2Enum_ARR_SynergyType = v2Enum_ARR_SynergyType;
            if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.Max)
                _v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;


            _jobTier = tier;

            if (_uIJobEffectElement != null)
                _uIJobEffectElement.SetJobEffectElement(_v2Enum_ARR_SynergyType, tier);


            if (_isUpGrade == false)
                EnableFilter(true);
            else
            {
                if (_jobTier == 1)
                    EnableFilter(true);
                else
                    EnableFilter(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ElementFilter(int element)
        {
            RefreshJobUI(element.IntToEnum32<V2Enum_ARR_SynergyType>(), _jobTier);

            EnableFilter(true);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_UpGradeJob()
        {
            if (Managers.GuideInteractorManager.Instance.PlayJobChangeTutorial == true)
            {
                return;
            }


            if (_isUpGrade == false)
            {
                if (Managers.JobManager.Instance.ChangeJobType(_v2Enum_ARR_SynergyType) == true)
                {
                    _lobbyCharacterJobContentDialog.CompleteChangeJobState(true);
                    ElementExit();
                }
            }
            else
            {
                if (Managers.JobManager.Instance.DoUpGradeJob(_v2Enum_ARR_SynergyType) == true)
                {
                    _lobbyCharacterJobContentDialog.CompleteChangeJobState(true);
                    ElementExit();
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}