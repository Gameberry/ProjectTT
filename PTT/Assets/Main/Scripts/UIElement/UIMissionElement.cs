using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIMissionElement : MonoBehaviour
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_rewardIcon;

        [SerializeField]
        private TMP_Text m_missionTitle;

        [SerializeField]
        private TMP_Text m_missionProcess;

        [SerializeField]
        private Image m_missionProcessGauge;

        [SerializeField]
        private Color m_notReadyProcessMission_Btn;

        [SerializeField]
        private Color m_readyProcessMission_Btn;


        [SerializeField]
        private Transform m_redDot;

        [Header("-------Button-------")]
        [SerializeField]
        private Button m_rewardMission_Btn;

        [SerializeField]
        private Color m_disableRewardMission_Btn;

        [SerializeField]
        private Color m_enableRewardMission_Btn;


        [Header("-------Light-------")]
        [SerializeField]
        private Image m_recvLightImage;

        [SerializeField]
        private Image m_shortCutImage;

        [Header("-------Text-------")]
        [SerializeField]
        private TMP_Text m_recvText;

        [SerializeField]
        private Color m_disableRecvText;

        [SerializeField]
        private Color m_enableRecvText;

        [SerializeField]
        private TMP_Text m_alReadyText;

        [SerializeField]
        private TMP_Text m_shortCutText;

        [SerializeField]
        private Image m_alReadyMission;

        private QuestData m_currentMissionData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_rewardMission_Btn != null)
                m_rewardMission_Btn.onClick.AddListener(OnClick_RecvMissionReward);

            Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        private void RefreshLocalize()
        {
            if (m_currentMissionData == null)
                return;

            if (m_missionTitle != null)
                m_missionTitle.text = Managers.QuestManager.Instance.GetMissionNameLocalString(m_currentMissionData);
        }
        //------------------------------------------------------------------------------------
        public void SetMissionElement(QuestData missionData)
        {
            if (missionData == null)
                return;

            m_currentMissionData = missionData;

            if (m_rewardIcon != null)
                m_rewardIcon.SetRewardElement(
                    missionData.ClearRewardType,
                    missionData.ClearRewardIndex.GetDecrypted(),
                    Managers.GoodsManager.Instance.GetGoodsSprite(missionData.ClearRewardType.Enum32ToInt(), missionData.ClearRewardIndex.GetDecrypted()),
                    Managers.GoodsManager.Instance.GetGoodsGrade(missionData.ClearRewardType.Enum32ToInt(), missionData.ClearRewardIndex.GetDecrypted()),
                    Managers.QuestManager.Instance.GetDisplayRewardAmount(missionData));

            if (m_missionTitle != null)
                m_missionTitle.text = Managers.QuestManager.Instance.GetMissionNameLocalString(m_currentMissionData);

            int actionCount = Managers.QuestManager.Instance.GetActionCount(missionData);

            if (m_missionProcess != null)
            {
                m_missionProcess.SetText("{0}/{1}", actionCount, missionData.QuestGoalValue.GetDecrypted());
                m_missionProcess.color = actionCount >= missionData.QuestGoalValue.GetDecrypted() ? m_readyProcessMission_Btn : m_notReadyProcessMission_Btn;
            }

            if (m_missionProcessGauge != null)
            {
                float ratio = (float)actionCount / (float)missionData.QuestGoalValue.GetDecrypted();
                m_missionProcessGauge.fillAmount = ratio;
            }

            bool isAlReadyReward = Managers.QuestManager.Instance.IsAlReadyRecvRewardMission(m_currentMissionData);

            if (isAlReadyReward == true)
            {
                if (m_alReadyMission != null)
                    m_alReadyMission.gameObject.SetActive(true);

                if (m_alReadyText != null)
                    m_alReadyText.gameObject.SetActive(true);

                if (m_recvText != null)
                    m_recvText.gameObject.SetActive(false);

                if (m_rewardMission_Btn != null)
                {
                    m_rewardMission_Btn.interactable = false;
                    m_rewardMission_Btn.image.color = m_disableRewardMission_Btn;
                }

                if (m_recvLightImage != null)
                    m_recvLightImage.gameObject.SetActive(false);

                if (m_shortCutImage != null)
                    m_shortCutImage.gameObject.SetActive(false);

                if (m_rewardMission_Btn != null)
                    m_rewardMission_Btn.gameObject.SetActive(false);
                

                //transform.SetAsLastSibling();
            }
            else
            {
                if (m_rewardMission_Btn != null)
                    m_rewardMission_Btn.gameObject.SetActive(true);


                bool isReadyRecv = Managers.QuestManager.Instance.IsReadyRecvRewardMission(m_currentMissionData);

                bool totalReady = isReadyRecv == true;

                if (m_alReadyMission != null)
                    m_alReadyMission.gameObject.SetActive(false);

                if (m_alReadyText != null)
                    m_alReadyText.gameObject.SetActive(false);

                if (m_recvText != null)
                {
                    m_recvText.gameObject.SetActive(true);
                    m_recvText.color = totalReady == true ? m_enableRecvText : m_disableRecvText;
                }

                if (m_rewardMission_Btn != null)
                {
                    m_rewardMission_Btn.interactable = totalReady;
                    m_rewardMission_Btn.image.color = totalReady == true ? m_enableRewardMission_Btn : m_disableRewardMission_Btn;
                }

                if (isReadyRecv == true)
                {
                    if (m_recvLightImage != null)
                        m_recvLightImage.gameObject.SetActive(true);

                    if (m_shortCutImage != null)
                        m_shortCutImage.gameObject.SetActive(false);

                    if (m_shortCutText != null)
                        m_shortCutText.gameObject.SetActive(false);

                    if (m_redDot != null)
                        m_redDot.gameObject.SetActive(true);
                }
                else
                {
                    if (m_recvLightImage != null)
                        m_recvLightImage.gameObject.SetActive(false);

                    bool conShortCut = Managers.QuestManager.Instance.CanShortcut(m_currentMissionData);

                    if (m_shortCutImage != null)
                        m_shortCutImage.gameObject.SetActive(conShortCut);

                    if (m_recvText != null)
                    {
                        m_recvText.gameObject.SetActive(conShortCut == false);
                    }

                    if (m_rewardMission_Btn != null)
                    {
                        m_rewardMission_Btn.interactable = conShortCut;
                    }

                    if (m_shortCutText != null)
                        m_shortCutText.gameObject.SetActive(conShortCut == true);

                    if (m_redDot != null)
                        m_redDot.gameObject.SetActive(false);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RecvMissionReward()
        {
            if (m_currentMissionData == null)
                return;

            if (Managers.QuestManager.Instance.IsReadyRecvRewardMission(m_currentMissionData) == true)
                Managers.QuestManager.Instance.DoRecvMission(m_currentMissionData);
            else if (Managers.QuestManager.Instance.CanShortcut(m_currentMissionData) == true)
            {
                Managers.QuestManager.Instance.DoShortcutRecvMission(m_currentMissionData);

                IDialog.RequestDialogExit<LobbyQuestContentDialog>();
            }

        }
        //------------------------------------------------------------------------------------
        public QuestData GetQuestData()
        {
            return m_currentMissionData;
        }
        //------------------------------------------------------------------------------------
        public UIGuideInteractor SetDailyMissionGuideBtn()
        {
            if (m_rewardMission_Btn != null)
            {
                UIGuideInteractor uIGuideInteractor = m_rewardMission_Btn.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = V2Enum_EventType.DailyMissionRewardGet;
                uIGuideInteractor.MyStepID = 4;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.ConnectInteractor();

                return uIGuideInteractor;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}