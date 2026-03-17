using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class HellManager : Singleton<HellManager>
    {
        public event Action OnHellStateChanged;

        private readonly System.Random _random = new System.Random();

        private HellChart _hellChart;
        private HellLevelChart _hellLevelChart;
        private HellTable _hellTable;
        private EquipRarityRuleChart _equipRarityRuleChart;

        protected override void Init()
        {
            _hellChart = GameChart.Get<HellChart>();
            _hellLevelChart = GameChart.Get<HellLevelChart>();
            _hellTable = UserTable.Get<HellTable>();
            _equipRarityRuleChart = GameChart.Get<EquipRarityRuleChart>();
        }

        public int GetHellLevel()
        {
            TryCompletePendingLevelUp();

            if (_hellTable == null)
                return 1;

            return _hellTable.GetLevel();
        }

        public int GetHellExp()
        {
            TryCompletePendingLevelUp();

            if (_hellTable == null)
                return 0;

            return _hellTable.GetExp();
        }

        public int GetExpToNextLevel()
        {
            TryCompletePendingLevelUp();

            int level = GetHellLevel();
            if (_hellLevelChart != null && level >= _hellLevelChart.GetMaxLevel())
                return 0;

            if (_hellLevelChart != null && _hellLevelChart.TryGetInfo(level, out HellLevelInfo info))
                return Mathf.Max(0, info.Exp);

            return 0;
        }

        public bool IsLevelUpInProgress()
        {
            TryCompletePendingLevelUp();

            if (_hellTable == null)
                return false;

            return GetLevelUpEndTimeSecInternal() > GetCurrentTimeSec();
        }

        public int GetRemainingLevelUpSeconds()
        {
            TryCompletePendingLevelUp();

            long remain = GetLevelUpEndTimeSecInternal() - GetCurrentTimeSec();
            return remain > 0 ? (int)remain : 0;
        }

        public bool CanStartLevelUp()
        {
            TryCompletePendingLevelUp();

            if (_hellTable == null || _hellLevelChart == null)
                return false;

            int level = _hellTable.GetLevel();
            if (level >= _hellLevelChart.GetMaxLevel())
                return false;

            if (_hellLevelChart.TryGetInfo(level, out HellLevelInfo info) == false)
                return false;

            return _hellTable.GetExp() >= Mathf.Max(0, info.Exp) && GetLevelUpEndTimeSecInternal() <= 0;
        }

        public bool TryStartLevelUp(bool immediate = true)
        {
            TryCompletePendingLevelUp(immediate);

            if (CanStartLevelUp() == false)
                return false;

            int level = _hellTable.GetLevel();
            if (_hellLevelChart.TryGetInfo(level, out HellLevelInfo info) == false)
                return false;

            long durationSec = Math.Max(0L, info.LevelUpTimeSec);
            if (durationSec <= 0L)
            {
                CompleteLevelUp(immediate);
                return true;
            }

            _hellTable.SetState(level, _hellTable.GetExp(), GetCurrentTimeSec() + durationSec);
            _hellTable.UpdateTable(immediate);
            OnHellStateChanged?.Invoke();
            return true;
        }

        public Enum_Rarity GetRarity()
        {
            return GetRarity(GetHellLevel());
        }

        public Enum_Rarity GetRarity(int hellLevel)
        {
            if (_hellChart == null)
                return Enum_Rarity.Common;

            int drawLevel = ResolveDrawLevel(hellLevel);
            if (drawLevel <= 0)
                return Enum_Rarity.Common;

            IReadOnlyList<HellInfo> rows = _hellChart.GetRows(drawLevel);
            if (rows == null || rows.Count <= 0)
                return Enum_Rarity.Common;

            double totalProb = 0d;
            for (int i = 0; i < rows.Count; ++i)
            {
                if (rows[i].Prob > 0d)
                    totalProb += rows[i].Prob;
            }

            if (totalProb <= 0d)
                return rows[0].Rarity;

            double rand = _random.NextDouble() * totalProb;
            for (int i = 0; i < rows.Count; ++i)
            {
                double prob = Math.Max(0d, rows[i].Prob);
                if (rand < prob)
                    return rows[i].Rarity;

                rand -= prob;
            }

            return rows[rows.Count - 1].Rarity;
        }

        public int GetSalvagePoints(Enum_Rarity rarity)
        {
            if (_equipRarityRuleChart != null &&
                _equipRarityRuleChart.TryGetRandomRule(rarity, out EquipRandomRuleInfo rule))
            {
                return Mathf.Max(0, rule.SalvagePoints);
            }

            return 0;
        }

        public int AddExpByRarity(Enum_Rarity rarity, bool immediate = true)
        {
            return AddExp(GetSalvagePoints(rarity), immediate);
        }

        public int AddExp(int amount, bool immediate = true)
        {
            TryCompletePendingLevelUp(immediate);

            if (_hellTable == null || _hellLevelChart == null || amount <= 0)
                return 0;

            int level = GetHellLevel();
            if (level >= _hellLevelChart.GetMaxLevel())
                return 0;

            int exp = GetHellExp() + Mathf.Max(0, amount);
            _hellTable.SetState(level, exp, GetLevelUpEndTimeSecInternal());
            _hellTable.UpdateTable(immediate);
            OnHellStateChanged?.Invoke();
            return amount;
        }

        private int ResolveDrawLevel(int desiredLevel)
        {
            if (_hellChart?.rows == null)
                return 0;

            int resolved = 0;
            for (int i = 0; i < _hellChart.rows.Length; ++i)
            {
                HellInfo row = _hellChart.rows[i];
                if (row.HellLevel <= desiredLevel && row.HellLevel > resolved)
                    resolved = row.HellLevel;
            }

            return resolved;
        }

        private bool TryCompletePendingLevelUp(bool immediate = true)
        {
            if (_hellTable == null || _hellLevelChart == null)
                return false;

            long endTimeSec = GetLevelUpEndTimeSecInternal();
            if (endTimeSec <= 0 || endTimeSec > GetCurrentTimeSec())
                return false;

            CompleteLevelUp(immediate);
            return true;
        }

        private void CompleteLevelUp(bool immediate)
        {
            if (_hellTable == null || _hellLevelChart == null)
                return;

            int level = _hellTable.GetLevel();
            int exp = _hellTable.GetExp();
            int maxLevel = _hellLevelChart.GetMaxLevel();

            if (level >= maxLevel)
            {
                _hellTable.SetState(maxLevel, 0, 0);
                _hellTable.UpdateTable(immediate);
                OnHellStateChanged?.Invoke();
                return;
            }

            if (_hellLevelChart.TryGetInfo(level, out HellLevelInfo info) == false)
                return;

            int needExp = Mathf.Max(0, info.Exp);
            if (needExp > 0)
                exp = Mathf.Max(0, exp - needExp);

            level += 1;
            _hellTable.SetState(level, exp, 0);
            _hellTable.UpdateTable(immediate);
            OnHellStateChanged?.Invoke();
        }

        private long GetLevelUpEndTimeSecInternal()
        {
            if (_hellTable == null)
                return 0;

            return Math.Max(0L, _hellTable.GetLevelUpEndTimeSec());
        }

        private static long GetCurrentTimeSec()
        {
            if (Managers.TimeManager.isAlive)
                return (long)Math.Floor((double)Managers.TimeManager.Instance.Current_TimeStamp);

            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
