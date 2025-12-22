using TMPro;
using UnityEngine;

namespace GameBerry
{
    [CreateAssetMenu(menuName = "CombatTextPreset", fileName = "Table/CombatTextPreset")]
    public class CombatTextPresetAsset : ScriptableObject
    {
        [Header("Text (Batching-friendly)")]
        public Material fontMaterial; // 같은 걸 공유하면 배칭에 유리
        public float fontSize = 36f;
        public FontStyles fontStyle = FontStyles.Normal;
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
        public Color color = Color.white;

        [Header("TMP Options (Performance)")]
        public bool autoSize = false;       // OFF 권장
        public bool wordWrap = false;       // OFF 권장
        public bool richText = false;       // OFF 권장
        public bool raycastTarget = false;  // OFF 권장

        [Header("Animation")]
        [Min(0.05f)] public float lifeTime = 0.7f;
        public float rise = 0.6f;

        [Header("Scale")]
        public float baseScale = 1.0f;

        [Header("Pop (Critical)")]
        public float popAmount = 0.35f;
        [Range(0.01f, 0.5f)] public float popInNormalized = 0.12f; // 수명 대비 비율

        [Header("Shake (Critical)")]
        public float shakeAmplitudePx = 12f;
        public float shakeFrequency = 28f;
    }
}