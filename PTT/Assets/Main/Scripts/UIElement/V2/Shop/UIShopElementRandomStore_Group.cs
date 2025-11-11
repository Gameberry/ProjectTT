using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class UIShopElementRandomStore_Group : UIShopElement_Group
    {
        [SerializeField]
        private CButton _refresh_AdBtn;

        [SerializeField]
        private TMP_Text _refresh_Ad_RemainCount;

        [SerializeField]
        private CButton _refresh_DiaBtn;

        [SerializeField]
        private TMP_Text _refresh_Dia_RemainCount;

        [SerializeField]
        private TMP_Text _refresh_Dia_Price;

        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIShopElementRandomStore_Free m_uIShopElement_Free;

        [SerializeField]
        private UIShopElementRandomStore m_uIShopElement;

        private List<UIShopElementRandomStore> uIShopElementIngameStores = new List<UIShopElementRandomStore>();

        private List<int> _goodsListener = new List<int>();

        [SerializeField]
        private TMP_Text _initTime;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_refresh_AdBtn != null)
                _refresh_AdBtn.onClick.AddListener(OnClick_RefreshAd);

            if (_refresh_DiaBtn != null)
                _refresh_DiaBtn.onClick.AddListener(OnClick_RefreshDia);

            Message.AddListener<GameBerry.Event.RefreshShopRandomStoreMsg>(RefreshShopRandomStore);
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            Message.RemoveListener<GameBerry.Event.RefreshShopRandomStoreMsg>(RefreshShopRandomStore);
        }
        //------------------------------------------------------------------------------------
        public override void SetShopElement()
        {
            Managers.TimeManager.Instance.RemainInitDailyContent_Text += SetInitInterval;

            List<ShopFreeGoodsData> shopDiamondChargeDatas = Managers.ShopRandomStoreManager.Instance.GetShopFreeGoodsDatas();

            for (int i = 0; i < shopDiamondChargeDatas.Count; ++i)
            {
                GameObject clone = Instantiate(m_uIShopElement_Free.gameObject, m_elementRoot);

                UIShopElementRandomStore_Free uIShopElement = clone.GetComponent<UIShopElementRandomStore_Free>();
                uIShopElement.gameObject.SetActive(true);
                uIShopElement.SetFreeData(shopDiamondChargeDatas[i]);
            }

            elementCount = shopDiamondChargeDatas.Count;

            SetLayoutElementSize();

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.Dia.Enum32ToInt(), RefreshDia);
            RefreshRandomStore();
            RefreshBtnState();
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopRandomStore(GameBerry.Event.RefreshShopRandomStoreMsg msg)
        {
            RefreshRandomStore();
            RefreshBtnState();
        }
        //------------------------------------------------------------------------------------
        private void RefreshRandomStore()
        {
            //if (m_uIShopElement_Free != null)
            //    m_uIShopElement_Free.RefreshFree();

            return;

            List<ObscuredInt> shopDiamondChargeDatas = Managers.ShopRandomStoreManager.Instance.GetDisPlayIndexs();

            for (int i = 0; i < _goodsListener.Count; ++i)
            {
                V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(_goodsListener[i]);
                if (v2Enum_Goods != V2Enum_Goods.Max)
                    Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(v2Enum_Goods, _goodsListener[i], RefreshElement);
            }

            _goodsListener.Clear();

            for (int i = 0; i < shopDiamondChargeDatas.Count; ++i)
            {
                ShopRandomStoreData shopRandomStoreData = Managers.ShopRandomStoreManager.Instance.GetShopRandomStoreData(shopDiamondChargeDatas[i]);
                if (shopRandomStoreData == null)
                    continue;

                UIShopElementRandomStore uIShopElement = null;

                if (uIShopElementIngameStores.Count > i)
                {
                    uIShopElement = uIShopElementIngameStores[i];
                }
                else
                {
                    GameObject clone = Instantiate(m_uIShopElement.gameObject, m_elementRoot);
                    uIShopElement = clone.GetComponent<UIShopElementRandomStore>();
                    uIShopElementIngameStores.Add(uIShopElement);
                }

                uIShopElement.SetShopElement(shopRandomStoreData);
                uIShopElement.gameObject.SetActive(true);
                if (_goodsListener.Contains(shopRandomStoreData.CostGoods.Index) == false)
                    _goodsListener.Add(shopRandomStoreData.CostGoods.Index);
            }


            for (int i = shopDiamondChargeDatas.Count; i < uIShopElementIngameStores.Count; ++i)
            {
                uIShopElementIngameStores[i].gameObject.SetActive(false);
            }

            elementCount = shopDiamondChargeDatas.Count;

            SetLayoutElementSize();

            for (int i = 0; i < _goodsListener.Count; ++i)
            {
                V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(_goodsListener[i]);
                if (v2Enum_Goods != V2Enum_Goods.Max)
                    Managers.GoodsManager.Instance.AddGoodsRefreshEvent(v2Enum_Goods, _goodsListener[i], RefreshElement);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshBtnState()
        {
            int remainAd = Managers.ShopRandomStoreManager.Instance.RemainResetStoreCount_AdView();

            if (_refresh_AdBtn != null)
                _refresh_AdBtn.SetInteractable(remainAd > 0);

            if (_refresh_Ad_RemainCount != null)
                _refresh_Ad_RemainCount.SetText("({0}/{1})", remainAd, Define.RandomShopAdRefreshCount);


            int remainDia = Managers.ShopRandomStoreManager.Instance.RemainResetStoreCount_Dia();

            double dia = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.Dia.Enum32ToInt());

            bool ready = dia >= Define.RandomShopRefreshValue;

            if (ready == true)
            {
                ready = remainDia > 0;
            }

            if (_refresh_DiaBtn != null)
                _refresh_DiaBtn.SetInteractable(ready);

            if (_refresh_Dia_RemainCount != null)
                _refresh_Dia_RemainCount.SetText("({0}/{1})", remainDia, Define.RandomShopRefreshCount);

            if (_refresh_Dia_Price != null)
                _refresh_Dia_Price.SetText(string.Format("{0:N0}", Define.RandomShopRefreshValue));
        }
        //------------------------------------------------------------------------------------
        private void RefreshElement(double amount)
        {
            for (int i = 0; i < uIShopElementIngameStores.Count; ++i)
            {
                uIShopElementIngameStores[i].RefreshGoodsAmount();
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshDia(double amount)
        {
            RefreshBtnState();
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval(string remaintime)
        {
            if (_initTime != null)
                _initTime.SetText(remaintime);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RefreshAd()
        {
            Managers.ShopRandomStoreManager.Instance.ResetStore_AdView();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RefreshDia()
        {
            Managers.ShopRandomStoreManager.Instance.ResetStore_Dia();
        }
        //------------------------------------------------------------------------------------
    }
}