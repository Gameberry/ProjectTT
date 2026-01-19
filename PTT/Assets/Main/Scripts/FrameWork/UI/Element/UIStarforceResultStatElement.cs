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
                Managers.LocalStringManager.Instance.SetLocalizeText(_statNameText, StatHelper.GetStatDisplayName(stat));
            }

            if (_beforeValueText != null)
            {
                _beforeValueText.SetText(StatHelper.FormatStatDisplayValue(stat, beforeValue));
            }

            if (_afterValueText != null)
            {
                _afterValueText.SetText(StatHelper.FormatStatDisplayValue(stat, afterValue));
                _afterValueText.color = afterValue > beforeValue ? _increaseColor : Color.white;
            }
        }
        //------------------------------------------------------------------------------------
    }
}
