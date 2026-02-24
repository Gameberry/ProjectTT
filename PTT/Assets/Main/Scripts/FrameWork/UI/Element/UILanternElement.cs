using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class UILanternElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _rarityColor;
        [SerializeField] private TMP_Text _tier;
        [SerializeField] private TMP_Text _level;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private Image _countFill;
        [SerializeField] private GameObject _equippedMark;
        [SerializeField] private GameObject _selectedMark;
        [SerializeField] private Image _dimOverlay;
        [SerializeField] private Button _selectButton;

        private int _itemId;
        private Action<int> _onSelect;

        private void Awake()
        {
            if (_selectButton == null)
                _selectButton = GetComponent<Button>();

            if (_selectButton != null)
                _selectButton.onClick.AddListener(OnClickSelect);
        }

        public void Bind(int itemId)
        {
            _itemId = itemId;
            Refresh();
        }

        public void SetOnSelect(Action<int> onSelect)
        {
            _onSelect = onSelect;
        }

        public void SetSelected(bool selected)
        {
            if (_selectedMark != null)
                _selectedMark.SetActive(selected);
        }

        public void Refresh()
        {
            if (_itemId <= 0)
                return;

            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(_itemId);
            if (itemInfo == null)
                return;

            LanternData lanternData = LanternManager.Instance.GetLanternData(_itemId);
            bool neverObtained = lanternData == null;

            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(_itemId);

            if (_frame != null)
                _frame.sprite = StaticResource.Instance.GetRarityFrame(itemInfo.Rarity);

            if (_rarityColor != null)
                _rarityColor.color = StaticResource.Instance.GetRarityTextColor(itemInfo.Rarity);

            if (_tier != null)
                _tier.SetText(itemInfo.Tier.ToString());

            int level = LanternManager.Instance.GetLanternLevel(_itemId);
            if (_level != null)
                _level.SetText($"Lv.{level}");

            long count = LanternManager.Instance.GetLanternCount(_itemId);
            int required = Mathf.Max(1, LanternManager.Instance.GetRequiredCountForGrowth(_itemId));
            if (_count != null)
                _count.SetText($"{count}/{required}");

            if (_countFill != null)
                _countFill.fillAmount = Mathf.Clamp01(required > 0 ? (float)count / required : 0f);

            if (_equippedMark != null)
                _equippedMark.SetActive(LanternManager.Instance.IsEquipped(_itemId));

            if (_dimOverlay != null)
                _dimOverlay.gameObject.SetActive(neverObtained);
        }

        private void OnClickSelect()
        {
            if (_itemId <= 0)
                return;

            _onSelect?.Invoke(_itemId);
        }
    }
}
