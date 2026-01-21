using System;
using GameBerry.Table;

namespace GameBerry
{
    [Serializable]
    public struct ItemHandle : IEquatable<ItemHandle>
    {
        public int itemId;
        public int instanceId;

        // Display-only (meta). Should not be used as user-owned data reference.
        public bool isMeta;
        public long metaAmount;
        public int metaLevel;

        public bool IsInstance => instanceId > 0;
        public bool IsStack => instanceId <= 0;

        // ----------------------------------------------------------------------
        // Factory (A안 네이밍)

        public static ItemHandle ForMeta(int itemId, long metaAmount = 0, int metaLevel = 0)
            => new ItemHandle
            {
                itemId = itemId,
                instanceId = 0,
                isMeta = true,
                metaAmount = metaAmount,
                metaLevel = metaLevel
            };

        public static ItemHandle FromData(ItemData data)
            => data == null
                ? default
                : new ItemHandle
                {
                    itemId = data.itemId,
                    instanceId = data.instanceId,
                    isMeta = false
                };

        public static ItemHandle FromInventory(InventoryEntry e) => FromData(e);
        public static ItemHandle FromPoint(PointData e) => FromData(e);
        public static ItemHandle FromSkin(SkinData e) => FromData(e);
        public static ItemHandle FromWeapon(WeaponData e) => FromData(e);

        public static ItemHandle ForStack(int itemId)
            => new ItemHandle { itemId = itemId, instanceId = 0, isMeta = false };

        public static ItemHandle ForInstance(int itemId, int instanceId)
            => new ItemHandle { itemId = itemId, instanceId = instanceId, isMeta = false };

        // ----------------------------------------------------------------------
        // Equality

        public bool Equals(ItemHandle other)
        {
            // Meta and non-meta can never be equal.
            if (isMeta != other.isMeta) return false;

            if (isMeta)
            {
                // Meta handles compare by display fields too.
                return itemId == other.itemId
                    && metaAmount == other.metaAmount
                    && metaLevel == other.metaLevel;
            }

            return itemId == other.itemId && instanceId == other.instanceId;
        }

        public override bool Equals(object obj) => obj is ItemHandle other && Equals(other);

        public override int GetHashCode()
        {
            return isMeta
                ? HashCode.Combine(itemId, metaAmount, metaLevel, true)
                : HashCode.Combine(itemId, instanceId, false);
        }

        public override string ToString()
            => isMeta
                ? $"ItemHandle(META itemId:{itemId}, amount:{metaAmount}, level:{metaLevel})"
                : $"ItemHandle(itemId:{itemId}, instanceId:{instanceId})";
    }

}
