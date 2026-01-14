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

    [System.Serializable]
    public class EquipTypeData
    {
        public Enum_EquipType EquipType;
        public Sprite SlotSprite;
    }

    [CreateAssetMenu(fileName = "StaticResource", menuName = "Table/StaticResource", order = 1)]
    public class StaticResourceAsset : ScriptableObject
    {
        public List<RarityColorData> RarityColorDatas = new List<RarityColorData>();
        public List<EquipTypeData> EquipTypeDatas = new List<EquipTypeData>();
    }
}
