using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;

namespace GameBerry.UI
{
    public class HellInfoDialog : IDialog
    {
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
        }

        protected override void OnEnter()
        {
            if (HellManager.Instance != null)
                HellManager.Instance.OnHellStateChanged += OnHellStateChanged;

            RebuildLevelList();
            ResolveSelectedLevel(forceCurrentLevel: true);
            RefreshProbability();
        }

        protected override void OnExit()
        {
            if (HellManager.Instance != null)
                HellManager.Instance.OnHellStateChanged -= OnHellStateChanged;
        }

        private void OnHellStateChanged()
        {
            RebuildLevelList();
            ResolveSelectedLevel(forceCurrentLevel: true);
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
    }
}
