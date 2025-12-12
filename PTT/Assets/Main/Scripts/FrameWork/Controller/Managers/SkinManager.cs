using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class SkinManager : MonoSingleton<SkinManager>
    {
        private SpineModelData _characterSpineModel;

        public Event.RefreshPlayerSkinMsg refreshPlayerSkinMsg = new Event.RefreshPlayerSkinMsg();

        private List<Table.TableBase> TransTest = new List<Table.TableBase>()
        {
            Table.UserTable.Get<Table.SkinTable>()
        };

        private readonly Skin _runtimeSkin = new Skin("runtime-equips");

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _characterSpineModel = StaticResource.Instance.GetCreatureSpineModelData(0);

            SetRuntimeSkin();
        }
        //------------------------------------------------------------------------------------
        public SpineModelData GetPlayerSpineModelData()
        {
            return _characterSpineModel;
        }
        //------------------------------------------------------------------------------------
        private void SetRuntimeSkin()
        {
            if (_characterSpineModel == null)
                return;

            _runtimeSkin.Clear();

            Table.SkinTable skinTable = Table.UserTable.Get<Table.SkinTable>();
            Chart.SkinChart skinChart = Chart.GameChart.Get<Chart.SkinChart>();

            for (int i = 0; i < (int)SkinSlotType.Max; ++i)
            {
                SkinSlotType skinSlotType = (SkinSlotType)i;

                Table.SkinData skinData = skinTable.GetSkinEquipData(skinSlotType);

                string skinname = string.Empty;

                if (skinData == null)
                    skinname = _characterSpineModel.DefaultSkin(skinSlotType);
                else
                    skinname = skinChart.GetSkinName(skinData.index);

                if (string.IsNullOrEmpty(skinname))
                    continue;

                _runtimeSkin.AddSkin(_characterSpineModel.SkeletonData.GetSkeletonData(true).FindSkin(skinname));
            }
        }
        //------------------------------------------------------------------------------------
        public Skin GetRuntimeSkin() => _runtimeSkin;
        //------------------------------------------------------------------------------------
        public void UnequipSlotSkin(SkinSlotType slot)
        {
            Table.UserTable.Get<Table.SkinTable>()?.UnequipSlotSkin(slot);
            Table.UserTable.Get<Table.SkinTable>()?.UpdateTable();

            SetRuntimeSkin();

            Message.Send(refreshPlayerSkinMsg);
        }
        //------------------------------------------------------------------------------------
        public void EquipSlotSkin(SkinSlotType slot, string skinName)
        {
            //Table.UserTable.Get<Table.SkinTable>()?.EquipSlotSkin(slot, skinName);
            Table.UserTable.Get<Table.SkinTable>()?.UpdateTable();

            SetRuntimeSkin();

            Message.Send(refreshPlayerSkinMsg);
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.H))
            {
                Table.UserTable.UpdateTable<Table.SkinTable>();
            }

            if (Input.GetKeyUp(KeyCode.J))
            {
                Table.UserTable.DynamicUpdateData(TransTest);
            }

            if (Input.GetKeyUp(KeyCode.K))
            {
                Table.SkinTable skinTable = Table.UserTable.Get<Table.SkinTable>();
                Table.UserTable.DynamicUpdateData(TransTest);
            }
        }
        //------------------------------------------------------------------------------------
    }
}

