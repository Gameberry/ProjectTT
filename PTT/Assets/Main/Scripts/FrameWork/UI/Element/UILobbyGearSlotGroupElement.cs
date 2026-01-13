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
        public Enum_EquipType _enum_EquipType = Enum_EquipType.Max;

        [SerializeField]
        private Image _slotImage;

        [SerializeField]
        private UIItemElement _uiItemElement;

        [SerializeField]
        private TMP_Text _slotLevel;

        [SerializeField]
        private Transform _canLevelUp;

        private System.Action<Enum_EquipType> _callBack;
        private System.Action<Enum_EquipType> _unEquipCallBack;

        public void Init(System.Action<Enum_EquipType> action
            , System.Action<Enum_EquipType> unEquipCallBack)
        {
            _callBack = action;
            _unEquipCallBack = unEquipCallBack;

            if (_enum_EquipType == Enum_EquipType.Max)
                return;

            RefreshSlot();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SlotBtn(int slotid)
        {
            _callBack?.Invoke(_enum_EquipType);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_UnEquipSlot(int slotid)
        {
            _unEquipCallBack?.Invoke(_enum_EquipType);
        }
        //------------------------------------------------------------------------------------
        public void RefreshSlot()
        {
            if (_enum_EquipType == Enum_EquipType.Max)
                return;

            if (EquipmentManager.Instance.TryGetEquipSlotToHandle(_enum_EquipType, out var itemHandle))
            {
                if (_uiItemElement != null)
                { 
                    _uiItemElement.gameObject.SetActive(true);
                    _uiItemElement.Bind(itemHandle);
                }

                if (_slotLevel != null)
                { 
                    _slotLevel.gameObject.SetActive(true);
                    _slotLevel.SetText("+{0}", EquipmentManager.Instance.GetEquipSlotLevel(_enum_EquipType));
                }
            }
            else
            {
                if (_uiItemElement != null)
                    _uiItemElement.gameObject.SetActive(false);

                if (_slotLevel != null)
                    _slotLevel.gameObject.SetActive(false);
            }

            //if (_canLevelUp != null)
            //{
            //    if (characterGearData == null)
            //        _canLevelUp.gameObject.SetActive(false);
            //    else
            //        _canLevelUp.gameObject.SetActive(Managers.GearManager.Instance.ReadySynergyEnhance(_enum_EquipType));
            //}
        }
        //------------------------------------------------------------------------------------
    }
}