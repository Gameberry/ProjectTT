using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class DungeonSelectDialog : IDialog
    {
        [SerializeField]
        private List<Image> m_dungeonRewardIcon;

        [SerializeField]
        private TMP_Text m_dungeonName;

        [SerializeField]
        private Image m_dungeonBanner;

        [SerializeField]
        private Transform m_defficultyGroup;

        [SerializeField]
        private Button m_prevDefficulty;

        [SerializeField]
        private Button m_nextDefficulty;


        [SerializeField]
        private TMP_Text m_dungeonLevel;

        [SerializeField]
        private TMP_Text m_defficultyBossName;

        [SerializeField]
        private Transform m_dungeonhightScoreGroup;

        [SerializeField]
        private TMP_Text m_dungeonhightScore;


        [SerializeField]
        private Transform m_dungeonhightStepGroup;

        [SerializeField]
        private TMP_Text m_dungeonhightStep;


        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement;

        [SerializeField]
        private Transform m_uIHellRewardRoot;

        [SerializeField]
        private List<UIGlobalGoodsRewardIconElement> m_uIHellDungeonGoodsRewardIconElement;

        [SerializeField]
        private TMP_Text m_dungeonhightScoreRewardAmount;

        [SerializeField]
        private TMP_Text m_dungeonhightPhase;

        [SerializeField]
        private Image m_dungeonTicketImage;

        [SerializeField]
        private Sprite m_dungeonAdIcon;

        [SerializeField]
        private TMP_Text m_dungeonTickAmount;


        [SerializeField]
        private Button m_sweepBtn;

        [SerializeField]
        private Button m_playBtn;

        [SerializeField]
        private Color m_btn_Disable;

        [SerializeField]
        private Color m_btn_Enable;


        [SerializeField]
        private TMP_Text m_sweepText;

        [SerializeField]
        private TMP_Text m_playText;

        [SerializeField]
        private Color m_text_Disable;

        [SerializeField]
        private Color m_text_Enable;

        [SerializeField]
        private Toggle m_autoEnter;

        [SerializeField]
        private UIDungeonSweepPopupElement m_uIDungeonSweepPopupElement;

        [SerializeField]
        private UIGuideInteractor m_diamondDungeonClearGuideActiveTabInteractor;

        [SerializeField]
        private UIGuideInteractor m_masteryDungeonClearGuideActiveTabInteractor;

        [SerializeField]
        private UIGuideInteractor m_goldDungeonClearGuideActiveTabInteractor;

        [SerializeField]
        private UIGuideInteractor m_soulStoneDungeonClearGuideActiveTabInteractor;

        [SerializeField]
        private UIGuideInteractor m_runeDungeonClearGuideActiveTabInteractor;

        private DungeonData m_dungeonData;
        private DungeonModeBase m_dungeonModeBase;

        private bool m_isAdMode = false;

        private RewardData m_rewardData = new RewardData();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_prevDefficulty != null)
                m_prevDefficulty.onClick.AddListener(OnClick_PrevDefficulty);

            if (m_nextDefficulty != null)
                m_nextDefficulty.onClick.AddListener(OnClick_NextDefficulty);

            if (m_sweepBtn != null)
                m_sweepBtn.onClick.AddListener(OnClick_SweepBtn);

            if (m_playBtn != null)
                m_playBtn.onClick.AddListener(OnClick_PlayBtn);

            if (m_uIDungeonSweepPopupElement != null)
            {
                m_uIDungeonSweepPopupElement.Load_Element();
                m_uIDungeonSweepPopupElement.Init(SetRefreshDungeonDetail_Sweep);
            }
            
            for (int i = 0; i < Define.HellDungeonClearRewardGroup.Count; ++i)
            {
                if (m_uIHellDungeonGoodsRewardIconElement.Count > i)
                {
                    RewardData rewardData = new RewardData();
                    rewardData.V2Enum_Goods = V2Enum_Goods.Point;
                    rewardData.Index = Define.HellDungeonClearRewardGroup[i];
                    rewardData.Amount = 0;
                    m_uIHellDungeonGoodsRewardIconElement[i].SetRewardElement(rewardData);
                }
            }

            Message.AddListener<GameBerry.Event.SetDungeonSelectMsg>(SetDungeonSelect);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetDungeonSelectMsg>(SetDungeonSelect);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (m_autoEnter != null)
                m_autoEnter.isOn = Managers.DungeonDataManager.Instance.DungeonAutoEnter;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.DiamondDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_diamondDungeonClearGuideActiveTabInteractor);
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.MasteryDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_masteryDungeonClearGuideActiveTabInteractor);
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GoldDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_goldDungeonClearGuideActiveTabInteractor);
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SoulStoneDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_soulStoneDungeonClearGuideActiveTabInteractor);
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RuneDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_runeDungeonClearGuideActiveTabInteractor);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.DiamondDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_diamondDungeonClearGuideActiveTabInteractor);
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.MasteryDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_masteryDungeonClearGuideActiveTabInteractor);
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GoldDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_goldDungeonClearGuideActiveTabInteractor);
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SoulStoneDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_soulStoneDungeonClearGuideActiveTabInteractor);
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }
            else if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RuneDungeonClear)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(m_runeDungeonClearGuideActiveTabInteractor);
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }
        }
        //------------------------------------------------------------------------------------
        private void SetDungeonSelect(GameBerry.Event.SetDungeonSelectMsg msg)
        {
            SetDungeonMode(msg.v2Enum_Dungeon);
        }
        //------------------------------------------------------------------------------------
        public void SetRefreshDungeonDetail_Sweep()
        {
            SetDungeonMode(m_dungeonData.DungeonType);
        }
        //------------------------------------------------------------------------------------
        private void SetDungeonMode(V2Enum_Dungeon v2Enum_Dungeon)
        {
            m_dungeonData = Managers.DungeonDataManager.Instance.GetDungeonData(v2Enum_Dungeon);

            if (m_dungeonData == null)
                return;

            for (int i = 0; i < m_dungeonRewardIcon.Count; ++i)
            {
                m_dungeonRewardIcon[i].sprite = Managers.DungeonDataManager.Instance.GetDungeonRewardSprite(m_dungeonData.DungeonType);
            }

            Managers.DungeonDataManager.Instance.GetDungeonRewardInfo(m_dungeonData.DungeonType, out m_rewardData.Index);

            if (m_dungeonName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonName, m_dungeonData.NameLocalStringKey);

            if (m_dungeonBanner != null)
                m_dungeonBanner.sprite = StaticResource.Instance.GetIcon(m_dungeonData.BannerIconStringKey);

            m_dungeonModeBase = Managers.DungeonDataManager.Instance.GetMaxEnterDungeonModeData(m_dungeonData.DungeonType);

            if (m_uIGlobalGoodsRewardIconElement != null)
                m_uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);

            if (m_uIHellRewardRoot != null)
                m_uIHellRewardRoot.gameObject.SetActive(false);

            switch (m_dungeonData.DungeonType)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        if (m_defficultyBossName != null)
                            Managers.LocalStringManager.Instance.SetLocalizeText(m_defficultyBossName, "monster/1055/name");

                        SetDungeonDetailData(m_dungeonModeBase);

                        break;
                    }
            }

            SetTickAndBtnInitState();
        }
        //------------------------------------------------------------------------------------
        private void SetDungeonDetailData(DungeonModeBase dungeonModeBase)
        {
            if (dungeonModeBase == null)
                return;

            if (m_defficultyGroup != null)
                m_defficultyGroup.gameObject.SetActive(true);

            if (m_dungeonhightScoreGroup != null)
                m_dungeonhightScoreGroup.gameObject.SetActive(false);

            if (m_dungeonhightStepGroup != null)
                m_dungeonhightStepGroup.gameObject.SetActive(false);

            if (m_prevDefficulty != null)
                m_prevDefficulty.gameObject.SetActive(dungeonModeBase.PrevData != null);

            if (m_nextDefficulty != null)
            {
                if (dungeonModeBase.NextData == null)
                    m_nextDefficulty.gameObject.SetActive(false);
                else
                {
                    m_nextDefficulty.gameObject.SetActive(true);
                    bool isprevstep = dungeonModeBase.DungeonNumber <= Managers.DungeonDataManager.Instance.GetDungeonRecord(m_dungeonData.DungeonType);
                    m_nextDefficulty.image.color = isprevstep == true ? Color.white : Color.gray;
                    m_nextDefficulty.interactable = isprevstep;
                }
            }

            if (m_dungeonLevel != null)
                m_dungeonLevel.SetText("{0}", dungeonModeBase.DungeonNumber);

            if (m_uIGlobalGoodsRewardIconElement != null)
            {
                m_rewardData.Amount = dungeonModeBase.ClearRewardParam2;
                m_uIGlobalGoodsRewardIconElement.SetRewardElement(m_rewardData);
            }

            if (m_dungeonhightScoreRewardAmount != null)
                m_dungeonhightScoreRewardAmount.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void SetJustHightStep(int hightStep)
        {
            if (m_dungeonData == null)
                return;

            if (m_defficultyGroup != null)
                m_defficultyGroup.gameObject.SetActive(false);

            if (m_dungeonhightScoreGroup != null)
                m_dungeonhightScoreGroup.gameObject.SetActive(false);

            if (m_dungeonhightStepGroup != null)
                m_dungeonhightStepGroup.gameObject.SetActive(false);

            if (m_dungeonhightStep != null)
                m_dungeonhightStep.text = hightStep.ToString();

            if (m_uIGlobalGoodsRewardIconElement != null)
            {
                m_rewardData.Amount = 0.0;
                m_uIGlobalGoodsRewardIconElement.SetRewardElement(m_rewardData);
            }

            if (m_dungeonhightScoreRewardAmount != null)
            { 
                m_dungeonhightScoreRewardAmount.gameObject.SetActive(true);
                Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonhightScoreRewardAmount, "dungeon/proportionalSoulReward");
            }

            if (m_dungeonhightPhase != null)
                m_dungeonhightPhase.text = hightStep.ToString();
        }
        //------------------------------------------------------------------------------------
        private void SetJustHightScore(double hightScore, int hightStep = 0)
        {
            if (m_dungeonData == null)
                return;

            if (m_defficultyGroup != null)
                m_defficultyGroup.gameObject.SetActive(false);

            if (m_dungeonhightScoreGroup != null)
                m_dungeonhightScoreGroup.gameObject.SetActive(true);

            if (m_dungeonhightScore != null)
                m_dungeonhightScore.text = Util.GetAlphabetNumber(hightScore);

            if (m_dungeonhightStepGroup != null)
                m_dungeonhightStepGroup.gameObject.SetActive(false);

            if (m_uIGlobalGoodsRewardIconElement != null)
            {
                m_rewardData.Amount = 0.0;
                m_uIGlobalGoodsRewardIconElement.SetRewardElement(m_rewardData);
            }

            if (m_dungeonhightScoreRewardAmount != null)
            {
                m_dungeonhightScoreRewardAmount.gameObject.SetActive(true);
                Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonhightScoreRewardAmount, "dungeon/proportionalReward");
            }

            if (m_dungeonhightPhase != null)
                m_dungeonhightPhase.text = hightStep.ToString();
        }
        //------------------------------------------------------------------------------------
        private void SetTickAndBtnInitState()
        {
            if (m_dungeonData == null)
                return;

            int remainticket = (int)Managers.DungeonDataManager.Instance.GetDungeonTicketAmount(m_dungeonData.DungeonType);

            DungeonData dungeonTicketData = Managers.DungeonDataManager.Instance.GetDungeonData(m_dungeonData.DungeonType);

            m_isAdMode = false;

            if (m_dungeonTicketImage != null)
            {
                if (remainticket == 0)
                {
                    m_dungeonTicketImage.sprite = m_dungeonAdIcon;
                    remainticket = Managers.DungeonDataManager.Instance.GetDungeonRemainAdViewCount(m_dungeonData.DungeonType);
                    m_isAdMode = true;
                }
                else
                {
                    m_dungeonTicketImage.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(dungeonTicketData.EnterCostParam1);
                }
            }

            if (m_dungeonTickAmount != null)
                m_dungeonTickAmount.text = string.Format("{0}/{1}",
                    remainticket,
                    (int)dungeonTicketData.EnterCostRechargeAmount);

            if (remainticket <= 0)
            {
                if (m_sweepBtn != null)
                {
                    m_sweepBtn.interactable = false;
                    m_sweepBtn.image.color = m_btn_Disable;
                }

                if (m_playBtn != null)
                {
                    m_playBtn.interactable = false;
                    m_playBtn.image.color = m_btn_Disable;
                }

                if (m_sweepText != null)
                {
                    m_sweepText.color = m_text_Disable;
                }

                if (m_playText != null)
                {
                    m_playText.color = m_text_Disable;
                }
            }
            else
            {
                bool sweep = Managers.DungeonDataManager.Instance.AlreadySweepDungeon(m_dungeonData.DungeonType);

                if (m_sweepBtn != null)
                {
                    m_sweepBtn.interactable = sweep;
                    m_sweepBtn.image.color = sweep == true ? m_btn_Enable : m_btn_Disable;
                }

                if (m_playBtn != null)
                {
                    m_playBtn.interactable = true;
                    m_playBtn.image.color = m_btn_Enable;
                }

                if (m_sweepText != null)
                {
                    m_sweepText.color = sweep == true ? m_text_Enable : m_text_Disable;
                }

                if (m_playText != null)
                {
                    m_playText.color = m_text_Enable;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            RequestDialogExit<DungeonSelectDialog>();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PrevDefficulty()
        {
            if (m_dungeonModeBase == null)
                return;

            if (m_dungeonModeBase.PrevData == null)
                return;

            m_dungeonModeBase = m_dungeonModeBase.PrevData;

            SetDungeonDetailData(m_dungeonModeBase);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_NextDefficulty()
        {
            if (m_dungeonModeBase == null)
                return;

            if (m_dungeonModeBase.NextData == null)
                return;

            m_dungeonModeBase = m_dungeonModeBase.NextData;

            SetDungeonDetailData(m_dungeonModeBase);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SweepBtn()
        {
            if (m_dungeonData == null)
                return;

            RequestDialogEnter<UIDungeonSweepPopupElement>();
            if (m_uIDungeonSweepPopupElement != null)
            {
                m_uIDungeonSweepPopupElement.ElementEnter();
                m_uIDungeonSweepPopupElement.SetDungeonSelect(m_dungeonData.DungeonType);
            }
            //if (m_uIDungeonSweepPopupElement != null)
            //{ 
            //    m_uIDungeonSweepPopupElement.gameObject.SetActive(true);
            //    m_uIDungeonSweepPopupElement.SetDungeonSelect(m_dungeonData.DungeonType);
            //}
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlayBtn()
        {
            if (m_isAdMode == true)
            {
                Managers.DungeonDataManager.Instance.SetAdTicket(m_dungeonData.DungeonType, () =>
                {
                    EnterDungeon();
                });
            }
            else
            {
                EnterDungeon();
            }
        }
        //------------------------------------------------------------------------------------
        private void EnterDungeon()
        {
            switch (m_dungeonData.DungeonType)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                case V2Enum_Dungeon.TowerDungeon:
                case V2Enum_Dungeon.GoldDungeon:
                case V2Enum_Dungeon.RuneDungeon:
                    {
                        if (m_dungeonData == null || m_dungeonModeBase == null)
                            return;

                        Managers.DungeonDataManager.Instance.SetEnterDungeonStep(m_dungeonData.DungeonType, m_dungeonModeBase.DungeonNumber);

                        break;

                    }
            }

            if (m_autoEnter != null)
            {
                Managers.DungeonDataManager.Instance.SetAutoEnter(m_autoEnter.isOn);
            }

            Managers.BattleSceneManager.Instance.ChangeBattleScene(m_dungeonData.DungeonType);
            OnClick_ExitBtn();
            RequestDialogExit<DungeonContentDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}