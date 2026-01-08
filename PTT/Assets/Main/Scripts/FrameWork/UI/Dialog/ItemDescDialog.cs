using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.UI;

namespace GameBerry.UI
{
    /// <summary>
    /// 인벤 아이템 클릭 시 뜨는 설명/행동 다이얼로그.
    /// - Stack/Instance를 ItemHandle로 통일해서 받는다.
    /// - 실제 강화/장착/판매 UI는 프로젝트에 맞춰 버튼을 연결/확장하면 됨.
    /// </summary>
    public class ItemDescDialog : IDialog
    {
        [Header("UI")]
        [SerializeField] private UIItemElement _uIItemElement;
        [SerializeField] private TMP_Text _itemName;
        [SerializeField] private TMP_Text _itemType;
        [SerializeField] private TMP_Text _itemRarity;
        [SerializeField] private TMP_Text _itemDesc;

        [Header("Buttons")]
        [SerializeField] private Button consumeButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button enhanceButton;
        [SerializeField] private Button sellButton;

        private ItemHandle _handle;

        // --------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (consumeButton != null) consumeButton.onClick.AddListener(OnConsume);
            if (equipButton != null) equipButton.onClick.AddListener(OnEquip);
            if (enhanceButton != null) enhanceButton.onClick.AddListener(OnEnhance);
            if (sellButton != null) sellButton.onClick.AddListener(OnSell);
        }
        // --------------------------------------------------------------------
        public void Bind(ItemHandle handle)
        {
            _handle = handle;

            if (_uIItemElement != null)
                _uIItemElement.Bind(_handle);

            // 최소 정보 표시(메타 차트 연동은 프로젝트 확장 포인트)
            if (_itemName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemName, ItemManager.Instance.GetItemNameLocalKey(handle.itemId));

            if (_itemType != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemType, ItemManager.Instance.GetItemType(handle.itemId).ToString());

            if (_itemRarity != null)
            {
                Enum_Rarity enum_Rarity = ItemManager.Instance.GetItemRarity(handle.itemId);
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemRarity, enum_Rarity.ToString());
                _itemRarity.color = StaticResource.Instance.GetRarityTextColor(enum_Rarity);
            }

            if (_itemDesc != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemDesc, ItemManager.Instance.GetItemDescLocalKey(handle.itemId));

            // 버튼 노출 제어(기본)
            if (equipButton != null) equipButton.gameObject.SetActive(_handle.IsInstance);
            if (enhanceButton != null) enhanceButton.gameObject.SetActive(_handle.IsInstance);

            // consume/sell은 프로젝트 룰에 따라 조절 가능
            if (consumeButton != null) consumeButton.gameObject.SetActive(true);
            if (sellButton != null) sellButton.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void OnConsume()
        {
            // 기본: 1개 소모(스택은 1, 인스턴스는 해당 1개)
            var res = ItemManager.Instance.Consume(_handle, 1, true);
            Debug.Log($"[ItemDescDialog] Consume {_handle} => {res.Success} ({res.Reason})");

            // 소모 후 닫기 (원하면 유지)
            Exit();
        }
        //------------------------------------------------------------------------------------
        private void OnEquip()
        {
            // 장착 시스템은 프로젝트별이므로 연결만 해두고 확장
            Debug.Log($"[ItemDescDialog] Equip requested: {_handle}");
            // 예) EquipmentManager.Instance.Equip(slot, _handle.instanceId);
        }
        //------------------------------------------------------------------------------------
        private void OnEnhance()
        {
            Debug.Log($"[ItemDescDialog] Enhance requested: {_handle}");
            // 예) EquipmentManager.Instance.Enhance(_handle.instanceId);
        }
        //------------------------------------------------------------------------------------
        private void OnSell()
        {
            Debug.Log($"[ItemDescDialog] Sell requested: {_handle}");
            // 보통 판매는 인스턴스는 1개, 스택은 입력 수량 기반
            // 일단 1개 판매=1개 소모로 가정
            var res = ItemManager.Instance.Consume(_handle, 1, true);
            Debug.Log($"[ItemDescDialog] Sell(Consume) {_handle} => {res.Success} ({res.Reason})");
            Exit();
        }
        //------------------------------------------------------------------------------------
    }
}
