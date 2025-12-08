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

    /// <summary>
    /// 장비/코디용 슬롯 타입
    /// </summary>
    public enum SkinSlotType
    {
        Body,
        Hair,
        Weapon,
        Face,
        Back,
    }

    /// <summary>
    /// 슬롯별 기본 스킨 정보 (캐릭터가 처음 입고 나오는 코디)
    /// </summary>
    [System.Serializable]
    public class SpineDefaultSlotSkin
    {
        public SkinSlotType Slot;
        [SpineSkin(dataField = "SkeletonData")]
        public string SkinName;
    }

    [System.Serializable]
    public class SpineModelData
    {
        public int ResourceIndex;

        public SkeletonDataAsset SkeletonData;

        public string Name;



        /// <summary>
        /// 이 모델에서 사용 가능한 모든 스킨 이름 목록
        /// (OnValidate에서 자동 채움)
        /// </summary>
        [SpineSkin(dataField = "SkeletonData")]
        public List<string> SkinList = new List<string>();

        public List<SpineModelAnimationData> AnimationList = new List<SpineModelAnimationData>();
        public Dictionary<string, SpineModelAnimationData> AnimationList_Dic = new Dictionary<string, SpineModelAnimationData>();

        public List<SpineModelStateAnimationNameData> AnimationState = new List<SpineModelStateAnimationNameData>();

        /// <summary>
        /// 캐릭터가 기본으로 장착하고 있을 슬롯별 스킨
        /// 예) Body=skin_body_01, Hair=skin_hair_01 ...
        /// 인스펙터에서 모델마다 설정해두면 됨.
        /// </summary>
        public List<SpineDefaultSlotSkin> DefaultSlotSkins =
            new List<SpineDefaultSlotSkin>();
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

                // 이름
                spineModelData.Name = spineModelData.SkeletonData.name.ToLower();

                // 사용 가능한 스킨 목록 갱신
                spineModelData.SkinList.Clear();
                foreach (var skin in skeletonData.Skins)
                {
                    if (skin == null || string.IsNullOrEmpty(skin.Name))
                        continue;

                    // default 류는 리스트에서 제외 (원하면 포함해도 됨)
                    if (skin.Name.Contains("default"))
                        continue;

                    spineModelData.SkinList.Add(skin.Name);
                }

                // 애니메이션 목록 갱신
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

                // DefaultSlotSkins은 인스펙터에서 수동 세팅.
                // 여기서는 유효성만 간단히 체크.
                if (spineModelData.DefaultSlotSkins != null)
                {
                    foreach (var slotSkin in spineModelData.DefaultSlotSkins)
                    {
                        if (slotSkin == null || string.IsNullOrEmpty(slotSkin.SkinName))
                            continue;

                        // 존재하지 않는 스킨이면 경고만 (필요하면 자동 수정 로직 넣어도 됨)
#if UNITY_EDITOR
                        if (!spineModelData.SkinList.Contains(slotSkin.SkinName))
                        {
                            Debug.LogWarning(
                                $"[SpineModelAsset] {spineModelData.Name} 의 DefaultSlotSkin '{slotSkin.SkinName}' 이(가) 실제 스켈레톤 스킨 목록에 없습니다.",
                                this);
                        }
#endif
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}