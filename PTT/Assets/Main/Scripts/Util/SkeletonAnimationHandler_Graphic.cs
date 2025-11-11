using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    public class SkeletonAnimationHandler_Graphic : MonoBehaviour
    {
        private SkeletonGraphic _skeletonAnimation;

        [SerializeField] private List<StateNameToAnimationReference> _statesAndAnimation = new List<StateNameToAnimationReference>();
        [SerializeField] private List<AnimationTransition> _transitions = new List<AnimationTransition>();
        [SerializeField] private List<AnimationEventParticle> _eventparticle = new List<AnimationEventParticle>();

        [System.Serializable]
        public class StateNameToAnimationReference
        {
            //public string stateName;
            //public @string animation;
            //public Spine.Animation animation;
            [SpineAnimation] public string stateName;
            public Spine.Animation animation;
        }

        [System.Serializable]
        public class AnimationTransition
        {
            [SpineAnimation] public string fromeName;
            public Spine.Animation from;
            [SpineAnimation] public string toName;
            public Spine.Animation to;
        }

        [System.Serializable]
        public class AnimationEventParticle
        {
            [SpineEvent] public string eventName;
            public List<ParticleSystem> particleSystem;
        }

        public System.Action<string> AnimationEvent;

        private void Awake()
        {
            _skeletonAnimation = GetComponent<SkeletonGraphic>();

            foreach (var entry in _statesAndAnimation)
            {
                SkeletonData skeletonData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);
                entry.animation = skeletonData != null ? skeletonData.FindAnimation(entry.stateName) : null;
            }

            foreach (var entry in _transitions)
            {
                SkeletonData skeletonData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);

                entry.from = skeletonData != null ? skeletonData.FindAnimation(entry.fromeName) : null;
                entry.to = skeletonData != null ? skeletonData.FindAnimation(entry.toName) : null;
            }
        }


        private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
        {
            string eventname = e.ToString();

            foreach (var eventparticle in _eventparticle)
            {
                if (eventparticle.eventName == eventname)
                {
                    if (eventparticle.particleSystem == null)
                        continue;

                    for (int i = 0; i < eventparticle.particleSystem.Count; ++i)
                    {
                        eventparticle.particleSystem[i].Stop();
                        eventparticle.particleSystem[i].Play();
                    }
                }
            }

            if (AnimationEvent != null)
                AnimationEvent(e.ToString());
        }


        [SpineEvent] public string eventname;
        [SpineAnimation] public string testAniName;
        [ContextMenu("TestPlayAnimation")]
        public void TestPlayAnimation()
        {
            PlayAnimation(testAniName);
        }

        public void PlayAnimation(string stateShortName)
        {
            PlayAnimationForState(stateShortName, 0);
        }

        public void PlayAnimation_Once(string stateShortName, bool loop)
        {
            var foundAnimation = GetAnimationForState(StringToHash(stateShortName));
            if (foundAnimation == null)
                return;

            _skeletonAnimation.AnimationState.SetAnimation(0, foundAnimation, loop);
        }

        /// <summary>
        /// 2D 뒤집기 메서드
        /// </summary>
        /// <param name="horizontal"></param>
        
        public void PlayAnimationForState(string stateShortName, int layerIndex)
        {
            PlayAnimationForState(StringToHash(stateShortName), layerIndex);
        }

        /// <summary>
        /// PlayAnimationForState Overloading 해당 애니메이션을 실행
        /// </summary>
        /// <param name="stateShortName">실행하고자 하는 애니메이션 이름</param>
        /// <param name="layerIndex">트랙/레이어 번호</param>
        public void PlayAnimationForState(int stateShortName, int layerIndex)
        {
            var foundAnimation = GetAnimationForState(stateShortName);
            if (foundAnimation == null)
                return;

            PlayNewAnimation(foundAnimation, layerIndex);
        }

        public Spine.Animation GetAnimationForState(string stateShortName)
        {
            return GetAnimationForState(StringToHash(stateShortName));
        }

        /// <summary>
        /// GetAnimationForState Overloading 해당 애니메이션을 반환(없다면 null)
        /// </summary>
        /// <param name="stateShortName">찾고자 하는 애니메이션 이름(정수로 들어옴)</param>
        /// <returns>해당 애니메이션</returns>
        public Spine.Animation GetAnimationForState(int stateShortName)
        {
            var foundState = _statesAndAnimation.Find(entry => StringToHash(entry.stateName) == stateShortName);
            return ((foundState == null) ? null : foundState.animation);
        }

        /// <summary>
        /// 애니메이션 재생 메서드
        /// 현재 진행중인 애니메이션이 없다면 || 전환 애니메이션이 없다면 바로 애니메이션 전환
        /// 있다면 전환 애니메이션 우선 재생 후 재생
        /// </summary>
        /// <param name="target"></param>
        /// <param name="layerIndex"></param>
        public void PlayNewAnimation(Spine.Animation target, int layerIndex)
        {
            Spine.Animation transition = null;
            Spine.Animation current = target;

            if (current != null)
                transition = TryGetTransition(current);

            if (transition != null)
            {
                _skeletonAnimation.AnimationState.SetAnimation(layerIndex, current, false);
                _skeletonAnimation.AnimationState.AddAnimation(layerIndex, transition, true, 0f);
            }
            else
            {
                _skeletonAnimation.AnimationState.SetAnimation(layerIndex, target, true);
            }
        }

        /// <summary>
        /// 현재 애니메이션에서 다음 애니메이션으로 전환될 때 전환 애니메이션이 있는지 판단
        /// </summary>
        /// <param name="from">현재 애니메이션</param>
        /// <param name="to">다음 애니메이션</param>
        /// <returns>없다면 null 있다면 전환애니메이션(ex)ldel-to-jump)</returns>
        private Spine.Animation TryGetTransition(Spine.Animation from)
        {
            foreach (var transition in _transitions)
            {
                if (transition.from == from && transition.to != null)
                {
                    return transition.to;
                }
            }

            return null;
        }

        /// <summary>
        /// 애니메이션 문자열을 해쉬값으로 반환
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private int StringToHash(string str)
        {
            return Animator.StringToHash(str);
        }
    }
}