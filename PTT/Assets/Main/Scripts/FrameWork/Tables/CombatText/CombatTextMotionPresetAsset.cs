using TMPro;
using UnityEngine;

namespace GameBerry
{
    [CreateAssetMenu(menuName = "CombatTextMotionPresetAsset", fileName = "Table/CombatTextMotionPresetAsset")]
    public class CombatTextMotionPresetAsset : ScriptableObject
    {
        [Min(0.05f)] public float lifeTime = 0.7f;
        public float rise = 0.6f;
        public float baseScale = 1.0f;

        public float popAmount = 0.35f;
        [Range(0.01f, 0.5f)] public float popInNormalized = 0.12f;

        public float shakeAmplitudePx = 12f;
        public float shakeFrequency = 28f;

        [Min(0f)] public float comboHideDelay = 1.5f;
    }
}