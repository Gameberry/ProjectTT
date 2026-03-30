using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    [System.Serializable]
    public class SpineModelAnimationData
    {
        public string stateName;
        public Spine.Animation animation;
    }

    [System.Serializable]
    public class SpineModelStateAnimationNameData
    {
        public CharacterState characterState;
        public string animationName;
    }

    [System.Serializable]
    public class SpineDefaultSlotSkin
    {
        public Enum_SkinSlotType Slot;
        [SpineSkin(dataField = "SkeletonData")]
        public List<string> SkinName;
    }

    [System.Serializable]
    public class SpineModelData
    {
        public int ResourceIndex;

        public SkeletonDataAsset SkeletonData;

        public string Name;

        [SpineSkin(dataField = "SkeletonData")]
        public List<string> SkinList = new List<string>();

        public List<SpineModelAnimationData> AnimationList = new List<SpineModelAnimationData>();
        public Dictionary<string, SpineModelAnimationData> AnimationList_Dic = new Dictionary<string, SpineModelAnimationData>();

        public List<SpineModelStateAnimationNameData> AnimationState = new List<SpineModelStateAnimationNameData>();

        [ArrayElementTitle("Slot")]
        public List<SpineDefaultSlotSkin> DefaultSlotSkins =
            new List<SpineDefaultSlotSkin>();

        public List<string> DefaultSkin(Enum_SkinSlotType skinSlotType)
        {
            SpineDefaultSlotSkin spineDefaultSlotSkin = DefaultSlotSkins.Find(x => x.Slot == skinSlotType);
            if (spineDefaultSlotSkin == null)
                return null;

            return spineDefaultSlotSkin.SkinName;
        }

        public string GetAnimationName(CharacterState characterState)
        {
            SpineModelStateAnimationNameData spineModelStateAnimationNameData = AnimationState.Find(x => x.characterState == characterState);
            return spineModelStateAnimationNameData == null ? string.Empty : spineModelStateAnimationNameData.animationName;
        }
    }

    [CreateAssetMenu(fileName = "SpineModel", menuName = "Table/SpineModel", order = 1)]
    public class SpineModelAsset : ScriptableObject
    {
        [ArrayElementTitle("Name")]
        public List<SpineModelData> SpineModelDatas = new List<SpineModelData>();

        //------------------------------------------------------------------------------------
        void OnValidate()
        {
            for (int i = 0; i < SpineModelDatas.Count; ++i)
            {
                SpineModelData spineModelData = SpineModelDatas[i];
                if (spineModelData.SkeletonData == null)
                    continue;

                var skeletonData = spineModelData.SkeletonData.GetSkeletonData(true);
                if (skeletonData == null)
                    continue;

                spineModelData.Name = spineModelData.SkeletonData.name.ToLower();

                spineModelData.SkinList.Clear();
                foreach (var skin in skeletonData.Skins)
                {
                    if (skin == null || string.IsNullOrEmpty(skin.Name))
                        continue;

                    if (skin.Name.Contains("default"))
                        continue;

                    spineModelData.SkinList.Add(skin.Name);
                }

                spineModelData.AnimationList.Clear();
                spineModelData.AnimationList_Dic.Clear();
                foreach (var animation in skeletonData.Animations)
                {
                    if (animation == null)
                        continue;

                    var data = new SpineModelAnimationData
                    {
                        stateName = animation.Name,
                        animation = animation
                    };

                    spineModelData.AnimationList.Add(data);
                    spineModelData.AnimationList_Dic[data.stateName] = data;
                }

                if (spineModelData.DefaultSlotSkins != null)
                {
                    foreach (var slotSkin in spineModelData.DefaultSlotSkins)
                    {
                        if (slotSkin == null || slotSkin.SkinName == null || slotSkin.SkinName.Count == 0)
                            continue;

#if UNITY_EDITOR
                        foreach (string skinName in slotSkin.SkinName)
                        {
                            if (string.IsNullOrEmpty(skinName))
                                continue;

                            if (!spineModelData.SkinList.Contains(skinName))
                            {
                                Debug.LogWarning(
                                    $"[SpineModelAsset] {spineModelData.Name} 의 DefaultSlotSkin [{slotSkin.Slot}] '{skinName}' 이(가) 실제 스켈레톤 스킨 목록에 없습니다.",
                                    this);
                            }
                        }
#endif
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}
