using UnityEngine;

namespace GameBerry
{
    [CreateAssetMenu(menuName = "CombatTextPresetLibraryAsset", fileName = "Table/CombatTextPresetLibraryAsset")]
    public class CombatTextPresetLibraryAsset : ScriptableObject
    {
        public CombatTextPresetAsset Miss;
        public CombatTextPresetAsset Critical;
        public CombatTextPresetAsset Combo;
        public CombatTextPresetAsset Damage;
    }
}