using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    /// <summary>
    /// 스킨 인벤토리(컬렉션) 화면용 스크립트.
    /// - 슬롯별 탭 + 해금/잠금 표시
    /// - 장착/프리뷰는 SkinSelectDialog로 넘기는 것을 추천 (원하면 여기서도 확장 가능)
    /// </summary>
    public class SkinInventoryDialog : IDialog
    {
        [Header("탭 버튼들 (선택 상태 표시용)")]
        public List<UINumberBtn> uINumberBtns = new List<UINumberBtn>();

        [Header("리스트")]
        [SerializeField] private UISkinElement _skinButtonPrefab;
        [SerializeField] private Transform _skinBtnRoot;

        private Enum_SkinSlotType _currentSlot = Enum_SkinSlotType.Max;
        private readonly List<UISkinElement> _created = new List<UISkinElement>();

        protected override void OnLoad()
        {
            for (int i = 0; i < uINumberBtns.Count; ++i)
                uINumberBtns[i].AddListener = OnClickTab;

            ShowSlot(Enum_SkinSlotType.Body);
        }

        protected override void OnEnter()
        {
            Refresh();
        }

        private void OnClickTab(int tab)
        {
            ShowSlot(tab.IntToEnum32<Enum_SkinSlotType>());
        }

        private void ShowSlot(Enum_SkinSlotType slot)
        {
            if (_currentSlot == slot)
                return;

            _currentSlot = slot;

            int slottype = slot.Enum32ToInt();
            for (int i = 0; i < uINumberBtns.Count; ++i)
                uINumberBtns[i].SetSelected(slottype == uINumberBtns[i].Num);

            Refresh();
        }

        private void Refresh()
        {
            List<Chart.SkinInfo> skinInfos = Managers.SkinManager.Instance.GetSkinSlotInfoList(_currentSlot);

            for (int i = 0; i < skinInfos.Count; ++i)
            {
                UISkinElement el;
                if (i < _created.Count) el = _created[i];
                else
                {
                    el = Instantiate(_skinButtonPrefab, _skinBtnRoot);
                    el.Init(OnClickSkin);
                    _created.Add(el);
                }

                el.SetSkinInfo(skinInfos[i]);
                el.gameObject.SetActive(true);
            }

            for (int i = skinInfos.Count; i < _created.Count; ++i)
                _created[i].gameObject.SetActive(false);
        }

        private void OnClickSkin(Chart.SkinInfo skinInfo)
        {
            // 필요하면 여기서 SkinSelectDialog를 열거나, 상세창을 띄워도 됨.
            Debug.Log($"SkinInventory click: {(skinInfo != null ? skinInfo.SkinName : "Reset")}");
        }
    }
}
