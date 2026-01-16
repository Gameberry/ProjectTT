using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIStarforceResultStatElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statNameText;
        [SerializeField] private TMP_Text _beforeValueText;
        [SerializeField] private Image _arrowImage;
        [SerializeField] private TMP_Text _afterValueText;

        [SerializeField] private Color _increaseColor = new Color(0.2f, 0.8f, 0.2f);

        //------------------------------------------------------------------------------------
        public void SetStat(Enum_Stat stat, double beforeValue, double afterValue)
        {
            if (_statNameText != null)
            {
                string statName = GetStatDisplayName(stat);
                _statNameText.SetText(statName);
            }

            if (_beforeValueText != null)
            {
                _beforeValueText.SetText(FormatStatValue(stat, beforeValue));
            }

            if (_afterValueText != null)
            {
                _afterValueText.SetText(FormatStatValue(stat, afterValue));
                _afterValueText.color = afterValue > beforeValue ? _increaseColor : Color.white;
            }
        }
        //------------------------------------------------------------------------------------
        private string GetStatDisplayName(Enum_Stat stat)
        {
            return stat switch
            {
                Enum_Stat.Attack => "공격력",
                Enum_Stat.HP => "최대 HP",
                Enum_Stat.Defence => "방어력",
                Enum_Stat.MoveSpeed => "이동속도",
                Enum_Stat.AttackSpeed => "공격속도",
                Enum_Stat.Attack_Inc => "공격력 증가",
                Enum_Stat.Hp_Inc => "HP 증가",
                Enum_Stat.Defence_Inc => "방어력 증가",
                Enum_Stat.MoveSpeed_Inc => "이동속도 증가",
                Enum_Stat.AttackSpeed_Inc => "공격속도 증가",
                Enum_Stat.CritChance => "크리티컬 확률",
                Enum_Stat.CritDmg_Inc => "크리티컬 데미지",
                Enum_Stat.Evasion => "회피",
                Enum_Stat.Accuracy => "명중",
                Enum_Stat.HpRecovery => "HP 회복",
                _ => stat.ToString()
            };
        }
        //------------------------------------------------------------------------------------
        private string FormatStatValue(Enum_Stat stat, double value)
        {
            // 퍼센트 계열 스탯
            if (stat == Enum_Stat.Attack_Inc || stat == Enum_Stat.Hp_Inc ||
                stat == Enum_Stat.Defence_Inc || stat == Enum_Stat.MoveSpeed_Inc ||
                stat == Enum_Stat.AttackSpeed_Inc || stat == Enum_Stat.CritChance ||
                stat == Enum_Stat.CritDmg_Inc || stat == Enum_Stat.HpRecovery)
            {
                return $"{value:P1}";
            }

            // 소수점 스탯
            if (stat == Enum_Stat.MoveSpeed || stat == Enum_Stat.AttackSpeed)
            {
                return $"{value:F2}";
            }

            // 정수 스탯
            return $"{value:N0}";
        }
        //------------------------------------------------------------------------------------
    }
}
