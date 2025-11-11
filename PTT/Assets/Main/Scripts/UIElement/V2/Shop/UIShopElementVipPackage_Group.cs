using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementVipPackage_Group : UIShopElement_Group
    {
        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIShopElement m_uIShopElement;

        [SerializeField]
        public List<UIShopElement> uIShopElements = new List<UIShopElement>();

        private Dictionary<VipPackageShopData, UIShopElement> _elements = new Dictionary<VipPackageShopData, UIShopElement>();

        [SerializeField]
        private int index = 150040021;

        [SerializeField]
        private UIShopElement_LimitTime _uIShopElement_LimitTime;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            Message.AddListener<GameBerry.Event.RefreshShopVipPackageMsg>(RefreshShopVipPackage);
            if (_uIShopElement_LimitTime != null)
                _uIShopElement_LimitTime.Init();
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            Message.RemoveListener<GameBerry.Event.RefreshShopVipPackageMsg>(RefreshShopVipPackage);
        }
        //------------------------------------------------------------------------------------
        public override void SetShopElement()
        {
            ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(index);
            if (_uIShopElement_LimitTime != null)
            {
                if (shopDataBase == null)
                    _uIShopElement_LimitTime.gameObject.SetActive(false);
                else
                {
                    PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo != null)
                    {
                        if (playerShopInfo.InitTimeStemp < Managers.TimeManager.Instance.Current_TimeStamp)
                            _uIShopElement_LimitTime.gameObject.SetActive(false);
                        else
                        {
                            _uIShopElement_LimitTime.gameObject.SetActive(true);
                            _uIShopElement_LimitTime.SetShopElement(shopDataBase);

                            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += PackageCustomUpdate1Sec;
                        }
                    }
                    else
                    {
                        _uIShopElement_LimitTime.gameObject.SetActive(true);
                        _uIShopElement_LimitTime.SetShopElement(shopDataBase);

                        Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += PackageCustomUpdate1Sec;
                    }
                }
            }

            List<VipPackageShopData> shopDiamondChargeDatas = Managers.VipPackageManager.Instance.GetVipPackageShopDatas();

            for (int i = 0; i < shopDiamondChargeDatas.Count; ++i)
            {
                UIShopElement uIShopElement = null;

                if (uIShopElements.Count > i)
                { 
                    uIShopElement = uIShopElements[i];
                    uIShopElement.Init();
                }
                else
                {
                    GameObject clone = Instantiate(m_uIShopElement.gameObject, m_elementRoot);

                    uIShopElement = clone.GetComponent<UIShopElement>();
                    uIShopElement.Init();
                }

                uIShopElement.gameObject.SetActive(true);
                uIShopElement.SetShopElement(shopDiamondChargeDatas[i]);

                _elements.Add(shopDiamondChargeDatas[i], uIShopElement);
            }

            for (int i = shopDiamondChargeDatas.Count; i < uIShopElements.Count; ++i)
            {
                uIShopElements[i].gameObject.SetActive(false);
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
        private void PackageCustomUpdate1Sec()
        {
            ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(index);
            if (_uIShopElement_LimitTime != null)
            {
                if (shopDataBase == null)
                    _uIShopElement_LimitTime.gameObject.SetActive(false);
                else
                {
                    PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo != null)
                    {
                        if (playerShopInfo.InitTimeStemp < Managers.TimeManager.Instance.Current_TimeStamp)
                        { 
                            _uIShopElement_LimitTime.gameObject.SetActive(false);
                            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec -= PackageCustomUpdate1Sec;
                        }
                            
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopVipPackage(GameBerry.Event.RefreshShopVipPackageMsg msg)
        {
            RefreshElement();
        }
        //------------------------------------------------------------------------------------
    }
}