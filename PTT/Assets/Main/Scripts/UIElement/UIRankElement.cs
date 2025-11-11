using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UIRankElement : InfiniteScrollItem
    {
        [SerializeField]
        private Image rankerMark;

        [SerializeField]
        private Sprite rankerMarkSprite_1;

        [SerializeField]
        private Sprite rankerMarkSprite_2;

        [SerializeField]
        private Sprite rankerMarkSprite_3;

        [SerializeField]
        private Image m_fame;

        [SerializeField]
        private Image m_profile;

        [SerializeField]
        private TMP_Text m_nickName;

        [SerializeField]
        private TMP_Text m_isMine;

        [SerializeField]
        private TMP_Text m_scoreTitle;

        [SerializeField]
        private TMP_Text m_score;

        [SerializeField]
        private TMP_Text m_rank;

        [SerializeField]
        private TMP_Text server;

        private string m_detail;

        [SerializeField]
        private Button m_elementBtn;

        private RankDetailInfo rankDetailInfo = new RankDetailInfo();
        private RankerData myRankerData;

        private void Awake()
        {
            if (m_elementBtn != null)
                m_elementBtn.onClick.AddListener(ShowDetailView);
        }
        //------------------------------------------------------------------------------------
        public override void UpdateData(InfiniteScrollData scrollData)
        {
            RankerData playerV3AllyInfo = scrollData as RankerData;
            if (playerV3AllyInfo == null)
            {
                myRankerData = null;
                return;
            }

            SetRankElement(playerV3AllyInfo);
        }
        //------------------------------------------------------------------------------------
        public void SetRankElement(RankerData rankerData)
        {
            if (rankerData == null)
                return;

            SetRankInfo(rankerData.detail);
            SetNickName(rankerData.nickName);
            SetScore(rankerData.v2Enum_RankType, rankerData.score);
            SetRank(rankerData.rank);
            SetProfile(rankDetailInfo.profile);
            SetServer(rankDetailInfo.server);

            m_detail = rankerData.detail;

            myRankerData = rankerData;
        }
        //------------------------------------------------------------------------------------
        public void SetNickName(string nickname)
        {
            if (m_nickName != null)
                m_nickName.SetText(nickname);

            if (m_isMine != null)
                m_isMine.gameObject.SetActive(nickname == TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName());
        }
        //------------------------------------------------------------------------------------
        public void SetScore(V2Enum_RankType v2Enum_RankType, double score)
        {
            if (m_scoreTitle != null)
            {
                if (v2Enum_RankType == V2Enum_RankType.Power)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_scoreTitle, "rank/combatPower");
                else if (v2Enum_RankType == V2Enum_RankType.Stage)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_scoreTitle, "rank/stage");
            }
            
            if (m_score != null)
            {
                if (v2Enum_RankType == V2Enum_RankType.Power)
                {
                    m_score.SetText(string.Format("{0:0,0}", score));
                }
                else if (v2Enum_RankType == V2Enum_RankType.Stage)
                {
                    m_score.SetText(MapOperator.ConvertWaveTotalNumberToUIString(score.ToInt()));
                }
                else
                    m_score.SetText(score.ToString());
            }
        }
        //------------------------------------------------------------------------------------
        public void SetRank(int rank)
        {
            if (m_rank != null)
                m_rank.SetText("{0}", rank);

            if (rankerMark != null)
            {
                if (rank == 1)
                {
                    rankerMark.gameObject.SetActive(true);
                    rankerMark.sprite = rankerMarkSprite_1;
                }
                else if (rank == 2)
                {
                    rankerMark.gameObject.SetActive(true);
                    rankerMark.sprite = rankerMarkSprite_2;
                }
                else if (rank == 3)
                {
                    rankerMark.gameObject.SetActive(true);
                    rankerMark.sprite = rankerMarkSprite_3;
                }
                else
                    rankerMark.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetProfile(int profile)
        {
            CharacterProfileData characterProfileData = Managers.PlayerDataManager.Instance.GetProfile(profile);

            if (m_profile != null)
            {
                if (characterProfileData == null)
                    m_profile.gameObject.SetActive(false);
                else
                {
                    m_profile.gameObject.SetActive(true);
                    m_profile.sprite = characterProfileData.sprite;
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetServer(string servertext)
        {
            if (server != null)
                server.text = servertext;
        }
        //------------------------------------------------------------------------------------
        public void SetRankInfo(string detail)
        {
            rankDetailInfo.SetDetailInfo(detail);
        }
        //------------------------------------------------------------------------------------
        public void ShowDetailView()
        {
            Managers.RankManager.Instance.ShowRankDetailView(myRankerData);
        }
        //------------------------------------------------------------------------------------
        public string GetDetail()
        {
            return m_detail;
        }
        //------------------------------------------------------------------------------------
    }
}