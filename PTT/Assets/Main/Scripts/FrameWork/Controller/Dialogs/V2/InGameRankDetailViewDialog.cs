using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class InGameRankDetailViewDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private TMP_Text userNickName;

        [SerializeField]
        private TMP_Text combatPower;

        [SerializeField]
        private TMP_Text clanLevel;

        [SerializeField]
        private Image fame;

        [SerializeField]
        private TMP_Text fameLevel;

        [SerializeField]
        private Image m_profile;

        [SerializeField]
        private TMP_Text m_server;


        //[SerializeField]
        //private List<UIAllyElement> allyElements = new List<UIAllyElement>();

        [SerializeField]
        private Transform buffRingDialog;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(() =>
                        {
                            UIManager.DialogExit<InGameRankDetailViewDialog>();
                        });
                }
            }

            Message.AddListener<GameBerry.Event.SetSodialDialogMsg>(SetRankDetailDialog);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetSodialDialogMsg>(SetRankDetailDialog);
        }
        //------------------------------------------------------------------------------------
        private void SetRankDetailDialog(GameBerry.Event.SetSodialDialogMsg msg)
        {
            if (userNickName != null)
                userNickName.text = msg.nickName;

            if (msg.needData == true)
            {
                if (buffRingDialog != null)
                    buffRingDialog.gameObject.SetActive(true);
            }
            else
                SetDetailList(msg.detailinfo);
        }
        //------------------------------------------------------------------------------------
        private void SetDetailList(string detail)
        {
            if (buffRingDialog != null)
                buffRingDialog.gameObject.SetActive(false);

            RankDetailInfo rankDetailInfo = new RankDetailInfo();
            rankDetailInfo.SetDetailInfo(detail);

            SetDetailList(rankDetailInfo);
        }
        //------------------------------------------------------------------------------------
        private void SetDetailList(RankDetailInfo rankDetailInfo)
        {
            if (combatPower != null)
                combatPower.text = string.Format("{0}", rankDetailInfo.combatpower);

            if (clanLevel != null)
                clanLevel.text = string.Format("Lv.{0}", rankDetailInfo.clanlevel);

            if (m_server != null)
            {
                Debug.Log(rankDetailInfo.server);
                m_server.text = rankDetailInfo.server;
            }

            if (m_profile != null)
            {
                CharacterProfileData characterProfileData = Managers.PlayerDataManager.Instance.GetProfile(rankDetailInfo.profile);

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



            //for (int i = 0; i < rankDetailInfo.allyDetailData.Count; ++i)
            //{
            //    if (allyElements.Count > i)
            //    {
            //        List<ObscuredInt> elements = rankDetailInfo.allyDetailData[i];

            //        if (elements.Count < 3)
            //        {
            //            allyElements[i].SetAllyElement(null);
            //            continue;
            //        }

            //        PlayerAllyV3Info playerV3AllyInfo = new PlayerAllyV3Info();

            //        playerV3AllyInfo.CreatureIndex = elements[0].GetDecrypted();
            //        playerV3AllyInfo.Level = elements[1].GetDecrypted();
            //        playerV3AllyInfo.Star = elements[2].GetDecrypted();

            //        //allyElements[i].SetAllyElement(playerV3AllyInfo, true);
            //        allyElements[i].gameObject.SetActive(true);
            //    }
            //}

            //for (int i = rankDetailInfo.allyDetailData.Count; i < allyElements.Count; ++i)
            //{
            //    allyElements[i].gameObject.SetActive(false);
            //}
        }
        //------------------------------------------------------------------------------------
    }
}