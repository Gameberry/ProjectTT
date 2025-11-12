using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class LobbySynergyRuneDetailPopupDialog : IDialog
    {
        [SerializeField]
        private Transform _guideBG4;

        [SerializeField]
        private Transform _guideBG5;

        [SerializeField]
        private UILobbySynergyRuneElement _synergyRuneElement;

        [SerializeField]
        private Button _synergyRuneEquip;


        [SerializeField]
        private TMP_Text m_goodsName;

        [SerializeField]
        private TMP_Text m_goodsDesc;

        [SerializeField]
        private TMP_Text m_applyJobBuff;

        [SerializeField]
        private TMP_Text _synergyRuneEquipText;

        [SerializeField]
        private Button _synergyRuneUnEquip;

        private SynergyRuneData currentSynergyRuneData;
        private bool isEquipState = false;
        private Enum_SynergyType currentEnum_SynergyType;
        private int currentSlotidx;

        private bool onEquip = false;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_synergyRuneEquip != null)
                _synergyRuneEquip.onClick.AddListener(OnClick_SynergyRuneEquip);

            if (_synergyRuneUnEquip != null)
                _synergyRuneUnEquip.onClick.AddListener(OnClick_SynergyRuneUnEquip);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            onEquip = false;
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RuneTutorial)
            {
                if (_guideBG5 != null)
                    _guideBG5.gameObject.SetActive(false);

                if (onEquip == false)
                {
                    if (_guideBG4 != null)
                        _guideBG4.gameObject.SetActive(true);

                    Managers.GuideInteractorManager.Instance.SetGuideStep(4);
                }
                else
                {
                    if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 5)
                    {
                        Managers.GuideInteractorManager.Instance.EndGuideQuest();
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SynergyRuneElement(SynergyRuneData synergyRuneData)
        {
            currentSynergyRuneData = synergyRuneData;


            if (m_goodsName != null)
            {
                string localkey = Managers.SynergyRuneManager.Instance.GetSynergyLocalKey(synergyRuneData.Index);
                Managers.LocalStringManager.Instance.SetLocalizeText(m_goodsName, localkey);
            }

            if (m_goodsDesc != null)
            {
                string descKey = string.Format("{0}/desc/{1}", V2Enum_Goods.SynergyRune.ToString().ToCamelCase(), synergyRuneData.Index);

                Managers.LocalStringManager.Instance.SetLocalizeText(m_goodsDesc, descKey);
            }

            if (m_applyJobBuff != null)
                m_applyJobBuff.gameObject.SetActive(synergyRuneData.SynergyType == Managers.JobManager.Instance.GetCurrentJobType());

            if (_synergyRuneElement != null)
                _synergyRuneElement.SetSynergyEffectData(synergyRuneData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyRuneEquip()
        {
            Managers.SynergyRuneManager.Instance.EquipSkill(currentSynergyRuneData);

            onEquip = true;

            ElementExit();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyRuneUnEquip()
        {
            Managers.SynergyRuneManager.Instance.UnEquipSkillSlot(
                currentSlotidx);

            ElementExit();
        }
        //------------------------------------------------------------------------------------
        public void SetSlotState(Enum_SynergyType Enum_SynergyType, int slotidx)
        {
            currentEnum_SynergyType = Enum_SynergyType;
            currentSlotidx = slotidx;
        }
        //------------------------------------------------------------------------------------
        public void SetEquipMode(bool mode)
        {
            isEquipState = mode;

            if (_synergyRuneEquip != null)
                _synergyRuneEquip.gameObject.SetActive(mode);

            if (_synergyRuneUnEquip != null)
                _synergyRuneUnEquip.gameObject.SetActive(!mode);

            if (currentSynergyRuneData == null)
                return;

            if (isEquipState == true)
            {
                bool Equipfull = Managers.SynergyRuneManager.Instance.GetCanEquipSkillSlotIdx() == -1;

                if (_synergyRuneEquipText != null)
                {
                    if (Equipfull == true)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_synergyRuneEquipText, "common/ui/slotfull");
                    else
                        Managers.LocalStringManager.Instance.SetLocalizeText(_synergyRuneEquipText, "common/ui/equip");
                }

                if (_synergyRuneEquip != null)
                {
                    _synergyRuneEquip.interactable = Equipfull == false;
                }

            }
        }
        //------------------------------------------------------------------------------------
    }
}