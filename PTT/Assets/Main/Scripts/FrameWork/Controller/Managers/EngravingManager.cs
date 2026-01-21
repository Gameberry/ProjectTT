using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class EngravingManager : Singleton<EngravingManager>
    {
        private List<Table.TableBase> EngravingTables = new List<Table.TableBase>()
            {
                Table.UserTable.Get<Table.EngravingTable>()
            };

        public event Action OnEngravingChanged;

        EngravingTable _engravingTable;
        EngravingChart _engravingChart;
        EngravingMatchingChart _engravingMatchingChart;
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _engravingTable = UserTable.Get<EngravingTable>();
            _engravingChart = GameChart.Get<EngravingChart>();
            _engravingMatchingChart = GameChart.Get<EngravingMatchingChart>();
        }
        //------------------------------------------------------------------------------------
        #region Data
        //------------------------------------------------------------------------------------
        public EngravingStageData GetEngraving(int stageNumber)
        {
            return _engravingTable.GetEngraving(stageNumber);
        }
        //------------------------------------------------------------------------------------
        public bool IsUnlocked(int stageNumber)
        {
            return _engravingTable.IsUnlocked(stageNumber);
        }
        //------------------------------------------------------------------------------------
        public Enum_Rarity GetSlotTierForStage(int stageNumber)
        {
            if (stageNumber == 1)
                return Enum_Rarity.Uncommon;

            if (_engravingTable.TryGetEngraving(stageNumber - 1, out var prevEngraving) == false)
                return Enum_Rarity.Uncommon;

            var lowestGrade = prevEngraving.GetLowestGrade();

            return lowestGrade switch
            {
                Enum_Rarity.Uncommon => Enum_Rarity.Uncommon,
                Enum_Rarity.Rare => Enum_Rarity.Rare,
                Enum_Rarity.Epic => Enum_Rarity.Epic,
                _ => Enum_Rarity.Uncommon
            };
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Stat
        //------------------------------------------------------------------------------------
        public void RefreshStat()
        {
            Dictionary<Enum_Stat, double> engravingStats = CalculateEngravingStats();

            CharacterControllerBase player = Managers.BattleSceneManager.Instance?.GetPlayer();
            if (player != null)
            {
                ApplyEngravingStats(player.CharacterStatOperator, engravingStats);
                player.RefreshStat(false);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<Enum_Stat, double> CalculateEngravingStats()
        {
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();

            foreach (var engraving in _engravingTable.GetAllEngravings())
            {
                if (!engraving.isUnlocked)
                    continue;

                foreach (var slot in engraving.slots)
                {
                    if (slot.IsEmpty)
                        continue;

                    AddStat(totalStats, slot.statType, slot.value);
                }
            }

            return totalStats;
        }
        //------------------------------------------------------------------------------------
        private void AddStat(Dictionary<Enum_Stat, double> stats, Enum_Stat stat, double value)
        {
            if (stats.ContainsKey(stat))
                stats[stat] += value;
            else
                stats[stat] = value;
        }
        //------------------------------------------------------------------------------------
        private void ApplyEngravingStats(CharacterStatOperator statOperator, Dictionary<Enum_Stat, double> engravingStats)
        {
            statOperator.ClearEngravingStats();

            foreach (var kvp in engravingStats)
            {
                statOperator.SetEngravingStat(kvp.Key, kvp.Value);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Roll
        //------------------------------------------------------------------------------------
        public int Roll(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > EngravingTable.MaxStage)
                return 0;

            if (_engravingTable.TryGetEngraving(stageNumber, out var engraving) == false)
                return 0;

            if (!engraving.isUnlocked)
                return 0;

            // 재화 체크
            if (ItemManager.Instance.ConsumeItem(1004, 1).Success == false)
                return 0;

            // 이후 단계 초기화
            ResetAfterStage(stageNumber);

            // 슬롯 티어 결정
            var tier = GetSlotTierForStage(stageNumber);

            // 매칭 확률 체크
            var matchingRate = _engravingMatchingChart.GetMatchingRate(stageNumber);
            var forceMatching = UnityEngine.Random.Range(0f, 100f) < matchingRate;

            if (forceMatching)
                RollMatchingSlots(stageNumber, tier);
            else
                RollNormalSlots(stageNumber, tier);

            // 다음 단계 해금 시도
            TryUnlockNextStage(stageNumber);

            // 저장
            _engravingTable.UpdateTable();
            OnEngravingChanged?.Invoke();

            RefreshStat();

            return 1;
        }
        //------------------------------------------------------------------------------------
        private void RollMatchingSlots(int stageNumber, Enum_Rarity tier)
        {
            var options = _engravingChart.GetByStageAndTier(stageNumber, tier);

            if (options.Length < 1)
                return;

            var statType = SelectRandomStat(options);
            var statOptions = options.Where(o => o.StatType == statType).ToArray();

            for (var i = 0; i < EngravingStageData.SlotCount; i++)
            {
                var selected = SelectOptionByProbability(statOptions);
                _engravingTable.SetSlot(stageNumber, i, selected.StatType, selected.Grade, GetRandomValue(selected));
            }
        }
        //------------------------------------------------------------------------------------
        private void RollNormalSlots(int stageNumber, Enum_Rarity tier)
        {
            var options = _engravingChart.GetByStageAndTier(stageNumber, tier);

            if (options.Length < 1)
                return;

            for (var i = 0; i < EngravingStageData.SlotCount; i++)
            {
                var selected = SelectOptionByProbability(options);
                _engravingTable.SetSlot(stageNumber, i, selected.StatType, selected.Grade, GetRandomValue(selected));
            }
        }
        //------------------------------------------------------------------------------------
        private Enum_Stat SelectRandomStat(EngravingInfo[] options)
        {
            var uniqueStats = options.Select(o => o.StatType).Distinct().ToArray();
            var statProbabilities = new Dictionary<Enum_Stat, float>();

            foreach (var stat in uniqueStats)
            {
                var totalProb = options.Where(o => o.StatType == stat).Sum(o => o.Probability);
                statProbabilities[stat] = totalProb;
            }

            var total = statProbabilities.Values.Sum();
            var random = UnityEngine.Random.Range(0f, total);
            var cumulative = 0f;

            foreach (var kvp in statProbabilities)
            {
                cumulative += kvp.Value;
                if (random <= cumulative)
                    return kvp.Key;
            }

            return uniqueStats[0];
        }
        //------------------------------------------------------------------------------------
        private EngravingInfo SelectOptionByProbability(EngravingInfo[] options)
        {
            var total = options.Sum(o => o.Probability);
            var random = UnityEngine.Random.Range(0f, total);
            var cumulative = 0f;

            foreach (var option in options)
            {
                cumulative += option.Probability;
                if (random <= cumulative)
                    return option;
            }

            return options[0];
        }
        //------------------------------------------------------------------------------------
        private float GetRandomValue(EngravingInfo option)
        {
            if (option.MinValue == option.MaxValue)
                return option.MinValue;

            bool isInteger = StatHelper.IsPercent(option.StatType) == false;

            if (isInteger)
            {
                return UnityEngine.Random.Range(
                    Mathf.RoundToInt(option.MinValue),
                    Mathf.RoundToInt(option.MaxValue) + 1);
            }

            return UnityEngine.Random.Range(option.MinValue, option.MaxValue);
        }
        //------------------------------------------------------------------------------------
        private void TryUnlockNextStage(int currentStage)
        {
            if (currentStage >= EngravingTable.MaxStage)
                return;

            if (_engravingTable.TryGetEngraving(currentStage, out var engraving) == false)
                return;

            if (engraving.HasMatchingStats())
            {
                _engravingTable.SetUnlocked(currentStage + 1, true);
            }
        }
        //------------------------------------------------------------------------------------
        private void ResetAfterStage(int stageNumber)
        {
            for (var i = stageNumber + 1; i <= EngravingTable.MaxStage; i++)
            {
                _engravingTable.ClearStage(i);
                _engravingTable.SetUnlocked(i, false);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Reset
        //------------------------------------------------------------------------------------
        public void ResetStage(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > EngravingTable.MaxStage)
                return;

            _engravingTable.ClearStage(stageNumber);
            _engravingTable.UpdateTable();
            OnEngravingChanged?.Invoke();

            RefreshStat();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}