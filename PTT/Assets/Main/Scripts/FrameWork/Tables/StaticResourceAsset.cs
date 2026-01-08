using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace GameBerry
{
    [System.Serializable]
    public class RarityColorData
    {
        public Enum_Rarity Rarity;
        public Color TextColor = Color.white;
        public Sprite FrameSprite;
    }

    [CreateAssetMenu(fileName = "StaticResource", menuName = "Table/StaticResource", order = 1)]
    public class StaticResourceAsset : ScriptableObject
    {
        public List<RarityColorData> RarityColorDatas = new List<RarityColorData>();
    }
}
