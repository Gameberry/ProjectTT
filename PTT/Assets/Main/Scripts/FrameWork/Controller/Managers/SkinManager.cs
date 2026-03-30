using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class SkinManager : Singleton<SkinManager>
    {
        private SpineModelData _characterSpineModel;

        public Event.RefreshPlayerSkinMsg refreshPlayerSkinMsg = new Event.RefreshPlayerSkinMsg();

        private readonly Skin _runtimeSkin = new Skin("runtime-equips");

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _characterSpineModel = StaticResource.Instance.GetCreatureSpineModelData(0);

            SetRuntimeSkin();
        }
        //------------------------------------------------------------------------------------
        #region Chart Func
        //------------------------------------------------------------------------------------
        public Chart.SkinInfo GetSkinInfo(int index) => Chart.GameChart.Get<Chart.SkinChart>()?.Get(index);
        //------------------------------------------------------------------------------------
        public List<Chart.SkinInfo> GetSkinSlotInfoList(Enum_SkinSlotType skinSlotType) => Chart.GameChart.Get<Chart.SkinChart>()?.GetSkinSlotInfoList(skinSlotType);
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Table Func
        //------------------------------------------------------------------------------------
        public void CapyEquipSkinDict(ref Dictionary<Enum_SkinSlotType, int> data) => Table.UserTable.Get<Table.SkinTable>()?.CapyEquipSkinDict(ref data);
        //------------------------------------------------------------------------------------
        public Table.SkinData GetSkinData(int index) => Table.UserTable.Get<Table.SkinTable>()?.GetSkinData(index);
        //------------------------------------------------------------------------------------
        public Table.SkinData GetSkinEquipData(Enum_SkinSlotType skinSlotType) => Table.UserTable.Get<Table.SkinTable>()?.GetSkinEquipData(skinSlotType);
        //------------------------------------------------------------------------------------
        #endregion
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

            SkeletonData skeletonData = _characterSpineModel.SkeletonData.GetSkeletonData(true);

            // 스킨구조 다시 잡히기 전까지 주석
            // for (int i = 0; i < (int)Enum_SkinSlotType.Max; ++i)
            // {
            //     Enum_SkinSlotType skinSlotType = (Enum_SkinSlotType)i;

            //     Table.SkinData skinData = skinTable.GetSkinEquipData(skinSlotType);

            //     string skinname = string.Empty;

            //     if (skinData == null)
            //         skinname = _characterSpineModel.DefaultSkin(skinSlotType);
            //     else
            //         skinname = skinChart.GetSkinName(skinData.itemId);

            //     if (string.IsNullOrEmpty(skinname))
            //         continue;

            //     _runtimeSkin.AddSkin(skeletonData.FindSkin(skinname));
            // }

            for (int i = 0; i < (int)Enum_SkinSlotType.Max; ++i)
            {
                Enum_SkinSlotType skinSlotType = (Enum_SkinSlotType)i;

                List<string> skinNames = _characterSpineModel.DefaultSkin(skinSlotType);

                if (skinNames == null || skinNames.Count == 0)
                    continue;

                foreach (string skinName in skinNames)
                {
                    Skin part = skeletonData.FindSkin(skinName);
                    if (part == null)
                        continue;

                    _runtimeSkin.AddSkin(part);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetDynamicSkin(Dictionary<Enum_SkinSlotType, int> skindata, ref Skin skin)
        {
            if (_characterSpineModel == null)
                return;

            skin.Clear();

            Table.SkinTable skinTable = Table.UserTable.Get<Table.SkinTable>();
            Chart.SkinChart skinChart = Chart.GameChart.Get<Chart.SkinChart>();

            SkeletonData skeletonData = _characterSpineModel.SkeletonData.GetSkeletonData(true);

            // 스킨구조 다시 잡히기 전까지 주석
            // for (int i = 0; i < (int)Enum_SkinSlotType.Max; ++i)
            // {
            //     Enum_SkinSlotType skinSlotType = (Enum_SkinSlotType)i;

            //     string skinname = string.Empty;

            //     if (skindata.ContainsKey(skinSlotType) == true)
            //         skinname = skinChart.GetSkinName(skindata[skinSlotType]);
            //     else
            //         skinname = _characterSpineModel.DefaultSkin(skinSlotType);

            //     if (string.IsNullOrEmpty(skinname))
            //         continue;

            //     skin.AddSkin(skeletonData.FindSkin(skinname));
            // }

            for (int i = 0; i < (int)Enum_SkinSlotType.Max; ++i)
            {
                Enum_SkinSlotType skinSlotType = (Enum_SkinSlotType)i;

                List<string> skinNames = _characterSpineModel.DefaultSkin(skinSlotType);

                if (skinNames == null || skinNames.Count == 0)
                    continue;

                foreach (string skinName in skinNames)
                {
                    Skin part = skeletonData.FindSkin(skinName);
                    if (part == null)
                        continue;

                    _runtimeSkin.AddSkin(part);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public Skin GetRuntimeSkin() => _runtimeSkin;
        //------------------------------------------------------------------------------------
        public void UnequipSlotSkin(Enum_SkinSlotType slot)
        {
            Table.UserTable.Get<Table.SkinTable>()?.UnequipSlotSkin(slot);
            Table.UserTable.Get<Table.SkinTable>()?.UpdateTable();

            SetRuntimeSkin();

            Message.Send(refreshPlayerSkinMsg);
        }
        //------------------------------------------------------------------------------------
        public void EquipSlotSkin(Enum_SkinSlotType slot, int index)
        {
            Table.UserTable.Get<Table.SkinTable>()?.EquipSlotSkin(slot, index);
            Table.UserTable.Get<Table.SkinTable>()?.UpdateTable();

            SetRuntimeSkin();

            Message.Send(refreshPlayerSkinMsg);
        }
        //------------------------------------------------------------------------------------
        public bool TryGetSkinByItem(int skinItemId, bool immediateServerUpdate = true)
        {
            var res = ItemManager.Instance.AddItem(skinItemId, 1, immediateServerUpdate);
            return res.Success && res.Added > 0;
        }
        //------------------------------------------------------------------------------------
    }
}

