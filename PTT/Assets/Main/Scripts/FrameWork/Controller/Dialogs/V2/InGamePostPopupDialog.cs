using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class InGamePostPopupDialog : IDialog
    {
        public static bool IsEnterState = false;

        [SerializeField]
        private List<Button> m_exitBtn;


        [SerializeField]
        private Button m_allRecvBtn;

        [Header("------------PostTab------------")]
        [SerializeField]
        private Button m_postAdmin;

        [SerializeField]
        private TMP_Text m_postAdmin_Text;


        [SerializeField]
        private Button m_postStore;

        [SerializeField]
        private TMP_Text m_postStore_Text;


        [SerializeField]
        private Sprite m_selectTabBG;

        [SerializeField]
        private Sprite m_noneTabBG;


        [SerializeField]
        private Color m_selectTab_Text;

        [SerializeField]
        private Color m_noneTab_Text;


        [Header("------------ElementGroup------------")]
        [SerializeField]
        private Image m_elementGroupFrame;

        [SerializeField]
        private UIPostElement m_postElement;

        [SerializeField]
        private RectTransform m_scrollRectContent;

        private List<UIPostElement> m_uIPostElements = new List<UIPostElement>();
        private Dictionary<string, UIPostElement> m_uIPostElements_Dic = new Dictionary<string, UIPostElement>();

        private Queue<UIPostElement> m_uIPostElementPool = new Queue<UIPostElement>();

        [SerializeField]
        private Image m_postRefreshingDirection;

        [Header("------------PostDetail------------")]
        [SerializeField]
        private UIPostDetailElement m_uIPostDetailElement;

        private bool m_waitState = false;

        private ContentDetailList m_v2Enum_CheckInType = ContentDetailList.None;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_uIPostDetailElement != null)
                m_uIPostDetailElement.Init(this);

            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(() =>
                        {
                            UIManager.DialogExit<InGamePostPopupDialog>();
                        });
                }
            }

            if (m_allRecvBtn != null)
                m_allRecvBtn.onClick.AddListener(OnClick_AllRecvAndDeletePost);

            if (m_postAdmin != null)
                m_postAdmin.onClick.AddListener(OnClick_PostGeneralTab);

            if (m_postStore != null)
                m_postStore.onClick.AddListener(OnClick_PostShopTab);

            SetPostShopElement(ContentDetailList.PostGeneral);

            Message.AddListener<GameBerry.Event.RefreshPostListMsg>(RefreshPostList);
            Message.AddListener<GameBerry.Event.RefreshShopPostListMsg>(RefreshShopPostList);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshPostListMsg>(RefreshPostList);
            Message.RemoveListener<GameBerry.Event.RefreshShopPostListMsg>(RefreshShopPostList);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (Managers.PostManager.Instance.NeedRefreshPost() == true)
            {
                Managers.PostManager.Instance.RefreshAdminPost();

                ChangeWaitState(true);
            }
            else
            {
                ChangeWaitState(false);
            }

            if (m_v2Enum_CheckInType == ContentDetailList.PostGeneral)
            {
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.PostGeneral);

                if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.MailGet)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
                }
            }
            else if (m_v2Enum_CheckInType == ContentDetailList.PostShop)
            { 
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.PostShop);

                if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.MailGet)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(4);
                }
            }

            IsEnterState = true;
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.MailGet)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(1);
            }

            IsEnterState = false;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PostGeneralTab()
        {
            SetPostShopElement(ContentDetailList.PostGeneral);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PostShopTab()
        {
            SetPostShopElement(ContentDetailList.PostShop);
        }
        //------------------------------------------------------------------------------------
        private void SetPostShopElement(ContentDetailList contentDetailList)
        {
            if (m_v2Enum_CheckInType == contentDetailList)
                return;

            m_v2Enum_CheckInType = contentDetailList;


            int elementGroupFrameSibling = m_elementGroupFrame.transform.GetSiblingIndex();

            if (m_postAdmin != null)
            {
                m_postAdmin.image.sprite = m_v2Enum_CheckInType == ContentDetailList.PostShop ? m_noneTabBG : m_selectTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_CheckInType == ContentDetailList.PostShop ? -1 : 1);
                m_postAdmin.transform.SetSiblingIndex(newsibling);
            }

            if (m_postStore != null)
            {
                m_postStore.image.sprite = m_v2Enum_CheckInType == ContentDetailList.PostShop ? m_selectTabBG : m_noneTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_CheckInType == ContentDetailList.PostShop ? 1 : -1);
                m_postStore.transform.SetSiblingIndex(newsibling);
            }


            if (m_postAdmin_Text != null)
                m_postAdmin_Text.color = m_v2Enum_CheckInType == ContentDetailList.PostShop ? m_noneTab_Text : m_selectTab_Text;

            if (m_postStore_Text != null)
                m_postStore_Text.color = m_v2Enum_CheckInType == ContentDetailList.PostShop ? m_selectTab_Text : m_noneTab_Text;

            if (m_v2Enum_CheckInType == ContentDetailList.PostGeneral)
                RefreshPostList(null);
            else
                RefreshShopPostList(null);
        }
        //------------------------------------------------------------------------------------
        private void RefreshPostList(GameBerry.Event.RefreshPostListMsg msg)
        {
            if (m_v2Enum_CheckInType != ContentDetailList.PostGeneral)
                return;

            SetPostElement(Managers.PostManager.Instance.GetAllPostInfos(), ContentDetailList.PostGeneral);
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopPostList(GameBerry.Event.RefreshShopPostListMsg msg)
        {
            if (m_v2Enum_CheckInType != ContentDetailList.PostShop)
                return;

            SetPostElement(Managers.ShopPostManager.Instance.GetAllPostInfos(), ContentDetailList.PostShop);
        }
        //------------------------------------------------------------------------------------
        private void SetPostElement(Dictionary<string, PostInfo> postInfos, ContentDetailList contentDetailList)
        {
            if (postInfos == null)
                return;

            List<string> removeList = new List<string>();

            foreach (KeyValuePair<string, UIPostElement> pair in m_uIPostElements_Dic)
            {
                if (postInfos.ContainsKey(pair.Key) == false)
                {
                    removeList.Add(pair.Key);
                }
            }

            for (int i = 0; i < removeList.Count; ++i)
            {
                PoolUIPostElement(removeList[i]);
            }

            foreach (KeyValuePair<string, PostInfo> pair in postInfos)
            {
                UIPostElement uIPostElement = null;

                if (m_uIPostElements_Dic.ContainsKey(pair.Key) == true)
                    continue;

                uIPostElement = GetUIPostElement();
                uIPostElement.gameObject.SetActive(true);
                uIPostElement.gameObject.name = pair.Value.Title;
                uIPostElement.SetPostElement(pair.Value);

                m_uIPostElements_Dic.Add(pair.Key, uIPostElement);
                m_uIPostElements.Add(uIPostElement);

                if (isEnter == false)
                    Managers.RedDotManager.Instance.ShowRedDot(contentDetailList);
                else
                    Managers.RedDotManager.Instance.HideRedDot(contentDetailList);

            }

            m_uIPostElements.Sort((x, y) =>
            {
                if (x.GetPostInfo() == null)
                    return -1;
                if (y.GetPostInfo() == null)
                    return 1;

                if (x.GetPostInfo().SentDate_TimeStamp > y.GetPostInfo().SentDate_TimeStamp)
                    return -1;
                else
                    return 1;

                return 0;
            });

            for (int i = 0; i < m_uIPostElements.Count; ++i)
            {
                m_uIPostElements[i].transform.SetAsLastSibling();
            }

            ChangeWaitState(false);
        }
        //------------------------------------------------------------------------------------
        private UIPostElement GetUIPostElement()
        {
            if (m_uIPostElementPool.Count > 0)
                return m_uIPostElementPool.Dequeue();


            GameObject clone = Instantiate(m_postElement.gameObject, m_scrollRectContent);
            UIPostElement uIPostElement = clone.GetComponent<UIPostElement>();

            if (uIPostElement != null)
            {
                uIPostElement.Init(OnClick_PostDetail);
            }

            return uIPostElement;
        }
        //------------------------------------------------------------------------------------
        public void ChangeWaitState(bool state)
        {
            if (m_postRefreshingDirection != null)
                m_postRefreshingDirection.gameObject.SetActive(state);

            m_waitState = state;
        }
        //------------------------------------------------------------------------------------
        private void PoolUIPostElement(string indata)
        {
            if (m_uIPostElements_Dic.ContainsKey(indata) == false)
                return;

            UIPostElement uIPostElement = m_uIPostElements_Dic[indata];
            if (uIPostElement == null)
                return;

            uIPostElement.ReleaseData();

            m_uIPostElements_Dic.Remove(indata);
            m_uIPostElements.Remove(uIPostElement);
            uIPostElement.gameObject.SetActive(false);
            m_uIPostElementPool.Enqueue(uIPostElement);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PostDetail(string indata)
        {
            if (m_waitState == true)
                return;

            if (m_uIPostElements_Dic.ContainsKey(indata) == false)
                return;

            if (m_uIPostDetailElement != null)
            {
                m_uIPostDetailElement.ElementEnter();
                //m_uIPostDetailElement.gameObject.SetActive(true);

                PostInfo postInfo = null;

                if (m_v2Enum_CheckInType == ContentDetailList.PostGeneral)
                    postInfo = Managers.PostManager.Instance.GetPostInfo(indata);
                else if (m_v2Enum_CheckInType == ContentDetailList.PostShop)
                    postInfo = Managers.ShopPostManager.Instance.GetPostInfo(indata);

                m_uIPostDetailElement.SetPostView(postInfo);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AllRecvAndDeletePost()
        {
            if (m_waitState == true)
                return;

            if (m_uIPostElements.Count <= 0)
                return;

            if (m_v2Enum_CheckInType == ContentDetailList.PostGeneral)
                Managers.PostManager.Instance.ReceiveAllPostItem();
            else if (m_v2Enum_CheckInType == ContentDetailList.PostShop)
                Managers.ShopPostManager.Instance.ReceiveAllPostItem();
        }
        //------------------------------------------------------------------------------------
    }
}