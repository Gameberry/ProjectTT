using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementDiamond : UIShopElement
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement;

        [SerializeField]
        private Image m_packageImage;

        [SerializeField]
        private TMP_Text m_baseAmount;

        [SerializeField]
        private TMP_Text m_bonusAmount;

        [SerializeField]
        private Transform _bonusTag;

        private ShopDiamondChargeData _shopDiamondChargeData;

        public override void SetShopElement(ShopDataBase shopDataBase)
        {
            base.SetShopElement(shopDataBase);

            ShopDiamondChargeData shopDiamondChargeData = shopDataBase as ShopDiamondChargeData;

            _shopDiamondChargeData = shopDiamondChargeData;

            if (m_packageImage != null)
            {
                m_packageImage.gameObject.SetActive(true);
                m_packageImage.sprite = Managers.ShopManager.Instance.GetPackageIcon(shopDiamondChargeData.PackageIconStringKey);
            }

            V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(shopDiamondChargeData.BaseGoodsParam1);

            if (m_uIGlobalGoodsRewardIconElement != null)
                m_uIGlobalGoodsRewardIconElement.SetRewardElement(
                    v2Enum_Goods,
                    shopDiamondChargeData.BaseGoodsParam1,
                    Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods.Enum32ToInt(), shopDiamondChargeData.BaseGoodsParam1),
                    Managers.GoodsManager.Instance.GetGoodsGrade(v2Enum_Goods.Enum32ToInt(), shopDiamondChargeData.BaseGoodsParam1),
                    0.0);

            if (m_baseAmount != null)
                m_baseAmount.text = string.Format("{0:#,###}", shopDiamondChargeData.BaseGoodsParam2);

            if (m_bonusAmount != null)
                m_bonusAmount.text = string.Format("{0:#,###}", shopDiamondChargeData.BonusGoodsParam2);

            OnRefresh();
        }
        //------------------------------------------------------------------------------------
        protected override void OnRefresh()
        {
            if (_shopDiamondChargeData == null)
                return;

            bool canBonus = Managers.ShopManager.Instance.CanBonus(_shopDiamondChargeData);


            if (m_bonusAmount != null)
                m_bonusAmount.gameObject.SetActive(canBonus);

            if (_bonusTag != null)
                _bonusTag.gameObject.SetActive(canBonus);
        }
        //------------------------------------------------------------------------------------
    }
}