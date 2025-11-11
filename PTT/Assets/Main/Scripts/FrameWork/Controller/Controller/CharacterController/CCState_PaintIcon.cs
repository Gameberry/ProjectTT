using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CCState_PaintIcon : MonoBehaviour
    {
        private SpriteAnimation m_spriteAnimation = null;

        private Coroutine m_cCStateCoroutine = null;

        [SerializeField]
        private SpriteRenderer m_cCSpriteIcon = null;

        private WaitForSeconds m_anidelay = new WaitForSeconds(0.1f);

        private bool m_visible = false;

        //------------------------------------------------------------------------------------
        public void SetAniData(SpriteAnimation spriteAnimation)
        {
            m_spriteAnimation = spriteAnimation;
        }
        //------------------------------------------------------------------------------------
        public void PlayCCIconAni()
        {
            if (m_cCStateCoroutine != null)
                StopCoroutine(m_cCStateCoroutine);

            m_cCStateCoroutine = StartCoroutine(PlayCCAnimation());
        }
        //------------------------------------------------------------------------------------
        private void OnEnable()
        {
            m_visible = true;
        }
        //------------------------------------------------------------------------------------
        private void OnDisable()
        {
            m_visible = false;
        }
        //------------------------------------------------------------------------------------
        private IEnumerator PlayCCAnimation()
        {
            int startidx = 0;

            while (m_visible)
            {
                if (m_spriteAnimation.ActionDatas.Count <= startidx)
                    startidx = 0;

                List<AnimationFrameData> framedata = m_spriteAnimation.ActionDatas[startidx].FrameDatas;
                AnimationFrameData animationFrameData = framedata.Find(x => x.PartID == AnimationPart.Body);
                if (animationFrameData != null)
                    m_cCSpriteIcon.sprite = CCPaintManager.Instance.GetCCSprite(animationFrameData.SpriteName);

                startidx++;
                yield return m_anidelay;
            }
        }
        //------------------------------------------------------------------------------------
    }
}