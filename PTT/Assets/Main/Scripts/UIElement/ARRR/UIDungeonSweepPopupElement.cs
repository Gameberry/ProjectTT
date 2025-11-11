using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIDungeonSweepPopupElement : IDialog
    {
        [SerializeField]
        private List<Image> m_dungeonRewardIcon;

        [SerializeField]
        private Transform m_rewardRoot;

        [SerializeField]
        private TMP_Text m_dungeonRewardAmount;

        [SerializeField]
        private Transform m_dungeonMultiRewardAmount;

        [SerializeField]
        private Transform m_dungeonMultiReward_Root;

        [SerializeField]
        private UIIconAmountElement m_dungeonMultiReward_Icon;

        private List<UIIconAmountElement> m_dungeonRewardRoot = new List<UIIconAmountElement>();

        [SerializeField]
        private Image m_dungeonTicketImage;

        [SerializeField]
        private Sprite m_dungeonAdIcon;

        [SerializeField]
        private TMP_Text m_dungeonTickAmount;


        [SerializeField]
        private Button m_sweepBtn;

        [SerializeField]
        private Button m_sweepAllBtn;

        [SerializeField]
        private Color m_btn_Disable;

        [SerializeField]
        private Color m_btn_Enable;


        [SerializeField]
        private TMP_Text m_sweepText;

        [SerializeField]
        private TMP_Text m_sweepAllText;

        [SerializeField]
        private Color m_text_Disable;

        [SerializeField]
        private Color m_text_Enable;


        private DungeonData m_dungeonData;
        private DungeonModeBase m_dungeonModeBase;

        private bool m_isAdMode = false;

        private System.Action m_refreshDungeonSelect = null;

        //------------------------------------------------------------------------------------
        public void Init(System.Action action)
        {
            if (m_sweepBtn != null)
                m_sweepBtn.onClick.AddListener(OnClick_SweepBtn);

            if (m_sweepAllBtn != null)
                m_sweepAllBtn.onClick.AddListener(OnClick_SweepAllBtn);

            m_refreshDungeonSelect = action;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            transform.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SweepBtn()
        {
            Managers.DungeonDataManager.Instance.DoSweepOnceDungeon(m_dungeonData.DungeonType, o =>
            {
                if (o == true)
                {
                    if (m_refreshDungeonSelect != null)
                        m_refreshDungeonSelect();

                    SetDungeonSelect(m_dungeonData.DungeonType);
                }
            });

            //if (Managers.DungeonDataManager.Instance.DoSweepOnceDungeon(m_dungeonData.DungeonType) == true)
            //{
            //    if (m_refreshDungeonSelect != null)
            //        m_refreshDungeonSelect();

            //    SetDungeonSelect(m_dungeonData.DungeonType);
            //}
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SweepAllBtn()
        {
            if (Managers.DungeonDataManager.Instance.DoSweepAllDungeon(m_dungeonData.DungeonType) == true)
            {
                if (m_refreshDungeonSelect != null)
                    m_refreshDungeonSelect();

                SetDungeonSelect(m_dungeonData.DungeonType);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetDungeonSelect(V2Enum_Dungeon v2Enum_Dungeon)
        {
            m_dungeonData = Managers.DungeonDataManager.Instance.GetDungeonData(v2Enum_Dungeon);

            if (m_dungeonData == null)
                return;


            for (int i = 0; i < m_dungeonRewardIcon.Count; ++i)
            {
                m_dungeonRewardIcon[i].sprite = Managers.DungeonDataManager.Instance.GetDungeonRewardSprite(m_dungeonData.DungeonType);
            }

            if (m_rewardRoot != null)
                m_rewardRoot.gameObject.SetActive(false);

            if (m_dungeonRewardAmount != null)
                m_dungeonRewardAmount.gameObject.SetActive(false);

            if (m_dungeonMultiRewardAmount != null)
                m_dungeonMultiRewardAmount.gameObject.SetActive(false);

            switch (m_dungeonData.DungeonType)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                case V2Enum_Dungeon.TowerDungeon:
                case V2Enum_Dungeon.GoldDungeon:
                case V2Enum_Dungeon.RuneDungeon:
                    {
                        m_dungeonModeBase = Managers.DungeonDataManager.Instance.GetMaxClearDungeonModeData(m_dungeonData.DungeonType);

                        if (m_rewardRoot != null)
                            m_rewardRoot.gameObject.SetActive(true);

                        if (m_dungeonRewardAmount != null)
                        {
                            m_dungeonRewardAmount.gameObject.SetActive(true);
                            m_dungeonRewardAmount.text = Util.GetAlphabetNumber(m_dungeonModeBase.ClearRewardParam2);
                        }
                        
                        break;
                    }
            }

            SetTickAndBtnInitState();
        }
        //------------------------------------------------------------------------------------
        private void SetTickAndBtnInitState()
        {
            if (m_dungeonData == null)
                return;

            int remainticket = (int)Managers.DungeonDataManager.Instance.GetDungeonTicketAmount(m_dungeonData.DungeonType);

            m_isAdMode = false;

            DungeonData dungeonTicketData = Managers.DungeonDataManager.Instance.GetDungeonData(m_dungeonData.DungeonType);

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

                if (m_sweepAllBtn != null)
                {
                    m_sweepAllBtn.interactable = false;
                    m_sweepAllBtn.image.color = m_btn_Disable;
                }

                if (m_sweepText != null)
                    m_sweepText.color = m_text_Disable;

                if (m_sweepAllText != null)
                    m_sweepAllText.color = m_text_Disable;
            }
            else
            {
                if (m_sweepBtn != null)
                {
                    m_sweepBtn.interactable = true;
                    m_sweepBtn.image.color = m_btn_Enable;
                }

                if (m_sweepAllBtn != null)
                {
                    m_sweepAllBtn.interactable = true;
                    m_sweepAllBtn.image.color = m_btn_Enable;
                    m_sweepAllBtn.gameObject.SetActive(m_isAdMode == false);
                }

                if (m_sweepText != null)
                    m_sweepText.color = m_text_Enable;

                if (m_sweepAllText != null)
                    m_sweepAllText.color = m_text_Enable;
            }
        }
        //------------------------------------------------------------------------------------
    }
}