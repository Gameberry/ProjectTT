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
            return $"AddItemResult Success : {Success}\nRequested : {Requested}\nAdded : {Added}\nReason : {Reason}";
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
            return $"ConsumeItemResult Success : {Success}\nRequested : {Requested}\nConsumed : {Consumed}\nReason : {Reason}";
        }
    }

    public interface IItemStorageHandler
    {
        GameBerry.Enum_ItemStorageType StorageType { get; }
        AddItemResult Add(ItemInfo meta, long amount, bool immediateServerUpdate);
        ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediateServerUpdate);
        ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediateServerUpdate);
        long GetAmount(ItemInfo meta);
    }

    public class ItemData
    {
        public int itemId;

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
        private Dictionary<int, Action<long>> _itemRefreshEvent = new Dictionary<int, Action<long>>();

        private const string _iconPath = "Icon/item/{0}";
        private Dictionary<int, Sprite> _itemIcons = new Dictionary<int, Sprite>();

        private const string _itemNameLocalKey = "item/{0}/name";
        private const string _itemDescLocalKey = "item/{0}/desc";

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
            return _itemChart?.Get(itemId);
        }

        public ItemInfo GetItemInfo(int itemId) => GetItemMeta(itemId);


        public Enum_ItemType GetItemType(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return Enum_ItemType.Max;

            return meta.ItemType;
        }

        public Enum_Rarity GetItemRarity(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return Enum_Rarity.Max;

            return meta.Rarity;
        }

        public string GetItemNameLocalKey(int itemId)
        {
            return string.Format(_itemNameLocalKey, itemId);
        }

        public string GetItemDescLocalKey(int itemId)
        {
            return string.Format(_itemDescLocalKey, itemId);
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
            InvokeItemRefresh(itemId);

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

            if (res.Success)
            {
                RaiseChanged(meta.StorageType);
                InvokeItemRefresh(itemId);
            }

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

            if (res.Success)
            {
                RaiseChanged(meta.StorageType);
                InvokeItemRefresh(itemId);
            }

            return res;
        }


        // ----------------------------------------------------------------------
        // Handle 기반 API (선택형 행동: 판매/강화/장착/분해 등)

        public ConsumeItemResult Consume(GameBerry.ItemHandle handle, long amount = 1, bool immediateServerUpdate = true)
        {
            if (handle.itemId <= 0)
                return new ConsumeItemResult { Success = false, Reason = "InvalidHandle" };

            // Display-only handle must never mutate user data.
            if (handle.isMeta)
                return new ConsumeItemResult { Success = false, Reason = "MetaHandle" };

            var meta = GetItemMeta(handle.itemId);
            if (meta == null)
                return new ConsumeItemResult { Success = false, Reason = "InvalidItemId" };

            // Prevent wrong route usage early.
            if (handle.IsInstance && meta.IsStack)
                return new ConsumeItemResult { Success = false, Reason = "StackItemCannotUseInstance" };

            if (!handle.IsInstance && !meta.IsStack && amount != 1)
            {
                // Non-stack items should be consumed by instance, not by amount.
                return new ConsumeItemResult { Success = false, Reason = "UseInstanceForNonStack" };
            }

            if (handle.IsInstance)
            {
                // instance는 '정확히 그 instance'만 처리한다.
                return ConsumeItem_Instance(handle.itemId, handle.instanceId, immediateServerUpdate);
            }

            return ConsumeItem(handle.itemId, amount, immediateServerUpdate);
        }


        public long GetCount(GameBerry.ItemHandle handle)
        {
            if (handle.itemId <= 0) return 0;

            if (handle.isMeta) return 0;

            if (handle.IsInstance)
            {
                var inv = UserTable.Get<InventoryTable>();
                return inv != null && inv.FindInstance(handle.instanceId) != null ? 1 : 0;
            }

            return GetItemAmount(handle.itemId);
        }

        public bool TryGetHandleByInstanceId(int instanceId, out GameBerry.ItemHandle handle)
        {
            handle = default;
            var inv = UserTable.Get<InventoryTable>();
            if (inv == null) return false;

            return inv.TryGetHandleByInstanceId(instanceId, out handle);
        }

        public long GetItemAmount(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return 0;

            if (!_handlers.TryGetValue(meta.StorageType, out var handler))
                return 0;

            return handler.GetAmount(meta);
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

        public void AddItemRefreshEvent(int itemId, Action<long> action)
        {
            if (itemId <= 0)
                return;

            if (!_itemRefreshEvent.ContainsKey(itemId))
                _itemRefreshEvent[itemId] = null;

            _itemRefreshEvent[itemId] += action;
        }

        public void RemoveItemRefreshEvent(int itemId, Action<long> action)
        {
            if (itemId <= 0)
                return;

            if (!_itemRefreshEvent.TryGetValue(itemId, out var exist))
                return;

            exist -= action;

            if (exist == null)
                _itemRefreshEvent.Remove(itemId);
            else
                _itemRefreshEvent[itemId] = exist;
        }

        public void InvokeItemRefresh(int itemId)
        {
            if (_itemRefreshEvent.TryGetValue(itemId, out var action))
                action?.Invoke(GetItemAmount(itemId));
        }

        public void ShowItemDesc(ItemHandle handle)
        {
            Debug.Log($"ShowItemDesc {handle}");

            UI.UIManager.Instance.DialogEnter<UI.ItemDescDialog>();

            var itemDescDialog = UI.UIManager.Get<UI.ItemDescDialog>() as UI.ItemDescDialog;
            if (itemDescDialog == null)
            {
                Debug.LogError("[ItemManager] ItemDescDialog not found via UIManager.Get.");
                return;
            }

            itemDescDialog.Bind(handle);
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

                // stack만 itemId+amount 소모를 지원한다.
                if (meta.IsStack == false)
                    return new ConsumeItemResult { Success = false, Reason = "UseHandleForInstance" };

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
                if (itemid == -1)
                    return new ConsumeItemResult { Success = false, Reason = "NotFound" };

                if (itemid != meta.ItemId)
                    return new ConsumeItemResult { Success = false, Reason = "ItemIdMismatch" };

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

            public long GetAmount(ItemInfo meta)
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

            public long GetAmount(ItemInfo meta)
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

            public long GetAmount(ItemInfo meta)
            {
                var st = UserTable.Get<SkinTable>();
                return st.IsUnlocked(meta.ItemId) ? 1 : 0;
            }
        }
    }
}
