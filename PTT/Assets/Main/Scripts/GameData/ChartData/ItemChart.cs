using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Chart
{
    [System.Serializable]
    public class ItemInfo
    {
        public int ItemId;

        public GameBerry.Enum_ItemStorageType StorageType;

        public GameBerry.Enum_ItemType ItemType;

        // 스택 가능 여부 (Inventory 타입에서만 의미 있음)
        public bool IsStack;

        // 희귀도
        public GameBerry.Enum_ItemRarity Rarity;

        // 해금/사용 조건 (아직 사용 안함)
        public int UnlockConditionId;
    }

    public class ItemChart : ChartBase
    {
        public ItemInfo[] rows;
        private Dictionary<int, ItemInfo> _idToInfo;

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _idToInfo = new Dictionary<int, ItemInfo>(rows.Length);
            foreach (var r in rows)
            {
                if (r == null) continue;
                _idToInfo[r.ItemId] = r;
            }
        }

        public ItemInfo Get(int itemId)
            => _idToInfo != null && _idToInfo.TryGetValue(itemId, out var v) ? v : null;
    }
}
