using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Table
{
    [Serializable]
    public class InventoryEntry : ItemData, IPackable
    {
        // Inventory is stack-only. Keep pack format as itemId,count.
        public string Pack() => $"{PackUtil.PackValue(itemId)},{PackUtil.PackValue(count)}";

        public void Unpack(string str)
        {
            itemId = 0;
            instanceId = 0;
            count = 0;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) itemId = PackUtil.UnpackValue<int>(sp[0]);
            if (sp.Length > 1) count = PackUtil.UnpackValue<long>(sp[1]);
            if (sp.Length > 2) instanceId = PackUtil.UnpackValue<int>(sp[2]); // legacy read only

            if (count < 0) count = 0;
        }
    }

    public enum Enum_InventorySort
    {
        AcquireSort = 0,
        TypeSort = 1,
        RaritySort = 2,
    }

    public class InventoryTable : TableBase
    {
        private const string inventoryKey = "Inventory";

        private List<InventoryEntry> inventory = new List<InventoryEntry>();

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == inventoryKey)
                    {
                        inventory = PackUtil.UnpackList<InventoryEntry>(data[i][key].ToString());
                    }
                }
            }
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(inventoryKey, PackUtil.PackList(inventory));
            return p;
        }

        public IReadOnlyList<InventoryEntry> Raw => inventory;

        public InventoryEntry FindStack(int itemId)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.itemId != itemId) continue;
                return e;
            }
            return null;
        }

        public void AddStack(int itemId, int amount)
        {
            if (amount <= 0) return;

            var e = FindStack(itemId);
            if (e == null)
            {
                inventory.Add(new InventoryEntry
                {
                    itemId = itemId,
                    count = amount,
                    instanceId = 0,
                });
            }
            else
            {
                e.count += amount;
            }
        }

        public bool RemoveStack(int itemId, int removeCount)
        {
            if (removeCount <= 0) return true;

            var e = FindStack(itemId);
            if (e == null) return false;
            if (e.count < removeCount) return false;

            e.count -= removeCount;
            if (e.count <= 0)
                inventory.Remove(e);

            return true;
        }

        public List<InventoryEntry> BuildView(Enum_InventorySort sort)
        {
            var itemChart = GameBerry.Chart.GameChart.Get<GameBerry.Chart.ItemChart>();
            var equipChart = GameBerry.Chart.GameChart.Get<GameBerry.Chart.EquipChart>();

            Enum_ItemType TypeEnum(InventoryEntry e) => itemChart?.Get(e.itemId)?.ItemType ?? Enum_ItemType.Max;
            int TypeKey(InventoryEntry e) => (int)(TypeEnum(e));
            int RarityKey(InventoryEntry e) => (int)(itemChart?.Get(e.itemId)?.Rarity ?? 0);

            Enum_EquipType EquipEnum(InventoryEntry e) => equipChart?.Get(e.itemId)?.EquipType ?? Enum_EquipType.Max;
            int EquipKey(InventoryEntry e) => (int)(EquipEnum(e));

            List<InventoryEntry> capyList = new List<InventoryEntry>(inventory);

            switch (sort)
            {
                case Enum_InventorySort.TypeSort:
                    capyList.Sort((x, y) =>
                    {
                        if (TypeKey(x) < TypeKey(y))
                            return -1;
                        else if (TypeKey(x) > TypeKey(y))
                            return 1;
                        else
                        {
                            Enum_ItemType enum_ItemType = TypeEnum(x);
                            if (enum_ItemType == Enum_ItemType.Equip)
                            {
                                if (EquipKey(x) < EquipKey(y))
                                    return -1;
                                else if (EquipKey(x) > EquipKey(y))
                                    return 1;
                            }

                            if (RarityKey(x) < RarityKey(y))
                                return 1;
                            else if (RarityKey(x) > RarityKey(y))
                                return -1;

                            if (x.itemId < y.itemId)
                                return -1;
                            else if (x.itemId > y.itemId)
                                return 1;
                        }

                        return 0;
                    });
                    break;

                case Enum_InventorySort.RaritySort:
                    capyList.Sort((x, y) =>
                    {

                        if (RarityKey(x) < RarityKey(y))
                            return 1;
                        else if (RarityKey(x) > RarityKey(y))
                            return -1;
                        else
                        {
                            if (TypeKey(x) < TypeKey(y))
                                return -1;
                            else if (TypeKey(x) > TypeKey(y))
                                return 1;

                            Enum_ItemType enum_ItemType = TypeEnum(x);
                            if (enum_ItemType == Enum_ItemType.Equip)
                            {
                                if (EquipKey(x) < EquipKey(y))
                                    return -1;
                                else if (EquipKey(x) > EquipKey(y))
                                    return 1;
                            }

                            if (x.itemId < y.itemId)
                                return -1;
                            else if (x.itemId > y.itemId)
                                return 1;
                        }

                        return 0;
                    });
                    break;
                default:
                    break;
            }

            return capyList;
        }

    }
}
