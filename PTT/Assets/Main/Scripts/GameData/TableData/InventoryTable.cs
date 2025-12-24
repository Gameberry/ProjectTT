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
        public long acquiredSeq;
        public int count;

        // 인스턴스(장비 등)
        public string instanceId;
        public int enhanceLevel;

        public bool IsInstance => string.IsNullOrEmpty(instanceId) == false;

        public string Pack() => $"{itemId},{acquiredSeq},{count},{instanceId},{enhanceLevel}";

        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            var sp = str.Split(',');
            if (sp.Length > 0) int.TryParse(sp[0], out itemId);
            if (sp.Length > 1) long.TryParse(sp[1], out acquiredSeq);
            if (sp.Length > 2) int.TryParse(sp[2], out count);
            if (sp.Length > 3) instanceId = sp[3] ?? string.Empty;
            if (sp.Length > 4) int.TryParse(sp[4], out enhanceLevel);

            if (count <= 0) count = 1;
            if (acquiredSeq < 0) acquiredSeq = 0;
            if (enhanceLevel < 0) enhanceLevel = 0;
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
        private const string acquireSeqKey = "AcquireSeq";

        private List<InventoryEntry> inventory = new List<InventoryEntry>();
        private long nextAcquireSeq = 1;

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == inventoryKey) inventory = PackUtil.UnpackList<InventoryEntry>(data[i][key].ToString());
                    else if (key == acquireSeqKey)
                    {
                        long.TryParse(data[i][key].ToString(), out nextAcquireSeq);
                        if (nextAcquireSeq <= 0) nextAcquireSeq = 1;
                    }
                }
            }
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(inventoryKey, PackUtil.PackList(inventory));
            p.Add(acquireSeqKey, nextAcquireSeq);
            return p;
        }

        public IReadOnlyList<InventoryEntry> Raw => inventory;

        public InventoryEntry FindStack(int itemId)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.itemId == itemId && string.IsNullOrEmpty(e.instanceId))
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
                if (string.IsNullOrEmpty(e.instanceId)) continue;
                cnt++;
            }
            return cnt;
        }

        public void AddStack(int itemId, int amount, long acquireSeq)
        {
            var e = FindStack(itemId);
            if (e == null)
            {
                inventory.Add(new InventoryEntry
                {
                    itemId = itemId,
                    acquiredSeq = acquireSeq,
                    count = Mathf.Max(1, amount),
                    instanceId = string.Empty,
                    enhanceLevel = 0
                });
            }
            else
            {
                e.count += Mathf.Max(1, amount);
            }
        }

        public void AddInstance(int itemId, string instanceId, long acquireSeq)
        {
            inventory.Add(new InventoryEntry
            {
                itemId = itemId,
                acquiredSeq = acquireSeq,
                count = 1,
                instanceId = instanceId ?? string.Empty,
                enhanceLevel = 0
            });
        }

        public long AllocateAcquireSeq()
        {
            long s = nextAcquireSeq;
            nextAcquireSeq++;
            if (nextAcquireSeq <= 0) nextAcquireSeq = 1;
            return s;
        }

        public bool TryGetInstance(string instanceId, out InventoryEntry entry)
        {
            entry = null;
            if (string.IsNullOrEmpty(instanceId)) return false;
            entry = inventory.Find(x => x != null && x.instanceId == instanceId);
            return entry != null;
        }

        public bool CanRemoveInstance(string instanceId)
        {
            var eq = UserTable.Get<EquipmentTable>();
            if (eq == null) return true;
            return !eq.IsEquipped(instanceId);
        }


        public bool RemoveStack(int itemId, int amount)
        {
            if (amount <= 0) return true;

            for (int i = 0; i < inventory.Count; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.itemId != itemId) continue;
                if (!string.IsNullOrEmpty(e.instanceId)) continue;

                if (e.count < amount) return false;

                e.count -= amount;
                if (e.count <= 0)
                    inventory.RemoveAt(i);

                return true;
            }
            return false;
        }

        public int RemoveInstances(int itemId, int amount)
        {
            if (amount <= 0) return 0;

            inventory.Sort((a, b) => a.acquiredSeq.CompareTo(b.acquiredSeq));

            int removed = 0;
            for (int i = 0; i < inventory.Count && removed < amount; i++)
            {
                var e = inventory[i];
                if (e == null) continue;
                if (e.itemId != itemId) continue;
                if (string.IsNullOrEmpty(e.instanceId)) continue;

                if (!CanRemoveInstance(e.instanceId))
                    continue;

                inventory.RemoveAt(i);
                i--;
                removed++;
            }

            return removed;
        }
        public List<InventoryEntry> BuildView(Enum_InventorySort sort)
        {
            var view = new List<InventoryEntry>(inventory);
            var itemChart = GameBerry.Chart.GameChart.Get<GameBerry.Chart.ItemChart>();

            int TypeKey(InventoryEntry e) => (int)(itemChart?.Get(e.itemId)?.ItemType ?? 0);
            int SortGroup(InventoryEntry e) => itemChart?.Get(e.itemId)?.SortGroup ?? 0;
            int Rarity(InventoryEntry e) => (int)(itemChart?.Get(e.itemId)?.Rarity ?? 0);

            switch (sort)
            {
                case Enum_InventorySort.AcquireAsc:
                    view.Sort((a, b) => a.acquiredSeq.CompareTo(b.acquiredSeq));
                    break;

                case Enum_InventorySort.TypeAsc:
                    view.Sort((a, b) =>
                    {
                        int c = SortGroup(a).CompareTo(SortGroup(b));
                        if (c != 0) return c;
                        c = TypeKey(a).CompareTo(TypeKey(b));
                        if (c != 0) return c;
                        c = Rarity(b).CompareTo(Rarity(a));
                        if (c != 0) return c;
                        return a.acquiredSeq.CompareTo(b.acquiredSeq);
                    });
                    break;

                case Enum_InventorySort.RarityDesc:
                    view.Sort((a, b) =>
                    {
                        int c = Rarity(b).CompareTo(Rarity(a));
                        if (c != 0) return c;
                        c = SortGroup(a).CompareTo(SortGroup(b));
                        if (c != 0) return c;
                        c = TypeKey(a).CompareTo(TypeKey(b));
                        if (c != 0) return c;
                        return a.acquiredSeq.CompareTo(b.acquiredSeq);
                    });
                    break;
            }

            return view;
        }
    }
}
