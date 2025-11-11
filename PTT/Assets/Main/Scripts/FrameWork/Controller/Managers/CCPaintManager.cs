using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CCPaintManager : MonoSingleton<CCPaintManager>
    {
        private Dictionary<V2Enum_SkillEffectType, SpriteAnimation> m_ccAniData = new Dictionary<V2Enum_SkillEffectType, SpriteAnimation>();
        private Queue<CCState_PaintIcon> m_cCState_PaintIconPool = new Queue<CCState_PaintIcon>();

        private CCState_PaintIcon m_cCState_PaintIconObj = null;

        private AnimationSpriteLibraryAsset m_animationSpriteLibraryAsset = null;

        protected override void Init()
        {
            AnimationTableAsset animationTableAsset = StaticResource.Instance.GetAnimationTableAsset();
            if (animationTableAsset == null)
                return;

            for (int i = 0; i < (int)V2Enum_SkillEffectType.Max; ++i)
            {
                V2Enum_SkillEffectType cCType = (V2Enum_SkillEffectType)i;

                SpriteAnimation spriteAnimation = animationTableAsset.GetAniData("Status Effects", cCType.ToString());

                if (spriteAnimation == null)
                    continue;

                m_ccAniData.Add(cCType, spriteAnimation);
            }

            m_cCState_PaintIconObj = StaticResource.Instance.GetCCState_PaintIcon();

            m_animationSpriteLibraryAsset = StaticResource.Instance.GetCCState_AnimationSpriteLibraryAsset();
        }
        //------------------------------------------------------------------------------------
        public SpriteAnimation GetCCSpriteAnimation(V2Enum_SkillEffectType cCType)
        {
            if (m_ccAniData.ContainsKey(cCType) == false)
                return null;

            return m_ccAniData[cCType];
        }
        //------------------------------------------------------------------------------------
        public CCState_PaintIcon GetCCState_Painter()
        {
            if (m_cCState_PaintIconPool.Count <= 0)
            {
                if (m_cCState_PaintIconObj == null)
                    return null;

                GameObject clone = Instantiate(m_cCState_PaintIconObj.gameObject, transform);
                if (clone == null)
                    return null;

                CCState_PaintIcon cCState_PaintIcon = clone.GetComponent<CCState_PaintIcon>();
                m_cCState_PaintIconPool.Enqueue(cCState_PaintIcon);
            }

            if (m_cCState_PaintIconPool.Count > 0)
                return m_cCState_PaintIconPool.Dequeue();

            return null;
        }
        //------------------------------------------------------------------------------------
        public void PoolCCState_Painter(CCState_PaintIcon cCState_PaintIcon)
        {
            if (cCState_PaintIcon == null)
                return;

            cCState_PaintIcon.transform.SetParent(transform);
            cCState_PaintIcon.gameObject.SetActive(false);
            m_cCState_PaintIconPool.Enqueue(cCState_PaintIcon);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetCCSprite(string spriteName)
        {
            if (m_animationSpriteLibraryAsset == null)
                return null;

            return m_animationSpriteLibraryAsset.GetSprite(spriteName);
        }
        //------------------------------------------------------------------------------------
    }
}