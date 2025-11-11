using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class InGameGuideQuestDialog : IDialog
    {
        [SerializeField]
        private TMP_Text m_questStep;

        [SerializeField]
        private TMP_Text m_questTitle;

        [SerializeField]
        private TMP_Text m_questProgress;

        [SerializeField]
        private Image m_questRewardIcon;

        [SerializeField]
        private Image m_questRewardIcon_Skill;

        [SerializeField]
        private TMP_Text m_questRewardAmount;

        [SerializeField]
        private Color m_questIdleColor = Color.white;

        [SerializeField]
        private Color m_questClearColor = Color.white;

        [SerializeField]
        private Button m_shortCutBtn;

        [SerializeField]
        private Button m_questRecvReward;

        [SerializeField]
        private Image m_questClearEffect;

        [SerializeField]
        private UIGuideInteractor m_CompleteUIGuideInteractor;

        private GuideQuestData m_guideQuestData;

        private bool prevCompleteState = false;
        private bool showInteractor = false;
        private float showGuideInteractorTimer = 0.0f;

        [SerializeField]
        private float showGuideInteractorTurm = 3.0f;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_shortCutBtn != null)
                m_shortCutBtn.onClick.AddListener(OnClick_ShortCut);

            if (m_questRecvReward != null)
                m_questRecvReward.onClick.AddListener(OnClick_RecvReward);

            Message.AddListener<GameBerry.Event.RefreshGuideQuestMsg>(RefreshGuideQuest);

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLocalize;

            Managers.UnityUpdateManager.Instance.LateUpdateFunc += LateUpdated;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshGuideQuestMsg>(RefreshGuideQuest);

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString -= RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        private void RefreshLocalize()
        {
            if (m_guideQuestData == null)
                return;

            if (m_questStep != null)
                m_questStep.SetText(string.Format(Managers.LocalStringManager.Instance.GetLocalString("common/ui/questTitle"), m_guideQuestData.QuestOrder.GetDecrypted()));

            if (m_questTitle != null)
                m_questTitle.text = Managers.GuideQuestManager.Instance.GetQuestNameLocalString(m_guideQuestData);
        }
        //------------------------------------------------------------------------------------
        private void RefreshGuideQuest(GameBerry.Event.RefreshGuideQuestMsg msg)
        {
            m_guideQuestData = msg.guideQuestData;

            if (m_guideQuestData == null)
                return;

            bool isClear = Managers.GuideQuestManager.Instance.IsClearGuideQuest(m_guideQuestData);

            if (m_questStep != null)
                m_questStep.SetText(string.Format(Managers.LocalStringManager.Instance.GetLocalString("common/ui/questTitle"), m_guideQuestData.QuestOrder.GetDecrypted()));

            if (m_questTitle != null)
            { 
                m_questTitle.text = Managers.GuideQuestManager.Instance.GetQuestNameLocalString(m_guideQuestData);
                m_questTitle.color = isClear == false ? m_questIdleColor : m_questClearColor;
            }

            if (m_questProgress != null)
            {
                bool isvisible = Managers.GuideQuestManager.Instance.IsVisibleProcessUI(m_guideQuestData);

                m_questProgress.gameObject.SetActive(isvisible);

                if (isvisible == true)
                {
                    m_questProgress.text = string.Format("{0}/{1}", Managers.GuideQuestManager.Instance.GetCurrentProcess(), m_guideQuestData.QuestParam.GetDecrypted());
                    m_questProgress.color = isClear == false ? m_questIdleColor : m_questClearColor;
                }
            }

            Sprite rewardIcon = Managers.GoodsManager.Instance.GetGoodsSprite(m_guideQuestData.ClearRewardType.Enum32ToInt(), m_guideQuestData.ClearRewardParam1.GetDecrypted());

            if (m_guideQuestData.ClearRewardType == V2Enum_Goods.CharacterSkill
                || m_guideQuestData.ClearRewardType == V2Enum_Goods.Ally)
            {
                if (m_questRewardIcon != null)
                    m_questRewardIcon.gameObject.SetActive(false);

                if (m_questRewardIcon_Skill != null)
                {
                    m_questRewardIcon_Skill.gameObject.SetActive(true);
                    m_questRewardIcon_Skill.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(m_guideQuestData.ClearRewardType.Enum32ToInt(), m_guideQuestData.ClearRewardParam1.GetDecrypted());
                }
            }
            else
            {
                if (m_questRewardIcon != null)
                { 
                    m_questRewardIcon.gameObject.SetActive(true);
                    m_questRewardIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(m_guideQuestData.ClearRewardType.Enum32ToInt(), m_guideQuestData.ClearRewardParam1.GetDecrypted());
                }

                if (m_questRewardIcon_Skill != null)
                    m_questRewardIcon_Skill.gameObject.SetActive(false);
            }

            if (m_questRewardAmount != null)
            { 
                m_questRewardAmount.text = Util.GetAlphabetNumber(m_guideQuestData.ClearRewardParam2.GetDecrypted());
                m_questRewardAmount.color = isClear == false ? m_questIdleColor : m_questClearColor;
            }

            if (m_shortCutBtn != null)
                m_shortCutBtn.gameObject.SetActive(!isClear);

            if (m_questClearEffect != null)
                m_questClearEffect.gameObject.SetActive(isClear);

            if (m_questRecvReward != null)
                m_questRecvReward.gameObject.SetActive(isClear);

            if (prevCompleteState == false && isClear == true && m_CompleteUIGuideInteractor != null)
            {
                if (m_guideQuestData.QuestOrder == 1)
                {
                    Managers.GuideInteractorManager.Instance.ShowCompleteFocus(m_CompleteUIGuideInteractor);
                    showInteractor = true;
                }
                else
                {
                    showGuideInteractorTimer = Time.time + showGuideInteractorTurm;
                }
            }

            if (isClear == false)
                showInteractor = false;

            prevCompleteState = isClear;
        }
        //------------------------------------------------------------------------------------
        private void LateUpdated()
        {
            if (prevCompleteState == true && showInteractor == false)
            {
                if (Time.time > showGuideInteractorTimer)
                {
                    Managers.GuideInteractorManager.Instance.ShowCompleteFocus(m_CompleteUIGuideInteractor);
                    showInteractor = true;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShortCut()
        {
            if (m_guideQuestData == null)
                return;

            bool isClear = Managers.GuideQuestManager.Instance.IsClearGuideQuest(m_guideQuestData);
            if (isClear == false)
            {
                Managers.GuideQuestManager.Instance.GoShortCut(m_guideQuestData);
                return;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RecvReward()
        {
            if (m_guideQuestData == null)
                return;

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (m_questRewardIcon != null)
                Managers.GoodsDropDirectionManager.Instance.ShowDropIn(m_guideQuestData.ClearRewardType, m_guideQuestData.ClearRewardParam1.GetDecrypted(), m_questRewardIcon.transform.position, m_guideQuestData.ClearRewardParam2.GetDecrypted().ToInt());

            Managers.GuideQuestManager.Instance.CompleteGuideQuest(m_guideQuestData);
        }
        //------------------------------------------------------------------------------------
    }
}