using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIStarforceStatElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statNameText;
        [SerializeField] private TMP_Text _currentValueText;
        [SerializeField] private TMP_Text _equipValueText;
        [SerializeField] private TMP_Text _addValueText;
        [SerializeField] private TMP_Text _diffValueText;

        //------------------------------------------------------------------------------------
        public void SetStat(Enum_Stat stat, double currentValue, double equipValue, double addValue, double diff)
        {
            if (_statNameText != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(_statNameText, StatHelper.GetStatDisplayName(stat));
            }

            if (_currentValueText != null)
                _currentValueText.SetText(StatHelper.FormatStatDisplayValue(stat, currentValue));

            if (_equipValueText != null)
                _equipValueText.SetText(StatHelper.FormatStatDisplayValue(stat, equipValue));

            if (_addValueText != null)
                _addValueText.SetText($"+{StatHelper.FormatStatDisplayValue(stat, addValue)}");

            if (_diffValueText != null)
            {
                if (diff > 0)
                {
                    _diffValueText.SetText($"+{StatHelper.FormatStatDisplayValue(stat, diff)}");
                    _diffValueText.gameObject.SetActive(true);
                }
                else
                {
                    _diffValueText.gameObject.SetActive(false);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}
