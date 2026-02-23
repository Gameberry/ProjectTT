using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameBerry.UI
{
    public class UISummonProbabilityGroupElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private TMP_Text _rarityProbText;
        [SerializeField] private Transform _itemRoot;
        [SerializeField] private UISummonProbabilityItemElement _itemPrefab;

        private readonly List<UISummonProbabilityItemElement> _items = new List<UISummonProbabilityItemElement>();

        public struct ItemViewData
        {
            public int ItemId;
            public Enum_Tier Tier;
            public float PercentInRarity;
        }

        public void Bind(Enum_Rarity rarity, float rarityPercent, IReadOnlyList<ItemViewData> entries)
        {
            if (_rarityText != null)
            {
                _rarityText.SetText(rarity.ToString());
                _rarityText.color = StaticResource.Instance.GetRarityTextColor(rarity);
            }

            if (_rarityProbText != null)
                _rarityProbText.SetText(FormatPercent(rarityPercent));

            int count = entries != null ? entries.Count : 0;
            EnsureCount(count);

            for (int i = 0; i < _items.Count; ++i)
            {
                bool active = i < count;
                _items[i].gameObject.SetActive(active);
                if (active)
                {
                    ItemViewData e = entries[i];
                    _items[i].Bind(e.ItemId, e.Tier, e.PercentInRarity);
                }
            }
        }

        private void EnsureCount(int count)
        {
            if (_itemPrefab == null || _itemRoot == null)
                return;

            while (_items.Count < count)
            {
                UISummonProbabilityItemElement item = Instantiate(_itemPrefab, _itemRoot);
                _items.Add(item);
            }
        }

        private static string FormatPercent(float value)
        {
            return $"{value:0.##}%";
        }
    }
}

