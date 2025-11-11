using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementMonth_Group : UIShopElement_Group
    {
        [SerializeField]
        private TMP_Text m_remainTime;

        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIShopElement m_uIShopElement;

        //------------------------------------------------------------------------------------
        public override void SetShopElement()
        {
            List<ShopPackageData> ShopPackageDatas = Managers.ShopManager.Instance.GetPackageDatas();

            if (ShopPackageDatas != null)
                ShopPackageDatas = ShopPackageDatas.FindAll(x => x.IntervalType == V2Enum_IntervalType.Month && x.MenuType == V2Enum_ShopMenuType.Monthly);

            for (int i = 0; i < ShopPackageDatas.Count; ++i)
            {
                GameObject clone = Instantiate(m_uIShopElement.gameObject, m_elementRoot);

                UIShopElement uIShopElement = clone.GetComponent<UIShopElement>();
                uIShopElement.Init();
                uIShopElement.SetShopElement(ShopPackageDatas[i]);
            }

            elementCount = ShopPackageDatas.Count;

            SetLayoutElementSize();

            Managers.TimeManager.Instance.RemainInitMonthContent_Text += SetInitInterval;
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval(string remaintime)
        {
            m_remainTime?.SetText(remaintime);
        }
        //------------------------------------------------------------------------------------
    }
}