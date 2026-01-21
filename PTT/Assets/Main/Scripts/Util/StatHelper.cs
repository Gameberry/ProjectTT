using UnityEngine;
using System.Globalization;
using System.Collections.Generic;

namespace GameBerry
{
    public static class StatHelper
    {
        private const string _statNameLocalKey = "stat/{0}/name";

        private static Dictionary<Enum_Stat, string> _statNameLocalKey_Dict = new Dictionary<Enum_Stat, string>();

        public static string GetStatDisplayName(Enum_Stat stat)
        {
            if (_statNameLocalKey_Dict.TryGetValue(stat, out var localkey))
                return localkey;

            //localkey = string.Format(_statNameLocalKey, stat.ToString());
            //юс╫ц
            localkey = stat.ToString();
            _statNameLocalKey_Dict.Add(stat, localkey);
            return localkey;
        }
        //------------------------------------------------------------------------------------
        public static string FormatStatDisplayValue(Enum_Stat stat, double value)
        {
            bool isPercent = IsPercent(stat);

            if (isPercent)
            {
                double percent = value * 100.0;

                return percent % 1 == 0
        ? percent.ToString("0", CultureInfo.InvariantCulture) + "%"
        : percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";
            }
            else
                return value.ToString("0", CultureInfo.InvariantCulture);
        }
        //------------------------------------------------------------------------------------
        public static bool IsPercent(Enum_Stat stat)
        {
            if (stat == Enum_Stat.Attack_Inc
    || stat == Enum_Stat.Hp_Inc
    || stat == Enum_Stat.Defence_Inc
    || stat == Enum_Stat.MoveSpeed_Inc
    || stat == Enum_Stat.AttackSpeed_Inc
    || stat == Enum_Stat.CritChance
    || stat == Enum_Stat.CritDmg_Inc
    || stat == Enum_Stat.MinDamagePer
    || stat == Enum_Stat.MaxDamagePer
    || stat == Enum_Stat.FinalDamage)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
    }
}