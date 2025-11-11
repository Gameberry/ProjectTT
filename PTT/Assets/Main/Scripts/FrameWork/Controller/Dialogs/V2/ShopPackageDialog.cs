using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopPackageDialog : IDialog
    {
        [SerializeField]
        private List<UIShopElement_Group> m_uIShopElement_Groups = new List<UIShopElement_Group>();

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
        protected override void OnEnter()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.FreePurchase)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(3);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.FreePurchase)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(2);
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