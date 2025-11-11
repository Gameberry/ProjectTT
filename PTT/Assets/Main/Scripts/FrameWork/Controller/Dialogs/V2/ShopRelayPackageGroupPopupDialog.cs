using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopRelayPackageGroupPopupDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [Header("------------ElementGroup------------")]
        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private float m_tempSnapOffSet = 420.0f;

        [SerializeField]
        private RectTransform m_elementRoot;

        [SerializeField]
        private UIShopElementRelay m_element;

        [SerializeField]
        private Transform m_prevPackage;

        [SerializeField]
        private TMP_Text m_prevPackage_Name;

        [SerializeField]
        private Transform m_nextPackage;

        [SerializeField]
        private TMP_Text m_nextPackage_Name;

        private List<UIShopElementRelay> m_element_List = new List<UIShopElementRelay>();

        private UIShopElementRelay m_focusElement = null;

        private ShopPackageRelayData m_directionFocus = null;
        private ShopPackageRelayGroupData m_currentData = null;
        private float m_scrollPosGab = 1.0f;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }

            if (m_elementScrollRect != null)
                m_elementScrollRect.onValueChanged.AddListener(OnValueChange_ScrollNormalPos);

            Message.AddListener<GameBerry.Event.SetRelayPackageGroupPopupMsg>(SetRelayPackageGroupPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetRelayPackageGroupPopupMsg>(SetRelayPackageGroupPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (m_elementScrollRect != null)
            {
                m_elementScrollRect.SetLayoutHorizontal();
                m_elementScrollRect.normalizedPosition = Vector2.zero;
            }

            ScrollViewSnapToItem();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            RequestDialogExit<ShopRelayPackageGroupPopupDialog>();
        }
        //------------------------------------------------------------------------------------
        private void ScrollViewSnapToItem()
        {
            if (m_focusElement == null)
                return;

            RectTransform rectTransform = null;
            if (m_focusElement.TryGetComponent(out rectTransform))
            {
                Vector2 offset = Vector2.zero;
                offset.x = m_tempSnapOffSet;

                Util.ScrollViewSnapToItem(m_elementScrollRect, m_elementRoot, rectTransform, offset);
                //Util.ScrollViewSnapToItem(m_fameElementScrollRect, m_fameElementRoot, rectTransform);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetRelayPackageGroupPopup(GameBerry.Event.SetRelayPackageGroupPopupMsg msg)
        {
            m_currentData = msg.RefreshData;

            SetFocusElement(msg.RefreshData.FocusShopPackageData);
            for (int i = 0; i < msg.RefreshData.ShopPackageRelayDatas.Count; ++i)
            {
                ShopPackageRelayData shopPackageRelayData = msg.RefreshData.ShopPackageRelayDatas[i];

                UIShopElementRelay uIShopElementRelay = null;

                if (m_element_List.Count > i)
                {
                    uIShopElementRelay = m_element_List[i];
                }
                else
                {
                    GameObject clone = Instantiate(m_element.gameObject, m_elementRoot);

                    UIShopElementRelay uIShopElement = clone.GetComponent<UIShopElementRelay>();
                    uIShopElement.Init();
                    uIShopElement.IsShowGroupBtn(false);
                    m_element_List.Add(uIShopElement);

                    uIShopElementRelay = uIShopElement;
                }

                uIShopElementRelay.SetShopElement(shopPackageRelayData);

                if (shopPackageRelayData == msg.RefreshData.FocusShopPackageData)
                    m_focusElement = uIShopElementRelay;

                uIShopElementRelay.gameObject.SetActive(true);
            }

            for (int i = msg.RefreshData.ShopPackageRelayDatas.Count; i < m_element_List.Count; ++i)
            {
                m_element_List[i].gameObject.SetActive(false);
            }


            m_scrollPosGab = 1.0f / (float)msg.RefreshData.ShopPackageRelayDatas.Count;
        }
        //------------------------------------------------------------------------------------
        private void SetFocusElement(ShopPackageRelayData focus)
        {
            if (focus == m_directionFocus)
                return;

            m_directionFocus = focus;

            if (m_currentData == null)
                return;

            if (m_currentData.ShopPackageRelayDatas.Count <= 1)
            {
                if (m_prevPackage != null)
                    m_prevPackage.gameObject.SetActive(false);

                if (m_nextPackage != null)
                    m_nextPackage.gameObject.SetActive(false);

                return;
            }

            int focusIdx = m_currentData.ShopPackageRelayDatas.IndexOf(m_directionFocus);

            if (focusIdx == -1)
            {
                if (m_prevPackage != null)
                    m_prevPackage.gameObject.SetActive(false);

                if (m_nextPackage != null)
                    m_nextPackage.gameObject.SetActive(false);

                return;
            }

            if (focusIdx == 0)
            {
                ShopPackageRelayData nextPackage = m_currentData.ShopPackageRelayDatas[focusIdx + 1];

                if (m_prevPackage != null)
                    m_prevPackage.gameObject.SetActive(false);


                if (m_nextPackage != null)
                    m_nextPackage.gameObject.SetActive(true);

                if (m_nextPackage_Name != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_nextPackage_Name, nextPackage.TitleLocalStringKey);
            }
            else if (m_currentData.ShopPackageRelayDatas.Count - 1 == focusIdx)
            {
                ShopPackageRelayData prevPackage = m_currentData.ShopPackageRelayDatas[focusIdx - 1];

                if (m_prevPackage != null)
                    m_prevPackage.gameObject.SetActive(true);

                if (m_prevPackage_Name != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_prevPackage_Name, prevPackage.TitleLocalStringKey);


                if (m_nextPackage != null)
                    m_nextPackage.gameObject.SetActive(false);
            }
            else
            {
                ShopPackageRelayData prevPackage = m_currentData.ShopPackageRelayDatas[focusIdx - 1];
                ShopPackageRelayData nextPackage = m_currentData.ShopPackageRelayDatas[focusIdx + 1];

                if (m_prevPackage != null)
                    m_prevPackage.gameObject.SetActive(true);

                if (m_prevPackage_Name != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_prevPackage_Name, prevPackage.TitleLocalStringKey);

                if (m_nextPackage != null)
                    m_nextPackage.gameObject.SetActive(true);

                if (m_nextPackage_Name != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_nextPackage_Name, nextPackage.TitleLocalStringKey);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_ScrollNormalPos(Vector2 pos)
        {
            if (m_currentData == null)
                return;

            if (m_currentData.ShopPackageRelayDatas.Count <= 1)
                return;

            for (int i = 0; i < m_currentData.ShopPackageRelayDatas.Count; ++i)
            {
                if (m_scrollPosGab * (i + 1) > pos.x)
                {
                    SetFocusElement(m_currentData.ShopPackageRelayDatas[i]);
                    //Debug.Log(Managers.LocalStringManager.Instance.GetLocalString(m_currentData.ShopPackageRelayDatas[i].TitleLocalStringKey));
                    break;
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}