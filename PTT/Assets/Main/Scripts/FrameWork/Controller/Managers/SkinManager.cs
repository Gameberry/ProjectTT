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

//        private readonly Type[] SaveTargets = new Type[]
//{
//        typeof(SkinTable),
//// 필요한 table을 여기에 추가하면 끝
//};

        private List<Table.TableBase> TransTest = new List<Table.TableBase>()
        {
            Table.UserTable.Get<Table.SkinTable>()
        };

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
        public void UnequipSlotSkin(SkinSlotType slot)
        {
            _skeletonAnimationHandler?.UnequipSlotSkin(slot);
        }
        //------------------------------------------------------------------------------------
        public void EquipSlotSkin(SkinSlotType slot, string skinName)
        {
            _skeletonAnimationHandler?.EquipSlotSkin(slot, skinName);
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.H))
            {
                Table.UserTable.Get<Table.SkinTable>().test++;
                Table.UserTable.UpdateTable<Table.SkinTable>();
            }

            if (Input.GetKeyUp(KeyCode.J))
            {
                Table.UserTable.Get<Table.SkinTable>().test++;
                Table.UserTable.DynamicUpdateData(TransTest);
            }
        }
        //------------------------------------------------------------------------------------
    }
}

