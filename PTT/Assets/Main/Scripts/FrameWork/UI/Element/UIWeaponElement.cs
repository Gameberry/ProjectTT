using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace GameBerry.UI
{
    public class UIWeaponElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _rarityColor;
        [SerializeField] private TMP_Text _tier;
        [SerializeField] private TMP_Text _level;
        [SerializeField] private TMP_Text _progress;
        [SerializeField] private Image _progressBar;
        [SerializeField] private GameObject _equippedMark;
        [SerializeField] private Button _selectButton;
        [SerializeField] private GameObject _selectedMark;
        [Header("Dim (Never Obtained)")]
        [SerializeField] private Image _dimOverlay;
        [Header("Awake Images")]
        [SerializeField] private Sprite _awakeFilled;
        [SerializeField] private Sprite _awakeEmpty;
        [SerializeField] private List<Image> _awakeImages = new List<Image>();

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

            var itemInfo = ItemManager.Instance.GetItemMeta(_itemId);
            if (itemInfo == null)
                return;

            var weaponData = WeaponManager.Instance.GetWeaponData(_itemId);
            bool neverObtained = weaponData == null;

            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(_itemId);

            if (_frame != null)
                _frame.sprite = StaticResource.Instance.GetRarityFrame(itemInfo.Rarity);

            if (_rarityColor != null)
                _rarityColor.color = StaticResource.Instance.GetRarityTextColor(itemInfo.Rarity);

            if (_tier != null)
                _tier.SetText(itemInfo.Tier.ToString());

            long count = WeaponManager.Instance.GetWeaponCount(_itemId);

            int level = WeaponManager.Instance.GetWeaponLevel(_itemId);
            if (_level != null)
                _level.SetText($"Lv.{level}");

            int awake = WeaponManager.Instance.GetAwakeLevel(_itemId);
            int maxAwake = WeaponManager.Instance.GetMaxAwake(_itemId);
            RefreshAwakeImages(awake, maxAwake);



            if (_progress != null)
            {
                long denominator = WeaponManager.Instance.IsMaxAwake(_itemId)
                    ? Define.WeaponCombineCount
                    : WeaponManager.Instance.GetAwakeCost(_itemId);

                if (denominator < 0)
                    denominator = 0;

                _progress.SetText($"{count}/{denominator}");
            }

            if(_progressBar != null)
            {
                long denominator = WeaponManager.Instance.IsMaxAwake(_itemId)
                    ? Define.WeaponCombineCount
                    : WeaponManager.Instance.GetAwakeCost(_itemId);

                float fillAmount = denominator > 0 ? (float)count / denominator : 0f;
                _progressBar.fillAmount = Mathf.Clamp01(fillAmount);
            }

            bool equipped = WeaponManager.Instance.IsEquipped(_itemId);
            if (_equippedMark != null)
                _equippedMark.SetActive(equipped);

            ApplyDimState(neverObtained);
        }

        private void RefreshAwakeImages(int awake, int maxAwake)
        {
            if (_awakeImages == null || _awakeImages.Count <= 0)
                return;

            int activeCount = Mathf.Clamp(awake, 0, Mathf.Max(0, maxAwake));
            for (int i = 0; i < _awakeImages.Count; ++i)
            {
                Image image = _awakeImages[i];
                if (image == null)
                    continue;

                bool active = i < activeCount;
                if (_awakeFilled != null && _awakeEmpty != null)
                {
                    image.gameObject.SetActive(true);
                    image.sprite = active ? _awakeFilled : _awakeEmpty;
                }
                else
                    image.gameObject.SetActive(active);
            }
        }

        private void OnClickSelect()
        {
            if (_itemId <= 0)
                return;

            _onSelect?.Invoke(_itemId);
        }

        private void ApplyDimState(bool dimmed)
        {
            if (_dimOverlay == null)
                return;

            _dimOverlay.gameObject.SetActive(dimmed);
        }
    }
}
