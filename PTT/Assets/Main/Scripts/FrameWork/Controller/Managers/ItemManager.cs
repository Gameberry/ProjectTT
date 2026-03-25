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
        public event Action OnEquipmentStorageChanged;
        public event Action OnWeaponStorageChanged;
        public event Action OnLanternStorageChanged;

        [Obsolete("Use OnWeaponStorageChanged instead.")]
        public event Action OnWeaponChanged
        {
            add => OnWeaponStorageChanged += value;
            remove => OnWeaponStorageChanged -= value;
        }

        private readonly Dictionary<GameBerry.Enum_ItemStorageType, IItemStorageHandler> _handlers = new();
        private Dictionary<int, Action<long>> _itemRefreshEvent = new Dictionary<int, Action<long>>();

        private const string _iconPath = "Icon/item/{0}";
        private Dictionary<int, Sprite> _itemIcons = new Dictionary<int, Sprite>();

        private const string _itemNameLocalKey = "item/{0}/name";
        private const string _itemDescLocalKey = "item/{0}/desc";

        ItemChart _itemChart;
        PointChart _pointChart;

        protected override void Init()
        {
            _itemChart = GameChart.Get<ItemChart>();
            _pointChart = GameChart.Get<PointChart>();

            Register(new InventoryStorageHandler());
            Register(new PointStorageHandler());
            Register(new SkinStorageHandler());
            Register(new EquipmentStorageHandler());
            Register(new WeaponStorageHandler());
            Register(new LanternStorageHandler());
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

        public Enum_Tier GetItemTier(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return Enum_Tier.Max;

            return meta.Tier;
        }

        public string GetItemNameLocalKey(int itemId)
        {
            ItemInfo ItemInfo = GetItemMeta(itemId);
            if(string.IsNullOrEmpty(ItemInfo.NameLocalKey))
                ItemInfo.NameLocalKey = string.Format(_itemNameLocalKey, itemId);

            return ItemInfo.NameLocalKey;
        }

        public string GetItemDescLocalKey(int itemId)
        {
            ItemInfo ItemInfo = GetItemMeta(itemId);
            if (string.IsNullOrEmpty(ItemInfo.DescLocalKey))
                ItemInfo.DescLocalKey = string.Format(_itemDescLocalKey, itemId);

            return ItemInfo.DescLocalKey;
        }

        public AddItemResult AddItem(int itemId, long amount, bool immediateServerUpdate = true)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null)
                return new AddItemResult { Success = false, Reason = "InvalidItemId" };

            if (IsExpItem(itemId))
            {
                bool success = PlayerManager.isAlive && PlayerManager.Instance.AddExp(amount, immediateServerUpdate);
                AddItemResult expRes = new AddItemResult
                {
                    Success = success,
                    Requested = amount,
                    Added = success ? amount : 0,
                    Reason = success ? string.Empty : "PlayerManagerUnavailable"
                };

                if (success)
                {
                    RaiseChanged(meta.StorageType);
                    InvokeItemRefresh(itemId);
                }

                return expRes;
            }

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
                var meta = GetItemMeta(handle.itemId);
                if (meta == null) return 0;

                if (meta.StorageType == Enum_ItemStorageType.Equipment)
                {
                    var eq = UserTable.Get<EquipmentTable>();
                    return (eq != null && eq.HasInstance(handle.instanceId)) ? 1 : 0;
                }

                return 0;
            }

            return GetItemAmount(handle.itemId);
        }

        public bool TryGetHandleByInstanceId(int instanceId, out GameBerry.ItemHandle handle)
        {
            handle = default;
            var eq = UserTable.Get<EquipmentTable>();
            if (eq != null && eq.TryGetHandleByInstanceId(instanceId, out handle))
                return true;

            return false;
        }

        public long GetItemAmount(int itemId)
        {
            var meta = GetItemMeta(itemId);
            if (meta == null) return 0;

            if (!_handlers.TryGetValue(meta.StorageType, out var handler))
                return 0;

            return handler.GetAmount(meta);
        }

        public bool IsExpItem(int itemId)
        {
            PointInfo pointInfo = _pointChart?.Get(itemId);
            return pointInfo != null && pointInfo.Type == Enum_PointType.Exp;
        }

        private void RaiseChanged(GameBerry.Enum_ItemStorageType t)
        {
            if (t == GameBerry.Enum_ItemStorageType.Inventory) OnInventoryChanged?.Invoke();
            else if (t == GameBerry.Enum_ItemStorageType.Point) OnPointChanged?.Invoke();
            else if (t == GameBerry.Enum_ItemStorageType.Skin) OnSkinChanged?.Invoke();
            else if (t == GameBerry.Enum_ItemStorageType.Equipment) OnEquipmentStorageChanged?.Invoke();
            else if (t == GameBerry.Enum_ItemStorageType.Weapon) OnWeaponStorageChanged?.Invoke();
            else if (t == GameBerry.Enum_ItemStorageType.Lantern) OnLanternStorageChanged?.Invoke();
        }

        public void NotifyStorageChanged(GameBerry.Enum_ItemStorageType storageType)
        {
            RaiseChanged(storageType);
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

            var itemDescDialog = UI.UIManager.Get<UI.ItemDescDialog>();
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

            public AddItemResult Add(ItemInfo meta, long amount, bool immediate)
            {
                var inv = UserTable.Get<InventoryTable>();
                if (!meta.IsStack)
                    return new AddItemResult { Success = false, Requested = amount, Added = 0, Reason = "InventoryNonStackNotSupported" };

                inv.AddStack(meta.ItemId, (int)amount);
                inv.UpdateTable(immediate);

                return new AddItemResult { Success = true, Requested = amount, Added = amount };
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
                return new ConsumeItemResult { Success = false, Reason = "NotSupported" };
            }

            public long GetAmount(ItemInfo meta)
            {
                var inv = UserTable.Get<InventoryTable>();
                return inv.FindStack(meta.ItemId)?.count ?? 0;
            }
        }

        private class EquipmentStorageHandler : IItemStorageHandler
        {
            public GameBerry.Enum_ItemStorageType StorageType => GameBerry.Enum_ItemStorageType.Equipment;

            public AddItemResult Add(ItemInfo meta, long amount, bool immediate)
            {
                if (amount <= 0)
                    return new AddItemResult { Success = true, Requested = amount, Added = 0 };

                long added = 0;
                bool autoSalvagedAny = false;
                for (int i = 0; i < amount; ++i)
                {
                    ItemHandle handle = EquipmentManager.Instance.AddEquipment(meta.ItemId);
                    if (handle.IsInstance == false)
                        continue;

                    if (EquipmentManager.Instance.TryAutoSalvage(handle, false))
                    {
                        autoSalvagedAny = true;
                        continue;
                    }

                    if (handle.IsInstance)
                        added++;
                }

                if (autoSalvagedAny)
                {
                    if (immediate)
                    {
                        UserTable.TransactionUpdate(new List<Table.TableBase>
                        {
                            UserTable.Get<EquipmentTable>(),
                            UserTable.Get<HellTable>()
                        });
                    }
                    else
                    {
                        UserTable.Get<EquipmentTable>()?.UpdateTable(false);
                        UserTable.Get<HellTable>()?.UpdateTable(false);
                    }
                }
                else
                {
                    UserTable.Get<EquipmentTable>().UpdateTable(immediate);
                }

                return new AddItemResult { Success = true, Requested = amount, Added = added };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                return new ConsumeItemResult { Success = false, Reason = "UseHandleForInstance" };
            }

            public ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediate)
            {
                var eq = UserTable.Get<EquipmentTable>();
                if (eq == null)
                    return new ConsumeItemResult { Success = false, Reason = "EquipmentTableMissing" };

                if (!eq.TryGetData(instanceId, out var data) || data == null)
                    return new ConsumeItemResult { Success = false, Reason = "NotFound" };

                if (data.itemId != meta.ItemId)
                    return new ConsumeItemResult { Success = false, Reason = "ItemIdMismatch" };

                if (eq.IsEquipped(instanceId))
                    return new ConsumeItemResult { Success = false, Reason = "Equipped" };

                if (!eq.RemoveEquipment(instanceId))
                    return new ConsumeItemResult { Success = false, Reason = "RemoveFailed" };

                Enum_Rarity rarity = data.rarity;
                if (rarity <= 0 || rarity >= Enum_Rarity.Max)
                    rarity = meta.Rarity;

                HellManager.Instance.AddExpByRarity(rarity, immediate);

                eq.UpdateTable(immediate);
                return new ConsumeItemResult { Success = true, Requested = 1, Consumed = 1 };
            }

            public long GetAmount(ItemInfo meta)
            {
                var eq = UserTable.Get<EquipmentTable>();
                return eq?.CountByItemId(meta.ItemId) ?? 0;
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

        private class WeaponStorageHandler : IItemStorageHandler
        {
            public GameBerry.Enum_ItemStorageType StorageType => GameBerry.Enum_ItemStorageType.Weapon;

            public AddItemResult Add(ItemInfo meta, long amount, bool immediate)
            {
                var wt = UserTable.Get<WeaponTable>();
                wt.Add(meta.ItemId, amount);
                wt.UpdateTable(immediate);

                return new AddItemResult { Success = true, Requested = amount, Added = amount };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                var wt = UserTable.Get<WeaponTable>();
                if (wt.GetAmount(meta.ItemId) < amount)
                    return new ConsumeItemResult { Success = false, Reason = "NotEnough" };

                wt.Add(meta.ItemId, -amount);
                wt.UpdateTable(immediate);

                return new ConsumeItemResult { Success = true, Requested = amount, Consumed = amount };
            }

            public ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediate)
            {
                return new ConsumeItemResult { Success = false, Reason = "NotSupported" };
            }

            public long GetAmount(ItemInfo meta)
            {
                var wt = UserTable.Get<WeaponTable>();
                return wt.GetAmount(meta.ItemId);
            }
        }

        private class LanternStorageHandler : IItemStorageHandler
        {
            public GameBerry.Enum_ItemStorageType StorageType => GameBerry.Enum_ItemStorageType.Lantern;

            public AddItemResult Add(ItemInfo meta, long amount, bool immediate)
            {
                var lt = UserTable.Get<LanternTable>();
                if (lt == null)
                    return new AddItemResult { Success = false, Requested = amount, Added = 0, Reason = "LanternTableMissing" };

                lt.Add(meta.ItemId, amount);
                lt.UpdateTable(immediate);
                LanternManager.Instance.RefreshStat();

                return new AddItemResult { Success = true, Requested = amount, Added = amount };
            }

            public ConsumeItemResult Consume(ItemInfo meta, long amount, bool immediate)
            {
                var lt = UserTable.Get<LanternTable>();
                if (lt == null)
                    return new ConsumeItemResult { Success = false, Requested = amount, Consumed = 0, Reason = "LanternTableMissing" };

                if (lt.GetAmount(meta.ItemId) < amount)
                    return new ConsumeItemResult { Success = false, Reason = "NotEnough" };

                lt.Add(meta.ItemId, -amount);
                lt.UpdateTable(immediate);
                LanternManager.Instance.RefreshStat();

                return new ConsumeItemResult { Success = true, Requested = amount, Consumed = amount };
            }

            public ConsumeItemResult Consume_Instance(ItemInfo meta, int instanceId, bool immediate)
            {
                return new ConsumeItemResult { Success = false, Reason = "NotSupported" };
            }

            public long GetAmount(ItemInfo meta)
            {
                var lt = UserTable.Get<LanternTable>();
                if (lt == null)
                    return 0;

                return lt.GetAmount(meta.ItemId);
            }
        }
    }
}
