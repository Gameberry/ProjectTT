using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class SkinManager : MonoSingleton<SkinManager>
    {
        private SpineModelData _characterSpineModel;

        public SkeletonAnimationHandler _skeletonAnimationHandler;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _characterSpineModel = StaticResource.Instance.GetCreatureSpineModelData(0);
        }
        //------------------------------------------------------------------------------------
        public SpineModelData GetPlayerSpineModelData()
        {
            return _characterSpineModel;
        }
        //------------------------------------------------------------------------------------
        public void SetTempPlayerSpineHandler(SkeletonAnimationHandler skeletonAnimationHandler)
        {
            _skeletonAnimationHandler = skeletonAnimationHandler;
        }
        //------------------------------------------------------------------------------------
        public void UnequipSlotSkin(SpineEquipSlot slot)
        {
            _skeletonAnimationHandler?.UnequipSlotSkin(slot);
        }
        //------------------------------------------------------------------------------------
        public void EquipSlotSkin(SpineEquipSlot slot, string skinName)
        {
            _skeletonAnimationHandler?.EquipSlotSkin(slot, skinName);
        }
        //------------------------------------------------------------------------------------
    }
}

