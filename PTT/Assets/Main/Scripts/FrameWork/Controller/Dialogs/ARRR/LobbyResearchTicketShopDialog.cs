using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class LobbyResearchTicketShopDialog : IDialog
    {
        [SerializeField]
        private UIShopElement_Group _uIShopElement_Group;

        protected override void OnLoad()
        {
            if (_uIShopElement_Group != null)
                _uIShopElement_Group.SetShopElement();
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyResearch_Shop);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.RedDotManager.isAlive == false)
                return;

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyResearch_Shop);
        }
        //------------------------------------------------------------------------------------
    }
}