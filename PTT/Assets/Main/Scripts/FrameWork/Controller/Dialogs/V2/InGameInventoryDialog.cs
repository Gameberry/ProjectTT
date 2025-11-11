using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class InGameInventoryDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;


        [Header("------------MasteryTab------------")]
        [SerializeField]
        private Button m_itemBtn;

        [SerializeField]
        private TMP_Text m_item_Text;


        [SerializeField]
        private Button m_pointBtn;

        [SerializeField]
        private TMP_Text m_point_Text;


        [SerializeField]
        private Sprite m_selectTabBG;

        [SerializeField]
        private Sprite m_noneTabBG;


        [SerializeField]
        private Color m_selectTab_Text;

        [SerializeField]
        private Color m_noneTab_Text;

        [Header("------------Element------------")]

        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private RectTransform m_scrollRectContent;

        [SerializeField]
        private RectTransform m_uIInventoryElementRoot;

        [SerializeField]
        private UIInventoryElement m_uIInventoryElement;

        private List<UIInventoryElement> uIInventoryElements = new List<UIInventoryElement>();

        private ContentDetailList m_v2Enum_InventoryType = ContentDetailList.None;

        [Header("------------UIInventoryUseItemElement------------")]

        [SerializeField]
        private UIInventoryUseItemElement m_uIInventoryUseItemElement;

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
                            RequestDialogExit<InGameInventoryDialog>();
                        });
                }
            }

            if (m_itemBtn != null)
                m_itemBtn.onClick.AddListener(OnClick_OnceTab);

            if (m_pointBtn != null)
                m_pointBtn.onClick.AddListener(OnClick_RepeatTab);


            if (m_uIInventoryUseItemElement != null)
            {
                m_uIInventoryUseItemElement.Load_Element();
            }

            SetCheckInElement(ContentDetailList.Inventory_Point);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {

        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (m_v2Enum_InventoryType == ContentDetailList.Inventory_Item)
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.Inventory_Item);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.RedDotManager.isAlive == false)
                return;

            if (m_v2Enum_InventoryType == ContentDetailList.Inventory_Item)
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.Inventory_Item);
        }
        //------------------------------------------------------------------------------------
        private void SetCheckInElement(ContentDetailList v2Enum_CheckInType)
        {
            if (m_v2Enum_InventoryType == v2Enum_CheckInType)
                return;

            m_v2Enum_InventoryType = v2Enum_CheckInType;

            int datatotalCount = 0;

            if (v2Enum_CheckInType == ContentDetailList.Inventory_Point)
            {
                List<PointData> checkInRewardDatas = Managers.PointDataManager.Instance.GetAllPointData();

                datatotalCount = checkInRewardDatas.Count;

                for (int i = 0; i < checkInRewardDatas.Count; ++i)
                {
                    UIInventoryElement uIInventoryElement = null;

                    if (uIInventoryElements.Count <= i)
                    {
                        GameObject clone = Instantiate(m_uIInventoryElement.gameObject, m_uIInventoryElementRoot);
                        if (clone == null)
                            break;

                        uIInventoryElement = clone.GetComponent<UIInventoryElement>();
                        if (uIInventoryElement == null)
                            break;

                        uIInventoryElement.Init(OnClick_ItemElement);
                        uIInventoryElements.Add(uIInventoryElement);
                    }
                    else
                    {
                        uIInventoryElement = uIInventoryElements[i];
                    }

                    if (uIInventoryElement == null)
                        continue;

                    uIInventoryElement.SetInvenToryIcon(V2Enum_Goods.Point, checkInRewardDatas[i].Index);

                    if (checkInRewardDatas[i].IsShow != 1)
                        uIInventoryElement.gameObject.SetActive(false);
                }
            }
            else
            {
                Dictionary<ObscuredInt, BoxData> boxDatas = Managers.BoxManager.Instance.GetAllBoxData();
                datatotalCount += boxDatas.Count;
                int i = 0;

                foreach (var pair in boxDatas)
                {
                    UIInventoryElement uIInventoryElement = null;

                    if (uIInventoryElements.Count <= i)
                    {
                        GameObject clone = Instantiate(m_uIInventoryElement.gameObject, m_uIInventoryElementRoot);
                        if (clone == null)
                            break;

                        uIInventoryElement = clone.GetComponent<UIInventoryElement>();
                        if (uIInventoryElement == null)
                            break;

                        uIInventoryElement.Init(OnClick_ItemElement);
                        uIInventoryElements.Add(uIInventoryElement);
                    }
                    else
                    {
                        uIInventoryElement = uIInventoryElements[i];
                    }

                    if (uIInventoryElement == null)
                        continue;

                    uIInventoryElement.SetInvenToryIcon(V2Enum_Goods.Box, pair.Value.Index);

                    i++;
                }


                Dictionary<ObscuredInt, SummonTicketData> summonTicketDatas = Managers.SummonTicketManager.Instance.GetAllBoxData();
                datatotalCount += summonTicketDatas.Count;

                foreach (var pair in summonTicketDatas)
                {
                    UIInventoryElement uIInventoryElement = null;

                    if (uIInventoryElements.Count <= i)
                    {
                        GameObject clone = Instantiate(m_uIInventoryElement.gameObject, m_uIInventoryElementRoot);
                        if (clone == null)
                            break;

                        uIInventoryElement = clone.GetComponent<UIInventoryElement>();
                        if (uIInventoryElement == null)
                            break;

                        uIInventoryElement.Init(OnClick_ItemElement);
                        uIInventoryElements.Add(uIInventoryElement);
                    }
                    else
                    {
                        uIInventoryElement = uIInventoryElements[i];
                    }

                    if (uIInventoryElement == null)
                        continue;

                    uIInventoryElement.SetInvenToryIcon(V2Enum_Goods.SummonTicket, pair.Value.Index);

                    i++;
                }
            }

            for (int i = datatotalCount; i < uIInventoryElements.Count; ++i)
            {
                uIInventoryElements[i].SetInvenToryIcon(V2Enum_Goods.Max, -1);
            }

            int elementGroupFrameSibling = m_elementScrollRect.transform.GetSiblingIndex();

            if (m_itemBtn != null)
            {
                m_itemBtn.image.sprite = m_v2Enum_InventoryType == ContentDetailList.Inventory_Point ? m_noneTabBG : m_selectTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_InventoryType == ContentDetailList.Inventory_Point ? -1 : 1);
                m_itemBtn.transform.SetSiblingIndex(newsibling);
            }

            elementGroupFrameSibling = m_elementScrollRect.transform.GetSiblingIndex();

            if (m_pointBtn != null)
            {
                m_pointBtn.image.sprite = m_v2Enum_InventoryType == ContentDetailList.Inventory_Point ? m_selectTabBG : m_noneTabBG;
                int newsibling = elementGroupFrameSibling + (m_v2Enum_InventoryType == ContentDetailList.Inventory_Point ? 1 : -1);
                m_pointBtn.transform.SetSiblingIndex(newsibling);
            }

            if (m_item_Text != null)
                m_item_Text.color = m_v2Enum_InventoryType == ContentDetailList.Inventory_Point ? m_noneTab_Text : m_selectTab_Text;

            if (m_point_Text != null)
                m_point_Text.color = m_v2Enum_InventoryType == ContentDetailList.Inventory_Point ? m_selectTab_Text : m_noneTab_Text;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_OnceTab()
        {
            SetCheckInElement(ContentDetailList.Inventory_Item);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RepeatTab()
        {
            SetCheckInElement(ContentDetailList.Inventory_Point);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ItemElement(V2Enum_Goods v2Enum_Goods, int index)
        {
            double amount = Managers.GoodsManager.Instance.GetGoodsAmount(v2Enum_Goods.Enum32ToInt(), index);
            if (amount < 1.0)
                return;

            if (m_uIInventoryUseItemElement != null)
            {
                m_uIInventoryUseItemElement.SetGoods(v2Enum_Goods, index);
                m_uIInventoryUseItemElement.ElementEnter();
            }
        }
        //------------------------------------------------------------------------------------
    }
}