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
        [SerializeField] private TMP_Text _statValueDiff;

        //------------------------------------------------------------------------------------
        private void SetStatTitle(Enum_Stat enum_Stat)
        {
            Managers.LocalStringManager.Instance.SetLocalizeText(_statTitle, StatHelper.GetStatDisplayName(enum_Stat));
        }
        //------------------------------------------------------------------------------------
        public void SetStatView(Enum_Stat enum_Stat, double value)
        {
            SetStatTitle(enum_Stat);

            if (_statValue != null)
                _statValue.SetText(StatHelper.FormatStatDisplayValue(enum_Stat, value));

            if (_statValueDiff != null)
                _statValueDiff.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        public void SetStatView(Enum_Stat enum_Stat, double value, double diff)
        {
            SetStatTitle(enum_Stat);

            if (_statValue != null)
                _statValue.SetText(StatHelper.FormatStatDisplayValue(enum_Stat, value));


            if (_statValueDiff != null)
            {
                if (diff > 0)
                {
                    _statValueDiff.SetText($"+{StatHelper.FormatStatDisplayValue(enum_Stat, diff)}");
                    _statValueDiff.color = Color.green;
                }
                else
                { 
                    _statValueDiff.SetText(StatHelper.FormatStatDisplayValue(enum_Stat, diff));
                    _statValueDiff.color = Color.red;
                }

                _statValueDiff.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
    }
}