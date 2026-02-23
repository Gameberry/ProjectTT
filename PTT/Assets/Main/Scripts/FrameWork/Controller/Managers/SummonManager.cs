using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;
using GameBerry.Managers;

namespace GameBerry
{
    public struct SummonResult
    {
        public Enum_SummonType SummonType;
        public int DrawCount;
        public List<int> DrawnItemIds;
        public List<ItemHandle> LevelUpRewards;
        public int BeforeLevel;
        public int AfterLevel;
        public int CurrentExp;
        public int ExpToNext;
        public int UsedPointItemId;
        public long UsedPointAmount;
        public bool IsAdSummon;
    }

    public struct SummonDisplayRewardInfo
    {
        public bool IsValid;
        public bool IsClaimable;
        public int RewardLevel;
        public ItemHandle RewardItemHandle;
    }

    public struct SummonCostPreview
    {
        public bool IsValid;
        public bool IsAffordable;
        public bool UseMainPoint;
        public int PointItemId;
        public int UnitPrice;
        public long TotalPrice;
        public long CurrentPointAmount;
    }

    public class SummonManager : Singleton<SummonManager>
    {
        public event Action<Enum_SummonType> OnSummonStateChanged;

        private readonly List<TableBase> _summonTables = new List<TableBase>();
        private readonly System.Random _random = new System.Random();

        private SummonChart _summonChart;
        private SummonLevelChart _summonLevelChart;
        private SummonPriceChart _summonPriceChart;
        private SummonTable _summonTable;

        protected override void Init()
        {
            _summonChart = GameChart.Get<SummonChart>();
            _summonLevelChart = GameChart.Get<SummonLevelChart>();
            _summonPriceChart = GameChart.Get<SummonPriceChart>();
            _summonTable = UserTable.Get<SummonTable>();

            _summonTables.Clear();
            if (_summonTable != null)
                _summonTables.Add(_summonTable);

            if (_summonTable != null && TimeManager.isAlive)
            {
                bool changed = _summonTable.ConsumeDirtyAfterLoad();
                changed |= _summonTable.EnsureDailyReset(TimeManager.Instance.Current_TimeStamp, TimeManager.Instance.DailyInit_TimeStamp);
                if (changed)
                    UserTable.TransactionUpdate_WaitSecond(_summonTables);

                TimeManager.Instance.OnInitDailyContent -= OnInitDailyContent;
                TimeManager.Instance.OnInitDailyContent += OnInitDailyContent;
            }
        }

        public int GetSummonLevel(Enum_SummonType summonType)
        {
            if (_summonTable == null)
                return 1;

            return _summonTable.GetLevel(summonType);
        }

        public int GetSummonExp(Enum_SummonType summonType)
        {
            if (_summonTable == null)
                return 0;

            return _summonTable.GetExp(summonType);
        }

        public int GetExpToNextLevel(Enum_SummonType summonType)
        {
            int level = GetSummonLevel(summonType);
            if (_summonLevelChart != null &&
                _summonLevelChart.TryGetSummonLevelInfo(summonType, level, out SummonLevelInfo info))
            {
                return Mathf.Max(0, info.Exp);
            }

            return 0;
        }

        private void OnInitDailyContent(double nextDailyTimestamp)
        {
            if (_summonTable == null)
                return;

            _summonTable.OnDailyReset(nextDailyTimestamp);
            UserTable.TransactionUpdate_WaitSecond(_summonTables);
            OnSummonStateChanged?.Invoke(Enum_SummonType.Max);
        }

        public bool TrySummonWithPoint(Enum_SummonType summonType, int count, out SummonResult result)
        {
            result = default;
            if (count <= 0)
                return false;

            if (TryGetCostPreview(summonType, count, out SummonCostPreview preview) == false || preview.IsAffordable == false)
                return false;

            var consumeResult = ItemManager.Instance.ConsumeItem(preview.PointItemId, preview.TotalPrice, true);
            if (consumeResult.Success == false)
                return false;

            if (SummonInternal(summonType, count, out result) == false)
            {
                ItemManager.Instance.AddItem(preview.PointItemId, preview.TotalPrice, true);
                return false;
            }

            result.UsedPointItemId = preview.PointItemId;
            result.UsedPointAmount = preview.TotalPrice;
            result.IsAdSummon = false;
            return true;
        }

        public bool TryAdSummon(Enum_SummonType summonType, out SummonResult result)
        {
            result = default;

            int limit = GetDailyAdViewLimit(summonType);
            if (limit <= 0 || _summonTable == null)
                return false;

            if (_summonTable.TryConsumeDailyAdView(summonType, limit) == false)
                return false;

            if (SummonInternal(summonType, Define.SummonAdDrawCount, out result) == false)
            {
                int rollback = Math.Max(0, _summonTable.GetDailyAdViewCount(summonType) - 1);
                _summonTable.SetDailyAdViewCount(summonType, rollback);
                return false;
            }

            result.UsedPointItemId = 0;
            result.UsedPointAmount = 0;
            result.IsAdSummon = true;
            return true;
        }

        public bool Summon(Enum_SummonType summonType, int count, out SummonResult result)
        {
            return SummonInternal(summonType, count, out result);
        }

        private bool SummonInternal(Enum_SummonType summonType, int count, out SummonResult result)
        {
            result = default;

            if (_summonChart == null || _summonLevelChart == null || _summonTable == null || count <= 0)
                return false;

            int beforeLevel = GetSummonLevel(summonType);
            int currentLevel = beforeLevel;
            int currentExp = GetSummonExp(summonType);

            List<int> drawnItems = new List<int>(count);
            List<ItemHandle> rewards = new List<ItemHandle>();

            for (int i = 0; i < count; ++i)
            {
                int itemId = RollItemId(summonType, currentLevel);
                if (itemId <= 0)
                    continue;

                AddDrawnItem(itemId);
                drawnItems.Add(itemId);

                currentExp += 1;
                TryLevelUp(summonType, ref currentLevel, ref currentExp);
            }

            _summonTable.SetState(summonType, currentLevel, currentExp);
            UserTable.TransactionUpdate_WaitSecond(_summonTables);

            result = new SummonResult
            {
                SummonType = summonType,
                DrawCount = count,
                DrawnItemIds = drawnItems,
                LevelUpRewards = rewards,
                BeforeLevel = beforeLevel,
                AfterLevel = currentLevel,
                CurrentExp = currentExp,
                ExpToNext = GetExpToNextLevel(summonType),
                UsedPointItemId = 0,
                UsedPointAmount = 0,
                IsAdSummon = false
            };

            OnSummonStateChanged?.Invoke(summonType);
            return true;
        }

        public SummonPriceInfo GetSummonPriceInfo(Enum_SummonType summonType)
        {
            if (_summonPriceChart != null && _summonPriceChart.TryGetInfo(summonType, out SummonPriceInfo priceInfo))
                return priceInfo;

            return default;
        }

        public bool TryGetCostPreview(Enum_SummonType summonType, int count, out SummonCostPreview preview)
        {
            preview = default;
            if (count <= 0 || _summonPriceChart == null)
                return false;

            if (_summonPriceChart.TryGetInfo(summonType, out SummonPriceInfo priceInfo) == false)
                return false;

            long mainAmount = ItemManager.Instance.GetItemAmount(priceInfo.MainPoint);
            long subAmount = ItemManager.Instance.GetItemAmount(priceInfo.SubPoint);
            long mainNeed = (long)Math.Max(0, priceInfo.MainPointPrice) * count;
            long subNeed = (long)Math.Max(0, priceInfo.SubPointPrice) * count;

            bool mainAffordable = priceInfo.MainPoint > 0 && priceInfo.MainPointPrice > 0 && mainAmount >= mainNeed;
            bool subAffordable = priceInfo.SubPoint > 0 && priceInfo.SubPointPrice > 0 && subAmount >= subNeed;

            if (mainAffordable)
            {
                preview.IsValid = true;
                preview.IsAffordable = true;
                preview.UseMainPoint = true;
                preview.PointItemId = priceInfo.MainPoint;
                preview.UnitPrice = Math.Max(0, priceInfo.MainPointPrice);
                preview.TotalPrice = mainNeed;
                preview.CurrentPointAmount = mainAmount;
                return true;
            }

            if (subAffordable)
            {
                preview.IsValid = true;
                preview.IsAffordable = true;
                preview.UseMainPoint = false;
                preview.PointItemId = priceInfo.SubPoint;
                preview.UnitPrice = Math.Max(0, priceInfo.SubPointPrice);
                preview.TotalPrice = subNeed;
                preview.CurrentPointAmount = subAmount;
                return true;
            }

            bool hasMainPrice = priceInfo.MainPoint > 0 && priceInfo.MainPointPrice > 0;
            bool hasSubPrice = priceInfo.SubPoint > 0 && priceInfo.SubPointPrice > 0;

            if (hasMainPrice)
            {
                preview.IsValid = true;
                preview.IsAffordable = false;
                preview.UseMainPoint = true;
                preview.PointItemId = priceInfo.MainPoint;
                preview.UnitPrice = Math.Max(0, priceInfo.MainPointPrice);
                preview.TotalPrice = mainNeed;
                preview.CurrentPointAmount = mainAmount;
                return true;
            }

            if (hasSubPrice)
            {
                preview.IsValid = true;
                preview.IsAffordable = false;
                preview.UseMainPoint = false;
                preview.PointItemId = priceInfo.SubPoint;
                preview.UnitPrice = Math.Max(0, priceInfo.SubPointPrice);
                preview.TotalPrice = subNeed;
                preview.CurrentPointAmount = subAmount;
                return true;
            }

            return false;
        }

        public int GetMaxAffordableCount(Enum_SummonType summonType, int minRequiredCount = 1)
        {
            if (_summonPriceChart == null || _summonPriceChart.TryGetInfo(summonType, out SummonPriceInfo info) == false)
                return 0;

            long mainAmount = ItemManager.Instance.GetItemAmount(info.MainPoint);
            long subAmount = ItemManager.Instance.GetItemAmount(info.SubPoint);

            int mainCount = (info.MainPointPrice > 0 && info.MainPoint > 0) ? (int)(mainAmount / info.MainPointPrice) : 0;
            int subCount = (info.SubPointPrice > 0 && info.SubPoint > 0) ? (int)(subAmount / info.SubPointPrice) : 0;

            if (mainCount >= Math.Max(1, minRequiredCount))
                return mainCount;

            if (subCount >= Math.Max(1, minRequiredCount))
                return subCount;

            return Math.Max(mainCount, subCount);
        }

        public int GetDailyAdViewLimit(Enum_SummonType summonType)
        {
            if (_summonPriceChart == null || _summonPriceChart.TryGetInfo(summonType, out SummonPriceInfo info) == false)
                return 0;

            return Math.Max(0, info.DailyAdViewCount);
        }

        public int GetDailyAdViewCount(Enum_SummonType summonType)
        {
            if (_summonTable == null)
                return 0;

            return Math.Max(0, _summonTable.GetDailyAdViewCount(summonType));
        }

        public int GetRemainDailyAdViewCount(Enum_SummonType summonType)
        {
            int limit = GetDailyAdViewLimit(summonType);
            int used = GetDailyAdViewCount(summonType);
            return Math.Max(0, limit - used);
        }

        public int GetMaxBulkSummonCount(Enum_SummonType summonType)
        {
            int affordable = GetMaxAffordableCount(summonType, 1);
            if (affordable <= 0)
                return 0;

            if (_summonLevelChart == null)
                return affordable;

            int currentLevel = GetSummonLevel(summonType);
            int maxLevel = _summonLevelChart.GetMaxLevel(summonType);
            if (currentLevel >= maxLevel)
                return affordable;

            int need = GetExpToNextLevel(summonType);
            int exp = GetSummonExp(summonType);
            int remainToNextLevel = Math.Max(0, need - exp);

            if (remainToNextLevel <= 0)
                return affordable;

            return Math.Min(affordable, remainToNextLevel);
        }

        private int RollItemId(Enum_SummonType summonType, int summonLevel)
        {
            int drawLevel = ResolveDrawLevel(summonType, summonLevel);
            if (drawLevel <= 0)
                return 0;

            IReadOnlyList<SummonInfo> rows = _summonChart.GetRows(summonType, drawLevel);
            if (rows == null || rows.Count <= 0)
                return 0;

            double totalProb = 0;
            for (int i = 0; i < rows.Count; ++i)
            {
                if (rows[i].Prob > 0)
                    totalProb += rows[i].Prob;
            }

            if (totalProb <= 0)
                return rows[0].Item;

            double rand = _random.NextDouble() * totalProb;
            for (int i = 0; i < rows.Count; ++i)
            {
                double p = Math.Max(0, rows[i].Prob);
                if (rand < p)
                    return rows[i].Item;
                rand -= p;
            }

            return rows[rows.Count - 1].Item;
        }

        private int ResolveDrawLevel(Enum_SummonType summonType, int desiredLevel)
        {
            if (_summonChart == null || _summonChart.rows == null)
                return 0;

            int resolved = 0;
            for (int i = 0; i < _summonChart.rows.Length; ++i)
            {
                SummonInfo row = _summonChart.rows[i];
                if (row.SummonType != summonType)
                    continue;

                if (row.SummonLevel <= desiredLevel && row.SummonLevel > resolved)
                    resolved = row.SummonLevel;
            }

            return resolved;
        }

        private void AddDrawnItem(int itemId)
        {
            if (itemId <= 0)
                return;

            ItemManager.Instance.AddItem(itemId, 1);
        }

        private void TryLevelUp(Enum_SummonType summonType, ref int level, ref int exp)
        {
            while (true)
            {
                int maxLevel = _summonLevelChart.GetMaxLevel(summonType);
                if (level >= maxLevel)
                {
                    exp = Mathf.Max(0, exp);
                    return;
                }

                if (_summonLevelChart.TryGetSummonLevelInfo(summonType, level, out SummonLevelInfo currInfo) == false)
                    return;

                int needExp = Mathf.Max(0, currInfo.Exp);
                if (needExp <= 0 || exp < needExp)
                    return;

                exp -= needExp;
                level += 1;
            }
        }

        public bool IsRewardClaimed(Enum_SummonType summonType, int rewardLevel)
        {
            if (_summonTable == null)
                return false;

            return _summonTable.IsRewardClaimed(summonType, rewardLevel);
        }

        public bool CanClaimReward(Enum_SummonType summonType, int rewardLevel)
        {
            if (_summonLevelChart == null || _summonTable == null)
                return false;

            if (_summonLevelChart.TryGetSummonLevelInfo(summonType, rewardLevel, out SummonLevelInfo levelInfo) == false)
                return false;

            if (levelInfo._RewardItemHandle.itemId <= 0)
                return false;

            if (GetSummonLevel(summonType) < rewardLevel)
                return false;

            if (_summonTable.IsRewardClaimed(summonType, rewardLevel))
                return false;

            return true;
        }

        public bool TryClaimReward(Enum_SummonType summonType, int rewardLevel, out ItemHandle reward)
        {
            reward = default;

            if (CanClaimReward(summonType, rewardLevel) == false)
                return false;

            if (_summonLevelChart.TryGetSummonLevelInfo(summonType, rewardLevel, out SummonLevelInfo levelInfo) == false)
                return false;

            reward = levelInfo._RewardItemHandle;
            if (reward.itemId <= 0)
                return false;

            if (_summonTable.ClaimReward(summonType, rewardLevel) == false)
                return false;

            ItemManager.Instance.AddItem(reward.itemId, Math.Max(1, reward.metaAmount));
            UserTable.TransactionUpdate_WaitSecond(_summonTables);
            OnSummonStateChanged?.Invoke(summonType);
            return true;
        }

        public bool TryGetDisplayRewardInfo(Enum_SummonType summonType, out SummonDisplayRewardInfo info)
        {
            info = default;

            if (_summonLevelChart == null || _summonTable == null)
                return false;

            IReadOnlyList<SummonLevelInfo> infos = _summonLevelChart.GetInfos(summonType);
            if (infos == null || infos.Count <= 0)
                return false;

            int currentLevel = GetSummonLevel(summonType);

            // 1) Claimable reward first.
            for (int i = 0; i < infos.Count; ++i)
            {
                SummonLevelInfo levelInfo = infos[i];
                if (levelInfo._RewardItemHandle.itemId <= 0)
                    continue;

                if (currentLevel >= levelInfo.SummonLevel &&
                    _summonTable.IsRewardClaimed(summonType, levelInfo.SummonLevel) == false)
                {
                    info.IsValid = true;
                    info.IsClaimable = true;
                    info.RewardLevel = levelInfo.SummonLevel;
                    info.RewardItemHandle = levelInfo._RewardItemHandle;
                    return true;
                }
            }

            // 2) Next upcoming reward.
            for (int i = 0; i < infos.Count; ++i)
            {
                SummonLevelInfo levelInfo = infos[i];
                if (levelInfo._RewardItemHandle.itemId <= 0)
                    continue;

                if (currentLevel < levelInfo.SummonLevel)
                {
                    info.IsValid = true;
                    info.IsClaimable = false;
                    info.RewardLevel = levelInfo.SummonLevel;
                    info.RewardItemHandle = levelInfo._RewardItemHandle;
                    return true;
                }
            }

            return false;
        }
    }
}
