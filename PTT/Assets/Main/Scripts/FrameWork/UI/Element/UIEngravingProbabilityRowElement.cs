using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    /// <summary>
    /// 각인 확률표의 한 행을 표시하는 Element
    /// 스탯명 + 등급별(Uncommon/Rare/Epic) 값/확률 표시
    /// </summary>
    public class UIEngravingProbabilityRowElement : MonoBehaviour
    {
        [Header("Stat Name")]
        [SerializeField] private TMP_Text _statNameText;

        [Header("Uncommon Grade")]
        [SerializeField] private TMP_Text _uncommonValueText;
        [SerializeField] private TMP_Text _uncommonProbText;
        [SerializeField] private Image _uncommonBackground;

        [Header("Rare Grade")]
        [SerializeField] private TMP_Text _rareValueText;
        [SerializeField] private TMP_Text _rareProbText;
        [SerializeField] private Image _rareBackground;

        [Header("Epic Grade")]
        [SerializeField] private TMP_Text _epicValueText;
        [SerializeField] private TMP_Text _epicProbText;
        [SerializeField] private Image _epicBackground;

        [Header("Row Background")]
        [SerializeField] private Image _rowBackground;
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _inactiveColor = Color.gray;

        //------------------------------------------------------------------------------------
        public void SetData(
            Enum_Stat statType,
            string uncommonValue, float uncommonProb,
            string rareValue, float rareProb,
            string epicValue, float epicProb)
        {
            // 스탯 이름
            if (_statNameText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_statNameText, StatHelper.GetStatDisplayName(statType));

            // Uncommon 등급
            if (_uncommonValueText != null)
                _uncommonValueText.SetText(uncommonValue);

            if (_uncommonProbText != null)
                _uncommonProbText.SetText(string.Format("{0:F2}%", uncommonProb));

            if (_uncommonBackground != null)
                _uncommonBackground.sprite = StaticResource.Instance.GetRarityFrame(Enum_Rarity.Uncommon);

            // Rare 등급
            if (_rareValueText != null)
                _rareValueText.SetText(rareValue);

            if (_rareProbText != null)
                _rareProbText.SetText(string.Format("{0:F2}%", rareProb));

            if (_rareBackground != null)
                _rareBackground.sprite = StaticResource.Instance.GetRarityFrame(Enum_Rarity.Rare);

            // Epic 등급
            if (_epicValueText != null)
                _epicValueText.SetText(epicValue);

            if (_epicProbText != null)
                _epicProbText.SetText(string.Format("{0:F2}%", epicProb));

            if (_epicBackground != null)
                _epicBackground.sprite = StaticResource.Instance.GetRarityFrame(Enum_Rarity.Epic);
        }
        //------------------------------------------------------------------------------------
        public void SetTargetTier(bool isTargetTier)
        {
            if (_rowBackground != null)
                _rowBackground.color = isTargetTier ? _activeColor : _inactiveColor;
        }
        //------------------------------------------------------------------------------------
    }
}
