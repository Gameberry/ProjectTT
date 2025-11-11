using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementIngameStore : UIShopElement
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement;

        [SerializeField]
        private Image m_packageImage;

        [SerializeField]
        private TMP_Text _returnGoodsamount;

        [SerializeField]
        private Image _costGoodsIcon;

        [SerializeField]
        private TMP_Text _costGoodsamount;


        [SerializeField]
        protected CButton _cBuyBtn;

        private ShopIngameStoreData _shopIngameStoreData;

        public override void SetShopElement(ShopDataBase shopDataBase)
        {
            base.SetShopElement(shopDataBase);

            ShopIngameStoreData shopDiamondChargeData = shopDataBase as ShopIngameStoreData;
            _shopIngameStoreData = shopDiamondChargeData;

            if (m_uIGlobalGoodsRewardIconElement != null)
                m_uIGlobalGoodsRewardIconElement.SetRewardElement(shopDiamondChargeData.ReturnGoods);

            if (m_packageImage != null)
            {
                m_packageImage.gameObject.SetActive(true);
                m_packageImage.sprite = Managers.ShopManager.Instance.GetPackageIcon(shopDiamondChargeData.PackageIconStringKey);
            }

            if (_returnGoodsamount != null)
                _returnGoodsamount.SetText(string.Format("{0:N0}", shopDiamondChargeData.ReturnGoods.Amount));

            if (_costGoodsamount != null)
                _costGoodsamount.SetText(string.Format("{0:N0}", shopDiamondChargeData.CostGoods.Amount));

            if (_costGoodsIcon != null)
                _costGoodsIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(shopDiamondChargeData.CostGoods.Index);

            RefreshGoodsAmount();
        }
        //------------------------------------------------------------------------------------
        protected override void OnClick_BuyBtn()
        {
            if (_shopIngameStoreData != null)
                Managers.ShopManager.Instance.SetShopIngameStore(_shopIngameStoreData);
        }
        //------------------------------------------------------------------------------------
        public void RefreshGoodsAmount()
        {
            if (_shopIngameStoreData == null)
                return;

            RewardData cost = _shopIngameStoreData.CostGoods;

            bool isReady = Managers.GoodsManager.Instance.GetGoodsAmount(cost.Index) >= cost.Amount;

            if (_cBuyBtn != null)
                _cBuyBtn.SetInteractable(isReady);
        }
        //------------------------------------------------------------------------------------
    }
}