using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopFixedTermDialog : IDialog
    {
        [SerializeField]
        private List<UIShopElement_Group> m_uIShopElement_Groups = new List<UIShopElement_Group>();

        [SerializeField]
        private List<ContentDetailListRectTrans> m_contentDetailListRectTrans = new List<ContentDetailListRectTrans>();

        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private RectTransform m_groupRoot;

        [SerializeField]
        private float m_snapOffSet = -10.0f;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < m_uIShopElement_Groups.Count; ++i)
            {
                m_uIShopElement_Groups[i].SetSiblingIdx(i + 1);
                m_uIShopElement_Groups[i].SetShopElement();
                m_uIShopElement_Groups[i].SetSibling();
            }

            Message.AddListener<GameBerry.Event.SetShopGerneralDialogStateMsg>(SetShopFixedTermDialogState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetShopGerneralDialogStateMsg>(SetShopFixedTermDialogState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.ShopDailyWeek);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.ShopManager.isAlive == false)
                return;

            if (Managers.RedDotManager.isAlive == false)
                return;

            if (Managers.ShopManager.Instance.IsReadyFixedTermAD() == true)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.ShopDailyWeek);
            else
                Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.ShopDailyWeek);
        }
        //------------------------------------------------------------------------------------
        public override void BackKeyCall()
        {
            Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.None);
        }
        //------------------------------------------------------------------------------------
        private void SetShopFixedTermDialogState(GameBerry.Event.SetShopGerneralDialogStateMsg msg)
        {
            ContentDetailListRectTrans contentDetailListRectTrans = m_contentDetailListRectTrans.Find(x => x.v2Enum_SummonType == msg.ContentDetailList);
            if (contentDetailListRectTrans != null)
            {
                RectTransform rectTransform = contentDetailListRectTrans.ContentRect;

                Vector2 offset = Vector2.zero;
                offset.y = m_snapOffSet;

                Util.ScrollViewSnapToItem(m_elementScrollRect, m_groupRoot, rectTransform, offset);
            }

        }
        //------------------------------------------------------------------------------------
    }
}