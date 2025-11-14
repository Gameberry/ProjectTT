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

        private Dictionary<V2Enum_Grade, V2GradeColorData> _v2GradeColorDatas_Dic = new Dictionary<V2Enum_Grade, V2GradeColorData>();

        [SerializeField]
        private SpineModelAsset _creatureSpineModelAsset;

        private Dictionary<int, SpineModelData> _creatureSpineModelDatas_Dic = new Dictionary<int, SpineModelData>();

        [SerializeField]
        private SpineModelAsset _arrrSpineModelAsset;

        [SerializeField]
        private BattleModeStaticDataAsset _battleModeStaticDataAsset;

        [SerializeField]
        private IconTableAsset _iconTableAsset;

        [SerializeField]
        private SoundTableAsset _soundTableAsset;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            List<V2GradeColorData> v2GradeColorDatas = _staticResourceAsset.GradeColorDatas;

            for (int i = 0; i < v2GradeColorDatas.Count; ++i)
            {
                if (_v2GradeColorDatas_Dic.ContainsKey(v2GradeColorDatas[i].v2Enum_Grade) == false)
                {
                    _v2GradeColorDatas_Dic.Add(v2GradeColorDatas[i].v2Enum_Grade, v2GradeColorDatas[i]);
                }
            }

            for (int i = 0; i < _creatureSpineModelAsset.SpineModelDatas.Count; ++i)
            {
                SpineModelData spineModelData = _creatureSpineModelAsset.SpineModelDatas[i];

                _creatureSpineModelDatas_Dic.Add(spineModelData.ResourceIndex, spineModelData);
            }
        }
        //------------------------------------------------------------------------------------
        #region Frame
        //------------------------------------------------------------------------------------
        public void SetFrame(V2Enum_Grade v2Enum_Grade, Image frame)
        {
            if (frame == null || _staticResourceAsset == null)
                return;

            _staticResourceAsset.elementFrameResourceData.SetFrame(v2Enum_Grade, frame);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Color
        //------------------------------------------------------------------------------------
        public V2SkillTriggerColorData GetV2SkillTriggerColorData(Enum_TriggerType Enum_TriggerType)
        {
            return _staticResourceAsset.SkillTriggerColorDatas.Find(x => x.SkillTriggerType == Enum_TriggerType);
        }
        //------------------------------------------------------------------------------------
        public V2GradeColorData GetV2GradeColorData(V2Enum_Grade v2Enum_Grade)
        {
            if (_v2GradeColorDatas_Dic.ContainsKey(v2Enum_Grade) == true)
                return _v2GradeColorDatas_Dic[v2Enum_Grade];

            return new V2GradeColorData();
        }
        //------------------------------------------------------------------------------------
        public Color GetV2GradeColor(V2Enum_Grade v2Enum_Grade)
        {
            V2GradeColorData v2GradeColorData = null;

            if (_v2GradeColorDatas_Dic.TryGetValue(v2Enum_Grade, out v2GradeColorData) == true)
                return v2GradeColorData.GradeColor;

            return Color.white;
        }
        //------------------------------------------------------------------------------------
        public Color GetV2GradeTextColor(V2Enum_Grade v2Enum_Grade)
        {
            V2GradeColorData v2GradeColorData = null;

            if (_v2GradeColorDatas_Dic.TryGetValue(v2Enum_Grade, out v2GradeColorData) == true)
                return v2GradeColorData.GradeTextColor;

            return Color.white;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetV2GradeTextSprite(V2Enum_Grade v2Enum_Grade)
        {
            V2GradeColorData v2GradeColorData = null;

            if (_v2GradeColorDatas_Dic.TryGetValue(v2Enum_Grade, out v2GradeColorData) == true)
                return v2GradeColorData.GradeTextImage;

            return null;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetV2GradeBGSprite(V2Enum_Grade v2Enum_Grade)
        {
            V2GradeColorData v2GradeColorData = null;

            if (_v2GradeColorDatas_Dic.TryGetValue(v2Enum_Grade, out v2GradeColorData) == true)
                return v2GradeColorData.GradeBGImage;

            return null;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetV2GradeSprite(V2Enum_Grade v2Enum_Grade)
        {
            V2GradeColorData v2GradeColorData = null;

            if (_v2GradeColorDatas_Dic.TryGetValue(v2Enum_Grade, out v2GradeColorData) == true)
                return v2GradeColorData.GradeSprite;

            return null;
        }
        //------------------------------------------------------------------------------------
        public Gradient GetHitColorGradient()
        {
            return _staticResourceAsset.HitColorGradient;
        }
        //------------------------------------------------------------------------------------
        public float GetHitRecoveryTime()
        {
            return _staticResourceAsset.HitRecoveryTime;
        }
        //------------------------------------------------------------------------------------
        public Gradient GetDeadColorGradient()
        {
            return _staticResourceAsset.DeadColorGradient;
        }
        //------------------------------------------------------------------------------------
        public float GetDeadDirectionTime()
        {
            return _staticResourceAsset.DeadDirectionTime;
        }
        //------------------------------------------------------------------------------------
        public float GetDeadDirectionSwingForce()
        {
            return _staticResourceAsset.DeadDirectionSwingForce;
        }
        //------------------------------------------------------------------------------------
        public float GetDeadDirectionSwingRotationSpeed()
        {
            return _staticResourceAsset.DeadDirectionSwingRotationSpeed;
        }
        //------------------------------------------------------------------------------------
        public GearResourceData GetGearResourceData(V2Enum_GearType v2Enum_GearType)
        {
            return _staticResourceAsset.GearResourceDatas.Find(x => x.v2Enum_GearType == v2Enum_GearType);
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
        public SpineModelData GetARRRSpineModelData()
        {
            return _arrrSpineModelAsset.SpineModelDatas[0];
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        public BattleModeStaticDataAsset GetBattleModeStaticData()
        {
            return _battleModeStaticDataAsset;
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