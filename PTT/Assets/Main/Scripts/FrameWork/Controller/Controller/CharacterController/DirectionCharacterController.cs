using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBerry
{
    [System.Serializable]
    public class SpineDirectionCharacter
    {
        public int Index;
        public SkeletonAnimationHandler SkeletonAnimationHandler;
    }

    public class DirectionCharacterController : CharacterControllerBase
    {
        [SerializeField]
        private Transform m_bubbleTrans;

        private Vector3 m_originSpriteRootPos = Vector3.zero;

        private bool m_isSpine = false;

        [SerializeField]
        private List<SpineDirectionCharacter> m_spineAnimationHandler = new List<SpineDirectionCharacter>();

        private SkeletonAnimationHandler m_selectAniHandler;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            if (_charAnicontroller != null)
            {
                _charAnicontroller.Init(transform);
                //m_charAnicontroller.ConnectAniActionState(AniActionCallBack);

                //if (m_characterSpriteRoot != null)
                //{
                //    m_originSpriteRootPos = m_characterSpriteRoot.localPosition;
                //}
            }

            m_isSpine = false;
        }
        //------------------------------------------------------------------------------------
        protected void SetFootPos()
        {
            //if (m_bodyRenderer != null)
            //{
            //    Vector3 pos = Vector3.zero;
            //    pos.y += m_bodyRenderer.bounds.size.y * 0.5f;
            //    m_characterSpriteRoot.localPosition = pos;
            //}
        }
        //------------------------------------------------------------------------------------
        public void SetMyActorType(IFFType actorType)
        {
            _iFFType = actorType;
        }
        //------------------------------------------------------------------------------------
        public void SetGroupAndVariationNumber(string groupIndex, int variationNumber)
        {
            //if (m_characterSpriteRoot != null)
            //    m_characterSpriteRoot.gameObject.SetActive(true);

            _charAnicontroller.SetAnimationSpriteLibrary();

            m_isSpine = false;

            if (m_selectAniHandler != null)
            {
                m_selectAniHandler.gameObject.SetActive(false);
                m_selectAniHandler = null;
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSpine(int id)
        {
            m_isSpine = true;

            if (m_selectAniHandler != null)
            {
                m_selectAniHandler.gameObject.SetActive(false);
                m_selectAniHandler = null;
            }

            //if (m_characterSpriteRoot != null)
            //    m_characterSpriteRoot.gameObject.SetActive(false);

            SpineDirectionCharacter spineDirectionCharacter = m_spineAnimationHandler.Find(x => x.Index == id);
            if (spineDirectionCharacter != null)
                m_selectAniHandler = spineDirectionCharacter.SkeletonAnimationHandler;
        }
        //------------------------------------------------------------------------------------
        public void PlayDirection()
        {
            if (m_isSpine == false)
            {
                PlayAnimation(CharacterState.Idle);
            }
            else
            {
                if (m_selectAniHandler != null)
                {
                    m_selectAniHandler.gameObject.SetActive(true);
                    m_selectAniHandler.PlayAnimation("Idle");
                }
            }

            SetFootPos();
        }
        //------------------------------------------------------------------------------------
        public void PlayActorEmotion(V2Enum_ActorEmotion state)
        {
            if (state == V2Enum_ActorEmotion.None)
                return;

            if (m_isSpine == true)
                return;

            switch (state)
            {
                case V2Enum_ActorEmotion.StandStill:
                    {
                        PlayAnimation(CharacterState.Idle);
                        break;
                    }
                case V2Enum_ActorEmotion.Exhausted:
                    {
                        if (_charAnicontroller != null)
                        {
                            _charAnicontroller.PlayAnimation(CharacterState.Dead, "Dead");
                        }
                        break;
                    }
                case V2Enum_ActorEmotion.FallDown:
                    {
                        if (_charAnicontroller != null)
                        {
                            _charAnicontroller.PlayAnimation(CharacterState.None, "Faint");
                        }
                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayAnimation(CharacterState state)
        {
            if (_charAnicontroller != null)
            {
                _charAnicontroller.PlayAnimation(state);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetVisibleBubble(bool visible)
        {
            if (m_bubbleTrans != null)
                m_bubbleTrans.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
    }
}
