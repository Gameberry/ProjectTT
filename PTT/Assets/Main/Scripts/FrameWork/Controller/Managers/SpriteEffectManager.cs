using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SpriteEffectManager : MonoSingleton<SpriteEffectManager>
    {
        private SpriteEffectPoolElement effectPoolElement;

        private AnimationTableAsset m_animationTableAsset = null;

        private Queue<SpriteEffectPoolElement> spriteEffectPoolElements = new Queue<SpriteEffectPoolElement>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_animationTableAsset = StaticResource.Instance.GetAnimationTableAsset();
            effectPoolElement = StaticResource.Instance.GetSpriteEffectPoolElement();
        }
        //------------------------------------------------------------------------------------
        public void PlaySpriteEffect(string effectName, Vector3 effectpos, Enum_ARR_LookDirection lookdirection = Enum_ARR_LookDirection.Right)
        {

        }
        //------------------------------------------------------------------------------------
        public void PlaySpriteEffect(string anikey, int varnum, string SpriteAnimationName, Vector3 effectpos, Enum_ARR_LookDirection lookdirection = Enum_ARR_LookDirection.Right)
        {
            AnimationSpriteLibraryAsset animationSpriteLibraryAsset = Managers.AnimationSpriteManager.Instance.GetAnimationSpriteLibraryAsset(anikey, varnum);
            SpriteAnimation spriteAnimation = m_animationTableAsset.GetAniData(anikey, SpriteAnimationName);

            if (spriteAnimation == null)
                return;

            PlaySpriteEffect(spriteAnimation, animationSpriteLibraryAsset, effectpos, lookdirection);
        }
        //------------------------------------------------------------------------------------
        public void PlaySpriteEffect(SpriteAnimation spriteAnimation, AnimationSpriteLibraryAsset animationSpriteLibraryAsset, Vector3 effectpos, Enum_ARR_LookDirection lookdirection = Enum_ARR_LookDirection.Right)
        {
            if (spriteEffectPoolElements.Count <= 0)
                AddSpriteEffectPool();

            SpriteEffectPoolElement spriteEffectPoolElement = spriteEffectPoolElements.Dequeue();

            spriteEffectPoolElement.PlaySpriteEffect(spriteAnimation, animationSpriteLibraryAsset, effectpos, lookdirection);
        }
        //------------------------------------------------------------------------------------
        public void PoolSpriteEffect(SpriteEffectPoolElement spriteEffectPoolElement)
        {
            spriteEffectPoolElement.gameObject.SetActive(false);
            spriteEffectPoolElements.Enqueue(spriteEffectPoolElement);
        }
        //------------------------------------------------------------------------------------
        private void AddSpriteEffectPool()
        {
            for (int i = 0; i < 5; ++i)
            {
                GameObject clone = Instantiate(effectPoolElement.gameObject, transform);
                SpriteEffectPoolElement spriteEffectPoolElement = clone.GetComponent<SpriteEffectPoolElement>();
                spriteEffectPoolElement.Init();

                clone.SetActive(false);
                spriteEffectPoolElements.Enqueue(spriteEffectPoolElement);
            }
        }
        //------------------------------------------------------------------------------------
    }
}