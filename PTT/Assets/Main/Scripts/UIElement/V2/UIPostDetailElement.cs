using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIPostDetailElement : IDialog
    {
        [Header("------------PostDetail------------")]
        [SerializeField]
        private TMP_Text m_postTitle;

        [SerializeField]
        private TMP_Text m_postContent;

        [SerializeField]
        private TMP_Text m_postRemainTime;

        [SerializeField]
        private Transform m_postItemRoot;

        [SerializeField]
        private Button m_postDetail_AllRecvBtn;

        [SerializeField]
        private List<Button> m_exitBtn;

        private InGamePostPopupDialog m_inGamePostPopupDialog;

        private PostInfo m_currentPostInfo = null;
        private List<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new List<UIGlobalGoodsRewardIconElement>();
        //------------------------------------------------------------------------------------
        public void Init(InGamePostPopupDialog inGamePostPopupDialog)
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }

            if (m_postDetail_AllRecvBtn != null)
                m_postDetail_AllRecvBtn.onClick.AddListener(OnClick_RecvAndDeletePost);

            m_inGamePostPopupDialog = inGamePostPopupDialog;

            
        }
        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString -= RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        private void RefreshLocalize()
        {
            if (m_currentPostInfo == null)
                return;

            if (m_postRemainTime != null)
                m_postRemainTime.text = string.Format("{0} : {1}", Managers.LocalStringManager.Instance.GetLocalString("common/expirationDate"), m_currentPostInfo.ExpirationDate.ToString());
        }
        //------------------------------------------------------------------------------------
        public void SetPostView(PostInfo postInfo)
        {
            m_currentPostInfo = postInfo;

            if (postInfo == null)
                return;

            Release();

            if (m_postTitle != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(m_postTitle, postInfo.Title);
            }

            if (m_postContent != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(m_postContent, postInfo.Content);
            }

            if (m_postRemainTime != null)
                m_postRemainTime.text = string.Format("{0} : {1}", Managers.LocalStringManager.Instance.GetLocalString("common/expirationDate"), postInfo.ExpirationDate.ToString());

            if (postInfo.RewardData != null)
            {
                for (int i = 0; i < postInfo.RewardData.Count; ++i)
                {
                    UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon_NoneParticle();

                    if (uIGlobalGoodsRewardIconElement == null)
                        continue;

                    RewardData rewardData = postInfo.RewardData[i];

                    uIGlobalGoodsRewardIconElement.transform.SetParent(m_postItemRoot);
                    uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);

                    uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
                    m_uIGlobalGoodsRewardIconElements.Add(uIGlobalGoodsRewardIconElement);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            ElementExit();
            Release();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RecvAndDeletePost()
        {
            if (m_currentPostInfo != null)
            {
                if (m_inGamePostPopupDialog != null)
                    m_inGamePostPopupDialog.ChangeWaitState(true);

                if (m_currentPostInfo.IsShop == true)
                    Managers.ShopPostManager.Instance.RecvPostReward(m_currentPostInfo.InData);
                else
                    Managers.PostManager.Instance.ReceivePostItem(m_currentPostInfo.InData);
                OnClick_ExitBtn();
            }
        }
        //------------------------------------------------------------------------------------
        private void Release()
        {
            for (int i = 0; i < m_uIGlobalGoodsRewardIconElements.Count; ++i)
            {
                Managers.RewardManager.Instance.PoolGoodsRewardIcon(m_uIGlobalGoodsRewardIconElements[i]);
            }

            m_uIGlobalGoodsRewardIconElements.Clear();
        }
        //------------------------------------------------------------------------------------
    }
}