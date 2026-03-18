using System;
using System.Collections.Generic;
using GameBerry.Managers;
using GameBerry.Table;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class EquipmentReformDialog : IDialog
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text _equipNameText;
        [SerializeField] private TMP_Text _descText;
        [SerializeField] private Image _priceImage;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TMP_Text _messageText;

        [Header("Rows")]
        [SerializeField] private RectTransform _rowRoot;
        [SerializeField] private UIEquipmentReformStatRowElement _rowPrefab;

        [Header("Buttons")]
        [SerializeField] private Button _reformButton;

        private ItemHandle _handle;
        private Action _onReformed;
        private readonly List<UIEquipmentReformStatRowElement> _rows = new List<UIEquipmentReformStatRowElement>();

        protected override void OnLoad()
        {
            if (_reformButton != null)
                _reformButton.onClick.AddListener(OnClickReform);

            if (_descText != null)
                _descText.SetText("Lock the stats you want to keep. Unlocked stats reroll immediately and consume RoyalCoin.");

            if (_priceImage != null)
                _priceImage.sprite = ItemManager.Instance.GetIcon(EquipmentManager.Instance.GetRoyalCoinItemId());
        }

        protected override void OnEnter()
        {
            if (_handle.itemId > 0)
                RefreshView();
        }

        public void Bind(ItemHandle handle, Action onReformed = null)
        {
            _handle = handle;
            _onReformed = onReformed;

            for (int i = 0; i < _rows.Count; ++i)
            {
                if (_rows[i] != null)
                    _rows[i].ResetLock();
            }

            if (isEnter)
                RefreshView();
        }

        private void RefreshView()
        {
            EquipmentData data = EquipmentManager.Instance?.GetEquipmentData(_handle);
            if (data == null)
            {
                Exit();
                return;
            }

            if (_equipNameText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_equipNameText, ItemManager.Instance.GetItemNameLocalKey(_handle.itemId));

            EnsureRows(data, EquipmentManager.Instance.GetEquipmentRarity(_handle));
            RefreshPriceAndBalance();
            SetMessage(string.Empty, Color.white);
        }

        private void EnsureRows(EquipmentData data, Enum_Rarity rarity)
        {
            if (_rowRoot == null || _rowPrefab == null)
                return;

            while (_rows.Count < data.addStatList.Count)
            {
                UIEquipmentReformStatRowElement row = Instantiate(_rowPrefab, _rowRoot);
                row.gameObject.SetActive(true);
                row.SetLockChangedCallback(RefreshPriceAndBalance);
                _rows.Add(row);
            }

            for (int i = 0; i < _rows.Count; ++i)
            {
                bool active = i < data.addStatList.Count;
                _rows[i].gameObject.SetActive(active);

                if (!active)
                    continue;

                EquipmentAddStat addStat = data.addStatList[i];
                bool hasRange = EquipmentManager.Instance.TryGetReformRange(addStat.stat, data.level, rarity, out double minValue, out double maxValue, out _);
                bool isMaxValue = hasRange && IsAtMaxValue(addStat.value, maxValue);

                _rows[i].SetData(
                    addStat.stat,
                    addStat.value,
                    hasRange ? minValue : addStat.value,
                    hasRange ? maxValue : addStat.value,
                    isMaxValue);
            }
        }

        private bool IsAtMaxValue(double currentValue, double maxValue)
        {
            return Math.Abs(currentValue - maxValue) <= 0.0001d || currentValue > maxValue;
        }

        private void RefreshPriceAndBalance()
        {
            EquipmentData data = EquipmentManager.Instance?.GetEquipmentData(_handle);
            int totalStatCount = data?.addStatList?.Count ?? 0;
            int lockedCount = GetLockedStats().Count;

            long price = EquipmentManager.Instance.GetReformPrice(_handle, lockedCount);
            int royalCoinItemId = EquipmentManager.Instance.GetRoyalCoinItemId();
            long balance = royalCoinItemId > 0 ? ItemManager.Instance.GetItemAmount(royalCoinItemId) : 0;
            bool hasUnlockedStat = totalStatCount > 0 && lockedCount < totalStatCount;

            if (_priceText != null)
                _priceText.SetText($"{price:N0}");

            if (_reformButton != null)
                _reformButton.interactable = royalCoinItemId > 0 && balance >= price && hasUnlockedStat;
        }

        private HashSet<Enum_Stat> GetLockedStats()
        {
            HashSet<Enum_Stat> lockedStats = new HashSet<Enum_Stat>();
            for (int i = 0; i < _rows.Count; ++i)
            {
                if (_rows[i] == null || _rows[i].gameObject.activeSelf == false)
                    continue;

                if (_rows[i].IsLocked)
                    lockedStats.Add(_rows[i].Stat);
            }

            return lockedStats;
        }

        private void OnClickReform()
        {
            HashSet<Enum_Stat> lockedStats = GetLockedStats();
            if (EquipmentManager.Instance.TryReform(_handle, lockedStats, out string reason))
            {
                _onReformed?.Invoke();
                RefreshView();
                SetMessage("Reform applied immediately.", new Color(0.55f, 1f, 0.55f, 1f));
                return;
            }

            SetMessage($"Reform failed: {reason}", new Color(1f, 0.55f, 0.55f, 1f));
            RefreshPriceAndBalance();
        }

        private void SetMessage(string message, Color color)
        {
            if (_messageText == null)
                return;

            _messageText.SetText(message);
            _messageText.color = color;
        }
    }
}
