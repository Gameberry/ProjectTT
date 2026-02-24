using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class LanternManager : Singleton<LanternManager>
    {
        private readonly List<TableBase> _lanternTables = new List<TableBase>()
        {
            UserTable.Get<LanternTable>()
        };

        public event Action OnLanternDataChanged;
        public event Action OnLanternEquipChanged;

        private LanternTable _lanternTable;
        private LanternChart _lanternChart;
        private LanternSlotChart _lanternSlotChart;
        private SkillChart _skillChart;
        private LanternController _activeLanternController;
        private GameObject _lanternControllerPrefab;
        private bool _isLanternControllerPrefabLoading;
        private readonly List<Action<GameObject>> _lanternPrefabLoadWaiters = new List<Action<GameObject>>();
        private const string LanternControllerPrefabPath = "BattleScene/LanternController";

        protected override void Init()
        {
            _lanternTable = UserTable.Get<LanternTable>();
            _lanternChart = GameChart.Get<LanternChart>();
            _lanternSlotChart = GameChart.Get<LanternSlotChart>();
            _skillChart = GameChart.Get<SkillChart>();
        }

        public LanternData GetLanternData(int itemId)
            => _lanternTable.GetLanternData(itemId);

        public LanternInfo GetLanternInfo(int itemId)
            => _lanternChart?.Get(itemId);

        public long GetLanternCount(int itemId)
            => _lanternTable.GetAmount(itemId);

        public List<LanternData> GetAllLanternData()
            => _lanternTable.GetAllLanterns();

        public LanternSlotInfo GetSlotInfo(Enum_LanternSlotType slotType)
            => _lanternSlotChart?.Get(slotType);

        public bool IsSlotUnlocked(Enum_LanternSlotType slotType)
        {
            LanternSlotInfo slotInfo = GetSlotInfo(slotType);
            if (slotInfo == null)
                return false;

            int summonLevel = 1;
            if (SummonManager.isAlive)
            {
                summonLevel = SummonManager.Instance.GetSummonLevel(Enum_SummonType.Lantern);
            }
            else
            {
                SummonTable summonTable = UserTable.Get<SummonTable>();
                if (summonTable != null)
                    summonLevel = summonTable.GetLevel(Enum_SummonType.Lantern);
            }

            return summonLevel >= Mathf.Max(1, slotInfo.UnLockSummonLevel);
        }

        public List<Enum_LanternSlotType> GetUnlockedSlots()
        {
            List<Enum_LanternSlotType> result = new List<Enum_LanternSlotType>();
            for (Enum_LanternSlotType t = Enum_LanternSlotType.Main; t < Enum_LanternSlotType.Max; ++t)
            {
                if (IsSlotUnlocked(t))
                    result.Add(t);
            }
            return result;
        }

        public int GetEquippedLanternId(Enum_LanternSlotType slotType)
            => _lanternTable.GetEquippedLanternId(slotType);

        public bool IsEquipped(int itemId)
            => _lanternTable.IsEquipped(itemId);

        public Enum_LanternSlotType GetEquippedSlotType(int itemId)
            => _lanternTable.FindSlotTypeByItemId(itemId);

        public int GetLanternLevel(int itemId)
        {
            LanternData data = _lanternTable.GetLanternData(itemId);
            return data?.level ?? 1;
        }

        public int GetMaxLevel(int itemId)
        {
            return Define.LanternBaseMaxLevel;
        }

        public bool IsMaxLevel(int itemId)
        {
            return GetLanternLevel(itemId) >= GetMaxLevel(itemId);
        }

        public int GetLevelUpCost(int itemId)
        {
            LanternData data = GetLanternData(itemId);
            if (data == null || IsMaxLevel(itemId))
                return 0;

            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);
            if (itemInfo == null)
                return 0;

            int nextLevel = Mathf.Max(1, data.level + 1);
            int rarityIndex = Mathf.Max(0, (int)itemInfo.Rarity - 1);
            int tierIndex = Mathf.Max(0, (int)itemInfo.Tier - 1);

            // Self-consume curve:
            // - nextLevel: accelerating growth
            // - rarity/tier: multiplicative weight
            double levelCurve = 1.0 + (nextLevel * 0.45) + (nextLevel * nextLevel * 0.06);
            double rarityWeight = 1.0 + (rarityIndex * 0.28);
            double tierWeight = 1.0 + (tierIndex * 0.22);

            int cost = Mathf.CeilToInt((float)(levelCurve * rarityWeight * tierWeight));
            return Mathf.Max(1, cost);
        }

        public bool CanLevelUp(int itemId)
        {
            LanternData data = GetLanternData(itemId);
            if (data == null)
                return false;

            if (IsMaxLevel(itemId))
                return false;

            int cost = GetLevelUpCost(itemId);
            return cost > 0 && data.count >= cost;
        }

        public int GetNextLanternId(int itemId)
        {
            // Ex) 7101 -> 7102, 7104 -> 7201
            int lastDigit = itemId % 10;
            if (lastDigit < 4)
                return itemId + 1;

            int tier = (itemId / 100) % 10;
            int nextTier = tier + 1;
            return (itemId / 1000) * 1000 + nextTier * 100 + 1;
        }

        public bool CanCombine(int itemId)
        {
            LanternData data = GetLanternData(itemId);
            if (data == null)
                return false;

            if (IsMaxLevel(itemId) == false)
                return false;

            if (data.count < Define.LanternCombineCount)
                return false;

            int nextId = GetNextLanternId(itemId);
            return _lanternChart?.Get(nextId) != null;
        }

        public int GetRequiredCountForGrowth(int itemId)
        {
            return IsMaxLevel(itemId) ? Define.LanternCombineCount : GetLevelUpCost(itemId);
        }

        public bool DoLevelUp(int itemId, bool immediate = true)
        {
            if (CanLevelUp(itemId) == false)
                return false;

            LanternData data = GetLanternData(itemId);
            int cost = GetLevelUpCost(itemId);

            data.count = Math.Max(0L, data.count - cost);
            data.level = Mathf.Min(GetMaxLevel(itemId), data.level + 1);

            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(_lanternTables);

            OnLanternDataChanged?.Invoke();
            RefreshStat();
            return true;
        }

        public bool DoCombine(int itemId, bool immediate = true)
        {
            if (CanCombine(itemId) == false)
                return false;

            int nextId = GetNextLanternId(itemId);
            _lanternTable.Add(itemId, -Define.LanternCombineCount);
            _lanternTable.Add(nextId, 1);

            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(_lanternTables);

            OnLanternDataChanged?.Invoke();
            RefreshStat();
            return true;
        }

        public bool SetEquip(Enum_LanternSlotType slotType, int itemId, bool immediate = true)
        {
            if (itemId <= 0)
                return false;
            if (IsSlotUnlocked(slotType) == false)
                return false;
            if (GetLanternData(itemId) == null)
                return false;
            if (_lanternChart?.Get(itemId) == null)
                return false;

            Enum_LanternSlotType equippedSlotType = _lanternTable.FindSlotTypeByItemId(itemId);
            if (equippedSlotType != Enum_LanternSlotType.Max && equippedSlotType != slotType)
            {
                _lanternTable.ClearEquip(equippedSlotType);
            }

            _lanternTable.SetEquipped(slotType, itemId);
            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(_lanternTables);

            OnLanternEquipChanged?.Invoke();
            RefreshStat();
            return true;
        }

        public bool UnEquip(Enum_LanternSlotType slotType, bool immediate = true)
        {
            if (_lanternTable.GetEquippedLanternId(slotType) <= 0)
                return false;

            _lanternTable.ClearEquip(slotType);
            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(_lanternTables);

            OnLanternEquipChanged?.Invoke();
            RefreshStat();
            return true;
        }

        public bool AutoEquip(bool immediate = true)
        {
            List<Enum_LanternSlotType> unlockedSlots = GetUnlockedSlots();
            if (unlockedSlots == null || unlockedSlots.Count <= 0)
                return false;

            List<LanternData> candidates = new List<LanternData>();
            List<LanternData> allLanterns = _lanternTable.GetAllLanterns();
            if (allLanterns == null)
                return false;

            for (int i = 0; i < allLanterns.Count; ++i)
            {
                LanternData data = allLanterns[i];
                if (data == null)
                    continue;
                if (_lanternChart.Get(data.itemId) == null)
                    continue;
                candidates.Add(data);
            }

            if (candidates.Count <= 0)
                return false;

            candidates.Sort(CompareLanternDataDesc);

            HashSet<int> usedItemIds = new HashSet<int>();
            bool changed = false;
            for (int i = 0; i < unlockedSlots.Count; ++i)
            {
                Enum_LanternSlotType slotType = unlockedSlots[i];
                int current = _lanternTable.GetEquippedLanternId(slotType);
                if (current > 0)
                    usedItemIds.Add(current);

                int pick = 0;
                for (int c = 0; c < candidates.Count; ++c)
                {
                    int itemId = candidates[c].itemId;
                    if (usedItemIds.Contains(itemId))
                        continue;
                    pick = itemId;
                    break;
                }

                if (pick <= 0 || current == pick)
                    continue;

                _lanternTable.SetEquipped(slotType, pick);
                usedItemIds.Add(pick);
                changed = true;
            }

            if (changed == false)
                return false;

            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(_lanternTables);

            OnLanternEquipChanged?.Invoke();
            RefreshStat();
            return true;
        }

        public bool DoAllLevelUp(bool immediate = true)
        {
            LanternInfo[] rows = _lanternChart?.rows;
            if (rows == null || rows.Length <= 0)
                return false;

            bool changed = false;
            for (int i = 0; i < rows.Length; ++i)
            {
                LanternInfo info = rows[i];
                if (info == null)
                    continue;

                int itemId = info.ItemId;

                while (CanLevelUp(itemId))
                {
                    LanternData data = GetLanternData(itemId);
                    if (data == null)
                        break;

                    int cost = GetLevelUpCost(itemId);
                    if (cost <= 0 || data.count < cost)
                        break;

                    data.count -= cost;
                    data.level = Mathf.Min(GetMaxLevel(itemId), data.level + 1);
                    changed = true;
                }

                while (CanCombine(itemId))
                {
                    int nextId = GetNextLanternId(itemId);
                    _lanternTable.Add(itemId, -Define.LanternCombineCount);
                    _lanternTable.Add(nextId, 1);
                    changed = true;
                }
            }

            if (changed == false)
                return false;

            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(_lanternTables);

            OnLanternDataChanged?.Invoke();
            RefreshStat();
            return true;
        }

        public int GetMainLanternId()
            => _lanternTable.GetEquippedLanternId(Enum_LanternSlotType.Main);

        public bool TryGetMainLanternSkillId(out int skillId)
        {
            skillId = 0;
            int mainLanternId = GetMainLanternId();
            if (mainLanternId <= 0)
                return false;

            LanternInfo lanternInfo = _lanternChart?.Get(mainLanternId);
            if (lanternInfo == null || lanternInfo.Skill <= 0)
                return false;

            skillId = lanternInfo.Skill;
            return true;
        }

        public SkillInfo GetMainLanternSkillInfo()
        {
            if (TryGetMainLanternSkillId(out int skillId) == false)
                return null;

            return _skillChart?.GetActive(skillId, Enum_SkillActorType.Lantern);
        }

        public void LoadLanternControllerPrefab(Action<GameObject> onLoaded)
        {
            if (onLoaded == null)
                return;

            if (_lanternControllerPrefab != null)
            {
                onLoaded.Invoke(_lanternControllerPrefab);
                return;
            }

            _lanternPrefabLoadWaiters.Add(onLoaded);
            if (_isLanternControllerPrefabLoading)
                return;

            _isLanternControllerPrefabLoading = true;
            ResourceLoader.Instance.Load<GameObject>(LanternControllerPrefabPath, o =>
            {
                _isLanternControllerPrefabLoading = false;
                _lanternControllerPrefab = o as GameObject;

                for (int i = 0; i < _lanternPrefabLoadWaiters.Count; ++i)
                {
                    _lanternPrefabLoadWaiters[i]?.Invoke(_lanternControllerPrefab);
                }

                _lanternPrefabLoadWaiters.Clear();
            });
        }

        public void CreateLanternController(PlayerController ownerPlayer, Transform parent, int lanternItemId, Action<LanternController> onCreated)
        {
            if (ownerPlayer == null)
            {
                onCreated?.Invoke(null);
                return;
            }

            LoadLanternControllerPrefab(prefab =>
            {
                GameObject clone = prefab != null
                    ? UnityEngine.Object.Instantiate(prefab, parent)
                    : new GameObject("LanternController");

                LanternController controller = clone.GetComponent<LanternController>();
                if (controller == null)
                    controller = clone.AddComponent<LanternController>();

                controller.Init();
                controller.Setup(ownerPlayer, lanternItemId);
                RegisterActiveLanternController(controller);

                onCreated?.Invoke(controller);
            });
        }

        public void RegisterActiveLanternController(LanternController controller)
        {
            _activeLanternController = controller;
        }

        public void UnregisterActiveLanternController(LanternController controller)
        {
            if (_activeLanternController == controller)
                _activeLanternController = null;
        }

        public LanternController GetActiveLanternController()
        {
            return _activeLanternController;
        }

        public void PlaySoulAbsorbEffect(Vector3 sourceWorldPos)
        {
            if (_activeLanternController == null)
                return;

            _activeLanternController.PlaySoulAbsorbFrom(sourceWorldPos);
        }

        public void AddLantern(int itemId, long amount = 1, bool immediate = true)
        {
            if (_lanternChart?.Get(itemId) == null)
                return;

            _lanternTable.Add(itemId, amount);
            if (immediate)
                UserTable.TransactionUpdate_WaitSecond(_lanternTables);

            OnLanternDataChanged?.Invoke();
            RefreshStat();
        }

        public Dictionary<Enum_Stat, double> CalculateOwnStats()
        {
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();
            List<LanternData> allLanterns = _lanternTable.GetAllLanterns();
            if (allLanterns == null)
                return totalStats;

            for (int i = 0; i < allLanterns.Count; ++i)
            {
                LanternData lanternData = allLanterns[i];
                if (lanternData == null)
                    continue;

                LanternInfo info = _lanternChart?.Get(lanternData.itemId);
                if (info == null)
                    continue;

                AddStats(totalStats, info.GetOwnStats(), GetLevelMultiplier(lanternData.level));
            }

            return totalStats;
        }

        public Dictionary<Enum_Stat, double> CalculateEquipStats()
        {
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();
            List<LanternEquipSlotData> slots = _lanternTable.GetAllEquipSlots();
            if (slots == null)
                return totalStats;

            for (int i = 0; i < slots.Count; ++i)
            {
                LanternEquipSlotData slot = slots[i];
                if (slot == null || slot.itemId <= 0)
                    continue;

                LanternData lanternData = _lanternTable.GetLanternData(slot.itemId);
                LanternInfo info = _lanternChart?.Get(slot.itemId);
                if (lanternData == null || info == null)
                    continue;

                AddStats(totalStats, info.GetEquipStats(), GetLevelMultiplier(lanternData.level));
            }

            return totalStats;
        }

        public void RefreshStat()
        {
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();
            MergeStats(totalStats, CalculateOwnStats());
            MergeStats(totalStats, CalculateEquipStats());

            CharacterControllerBase player = Managers.BattleSceneManager.Instance?.GetPlayer();
            if (player == null)
                return;

            player.CharacterStatOperator.ClearLanternStats();
            foreach (var kvp in totalStats)
            {
                player.CharacterStatOperator.SetLanternStat(kvp.Key, kvp.Value);
            }

            player.RefreshStat(false);
        }

        private static double GetLevelMultiplier(int level)
        {
            int setLevel = Mathf.Max(1, level);
            return 1.0 + (setLevel - 1) * 0.1;
        }

        private static void AddStats(Dictionary<Enum_Stat, double> dst, IReadOnlyDictionary<Enum_Stat, double> src, double multiplier)
        {
            if (src == null)
                return;

            foreach (var kvp in src)
            {
                if (dst.ContainsKey(kvp.Key))
                    dst[kvp.Key] += kvp.Value * multiplier;
                else
                    dst[kvp.Key] = kvp.Value * multiplier;
            }
        }

        private static void MergeStats(Dictionary<Enum_Stat, double> dst, Dictionary<Enum_Stat, double> src)
        {
            if (src == null)
                return;

            foreach (var kvp in src)
            {
                if (dst.ContainsKey(kvp.Key))
                    dst[kvp.Key] += kvp.Value;
                else
                    dst[kvp.Key] = kvp.Value;
            }
        }

        private static int CompareLanternDataDesc(LanternData a, LanternData b)
        {
            ItemInfo ai = ItemManager.Instance?.GetItemMeta(a.itemId);
            ItemInfo bi = ItemManager.Instance?.GetItemMeta(b.itemId);

            int ar = ai != null ? (int)ai.Rarity : 0;
            int br = bi != null ? (int)bi.Rarity : 0;
            if (ar != br) return br.CompareTo(ar);

            int at = ai != null ? (int)ai.Tier : 0;
            int bt = bi != null ? (int)bi.Tier : 0;
            if (at != bt) return bt.CompareTo(at);

            if (a.level != b.level) return b.level.CompareTo(a.level);
            if (a.count != b.count) return b.count.CompareTo(a.count);
            return b.itemId.CompareTo(a.itemId);
        }

        public void ShowLanternInventoryDialog()
        {
            UI.UIManager.Instance.DialogEnter<UI.LanternInventoryDialog>();
        }
    }
}
