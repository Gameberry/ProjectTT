using UnityEngine;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    public class UIPlayerSpineObject : MonoBehaviour
    {
        [Header("-------Spine-------")]
        [SerializeField]
        private SkeletonGraphic _skeletonGraphic;

        [SerializeField]
        private bool _addMessage = true;

        private SpineModelData _currentModelData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_addMessage == true)
                Message.AddListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);

            _currentModelData = Managers.SkinManager.Instance.GetPlayerSpineModelData();

            _skeletonGraphic.skeletonDataAsset = _currentModelData.SkeletonData;
            _skeletonGraphic.initialSkinName = _currentModelData.SkinList[0];
            _skeletonGraphic.Initialize(true);

            RefreshPlayerSkin(null);
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            if (_addMessage == true)
                Message.RemoveListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);
        }
        //------------------------------------------------------------------------------------
        private void RefreshPlayerSkin(Event.RefreshPlayerSkinMsg msg)
        {
            SetSkin(Managers.SkinManager.Instance.GetRuntimeSkin());
        }
        //------------------------------------------------------------------------------------
        public void SetSkin(Skin skin)
        {
            if (skin == null)
                return;

            Skeleton skeleton = _skeletonGraphic.Skeleton;

            skeleton.SetSkin(skin);
            skeleton.SetSlotsToSetupPose();

            _skeletonGraphic.AnimationState.SetAnimation(0, _currentModelData.GetAnimationName(CharacterState.Idle), true);
        }
        //------------------------------------------------------------------------------------
    }
}