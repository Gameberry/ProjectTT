using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementIngameStore_Group : UIShopElement_Group
    {
        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIShopElementIngameStore m_uIShopElement;

        [SerializeField]
        private V2Enum_ShopMenuType _ShopMenuType = V2Enum_ShopMenuType.Descend;

        private List<UIShopElementIngameStore> uIShopElementIngameStores = new List<UIShopElementIngameStore>();

        //------------------------------------------------------------------------------------
        public override void SetShopElement()
        {
            List<ShopIngameStoreData> shopDiamondChargeDatas = Managers.ShopManager.Instance.GetShopIngameStoreDatas();

            shopDiamondChargeDatas = shopDiamondChargeDatas.FindAll(x => x.MenuType == _ShopMenuType);

            List<int> listener = new List<int>();

            for (int i = 0; i < shopDiamondChargeDatas.Count; ++i)
            {
                GameObject clone = Instantiate(m_uIShopElement.gameObject, m_elementRoot);

                UIShopElementIngameStore uIShopElement = clone.GetComponent<UIShopElementIngameStore>();
                uIShopElement.Init();
                uIShopElement.SetShopElement(shopDiamondChargeDatas[i]);

                if (listener.Contains(shopDiamondChargeDatas[i].CostGoods.Index) == false)
                    listener.Add(shopDiamondChargeDatas[i].CostGoods.Index);

                uIShopElementIngameStores.Add(uIShopElement);
            }

            elementCount = shopDiamondChargeDatas.Count;

            SetLayoutElementSize();

            for (int i = 0; i < listener.Count; ++i)
            {
                V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(listener[i]);
                if(v2Enum_Goods != V2Enum_Goods.Max)
                    Managers.GoodsManager.Instance.AddGoodsRefreshEvent(v2Enum_Goods, listener[i], RefreshDia);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshDia(double amount)
        {
            for (int i = 0; i < uIShopElementIngameStores.Count; ++i)
            {
                uIShopElementIngameStores[i].RefreshGoodsAmount();
            }
        }
        //------------------------------------------------------------------------------------
    }
}