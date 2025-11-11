using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementRandomStore_Free : MonoBehaviour
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement;

        [SerializeField]
        private TMP_Text _title;

        [SerializeField]
        private Transform _soldOut;

        [SerializeField]
        protected CButton _cBuyBtn;

        [SerializeField]
        private Transform _freeRedDot;

        [SerializeField]
        private Transform _freeTrans;

        [SerializeField]
        private Transform _adTrans;

        [SerializeField]
        private TMP_Text _adRemainCount;

        [SerializeField]
        private Transform _adIcon;

        [SerializeField]
        private V2Enum_ShopMenuType _ShopMenuType = V2Enum_ShopMenuType.Descend;

        [SerializeField]
        private bool _autoSetting = false;

        private ShopFreeGoodsData _shopFreeGoodsData = null;

        bool addedMessage = false;

        private void Awake()
        {
            if (_cBuyBtn != null)
                _cBuyBtn.onClick.AddListener(OnClick_BuyBtn);

            
            if (_autoSetting == true)
            {
                _shopFreeGoodsData = Managers.ShopRandomStoreManager.Instance.GetShopFreeGoodsData(_ShopMenuType);
                SetFreeData(_shopFreeGoodsData);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetFreeData(ShopFreeGoodsData shopFreeGoodsData)
        {
            _shopFreeGoodsData = shopFreeGoodsData;
            if (_shopFreeGoodsData != null && addedMessage == false)
            { 
                Message.AddListener(Managers.ShopManager.Instance.GetRefreshMessageID(_shopFreeGoodsData.Index), RefreshFree);
                addedMessage = true;
            }

            RefreshFree();
        }
        //------------------------------------------------------------------------------------
        public void RefreshFree()
        {
            if (_shopFreeGoodsData == null)
                return;

            RewardData freedia = _shopFreeGoodsData.ReturnGoods;

            if (m_uIGlobalGoodsRewardIconElement != null)
                m_uIGlobalGoodsRewardIconElement.SetRewardElement(freedia);


            if (_title != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(_title, Managers.GoodsManager.Instance.GetGoodsLocalKey(freedia.Index));
            }


            bool soldOut = false;

            if (Managers.ShopRandomStoreManager.Instance.CanRecvFreeDia(_shopFreeGoodsData) == true)
            {
                if (_freeRedDot != null)
                    _freeRedDot.gameObject.SetActive(true);

                if (_freeTrans != null)
                    _freeTrans.gameObject.SetActive(true);

                if (_adTrans != null)
                    _adTrans.gameObject.SetActive(false);
            }
            else
            {
                if (_freeRedDot != null)
                    _freeRedDot.gameObject.SetActive(false);

                if (_freeTrans != null)
                    _freeTrans.gameObject.SetActive(false);

                if (_adTrans != null)
                    _adTrans.gameObject.SetActive(true);

                if (_adRemainCount != null)
                    _adRemainCount.SetText("({0}/{1})", Managers.ShopRandomStoreManager.Instance.RemainFreeDiaCount_AdView(_shopFreeGoodsData), Define.RandomShopFreeDiaAD);

                soldOut = Managers.ShopRandomStoreManager.Instance.RemainFreeDiaCount_AdView(_shopFreeGoodsData) <= 0;

                if (_adIcon != null)
                    _adIcon.gameObject.SetActive(soldOut == false);
            }

            if (_soldOut != null)
                _soldOut.gameObject.SetActive(soldOut);

            if (_cBuyBtn != null)
                _cBuyBtn.SetInteractable(soldOut == false);
        }
        //------------------------------------------------------------------------------------
        protected void OnClick_BuyBtn()
        {
            if (_shopFreeGoodsData != null)
                Managers.ShopRandomStoreManager.Instance.RecvFreeDia(_shopFreeGoodsData);
        }
        //------------------------------------------------------------------------------------
    }
}