using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopSpecialDialog : IDialog
    {
        [SerializeField]
        private List<UIShopElement_Group> m_uIShopElement_Groups = new List<UIShopElement_Group>();

        [SerializeField]
        private Transform m_groupRoot;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < m_uIShopElement_Groups.Count; ++i)
            {
                m_uIShopElement_Groups[i].SetSiblingIdx(i + 1);
                m_uIShopElement_Groups[i].SetShopElement();
                m_uIShopElement_Groups[i].SetSibling();
            }
        }
        //------------------------------------------------------------------------------------
        public override void BackKeyCall()
        {
            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.None);
        }
        //------------------------------------------------------------------------------------
    }
}