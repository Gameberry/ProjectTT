using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.Managers
{
    public struct AddItemResult
    {
        public bool Success;
        public long Requested;
        public long Added;
        public string Reason;
    }

    public struct ConsumeItemResult
    {
        public bool Success;
        public long Requested;
        public long Consumed;
        public string Reason;
    }

    public interface IItemStorageHandler
    {
        GameBerry.Enum_ItemStorageType StorageType { get; }
        AddItemResult Add(ItemInfo meta, long amount, bool immediateServerUpdate);
        ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediateServerUpdate);
        long GetCount(ItemInfo meta);
    }

    public class ItemManager : Singleton<ItemManager>
    {
        public event Action OnInventoryChanged;
        public event Action OnPointChanged;
        public event Action OnSkinChanged;

        private readonly Dictionary<GameBerry.Enum_ItemStorageType, IItemStorageHandler> _handlers = new();

        public const string _iconPath = "Icon/item/{0}";
        private Dictionary<int, Sprite> _itemIcons = new Dictionary<int, Sprite>();

        ItemChart _itemChart;

        protected override void Init()
        {
            _itemChart = GameChart.Get<ItemChart>();

            Register(new InventoryStorageHandler());
            Register(new PointStorageHandler());
            Register(new SkinStorageHandler());
        }

        private void Register(IItemStorageHandler h)
        {
            _handlers[h.StorageType] = h;
        }

        public ItemInfo GetItemMeta(int itemId)
        {
            var chart = GameChart.Get<ItemChart>();
            return chart?.Get(itemId);
        }

        public Enum_ItemType GetItemType(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return Enum_ItemType.Max;

            return meta.ItemType;
        }

        public AddItemResult AddItem(int itemId, long amount, bool immediateServerUpdate = true)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null)
                return new AddItemResult { Success = false, Reason = "InvalidItemId" };

            if (!_handlers.TryGetValue(meta.StorageType, out var handler))
                return new AddItemResult { Success = false, Reason = "NoHandler" };

            var res = handler.Add(meta, amount, immediateServerUpdate);
            RaiseChanged(meta.StorageType);
            return res;
        }

        public ConsumeItemResult ConsumeItem(int itemId, long amount, bool immediateServerUpdate = true)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null)
                return new ConsumeItemResult { Success = false, Reason = "InvalidItemId" };

            if (!_handlers.TryGetValue(meta.StorageType, out var handler))
                return new ConsumeItemResult { Success = false, Reason = "NoHandler" };

            var res = handler.Consume(meta, amount, immediateServerUpdate);
            if (res.Success) RaiseChanged(meta.StorageType);
            return res;
        }

        public long GetCount(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return 0;

            if (!_handlers.TryGetValue(meta.StorageType, out var handler))
                return 0;

            return handler.GetCount(meta);
        }

        private void RaiseChanged(GameBerry.Enum_ItemStorageType t)
        {
            if (t == GameBerry.Enum_ItemStorageType.Inventory) OnInventoryChanged?.Invoke();
            else if (t == GameBerry.Enum_ItemStorageType.Point) OnPointChanged?.Invoke();
            else if (t == GameBerry.Enum_ItemStorageType.Skin) OnSkinChanged?.Invoke();
        }

        public Sprite GetIcon(int itemId)
        {
            Sprite sp = null;

            if (_itemIcons.ContainsKey(itemId) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(_iconPath, itemId), o =>
                {
                    sp = o as Sprite;
                    _itemIcons.Add(itemId, sp);
                });
            }
            else
                sp = _itemIcons[itemId];

            return sp;
        }


        // --- Handlers ---

        private class InventoryStorageHandler : IItemStorageHandler
        {
            public GameBerry.Enum_ItemStorageType StorageType => GameBerry.Enum_ItemStorageType.Inventory;

            private List<Table.TableBase> EquipInvenTables = new List<Table.TableBase>()
            {
                Table.UserTable.Get<Table.InventoryTable>(),
                Table.UserTable.Get<Table.EquipmentTable>()
            };

            public AddItemResult Add(ItemInfo meta, long amount, bool immediate)
            {
                var inv = UserTable.Get<InventoryTable>();
                long added = 0;

                if (meta.IsStack)
                {
                    inv.AddStack(meta.ItemId, (int)amount);
                    added = amount;
                }
                else
                {
                    for (int i = 0; i < amount; i++)
                    {
                        int instanceId = inv.AddInstance(meta.ItemId);
                        if (meta.ItemType == Enum_ItemType.Equip)
                            UserTable.Get<EquipmentTable>()?.AddEquipment(instanceId);
                    }

                    if (meta.ItemType == Enum_ItemType.Equip)
                        Table.UserTable.DynamicUpdateData(EquipInvenTables);

                    added = amount;
                }

                if (meta.ItemType != Enum_ItemType.Equip && immediate) inv.UpdateTable();
                return new AddItemResult { Success = true, Requested = amount, Added = added };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                var inv = UserTable.Get<InventoryTable>();
                bool ok = meta.IsStack
                    ? inv.RemoveStack(meta.ItemId, (int)amount)
                    : inv.RemoveInstances(meta.ItemId, (int)amount) == amount;

                if (!ok)
                    return new ConsumeItemResult { Success = false, Reason = "NotEnough" };

                if (immediate) inv.UpdateTable();
                return new ConsumeItemResult { Success = true, Requested = amount, Consumed = amount };
            }

            public long GetCount(ItemInfo meta)
            {
                var inv = UserTable.Get<InventoryTable>();
                return meta.IsStack
                    ? inv.FindStack(meta.ItemId)?.count ?? 0
                    : inv.CountInstance(meta.ItemId);
            }
        }

        private class PointStorageHandler : IItemStorageHandler
        {
            public GameBerry.Enum_ItemStorageType StorageType => GameBerry.Enum_ItemStorageType.Point;

            public AddItemResult Add(ItemInfo meta, long amount, bool immediate)
            {
                var pt = UserTable.Get<PointTable>();
                pt.Add(meta.ItemId, amount);
                if (immediate) pt.UpdateTable();
                return new AddItemResult { Success = true, Requested = amount, Added = amount };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                var pt = UserTable.Get<PointTable>();
                if (pt.GetAmount(meta.ItemId) < amount)
                    return new ConsumeItemResult { Success = false, Reason = "NotEnough" };

                pt.Add(meta.ItemId, -amount);
                if (immediate) pt.UpdateTable();
                return new ConsumeItemResult { Success = true, Requested = amount, Consumed = amount };
            }

            public long GetCount(ItemInfo meta)
            {
                var pt = UserTable.Get<PointTable>();
                return pt.GetAmount(meta.ItemId);
            }
        }

        private class SkinStorageHandler : IItemStorageHandler
        {
            public GameBerry.Enum_ItemStorageType StorageType => GameBerry.Enum_ItemStorageType.Skin;

            public AddItemResult Add(ItemInfo meta, long amount, bool immediate)
            {
                var st = UserTable.Get<SkinTable>();
                bool unlocked = st.TryUnlock(meta.ItemId);
                if (immediate) st.UpdateTable();

                return new AddItemResult
                {
                    Success = true,
                    Requested = amount,
                    Added = unlocked ? 1 : 0,
                    Reason = unlocked ? "Unlocked" : "AlreadyUnlocked"
                };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                return new ConsumeItemResult { Success = false, Reason = "NotConsumable" };
            }

            public long GetCount(ItemInfo meta)
            {
                var st = UserTable.Get<SkinTable>();
                return st.IsUnlocked(meta.ItemId) ? 1 : 0;
            }
        }
    }
}
