using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gpm.Ui;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class LobbyCharacterJobUpgradeCompleteDialog : IDialog
    {
        [Header("-------Barbar-------")]
        [SerializeField]
        private SkeletonGraphic _skeletonGraphic;

        protected override void OnEnter()
        {
            SpineModelData _currentSpineModelData = StaticResource.Instance.GetARRRSpineModelData();

            Skin myEquipsSkin = new Skin("my new skin");

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.skeletonDataAsset = _currentSpineModelData.SkeletonData;
                _skeletonGraphic.initialSkinName = _currentSpineModelData.SkinList[0];
                _skeletonGraphic.Initialize(true);

                _skeletonGraphic.Skeleton.SetSkin(_currentSpineModelData.SkinList[0]);

                Skeleton skeleton = _skeletonGraphic.Skeleton;
                SkeletonData skeletonData = skeleton.Data;

                myEquipsSkin.SetARRRSkin(skeletonData);

                skeleton.SetSkin(myEquipsSkin);
                skeleton.SetSlotsToSetupPose();

                _skeletonGraphic.AnimationState.SetAnimation(0, "Idle", true);
            }
        }
    }
}