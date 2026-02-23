using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;

namespace GameBerry.UI
{
    public class SummonInfoDialog : IDialog
    {
        private struct ProbEntry
        {
            public int ItemId;
            public Enum_Tier Tier;
            public double Prob;
        }

        [Serializable]
        private enum InfoTab
        {
            Probability = 0,
            LevelReward = 1,
        }

        [Header("Tab")]
        [SerializeField] private UINumberBtn _probTabButton;
        [SerializeField] private UINumberBtn _rewardTabButton;
        [SerializeField] private GameObject _probRoot;
        [SerializeField] private GameObject _rewardRoot;

        [Header("Probability")]
        [SerializeField] private TMP_Text _probLevelText;
        [SerializeField] private Button _prevLevelButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Transform _probGroupRoot;
        [SerializeField] private UISummonProbabilityGroupElement _probGroupPrefab;

        [Header("Reward")]
        [SerializeField] private Transform _rewardContentRoot;
        [SerializeField] private UISummonLevelRewardElement _rewardElementPrefab;

        private readonly List<UISummonProbabilityGroupElement> _probGroups = new List<UISummonProbabilityGroupElement>();
        private readonly List<UISummonLevelRewardElement> _rewardRows = new List<UISummonLevelRewardElement>();
        private readonly List<int> _availableDrawLevels = new List<int>();

        private Enum_SummonType _summonType = Enum_SummonType.Weapon;
        private int _selectedDrawLevel = 1;
        private InfoTab _selectedTab = InfoTab.Probability;

        protected override void OnLoad()
        {
            if (_probTabButton != null)
            {
                _probTabButton.Num = (int)InfoTab.Probability;
                _probTabButton.AddListener += OnClickTabButton;
            }

            if (_rewardTabButton != null)
            {
                _rewardTabButton.Num = (int)InfoTab.LevelReward;
                _rewardTabButton.AddListener += OnClickTabButton;
            }

            if (_prevLevelButton != null) _prevLevelButton.onClick.AddListener(OnClickPrevLevel);
            if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(OnClickNextLevel);
        }

        protected override void OnEnter()
        {
            if (SummonManager.Instance != null)
                SummonManager.Instance.OnSummonStateChanged += OnSummonStateChanged;

            RefreshAll();
        }

        protected override void OnExit()
        {
            if (SummonManager.Instance != null)
                SummonManager.Instance.OnSummonStateChanged -= OnSummonStateChanged;
        }

        public void Bind(Enum_SummonType summonType, bool openProbabilityTab = true)
        {
            _summonType = summonType;
            BuildDrawLevelList();
            ResolveDefaultDrawLevel();
            SelectTab(openProbabilityTab ? InfoTab.Probability : InfoTab.LevelReward);
            RefreshAll();
        }

        private void OnSummonStateChanged(Enum_SummonType summonType)
        {
            if (summonType != _summonType && summonType != Enum_SummonType.Max)
                return;

            BuildDrawLevelList();
            ResolveDefaultDrawLevel();
            RefreshAll();
        }

        private void OnClickTabButton(int tabNum)
        {
            InfoTab tab = tabNum == (int)InfoTab.LevelReward ? InfoTab.LevelReward : InfoTab.Probability;
            SelectTab(tab);
        }

        private void SelectTab(InfoTab tab)
        {
            _selectedTab = tab;

            if (_probTabButton != null) _probTabButton.SetSelected(tab == InfoTab.Probability);
            if (_rewardTabButton != null) _rewardTabButton.SetSelected(tab == InfoTab.LevelReward);
            if (_probRoot != null) _probRoot.SetActive(tab == InfoTab.Probability);
            if (_rewardRoot != null) _rewardRoot.SetActive(tab == InfoTab.LevelReward);
        }

        private void BuildDrawLevelList()
        {
            _availableDrawLevels.Clear();

            SummonChart summonChart = GameChart.Get<SummonChart>();
            if (summonChart?.rows == null)
                return;

            HashSet<int> unique = new HashSet<int>();
            for (int i = 0; i < summonChart.rows.Length; ++i)
            {
                SummonInfo row = summonChart.rows[i];
                if (row.SummonType != _summonType)
                    continue;

                if (unique.Add(row.SummonLevel))
                    _availableDrawLevels.Add(row.SummonLevel);
            }

            _availableDrawLevels.Sort();
        }

        private void ResolveDefaultDrawLevel()
        {
            if (_availableDrawLevels.Count <= 0)
            {
                _selectedDrawLevel = 1;
                return;
            }

            int currentSummonLevel = SummonManager.Instance != null
                ? SummonManager.Instance.GetSummonLevel(_summonType)
                : _availableDrawLevels[0];

            int resolved = _availableDrawLevels[0];
            for (int i = 0; i < _availableDrawLevels.Count; ++i)
            {
                int level = _availableDrawLevels[i];
                if (level <= currentSummonLevel)
                    resolved = level;
            }

            if (_availableDrawLevels.Contains(_selectedDrawLevel))
                return;

            _selectedDrawLevel = resolved;
        }

        private void OnClickPrevLevel()
        {
            int idx = _availableDrawLevels.IndexOf(_selectedDrawLevel);
            if (idx <= 0)
                return;

            _selectedDrawLevel = _availableDrawLevels[idx - 1];
            RefreshProbabilityTab();
        }

        private void OnClickNextLevel()
        {
            int idx = _availableDrawLevels.IndexOf(_selectedDrawLevel);
            if (idx < 0 || idx >= _availableDrawLevels.Count - 1)
                return;

            _selectedDrawLevel = _availableDrawLevels[idx + 1];
            RefreshProbabilityTab();
        }

        private void RefreshAll()
        {
            RefreshProbabilityTab();
            RefreshRewardTab();
        }

        private void RefreshProbabilityTab()
        {
            if (_probLevelText != null)
                _probLevelText.SetText($"{_selectedDrawLevel} Lv Prob");

            int idx = _availableDrawLevels.IndexOf(_selectedDrawLevel);
            if (_prevLevelButton != null)
                _prevLevelButton.interactable = idx > 0;
            if (_nextLevelButton != null)
                _nextLevelButton.interactable = idx >= 0 && idx < _availableDrawLevels.Count - 1;

            SummonChart summonChart = GameChart.Get<SummonChart>();
            if (summonChart == null)
            {
                SetProbGroupActiveCount(0);
                return;
            }

            IReadOnlyList<SummonInfo> rows = summonChart.GetRows(_summonType, _selectedDrawLevel);
            if (rows == null || rows.Count <= 0)
            {
                SetProbGroupActiveCount(0);
                return;
            }

            var rarityMap = new Dictionary<Enum_Rarity, List<ProbEntry>>();
            double allProb = 0.0;

            for (int i = 0; i < rows.Count; ++i)
            {
                SummonInfo row = rows[i];
                if (row.Item <= 0 || row.Prob <= 0)
                    continue;

                ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(row.Item);
                if (itemInfo == null)
                    continue;

                allProb += row.Prob;

                if (rarityMap.TryGetValue(itemInfo.Rarity, out List<ProbEntry> list) == false)
                {
                    list = new List<ProbEntry>();
                    rarityMap.Add(itemInfo.Rarity, list);
                }

                list.Add(new ProbEntry
                {
                    ItemId = row.Item,
                    Tier = itemInfo.Tier,
                    Prob = row.Prob
                });
            }

            if (allProb <= 0.0)
            {
                SetProbGroupActiveCount(0);
                return;
            }

            List<Enum_Rarity> rarities = new List<Enum_Rarity>(rarityMap.Keys);
            rarities.Sort((a, b) => ((int)a).CompareTo((int)b));

            EnsureProbGroupCount(rarities.Count);

            for (int i = 0; i < rarities.Count; ++i)
            {
                Enum_Rarity rarity = rarities[i];
                List<ProbEntry> entries = rarityMap[rarity];
                entries.Sort((a, b) =>
                {
                    int tierCompare = ((int)a.Tier).CompareTo((int)b.Tier);
                    if (tierCompare != 0) return tierCompare;
                    return b.Prob.CompareTo(a.Prob);
                });

                double rarityProbSum = 0.0;
                for (int j = 0; j < entries.Count; ++j)
                    rarityProbSum += entries[j].Prob;

                float rarityPercent = (float)(rarityProbSum / allProb * 100.0);

                List<UISummonProbabilityGroupElement.ItemViewData> viewData = new List<UISummonProbabilityGroupElement.ItemViewData>(entries.Count);
                for (int j = 0; j < entries.Count; ++j)
                {
                    float inRarityPercent = rarityProbSum > 0.0
                        ? (float)(entries[j].Prob / rarityProbSum * 100.0)
                        : 0f;

                    viewData.Add(new UISummonProbabilityGroupElement.ItemViewData
                    {
                        ItemId = entries[j].ItemId,
                        Tier = entries[j].Tier,
                        PercentInRarity = inRarityPercent
                    });
                }

                _probGroups[i].gameObject.SetActive(true);
                _probGroups[i].Bind(rarity, rarityPercent, viewData);
            }

            for (int i = rarities.Count; i < _probGroups.Count; ++i)
                _probGroups[i].gameObject.SetActive(false);
        }

        private void RefreshRewardTab()
        {
            SummonLevelChart levelChart = GameChart.Get<SummonLevelChart>();
            SummonManager summonManager = SummonManager.Instance;
            if (levelChart == null || summonManager == null)
                return;

            IReadOnlyList<SummonLevelInfo> infos = levelChart.GetInfos(_summonType);
            if (infos == null)
            {
                SetRewardRowActiveCount(0);
                return;
            }

            EnsureRewardRowCount(infos.Count);
            int currentLevel = summonManager.GetSummonLevel(_summonType);

            for (int i = 0; i < infos.Count; ++i)
            {
                SummonLevelInfo info = infos[i];
                if (info._RewardItemHandle.itemId == 0)
                {
                    _rewardRows[i].gameObject.SetActive(false);
                    continue;
                }

                bool claimed = summonManager.IsRewardClaimed(_summonType, info.SummonLevel);
                bool claimable = currentLevel >= info.SummonLevel && claimed == false;

                _rewardRows[i].gameObject.SetActive(true);
                _rewardRows[i].Bind(info, claimable, claimed);
            }

            for (int i = infos.Count; i < _rewardRows.Count; ++i)
                _rewardRows[i].gameObject.SetActive(false);
        }

        private void EnsureProbGroupCount(int count)
        {
            if (_probGroupPrefab == null || _probGroupRoot == null)
                return;

            while (_probGroups.Count < count)
            {
                UISummonProbabilityGroupElement row = Instantiate(_probGroupPrefab, _probGroupRoot);
                _probGroups.Add(row);
            }
        }

        private void SetProbGroupActiveCount(int count)
        {
            for (int i = 0; i < _probGroups.Count; ++i)
                _probGroups[i].gameObject.SetActive(i < count);
        }

        private void EnsureRewardRowCount(int count)
        {
            if (_rewardElementPrefab == null || _rewardContentRoot == null)
                return;

            while (_rewardRows.Count < count)
            {
                UISummonLevelRewardElement row = Instantiate(_rewardElementPrefab, _rewardContentRoot);
                _rewardRows.Add(row);
            }
        }

        private void SetRewardRowActiveCount(int count)
        {
            for (int i = 0; i < _rewardRows.Count; ++i)
                _rewardRows[i].gameObject.SetActive(i < count);
        }
    }
}
