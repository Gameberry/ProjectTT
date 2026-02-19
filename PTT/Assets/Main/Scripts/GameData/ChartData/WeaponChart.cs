using System;
using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class WeaponInfo
    {
        public int ItemId;
        public int MaxAwake;
        public string EquipStat;
        public string EquipBonusStat;
        public string OwnStat;

        [NonSerialized] private Dictionary<Enum_Stat, double> _equipStatDict;

        public IReadOnlyDictionary<Enum_Stat, double> GetEquipStats()
        {
            if (_equipStatDict == null)
                _equipStatDict = StatHelper.ParseStatsPacked(EquipStat);
            return _equipStatDict;
        }


        [NonSerialized] private Dictionary<Enum_Stat, double> _equipBonusStatDict;

        public IReadOnlyDictionary<Enum_Stat, double> GetEquipBonusStats()
        {
            if (_equipBonusStatDict == null)
                _equipBonusStatDict = StatHelper.ParseStatsPacked(EquipBonusStat);
            return _equipBonusStatDict;
        }


        [NonSerialized] private Dictionary<Enum_Stat, double> _ownStatDict;

        public IReadOnlyDictionary<Enum_Stat, double> GetOwnStats()
        {
            if (_ownStatDict == null)
                _ownStatDict = StatHelper.ParseStatsPacked(OwnStat);
            return _ownStatDict;
        }
    }

    public class WeaponChart : ChartBase
    {
        public WeaponInfo[] rows;
        private Dictionary<int, WeaponInfo> _itemIdToInfo;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _itemIdToInfo = new Dictionary<int, WeaponInfo>(rows.Length);
            foreach (var r in rows)
            {
                if (r == null) continue;
                _itemIdToInfo[r.ItemId] = r;
            }
        }

        public WeaponInfo Get(int itemId)
    => _itemIdToInfo != null && _itemIdToInfo.TryGetValue(itemId, out var v) ? v : null;
    }

}