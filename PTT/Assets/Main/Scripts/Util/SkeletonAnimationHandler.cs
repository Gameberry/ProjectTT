using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    public class SkeletonAnimationHandler : MonoBehaviour
    {
        [SerializeField] public SkeletonAnimation _skeletonAnimation;

        [Header("Animation Data")]
        [SerializeField]
        private List<SpineModelAnimationData> _statesAndAnimation =
            new List<SpineModelAnimationData>();

        [SerializeField]
        private List<AnimationTransition> _transitions =
            new List<AnimationTransition>();

        [SerializeField]
        private List<AnimationEventParticle> _eventparticle =
            new List<AnimationEventParticle>();

        [SerializeField] private bool _awakeInit = false;

        public Dictionary<string, Spine.Animation> AnimationList_Dic =
            new Dictionary<string, Spine.Animation>();

        private readonly Dictionary<CharacterState, string> _myAnimation =
            new Dictionary<CharacterState, string>();

        private MeshRenderer _meshRenderer;

        // 현재 사용 중인 SpineModelData(스킨 슬롯 기본값 참조용)
        private SpineModelData _currentModelData;

        #region Nested types

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

        #endregion

        #region Events

        public System.Action<string, string> AnimationEvent;

        #endregion

        #region Unity

        private void Awake()
        {
            if (_skeletonAnimation == null)
                _skeletonAnimation = GetComponent<SkeletonAnimation>();

            if (_meshRenderer == null)
                _meshRenderer = GetComponent<MeshRenderer>();

            if (_skeletonAnimation != null && _skeletonAnimation.state != null)
            {
                _skeletonAnimation.state.Event += HandleEvent;
                _skeletonAnimation.state.Start += StartEvent;
                _skeletonAnimation.state.Complete += EndEvent;
            }

            if (_awakeInit && _skeletonAnimation != null
                && _skeletonAnimation.skeletonDataAsset != null)
            {
                var skeletonData =
                    _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);

                foreach (var entry in _statesAndAnimation)
                    entry.animation = skeletonData?.FindAnimation(entry.stateName);

                foreach (var entry in _transitions)
                {
                    entry.from = skeletonData?.FindAnimation(entry.fromeName);
                    entry.to = skeletonData?.FindAnimation(entry.toName);
                }
            }
        }

        #endregion

        #region Model & Animation Setup

        /// <summary>
        /// SpineModelData를 세팅하고, 애니메이션/스킨 슬롯 초기화
        /// </summary>
        public void SetSpineModel(SpineModelData spineModelData)
        {
            if (spineModelData == null)
                return;

            _currentModelData = spineModelData;

            // 스켈레톤 데이터 세팅
            _skeletonAnimation.skeletonDataAsset = spineModelData.SkeletonData;

            // 최초 기본 스킨(없으면 default)
            if (spineModelData.SkinList.Count > 0)
                _skeletonAnimation.initialSkinName = spineModelData.SkinList[0];
            else
                _skeletonAnimation.initialSkinName = "default";

            _skeletonAnimation.Initialize(true);

            // 애니메이션 목록 세팅
            var skeletonData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);
            _statesAndAnimation = spineModelData.AnimationList;

            _skeletonAnimation.state.Event += HandleEvent;
            _skeletonAnimation.state.Start += StartEvent;
            _skeletonAnimation.state.Complete += EndEvent;

            AnimationList_Dic.Clear();
            _myAnimation.Clear();

            foreach (var pair in spineModelData.AnimationState)
            {
                _myAnimation[pair.characterState] = pair.animationName;
            }

            foreach (var animation in skeletonData.Animations)
            {
                if (!AnimationList_Dic.ContainsKey(animation.Name))
                    AnimationList_Dic.Add(animation.Name, animation);
            }

            foreach (var entry in _transitions)
            {
                entry.from = skeletonData?.FindAnimation(entry.fromeName);
                entry.to = skeletonData?.FindAnimation(entry.toName);
            }
        }

        public void SetOrderInLayer(int orderinlayer)
        {
            if (_meshRenderer != null)
                _meshRenderer.sortingOrder = orderinlayer;
        }

        #endregion

        #region Spine Events

        private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
        {
            string eventname = e.ToString();

            foreach (var eventparticle in _eventparticle)
            {
                if (eventparticle.eventName != eventname)
                    continue;

                if (eventparticle.particleSystem == null)
                    continue;

                for (int i = 0; i < eventparticle.particleSystem.Count; ++i)
                {
                    eventparticle.particleSystem[i].Stop();
                    eventparticle.particleSystem[i].Play();
                }
            }

            AnimationEvent?.Invoke(trackEntry.ToString(), eventname);
        }

        private void StartEvent(TrackEntry trackEntry)
        {
            AnimationEvent?.Invoke(trackEntry.ToString(), "Start");
        }

        private void EndEvent(TrackEntry trackEntry)
        {
            AnimationEvent?.Invoke(trackEntry.ToString(), "End");
        }

        #endregion

        #region Public Animation API

        public void SetAnimationSpeed(float speed)
        {
            _skeletonAnimation.AnimationState.TimeScale = speed;
        }

        public void SetColor(Color color)
        {
            if (_skeletonAnimation != null)
                _skeletonAnimation.skeleton.SetColor(color);
        }

        [SpineAnimation] public string testAniName;
        [SpineSkin] public string testSpineSkin;

        [ContextMenu("TestPlayAnimation")]
        public void TestPlayAnimation()
        {
            PlayAnimation_Once(testAniName, false);
        }

        [ContextMenu("TesChangeSkin")]
        public void TesChangeSkin()
        {
            SetSkin(testSpineSkin);
        }

        /// <summary>
        /// Spine의 skin을 통째로 갈아끼우는 용도 (슬롯 시스템을 안 쓰고,
        /// 스켈레톤 내 하나의 skin으로만 보여주고 싶을 때)
        /// </summary>
        public void SetSkin(string skin)
        {
            if (_skeletonAnimation == null)
                return;

            var skeleton = _skeletonAnimation.skeleton;
            skeleton.SetSkin(skin);
            skeleton.SetSlotsToSetupPose();
            skeleton.SetBonesToSetupPose();
            _skeletonAnimation.AnimationState.Apply(skeleton);
        }

        private string GetAniClipName(CharacterState characterState)
        {
            if (!_myAnimation.ContainsKey(characterState))
                _myAnimation.Add(characterState, characterState.ToString());

            return _myAnimation[characterState];
        }

        public void PlayAnimation(string stateShortName)
        {
            PlayAnimationForState(stateShortName, 0);
        }

        public void PlayAnimation_Once(CharacterState characterState, bool loop)
        {
            PlayAnimation_Once(GetAniClipName(characterState), loop);
        }

        public void PlayAnimation_Once(string stateShortName, bool loop)
        {
            if (!AnimationList_Dic.ContainsKey(stateShortName))
                return;

            _skeletonAnimation.AnimationState.SetAnimation(0, stateShortName, loop);
        }

        /// <summary>
        /// 2D 좌우 뒤집기
        /// </summary>
        public void SetFlip(float horizontal)
        {
            if (horizontal == 0)
                return;

            _skeletonAnimation.skeleton.ScaleX = horizontal > 0 ? 1f : -1f;
        }

        public void PlayAnimationForState(string stateShortName, int layerIndex)
        {
            PlayAnimationForState(StringToHash(stateShortName), layerIndex);
        }

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

        public Spine.Animation GetAnimationForState(int stateShortName)
        {
            var foundState =
                _statesAndAnimation.Find(entry =>
                    StringToHash(entry.stateName) == stateShortName);
            return foundState == null ? null : foundState.animation;
        }

        public void PlayNewAnimation(Spine.Animation target, int layerIndex)
        {
            Spine.Animation transition = null;
            Spine.Animation current = target;

            if (current != null)
                transition = TryGetTransition(current);

            if (transition != null)
            {
                _skeletonAnimation.AnimationState.SetAnimation(layerIndex, current, false);
                _skeletonAnimation.AnimationState.AddAnimation(layerIndex, transition, true,
                    0f);
            }
            else
            {
                _skeletonAnimation.AnimationState.SetAnimation(layerIndex, target, true);
            }
        }

        private Spine.Animation TryGetTransition(Spine.Animation from)
        {
            foreach (var transition in _transitions)
            {
                if (transition.from == from && transition.to != null)
                    return transition.to;
            }

            return null;
        }

        private int StringToHash(string str)
        {
            return Animator.StringToHash(str);
        }

        #endregion

        #region Skin System (슬롯 + 추가 Attach)

        public void SetSkin(Skin skin)
        {
            if (_skeletonAnimation == null || _skeletonAnimation.skeleton == null)
                return;

            var skeleton = _skeletonAnimation.skeleton;
            var data = skeleton.Data;

            // 실제 스켈레톤에 반영
            skeleton.SetSkin(skin);
            skeleton.SetSlotsToSetupPose();
            skeleton.SetBonesToSetupPose();
            _skeletonAnimation.AnimationState.Apply(skeleton);
        }

        #endregion
    }
}
