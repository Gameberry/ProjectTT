using UnityEngine;
using System.Collections.Generic;

namespace GameBerry
{
    public enum AnimationPart : byte
    {
        Body = 0,
        Hand,
        Weapon,

        Effect1,
        Effect2,
        Effect3,

        LinearDodge1,
        LinearDodge2,
        LinearDodge3,

        Max,
    }

    public enum AnimationAction : byte
    {
        None = 0,
        AniStart,
        AniStartAndAction,
        AniAction,
        AniEnd,
        AniActionReady,
        AniActionEnd,

        AniCustomAction1,
        AniCustomAction2,
        AniCustomAction3,
        AniCustomAction4,
        AniCustomAction5,
        AniCustomAction6,
    }

    [System.Serializable]
    public class AnimationFrameData
    {
        public AnimationPart PartID;
        public int OrderInLayer;
        public Vector3 LocalPosition, LocalRotation;
        public Vector3 LocalScale = Vector3.one;

        public Sprite Sprite;
        public string SpriteName;
        public string SpriteBundleTag;

        public ParticleSystem Particle;
        public string ParticleName;
        public string ParticleBundleTag;

        public bool ParticleWorldView;
    }

    [System.Serializable]
    public class AnimationActionData
    {
        public AnimationAction ActionID;
        public List<AnimationFrameData> FrameDatas = new List<AnimationFrameData>();
        public Vector3 CharWorldPosition, CharWorldRotation;
    }

    [System.Serializable]
    public class SpriteAnimation
    {
        public CharacterState AnimationGroup;
        public string AnimationID;
        public float Duration;
        public bool Loop = false;

        public Vector3 ReTouchWorldPos;

        [ArrayElementTitle("ActionID")]
        public List<AnimationActionData> ActionDatas = new List<AnimationActionData>();
    }

    [System.Serializable]
    public class SpriteAniGroupData
    {
        public string AniResourceKey = string.Empty;

        [ArrayElementTitle("AnimationID")]
        public List<SpriteAnimation> SpriteAnimation_List = new List<SpriteAnimation>();

        public Dictionary<CharacterState, List<string>> SpriteAnimationID_Dic = new Dictionary<CharacterState, List<string>>();
        public Dictionary<string, SpriteAnimation> SpriteAnimation_Dic = new Dictionary<string, SpriteAnimation>();
    }

    [CreateAssetMenu(fileName = "AnimationTable", menuName = "Table/AnimationTable", order = 1)]
    public class AnimationTableAsset : ScriptableObject
    {
        [Header("-----------------CheatKey-----------------")]
        public string CheatAniResourceKey = "";
        public AnimationPart CheatAniPart = AnimationPart.Body;
        public int CheatOrderInLayer = 0;
        [Header("------------------------------------------")]


        public Dictionary<CharacterState, List<string>> SpriteAnimationID_Dic = new Dictionary<CharacterState, List<string>>();
        public Dictionary<string, SpriteAnimation> SpriteAnimation_Dic = new Dictionary<string, SpriteAnimation>();

        [Header("------------------------------------------")]
        [ArrayElementTitle("AniResourceKey")]
        public List<SpriteAniGroupData> SpriteAniGroupData_List = new List<SpriteAniGroupData>();

        private Dictionary<string, SpriteAniGroupData> m_spriteAniGroupData_Dic = new Dictionary<string, SpriteAniGroupData>();



        //------------------------------------------------------------------------------------
        public void OnEnable()
        {
            SpriteAnimationID_Dic.Clear();
            SpriteAnimation_Dic.Clear();

            m_spriteAniGroupData_Dic.Clear();

            for (int monIdx = 0; monIdx < SpriteAniGroupData_List.Count; ++monIdx)
            {
                SpriteAniGroupData monAniData = SpriteAniGroupData_List[monIdx];

                monAniData.SpriteAnimationID_Dic.Clear();
                monAniData.SpriteAnimation_Dic.Clear();

                for (int i = 0; i < monAniData.SpriteAnimation_List.Count; ++i)
                {
                    CharacterState group = monAniData.SpriteAnimation_List[i].AnimationGroup;

                    if (monAniData.SpriteAnimationID_Dic.ContainsKey(group) == false)
                        monAniData.SpriteAnimationID_Dic.Add(group, new List<string>());

                    if (monAniData.SpriteAnimationID_Dic[group].Contains(monAniData.SpriteAnimation_List[i].AnimationID) == true)
                        continue;

                    monAniData.SpriteAnimationID_Dic[group].Add(monAniData.SpriteAnimation_List[i].AnimationID);
                    monAniData.SpriteAnimation_Dic.Add(monAniData.SpriteAnimation_List[i].AnimationID, monAniData.SpriteAnimation_List[i]);
                }

                //monAniData.SpriteLibrary.MonsterAniSprites_Dic.Clear();

                //for (int i = 0; i < monAniData.SpriteLibrary.MonsterAniSprites.Count; ++i)
                //{
                //    monAniData.SpriteLibrary.MonsterAniSprites_Dic.Add(monAniData.SpriteLibrary.MonsterAniSprites[i].name, monAniData.SpriteLibrary.MonsterAniSprites[i]);
                //}

                m_spriteAniGroupData_Dic.Add(monAniData.AniResourceKey, monAniData);
            }
        }
        //------------------------------------------------------------------------------------
        public SpriteAnimation GetRandomStateAniData(string AniResourceKey, CharacterState state)
        {
            SpriteAnimation data = null;

            if (m_spriteAniGroupData_Dic.ContainsKey(AniResourceKey) == true)
            {
                SpriteAniGroupData anigroup = m_spriteAniGroupData_Dic[AniResourceKey];

                if (anigroup.SpriteAnimationID_Dic.ContainsKey(state) == true)
                {
                    List<string> dataidlist = null;
                    anigroup.SpriteAnimationID_Dic.TryGetValue(state, out dataidlist);
                    if (dataidlist.Count > 0)
                    {
                        //string aniidkey = dataidlist[Random.Range(0, dataidlist.Count)];
                        //if (anigroup.SpriteAnimation_Dic.ContainsKey(aniidkey) == true)
                        //{
                        //    if (anigroup.SpriteAnimation_Dic.TryGetValue(aniidkey, out data) == true)
                        //        return data;
                        //}

                        data = GetAniData(AniResourceKey, dataidlist[Random.Range(0, dataidlist.Count)]);
                    }
                }
            }

            return data;
        }
        //------------------------------------------------------------------------------------
        public SpriteAnimation GetAniData(string AniResourceKey, string aniid)
        {
            SpriteAnimation data = null;

            if (m_spriteAniGroupData_Dic.ContainsKey(AniResourceKey) == true)
            {
                SpriteAniGroupData anigroup = m_spriteAniGroupData_Dic[AniResourceKey];

                if (anigroup.SpriteAnimation_Dic.ContainsKey(aniid) == true)
                {
                    if (anigroup.SpriteAnimation_Dic.TryGetValue(aniid, out data) == true)
                        return data;
                }
            }

            return data;
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("SetSpriteName")]
        public void SetSpriteName()
        {
            for (int i = 0; i < SpriteAniGroupData_List.Count; ++i)
            {
                ApplySpriteName(SpriteAniGroupData_List[i].SpriteAnimation_List);
            }
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("SetParticleName")]
        public void SetParticleName()
        {
            for (int i = 0; i < SpriteAniGroupData_List.Count; ++i)
            {
                ApplyParticleName(SpriteAniGroupData_List[i].SpriteAnimation_List);
            }
        }
        //------------------------------------------------------------------------------------

        [ContextMenu("CheatKey")]
        public void CheatKey()
        {
            for (int i = 0; i < SpriteAniGroupData_List.Count; ++i)
            {
                if (SpriteAniGroupData_List[i].AniResourceKey == CheatAniResourceKey)
                {
                    for (int j = 0; j < SpriteAniGroupData_List[i].SpriteAnimation_List.Count; ++j)
                    {
                        SpriteAnimation spri = SpriteAniGroupData_List[i].SpriteAnimation_List[j];
                        for (int k = 0; k < spri.ActionDatas.Count; ++k)
                        {
                            AnimationActionData aniaction = spri.ActionDatas[k];
                            for (int x = 0; x < aniaction.FrameDatas.Count; ++x)
                            {
                                AnimationFrameData aniframe = aniaction.FrameDatas[x];
                                if (aniframe.PartID == CheatAniPart)
                                    aniframe.OrderInLayer = CheatOrderInLayer;
                            }
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ApplySpriteName(List<SpriteAnimation> TargetSpriteAnimation_List)
        {
            for (int i = 0; i < TargetSpriteAnimation_List.Count; ++i)
            {
                for (int j = 0; j < TargetSpriteAnimation_List[i].ActionDatas.Count; ++j)
                {
                    for (int k = 0; k < TargetSpriteAnimation_List[i].ActionDatas[j].FrameDatas.Count; ++k)
                    {
                        AnimationFrameData data = TargetSpriteAnimation_List[i].ActionDatas[j].FrameDatas[k];
                        Debug.Log(string.Format("AnimationID : {0}  ActionID : {1}  SaveThis : {2}", TargetSpriteAnimation_List[i].AnimationID, TargetSpriteAnimation_List[i].ActionDatas[j].ActionID, data.Sprite.name));
                        string spritename = data.Sprite.name;
                        string applyspritename = Util.GetSpriteVariationName(spritename);

                        data.SpriteName = applyspritename;

                        data.SpriteBundleTag = Util.GetResourceBundleTag(data.Sprite);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ApplyParticleName(List<SpriteAnimation> TargetSpriteAnimation_List)
        {
            for (int i = 0; i < TargetSpriteAnimation_List.Count; ++i)
            {
                for (int j = 0; j < TargetSpriteAnimation_List[i].ActionDatas.Count; ++j)
                {
                    for (int k = 0; k < TargetSpriteAnimation_List[i].ActionDatas[j].FrameDatas.Count; ++k)
                    {
                        AnimationFrameData data = TargetSpriteAnimation_List[i].ActionDatas[j].FrameDatas[k];
                        if (data.Particle == null)
                        {
                            data.ParticleName = string.Empty;
                            data.ParticleBundleTag = string.Empty;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(data.ParticleName)
                                || data.ParticleName != data.Particle.name)
                                Debug.Log(string.Format("NewName : {0}", data.Particle.name));

                            data.ParticleName = data.Particle.name;

                            if (string.IsNullOrEmpty(data.ParticleBundleTag)
                                || data.ParticleBundleTag != Util.GetResourceBundleTag(data.Particle))
                                Debug.Log(string.Format("ParticleBundleTag : {0}", Util.GetResourceBundleTag(data.Particle)));

                            data.ParticleBundleTag = Util.GetResourceBundleTag(data.Particle);
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}
