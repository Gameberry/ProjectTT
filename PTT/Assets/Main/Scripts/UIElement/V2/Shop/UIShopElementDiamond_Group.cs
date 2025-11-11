using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementDiamond_Group : UIShopElement_Group
    {
        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIShopElement m_uIShopElement;

        private Dictionary<ShopDiamondChargeData, UIShopElement> _elements = new Dictionary<ShopDiamondChargeData, UIShopElement>();

        //------------------------------------------------------------------------------------
        public override void SetShopElement()
        {
            List<ShopDiamondChargeData> shopDiamondChargeDatas = Managers.ShopManager.Instance.GetShopDiamondChargeDatas();

            for (int i = 0; i < shopDiamondChargeDatas.Count; ++i)
            {
                GameObject clone = Instantiate(m_uIShopElement.gameObject, m_elementRoot);

                UIShopElement uIShopElement = clone.GetComponent<UIShopElement>();
                uIShopElement.Init();
                uIShopElement.SetShopElement(shopDiamondChargeDatas[i]);

                _elements.Add(shopDiamondChargeDatas[i], uIShopElement);
            }

            elementCount = shopDiamondChargeDatas.Count;

            SetLayoutElementSize();

            Managers.TimeManager.Instance.OnInitMonthContent += OnInitMonthlyContent;
        }
        //------------------------------------------------------------------------------------
        public void OnInitMonthlyContent(double nextinittimestamp)
        {
            RefreshElement();
        }
        //------------------------------------------------------------------------------------
        private void RefreshElement()
        {
            foreach (var pair in _elements)
            {
                pair.Value.Refresh();
            }
        }
        //------------------------------------------------------------------------------------
    }
}
