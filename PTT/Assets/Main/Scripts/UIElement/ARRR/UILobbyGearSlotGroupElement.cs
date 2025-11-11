using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UILobbyGearSlotGroupElement : MonoBehaviour
    {
        public V2Enum_GearType SynergyType = V2Enum_GearType.Max;

        [SerializeField]
        private UIARRRSkillSlotElement uIARRRSkillSlotElements;

        [SerializeField]
        private TMP_Text _slotLevel;

        [SerializeField]
        private Transform _canLevelUp;

        private System.Action<V2Enum_GearType> _callBack;
        private System.Action<V2Enum_GearType> _unEquipCallBack;

        public void Init(System.Action<V2Enum_GearType> action
            , System.Action<V2Enum_GearType> unEquipCallBack)
        {
            _callBack = action;
            _unEquipCallBack = unEquipCallBack;

            if (SynergyType == V2Enum_GearType.Max)
                return;

            uIARRRSkillSlotElements.Init(OnClick_SlotBtn, null, null, null);
            uIARRRSkillSlotElements.ConnectUnEquipBtn(OnClick_UnEquipSlot);
            uIARRRSkillSlotElements.SetSlotID(0);
            uIARRRSkillSlotElements.DragLock(true);

            RefreshAllSlot();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SlotBtn(int slotid)
        {
            _callBack?.Invoke(SynergyType);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_UnEquipSlot(int slotid)
        {
            _unEquipCallBack?.Invoke(SynergyType);
        }
        //------------------------------------------------------------------------------------
        public void RefreshAllSlot()
        {
            if (SynergyType == V2Enum_GearType.Max)
                return;

            UIARRRSkillSlotElement uIARRRSkillSlotElement = uIARRRSkillSlotElements;

            GearData characterGearData = Managers.GearManager.Instance.EquipedRuneData(SynergyType);
            uIARRRSkillSlotElement.SetSkill(characterGearData);

            if (_slotLevel != null)
            {
                int level = Managers.GearManager.Instance.GetSlotLevel(SynergyType);
                if (level <= 0)
                    _slotLevel.gameObject.SetActive(false);
                else
                { 
                    _slotLevel.gameObject.SetActive(true);
                    _slotLevel.SetText("+{0}", level);
                }
            }

            if (_canLevelUp != null)
            {
                if (characterGearData == null)
                    _canLevelUp.gameObject.SetActive(false);
                else
                    _canLevelUp.gameObject.SetActive(Managers.GearManager.Instance.ReadySynergyEnhance(SynergyType));
            }
        }
        //------------------------------------------------------------------------------------
    }
}