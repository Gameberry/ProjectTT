using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class WeaponManager : Singleton<WeaponManager>
    {
        private List<Table.TableBase> WeaponTables = new List<Table.TableBase>()
        {
            Table.UserTable.Get<Table.WeaponTable>()
        };

        public event Action OnWeaponDataChanged;
        public event Action OnWeaponEquipChanged;

        [Obsolete("Use OnWeaponDataChanged instead.")]
        public event Action OnWeaponChanged
        {
            add => OnWeaponDataChanged += value;
            remove => OnWeaponDataChanged -= value;
        }

        WeaponTable _weaponTable;
        WeaponChart _weaponChart;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _weaponTable = UserTable.Get<WeaponTable>();
            _weaponChart = GameChart.Get<WeaponChart>();
        }
        //------------------------------------------------------------------------------------
        #region Data
        //------------------------------------------------------------------------------------
        public WeaponData GetWeaponData(int itemId)
        {
            return _weaponTable.GetWeaponData(itemId);
        }
        //------------------------------------------------------------------------------------
        public WeaponInfo GetWeaponInfo(int itemId)
        {
            return _weaponChart.Get(itemId);
        }
        //------------------------------------------------------------------------------------
        public bool HasWeapon(int itemId)
        {
            return GetWeaponData(itemId) != null;
        }
        //------------------------------------------------------------------------------------
        public long GetWeaponCount(int itemId)
        {
            return _weaponTable.GetAmount(itemId);
        }
        //------------------------------------------------------------------------------------
        public int GetEquippedWeaponId()
        {
            return _weaponTable.GetEquippedWeaponId();
        }
        //------------------------------------------------------------------------------------
        public bool TryGetEquippedWeaponData(out WeaponData weaponData)
        {
            int equippedId = GetEquippedWeaponId();
            if (equippedId <= 0)
            {
                weaponData = null;
                return false;
            }

            weaponData = GetWeaponData(equippedId);
            return weaponData != null;
        }
        //------------------------------------------------------------------------------------
        public List<WeaponData> GetAllWeaponData()
        {
            return _weaponTable.GetAllWeapons();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Equip
        //------------------------------------------------------------------------------------
        public bool SetEquip(int itemId)
        {
            WeaponInfo weaponInfo = _weaponChart.Get(itemId);
            if (weaponInfo == null)
                return false;

            if (HasWeapon(itemId) == false)
                return false;

            _weaponTable.SetEquipped(itemId);

            UserTable.TransactionUpdate_WaitSecond(WeaponTables);

            OnWeaponEquipChanged?.Invoke();

            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool IsEquipped(int itemId)
        {
            return GetEquippedWeaponId() == itemId;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Level
        //------------------------------------------------------------------------------------
        public int GetWeaponLevel(int itemId)
        {
            WeaponData data = GetWeaponData(itemId);
            return data?.level ?? 1;
        }
        //------------------------------------------------------------------------------------
        public int GetMaxLevel(int itemId)
        {
            WeaponInfo info = _weaponChart.Get(itemId);
            if (info == null)
                return 1;

            WeaponData data = GetWeaponData(itemId);
            int awakeLevel = data?.Awake ?? 0;

            return Define.WeaponBaseMaxLevel + (awakeLevel * Define.WeaponAwakeAddLevel);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevel(int itemId)
        {
            return GetWeaponLevel(itemId) >= GetMaxLevel(itemId);
        }
        //------------------------------------------------------------------------------------
        public long GetLevelUpCost(int itemId)
        {
            WeaponData data = GetWeaponData(itemId);
            if (data == null)
                return 0;

            if (IsMaxLevel(itemId))
                return 0;

            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);
            if (itemInfo == null)
                return 0;

            int nextLevel = Mathf.Max(1, data.level + 1);
            int rarityIndex = Mathf.Max(0, (int)itemInfo.Rarity - 1);
            int tierIndex = Mathf.Max(0, (int)itemInfo.Tier - 1);
            int awakeLevel = Mathf.Max(0, data.Awake);

            // Cost curve:
            // - nextLevel: quadratic growth
            // - rarity/tier: multiplicative weights
            // - awake: mild extra multiplier for expanded max-level range
            double levelCurve = 12.0 + (nextLevel * 2.2) + (nextLevel * nextLevel * 0.38);
            double rarityWeight = 1.0 + (rarityIndex * 0.30);
            double tierWeight = 1.0 + (tierIndex * 0.25);
            double awakeWeight = 1.0 + (awakeLevel * 0.12);

            long cost = (long)Math.Ceiling(levelCurve * rarityWeight * tierWeight * awakeWeight);
            return Math.Max(1, cost);
        }
        //------------------------------------------------------------------------------------
        public bool DoLevelUp(int itemId, bool immediate = true)
        {
            WeaponData data = GetWeaponData(itemId);
            if (data == null)
                return false;

            int maxLevel = GetMaxLevel(itemId);
            if (data.level >= maxLevel)
                return false;

            // TODO: 레벨업 비용 체크 및 소모
            long cost = GetLevelUpCost(itemId);
            if (ItemManager.Instance.GetItemAmount(Define.WeaponLevelUpCostKey) < cost)
                return false;
            ItemManager.Instance.ConsumeItem(Define.WeaponLevelUpCostKey, cost, false);

            _weaponTable.LevelUp(itemId);

            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(WeaponTables);

            OnWeaponDataChanged?.Invoke();

            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Awake
        //------------------------------------------------------------------------------------
        public int GetAwakeLevel(int itemId)
        {
            WeaponData data = GetWeaponData(itemId);
            return data?.Awake ?? 0;
        }
        //------------------------------------------------------------------------------------
        public int GetMaxAwake(int itemId)
        {
            WeaponInfo info = _weaponChart.Get(itemId);
            return info?.MaxAwake ?? 0;
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxAwake(int itemId)
        {
            return GetAwakeLevel(itemId) >= GetMaxAwake(itemId);
        }
        //------------------------------------------------------------------------------------
        public int GetAwakeCost(int itemId)
        {
            // 각성에 필요한 무기 수 : 현재 각성 레벨 + 1
            WeaponData data = GetWeaponData(itemId);
            if (data == null)
                return 1;

            return data.Awake + 1;
        }
        //------------------------------------------------------------------------------------
        public bool CanAwake(int itemId)
        {
            if (IsMaxAwake(itemId))
                return false;

            int cost = GetAwakeCost(itemId);
            long currentCount = GetWeaponCount(itemId);

            // 각성 비용만큼 보유하면 각성 가능 (count가 0이 되어도 보유 상태는 유지)
            return currentCount >= cost;
        }
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        public bool CanAnyAwake()
        {
            List<WeaponData> allWeapons = GetAllWeaponData();
            if (allWeapons == null)
                return false;

            for (int i = 0; i < allWeapons.Count; ++i)
            {
                WeaponData data = allWeapons[i];
                if (data == null)
                    continue;

                if (CanAwake(data.itemId))
                    return true;
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool DoAllAwake()
        {
            WeaponInfo[] allWeapons = _weaponChart.rows;
            if (allWeapons == null || allWeapons.Length <= 0)
                return false;

            bool changed = false;

            for (int i = 0; i < allWeapons.Length; ++i)
            {
                int itemId = allWeapons[i].ItemId;

                while (CanAwake(itemId))
                {
                    int cost = GetAwakeCost(itemId);
                    _weaponTable.Add(itemId, -cost);
                    _weaponTable.AwakeUp(itemId);
                    changed = true;
                }
            }

            if (changed == false)
                return false;

            UserTable.TransactionUpdate(WeaponTables);
            OnWeaponDataChanged?.Invoke();
            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoAwake(int itemId, bool immediate = true)
        {
            if (CanAwake(itemId) == false)
                return false;

            int cost = GetAwakeCost(itemId);

            // 무기 소모
            _weaponTable.Add(itemId, -cost);

            // 각성 레벨 증가
            _weaponTable.AwakeUp(itemId);

            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(WeaponTables);

            OnWeaponDataChanged?.Invoke();

            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Combine (합성)
        //------------------------------------------------------------------------------------
        public int GetNextWeaponId(int itemId)
        {
            // 예: 7101 -> 7102, 7104 -> 7201
            int lastDigit = itemId % 10;
            if (lastDigit < 4)
            {
                return itemId + 1;
            }
            else
            {
                // 7104 -> 7201, 7204 -> 7301 ...
                int tier = (itemId / 100) % 10;
                int nextTier = tier + 1;
                return (itemId / 1000) * 1000 + nextTier * 100 + 1;
            }
        }
        //------------------------------------------------------------------------------------
        public bool CanCombine(int itemId)
        {
            // 최대 각성 상태여야 합성 가능
            if (IsMaxAwake(itemId) == false)
                return false;

            // 합성에 필요한 무기 수
            long currentCount = GetWeaponCount(itemId);
            if (currentCount < Define.WeaponCombineCount)
                return false;

            // 다음 무기가 차트에 존재해야 함
            int nextId = GetNextWeaponId(itemId);
            WeaponInfo nextInfo = _weaponChart.Get(nextId);

            return nextInfo != null;
        }
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        public bool CanAnyCombine()
        {
            List<WeaponData> allWeapons = GetAllWeaponData();
            if (allWeapons == null)
                return false;

            for (int i = 0; i < allWeapons.Count; ++i)
            {
                WeaponData data = allWeapons[i];
                if (data == null)
                    continue;

                if (CanCombine(data.itemId))
                    return true;
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool DoAllCombine()
        {
            WeaponInfo[] allWeapons = _weaponChart.rows;
            if (allWeapons == null || allWeapons.Length <= 0)
                return false;

            bool changed = false;

            for (int i = 0; i < allWeapons.Length; ++i)
            {
                int itemId = allWeapons[i].ItemId;

                if (CanCombine(itemId))
                {
                    int nextId = GetNextWeaponId(itemId);

                    long currentCount = GetWeaponCount(itemId);
                    long nextAddCount = currentCount / Define.WeaponCombineCount;
                    _weaponTable.Add(itemId, -Define.WeaponCombineCount * nextAddCount);
                    _weaponTable.Add(nextId, nextAddCount);
                    changed = true;
                }
            }

            if (changed == false)
                return false;

            UserTable.TransactionUpdate(WeaponTables);
            OnWeaponDataChanged?.Invoke();
            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoCombine(int itemId, bool immediate = true)
        {
            if (CanCombine(itemId) == false)
                return false;

            int nextId = GetNextWeaponId(itemId);

            // 현재 무기 소모
            _weaponTable.Add(itemId, -Define.WeaponCombineCount);

            // 다음 무기 획득
            _weaponTable.Add(nextId, 1);

            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(WeaponTables);

            OnWeaponDataChanged?.Invoke();

            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Stat
        //------------------------------------------------------------------------------------
        public void RefreshStat()
        {
            Dictionary<Enum_Stat, double> ownStats = CalculateOwnStats();
            Dictionary<Enum_Stat, double> equipStats = CalculateEquipStats();

            // 전체 무기 스탯 = 보유 효과 + 장착 효과
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();

            foreach (var kvp in ownStats)
            {
                AddStat(totalStats, kvp.Key, kvp.Value);
            }

            foreach (var kvp in equipStats)
            {
                AddStat(totalStats, kvp.Key, kvp.Value);
            }

            // PlayerController에 무기 스탯 적용
            CharacterControllerBase player = Managers.BattleSceneManager.Instance?.GetPlayer();
            if (player != null)
            {
                ApplyWeaponStats(player.CharacterStatOperator, totalStats);
                player.RefreshStat(false);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<Enum_Stat, double> CalculateOwnStats()
        {
            // 보유 효과: 보유한 모든 무기의 OwnStat 합산
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();

            List<WeaponData> allWeapons = GetAllWeaponData();
            if (allWeapons == null)
                return totalStats;

            foreach (var weaponData in allWeapons)
            {
                if (weaponData == null)
                    continue;

                WeaponInfo info = _weaponChart.Get(weaponData.itemId);
                if (info == null)
                    continue;

                var ownStats = info.GetOwnStats();
                if (ownStats == null)
                    continue;

                // 레벨에 따른 보유 효과 배율 적용
                double levelMultiplier = GetLevelMultiplier(weaponData.level);

                foreach (var kvp in ownStats)
                {
                    double value = kvp.Value * levelMultiplier;
                    AddStat(totalStats, kvp.Key, value);
                }
            }

            return totalStats;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<Enum_Stat, double> CalculateEquipStats()
        {
            // 장착 효과:
            // - EquipStat: 레벨 배율 적용
            // - EquipBonusStat: 레벨 배율 미적용
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();

            int equippedId = GetEquippedWeaponId();
            if (equippedId <= 0)
                return totalStats;

            WeaponData weaponData = GetWeaponData(equippedId);
            if (weaponData == null)
                return totalStats;

            WeaponInfo info = _weaponChart.Get(equippedId);
            if (info == null)
                return totalStats;

            var equipStats = info.GetEquipStats();
            var equipBonusStats = info.GetEquipBonusStats();

            // 레벨에 따른 장착 효과 배율 적용
            double levelMultiplier = GetLevelMultiplier(weaponData.level);

            if (equipStats != null)
            {
                foreach (var kvp in equipStats)
                {
                    double value = kvp.Value * levelMultiplier;
                    AddStat(totalStats, kvp.Key, value);
                }
            }

            if (equipBonusStats != null)
            {
                foreach (var kvp in equipBonusStats)
                {
                    AddStat(totalStats, kvp.Key, kvp.Value);
                }
            }

            return totalStats;
        }
        //------------------------------------------------------------------------------------
        private double GetLevelMultiplier(int level)
        {
            // TODO: 레벨에 따른 배율 계산 로직 (차트나 Define에서 가져올 수 있음)
            // 예시: 레벨 1 = 1.0, 레벨 140 = 14.0 (레벨당 0.1 증가)
            return 1.0 + (level - 1) * 0.1;
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
        private void ApplyWeaponStats(CharacterStatOperator statOperator, Dictionary<Enum_Stat, double> weaponStats)
        {
            statOperator.ClearWeaponStats();

            foreach (var kvp in weaponStats)
            {
                statOperator.SetWeaponStat(kvp.Key, kvp.Value);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Add Weapon (획득)
        //------------------------------------------------------------------------------------
        public void ShowWeaponInventoryDialog()
        {
            UI.UIManager.Instance.DialogEnter<UI.WeaponInventoryDialog>();
        }
        //------------------------------------------------------------------------------------
        public bool AddWeapon(int itemId, long amount = 1)
        {
            WeaponInfo info = _weaponChart.Get(itemId);
            if (info == null)
                return false;

            _weaponTable.Add(itemId, amount);

            UserTable.TransactionUpdate_WaitSecond(WeaponTables);

            OnWeaponDataChanged?.Invoke();

            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
