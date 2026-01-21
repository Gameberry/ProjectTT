using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;

namespace GameBerry.UI
{
    /// <summary>
    /// 각인 확률표 다이얼로그
    /// 스테이지별 티어/스탯 확률 정보 표시
    /// </summary>
    public class EngravingProbabilityDialog : IDialog
    {
        [Header("Stage Info")]
        [SerializeField] private TMP_Text _stageText;
        [SerializeField] private TMP_Text _tierText;
        [SerializeField] private Image _tierIndicator;

        [Header("Probability Table")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _probabilityContainer;
        [SerializeField] private UIEngravingProbabilityRowElement _rowPrefab;

        [Header("Matching Probability")]
        [SerializeField] private TMP_Text _matchingProbText;
        [SerializeField] private Image _matchingProbBackground;

        private readonly List<UIEngravingProbabilityRowElement> _rowInstances = new List<UIEngravingProbabilityRowElement>();
        private readonly Enum_Rarity[] _allTiers = { Enum_Rarity.Uncommon, Enum_Rarity.Rare, Enum_Rarity.Epic };

        //------------------------------------------------------------------------------------
        public void Init(int stageNumber)
        {
            Enum_Rarity currentTier = EngravingManager.Instance.GetSlotTierForStage(stageNumber);

            UpdateStageInfo(stageNumber, currentTier);
            UpdateProbabilityTable(stageNumber, currentTier);
            UpdateMatchingProbability(stageNumber);

            if (_scrollRect != null)
                _scrollRect.verticalNormalizedPosition = 1f;
        }
        //------------------------------------------------------------------------------------
        private void UpdateStageInfo(int stageNumber, Enum_Rarity currentTier)
        {
            if (_stageText != null)
                _stageText.SetText("{0}", stageNumber);

            if (_tierText != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(_tierText, currentTier.ToString());
                _tierText.color = StaticResource.Instance.GetRarityTextColor(currentTier);
            }

            if (_tierIndicator != null)
                _tierIndicator.color = StaticResource.Instance.GetRarityTextColor(currentTier);
        }
        //------------------------------------------------------------------------------------
        private void UpdateProbabilityTable(int stageNumber, Enum_Rarity currentTier)
        {
            // 기존 행 비활성화
            foreach (var row in _rowInstances)
            {
                if (row != null)
                    row.gameObject.SetActive(false);
            }

            int rowIndex = 0;
            var engravingChart = GameChart.Get<EngravingChart>();

            foreach (var tier in _allTiers)
            {
                var options = engravingChart.GetByStageAndTier(stageNumber, tier);
                var statGroups = GroupByStatType(options);

                foreach (var statGroup in statGroups)
                {
                    var row = GetOrCreateRow(rowIndex);
                    SetRowData(row, statGroup.Key, statGroup.Value, tier == currentTier);
                    rowIndex++;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private Dictionary<Enum_Stat, List<EngravingInfo>> GroupByStatType(EngravingInfo[] options)
        {
            var groups = new Dictionary<Enum_Stat, List<EngravingInfo>>();

            foreach (var option in options)
            {
                if (!groups.ContainsKey(option.StatType))
                    groups[option.StatType] = new List<EngravingInfo>();

                groups[option.StatType].Add(option);
            }

            return groups;
        }
        //------------------------------------------------------------------------------------
        private UIEngravingProbabilityRowElement GetOrCreateRow(int index)
        {
            if (index < _rowInstances.Count)
            {
                _rowInstances[index].gameObject.SetActive(true);
                return _rowInstances[index];
            }

            var row = Instantiate(_rowPrefab, _probabilityContainer);
            _rowInstances.Add(row);
            return row;
        }
        //------------------------------------------------------------------------------------
        private void SetRowData(UIEngravingProbabilityRowElement row, Enum_Stat statType, List<EngravingInfo> options, bool isTargetTier)
        {
            var uncommonOptions = options.Where(o => o.Grade == Enum_Rarity.Uncommon).ToList();
            var rareOptions = options.Where(o => o.Grade == Enum_Rarity.Rare).ToList();
            var epicOptions = options.Where(o => o.Grade == Enum_Rarity.Epic).ToList();

            string uncommonValue = uncommonOptions.Count > 0 ? GetValueString(uncommonOptions[0]) : "-";
            float uncommonProb = uncommonOptions.Sum(o => o.Probability);

            string rareValue = rareOptions.Count > 0 ? GetValueString(rareOptions[0]) : "-";
            float rareProb = rareOptions.Sum(o => o.Probability);

            string epicValue = epicOptions.Count > 0 ? GetValueString(epicOptions[0]) : "-";
            float epicProb = epicOptions.Sum(o => o.Probability);

            row.SetData(
                statType,
                uncommonValue, uncommonProb,
                rareValue, rareProb,
                epicValue, epicProb
            );

            row.SetTargetTier(isTargetTier);
        }
        //------------------------------------------------------------------------------------
        private void UpdateMatchingProbability(int stageNumber)
        {
            float matchingProb = GameChart.Get<EngravingMatchingChart>().GetMatchingRate(stageNumber);

            if (_matchingProbText != null)
                _matchingProbText.SetText("{0}%", matchingProb);

            if (_matchingProbBackground != null)
            {
                float intensity = matchingProb / 10f;
                _matchingProbBackground.color = Color.Lerp(Color.gray, Color.cyan, intensity);
            }
        }
        //------------------------------------------------------------------------------------
        private string GetValueString(EngravingInfo option)
        {
            if (option.MinValue == option.MaxValue)
                return StatHelper.FormatStatDisplayValue(option.StatType, option.MinValue);

            return $"{StatHelper.FormatStatDisplayValue(option.StatType, option.MinValue)}~{StatHelper.FormatStatDisplayValue(option.StatType, option.MaxValue)}";
        }
        //------------------------------------------------------------------------------------
    }
}
