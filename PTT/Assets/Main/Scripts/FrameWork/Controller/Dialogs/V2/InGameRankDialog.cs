using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class InGameRankDialog : IDialog
    {
        [Header("------------PostTab------------")]
        [SerializeField]
        private Button m_combatPower;

        [SerializeField]
        private TMP_Text m_combatPower_Text;


        [SerializeField]
        private Button m_stage;

        [SerializeField]
        private TMP_Text m_stage_Text;


        [SerializeField]
        private Sprite m_selectTabBG;

        [SerializeField]
        private Sprite m_noneTabBG;


        [SerializeField]
        private Color m_selectTab_Text;

        [SerializeField]
        private Color m_noneTab_Text;

        [SerializeField]
        private TMP_Text m_server_Name;

        [Header("------------ElementGroup------------")]
        [SerializeField]
        private Image m_elementGroupFrame;

        [SerializeField]
        private InfiniteScroll m_rankElementInfinityScroll;

        [SerializeField]
        private UIRankElement m_myRankElement;

        private ContentDetailList m_v2Enum_CheckInType = ContentDetailList.None;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_combatPower != null)
                m_combatPower.onClick.AddListener(OnClick_CombatPower);

            if (m_stage != null)
                m_stage.onClick.AddListener(OnClick_Stage);

            //if (m_rankElementInfinityScroll != null)
            //    m_rankElementInfinityScroll.AddSelectCallback(OnClick_AllyElement);

            //SetRankElement(ContentDetailList.Rank_CombatPower);

            //Message.AddListener<GameBerry.Event.RefreshPostListMsg>(RefreshPostList);
            //Message.AddListener<GameBerry.Event.RefreshShopPostListMsg>(RefreshShopPostList);

            if (m_server_Name != null)
                m_server_Name.text = string.Format("{0}", PlayerDataContainer.RankWindowDisplayServerName);
            Message.AddListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshNickNameMsg>(RefreshNickName);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if(m_v2Enum_CheckInType == ContentDetailList.None)
                SetRankElement(ContentDetailList.Rank_CombatPower);

            if (TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName() == BackEnd.Backend.UID)
                UIManager.DialogEnter<InGameNickNameChangePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        private void RefreshNickName(GameBerry.Event.RefreshNickNameMsg msg)
        {
            ContentDetailList contentDetailList = m_v2Enum_CheckInType;

            m_v2Enum_CheckInType = ContentDetailList.None;
            SetRankElement(contentDetailList);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CombatPower()
        {
            SetRankElement(ContentDetailList.Rank_CombatPower);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Stage()
        {
            SetRankElement(ContentDetailList.Rank_Stage);
        }
        //------------------------------------------------------------------------------------
        private void SetRankElement(ContentDetailList contentDetailList)
        {
            if (m_v2Enum_CheckInType == contentDetailList)
                return;

            m_v2Enum_CheckInType = contentDetailList;


            int elementGroupFrameSibling = m_elementGroupFrame.transform.GetSiblingIndex();

            if (m_combatPower != null)
            {
                m_combatPower.image.sprite = m_v2Enum_CheckInType == ContentDetailList.Rank_Stage ? m_noneTabBG : m_selectTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_CheckInType == ContentDetailList.Rank_Stage ? -1 : 1);
                m_combatPower.transform.SetSiblingIndex(newsibling);
            }

            if (m_stage != null)
            {
                m_stage.image.sprite = m_v2Enum_CheckInType == ContentDetailList.Rank_Stage ? m_selectTabBG : m_noneTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_CheckInType == ContentDetailList.Rank_Stage ? 1 : -1);
                m_stage.transform.SetSiblingIndex(newsibling);
            }


            if (m_combatPower_Text != null)
                m_combatPower_Text.color = m_v2Enum_CheckInType == ContentDetailList.Rank_Stage ? m_noneTab_Text : m_selectTab_Text;

            if (m_stage_Text != null)
                m_stage_Text.color = m_v2Enum_CheckInType == ContentDetailList.Rank_Stage ? m_selectTab_Text : m_noneTab_Text;

            Managers.RankManager.Instance.RefreshMyRankData();

            if (m_v2Enum_CheckInType == ContentDetailList.Rank_CombatPower)
            {
                if (Managers.RankManager.Instance.NeedUpdateRank(V2Enum_RankType.Power) == false)
                {
                    TheBackEnd.TheBackEnd_Rank.GetRankList(V2Enum_RankType.Power, RefreshRankWindow);
                }
                else
                {
                    TheBackEnd.TheBackEnd_Rank.UpdateUserScore(
                        V2Enum_RankType.Power,
                        Managers.RankManager.Instance.GetMyRankData().CombatPower.GetDecrypted(),
                        Managers.RankManager.Instance.GetDetailString(), 
                        () =>
                        {
                            TheBackEnd.TheBackEnd_Rank.GetRankList(V2Enum_RankType.Power, RefreshRankWindow);
                        });
                }
            }
            else if (m_v2Enum_CheckInType == ContentDetailList.Rank_Stage)
            {
                if (Managers.RankManager.Instance.NeedUpdateRank(V2Enum_RankType.Stage) == false)
                {
                    TheBackEnd.TheBackEnd_Rank.GetRankList(V2Enum_RankType.Stage, RefreshRankWindow);
                }
                else
                {
                    TheBackEnd.TheBackEnd_Rank.UpdateUserScore(
                        V2Enum_RankType.Stage,
                        Managers.RankManager.Instance.GetMyRankData().Stage.GetDecrypted(),
                        Managers.RankManager.Instance.GetDetailString(),
                        () =>
                        {
                            TheBackEnd.TheBackEnd_Rank.GetRankList(V2Enum_RankType.Stage, RefreshRankWindow);
                        });
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshRankWindow()
        {
            V2Enum_RankType v2Enum_RankType = m_v2Enum_CheckInType == ContentDetailList.Rank_Stage ? V2Enum_RankType.Stage : V2Enum_RankType.Power;

            if (m_rankElementInfinityScroll != null)
            {
                m_rankElementInfinityScroll.Clear();

                List <RankerData> RankerDatas = Managers.RankManager.Instance.GetRankers().FindAll(x => x.v2Enum_RankType == v2Enum_RankType);

                for (int i = 0; i < RankerDatas.Count; ++i)
                {
                    m_rankElementInfinityScroll.InsertData(RankerDatas[i]);
                }

                m_rankElementInfinityScroll.MoveToFirstData();
            }

            if (m_myRankElement != null)
            {
                RankTable rankTable = Managers.RankManager.Instance.GetRankTable(v2Enum_RankType);
                if (rankTable == null)
                    return;

                m_myRankElement.SetNickName(TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName());
                m_myRankElement.SetScore(v2Enum_RankType, rankTable.Score);
                m_myRankElement.SetRank(rankTable.MyRank);

                MyRankInfo myRankInfo = Managers.RankManager.Instance.GetMyRankData();
                if (myRankInfo != null)
                {
                    m_myRankElement.SetProfile(myRankInfo.Detail.profile);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}