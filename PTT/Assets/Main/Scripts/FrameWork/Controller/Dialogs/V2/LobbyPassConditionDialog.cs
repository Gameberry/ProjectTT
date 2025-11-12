using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class LobbyPassConditionDialog : IDialog
    {
        [SerializeField]
        private Image m_passBanner;

        [SerializeField]
        private TMP_Text m_passName;

        [SerializeField]
        private TMP_Text m_passDesc;

        [SerializeField]
        private TMP_Text m_passCurrentStepText;

        [SerializeField]
        private Transform m_rewardRoot;

        [SerializeField]
        private Button m_prevLevelPass;

        [SerializeField]
        private TMP_Text m_prevLevelPass_Text;

        [SerializeField]
        private Image m_prevLevelRedDot;

        [SerializeField]
        private Button m_nextLevelPass;

        [SerializeField]
        private TMP_Text m_nextLevelPass_Text;

        [SerializeField]
        private Image m_nextLevelRedDot;

        [SerializeField]
        private TMP_Text _currentPassStep;

        [Header("-------BuyBtn-------")]
        [SerializeField]
        private Button m_buyBtn;

        [SerializeField]
        private Color m_disableBuy_Btn;

        [SerializeField]
        private Color m_enableBuy_Btn;

        [SerializeField]
        private TMP_Text m_buyPriceText;

        [SerializeField]
        private Color m_disableBuyText;

        [SerializeField]
        private Color m_enableBuyText;

        [SerializeField]
        private Image m_buyLightImg;

        [SerializeField]
        protected Transform _tagGroup;

        [SerializeField]
        private TMP_Text _tagText;

        [Header("-------PassCurrent-------")]
        [SerializeField]
        private TMP_Text m_currentCombatPower;

        [Header("-------DisPlayReward-------")]
        [SerializeField]
        private List<UIGlobalGoodsRewardIconElement> _disPlayReward = new List<UIGlobalGoodsRewardIconElement>();

        [Header("-------PassReward-------")]
        [SerializeField]
        private InfiniteScroll m_passElementInfinityScroll;

        //[SerializeField]
        //private ScrollRect m_elementScrollRect;

        //[SerializeField]
        //private RectTransform m_uIPassRewardElementRoot;

        //[SerializeField]
        //private UIPassConditionElement m_uIPassRewardElement;

        //[SerializeField]
        //private ContentSizeFitter m_rootContentSizeFitter;

        private List<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new List<UIGlobalGoodsRewardIconElement>();

        //private List<UIPassConditionElement> m_passConditionList = new List<UIPassConditionElement>();

        private PassData m_focusData = null;
        private V2Enum_PassType m_currentPassType;

        private UIGuideInteractor m_passGuideElementInteractor;
        private UIPassConditionElement m_lastCanRecvReward = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_prevLevelPass != null)
                m_prevLevelPass.onClick.AddListener(OnClick_PrevBtn);

            if (m_nextLevelPass != null)
                m_nextLevelPass.onClick.AddListener(OnClick_NextBtn);

            if (m_buyBtn != null)
                m_buyBtn.onClick.AddListener(OnClick_BuyBtn);

            Message.AddListener<GameBerry.Event.RefreshPassUIMsg>(RefreshPassUI);
            Message.AddListener<GameBerry.Event.SetPassRewardDialogMsg>(SetPassRewardDialog);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshPassUIMsg>(RefreshPassUI);
            Message.RemoveListener<GameBerry.Event.SetPassRewardDialogMsg>(SetPassRewardDialog);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            //ScrollMoveToFocusElement();

            //if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.PassFreeRewardClaim
            //    && m_currentPassType == V2Enum_PassType.Stage)
            //{
            //    if (m_passConditionList != null && m_passConditionList.Count > 0)
            //    {
            //        UIPassConditionElement uIPassConditionElement = m_passConditionList[m_passConditionList.Count - 1];
            //        if (uIPassConditionElement == null)
            //            return;

            //        m_passGuideElementInteractor = uIPassConditionElement.SetPassGuideBtn();
            //    }

            //    Managers.GuideInteractorManager.Instance.SetGuideStep(m_passGuideElementInteractor);
            //}
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (m_focusData != null)
            {
                Managers.RedDotManager.Instance.HideRedDot(Managers.PassManager.Instance.ConvertPassTypeToContentDetail(m_focusData.PassType));
            }

            m_focusData = null;

            UIManager.DialogExit<UI.LobbyPassDialog>();
        }
        //------------------------------------------------------------------------------------
        private void RefreshPassUI(GameBerry.Event.RefreshPassUIMsg msg)
        {
            if (m_focusData == null)
                return;

            RefreshPassData(m_focusData);
            ScrollMoveToFocusElement();
        }
        //------------------------------------------------------------------------------------
        private void SetPassRewardDialog(GameBerry.Event.SetPassRewardDialogMsg msg)
        {
            //if (m_currentPassType != msg.v2Enum_PassType)
            {
                m_currentPassType = msg.v2Enum_PassType;
                SetFocusData(msg.v2Enum_PassType);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetFocusData(V2Enum_PassType v2Enum_PassType)
        {
            m_focusData = Managers.PassManager.Instance.GetPassFocusData(v2Enum_PassType);

            if (m_passBanner != null)
                m_passBanner.sprite = Managers.PassManager.Instance.GetPassBanner(v2Enum_PassType);

            if (m_focusData == null)
                return;

            if (m_passCurrentStepText != null)
            {
                if (v2Enum_PassType == V2Enum_PassType.MonsterKill)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_passCurrentStepText, "common/ui/enemyKillCount");
                else
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_passCurrentStepText, "pass/currentStep");
            }

            RefreshPassData(m_focusData);
            SetFocusData(m_focusData);
            ScrollMoveToFocusElement();
        }
        //------------------------------------------------------------------------------------
        private void SetRewardIcon()
        {
            if (m_focusData == null)
                return;

            for (int i = 0; i < m_focusData.ShopRewardData.Count; ++i)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = null;
                if (m_uIGlobalGoodsRewardIconElements.Count <= i)
                {
                    uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon_NoneParticle();
                    m_uIGlobalGoodsRewardIconElements.Add(uIGlobalGoodsRewardIconElement);
                }
                else
                {
                    uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[i];
                    uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                }

                RewardData rewardData = m_focusData.ShopRewardData[i];

                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                uIGlobalGoodsRewardIconElement.transform.SetParent(m_rewardRoot);
                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
            }

            for (int i = m_focusData.ShopRewardData.Count; i < m_uIGlobalGoodsRewardIconElements.Count; ++i)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[i];
                uIGlobalGoodsRewardIconElement.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetFocusData(PassData passData)
        {
            if (m_passName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_passName, passData.TitleLocalStringKey);

            if (m_focusData != null)
            {
                if (m_passDesc != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_passDesc, m_focusData.SubTitleLocalStringKey);
            }

            if (_currentPassStep != null)
                _currentPassStep.SetText("step {0}", passData.PassStep.GetDecrypted());

            PassData prevPassData = Managers.PassManager.Instance.GetPrevLevelPass(passData);
            if (prevPassData == null)
            {
                if (m_prevLevelPass != null)
                    m_prevLevelPass.gameObject.SetActive(false);
            }
            else
            {
                if (m_prevLevelPass != null)
                    m_prevLevelPass.gameObject.SetActive(true);

                if (m_prevLevelPass_Text != null)
                    m_prevLevelPass_Text.SetText("Lv.{0}", prevPassData.PassStep.GetDecrypted());

                if (m_prevLevelRedDot != null)
                    m_prevLevelRedDot.gameObject.SetActive(Managers.PassManager.Instance.IsReadyReward(prevPassData));
            }


            PassData nextPassData = Managers.PassManager.Instance.GetNextLevelPass(passData);
            if (nextPassData == null)
            {
                if (m_nextLevelPass != null)
                    m_nextLevelPass.gameObject.SetActive(false);
            }
            else
            {
                if (m_nextLevelPass != null)
                    m_nextLevelPass.gameObject.SetActive(true);

                if (m_nextLevelPass_Text != null)
                    m_nextLevelPass_Text.SetText("Lv.{0}", nextPassData.PassStep.GetDecrypted());

                if (m_nextLevelRedDot != null)
                    m_nextLevelRedDot.gameObject.SetActive(Managers.PassManager.Instance.IsReadyReward(nextPassData));
            }

            double currentCount = Managers.PassManager.Instance.PassCurrentCount(passData.PassType);

            bool isFocusPass = passData.IsMinClearParam <= currentCount && passData.IsMaxClearParam >= currentCount;

            if (m_buyBtn != null)
            {
                if (passData.PassStep.GetDecrypted() == 1)
                    m_buyBtn.gameObject.SetActive(true);
                else
                    m_buyBtn.gameObject.SetActive(passData.IsMinClearParam <= currentCount);
            }

            for (int i = 0; i < passData.DisplayRewardData.Count; ++i)
            {
                if (_disPlayReward.Count <= i)
                    break;

                RewardData rewardData = passData.DisplayRewardData[i];

                V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(rewardData.Index);

                rewardData.V2Enum_Goods = v2Enum_Goods;

                _disPlayReward[i].SetRewardElement(rewardData);
                _disPlayReward[i].gameObject.SetActive(true);
            }

            for (int i = passData.DisplayRewardData.Count; i < _disPlayReward.Count; ++i)
            {
                _disPlayReward[i].gameObject.SetActive(true);
            }


            SetRewardIcon();
        }
        //------------------------------------------------------------------------------------
        public void RefreshPassData(PassData passData)
        {
            bool isBuy = Managers.PassManager.Instance.IsBuy(passData);

            if (m_buyBtn != null)
            {
                m_buyBtn.interactable = isBuy == false;
                m_buyBtn.image.color = isBuy == false ? m_enableBuy_Btn : m_disableBuy_Btn;
            }

            if (_tagGroup != null)
                _tagGroup.gameObject.SetActive(isBuy == false);

            if (isBuy == false)
            {
                if (_tagText != null)
                {
                    Managers.LocalStringManager.Instance.SetLocalizeText(_tagText, passData.TagString);
                }
            }

            if (m_buyPriceText != null)
            {
                m_buyPriceText.text = Managers.PassManager.Instance.GetPriceText(passData);
                m_buyPriceText.color = isBuy == false ? m_enableBuyText : m_disableBuyText;
            }

            if (m_buyLightImg != null)
                m_buyLightImg.gameObject.SetActive(isBuy == false);

            int currentCount = Managers.PassManager.Instance.PassCurrentCount(m_focusData.PassType);

            if (m_currentCombatPower != null)
            {
                m_currentCombatPower.gameObject.SetActive(true);
                m_currentCombatPower.text = Managers.PassManager.Instance.GetConvertPassCountText(m_focusData.PassType, Managers.PassManager.Instance.PassCurrentCount(m_focusData.PassType));
            }

            if (m_passElementInfinityScroll != null)
            {
                m_passElementInfinityScroll.Clear();

                for (int i = 0; i < passData.passConditionRewardDatas.Count; ++i)
                {
                    m_passElementInfinityScroll.InsertData(passData.passConditionRewardDatas[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ScrollMoveToFocusElement()
        {
            if (m_focusData == null)
                return;

            double currentCount = Managers.PassManager.Instance.PassCurrentCount(m_focusData.PassType);

            bool isFocusPass = m_focusData.IsMinClearParam <= currentCount && m_focusData.IsMaxClearParam >= currentCount;


            if (m_passElementInfinityScroll != null)
            {
                //m_passElementInfinityScroll.UpdateAllData();

                if (isFocusPass == false)
                {
                    if (m_focusData.IsMaxClearParam <= currentCount)
                    {
                        //normalPos.y = 0.0f;
                        m_passElementInfinityScroll.MoveToLastData();
                    }
                    else
                    {
                        //normalPos.y = 1.0f;
                        m_passElementInfinityScroll.MoveToFirstData();
                    }
                }
                else
                {
                    //normalPos.y = 1.0f - (float)((currentCount - m_focusData.IsMinClearParam) / (m_focusData.IsMaxClearParam - m_focusData.IsMinClearParam));

                    PassConditionRewardData focusrewarddata = null;

                    for (int i = 0; i < m_focusData.passConditionRewardDatas.Count; ++i)
                    {
                        if (m_focusData.passConditionRewardDatas[i].ConditionClearParam.GetDecrypted() <= currentCount)
                            focusrewarddata = m_focusData.passConditionRewardDatas[i];
                        else
                            break;
                    }

                    if (focusrewarddata == null)
                    {
                        m_passElementInfinityScroll.MoveToFirstData();
                    }
                    else
                    {
                        m_passElementInfinityScroll.MoveToFirstData();
                        m_passElementInfinityScroll.MoveTo(focusrewarddata, InfiniteScroll.MoveToType.MOVE_TO_CENTER);
                    }
                }

                
            }
        }
        //------------------------------------------------------------------------------------
        public void OnClick_BuyBtn()
        {
            if (m_focusData == null)
                return;

            Managers.PassManager.Instance.Buy(m_focusData);
        }
        //------------------------------------------------------------------------------------
        public void OnClick_PrevBtn()
        {
            if (m_focusData == null)
                return;

            PassData passData = Managers.PassManager.Instance.GetPrevLevelPass(m_focusData);
            if (passData == null)
                return;

            m_focusData = passData;

            RefreshPassData(passData);
            SetFocusData(passData);

            ScrollMoveToFocusElement();
        }
        //------------------------------------------------------------------------------------
        public void OnClick_NextBtn()
        {
            if (m_focusData == null)
                return;

            PassData passData = Managers.PassManager.Instance.GetNextLevelPass(m_focusData);
            if (passData == null)
                return;

            m_focusData = passData;

            RefreshPassData(passData);
            SetFocusData(passData);

            ScrollMoveToFocusElement();
        }
        //------------------------------------------------------------------------------------
    }
}