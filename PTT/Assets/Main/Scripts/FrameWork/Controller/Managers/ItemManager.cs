using System;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry
{
    public struct AddItemResult
    {
        public bool Success;
        public long Requested;
        public long Added;
        public string Reason;

        public override string ToString()
        { 
            return $"AddItemResult Success : {Success}\nRequested : {Requested}\nAdded : {Added}\neason : {Reason}";
        }
    }

    public struct ConsumeItemResult
    {
        public bool Success;
        public long Requested;
        public long Consumed;
        public string Reason;

        public override string ToString()
        {
            return $"ConsumeItemResult Success : {Success}\nRequested : {Requested}\nConsumed : {Consumed}\neason : {Reason}";
        }
    }

    public interface IItemStorageHandler
    {
        GameBerry.Enum_ItemStorageType StorageType { get; }
        AddItemResult Add(ItemInfo meta, long amount, bool immediateServerUpdate);
        ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediateServerUpdate);
        ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediateServerUpdate);
        long GetCount(ItemInfo meta);
    }

    public class ItemData
    {
        public int itemId;

        // 인스턴스(장비 등)
        public int instanceId;

        public long count;

        public bool IsInstance => instanceId > 0;
        public bool IsStack => instanceId <= 0;
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

        public Enum_ItemRarity GetItemRarity(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return Enum_ItemRarity.Max;

            return meta.Rarity;
        }

        public AddItemResult AddItem(int itemId, long amount, bool immediateServerUpdate = true)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null)
                return new AddItemResult { Success = false, Reason = "InvalidItemId" };

            if (!_handlers.TryGetValue(meta.StorageType, out var handler))
                return new AddItemResult { Success = false, Reason = "NoHandler" };

            var res = handler.Add(meta, amount, immediateServerUpdate);
#if DEV_DEFINE
            if (res.Success == false)
                Debug.LogError(res.ToString());
#endif
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
#if DEV_DEFINE
            if (res.Success == false)
                Debug.LogError(res.ToString());
#endif
            if (res.Success) RaiseChanged(meta.StorageType);
            return res;
        }

        public ConsumeItemResult ConsumeItem_Instance(int itemId, int instanceId, bool immediateServerUpdate = true)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null)
                return new ConsumeItemResult { Success = false, Reason = "InvalidItemId" };

            if (!_handlers.TryGetValue(meta.StorageType, out var handler))
                return new ConsumeItemResult { Success = false, Reason = "NoHandler" };

            var res = handler.Consume_Instance(meta, instanceId, immediateServerUpdate);
#if DEV_DEFINE
            if (res.Success == false)
                Debug.LogError(res.ToString());
#endif
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

                    added = amount;
                }

                if (meta.ItemType == Enum_ItemType.Equip)
                {
                    if (immediate == false)
                    {
                        for (int i = 0; i < EquipInvenTables.Count; ++i)
                        { 
                            EquipInvenTables[i].UpdateTable(immediate);
                        }
                    }
                    else
                        UserTable.TransactionUpdate(EquipInvenTables);
                }
                else
                    inv.UpdateTable(immediate);

                return new AddItemResult { Success = true, Requested = amount, Added = added };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                var inv = UserTable.Get<InventoryTable>();

                if (meta.IsStack == true)
                    return new ConsumeItemResult { Success = false, Reason = "NotSupported" };

                bool ok = inv.RemoveStack(meta.ItemId, (int)amount);
                if (!ok)
                    return new ConsumeItemResult { Success = false, Reason = "NotEnough" };

                inv.UpdateTable(immediate);

                return new ConsumeItemResult { Success = true, Requested = amount, Consumed = amount };
            }

            public ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediate)
            {
                var inv = UserTable.Get<InventoryTable>();

                int itemid = inv.RemoveInstance(instanceId);
                if(itemid == -1)
                    return new ConsumeItemResult { Success = false, Reason = "ItemId == -1" };

                Enum_ItemType enum_ItemType = Instance.GetItemType(itemid);
                if (enum_ItemType == Enum_ItemType.Equip)
                    UserTable.Get<EquipmentTable>().RemoveEquipment(instanceId);

                if (enum_ItemType == Enum_ItemType.Equip)
                {
                    if (immediate == false)
                    {
                        for (int i = 0; i < EquipInvenTables.Count; ++i)
                        {
                            EquipInvenTables[i].UpdateTable(immediate);
                        }
                    }
                    else
                        UserTable.TransactionUpdate(EquipInvenTables);
                }
                else
                    inv.UpdateTable(immediate);

                return new ConsumeItemResult { Success = true };
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
                pt.UpdateTable(immediate);

                return new AddItemResult { Success = true, Requested = amount, Added = amount };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                var pt = UserTable.Get<PointTable>();
                if (pt.GetAmount(meta.ItemId) < amount)
                    return new ConsumeItemResult { Success = false, Reason = "NotEnough" };

                pt.Add(meta.ItemId, -amount);
                pt.UpdateTable(immediate);

                return new ConsumeItemResult { Success = true, Requested = amount, Consumed = amount };
            }

            public ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediate)
            {
                return new ConsumeItemResult { Success = false, Reason = "NotSupported" };
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
                bool unlocked = st.AddSkin(meta.ItemId);
                st.UpdateTable(immediate);

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

            public ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediate)
            {
                return new ConsumeItemResult { Success = false, Reason = "NotSupported" };
            }

            public long GetCount(ItemInfo meta)
            {
                var st = UserTable.Get<SkinTable>();
                return st.IsUnlocked(meta.ItemId) ? 1 : 0;
            }
        }
    }
}
