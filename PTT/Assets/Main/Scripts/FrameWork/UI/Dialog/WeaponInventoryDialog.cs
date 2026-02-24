using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Common;
using GameBerry.Chart;

namespace GameBerry.UI
{
    public class WeaponInventoryDialog : IDialog
    {
        [SerializeField] private UIWeaponElement _weaponElementPrefab;
        [SerializeField] private Transform _contentRoot;
        [Header("Detail")]
        [SerializeField] private GameObject _detailRoot;
        [SerializeField] private Image _detailIcon;
        [SerializeField] private Image _detailFrame;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailRarity;
        [SerializeField] private TMP_Text _detailTier;
        [SerializeField] private TMP_Text _detailLevel;
        [SerializeField] private TMP_Text _detailAwake;

        [SerializeField] private TMP_Text _detailEquipStats;
        [SerializeField] private TMP_Text _detailOwnStats;
        [Header("Detail Awake Images")]
        [SerializeField] private Sprite _awakeFilled;
        [SerializeField] private Sprite _awakeEmpty;
        [SerializeField] private List<Image> _awakeImages = new List<Image>();
        [Header("Detail Buttons")]
        [SerializeField] private Image _awakeNeedIcon;
        [SerializeField] private TMP_Text _awakeNeedCountText;
        [SerializeField] private Button _awakeButton;
        [SerializeField] private TMP_Text _awakeButtonText;

        [SerializeField] private Image _levelUpNeedIcon;
        [SerializeField] private TMP_Text _levelUpNeedCountText;
        [SerializeField] private Button _levelUpButton;
        [SerializeField] private TMP_Text _levelUpButtonText;

        [SerializeField] private Button _equipButton;
        [SerializeField] private TMP_Text _equipButtonText;
        [Header("Batch Buttons")]
        [SerializeField] private Button _allAwakeOrCombineButton;
        [SerializeField] private TMP_Text _allAwakeOrCombineButtonText;

        private readonly List<UIWeaponElement> _spawned = new List<UIWeaponElement>();
        private readonly ObjectPool<UIWeaponElement> _elementPool = new ObjectPool<UIWeaponElement>();
        private readonly List<int> _weaponItemIds = new List<int>();
        private readonly StringBuilder _sb = new StringBuilder(128);
        private int _selectedItemId = 0;
        private bool _batchActionIsAwake = false;

        protected override void OnLoad()
        {
            if (_awakeButton != null) _awakeButton.onClick.AddListener(OnClickAwakeOrCombine);
            if (_levelUpButton != null) _levelUpButton.onClick.AddListener(OnClickLevelUp);
            if (_equipButton != null) _equipButton.onClick.AddListener(OnClickEquip);
            if (_allAwakeOrCombineButton != null) _allAwakeOrCombineButton.onClick.AddListener(OnClickAllAwakeOrCombine);

            Refresh();
        }

        protected override void OnEnter()
        {
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.OnWeaponDataChanged += Refresh;
                WeaponManager.Instance.OnWeaponEquipChanged += Refresh;
            }

            if (ItemManager.Instance != null)
                ItemManager.Instance.OnWeaponStorageChanged += Refresh;

            Refresh();
        }

        protected override void OnExit()
        {
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.OnWeaponDataChanged -= Refresh;
                WeaponManager.Instance.OnWeaponEquipChanged -= Refresh;
            }

            if (ItemManager.Instance != null)
                ItemManager.Instance.OnWeaponStorageChanged -= Refresh;
        }

        protected override void OnUnload()
        {
            ReleaseAllSpawned();
            _elementPool.ClearAll();
        }

        private void BuildWeaponIdList()
        {
            _weaponItemIds.Clear();

            var chart = Chart.GameChart.Get<Chart.WeaponChart>();
            if (chart?.rows == null)
                return;

            for (int i = 0; i < chart.rows.Length; ++i)
            {
                var info = chart.rows[i];
                if (info == null)
                    continue;

                _weaponItemIds.Add(info.ItemId);
            }
        }

        private void Rebuild()
        {
            ReleaseAllSpawned();

            if (_weaponElementPrefab == null || _contentRoot == null)
                return;

            for (int i = 0; i < _weaponItemIds.Count; ++i)
            {
                var element = GetOrCreateElement();
                if (element == null)
                    continue;

                element.transform.SetParent(_contentRoot, false);
                element.gameObject.SetActive(true);
                element.SetOnSelect(OnSelectWeapon);
                element.Bind(_weaponItemIds[i]);
                _spawned.Add(element);
            }

            EnsureSelectedWeapon();
            RefreshSelectionVisual();
        }

        private void Refresh()
        {
            BuildWeaponIdList();
            Rebuild();
            RefreshBatchButtons();
            RefreshDetail();
        }

        private void RefreshBatchButtons()
        {
            WeaponManager wm = WeaponManager.Instance;
            bool anyAwake = wm != null && wm.CanAnyAwake();
            bool anyCombine = wm != null && wm.CanAnyCombine();
            _batchActionIsAwake = anyAwake;

            if (_allAwakeOrCombineButton != null)
            {
                _allAwakeOrCombineButton.gameObject.SetActive(true);
                _allAwakeOrCombineButton.interactable = anyAwake || anyCombine;
            }

            if (_allAwakeOrCombineButtonText != null)
                _allAwakeOrCombineButtonText.SetText(anyAwake ? "AllAwake" : "AllCombine");
        }

        private UIWeaponElement GetOrCreateElement()
        {
            UIWeaponElement element = _elementPool.GetObject();
            if (element != null)
                return element;

            if (_weaponElementPrefab == null || _contentRoot == null)
                return null;

            return Instantiate(_weaponElementPrefab, _contentRoot);
        }

        private void ReleaseAllSpawned()
        {
            for (int i = 0; i < _spawned.Count; ++i)
            {
                UIWeaponElement element = _spawned[i];
                if (element == null)
                    continue;

                element.gameObject.SetActive(false);
                _elementPool.PoolObject(element);
            }

            _spawned.Clear();
        }

        private void OnSelectWeapon(int itemId)
        {
            _selectedItemId = itemId;
            RefreshSelectionVisual();
            RefreshDetail();
        }

        private void EnsureSelectedWeapon()
        {
            if (_weaponItemIds.Count <= 0)
            {
                _selectedItemId = 0;
                return;
            }

            if (_selectedItemId > 0 && _weaponItemIds.Contains(_selectedItemId))
                return;

            int equippedId = WeaponManager.Instance != null ? WeaponManager.Instance.GetEquippedWeaponId() : 0;
            if (equippedId > 0 && _weaponItemIds.Contains(equippedId))
            {
                _selectedItemId = equippedId;
                return;
            }

            _selectedItemId = _weaponItemIds[0];
        }

        private void RefreshSelectionVisual()
        {
            for (int i = 0; i < _spawned.Count; ++i)
            {
                if (_spawned[i] == null)
                    continue;

                bool selected = _weaponItemIds.Count > i && _weaponItemIds[i] == _selectedItemId;
                _spawned[i].SetSelected(selected);
                _spawned[i].Refresh();
            }
        }

        private void RefreshDetail()
        {
            if (_detailRoot != null)
                _detailRoot.SetActive(_selectedItemId > 0);

            if (_selectedItemId <= 0)
                return;

            var wm = WeaponManager.Instance;
            var im = ItemManager.Instance;
            if (wm == null || im == null)
                return;

            ItemInfo itemInfo = im.GetItemMeta(_selectedItemId);
            WeaponInfo weaponInfo = wm.GetWeaponInfo(_selectedItemId);
            if (itemInfo == null || weaponInfo == null)
                return;

            long count = wm.GetWeaponCount(_selectedItemId);
            int level = wm.GetWeaponLevel(_selectedItemId);
            int maxLevel = wm.GetMaxLevel(_selectedItemId);
            int awake = wm.GetAwakeLevel(_selectedItemId);
            int maxAwake = wm.GetMaxAwake(_selectedItemId);
            bool isMaxAwake = wm.IsMaxAwake(_selectedItemId);
            bool isEquipped = wm.IsEquipped(_selectedItemId);
            bool neverObtained = wm.GetWeaponData(_selectedItemId) == null;
            long requiredCount = isMaxAwake ? Define.WeaponCombineCount : wm.GetAwakeCost(_selectedItemId);
            long levelUpCost = wm.GetLevelUpCost(_selectedItemId);
            long levelUpCurrency = im.GetItemAmount(Define.WeaponLevelUpCostKey);

            if (_detailIcon != null) _detailIcon.sprite = im.GetIcon(_selectedItemId);
            if (_detailFrame != null) _detailFrame.sprite = StaticResource.Instance.GetRarityFrame(itemInfo.Rarity);
            if (_detailName != null) Managers.LocalStringManager.Instance.SetLocalizeText(_detailName, im.GetItemNameLocalKey(_selectedItemId));
            if (_detailRarity != null)
            {
                _detailRarity.SetText(itemInfo.Rarity.ToString());
                _detailRarity.color = StaticResource.Instance.GetRarityTextColor(itemInfo.Rarity);
            }
            if (_detailTier != null) _detailTier.SetText(itemInfo.Tier.ToString());
            if (_detailLevel != null) _detailLevel.SetText($"Lv.{level}/{maxLevel}");
            if (_detailAwake != null) _detailAwake.SetText($"{awake} Awake");
            RefreshAwakeImages(awake, maxAwake);

            if (_awakeNeedIcon != null)
                _awakeNeedIcon.sprite = im.GetIcon(_selectedItemId);

            if (_awakeNeedCountText != null)
                _awakeNeedCountText.SetText($"{count}/{requiredCount}");

            if (_levelUpNeedIcon != null)
                _levelUpNeedIcon.sprite = im.GetIcon(Define.WeaponLevelUpCostKey);

            if (_levelUpNeedCountText != null)
                _levelUpNeedCountText.SetText($"{levelUpCurrency}/{levelUpCost}");

            if (_detailEquipStats != null)
                _detailEquipStats.SetText(BuildEquipStatText(weaponInfo, level));

            if (_detailOwnStats != null)
                _detailOwnStats.SetText(BuildStatText(weaponInfo.GetOwnStats(), level));

            if (_awakeButton != null)
            {
                bool canAction = isMaxAwake ? wm.CanCombine(_selectedItemId) : wm.CanAwake(_selectedItemId);
                _awakeButton.interactable = neverObtained == false && canAction;
            }

            if (_awakeButtonText != null)
                _awakeButtonText.SetText(isMaxAwake ? "Combine" : "Awake");

            if (_levelUpButton != null)
                _levelUpButton.interactable = neverObtained == false && wm.IsMaxLevel(_selectedItemId) == false && levelUpCurrency >= levelUpCost;

            if (_levelUpButtonText != null)
                _levelUpButtonText.SetText("Level Up");

            if (_equipButton != null)
                _equipButton.interactable = neverObtained == false && isEquipped == false;

            if (_equipButtonText != null)
                _equipButtonText.SetText(isEquipped ? "Equipped" : "Equip");
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
        private string BuildStatText(IReadOnlyDictionary<Enum_Stat, double> stats, int level)
        {
            _sb.Clear();

            if (stats == null || stats.Count <= 0)
                return "-";

            double multiplier = 1.0 + (level - 1) * 0.1;
            var ordered = new List<KeyValuePair<Enum_Stat, double>>(stats);
            ordered.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (int i = 0; i < ordered.Count; ++i)
            {
                var kv = ordered[i];
                double value = kv.Value * multiplier;
                _sb.Append(StatHelper.GetStatDisplayName(kv.Key));
                _sb.Append(' ');
                _sb.Append(StatHelper.FormatStatDisplayValue(kv.Key, value));
                if (i < ordered.Count - 1)
                    _sb.Append('\n');
            }

            return _sb.ToString();
        }

        private string BuildEquipStatText(WeaponInfo weaponInfo, int level)
        {
            if (weaponInfo == null)
                return "-";

            var equipStats = weaponInfo.GetEquipStats();
            var equipBonusStats = weaponInfo.GetEquipBonusStats();

            if ((equipStats == null || equipStats.Count <= 0) &&
                (equipBonusStats == null || equipBonusStats.Count <= 0))
                return "-";

            Dictionary<Enum_Stat, double> merged = new Dictionary<Enum_Stat, double>();
            double multiplier = 1.0 + (level - 1) * 0.1;

            if (equipStats != null)
            {
                foreach (var kvp in equipStats)
                {
                    if (merged.ContainsKey(kvp.Key))
                        merged[kvp.Key] += kvp.Value * multiplier;
                    else
                        merged[kvp.Key] = kvp.Value * multiplier;
                }
            }

            if (equipBonusStats != null)
            {
                foreach (var kvp in equipBonusStats)
                {
                    if (merged.ContainsKey(kvp.Key))
                        merged[kvp.Key] += kvp.Value;
                    else
                        merged[kvp.Key] = kvp.Value;
                }
            }

            _sb.Clear();
            var ordered = new List<KeyValuePair<Enum_Stat, double>>(merged);
            ordered.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (int i = 0; i < ordered.Count; ++i)
            {
                var kv = ordered[i];
                _sb.Append(StatHelper.GetStatDisplayName(kv.Key));
                _sb.Append(' ');
                _sb.Append(StatHelper.FormatStatDisplayValue(kv.Key, kv.Value));
                if (i < ordered.Count - 1)
                    _sb.Append('\n');
            }

            return _sb.ToString();
        }

        private void OnClickAwakeOrCombine()
        {
            if (_selectedItemId <= 0 || WeaponManager.Instance == null)
                return;
            if (WeaponManager.Instance.GetWeaponData(_selectedItemId) == null)
                return;

            bool isMaxAwake = WeaponManager.Instance.IsMaxAwake(_selectedItemId);
            if (isMaxAwake)
                WeaponManager.Instance.DoCombine(_selectedItemId);
            else
                WeaponManager.Instance.DoAwake(_selectedItemId);
        }

        private void OnClickLevelUp()
        {
            if (_selectedItemId <= 0 || WeaponManager.Instance == null)
                return;
            if (WeaponManager.Instance.GetWeaponData(_selectedItemId) == null)
                return;

            WeaponManager.Instance.DoLevelUp(_selectedItemId);
        }

        private void OnClickEquip()
        {
            if (_selectedItemId <= 0 || WeaponManager.Instance == null)
                return;
            if (WeaponManager.Instance.GetWeaponData(_selectedItemId) == null)
                return;

            WeaponManager.Instance.SetEquip(_selectedItemId);
        }

        private void OnClickAllAwakeOrCombine()
        {
            WeaponManager wm = WeaponManager.Instance;
            if (wm == null)
                return;

            if (_batchActionIsAwake)
                wm.DoAllAwake();
            else
                wm.DoAllCombine();
        }
    }
}

