using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Table
{
    [Serializable]
    public class InventoryEntry : IPackable
    {
        public int itemId;
        public int count;

        // 인스턴스(장비 등)
        public int instanceId;
        public int enhanceLevel;

        public bool IsInstance => instanceId > 0;
        public bool IsStack => instanceId <= 0;

        public string Pack() => $"{itemId},{count},{instanceId},{enhanceLevel}";

        public void Unpack(string str)
        {
            itemId = 0;
            instanceId = 0;
            count = 1;
            enhanceLevel = 0;

            if (string.IsNullOrEmpty(str)) 
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) int.TryParse(sp[0], out itemId);
            if (sp.Length > 1) int.TryParse(sp[1], out count);
            if (sp.Length > 2) int.TryParse(sp[2], out instanceId);
            if (sp.Length > 3) int.TryParse(sp[3], out enhanceLevel);

            if (count <= 0) count = 1;
            if (enhanceLevel < 0) enhanceLevel = 0;
            if (IsInstance) count = 1;
        }
    }

    public enum Enum_InventorySort
    {
        AcquireAsc = 0,
        TypeAsc = 1,
        RarityDesc = 2,
    }

    public class InventoryTable : TableBase
    {
        private const string inventoryKey = "Inventory";

        private List<InventoryEntry> inventory = new List<InventoryEntry>();

        private int nextInstanceId = 1;

        private void SetInstanceId()
        {
            if (nextInstanceId > inventory.Count)
                nextInstanceId = 1;

            List<InventoryEntry> tempinven = inventory.FindAll(x => x.IsInstance == true && x.instanceId > nextInstanceId);
            if (tempinven.Count == 0)
                return;

            tempinven.Sort((a, b) =>
            {
                return a.instanceId.CompareTo(b.instanceId);
            });

            foreach (var pair in inventory)
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
                        SetInstanceId();
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
                    enhanceLevel = 0
                });
            }
            else
            {
                e.count += Mathf.Max(1, amount);
            }
        }

        public int AddInstance(int itemId)
        {
            int instanceId = nextInstanceId;
            inventory.Add(new InventoryEntry
            {
                itemId = itemId,
                count = 1,
                instanceId = instanceId,
                enhanceLevel = 0
            });

            // nextInstanceId를 사용하고 꼭 다시 셋 해야함
            SetInstanceId();

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
        public List<InventoryEntry> BuildView(Enum_InventorySort sort)
        {
            var view = new List<InventoryEntry>(inventory);
            var itemChart = GameBerry.Chart.GameChart.Get<GameBerry.Chart.ItemChart>();

            int TypeKey(InventoryEntry e) => (int)(itemChart?.Get(e.itemId)?.ItemType ?? 0);
            int Rarity(InventoryEntry e) => (int)(itemChart?.Get(e.itemId)?.Rarity ?? 0);

            switch (sort)
            {
                case Enum_InventorySort.AcquireAsc:
                    //view.Sort((a, b) => a.acquiredSeq.CompareTo(b.acquiredSeq));
                    break;

                case Enum_InventorySort.TypeAsc:
                    view.Sort((a, b) =>
                    {
                        int c = TypeKey(a).CompareTo(TypeKey(b));
                        if (c != 0) return c;
                        c = Rarity(b).CompareTo(Rarity(a));
                        if (c != 0) return c;
                        return a.itemId.CompareTo(b.itemId);
                    });
                    break;

                case Enum_InventorySort.RarityDesc:
                    view.Sort((a, b) =>
                    {
                        int c = Rarity(b).CompareTo(Rarity(a));
                        if (c != 0) return c;
                        c = TypeKey(a).CompareTo(TypeKey(b));
                        if (c != 0) return c;
                        return a.itemId.CompareTo(b.itemId);
                    });
                    break;
            }

            return view;
        }
    }
}
