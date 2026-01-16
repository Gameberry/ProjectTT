using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        [SerializeField] private Transform _destroyMark;
        [SerializeField] private Image _selectedFrame;

        [SerializeField] private Button _button;

        public event System.Action<Enum_EquipType> OnSlotClicked;

        public bool HasEquipment { get; private set; }

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void Init()
        {
            if (_enum_EquipType == Enum_EquipType.Max)
                return;

            if (_slotImage != null)
                _slotImage.sprite = StaticResource.Instance.GetEquipSlotSprite(_enum_EquipType);

            SetSelected(false);
        }
        //------------------------------------------------------------------------------------
        public void RefreshSlot()
        {
            if (_enum_EquipType == Enum_EquipType.Max)
            {
                HasEquipment = false;
                return;
            }

            if (EquipmentManager.Instance.TryGetEquippedHandle(_enum_EquipType, out var itemHandle))
            {
                HasEquipment = true;

                if (_uiItemElement != null)
                { 
                    _uiItemElement.gameObject.SetActive(true);
                    _uiItemElement.Bind(itemHandle);
                }

                int level = EquipmentManager.Instance.GetStarforceLevel(_enum_EquipType);
                bool isDestroyed = EquipmentManager.Instance.IsDestroyStarforce(_enum_EquipType);

                if (_destroyMark != null)
                    _destroyMark.gameObject.SetActive(isDestroyed);

                if (isDestroyed)
                {
                    if (_slotLevelGroup != null)
                        _slotLevelGroup.gameObject.SetActive(false);
                }
                else
                {
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

                
            }
            else
            {
                HasEquipment = false;

                if (_uiItemElement != null)
                    _uiItemElement.gameObject.SetActive(false);

                if (_slotLevelGroup != null)
                    _slotLevelGroup.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSelected(bool selected)
        {
            if (_selectedFrame != null)
                _selectedFrame.gameObject.SetActive(selected);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            if (HasEquipment)
                OnSlotClicked?.Invoke(_enum_EquipType);
        }
        //------------------------------------------------------------------------------------
    }
}