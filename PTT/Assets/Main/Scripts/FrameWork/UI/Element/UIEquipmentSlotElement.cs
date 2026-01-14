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
    public class UIEquipmentSlotElement : MonoBehaviour
    {
        public Enum_EquipType _enum_EquipType = Enum_EquipType.Max;

        [SerializeField]
        private Image _slotImage;

        [SerializeField]
        private UIItemElement _uiItemElement;

        [SerializeField]
        private Transform _slotLevelGroup;

        [SerializeField]
        private TMP_Text _slotLevel;

        public void Init()
        {
            if (_enum_EquipType == Enum_EquipType.Max)
                return;

            if (_slotImage != null)
                _slotImage.sprite = StaticResource.Instance.GetEquipSlotSprite(_enum_EquipType);

            RefreshSlot();
        }
        //------------------------------------------------------------------------------------
        public void RefreshSlot()
        {
            if (_enum_EquipType == Enum_EquipType.Max)
                return;

            if (EquipmentManager.Instance.TryGetEquippedHandle(_enum_EquipType, out var itemHandle))
            {
                if (_uiItemElement != null)
                { 
                    _uiItemElement.gameObject.SetActive(true);
                    _uiItemElement.Bind(itemHandle);
                }

                int level = EquipmentManager.Instance.GetEquipSlotLevel(_enum_EquipType);

                if (level > 0)
                {
                    if (_slotLevelGroup != null)
                        _slotLevelGroup.gameObject.SetActive(true);

                    if (_slotLevel != null)
                        _slotLevel.SetText("+{0}", level);
                }
                else
                {
                    if (_slotLevelGroup != null)
                        _slotLevelGroup.gameObject.SetActive(false);
                }
            }
            else
            {
                if (_uiItemElement != null)
                    _uiItemElement.gameObject.SetActive(false);

                if (_slotLevelGroup != null)
                    _slotLevelGroup.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
    }
}