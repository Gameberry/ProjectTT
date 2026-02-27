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

        public event Action OnEquipSlotChanged;

        EquipmentTable _equipmentTable;
        EquipChart _equipChart;
        EquipRandomRuleChart _equipRandomRuleChart;
        EquipRandomPoolChart _equipRandomPoolChart;
        EquipSlotEnhanceChart _equipSlotEnhanceChart;

        InventoryTable _inventoryTable;

        private readonly System.Random _random = new System.Random();

        WeightedRandomPicker<EquipRandomPoolInfo> _picker = null;

        WeightedRandomPicker<Enum_StarforceResult> _starForcePicker = null;
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _equipmentTable = UserTable.Get<EquipmentTable>();
            _equipChart = GameChart.Get<EquipChart>();
            _equipRandomRuleChart = GameChart.Get<EquipRandomRuleChart>();
            _equipRandomPoolChart = GameChart.Get<EquipRandomPoolChart>();
            _equipSlotEnhanceChart = GameChart.Get<EquipSlotEnhanceChart>();

            _inventoryTable = UserTable.Get<InventoryTable>();
        }
        //------------------------------------------------------------------------------------
        #region Data
        //------------------------------------------------------------------------------------
        public bool AddEquipment(ItemHandle itemHandle)
        {
            EquipmentData equipmentData = new EquipmentData { instanceId = itemHandle.instanceId, addStatList = CreateRandomOptionStats(itemHandle) };
            
            return _equipmentTable.AddEquipment(equipmentData);
        }
        //------------------------------------------------------------------------------------
        private List<EquipmentAddStat> CreateRandomOptionStats(ItemHandle itemHandle)
        {
            ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemHandle.itemId);
            EquipInfo equipInfo = _equipChart.Get(itemHandle.itemId);

            if (itemInfo == null || equipInfo == null)
                return new List<EquipmentAddStat>();

            var result = new List<EquipmentAddStat>();

            if (!_equipRandomRuleChart.TryGetRandomRule(itemInfo.Rarity, out EquipRandomRuleInfo equipRandomRuleInfo))
                return result;

            int optionCount = UnityEngine.Random.Range(equipRandomRuleInfo.OptionCountMin, equipRandomRuleInfo.OptionCountMax + 1);

            if (optionCount <= 0)
                return result;

            if (_picker == null)
                _picker = new WeightedRandomPicker<EquipRandomPoolInfo>();

            _picker.Clear();

            List<EquipRandomPoolInfo> pool = _equipRandomPoolChart.GetRandomPool(equipInfo.EquipType);

            if (pool == null || pool.Count <= 0)
                return new List<EquipmentAddStat>();

            for (int i = 0; i < pool.Count; ++i)
            {
                _picker.Add(pool[i], pool[i].Weight);
            }

            bool allowDuplicate = equipRandomRuleInfo.AllowDuplicateStat == 1;

            if (allowDuplicate == true)
            { // 중복 허용이면 그냥 가중치로 뽑아서 ㄱㄱ
                for (int i = 0; i < optionCount; ++i)
                {
                    EquipRandomPoolInfo pick = _picker.Pick();
                    result.Add(EquipmentAddStat.Set(pick.Stat, pick.GetRandomStatValue()));
                }
            }
            else
            {
                for (int i = 0; i < optionCount; ++i)
                {
                    if (_picker.Count <= 0)
                        break;

                    EquipRandomPoolInfo pick = _picker.Pick();
                    result.Add(EquipmentAddStat.Set(pick.Stat, pick.GetRandomStatValue()));
                    _picker.Remove(pick, pick.Weight);
                }
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        public EquipmentData GetEquipmentData(ItemHandle itemHandle)
        {
            if (_equipmentTable.TryGetData(itemHandle.instanceId, out var data))
                return data;

            return null;
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

            InventoryEntry inventoryEntry = _inventoryTable.FindInstance(instanceId);

            itemHandle = ItemHandle.FromInventory(inventoryEntry);

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
            return _equipmentTable.IsEquipped(itemHandle.instanceId);
        }
        //------------------------------------------------------------------------------------
        public bool SetEquip(ItemHandle itemHandle)
        {
            EquipInfo equipInfo = _equipChart.Get(itemHandle.itemId);

            if (equipInfo == null)
                return false;

            _equipmentTable.SetEquipped(equipInfo.EquipType, itemHandle.instanceId);
            OnEquipSlotChanged.Invoke();

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