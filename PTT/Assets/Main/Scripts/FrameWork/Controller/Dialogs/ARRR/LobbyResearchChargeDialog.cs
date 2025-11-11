using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class LobbyResearchChargeDialog : IDialog
    {
        [SerializeField]
        private TMP_Text _researchGetIntervelDesc;

        [SerializeField]
        private TMP_Text _researchAccumChargeCount;

        [SerializeField]
        private Transform _researchMaxCharge;

        [SerializeField]
        private Button _researchChargeGetBtn;


        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_researchChargeGetBtn != null)
                _researchChargeGetBtn.onClick.AddListener(OnClick_GetBtn);

            Managers.ResearchManager.Instance.RechargeTime += SetResearchChargeRemainTime;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyResearch_Charge);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.RedDotManager.isAlive == false)
                return;

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyResearch_Charge);
        }
        //------------------------------------------------------------------------------------
        private void SetResearchChargeRemainTime(string timestamp)
        {
            if (_researchAccumChargeCount != null)
                _researchAccumChargeCount.SetText(string.Format("{0:0}/{1}", ResearchContainer.ChargeResearchCount, Managers.ResearchManager.Instance.GetMaxResearchCharge()));

            if (_researchMaxCharge != null)
            {
                _researchMaxCharge.gameObject.SetActive(ResearchContainer.ChargeResearchCount >= Managers.ResearchManager.Instance.GetMaxResearchCharge());
            }

            if (_researchGetIntervelDesc != null)
            {
                string timestr = Managers.TimeManager.Instance.GetSecendToDayString_MS(Managers.ResearchManager.Instance.GeResearchChargeInterval().ToInt());

                _researchGetIntervelDesc.SetText(string.Format(Managers.LocalStringManager.Instance.GetLocalString("research/chargedesc"), timestr, Managers.ResearchManager.Instance.GeResearchChargeCount()));
                //_researchGetIntervelDesc.SetText(string.Format("{0} / {1}", timestr, Managers.ResearchManager.Instance.GeResearchChargeCount()));
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GetBtn()
        {
            if (Managers.ResearchManager.Instance.GetResearchChargeAmount() == true)
            {
                ElementExit();
            }
        }
        //------------------------------------------------------------------------------------
    }
}