using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;

namespace GameBerry.UI
{
    public class SummonDialog : IDialog
    {
        [Serializable]
        public class SummonTabConfig
        {
            public Enum_SummonType SummonType;
            public string Title;
            [TextArea] public string Description;
            public Sprite Icon;
            public bool Unlocked = true;
        }

        [Header("SummonTicket")]
        [SerializeField] private UIItemElement _summonTicketItem;

        [Header("Left Tabs")]
        [SerializeField] private Transform _tabRoot;
        [SerializeField] private UISummonTypeTabElement _tabPrefab;
        [SerializeField] private List<SummonTabConfig> _tabConfigs = new List<SummonTabConfig>();

        [Header("Center")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descText;
        [SerializeField] private TMP_Text _summonLevelText;
        [SerializeField] private TMP_Text _expText;
        [SerializeField] private Image _expFill;

        [Header("Reward Icon")]
        [SerializeField] private UIItemElement _rewardIconItem;
        [SerializeField] private TMP_Text _rewardLevelText;
        [SerializeField] private Button _claimRewardButton;
        [SerializeField] private TMP_Text _claimRewardButtonText;

        [Header("Summon Buttons")]
        [SerializeField] private Button _adSummonButton;
        [SerializeField] private TMP_Text _adSummonCountText;
        [SerializeField] private Image _oneCostIcon;
        [SerializeField] private TMP_Text _oneCostText;
        [SerializeField] private Button _oneSummonButton;
        [SerializeField] private Image _bulkCostIcon;
        [SerializeField] private TMP_Text _bulkCostText;
        [SerializeField] private Button _bulkSummonButton;

        [Header("Bulk Popup")]
        [SerializeField] private IDialog _bulkPopupRoot;
        [SerializeField] private Slider _bulkCountSlider;
        [SerializeField] private TMP_Text _bulkCountText;
        [SerializeField] private TMP_Text _bulkMinCountText;
        [SerializeField] private TMP_Text _bulkMaxCountText;
        [SerializeField] private Button _bulkMinusButton;
        [SerializeField] private Button _bulkPlusButton;
        [SerializeField] private Button _bulkMinButton;
        [SerializeField] private Button _bulkMaxButton;
        [SerializeField] private Image _bulkPopupCostIcon;
        [SerializeField] private TMP_Text _bulkPopupCostText;
        [SerializeField] private Button _bulkPopupConfirmButton;

        [Header("Summon Result")]
        [SerializeField] private IDialog _resultRoot;
        [SerializeField] private Transform _resultContentRoot;
        [SerializeField] private UIItemElement _resultItemPrefab;

        [SerializeField] private Button _openLevelRewardsPopupButton;

        private readonly List<UISummonTypeTabElement> _tabs = new List<UISummonTypeTabElement>();
        private readonly List<UIItemElement> _resultItems = new List<UIItemElement>();
        private Enum_SummonType _selectedType = Enum_SummonType.Weapon;

        private const int DefaultMinBulkCount = 10;
        private int _bulkCount = DefaultMinBulkCount;

        protected override void OnLoad()
        {
            if (_adSummonButton != null) _adSummonButton.onClick.AddListener(OnClickAdSummon);
            if (_oneSummonButton != null) _oneSummonButton.onClick.AddListener(() => DoPaidSummon(1));
            if (_bulkSummonButton != null) _bulkSummonButton.onClick.AddListener(OpenBulkPopup);
            if (_claimRewardButton != null) _claimRewardButton.onClick.AddListener(OnClickClaimReward);
            if (_openLevelRewardsPopupButton != null) _openLevelRewardsPopupButton.onClick.AddListener(OpenSummonInfoDialog);

            if (_bulkCountSlider != null) _bulkCountSlider.onValueChanged.AddListener(OnBulkSliderChanged);
            if (_bulkMinusButton != null) _bulkMinusButton.onClick.AddListener(() => ChangeBulkCount(-1));
            if (_bulkPlusButton != null) _bulkPlusButton.onClick.AddListener(() => ChangeBulkCount(1));
            if (_bulkMinButton != null) _bulkMinButton.onClick.AddListener(SetBulkToMin);
            if (_bulkMaxButton != null) _bulkMaxButton.onClick.AddListener(SetBulkToMax);
            if (_bulkPopupConfirmButton != null) _bulkPopupConfirmButton.onClick.AddListener(OnClickBulkSummonConfirm);

            BuildTabs();
            if (_resultRoot != null) _resultRoot.Load_Element();
            if (_bulkPopupRoot != null) _bulkPopupRoot.Load_Element();
        }

        protected override void OnEnter()
        {
            if (SummonManager.Instance != null)
                SummonManager.Instance.OnSummonStateChanged += OnSummonStateChanged;

            if (ItemManager.Instance != null)
                ItemManager.Instance.OnPointChanged += OnPointChanged;

            RefreshAll();
        }

        protected override void OnExit()
        {
            if (SummonManager.Instance != null)
                SummonManager.Instance.OnSummonStateChanged -= OnSummonStateChanged;

            if (ItemManager.Instance != null)
                ItemManager.Instance.OnPointChanged -= OnPointChanged;
        }

        private void BuildTabs()
        {
            for (int i = 0; i < _tabs.Count; ++i)
            {
                if (_tabs[i] != null)
                    Destroy(_tabs[i].gameObject);
            }
            _tabs.Clear();

            if (_tabRoot == null || _tabPrefab == null)
                return;

            for (int i = 0; i < _tabConfigs.Count; ++i)
            {
                SummonTabConfig cfg = _tabConfigs[i];
                UISummonTypeTabElement tab = Instantiate(_tabPrefab, _tabRoot);
                bool redDot = IsAnyRewardClaimable(cfg.SummonType);
                tab.Bind(cfg.SummonType, cfg.Title, cfg.Icon, cfg.Unlocked, cfg.SummonType == _selectedType, redDot, OnClickTab);
                _tabs.Add(tab);
            }
        }

        private void OnClickTab(Enum_SummonType summonType)
        {
            _selectedType = summonType;

            RefreshAll();
        }

        private void OnSummonStateChanged(Enum_SummonType summonType)
        {
            if (summonType != _selectedType)
                RefreshTabRedDots();

            RefreshAll();
        }

        private void OnPointChanged()
        {
            RefreshSummonButtons();
            RefreshBulkPopup();
        }

        private void RefreshAll()
        {
            RefreshTabVisual();
            RefreshMainInfo();
            RefreshRewardIcon();
            RefreshSummonButtons();
            RefreshBulkPopup();
        }

        private void RefreshTabVisual()
        {
            for (int i = 0; i < _tabs.Count; ++i)
            {
                if (_tabs[i] == null || i >= _tabConfigs.Count)
                    continue;

                bool selected = _tabConfigs[i].SummonType == _selectedType;
                _tabs[i].SetSelected(selected);
            }

            RefreshTabRedDots();

            SummonPriceInfo info = SummonManager.Instance.GetSummonPriceInfo(_selectedType);

            ItemHandle _handle = ItemHandle.ForStack(info.MainPoint);
            _summonTicketItem.RemoveEvent();
            _summonTicketItem.Bind(_handle);
            _summonTicketItem.AddEvent();
        }

        private void RefreshTabRedDots()
        {
            for (int i = 0; i < _tabs.Count; ++i)
            {
                if (_tabs[i] == null || i >= _tabConfigs.Count)
                    continue;

                _tabs[i].SetRedDot(IsAnyRewardClaimable(_tabConfigs[i].SummonType));
            }
        }

        private void RefreshMainInfo()
        {
            SummonManager sm = SummonManager.Instance;
            SummonTabConfig cfg = GetCurrentConfig();
            if (sm == null || cfg == null)
                return;

            if (_titleText != null) _titleText.SetText(string.IsNullOrEmpty(cfg.Title) ? cfg.SummonType.ToString() : cfg.Title);
            if (_descText != null) _descText.SetText(cfg.Description ?? string.Empty);

            int level = sm.GetSummonLevel(_selectedType);
            int exp = sm.GetSummonExp(_selectedType);
            int need = sm.GetExpToNextLevel(_selectedType);

            if (_summonLevelText != null) _summonLevelText.SetText($"Lv.{level}");
            if (_expText != null) _expText.SetText($"{exp}/{need}");
            if (_expFill != null)
                _expFill.fillAmount = need > 0 ? Mathf.Clamp01((float)exp / need) : 1f;
        }

        private void RefreshRewardIcon()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            bool hasInfo = sm.TryGetDisplayRewardInfo(_selectedType, out SummonDisplayRewardInfo info);
            if (_rewardIconItem != null)
            {
                _rewardIconItem.gameObject.SetActive(hasInfo);
                if (hasInfo)
                    _rewardIconItem.Bind(info.RewardItemHandle);
            }

            if (_rewardLevelText != null)
            {
                _rewardLevelText.gameObject.SetActive(hasInfo);
                if (hasInfo)
                    _rewardLevelText.SetText($"Lv.{info.RewardLevel}");
            }

            if (_claimRewardButton != null)
            {
                bool canClaim = hasInfo && info.IsClaimable;
                _claimRewardButton.gameObject.SetActive(canClaim);
                _claimRewardButton.interactable = canClaim;
            }

            if (_claimRewardButtonText != null)
                _claimRewardButtonText.SetText(hasInfo && info.IsClaimable ? "Claim" : "0/1");
        }

        private void RefreshSummonButtons()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            if (sm.TryGetCostPreview(_selectedType, 1, out SummonCostPreview onePreview))
            {
                if (_oneCostIcon != null) _oneCostIcon.sprite = ItemManager.Instance.GetIcon(onePreview.PointItemId);
                if (_oneCostText != null) _oneCostText.SetText(onePreview.TotalPrice.ToString());
                if (_oneSummonButton != null) _oneSummonButton.interactable = onePreview.IsAffordable;
            }
            else
            {
                if (_oneSummonButton != null) _oneSummonButton.interactable = false;
                if (_oneCostText != null) _oneCostText.SetText("0");
            }

            int bulkMinCount = GetBulkMinCount(sm);
            if (sm.TryGetCostPreview(_selectedType, bulkMinCount, out SummonCostPreview bulkPreview))
            {
                if (_bulkCostIcon != null) _bulkCostIcon.sprite = ItemManager.Instance.GetIcon(bulkPreview.PointItemId);
                if (_bulkCostText != null) _bulkCostText.SetText(bulkPreview.TotalPrice.ToString());
            }
            else if (_bulkCostText != null)
            {
                _bulkCostText.SetText("0");
            }

            int maxAffordableCount = GetBulkMaxCount(sm);
            if (_bulkSummonButton != null)
                _bulkSummonButton.interactable = maxAffordableCount > 0;

            int adRemain = sm.GetRemainDailyAdViewCount(_selectedType);
            int adLimit = sm.GetDailyAdViewLimit(_selectedType);
            if (_adSummonButton != null)
                _adSummonButton.interactable = adRemain > 0;

            if (_adSummonCountText != null)
                _adSummonCountText.SetText($"{adRemain}/{adLimit}");
        }

        private void DoPaidSummon(int count)
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null || count <= 0)
                return;

            if (sm.TrySummonWithPoint(_selectedType, count, out SummonResult result) == false)
                return;

            ShowResult(result);
        }

        private void OnClickAdSummon()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            UnityPlugins.appLovin.ShowRewardedAd(
                () =>
                {
                    if (sm.TryAdSummon(_selectedType, out SummonResult result) == false)
                        return;

                    ShowResult(result);
                },
                failReason =>
                {
                    Debug.LogWarning($"[SummonDialog] RewardedAd failed: {failReason}");
                });
        }

        private void ShowResult(SummonResult result)
        {
            Dictionary<int, long> itemToCount = new Dictionary<int, long>();
            for (int i = 0; i < result.DrawnItemIds.Count; ++i)
            {
                int itemId = result.DrawnItemIds[i];
                if (itemToCount.ContainsKey(itemId)) itemToCount[itemId] += 1;
                else itemToCount[itemId] = 1;
            }

            EnsureResultItemCount(itemToCount.Count);

            int idx = 0;
            foreach (var pair in itemToCount)
            {
                UIItemElement element = _resultItems[idx];
                element.gameObject.SetActive(true);
                element.Bind(ItemHandle.ForMeta(pair.Key, pair.Value));
                idx++;
            }

            for (int i = idx; i < _resultItems.Count; ++i)
                _resultItems[i].gameObject.SetActive(false);

            if (_resultRoot != null)
                _resultRoot.ElementEnter();
        }

        private void EnsureResultItemCount(int count)
        {
            if (_resultItemPrefab == null || _resultContentRoot == null)
                return;

            while (_resultItems.Count < count)
            {
                UIItemElement el = Instantiate(_resultItemPrefab, _resultContentRoot);
                _resultItems.Add(el);
            }
        }

        private void OnClickClaimReward()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            if (sm.TryGetDisplayRewardInfo(_selectedType, out SummonDisplayRewardInfo info) == false)
                return;

            if (info.IsClaimable == false)
                return;

            sm.TryClaimReward(_selectedType, info.RewardLevel, out _);
        }

        private void OpenBulkPopup()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            int max = GetBulkMaxCount(sm);
            int min = GetBulkMinCount(sm);
            _bulkCount = Mathf.Clamp(_bulkCount, min, Mathf.Max(min, max));

            if (_bulkCountSlider != null)
            {
                _bulkCountSlider.wholeNumbers = true;
                _bulkCountSlider.minValue = min;
                _bulkCountSlider.maxValue = Mathf.Max(min, max);
                _bulkCountSlider.SetValueWithoutNotify(_bulkCount);
            }

            if (_bulkPopupRoot != null)
                _bulkPopupRoot.ElementEnter();

            RefreshBulkPopup();
        }

        private void OnBulkSliderChanged(float value)
        {
            _bulkCount = Mathf.RoundToInt(value);
            RefreshBulkPopup();
        }

        private void ChangeBulkCount(int delta)
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            int max = GetBulkMaxCount(sm);
            int min = GetBulkMinCount(sm);
            _bulkCount = Mathf.Clamp(_bulkCount + delta, min, Mathf.Max(min, max));
            if (_bulkCountSlider != null)
                _bulkCountSlider.SetValueWithoutNotify(_bulkCount);

            RefreshBulkPopup();
        }

        private void SetBulkToMin()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            _bulkCount = GetBulkMinCount(sm);
            if (_bulkCountSlider != null)
                _bulkCountSlider.SetValueWithoutNotify(_bulkCount);

            RefreshBulkPopup();
        }

        private void SetBulkToMax()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            _bulkCount = Mathf.Max(GetBulkMinCount(sm), GetBulkMaxCount(sm));
            if (_bulkCountSlider != null)
                _bulkCountSlider.SetValueWithoutNotify(_bulkCount);

            RefreshBulkPopup();
        }

        private void OnClickBulkSummonConfirm()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            if (_bulkCount < GetBulkMinCount(sm))
                return;

            DoPaidSummon(_bulkCount);
            if (_bulkPopupRoot != null)
                _bulkPopupRoot.ElementExit(); // Close the popup after confirming the summon   
        }

        private void RefreshBulkPopup()
        {
            SummonManager sm = SummonManager.Instance;
            if (sm == null)
                return;

            int maxAffordableCount = Mathf.Max(0, GetBulkMaxCount(sm));
            int minBulkCount = GetBulkMinCount(sm);
            int sliderMax = Mathf.Max(minBulkCount, maxAffordableCount);
            _bulkCount = Mathf.Clamp(_bulkCount, minBulkCount, sliderMax);

            if (_bulkCountText != null) _bulkCountText.SetText(_bulkCount.ToString());
            if (_bulkMinCountText != null) _bulkMinCountText.SetText(minBulkCount.ToString());
            if (_bulkMaxCountText != null) _bulkMaxCountText.SetText(maxAffordableCount.ToString());

            if (_bulkCountSlider != null)
            {
                _bulkCountSlider.minValue = minBulkCount;
                _bulkCountSlider.maxValue = sliderMax;
                _bulkCountSlider.wholeNumbers = true;
                _bulkCountSlider.SetValueWithoutNotify(_bulkCount);
            }

            bool canBulk = maxAffordableCount >= minBulkCount && maxAffordableCount > 0;
            if (sm.TryGetCostPreview(_selectedType, _bulkCount, out SummonCostPreview preview))
            {
                if (_bulkPopupCostIcon != null) _bulkPopupCostIcon.sprite = ItemManager.Instance.GetIcon(preview.PointItemId);
                if (_bulkPopupCostText != null) _bulkPopupCostText.SetText(preview.TotalPrice.ToString());
                if (_bulkPopupConfirmButton != null) _bulkPopupConfirmButton.interactable = canBulk && preview.IsAffordable;
            }
            else
            {
                if (_bulkPopupCostText != null) _bulkPopupCostText.SetText("0");
                if (_bulkPopupConfirmButton != null) _bulkPopupConfirmButton.interactable = false;
            }
        }

        private int GetBulkMaxCount(SummonManager sm)
        {
            if (sm == null)
                return 0;

            return Mathf.Max(0, sm.GetMaxBulkSummonCount(_selectedType));
        }

        private int GetBulkMinCount(SummonManager sm)
        {
            int max = GetBulkMaxCount(sm);
            if (max <= 0)
                return DefaultMinBulkCount;

            return Mathf.Min(DefaultMinBulkCount, max);
        }

        private void OpenSummonInfoDialog()
        {
            UIManager.Instance.Load(nameof(SummonInfoDialog), ui =>
            {
                SummonInfoDialog dialog = ui as SummonInfoDialog;
                if (dialog == null)
                    return;

                dialog.Bind(_selectedType, openProbabilityTab: true);
                dialog.Enter();
            });
        }

        private SummonTabConfig GetCurrentConfig()
        {
            for (int i = 0; i < _tabConfigs.Count; ++i)
            {
                if (_tabConfigs[i].SummonType == _selectedType)
                    return _tabConfigs[i];
            }

            return _tabConfigs.Count > 0 ? _tabConfigs[0] : null;
        }

        private bool IsAnyRewardClaimable(Enum_SummonType summonType)
        {
            SummonManager sm = SummonManager.Instance;
            SummonLevelChart levelChart = GameChart.Get<SummonLevelChart>();
            if (sm == null || levelChart == null)
                return false;

            IReadOnlyList<SummonLevelInfo> infos = levelChart.GetInfos(summonType);
            if (infos == null)
                return false;

            int currentLevel = sm.GetSummonLevel(summonType);
            for (int i = 0; i < infos.Count; ++i)
            {
                SummonLevelInfo info = infos[i];
                if (info._RewardItemHandle.itemId <= 0)
                    continue;

                if (currentLevel >= info.SummonLevel && sm.IsRewardClaimed(summonType, info.SummonLevel) == false)
                    return true;
            }

            return false;
        }
    }
}
