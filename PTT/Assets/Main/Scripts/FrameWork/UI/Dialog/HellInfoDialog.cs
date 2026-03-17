using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class HellInfoDialog : IDialog
    {
        [Header("Status")]
        [SerializeField] private TMP_Text _expText;
        [SerializeField] private Button _levelUpButton;
        [SerializeField] private TMP_Text _levelUpButtonText;
        [SerializeField] private TMP_Text _timerText;

        [Header("Probability")]
        [SerializeField] private TMP_Text _probLevelText;
        [SerializeField] private Button _prevLevelButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Transform _probGroupRoot;
        [SerializeField] private UISummonProbabilityGroupElement _probGroupPrefab;

        private readonly List<UISummonProbabilityGroupElement> _probGroups = new List<UISummonProbabilityGroupElement>();
        private readonly List<int> _availableLevels = new List<int>();
        private int _selectedLevel = 1;

        protected override void OnLoad()
        {
            if (_prevLevelButton != null) _prevLevelButton.onClick.AddListener(OnClickPrevLevel);
            if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(OnClickNextLevel);
            if (_levelUpButton != null) _levelUpButton.onClick.AddListener(OnClickLevelUp);
        }

        protected override void OnEnter()
        {
            if (HellManager.Instance != null)
                HellManager.Instance.OnHellStateChanged += OnHellStateChanged;

            RebuildLevelList();
            ResolveSelectedLevel(forceCurrentLevel: true);
            RefreshAll();
        }

        protected override void OnExit()
        {
            if (HellManager.Instance != null)
                HellManager.Instance.OnHellStateChanged -= OnHellStateChanged;
        }

        private void Update()
        {
            if (isEnter == false)
                return;

            if (HellManager.isAlive && HellManager.Instance.IsLevelUpInProgress())
                RefreshStatus();
        }

        private void OnHellStateChanged()
        {
            RebuildLevelList();
            ResolveSelectedLevel(forceCurrentLevel: false);
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshStatus();
            RefreshProbability();
        }

        private void RebuildLevelList()
        {
            _availableLevels.Clear();

            HellChart hellChart = GameChart.Get<HellChart>();
            if (hellChart?.rows == null)
                return;

            HashSet<int> unique = new HashSet<int>();
            for (int i = 0; i < hellChart.rows.Length; ++i)
            {
                HellInfo row = hellChart.rows[i];
                if (unique.Add(row.HellLevel))
                    _availableLevels.Add(row.HellLevel);
            }

            _availableLevels.Sort();
        }

        private void ResolveSelectedLevel(bool forceCurrentLevel)
        {
            if (_availableLevels.Count <= 0)
            {
                _selectedLevel = 1;
                return;
            }

            int currentLevel = HellManager.Instance != null
                ? HellManager.Instance.GetHellLevel()
                : _availableLevels[0];

            int resolved = _availableLevels[0];
            for (int i = 0; i < _availableLevels.Count; ++i)
            {
                int level = _availableLevels[i];
                if (level <= currentLevel)
                    resolved = level;
            }

            if (forceCurrentLevel || _availableLevels.Contains(_selectedLevel) == false)
            {
                _selectedLevel = resolved;
            }
        }

        private void OnClickPrevLevel()
        {
            int idx = _availableLevels.IndexOf(_selectedLevel);
            if (idx <= 0)
                return;

            _selectedLevel = _availableLevels[idx - 1];
            RefreshProbability();
        }

        private void OnClickNextLevel()
        {
            int idx = _availableLevels.IndexOf(_selectedLevel);
            if (idx < 0 || idx >= _availableLevels.Count - 1)
                return;

            _selectedLevel = _availableLevels[idx + 1];
            RefreshProbability();
        }

        private void RefreshProbability()
        {
            if (_probLevelText != null)
                _probLevelText.SetText($"Hell Lv.{_selectedLevel}");

            int idx = _availableLevels.IndexOf(_selectedLevel);
            if (_prevLevelButton != null)
                _prevLevelButton.interactable = idx > 0;
            if (_nextLevelButton != null)
                _nextLevelButton.interactable = idx >= 0 && idx < _availableLevels.Count - 1;

            HellChart hellChart = GameChart.Get<HellChart>();
            if (hellChart == null)
            {
                SetProbGroupActiveCount(0);
                return;
            }

            IReadOnlyList<HellInfo> rows = hellChart.GetRows(_selectedLevel);
            if (rows == null || rows.Count <= 0)
            {
                SetProbGroupActiveCount(0);
                return;
            }

            List<HellInfo> validRows = new List<HellInfo>();
            double totalProb = 0d;
            for (int i = 0; i < rows.Count; ++i)
            {
                HellInfo row = rows[i];
                if (row.Prob <= 0d)
                    continue;

                validRows.Add(row);
                totalProb += row.Prob;
            }

            if (validRows.Count <= 0 || totalProb <= 0d)
            {
                SetProbGroupActiveCount(0);
                return;
            }

            validRows.Sort((a, b) => ((int)b.Rarity).CompareTo((int)a.Rarity));
            EnsureProbGroupCount(validRows.Count);

            for (int i = 0; i < validRows.Count; ++i)
            {
                HellInfo info = validRows[i];
                _probGroups[i].gameObject.SetActive(true);
                _probGroups[i].Bind(
                    info.Rarity,
                    (float)(info.Prob / totalProb * 100d),
                    System.Array.Empty<UISummonProbabilityGroupElement.ItemViewData>());
            }

            for (int i = validRows.Count; i < _probGroups.Count; ++i)
                _probGroups[i].gameObject.SetActive(false);
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

        private void OnClickLevelUp()
        {
            if (HellManager.isAlive == false)
                return;

            HellManager.Instance.TryStartLevelUp();
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (HellManager.isAlive == false)
                return;

            int currentLevel = HellManager.Instance.GetHellLevel();
            int currentExp = HellManager.Instance.GetHellExp();
            int needExp = HellManager.Instance.GetExpToNextLevel();
            bool isMaxLevel = needExp <= 0;
            bool isLeveling = HellManager.Instance.IsLevelUpInProgress();
            bool canLevelUp = HellManager.Instance.CanStartLevelUp();

            if (_expText != null)
            {
                if (isMaxLevel)
                    _expText.SetText($"Lv.{currentLevel}  MAX");
                else
                    _expText.SetText($"Lv.{currentLevel}  {currentExp}/{needExp}");
            }

            if (_levelUpButton != null)
                _levelUpButton.interactable = canLevelUp;

            if (_levelUpButtonText != null)
            {
                if (isMaxLevel)
                    _levelUpButtonText.SetText("MAX");
                else if (isLeveling)
                    _levelUpButtonText.SetText("Leveling...");
                else if (canLevelUp)
                    _levelUpButtonText.SetText("Level Up");
                else
                    _levelUpButtonText.SetText("Need EXP");
            }

            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(isLeveling);
                if (isLeveling)
                {
                    int remain = HellManager.Instance.GetRemainingLevelUpSeconds();
                    if (TimeManager.isAlive)
                        _timerText.SetText($"Time Left  {TimeManager.Instance.GetSecendToDayString_MS(remain)}");
                    else
                        _timerText.SetText($"Time Left  {remain}s");
                }
            }
        }
    }
}
