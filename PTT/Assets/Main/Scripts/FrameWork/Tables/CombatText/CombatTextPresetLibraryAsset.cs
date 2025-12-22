using UnityEngine;

namespace GameBerry
{
    [CreateAssetMenu(menuName = "CombatTextPresetLibraryAsset", fileName = "Table/CombatTextPresetLibraryAsset")]
    public class CombatTextPresetLibraryAsset : ScriptableObject
    {
        public CombatTextPresetAsset miss;
        public CombatTextPresetAsset critical;
        public CombatTextPresetAsset damage;

        public CombatTextPresetAsset comboNumber;
        public CombatTextPresetAsset comboLabel;
    }
}