using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine.UI;

namespace GameBerry
{
    public class StaticResource : MonoSingleton<StaticResource>
    {
        [SerializeField]
        private StaticResourceAsset _staticResourceAsset;

        [SerializeField]
        private SpineModelAsset _creatureSpineModelAsset;

        private Dictionary<int, SpineModelData> _creatureSpineModelDatas_Dic = new Dictionary<int, SpineModelData>();

        [SerializeField]
        private BattleModeStaticDataAsset _battleModeStaticDataAsset;

        [SerializeField]
        private ConditionDataAsset _conditionDataAsset;

        [SerializeField]
        private ComboDataAsset _comboDataAsset;

        [SerializeField]
        private StageDataAsset _stageDataAsset;

        [SerializeField]
        private IconTableAsset _iconTableAsset;

        [SerializeField]
        private SoundTableAsset _soundTableAsset;


        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            for (int i = 0; i < _creatureSpineModelAsset.SpineModelDatas.Count; ++i)
            {
                SpineModelData spineModelData = _creatureSpineModelAsset.SpineModelDatas[i];

                _creatureSpineModelDatas_Dic.Add(spineModelData.ResourceIndex, spineModelData);
            }
        }
        //------------------------------------------------------------------------------------
        #region Color
        //------------------------------------------------------------------------------------
        public RarityColorData GetRarityColorData(Enum_Rarity enum_Rarity)
        {
            return _staticResourceAsset.RarityColorDatas.Find(x => x.Rarity == enum_Rarity);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetRarityFrame(Enum_Rarity enum_Rarity)
        {
            RarityColorData rarityColorData = GetRarityColorData(enum_Rarity);
            if (rarityColorData == null)
                return null;

            return rarityColorData.FrameSprite;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SpineModel
        //------------------------------------------------------------------------------------
        public SpineModelAsset GetSpineModelAsset()
        {
            return _creatureSpineModelAsset;
        }
        //------------------------------------------------------------------------------------
        public SpineModelData GetCreatureSpineModelData(int index)
        {
            if (_creatureSpineModelDatas_Dic.ContainsKey(index) == true)
                return _creatureSpineModelDatas_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        public BattleModeStaticDataAsset GetBattleModeStaticData()
        {
            return _battleModeStaticDataAsset;
        }
        //------------------------------------------------------------------------------------
        public ConditionDataAsset GetConditionData()
        {
            return _conditionDataAsset;
        }
        //------------------------------------------------------------------------------------
        public ComboDataAsset GetComboData()
        {
            return _comboDataAsset;
        }
        //------------------------------------------------------------------------------------
        public StageDataAsset GetStageData()
        {
            return _stageDataAsset;
        }
        //------------------------------------------------------------------------------------
        #region Icon
        //------------------------------------------------------------------------------------
        public Sprite GetIcon(string key)
        {
            if (_iconTableAsset == null)
                return null;

            return _iconTableAsset.GetIcon(key);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region SoundTable
        //------------------------------------------------------------------------------------
        public SoundTableAsset GetSoundTableAsset()
        {
            return _soundTableAsset;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}