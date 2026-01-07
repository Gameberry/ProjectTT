using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace GameBerry.Table
{
    [Serializable]
    public class InventoryEntry : ItemData, IPackable
    {
        public string Pack() => $"{itemId},{count},{instanceId}";

        public void Unpack(string str)
        {
            itemId = 0;
            instanceId = 0;
            count = 1;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) int.TryParse(sp[0], out itemId);
            if (sp.Length > 1) long.TryParse(sp[1], out count);
            if (sp.Length > 2) int.TryParse(sp[2], out instanceId);

            if (count <= 0) count = 1;
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

        private int nextInstanceId = 1;

        private int GetNewInstanceId()
        {
            if (nextInstanceId > inventory.Count)
                nextInstanceId = 1;

            List<InventoryEntry> tempinven = inventory.FindAll(x => x.IsInstance == true && x.instanceId >= nextInstanceId);
            if (tempinven.Count == 0)
                return nextInstanceId;

            tempinven.Sort((a, b) =>
            {
                return a.instanceId.CompareTo(b.instanceId);
            });

            foreach (var pair in tempinven)
            {
                InventoryEntry inventoryEntry = pair;

                if (inventoryEntry.IsInstance == false)
                    continue;

                if (nextInstanceId < inventoryEntry.instanceId)
                    break;
                else if (nextInstanceId == inventoryEntry.instanceId)
                {
                    nextInstanceId++;
                    continue;
                }

                break;
            }

            return nextInstanceId;
        }

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


        public InventoryEntry FindInstance(int instanceId)
        {
            if (instanceId <= 0) return null;

            for (int i = 0; i < inventory.Count; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.instanceId != instanceId) continue;
                if (!e.IsInstance) continue;
                return e;
            }
            return null;
        }

        public bool TryGetHandleByInstanceId(int instanceId, out GameBerry.ItemHandle handle)
        {
            handle = default;
            var e = FindInstance(instanceId);
            if (e == null) return false;

            handle = GameBerry.ItemHandle.Instance(e.itemId, e.instanceId);
            return true;
        }

        public InventoryEntry FindStack(int itemId)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.itemId != itemId) continue;
                if (!e.IsStack) continue;
                return e;
            }
            return null;
        }



        public int CountInstance(int itemId)
        {
            int cnt = 0;
            for (int i = 0; i < inventory.Count; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.itemId != itemId) continue;
                if (!e.IsInstance) continue;
                cnt++;
            }
            return cnt;
        }

        public void AddStack(int itemId, int amount)
        {
            var e = FindStack(itemId);
            if (e == null)
            {
                inventory.Add(new InventoryEntry
                {
                    itemId = itemId,
                    count = Mathf.Max(1, amount),
                    instanceId = 0,
                });
            }
            else
            {
                e.count += Mathf.Max(1, amount);
            }
        }

        public int AddInstance(int itemId)
        {
            int instanceId = GetNewInstanceId();
            inventory.Add(new InventoryEntry
            {
                itemId = itemId,
                count = 1,
                instanceId = instanceId,
            });

            return instanceId;
        }

        public bool CanRemoveInstance(int instanceId)
        {
            var eq = UserTable.Get<EquipmentTable>();
            if (eq == null) return true;
            return !eq.IsEquipped(instanceId);
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

        public int RemoveInstances(int itemId, int amount)
        {
            if (amount <= 0) return 0;

            int removed = 0;
            for (int i = 0; i < inventory.Count && removed < amount; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.itemId != itemId) continue;
                if (!e.IsInstance) continue;
                if (CanRemoveInstance(e.instanceId) == false) continue;

                int releasedId = e.instanceId;
                inventory.RemoveAt(i);
                i--;
                removed++;

                if (nextInstanceId > releasedId)
                    nextInstanceId = releasedId;
            }

            return removed;
        }
        
        public int RemoveInstance(int instanceId)
        {
            InventoryEntry inventoryEntry = inventory.Find(x => x.instanceId == instanceId);

            if (inventoryEntry == null) return -1;
            if (CanRemoveInstance(instanceId) == false) return -1;

            inventory.Remove(inventoryEntry);

            return inventoryEntry.itemId;
        }

        public List<InventoryEntry> BuildView(Enum_InventorySort sort)
        {
            // 원본 inventory 리스트 순서가 곧 획득순(저장 순서)이다.
            // 정렬 탭은 "뷰"만 정렬하고, 동률은 원본 index로 안정(stable)하게 유지한다.
            var itemChart = GameBerry.Chart.GameChart.Get<GameBerry.Chart.ItemChart>();

            int TypeKey(InventoryEntry e) => (int)(itemChart?.Get(e.itemId)?.ItemType ?? 0);
            int RarityKey(InventoryEntry e) => (int)(itemChart?.Get(e.itemId)?.Rarity ?? 0);

            var indexed = inventory
                .Select((e, idx) => (e, idx))
                .Where(x => x.e != null)
                .ToList();

            switch (sort)
            {
                case Enum_InventorySort.TypeSort:
                    indexed = indexed
                        .OrderBy(x => TypeKey(x.e))
                        .ThenByDescending(x => RarityKey(x.e))
                        .ThenBy(x => x.idx)
                        .ToList();
                    break;

                case Enum_InventorySort.RaritySort:
                    indexed = indexed
                        .OrderByDescending(x => RarityKey(x.e))
                        .ThenBy(x => TypeKey(x.e))
                        .ThenBy(x => x.idx)
                        .ToList();
                    break;

                case Enum_InventorySort.AcquireSort:
                default:
                    // 그대로
                    break;
            }

            return indexed.Select(x => x.e).ToList();
        }

    }
}
