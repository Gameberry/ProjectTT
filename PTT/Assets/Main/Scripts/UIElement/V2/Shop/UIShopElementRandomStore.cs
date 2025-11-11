using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementRandomStore : MonoBehaviour
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement;

        [SerializeField]
        private TMP_Text _title;

        [SerializeField]
        private Image _costGoodsIcon;

        [SerializeField]
        private TMP_Text _costGoodsamount;

        [SerializeField]
        private Transform _soldOut;

        [SerializeField]
        private Transform _discountGroup;

        [SerializeField]
        private TMP_Text _discountText;

        [SerializeField]
        protected CButton _cBuyBtn;

        private ShopRandomStoreData _shopRandomStoreData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_cBuyBtn != null)
                _cBuyBtn.onClick.AddListener(OnClick_BuyBtn);
        }
        //------------------------------------------------------------------------------------
        public void SetShopElement(ShopRandomStoreData shopRandomStoreData)
        {
            if (shopRandomStoreData == null)
                return;

            _shopRandomStoreData = shopRandomStoreData;

            if (_title != null)
            { 
                Managers.LocalStringManager.Instance.SetLocalizeText(_title, Managers.GoodsManager.Instance.GetGoodsLocalKey(shopRandomStoreData.ReturnGoods.Index));
            }


            if (m_uIGlobalGoodsRewardIconElement != null)
                m_uIGlobalGoodsRewardIconElement.SetRewardElement(shopRandomStoreData.ReturnGoods);

            if (_costGoodsamount != null)
                _costGoodsamount.SetText(string.Format("{0:0}", shopRandomStoreData.CostGoods.Amount));

            if (_costGoodsIcon != null)
                _costGoodsIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(shopRandomStoreData.CostGoods.Index);

            if (_soldOut != null)
                _soldOut.gameObject.SetActive(Managers.ShopRandomStoreManager.Instance.IsSoldOut(_shopRandomStoreData));

            if (_discountGroup != null)
            {
                if (shopRandomStoreData.DiscountPercent > 0)
                {
                    _discountGroup.gameObject.SetActive(true);
                    if (_discountText != null)
                        _discountText.SetText("{0:0}%", shopRandomStoreData.DiscountPercent);
                }
                else
                {
                    _discountGroup.gameObject.SetActive(false);
                }
            }

            RefreshGoodsAmount();
        }
        //------------------------------------------------------------------------------------
        protected void OnClick_BuyBtn()
        {
            if (_shopRandomStoreData != null)
            {
                if (Managers.ShopRandomStoreManager.Instance.RecvRandomStore(_shopRandomStoreData) == true)
                {
                    SetShopElement(_shopRandomStoreData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshGoodsAmount()
        {
            if (_shopRandomStoreData == null)
                return;

            RewardData cost = _shopRandomStoreData.CostGoods;

            bool isReady = Managers.GoodsManager.Instance.GetGoodsAmount(cost.Index) >= cost.Amount;

            if (_cBuyBtn != null)
                _cBuyBtn.SetInteractable(isReady);
        }
        //------------------------------------------------------------------------------------
    }
}