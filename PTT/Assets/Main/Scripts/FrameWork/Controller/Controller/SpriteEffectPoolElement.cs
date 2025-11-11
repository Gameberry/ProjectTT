using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SpriteEffectPoolElement : MonoBehaviour
    {
        [SerializeField]
        private List<SpriteAniPart> m_charAniPart = new List<SpriteAniPart>();
        private Dictionary<AnimationPart, SpriteAniPart> m_charAniPart_Dic = new Dictionary<AnimationPart, SpriteAniPart>();
        private LinkedList<SpriteAniPart> m_changedAniPart_List = new LinkedList<SpriteAniPart>();

        private SpriteAnimation m_currentSpriteAnimation;

        private AnimationSpriteLibraryAsset m_animationSpriteLibraryAsset = null;

        private int m_addSortingRenderer;


        private int CurrentAnimationIdx = 0;

        private float EndTime = 0.0f;

        private float AnimationFrameTime = 0.1f;

        private float CheckLastChangeFrameTime = 0.0f;

        private Coroutine m_aniPlayerCoroutine = null;

        private List<ParticlePoolElement> m_playedPool = new List<ParticlePoolElement>();

        //------------------------------------------------------------------------------------
        public void Init()
        {
            for (int i = 0; i < m_charAniPart.Count; ++i)
            {
                m_charAniPart_Dic.Add(m_charAniPart[i].PartID, m_charAniPart[i]);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnDisable()
        {
            while (m_playedPool.Count > 0)
            {
                ParticlePoolElement particlePoolElement = m_playedPool[0];
                particlePoolElement.StopParticle();
            }
        }
        //------------------------------------------------------------------------------------
        private void EndParticle(ParticlePoolElement particlePoolElement)
        {
            m_playedPool.Remove(particlePoolElement);
            particlePoolElement.ParticleStop -= EndParticle;
        }
        //------------------------------------------------------------------------------------
        public void PlaySpriteEffect(SpriteAnimation spriteAnimation, AnimationSpriteLibraryAsset animationSpriteLibraryAsset, Vector3 effectpos, Enum_ARR_LookDirection lookdirection = Enum_ARR_LookDirection.Right)
        {
            gameObject.SetActive(true);

            m_currentSpriteAnimation = spriteAnimation;
            m_animationSpriteLibraryAsset = animationSpriteLibraryAsset;

            transform.position = effectpos;

            AnimationFrameTime = m_currentSpriteAnimation.Duration / (float)m_currentSpriteAnimation.ActionDatas.Count;

            SetAnimationData(m_currentSpriteAnimation, 0);

            CurrentAnimationIdx = 0;
            CheckLastChangeFrameTime = Time.time;

            EndTime = Time.time + m_currentSpriteAnimation.Duration;

            if (lookdirection == Enum_ARR_LookDirection.Right)
            {
                Vector3 rotate = transform.eulerAngles;
                rotate.y = 0.0f;
                transform.eulerAngles = rotate;

                Vector3 pos = transform.position;
                pos.x += m_currentSpriteAnimation.ReTouchWorldPos.x;
                pos.y += m_currentSpriteAnimation.ReTouchWorldPos.y;
                pos.z += m_currentSpriteAnimation.ReTouchWorldPos.z;
                transform.position = pos;
            }
            else if (lookdirection == Enum_ARR_LookDirection.Left)
            {
                Vector3 rotate = transform.eulerAngles;
                rotate.y = 180.0f;
                transform.eulerAngles = rotate;

                Vector3 pos = transform.position;
                pos.x -= m_currentSpriteAnimation.ReTouchWorldPos.x;
                pos.y += m_currentSpriteAnimation.ReTouchWorldPos.y;
                pos.z += m_currentSpriteAnimation.ReTouchWorldPos.z;
                transform.position = pos;
            }

            if (m_aniPlayerCoroutine != null)
                StopCoroutine(m_aniPlayerCoroutine);

            m_aniPlayerCoroutine = StartCoroutine(AniPlayer());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator AniPlayer()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(AnimationFrameTime);

            while (EndTime > Time.time)
            {
                yield return waitForSeconds;

                CurrentAnimationIdx++;

                if (m_currentSpriteAnimation.ActionDatas.Count > 0)
                {
                    if (m_currentSpriteAnimation.ActionDatas.Count <= CurrentAnimationIdx)
                        break;

                    SetAnimationData(m_currentSpriteAnimation, CurrentAnimationIdx);
                }
            }

            m_aniPlayerCoroutine = null;
            EndSpriteEffect();
        }
        //------------------------------------------------------------------------------------
        private void SetAnimationData(SpriteAnimation anidata, int idx)
        {
            List<AnimationFrameData> framedata = anidata.ActionDatas[idx].FrameDatas;

            LinkedListNode<SpriteAniPart> node = m_changedAniPart_List.First;
            LinkedListNode<SpriteAniPart> nextnode = null;

            while (node != null)
            {
                nextnode = node.Next;

                node.Value.Renderer.sprite = null;
                m_changedAniPart_List.Remove(node);

                node = nextnode;
            }

            for (int i = 0; i < framedata.Count; ++i)
            {
                SpriteAniPart data = null;
                if (m_charAniPart_Dic.ContainsKey(framedata[i].PartID) == true)
                {
                    m_charAniPart_Dic.TryGetValue(framedata[i].PartID, out data);

                    if (data == null)
                        return;
                }
                else
                    continue;

                if (m_animationSpriteLibraryAsset != null)
                    data.Renderer.sprite = m_animationSpriteLibraryAsset.GetSprite(framedata[i].SpriteName);
                //data.Renderer.sprite = Managers.AnimationSpriteManager.Instance.GetSprite(m_currentMonsterData.MonsterResource, m_currentMonsterData.MonsterResourceVariNum, framedata[i].SpriteName);

                data.Renderer.sortingOrder = framedata[i].OrderInLayer + m_addSortingRenderer;
                data.Renderer.transform.localPosition = framedata[i].LocalPosition;
                data.Renderer.transform.localEulerAngles = framedata[i].LocalRotation;
                data.Renderer.transform.localScale = framedata[i].LocalScale;

                if (string.IsNullOrEmpty(framedata[i].ParticleName) == false)
                {
                    ParticlePoolElement particlePoolElement = ParticleManager.Instance.GetParticle(framedata[i].ParticleBundleTag, framedata[i].ParticleName);

                    if (particlePoolElement != null)
                    {
                        if (framedata[i].ParticleWorldView == false)
                        {
                            particlePoolElement.transform.SetParent(data.Renderer.transform);
                            particlePoolElement.transform.localPosition = Vector3.zero;
                            particlePoolElement.transform.localEulerAngles = Vector3.zero;
                        }
                        else
                        {
                            particlePoolElement.transform.SetParent(null);
                            particlePoolElement.transform.position = data.Renderer.transform.position;
                            particlePoolElement.transform.localEulerAngles = Vector3.zero;
                        }

                        particlePoolElement.gameObject.SetActive(true);
                        particlePoolElement.PlayParticle();

                        particlePoolElement.ParticleStop += EndParticle;
                    }
                    
                    m_playedPool.Add(particlePoolElement);
                }

                m_changedAniPart_List.AddLast(data);
            }
        }
        //------------------------------------------------------------------------------------
        private void EndSpriteEffect()
        {
            SpriteEffectManager.Instance.PoolSpriteEffect(this);
        }
        //------------------------------------------------------------------------------------
    }
}