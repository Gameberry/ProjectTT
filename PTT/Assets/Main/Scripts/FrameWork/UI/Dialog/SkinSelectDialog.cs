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
        private SkinSlotType _currentSlot = SkinSlotType.Max;
        private Chart.SkinInfo _currentSkinInfo = null;

        private Dictionary<SkinSlotType, int> _uiTempSkin = new Dictionary<SkinSlotType, int>();
        private Skin _uiSkin = new Skin("uiskin");

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
            ShowSlot(SkinSlotType.Body);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            _uiTempSkin.Clear();
            
            Table.UserTable.Get<Table.SkinTable>()?.CapyEquipSkinDict(ref _uiTempSkin);

            Managers.SkinManager.Instance.SetDynamicSkin(_uiTempSkin, ref _uiSkin);

            _uIPlayerSpineObject?.SetSkin(_uiSkin);
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
            ShowSlot(tab.IntToEnum32<SkinSlotType>());
        }
        //------------------------------------------------------------------------------------
        private void ShowSlot(SkinSlotType slot)
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
        private void SetSkinElement(SkinSlotType skinSlotType)
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

                uISkinElement.SetSkinInfo(skinInfos[i]); ;
                uISkinElement.gameObject.SetActive(true);
            }

            for (int i = skinInfos.Count; i < _createdSkinElement.Count; ++i)
            {
                _createdSkinElement[i].gameObject.SetActive(false);
            }

            if (_uiTempSkin.ContainsKey(skinSlotType) == true)
                _currentSkinInfo = Managers.SkinManager.Instance.GetSkinInfo(_uiTempSkin[skinSlotType]);
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
                if (_uiTempSkin.ContainsKey(_currentSlot) == true)
                {
                    _uiTempSkin.Remove(_currentSlot);
                }
                else
                    return;
            }
            else
            {
                if (_uiTempSkin.ContainsKey(_currentSlot) == true)
                {
                    if (_uiTempSkin[_currentSlot] == skinInfo.Index)
                        return;

                    _uiTempSkin[_currentSlot] = skinInfo.Index;
                }
                else
                    _uiTempSkin.Add(_currentSlot, skinInfo.Index);
            }

            Managers.SkinManager.Instance.SetDynamicSkin(_uiTempSkin, ref _uiSkin);
            _uIPlayerSpineObject?.SetSkin(_uiSkin);

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
                Table.SkinData skinData = Managers.SkinManager.Instance.GetSkinData(skinInfo.Index);

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
        private void OnClick_SkinEquip()
        {
            if (_currentSkinInfo == null)
                Managers.SkinManager.Instance.UnequipSlotSkin(_currentSlot);
            else
            {
                if (_uiTempSkin.ContainsKey(_currentSlot) == true)
                {
                    Managers.SkinManager.Instance.EquipSlotSkin(_currentSlot, _uiTempSkin[_currentSlot]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SkinGet()
        {
            Table.SkinData skinData = Managers.SkinManager.Instance.GetSkin(_currentSkinInfo);
            if (skinData != null)
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
