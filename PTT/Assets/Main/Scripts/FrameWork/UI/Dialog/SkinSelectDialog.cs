using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public class SkinSelectDialog : IDialog
    {
        [Header("Target")]
        [SerializeField] private SkeletonAnimationHandler _uiHandler;

        [Header("UI Prefab")]
        [SerializeField] private UISpineSkinButtonElement _skinButtonPrefab;

        [Header("슬롯별 버튼이 생성될 패널(부모 트랜스폼)")]
        public Transform BodyParent;
        public Transform HairParent;
        public Transform WeaponParent;
        public Transform FaceParent;
        public Transform BackParent;

        [Header("탭 버튼들 (선택 상태 표시용)")]
        public UISpineSkinTabButtonElement BodyTab;
        public UISpineSkinTabButtonElement HairTab;
        public UISpineSkinTabButtonElement WeaponTab;
        public UISpineSkinTabButtonElement FaceTab;
        public UISpineSkinTabButtonElement BackTab;

        [Serializable]
        public class SlotSkinOption
        {
            public SkinSlotType Slot;
            public string SkinName;
            public string DisplayName;
        }

        [Header("슬롯별로 제공할 스킨 목록")]
        public List<SlotSkinOption> SkinOptions = new List<SlotSkinOption>();

        private SpineModelData _modelData;

        // 슬롯별 패널 캐시
        private readonly Dictionary<SkinSlotType, Transform> _slotParents =
            new Dictionary<SkinSlotType, Transform>();

        // 현재 선택된 탭
        private SkinSlotType _currentSlot = SkinSlotType.Body;

        protected override void OnLoad()
        {
            // 슬롯 패널 맵핑
            _slotParents[SkinSlotType.Body] = BodyParent;
            _slotParents[SkinSlotType.Hair] = HairParent;
            _slotParents[SkinSlotType.Weapon] = WeaponParent;
            _slotParents[SkinSlotType.Face] = FaceParent;
            _slotParents[SkinSlotType.Back] = BackParent;

            _modelData = Managers.SkinManager.Instance.GetPlayerSpineModelData();
            _uiHandler.SetSpineModel(_modelData);
            _uiHandler.PlayAnimation_Once(CharacterState.Idle, true);

            for (int i = 0; i < _modelData.SkinList.Count; ++i)
            {
                string skinName = _modelData.SkinList[i];
                SlotSkinOption slotSkinOption = new SlotSkinOption();

                if (skinName.Contains("Weapon"))
                    slotSkinOption.Slot = SkinSlotType.Weapon;
                else if (skinName.Contains("Back"))
                    slotSkinOption.Slot = SkinSlotType.Back;
                else if (skinName.Contains("Glass"))
                    slotSkinOption.Slot = SkinSlotType.Face;
                else if (skinName.Contains("Hair"))
                    slotSkinOption.Slot = SkinSlotType.Hair;
                else
                    slotSkinOption.Slot = SkinSlotType.Body;

                slotSkinOption.SkinName = skinName;
                slotSkinOption.DisplayName = skinName;

                SkinOptions.Add(slotSkinOption);
            }

            BuildUI();

            // ▶ 기본으로 Body 탭 보이게
            ShowSlot(SkinSlotType.Body);
        }

        private void BuildUI()
        {
            if (_skinButtonPrefab == null || _uiHandler == null)
                return;

            // 슬롯별 "없음(기본)" 버튼
            //CreateNoneButton(SpineEquipSlot.Body, BodyParent);
            //CreateNoneButton(SpineEquipSlot.Hair, HairParent);
            //CreateNoneButton(SpineEquipSlot.Weapon, WeaponParent);
            CreateNoneButton(SkinSlotType.Face, FaceParent);
            CreateNoneButton(SkinSlotType.Back, BackParent);

            // 실제 옵션 버튼
            foreach (var opt in SkinOptions)
            {
                var parent = GetParentForSlot(opt.Slot);
                if (parent == null)
                    continue;

                var btn = Instantiate(_skinButtonPrefab, parent);
                var displayName = string.IsNullOrEmpty(opt.DisplayName)
                    ? opt.SkinName
                    : opt.DisplayName;

                btn.Init(_uiHandler, opt.Slot, opt.SkinName, displayName);
            }
        }

        private void CreateNoneButton(SkinSlotType slot, Transform parent)
        {
            if (parent == null)
                return;

            var btn = Instantiate(_skinButtonPrefab, parent);
            btn.Init(_uiHandler, slot, null, "없음");
        }

        private Transform GetParentForSlot(SkinSlotType slot)
        {
            Transform t;
            return _slotParents.TryGetValue(slot, out t) ? t : null;
        }

        // ─────────────────────────────────────
        // 탭 클릭 콜백
        // ─────────────────────────────────────
        public void OnClickSlotTab(SkinSlotType slot)
        {
            ShowSlot(slot);
        }

        private void ShowSlot(SkinSlotType slot)
        {
            _currentSlot = slot;

            // 패널 On/Off
            foreach (var pair in _slotParents)
            {
                if (pair.Value == null) continue;
                pair.Value.gameObject.SetActive(pair.Key == slot);
            }

            // 탭 선택 상태 표시
            SetTabSelected(BodyTab, SkinSlotType.Body == slot);
            SetTabSelected(HairTab, SkinSlotType.Hair == slot);
            SetTabSelected(WeaponTab, SkinSlotType.Weapon == slot);
            SetTabSelected(FaceTab, SkinSlotType.Face == slot);
            SetTabSelected(BackTab, SkinSlotType.Back == slot);
        }

        private void SetTabSelected(UISpineSkinTabButtonElement tab, bool selected)
        {
            if (tab != null)
                tab.SetSelected(selected);
        }
    }
}
