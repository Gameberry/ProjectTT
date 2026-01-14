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

        public event Action OnEquipSlotChanged;

        EquipmentTable _equipmentTable;
        EquipChart _equipChart;
        EquipRandomRuleChart _equipRandomRuleChart;
        EquipRandomPoolChart _equipRandomPoolChart;

        InventoryTable _inventoryTable;

        WeightedRandomPicker<EquipRandomPoolInfo> _picker = null;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _equipmentTable = UserTable.Get<EquipmentTable>();
            _equipChart = GameChart.Get<EquipChart>();
            _equipRandomRuleChart = GameChart.Get<EquipRandomRuleChart>();
            _equipRandomPoolChart = GameChart.Get<EquipRandomPoolChart>();

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
        #region Slot/Equip
        //------------------------------------------------------------------------------------
        public int GetEquipSlotLevel(Enum_EquipType enum_EquipType)
        {
            return _equipmentTable.GetSlotLevel(enum_EquipType);
        }
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

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}