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
    public class SpineModelData
    {
        public int ResourceIndex;

        public SkeletonDataAsset SkeletonData;

        public string Name;

        public List<string> SkinList = new List<string>();
        public List<string> DefaultAttackSkin = new List<string>();

        public List<SpineModelAnimationData> AnimationList = new List<SpineModelAnimationData>();
        public Dictionary<string, SpineModelAnimationData> AnimationList_Dic = new Dictionary<string, SpineModelAnimationData>();
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

                spineModelData.SkinList.Clear();

                spineModelData.Name = spineModelData.SkeletonData.name.ToLower();

                foreach (var pair in skeletonData.Skins)
                {
                    if (pair.Name.Contains("default"))
                        continue;
                    spineModelData.SkinList.Add(pair.Name);
                }

                spineModelData.AnimationList.Clear();
                spineModelData.AnimationList_Dic.Clear();
                foreach (var pair in skeletonData.Animations)
                {
                    SpineModelAnimationData spineModelAnimationData = new SpineModelAnimationData();
                    spineModelAnimationData.stateName = pair.Name;
                    spineModelAnimationData.animation = pair;

                    spineModelData.AnimationList.Add(spineModelAnimationData);
                    spineModelData.AnimationList_Dic.Add(spineModelAnimationData.stateName, spineModelAnimationData);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}