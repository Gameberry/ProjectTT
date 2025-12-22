using TMPro;
using UnityEngine;

namespace GameBerry
{
    [CreateAssetMenu(menuName = "CombatTextPreset", fileName = "Table/CombatTextPreset")]
    public class CombatTextPresetAsset : ScriptableObject
    {
        public Material fontMaterial;

        public float fontSize = 36f;
        public FontStyles fontStyle = FontStyles.Bold;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
        public Color color = Color.white;

        public bool autoSize = false;
        public bool wordWrap = false;
        public bool richText = false;
        public bool raycastTarget = false;

        public void ApplyTo(TMP_Text t)
        {
            if (fontMaterial != null) t.fontMaterial = fontMaterial;

            t.fontSize = fontSize;
            t.fontStyle = fontStyle;
            t.alignment = alignment;
            t.color = color;

            t.enableAutoSizing = autoSize;
            t.enableWordWrapping = wordWrap;
            t.richText = richText;
            t.raycastTarget = raycastTarget;
        }
    }
}