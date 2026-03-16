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
            if (_hellTable == null)
                return 1;

            return _hellTable.GetLevel();
        }

        public int GetHellExp()
        {
            if (_hellTable == null)
                return 0;

            return _hellTable.GetExp();
        }

        public int GetExpToNextLevel()
        {
            int level = GetHellLevel();
            if (_hellLevelChart != null && _hellLevelChart.TryGetInfo(level, out HellLevelInfo info))
                return Mathf.Max(0, info.Exp);

            return 0;
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
            if (_hellTable == null || _hellLevelChart == null || amount <= 0)
                return 0;

            int level = GetHellLevel();
            int exp = GetHellExp() + Mathf.Max(0, amount);

            TryLevelUp(ref level, ref exp);
            _hellTable.SetState(level, exp);
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

        private void TryLevelUp(ref int level, ref int exp)
        {
            while (true)
            {
                int maxLevel = _hellLevelChart.GetMaxLevel();
                if (level >= maxLevel)
                {
                    exp = Mathf.Max(0, exp);
                    return;
                }

                if (_hellLevelChart.TryGetInfo(level, out HellLevelInfo info) == false)
                    return;

                int needExp = Mathf.Max(0, info.Exp);
                if (needExp <= 0 || exp < needExp)
                    return;

                exp -= needExp;
                level += 1;
            }
        }
    }
}
