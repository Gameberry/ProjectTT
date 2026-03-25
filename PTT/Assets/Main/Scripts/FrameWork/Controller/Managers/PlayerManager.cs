using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        public event Action OnLevelChanged;
        public event Action OnJobChanged;
        public event Action<double> OnExpChanged;

        private PlayerTable _playerTable;
        private PlayerLevelChart _levelChart;
        private PlayerJobChart _jobChart;

        private List<TableBase> _playerTables;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _playerTable = UserTable.Get<PlayerTable>();
            _levelChart = GameChart.Get<PlayerLevelChart>();
            _jobChart = GameChart.Get<PlayerJobChart>();

            _playerTables = new List<TableBase>()
            {
                _playerTable
            };
        }
        //------------------------------------------------------------------------------------
        #region Level & Exp
        //------------------------------------------------------------------------------------
        public int GetLevel() => _playerTable.GetLevel();
        public double GetExp() => _playerTable.GetExp();
        public int GetJobId() => _playerTable.GetJobId();
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 경험치 추가 및 레벨업 처리
        /// </summary>
        public bool AddExp(double amount, bool immediateServerUpdate = false)
        {
            if (amount <= 0)
                return false;

            int beforeLevel = GetLevel();
            double newTotalExp = GetExp() + amount;

            _playerTable.SetExp(newTotalExp, false);

            // 레벨 계산
            int newLevel = _levelChart.CalculateLevelFromExp(newTotalExp);
            int maxLevel = _levelChart.GetMaxLevel();
            newLevel = Mathf.Min(newLevel, maxLevel);

            bool leveledUp = newLevel > beforeLevel;

            if (leveledUp)
            {
                _playerTable.SetLevel(newLevel, false);
                OnLevelChanged?.Invoke();

                // 전직 가능 여부 체크
                CheckJobAvailable(newLevel);

                // 스탯 갱신
                RefreshStat();

                if (immediateServerUpdate == false)
                    immediateServerUpdate = true;
            }

            OnExpChanged?.Invoke(newTotalExp);

            if (immediateServerUpdate)
                UserTable.TransactionUpdate(_playerTables);
            else
                _playerTable.UpdateTable(false);

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 현재 레벨에서 다음 레벨까지 필요한 경험치
        /// </summary>
        public double GetExpToNextLevel()
        {
            int currentLevel = GetLevel();
            int maxLevel = _levelChart.GetMaxLevel();

            if (currentLevel >= maxLevel)
                return 0;

            if (!_levelChart.TryGetLevelInfo(currentLevel + 1, out PlayerLevelInfo nextInfo))
                return 0;

            double currentExp = GetExp();
            return Math.Max(0, nextInfo.RequiredExp - currentExp);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 현재 레벨 진행도 (0.0 ~ 1.0)
        /// </summary>
        public float GetExpProgress()
        {
            int currentLevel = GetLevel();
            int maxLevel = _levelChart.GetMaxLevel();

            if (currentLevel >= maxLevel)
                return 1f;

            if (!_levelChart.TryGetLevelInfo(currentLevel, out PlayerLevelInfo currentInfo))
                return 0f;

            if (!_levelChart.TryGetLevelInfo(currentLevel + 1, out PlayerLevelInfo nextInfo))
                return 1f;

            double currentExp = GetExp();
            double expInLevel = currentExp - currentInfo.RequiredExp;
            double expNeeded = nextInfo.RequiredExp - currentInfo.RequiredExp;

            if (expNeeded <= 0)
                return 1f;

            return (float)Math.Clamp(expInLevel / expNeeded, 0, 1);
        }
        //------------------------------------------------------------------------------------
        public float GetCurrentExpPercent()
        {
            return GetExpProgress() * 100f;
        }
        //------------------------------------------------------------------------------------
        public float GetExpPercentFromAmount(double amount)
        {
            if (amount <= 0)
                return 0f;

            int currentLevel = GetLevel();
            int maxLevel = _levelChart.GetMaxLevel();
            if (currentLevel >= maxLevel)
                return 0f;

            if (!_levelChart.TryGetLevelInfo(currentLevel, out PlayerLevelInfo currentInfo))
                return 0f;

            if (!_levelChart.TryGetLevelInfo(currentLevel + 1, out PlayerLevelInfo nextInfo))
                return 0f;

            double expNeeded = nextInfo.RequiredExp - currentInfo.RequiredExp;
            if (expNeeded <= 0)
                return 0f;

            return (float)((amount / expNeeded) * 100.0);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevel()
        {
            return GetLevel() >= _levelChart.GetMaxLevel();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Job (전직)
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 전직 가능 여부 확인
        /// </summary>
        public bool CanJobChange()
        {
            int currentJobId = GetJobId();
            int currentLevel = GetLevel();

            if (!_jobChart.TryGetNextJob(currentJobId, out PlayerJobInfo nextInfo))
                return false;

            return currentLevel >= nextInfo.RequiredLevel;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 전직 가능한 레벨 도달 시 알림용
        /// </summary>
        private void CheckJobAvailable(int newLevel)
        {
            if (_jobChart.TryGetJobByLevel(newLevel, out _))
            {
                Debug.Log($"[PlayerManager] Job change available at level {newLevel}!");
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 전직 실행
        /// </summary>
        public bool DoJobChange(bool immediateServerUpdate = true)
        {
            if (!CanJobChange())
                return false;

            int currentJobId = GetJobId();

            if (!_jobChart.TryGetNextJob(currentJobId, out PlayerJobInfo nextInfo))
                return false;

            _playerTable.SetJobId(nextInfo.JobId, false);

            if (immediateServerUpdate)
                UserTable.TransactionUpdate(_playerTables);
            else
                _playerTable.UpdateTable(false);

            OnJobChanged?.Invoke();
            RefreshStat();

            Debug.Log($"[PlayerManager] Job changed to {nextInfo.Name} (ID: {nextInfo.JobId})");

            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 다음 전직 정보
        /// </summary>
        public bool TryGetNextJobInfo(out PlayerJobInfo info)
        {
            int currentJobId = GetJobId();
            return _jobChart.TryGetNextJob(currentJobId, out info);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 현재 전직 정보
        /// </summary>
        public bool TryGetCurrentJobInfo(out PlayerJobInfo info)
        {
            int currentJobId = GetJobId();
            return _jobChart.TryGetJobInfo(currentJobId, out info);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxJob()
        {
            return GetJobId() >= _jobChart.GetMaxJobId();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Stats
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 레벨 + 전직 기반 스탯 계산
        /// </summary>
        public Dictionary<Enum_Stat, double> CalculatePlayerStats()
        {
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();

            int currentLevel = GetLevel();
            int currentJobId = GetJobId();

            // 레벨 기반 스탯 (레벨 1부터 현재까지 누적)
            for (int lv = 1; lv <= currentLevel; lv++)
            {
                if (_levelChart.TryGetLevelInfo(lv, out PlayerLevelInfo levelInfo))
                {
                    var levelStats = StatHelper.ParseStatsPacked(levelInfo.BaseStats);
                    foreach (var kvp in levelStats)
                    {
                        AddStat(totalStats, kvp.Key, kvp.Value);
                    }
                }
            }

            // 전직 보너스 스탯 (달성한 전직들 누적)
            for (int jobId = 1; jobId <= currentJobId; jobId++)
            {
                if (_jobChart.TryGetJobInfo(jobId, out PlayerJobInfo jobInfo))
                {
                    var jobStats = StatHelper.ParseStatsPacked(jobInfo.BonusStats);
                    foreach (var kvp in jobStats)
                    {
                        AddStat(totalStats, kvp.Key, kvp.Value);
                    }
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
        public void RefreshStat()
        {
            Dictionary<Enum_Stat, double> playerStats = CalculatePlayerStats();

            CharacterControllerBase player = Managers.BattleSceneManager.Instance?.GetPlayer();
            if (player != null)
            {
                ApplyPlayerStat(player.CharacterStatOperator, playerStats);
                player.RefreshStat(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void ApplyPlayerStat(CharacterStatOperator statOperator, Dictionary<Enum_Stat, double> stats)
        {
            statOperator.ClearPlayerStats();

            foreach (var kvp in stats)
            {
                statOperator.SetPlayerStat(kvp.Key, kvp.Value);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
