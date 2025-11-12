using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIDungeonContentElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_dungeonBanner;

        [SerializeField]
        private Image m_dungeonRewardIcon;

        [SerializeField]
        private TMP_Text m_dungeonName;

        [SerializeField]
        private Transform m_defficultyGroup;

        [SerializeField]
        private Button m_prevDefficulty;

        [SerializeField]
        private Button m_nextDefficulty;


        [SerializeField]
        private TMP_Text m_dungeonLevel;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement;


        [Header("------------ButtonGroup------------")]
        [SerializeField]
        private Image m_dungeonTicketGroup;

        [SerializeField]
        private List<Image> m_dungeonTicketImage = new List<Image>();

        [SerializeField]
        private List<Image> m_dungeonEnable = new List<Image>();

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
        private Transform _endDungeon;


        [Header("------------DungeonLock------------")]
        [SerializeField]
        private Transform m_dungeon_Lock_Group;

        [SerializeField]
        private TMP_Text m_dungeon_Lock_Text;

        [SerializeField]
        private UIRedDotElement m_uiRedDotElement = null;

        private bool m_isAdMode = false;

        private DungeonData m_dungeonData = null;
        private DungeonModeBase m_dungeonModeBase;

        private V2Enum_ContentType m_myContentType = V2Enum_ContentType.Max;
        private ContentUnlockData m_mycontentUnlockData = null;

        private RewardData m_rewardData = new RewardData();

        //------------------------------------------------------------------------------------
        public void Init()
        {
            if (m_prevDefficulty != null)
                m_prevDefficulty.onClick.AddListener(OnClick_PrevDefficulty);

            if (m_nextDefficulty != null)
                m_nextDefficulty.onClick.AddListener(OnClick_NextDefficulty);

            if (m_sweepBtn != null)
                m_sweepBtn.onClick.AddListener(OnClick_SweepBtn);

            if (m_playBtn != null)
                m_playBtn.onClick.AddListener(OnClick_PlayBtn);
        }
        //------------------------------------------------------------------------------------
        public void SetDungeonContentElement(DungeonData dungeonData)
        {
            bool isOpen = Managers.ContentOpenConditionManager.Instance.IsOpen(m_myContentType);

            if (isOpen == false)
            {
                m_mycontentUnlockData = Managers.ContentOpenConditionManager.Instance.GetContentUnlockData(m_myContentType);

                if (m_dungeon_Lock_Group != null)
                    m_dungeon_Lock_Group.gameObject.SetActive(true);

                if (m_dungeon_Lock_Text != null)
                    m_dungeon_Lock_Text.SetText(Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(m_mycontentUnlockData.UnlockConditionType, m_mycontentUnlockData.UnlockConditionParam));

                Managers.ContentOpenConditionManager.Instance.AddOpenConditionEvent(m_mycontentUnlockData.UnlockConditionType, RefreshOpenCondition);
            }
            else
            {
                if (m_dungeon_Lock_Group != null)
                    m_dungeon_Lock_Group.gameObject.SetActive(false);
            }

            if (dungeonData.EnterCostParam1 != -1)
            {
                Managers.GoodsManager.Instance.AddGoodsRefreshEvent(dungeonData.EnterCostParam1, SetTicketAmount);
                SetTicketAmount(Managers.GoodsManager.Instance.GetGoodsAmount(dungeonData.EnterCostParam1));
            }

            Managers.DungeonDataManager.Instance.GetDungeonRewardInfo(dungeonData.DungeonType, out m_rewardData.Index);
        }
        //------------------------------------------------------------------------------------
        public void SetDungeonMode(Enum_Dungeon enumDungeon)
        {
            m_dungeonData = Managers.DungeonDataManager.Instance.GetDungeonData(enumDungeon);

            if (m_dungeonData == null)
                return;

            Managers.DungeonDataManager.Instance.GetDungeonRewardInfo(m_dungeonData.DungeonType, out m_rewardData.Index);

            if (m_dungeonRewardIcon != null)
                m_dungeonRewardIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(m_rewardData.Index);

            if (m_dungeonName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_dungeonName, m_dungeonData.NameLocalStringKey);

            if (m_dungeonBanner != null)
                m_dungeonBanner.sprite = StaticResource.Instance.GetIcon(m_dungeonData.BannerIconStringKey);

            m_dungeonModeBase = Managers.DungeonDataManager.Instance.GetMaxEnterDungeonModeData(m_dungeonData.DungeonType);

            if (m_uIGlobalGoodsRewardIconElement != null)
                m_uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);

            if (_endDungeon != null)
                _endDungeon.gameObject.SetActive(false);

            if (m_dungeonRewardIcon != null)
                m_dungeonRewardIcon.gameObject.SetActive(true);

            switch (m_dungeonData.DungeonType)
            {
                case Enum_Dungeon.DiamondDungeon:
                    {
                        SetDungeonDetailData(m_dungeonModeBase);
                        SetTickAndBtnInitState();
                        break;
                    }
                case Enum_Dungeon.TowerDungeon:
                    {
                        SetDungeonDetailData(m_dungeonModeBase);

                        if (m_sweepBtn != null)
                            m_sweepBtn.gameObject.SetActive(false);

                        if (m_playBtn != null)
                        {
                            m_playBtn.interactable = true;
                            m_playBtn.image.color = m_btn_Enable;
                            m_playBtn.gameObject.SetActive(true);
                        }

                        if (m_playText != null)
                        {
                            m_playText.color = m_text_Enable;
                        }

                        if (m_dungeonTicketGroup != null)
                            m_dungeonTicketGroup.gameObject.SetActive(false);

                        for (int i = 0; i < m_dungeonTicketImage.Count; ++i)
                        {
                            m_dungeonTicketImage[i].gameObject.SetActive(false);
                        }

                        for (int i = 0; i < m_dungeonEnable.Count; ++i)
                        {
                            m_dungeonEnable[i].gameObject.SetActive(false);
                        }

                        if (m_prevDefficulty != null)
                            m_prevDefficulty.gameObject.SetActive(false);

                        if (m_nextDefficulty != null)
                            m_nextDefficulty.gameObject.SetActive(false);

                        if (m_dungeonModeBase.DungeonNumber == Managers.DungeonDataManager.Instance.GetDungeonRecord(Enum_Dungeon.TowerDungeon).ToInt())
                        {
                            if (_endDungeon != null)
                                _endDungeon.gameObject.SetActive(true);

                            if (m_playBtn != null)
                                m_playBtn.gameObject.SetActive(false);
                            if (m_dungeonRewardIcon != null)
                                m_dungeonRewardIcon.gameObject.SetActive(false);
                        }

                        break;
                    }
            }

            
        }
        //------------------------------------------------------------------------------------
        private void SetDungeonDetailData(DungeonModeBase dungeonModeBase)
        {
            if (dungeonModeBase == null)
                return;

            if (m_defficultyGroup != null)
                m_defficultyGroup.gameObject.SetActive(true);

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
        }
        //------------------------------------------------------------------------------------
        private void SetTickAndBtnInitState()
        {
            if (m_dungeonData == null)
                return;

            int remainticket = (int)Managers.DungeonDataManager.Instance.GetDungeonTicketAmount(m_dungeonData.DungeonType);

            DungeonData dungeonTicketData = Managers.DungeonDataManager.Instance.GetDungeonData(m_dungeonData.DungeonType);

            m_isAdMode = false;

            if (m_dungeonTicketGroup != null)
                m_dungeonTicketGroup.gameObject.SetActive(true);

            if (m_dungeonTicketImage != null)
            {
                Sprite sp = null;

                if (remainticket == 0)
                {
                    sp = m_dungeonAdIcon;
                    remainticket = Managers.DungeonDataManager.Instance.GetDungeonRemainAdViewCount(m_dungeonData.DungeonType);
                    m_isAdMode = true;
                }
                else
                {
                    sp = Managers.GoodsManager.Instance.GetGoodsSprite(dungeonTicketData.EnterCostParam1);
                }

                for (int i = 0; i < m_dungeonTicketImage.Count; ++i)
                {
                    m_dungeonTicketImage[i].sprite = sp;
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
        private void RefreshOpenCondition(V2Enum_OpenConditionType v2Enum_OpenConditionType, int conditionValue)
        {
            bool isopen = Managers.ContentOpenConditionManager.Instance.IsOpen(m_myContentType);

            if (isopen == true)
            {
                if (m_dungeon_Lock_Group != null)
                    m_dungeon_Lock_Group.gameObject.SetActive(false);
                Managers.ContentOpenConditionManager.Instance.ShowOpenConditionNotice(m_myContentType);
                Managers.ContentOpenConditionManager.Instance.RemoveOpenConditionEvent(v2Enum_OpenConditionType, RefreshOpenCondition);

                ShowRedDot();
            }
        }
        //------------------------------------------------------------------------------------
        public void SetTicketAmount(double amount)
        {
            if (m_dungeonData == null)
                return;

            DungeonData dungeonTicketData = Managers.DungeonDataManager.Instance.GetDungeonData(m_dungeonData.DungeonType);

            if (m_dungeonTickAmount != null)
                m_dungeonTickAmount.text = string.Format("{0}/{1}", amount, (int)dungeonTicketData.EnterCostRechargeAmount);

            if (amount > 0)
            {
                ShowRedDot();
            }
            else
            {
                if (Managers.DungeonDataManager.Instance.GetDungeonRemainAdViewCount(m_dungeonData.DungeonType) > 0)
                    ShowRedDot();
                else
                    HideRedDot();
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowRedDot()
        {
            if (m_dungeonData == null)
                return;

            if (gameObject.activeInHierarchy == false)
            {
                if (Managers.ContentOpenConditionManager.Instance.IsOpen(m_myContentType) == false)
                    return;

                switch (m_dungeonData.DungeonType)
                {
                    case Enum_Dungeon.DiamondDungeon:
                        {
                            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.DungeonDiamond);
                            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Dungeon);
                            break;
                        }
                    case Enum_Dungeon.TowerDungeon:
                        {
                            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.DungeonTower);
                            break;
                        }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void HideRedDot()
        {
            if (m_dungeonData == null)
                return;

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(m_myContentType) == false)
                return;

            switch (m_dungeonData.DungeonType)
            {
                case Enum_Dungeon.DiamondDungeon:
                    {
                        Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.DungeonDiamond);
                        break;
                    }
                case Enum_Dungeon.TowerDungeon:
                    {
                        Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.DungeonTower);
                        break;
                    }
            }
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

            Managers.DungeonDataManager.Instance.DoSweepOnceDungeon(m_dungeonData.DungeonType, o =>
            {
                if (o == true)
                {
                    SetDungeonMode(m_dungeonData.DungeonType);
                }
            });
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
                case Enum_Dungeon.DiamondDungeon:
                case Enum_Dungeon.TowerDungeon:
                    {
                        if (m_dungeonData == null || m_dungeonModeBase == null)
                            return;

                        Managers.DungeonDataManager.Instance.SetEnterDungeonStep(m_dungeonData.DungeonType, m_dungeonModeBase.DungeonNumber);

                        break;

                    }
            }

            Managers.BattleSceneManager.Instance.ChangeBattleScene(m_dungeonData.DungeonType);
            IDialog.RequestDialogExit<DungeonContentDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}