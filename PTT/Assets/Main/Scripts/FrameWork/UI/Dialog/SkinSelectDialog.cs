using System;
using System.Collections.Generic;
using UnityEngine;
using Spine;

namespace GameBerry.UI
{
    public class SkinSelectDialog : IDialog
    {
        [Header("Target")]
        [SerializeField] private UIPlayerSpineObject _uIPlayerSpineObject;

        [Header("SkinBtnGroup")]
        [SerializeField] private UISkinElement _skinButtonPrefab;

        [SerializeField]
        private Transform _skinBtnRoot;

        private UISkinElement _resetBtn;

        private List<UISkinElement> _createdSkinElement = new List<UISkinElement>();

        [SerializeField]
        private CButton _equipBtn;

        [SerializeField]
        private CButton _getBtn;

        [Header("탭 버튼들 (선택 상태 표시용)")]
        public List<UINumberBtn> uINumberBtns = new List<UINumberBtn>();

        // 현재 선택된 탭
        private Enum_SkinSlotType _currentSlot = Enum_SkinSlotType.Max;
        private Chart.SkinInfo _currentSkinInfo = null;

        private Dictionary<Enum_SkinSlotType, int> _uiTempSkinDic = new Dictionary<Enum_SkinSlotType, int>();
        private Skin _uiTempSkin = new Skin("uiskin");

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < uINumberBtns.Count; ++i)
            {
                uINumberBtns[i].AddListener = OnClick_SkinTab;
            }

            if (_equipBtn != null)
                _equipBtn.onClick.AddListener(OnClick_SkinEquip);

            if (_getBtn != null)
                _getBtn.onClick.AddListener(OnClick_SkinGet);

            _resetBtn = CreateSkinElement();
            _resetBtn.SetSkinInfo(null);

            // ▶ 기본으로 Body 탭 보이게
            ShowSlot(Enum_SkinSlotType.Body);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            _uiTempSkinDic.Clear();
            
            Table.UserTable.Get<Table.SkinTable>()?.CapyEquipSkinDict(ref _uiTempSkinDic);

            Managers.SkinManager.Instance.SetDynamicSkin(_uiTempSkinDic, ref _uiTempSkin);

            _uIPlayerSpineObject?.SetSkin(_uiTempSkin);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            //_currentSlot = SkinSlotType.Max;
        }
        //------------------------------------------------------------------------------------
        #region SkinTab Func
        //------------------------------------------------------------------------------------
        private void OnClick_SkinTab(int tab)
        {
            ShowSlot(tab.IntToEnum32<Enum_SkinSlotType>());
        }
        //------------------------------------------------------------------------------------
        private void ShowSlot(Enum_SkinSlotType slot)
        {
            if (_currentSlot == slot)
                return;

            _currentSlot = slot;

            int slottype = slot.Enum32ToInt();

            for (int i = 0; i < uINumberBtns.Count; ++i)
            {
                uINumberBtns[i].SetSelected(slottype == uINumberBtns[i].Num);
            }

            SetSkinElement(slot);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SkinElement Func
        //------------------------------------------------------------------------------------
        private void SetSkinElement(Enum_SkinSlotType skinSlotType)
        {
            List<Chart.SkinInfo> skinInfos = Managers.SkinManager.Instance.GetSkinSlotInfoList(skinSlotType);

            for (int i = 0; i < skinInfos.Count; ++i)
            {
                UISkinElement uISkinElement = null;

                if (i < _createdSkinElement.Count)
                    uISkinElement = _createdSkinElement[i];
                else
                { 
                    uISkinElement = CreateSkinElement();
                    _createdSkinElement.Add(uISkinElement);
                }

                uISkinElement.SetSkinInfo(skinInfos[i]);
                uISkinElement.gameObject.SetActive(true);
            }

            for (int i = skinInfos.Count; i < _createdSkinElement.Count; ++i)
            {
                _createdSkinElement[i].gameObject.SetActive(false);
            }

            if (_uiTempSkinDic.ContainsKey(skinSlotType) == true)
                _currentSkinInfo = Managers.SkinManager.Instance.GetSkinInfo(_uiTempSkinDic[skinSlotType]);
            else
                _currentSkinInfo = null;

            SetSkinBtn(_currentSkinInfo);
        }
        //------------------------------------------------------------------------------------
        private UISkinElement CreateSkinElement()
        {
            var btn = Instantiate(_skinButtonPrefab, _skinBtnRoot);
            btn.Init(OnClick_SkinElement);

            return btn;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SkinElement(Chart.SkinInfo skinInfo)
        {
            if (skinInfo == null)
            { // Reset버튼을 눌렀을 때
                UnequipTempSkin(_currentSlot);
            }
            else
            {
                EquipTempSkin(_currentSlot, skinInfo.ItemId);
            }

            SetSkinBtn(skinInfo);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SkinBtn Func
        //------------------------------------------------------------------------------------
        private void SetSkinBtn(Chart.SkinInfo skinInfo)
        {
            bool enableEquip = false;

            if (skinInfo != null)
            { // Reset이 아님
                Table.SkinData skinData = Managers.SkinManager.Instance.GetSkinData(skinInfo.ItemId);

                if (skinData != null)
                { // 가지고 있는 스킨임
                    _getBtn?.gameObject.SetActive(false);

                    if (Managers.SkinManager.Instance.GetSkinEquipData(skinInfo.SkinType) != skinData)
                        enableEquip = true;
                }
                else
                {
                    _getBtn?.gameObject.SetActive(true);
                }
            }
            else
            {
                enableEquip = true;
                _getBtn?.gameObject.SetActive(false);
            }

            _currentSkinInfo = skinInfo;

            _equipBtn?.SetInteractable(enableEquip);
        }
        //------------------------------------------------------------------------------------
        private void EquipTempSkin(Enum_SkinSlotType slot, int index)
        {
            if (_uiTempSkinDic.ContainsKey(slot) == true)
            {
                if (_uiTempSkinDic[slot] == index)
                    return;

                _uiTempSkinDic[slot] = index;
            }
            else
                _uiTempSkinDic.Add(slot, index);

            Managers.SkinManager.Instance.SetDynamicSkin(_uiTempSkinDic, ref _uiTempSkin);
            _uIPlayerSpineObject?.SetSkin(_uiTempSkin);
        }
        //------------------------------------------------------------------------------------
        private void UnequipTempSkin(Enum_SkinSlotType slot)
        {
            if (_uiTempSkinDic.ContainsKey(slot) == true)
            {
                _uiTempSkinDic.Remove(slot);
            }
            else
                return;

            Managers.SkinManager.Instance.SetDynamicSkin(_uiTempSkinDic, ref _uiTempSkin);
            _uIPlayerSpineObject?.SetSkin(_uiTempSkin);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SkinEquip()
        {
            if (_currentSkinInfo == null)
            { 
                Managers.SkinManager.Instance.UnequipSlotSkin(_currentSlot);
                UnequipTempSkin(_currentSlot);
            }
            else
            {
                Managers.SkinManager.Instance.EquipSlotSkin(_currentSlot, _uiTempSkinDic[_currentSlot]);
                EquipTempSkin(_currentSlot, _uiTempSkinDic[_currentSlot]);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SkinGet()
        {
            bool unlocked = Managers.SkinManager.Instance.TryGetSkinByItem(_currentSkinInfo.ItemId);
            if (unlocked == true)
            { 
                _createdSkinElement.Find(x => x._skinInfo == _currentSkinInfo)?.SetSkinInfo(_currentSkinInfo);
                SetSkinBtn(_currentSkinInfo);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
