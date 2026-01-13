using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.UI;

namespace GameBerry.UI
{
    public class UIStatElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statTitle;
        [SerializeField] private TMP_Text _statValue;

        public void SetStatView(Enum_Stat enum_Stat, double value)
        {
            _statTitle?.SetText(enum_Stat.ToString());

            if (_statValue != null)
            {
                bool isPercent = false;
                if (enum_Stat == Enum_Stat.Attack_Inc
                    || enum_Stat == Enum_Stat.Hp_Inc
                    || enum_Stat == Enum_Stat.Defence_Inc
                    || enum_Stat == Enum_Stat.MoveSpeed_Inc
                    || enum_Stat == Enum_Stat.CritChance
                    || enum_Stat == Enum_Stat.CritDmg_Inc)
                    isPercent = true;

                Util.SetCommaFromDoubleFloor(_statValue, isPercent == true ? value * 100.0 : value, isPercent);
            }
        }
    }
}