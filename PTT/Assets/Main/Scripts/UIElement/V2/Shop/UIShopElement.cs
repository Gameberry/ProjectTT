using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElement : MonoBehaviour
    {
        [SerializeField]
        private UIShopTagElement m_uIShopTagElement;

        [SerializeField]
        private TMP_Text m_uIShopTitle;

        [SerializeField]
        private TMP_Text m_uIShopSubTitle;

        [SerializeField]
        protected TMP_Text m_uIShopPrice;

        [SerializeField]
        protected Button m_buyBtn;

        [SerializeField]
        private Transform m_soldOutTrans;

        [SerializeField]
        private Transform m_elementBuyLight;

        [SerializeField]
        private TMP_Text m_intervalType;

        [SerializeField]
        private TMP_Text m_intervalLimit;

        [SerializeField]
        protected Transform _tagGroup;

        [SerializeField]
        private TMP_Text _tagText;

        protected ShopDataBase m_shopDataBase = null;

        //------------------------------------------------------------------------------------
        public virtual void Init()
        {
            if (m_buyBtn != null)
                m_buyBtn.onClick.AddListener(OnClick_BuyBtn);

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        public virtual void SetShopElement(ShopDataBase shopDataBase)
        {
            if (m_shopDataBase != null)
            {
                Message.RemoveListener(Managers.ShopManager.Instance.GetRefreshMessageID(m_shopDataBase), Refresh);
            }

            m_shopDataBase = shopDataBase;

            if (m_uIShopTagElement != null)
                m_uIShopTagElement.SetTag(shopDataBase.TagString);

            if (m_uIShopTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_uIShopTitle, shopDataBase.TitleLocalStringKey);

            if (m_uIShopSubTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_uIShopSubTitle, shopDataBase.SubTitleLocalStringKey);

            if (m_uIShopPrice != null)
                m_uIShopPrice.text = Managers.ShopManager.Instance.GetPriceText(shopDataBase);

            SetWorth();
            SetIntervalLimit();
            VisibleSoldOut(Managers.ShopManager.Instance.IsSoldOut(shopDataBase));

            Message.AddListener(Managers.ShopManager.Instance.GetRefreshMessageID(shopDataBase), Refresh);
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            if (Managers.ShopManager.isAlive == true)
                Message.RemoveListener(Managers.ShopManager.Instance.GetRefreshMessageID(m_shopDataBase), Refresh);

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString -= RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        protected virtual void RefreshLocalize()
        {
            if (m_shopDataBase != null
                && ShopContainer.m_freeProductData != null)
            {
                if (ShopContainer.m_freeProductData.Contains(m_shopDataBase) == true
                    || Managers.ShopManager.Instance.IsAD(m_shopDataBase) == true)
                {
                    if (m_uIShopPrice != null)
                        m_uIShopPrice.text = Managers.ShopManager.Instance.GetPriceText(m_shopDataBase);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void Refresh()
        {
            SetWorth();
            SetIntervalLimit();
            VisibleSoldOut(Managers.ShopManager.Instance.IsSoldOut(m_shopDataBase));

            if (m_uIShopPrice != null)
                m_uIShopPrice.text = Managers.ShopManager.Instance.GetPriceText(m_shopDataBase);

            OnRefresh();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnRefresh()
        { 

        }
        //------------------------------------------------------------------------------------
        protected void UnLinkMessage()
        {
            
        }
        //------------------------------------------------------------------------------------
        public virtual void ReleaseElement()
        {
            if (m_shopDataBase != null)
            {
                if (m_shopDataBase != null)
                {
                    Message.RemoveListener(Managers.ShopManager.Instance.GetRefreshMessageID(m_shopDataBase), Refresh);
                }
                m_shopDataBase = null;
            }
        }
        //------------------------------------------------------------------------------------
        protected void SetWorth()
        {
            if (m_shopDataBase == null)
                return;

            if(_tagGroup != null && _tagText != null)
            {
                if (m_shopDataBase.TagString == "-1")
                    _tagGroup.gameObject.SetActive(false);
                else
                {
                    _tagGroup.gameObject.SetActive(true);
                    Managers.LocalStringManager.Instance.SetLocalizeText(_tagText, m_shopDataBase.TagString);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected void SetIntervalLimit()
        {
            if (m_shopDataBase == null)
                return;

            if (m_intervalType != null)
            {
                if (m_shopDataBase.IntervalType == V2Enum_IntervalType.Day)
                {
                    m_intervalType.gameObject.SetActive(true);
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_intervalType
                        , string.Format("shop/ui/dailyLimit"));
                }
                else if (m_shopDataBase.IntervalType == V2Enum_IntervalType.Week)
                {
                    m_intervalType.gameObject.SetActive(true);
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_intervalType
                        , string.Format("shop/ui/weeklyLimit"));
                }
                else if (m_shopDataBase.IntervalType == V2Enum_IntervalType.Month)
                {
                    m_intervalType.gameObject.SetActive(true);
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_intervalType
                        , string.Format("shop/ui/monthlyLimit"));
                }
                else
                    m_intervalType.gameObject.SetActive(false);
            }

            if (m_intervalLimit != null)
            {
                if (m_shopDataBase.IntervalParam > 0)
                {
                    m_intervalLimit.gameObject.SetActive(true);
                    m_intervalLimit.text = string.Format("({0}/{1})", Managers.ShopManager.Instance.GetBuyCount(m_shopDataBase), m_shopDataBase.IntervalParam);
                }
                else
                    m_intervalLimit.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        protected void VisibleSoldOut(bool visible)
        {
            if (m_soldOutTrans != null)
                m_soldOutTrans.gameObject.SetActive(visible);

            if (m_elementBuyLight != null)
                m_elementBuyLight.gameObject.SetActive(visible == false);

            if (visible == true)
            {
                if (_tagGroup != null)
                {
                    _tagGroup.gameObject.SetActive(false);
                    Managers.LocalStringManager.Instance.SetLocalizeText(_tagText, m_shopDataBase.TagString);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnClick_BuyBtn()
        {
            Managers.ShopManager.Instance.Buy(m_shopDataBase);
        }
        //------------------------------------------------------------------------------------
    }
}