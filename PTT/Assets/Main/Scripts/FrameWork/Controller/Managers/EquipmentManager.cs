using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public class EquipmentManager : Singleton<EquipmentManager>
    {
        private List<Table.TableBase> EquipTables = new List<Table.TableBase>()
            {
                Table.UserTable.Get<Table.EquipmentTable>()
            };

        private List<Table.TableBase> EquipSlotEnhanceTables = new List<Table.TableBase>()
            {
                Table.UserTable.Get<Table.EquipmentTable>(),
                Table.UserTable.Get<Table.PointTable>()
            };

        private List<Table.TableBase> EquipSalvageTables = new List<Table.TableBase>()
            {
                Table.UserTable.Get<Table.EquipmentTable>(),
                Table.UserTable.Get<Table.HellTable>()
            };

        public event Action OnEquipSlotChanged;

        EquipmentTable _equipmentTable;
        EquipChart _equipChart;
        EquipRarityRuleChart _equipRarityRuleChart;
        EquipStatRangeChart _equipStatRangeChart;
        EquipTypeRuleChart _equipTypeRuleChart;
        EquipSlotEnhanceChart _equipSlotEnhanceChart;

        private readonly System.Random _random = new System.Random();

        WeightedRandomPicker<Enum_StarforceResult> _starForcePicker = null;

        private const string AutoSalvageEnabledKey = "equipment.auto_salvage.enabled";
        private const string AutoSalvageThresholdKey = "equipment.auto_salvage.threshold";
        private const int AutoSalvageThresholdOff = 0;
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _equipmentTable = UserTable.Get<EquipmentTable>();
            _equipChart = GameChart.Get<EquipChart>();
            _equipRarityRuleChart = GameChart.Get<EquipRarityRuleChart>();
            _equipStatRangeChart = GameChart.Get<EquipStatRangeChart>();
            _equipTypeRuleChart = GameChart.Get<EquipTypeRuleChart>();
            _equipSlotEnhanceChart = GameChart.Get<EquipSlotEnhanceChart>();

        }
        //------------------------------------------------------------------------------------
        #region Data
        //------------------------------------------------------------------------------------
        public ItemHandle AddEquipment(int itemId, int level = -1)
        {
            int resolvedLevel = ResolveEquipmentLevel(level);
            Enum_Rarity rarity = ResolveEquipmentRarity(itemId);
            List<EquipmentAddStat> addStatList = CreateRandomOptionStats(itemId, resolvedLevel, rarity);
            return _equipmentTable.AddEquipment(itemId, resolvedLevel, rarity, addStatList);
        }
        //------------------------------------------------------------------------------------
        private List<EquipmentAddStat> CreateRandomOptionStats(int itemId, int level, Enum_Rarity rarity)
        {
            EquipInfo equipInfo = _equipChart.Get(itemId);

            if (equipInfo == null)
                return new List<EquipmentAddStat>();

            var result = new List<EquipmentAddStat>();

            if (_equipRarityRuleChart == null ||
                _equipRarityRuleChart.TryGetRandomRule(rarity, out EquipRandomRuleInfo equipRandomRuleInfo) == false)
                return result;

            if (_equipTypeRuleChart == null ||
                _equipTypeRuleChart.TryGetRule(equipInfo.EquipType, out EquipTypeRuleInfo typeRuleInfo) == false ||
                typeRuleInfo == null)
                return result;

            AddFixedStats(result, typeRuleInfo, level, rarity);

            int randomOptionCount = UnityEngine.Random.Range(equipRandomRuleInfo.RandomStatMin, equipRandomRuleInfo.RandomStatMax + 1);

            List<Enum_Stat> guaranteedRandomStats = BuildGuaranteedRandomStats(equipInfo);
            List<Enum_Stat> randomCandidates = BuildRandomStatCandidates(typeRuleInfo, guaranteedRandomStats);

            randomOptionCount = Mathf.Clamp(
                Mathf.Max(randomOptionCount, guaranteedRandomStats.Count),
                0,
                equipRandomRuleInfo.RandomStatMax);

            if (randomOptionCount <= 0)
                return result;

            if (guaranteedRandomStats.Count > randomOptionCount)
                guaranteedRandomStats = guaranteedRandomStats.GetRange(0, randomOptionCount);

            for (int i = 0; i < guaranteedRandomStats.Count; ++i)
                result.Add(EquipmentAddStat.Set(guaranteedRandomStats[i], GetRandomOptionStatValue(guaranteedRandomStats[i], level, rarity)));

            int remainingRandomCount = randomOptionCount - guaranteedRandomStats.Count;
            if (remainingRandomCount <= 0 || randomCandidates.Count <= 0)
                return result;

            remainingRandomCount = Mathf.Min(remainingRandomCount, randomCandidates.Count);

            for (int i = 0; i < remainingRandomCount; ++i)
            {
                int pickIndex = _random.Next(randomCandidates.Count);
                Enum_Stat stat = randomCandidates[pickIndex];
                randomCandidates.RemoveAt(pickIndex);

                double statValue = GetRandomOptionStatValue(stat, level, rarity);
                result.Add(EquipmentAddStat.Set(stat, statValue));
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        private void AddFixedStats(List<EquipmentAddStat> result, EquipTypeRuleInfo typeRuleInfo, int level, Enum_Rarity rarity)
        {
            if (result == null || typeRuleInfo?.FixedStatType == null)
                return;

            for (int i = 0; i < typeRuleInfo.FixedStatType.Length; ++i)
            {
                Enum_Stat stat = typeRuleInfo.FixedStatType[i];
                if (stat == Enum_Stat.Max)
                    continue;

                result.Add(EquipmentAddStat.Set(stat, GetRandomOptionStatValue(stat, level, rarity)));
            }
        }
        //------------------------------------------------------------------------------------
        private List<Enum_Stat> BuildGuaranteedRandomStats(EquipInfo equipInfo)
        {
            var result = new List<Enum_Stat>();
            if (equipInfo?.FixedRandomStat == null)
                return result;

            for (int i = 0; i < equipInfo.FixedRandomStat.Length; ++i)
            {
                Enum_Stat stat = equipInfo.FixedRandomStat[i];
                if (stat == Enum_Stat.Max || result.Contains(stat))
                    continue;

                result.Add(stat);
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        private List<Enum_Stat> BuildRandomStatCandidates(EquipTypeRuleInfo typeRuleInfo, List<Enum_Stat> guaranteedRandomStats)
        {
            var result = new List<Enum_Stat>();
            if (typeRuleInfo?.RandomStatType == null)
                return result;

            for (int i = 0; i < typeRuleInfo.RandomStatType.Length; ++i)
            {
                Enum_Stat stat = typeRuleInfo.RandomStatType[i];
                if (stat == Enum_Stat.Max || result.Contains(stat))
                    continue;

                if (guaranteedRandomStats != null && guaranteedRandomStats.Contains(stat))
                    continue;

                result.Add(stat);
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        private double GetRandomOptionStatValue(Enum_Stat stat, int level, Enum_Rarity rarity)
        {
            EquipStatRangeInfo rangeInfo = FindEquipStatRangeInfo(stat);
            if (rangeInfo == null)
                return 0d;

            if (level <= 0)
                level = 1;

            double rarityMultiplier = GetEquipStatRarityMultiplier(rangeInfo, rarity);
            double levelMultiplier = level * rangeInfo.LevelMultiple;
            double minValue = rangeInfo.Min * (1.0 + (rarityMultiplier * levelMultiplier));
            double maxValue = rangeInfo.Max * (1.0 + (rarityMultiplier * levelMultiplier));

            if (maxValue < minValue)
                maxValue = minValue;

            Enum_StatMode statMode = Enum_StatMode.Double;
            if (Enum.TryParse(rangeInfo.ValueMode, true, out Enum_StatMode parsedMode))
                statMode = parsedMode;

            if (statMode == Enum_StatMode.Int)
            {
                int minInt = Mathf.RoundToInt((float)minValue);
                int maxInt = Mathf.RoundToInt((float)maxValue);
                if (maxInt < minInt)
                    maxInt = minInt;

                return UnityEngine.Random.Range(minInt, maxInt + 1);
            }

            double randomValue = minValue + (_random.NextDouble() * (maxValue - minValue));
            return System.Math.Round(randomValue, 2);
        }
        //------------------------------------------------------------------------------------
        private EquipStatRangeInfo FindEquipStatRangeInfo(Enum_Stat stat)
        {
            if (_equipStatRangeChart?.rows == null)
                return null;

            string statName = stat.ToString();
            for (int i = 0; i < _equipStatRangeChart.rows.Length; ++i)
            {
                EquipStatRangeInfo info = _equipStatRangeChart.rows[i];
                if (info == null || string.IsNullOrEmpty(info.StatType))
                    continue;

                if (string.Equals(info.StatType, statName, StringComparison.OrdinalIgnoreCase))
                    return info;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        private int ResolveEquipmentLevel(int explicitLevel)
        {
            if (explicitLevel > 0)
                return explicitLevel;

            if (StageManager.Instance.TryGetCurrentStageInfo(out StageInfo stageInfo) && stageInfo != null)
            {
                int minLevel = Mathf.Max(1, stageInfo.EquipLevelMin);
                int maxLevel = Mathf.Max(minLevel, stageInfo.EquipLevelMax);
                return UnityEngine.Random.Range(minLevel, maxLevel + 1);
            }

            return 1;
        }
        //------------------------------------------------------------------------------------
        private double GetEquipStatRarityMultiplier(EquipStatRangeInfo rangeInfo, Enum_Rarity rarity)
        {
            return rarity switch
            {
                Enum_Rarity.Common => rangeInfo.Common,
                Enum_Rarity.Uncommon => rangeInfo.Uncommon,
                Enum_Rarity.Rare => rangeInfo.Rare,
                Enum_Rarity.Epic => rangeInfo.Epic,
                Enum_Rarity.Legendary => rangeInfo.Legendary,
                Enum_Rarity.Mythic => rangeInfo.Mythic,
                Enum_Rarity.Special => rangeInfo.Special,
                _ => 1d,
            };
        }
        //------------------------------------------------------------------------------------
        private Enum_Rarity ResolveEquipmentRarity(int itemId)
        {
            EquipInfo equipInfo = _equipChart?.Get(itemId);
            if (equipInfo == null)
                return Enum_Rarity.Common;

            if (equipInfo.FixedRarity > 0 && equipInfo.FixedRarity < Enum_Rarity.Max)
                return equipInfo.FixedRarity;

            return HellManager.Instance.GetRarity();
        }
        //------------------------------------------------------------------------------------
        public EquipmentData GetEquipmentData(ItemHandle itemHandle)
        {
            if (_equipmentTable.TryGetData(itemHandle.instanceId, out var data))
                return data;

            return null;
        }
        //------------------------------------------------------------------------------------
        public Enum_Rarity GetEquipmentRarity(ItemHandle itemHandle)
        {
            EquipmentData data = GetEquipmentData(itemHandle);
            if (data != null)
                return ResolveEquipmentDataRarity(data, itemHandle.itemId);

            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemHandle.itemId);
            return itemInfo?.Rarity ?? Enum_Rarity.Common;
        }
        //------------------------------------------------------------------------------------
        public bool GetAutoSalvageEnabled()
        {
            return PlayerPrefs.GetInt(AutoSalvageEnabledKey, 0) == 1;
        }
        //------------------------------------------------------------------------------------
        public void SetAutoSalvageEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(AutoSalvageEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        //------------------------------------------------------------------------------------
        public Enum_Rarity GetAutoSalvageThreshold()
        {
            int saved = PlayerPrefs.GetInt(AutoSalvageThresholdKey, AutoSalvageThresholdOff);
            if (saved <= AutoSalvageThresholdOff)
                return Enum_Rarity.Max;

            Enum_Rarity rarity = saved.IntToEnum32<Enum_Rarity>();
            if (rarity <= 0 || rarity >= Enum_Rarity.Max)
                return Enum_Rarity.Max;

            return rarity;
        }
        //------------------------------------------------------------------------------------
        public void SetAutoSalvageThreshold(Enum_Rarity rarity)
        {
            int saved = rarity == Enum_Rarity.Max ? AutoSalvageThresholdOff : rarity.Enum32ToInt();
            PlayerPrefs.SetInt(AutoSalvageThresholdKey, saved);
            PlayerPrefs.Save();
        }
        //------------------------------------------------------------------------------------
        public bool ShouldAutoSalvage(ItemHandle itemHandle)
        {
            if (GetAutoSalvageEnabled() == false || itemHandle.IsInstance == false)
                return false;

            Enum_Rarity threshold = GetAutoSalvageThreshold();
            if (threshold == Enum_Rarity.Max)
                return false;

            return GetEquipmentRarity(itemHandle) <= threshold;
        }
        //------------------------------------------------------------------------------------
        public bool TryAutoSalvage(ItemHandle itemHandle, bool immediate = true)
        {
            if (ShouldAutoSalvage(itemHandle) == false)
                return false;

            return TrySalvage(itemHandle, immediate, false);
        }
        //------------------------------------------------------------------------------------
        public int SalvageAllAtOrBelow(Enum_Rarity maxRarity, bool immediate = true)
        {
            if (maxRarity == Enum_Rarity.Max)
                return 0;

            List<EquipmentData> allEquipment = _equipmentTable.GetAllEquipmentData();
            if (allEquipment == null || allEquipment.Count <= 0)
                return 0;

            int salvagedCount = 0;

            for (int i = 0; i < allEquipment.Count; ++i)
            {
                EquipmentData data = allEquipment[i];
                if (data == null)
                    continue;

                if (_equipmentTable.IsEquipped(data.instanceId))
                    continue;

                Enum_Rarity rarity = ResolveEquipmentDataRarity(data, data.itemId);
                if (rarity > maxRarity)
                    continue;

                ItemHandle handle = ItemHandle.ForInstance(data.itemId, data.instanceId);
                if (TrySalvage(handle, false, false))
                    salvagedCount++;
            }

            if (salvagedCount <= 0)
                return 0;

            if (immediate)
                UserTable.TransactionUpdate(EquipSalvageTables);
            else
            {
                UserTable.Get<EquipmentTable>()?.UpdateTable(false);
                UserTable.Get<HellTable>()?.UpdateTable(false);
            }

            ItemManager.Instance?.NotifyStorageChanged(Enum_ItemStorageType.Equipment);
            return salvagedCount;
        }
        //------------------------------------------------------------------------------------
        private bool TrySalvage(ItemHandle itemHandle, bool immediate, bool notifyChange)
        {
            if (itemHandle.IsInstance == false)
                return false;

            EquipmentData data = GetEquipmentData(itemHandle);
            if (data == null || _equipmentTable.IsEquipped(itemHandle.instanceId))
                return false;

            Enum_Rarity rarity = ResolveEquipmentDataRarity(data, itemHandle.itemId);
            int salvagePoints = HellManager.Instance.GetSalvagePoints(rarity);

            if (_equipmentTable.RemoveEquipment(itemHandle.instanceId) == false)
                return false;

            if (salvagePoints > 0)
                HellManager.Instance.AddExp(salvagePoints, false);

            if (immediate)
                UserTable.TransactionUpdate(EquipSalvageTables);

            if (notifyChange)
                ItemManager.Instance?.NotifyStorageChanged(Enum_ItemStorageType.Equipment);

            return true;
        }
        //------------------------------------------------------------------------------------
        private Enum_Rarity ResolveEquipmentDataRarity(EquipmentData data, int fallbackItemId)
        {
            if (data != null && data.rarity > 0 && data.rarity < Enum_Rarity.Max)
                return data.rarity;

            int itemId = data != null && data.itemId > 0 ? data.itemId : fallbackItemId;
            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);
            return itemInfo?.Rarity ?? Enum_Rarity.Common;
        }
        //------------------------------------------------------------------------------------
        public bool TryGetInstanceIdToHandle(int instanceId, out ItemHandle itemHandle)
        {
            EquipmentData data = _equipmentTable.GetEquipmentData(instanceId);

            if (data == null)
            {
                itemHandle = default;
                return false;
            }

            itemHandle = ItemHandle.ForInstance(data.itemId, data.instanceId);

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool TryGetEquippedHandle(Enum_EquipType enum_EquipType, out ItemHandle itemHandle)
        {
            int instanceId = _equipmentTable.GetEquippedInstanceId(enum_EquipType);
            if (instanceId <= 0)
            {
                itemHandle = default;
                return false;
            }

            return TryGetInstanceIdToHandle(instanceId, out itemHandle);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Stat
        //------------------------------------------------------------------------------------
        public void RefreshStat()
        {
            Dictionary<Enum_Stat, double> equipStats = CalculateEquipmentStats();

            // PlayerController에 장비 스탯 적용
            CharacterControllerBase player = Managers.BattleSceneManager.Instance?.GetPlayer();
            if (player != null)
            {
                ApplyEquipmentStats(player.CharacterStatOperator, equipStats);
                player.RefreshStat(false);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<Enum_Stat, double> CalculateEquipmentStats()
        {
            Dictionary<Enum_Stat, double> totalStats = new Dictionary<Enum_Stat, double>();

            foreach (Enum_EquipType equipType in System.Enum.GetValues(typeof(Enum_EquipType)))
            {
                if (equipType == Enum_EquipType.Max)
                    continue;

                if (!TryGetEquippedHandle(equipType, out ItemHandle itemHandle))
                    continue;

                EquipInfo equipInfo = _equipChart.Get(itemHandle.itemId);
                if (equipInfo == null)
                    continue;

                EquipmentData equipmentData = GetEquipmentData(itemHandle);

                int slotLevel = GetStarforceLevel(equipType);

                if (!_equipSlotEnhanceChart.TryGetEquipSlotEnhanceInfo(slotLevel, out EquipSlotEnhanceInfo enhanceInfo))
                    continue;

                // 1. 기본 스탯 (MainStatPer 적용)
                var baseStats = equipInfo.GetBaseStats();
                foreach (var kvp in baseStats)
                {
                    double value = kvp.Value * (1.0 + enhanceInfo.MainStatPer);
                    AddStat(totalStats, kvp.Key, value);
                }

                // 2. 랜덤 옵션 스탯 (SubStatPer 적용)
                if (equipmentData?.addStatList != null)
                {
                    for (int i = 0; i < equipmentData.addStatList.Count; i++)
                    {
                        var addStat = equipmentData.addStatList[i];
                        double value = addStat.value * (1.0 + enhanceInfo.SubStatPer);
                        AddStat(totalStats, addStat.stat, value);
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
        private void ApplyEquipmentStats(CharacterStatOperator statOperator, Dictionary<Enum_Stat, double> equipStats)
        {
            statOperator.ClearEquipmentStats();

            foreach (var kvp in equipStats)
            {
                statOperator.SetEquipmentStat(kvp.Key, kvp.Value);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Slot Equip
        //------------------------------------------------------------------------------------
        public bool IsEquip(ItemHandle itemHandle)
        {
            if (!itemHandle.IsInstance)
                return false;

            return _equipmentTable.IsEquipped(itemHandle.instanceId);
        }
        //------------------------------------------------------------------------------------
        public bool SetEquip(ItemHandle itemHandle)
        {
            EquipInfo equipInfo = _equipChart.Get(itemHandle.itemId);

            if (equipInfo == null)
                return false;

            if (!itemHandle.IsInstance)
                return false;

            _equipmentTable.SetEquipped(equipInfo.EquipType, itemHandle.instanceId);
            OnEquipSlotChanged?.Invoke();

            UserTable.TransactionUpdate_WaitSecond(EquipTables);

            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Slot Starforce
        //------------------------------------------------------------------------------------
        public int GetStarforceLevel(Enum_EquipType enum_EquipType)
        {
            return _equipmentTable.GetStarforceLevel(enum_EquipType);
        }
        //------------------------------------------------------------------------------------
        public bool IsDestroyStarforce(Enum_EquipType enum_EquipType)
        {
            return _equipmentTable.IsDestroyStarforce(enum_EquipType);
        }
        //------------------------------------------------------------------------------------
        public bool DoStarforceRestoration(GameBerry.Enum_EquipType enum_EquipType)
        { // 파괴 복구
            if (IsDestroyStarforce(enum_EquipType) == false)
                return false;

            if (ItemManager.Instance.GetItemAmount(Define.StarforceRestoration1_Key) < Define.StarforceRestoration1_Price)
                return false;

            if (ItemManager.Instance.GetItemAmount(Define.StarforceRestoration2_Key) < Define.StarforceRestoration2_Price)
                return false;

            if (_equipmentTable.DoSlotRestoration(enum_EquipType) == false)
                return false;


            ItemManager.Instance.ConsumeItem(Define.StarforceRestoration1_Key, Define.StarforceRestoration1_Price, false);
            ItemManager.Instance.ConsumeItem(Define.StarforceRestoration2_Key, Define.StarforceRestoration2_Price, false);

            UserTable.TransactionUpdate(EquipSlotEnhanceTables);

            RefreshStat();

            return true;
        }
        //------------------------------------------------------------------------------------
        public Enum_StarforceResult DoStarforceUp(Enum_EquipType enum_EquipType, bool downAid, bool destroyAid)
        {
            // 강화 완료 시 장착한 부위
            int currentSlotLevel = GetStarforceLevel(enum_EquipType);
            int nextSlotLevel = currentSlotLevel + 1;

            EquipSlotEnhanceInfo enhanceInfo;


            if (_equipSlotEnhanceChart.TryGetEquipSlotEnhanceInfo(currentSlotLevel, out enhanceInfo) == false)
                return Enum_StarforceResult.Max;

            if (_equipSlotEnhanceChart.TryGetEquipSlotEnhanceInfo(currentSlotLevel + 1, out var tempinfo) == false)
                return Enum_StarforceResult.Max;

            if (ItemManager.Instance.GetItemAmount(enhanceInfo.MainPriceKey) < enhanceInfo.MainPrice)
                return Enum_StarforceResult.Max;

            long subPrice = enhanceInfo.SubPrice;

            // 옵션 선택한 만큼 추가 금액
            subPrice += downAid == true ? enhanceInfo.SubPrice : 0;
            subPrice += destroyAid == true ? enhanceInfo.SubPrice : 0;

            if (ItemManager.Instance.GetItemAmount(enhanceInfo.SubPriceKey) < subPrice)
                return Enum_StarforceResult.Max;

            if (_starForcePicker == null)
                _starForcePicker = new WeightedRandomPicker<Enum_StarforceResult>();

            _starForcePicker.Clear();

            Enum_StarforceResult enum_StarforceResult = Enum_StarforceResult.Max;

            _starForcePicker.Add(Enum_StarforceResult.Success, enhanceInfo.Success);

            float stay = enhanceInfo.Stay;
            float down = enhanceInfo.Down;
            float destroy = enhanceInfo.Destroy;


            if (downAid == true)
            {
                down = 0;
                stay += enhanceInfo.Down;
            }

            if (destroyAid == true)
            {
                destroy = enhanceInfo.Destroy * 0.5f;
                stay += enhanceInfo.Destroy * 0.5f;
            }

            _starForcePicker.Add(Enum_StarforceResult.Stay, stay);
            _starForcePicker.Add(Enum_StarforceResult.Down, down);
            _starForcePicker.Add(Enum_StarforceResult.Destroy, destroy);

            enum_StarforceResult = _starForcePicker.Pick();

            if (_equipmentTable.EnhanceSlot(enum_EquipType, enum_StarforceResult, false) == false)
                return Enum_StarforceResult.Max;

            ItemManager.Instance.ConsumeItem(enhanceInfo.MainPriceKey, enhanceInfo.MainPrice, false);
            ItemManager.Instance.ConsumeItem(enhanceInfo.SubPriceKey, subPrice, false);

            //UserTable.TransactionUpdate(EquipSlotEnhanceTables);
            Table.UserTable.Get<Table.EquipmentTable>().UpdateTable(false);
            Table.UserTable.Get<Table.PointTable>().UpdateTable(false);

            // 스탯 갱신
            RefreshStat();

            return enum_StarforceResult;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
