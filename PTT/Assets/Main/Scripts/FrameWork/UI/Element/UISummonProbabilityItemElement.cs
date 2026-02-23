using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UISummonProbabilityItemElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _rarityColor;
        [SerializeField] private TMP_Text _tierText;
        [SerializeField] private TMP_Text _probText;

        public void Bind(int itemId, Enum_Tier tier, float percent)
        {
            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(itemId);

            var itemInfo = ItemManager.Instance.GetItemMeta(itemId);
            if (_frame != null && itemInfo != null)
                _frame.sprite = StaticResource.Instance.GetRarityFrame(itemInfo.Rarity);

            if (_rarityColor != null)
                _rarityColor.color = StaticResource.Instance.GetRarityTextColor(itemInfo.Rarity);

            if (_tierText != null)
                _tierText.SetText(tier.ToString());

            if (_probText != null)
                _probText.SetText(FormatPercent(percent));
        }

        private static string FormatPercent(float value)
        {
            if (Mathf.Approximately(value, Mathf.Round(value)))
                return $"{Mathf.RoundToInt(value)}%";

            return $"{value:0.##}%";
        }
    }
}

