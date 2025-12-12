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

        private SpineModelData _currentModelData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            Message.AddListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);
            RefreshPlayerSkin(null);

            _currentModelData = Managers.SkinManager.Instance.GetPlayerSpineModelData();

            _skeletonGraphic.skeletonDataAsset = _currentModelData.SkeletonData;
            _skeletonGraphic.Initialize(true);
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            Message.RemoveListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);
        }
        //------------------------------------------------------------------------------------
        private void RefreshPlayerSkin(Event.RefreshPlayerSkinMsg msg)
        {
            Skeleton skeleton = _skeletonGraphic.Skeleton;
            SkeletonData skeletonData = skeleton.Data;

            Skin skin = Managers.SkinManager.Instance.GetRuntimeSkin();

            skeleton.SetSkin(skin);
            skeleton.SetSlotsToSetupPose();

            _skeletonGraphic.AnimationState.SetAnimation(0, "Idle", true);
        }
        //------------------------------------------------------------------------------------
    }
}