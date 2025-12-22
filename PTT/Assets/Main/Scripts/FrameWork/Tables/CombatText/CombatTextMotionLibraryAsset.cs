using TMPro;
using UnityEngine;

namespace GameBerry
{
    [CreateAssetMenu(menuName = "CombatTextMotionLibraryAsset", fileName = "Table/CombatTextMotionLibraryAsset")]
    public class CombatTextMotionLibraryAsset : ScriptableObject
    {
        public CombatTextMotionPresetAsset miss;
        public CombatTextMotionPresetAsset critical;
        public CombatTextMotionPresetAsset damage;
        public CombatTextMotionPresetAsset combo;
    }
}