using UnityEngine;

namespace GameBerry.UI
{
    [CreateAssetMenu(
        fileName = "DialogAnimationPreset",
        menuName = "GameBerry/UI/Dialog Animation Preset",
        order = 1)]
    public class DialogAnimationPreset : ScriptableObject
    {
        public bool useInAnimation;
        public bool useOutAnimation;

        public IDialogAnimations InAnimation = new IDialogAnimations();
        public IDialogAnimations OutAnimation = new IDialogAnimations();
    }
}
