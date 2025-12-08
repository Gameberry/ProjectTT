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

                if (skinList.Count > 0)
                    RebuildRuntimeSkin();
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
            //if (spineModelData.SkinList.Count > 0)
            //    _skeletonAnimation.initialSkinName = spineModelData.SkinList[0];
            //else
            //    _skeletonAnimation.initialSkinName = "default";

            //_skeletonAnimation.initialSkinName = "default";

            _skeletonAnimation.Initialize(true);

            // 애니메이션 목록 세팅
            var skeletonData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);
            _statesAndAnimation = spineModelData.AnimationList;

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

            // 슬롯/부가 스킨을 포함한 런타임 스킨 구성
            ResetSlotSkinsToDefault();
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

        /// <summary>
        /// 인스펙터에서 테스트 / 예전 구조와의 하위 호환을 위해 남겨둔 리스트.
        /// 이 리스트에 들어 있는 스킨은 항상 "추가로 덮어씌우는 애드온" 으로 처리된다.
        /// </summary>
        [SpineSkin]
        public List<string> skinList = new List<string>();

        /// <summary>
        /// 런타임에서 합성해서 쓰는 스킨
        /// </summary>
        private readonly Skin _runtimeSkin = new Skin("runtime-equips");

        /// <summary>
        /// 슬롯별 장착 스킨 (코디)
        /// </summary>
        private readonly Dictionary<SkinSlotType, string> _equippedSlotSkins =
            new Dictionary<SkinSlotType, string>();

        /// <summary>
        /// 외부에서 "추가 스킨" 등록 (예전 AddAttachSkin 하위 호환)
        /// </summary>
        public void AddAttachSkin(string attachSkin)
        {
            if (string.IsNullOrEmpty(attachSkin))
                return;

            if (!skinList.Contains(attachSkin))
                skinList.Add(attachSkin);

            RebuildRuntimeSkin();
        }

        public void ReleaseAttachSkin()
        {
            skinList.Clear();
            RebuildRuntimeSkin();
        }

        /// <summary>
        /// 예전 RefreshAttachSkin 대신 이제 전체 런타임 스킨을 다시 빌드
        /// </summary>
        [ContextMenu("RefreshAttachSkin")]
        public void RefreshAttachSkin()
        {
            RebuildRuntimeSkin();
        }

        /// <summary>
        /// 슬롯별 스킨 장착
        /// </summary>
        public void EquipSlotSkin(SkinSlotType slot, string skinName)
        {
            if (string.IsNullOrEmpty(skinName))
            {
                UnequipSlotSkin(slot);
                return;
            }

            if (_equippedSlotSkins.ContainsKey(slot) == false)
                _equippedSlotSkins.Add(slot, skinName);

            _equippedSlotSkins[slot] = skinName;
            RebuildRuntimeSkin();
        }

        /// <summary>
        /// 슬롯별 스킨 해제 (기본 스킨으로 돌아감)
        /// </summary>
        public void UnequipSlotSkin(SkinSlotType slot)
        {
            if (_equippedSlotSkins.Remove(slot))
                RebuildRuntimeSkin();
        }

        /// <summary>
        /// 모델에 설정해 둔 DefaultSlotSkins 값으로 슬롯 전체 초기화
        /// </summary>
        public void ResetSlotSkinsToDefault()
        {
            _equippedSlotSkins.Clear();

            if (_currentModelData != null &&
                _currentModelData.DefaultSlotSkins != null)
            {
                foreach (var slotSkin in _currentModelData.DefaultSlotSkins)
                {
                    if (slotSkin == null || string.IsNullOrEmpty(slotSkin.SkinName))
                        continue;

                    _equippedSlotSkins[slotSkin.Slot] = slotSkin.SkinName;
                }
            }

            RebuildRuntimeSkin();
        }

        /// <summary>
        /// baseSkin + 슬롯 스킨 + 추가 attachSkin 을 모두 합쳐서
        /// 하나의 runtime skin 으로 세팅
        /// </summary>
        private void RebuildRuntimeSkin()
        {
            if (_skeletonAnimation == null || _skeletonAnimation.skeleton == null)
                return;

            var skeleton = _skeletonAnimation.skeleton;
            var data = skeleton.Data;

            _runtimeSkin.Clear();

            // 1) base skin (initialSkinName 기준)
            if (!string.IsNullOrEmpty(_skeletonAnimation.initialSkinName))
            {
                var baseSkin = data.FindSkin(_skeletonAnimation.initialSkinName);
                if (baseSkin != null)
                    _runtimeSkin.AddSkin(baseSkin);
            }

            foreach (var pair in _equippedSlotSkins)
            {
                string skinName = pair.Value;

                var slotSkin = data.FindSkin(skinName);
                if (slotSkin != null)
                    _runtimeSkin.AddSkin(slotSkin);
            }

            //// 2) 슬롯별 스킨 (장착/코디)
            //if (_currentModelData != null && _currentModelData.DefaultSlotSkins != null)
            //{
            //    foreach (var defaultSlot in _currentModelData.DefaultSlotSkins)
            //    {
            //        if (defaultSlot == null)
            //            continue;

            //        // 슬롯에 별도 장착 스킨이 있으면 그걸 우선 사용
            //        string skinName = null;
            //        if (!_equippedSlotSkins.TryGetValue(defaultSlot.Slot, out skinName)
            //            || string.IsNullOrEmpty(skinName))
            //        {
            //            skinName = defaultSlot.SkinName;
            //        }

            //        if (string.IsNullOrEmpty(skinName))
            //            continue;

            //        var slotSkin = data.FindSkin(skinName);
            //        if (slotSkin != null)
            //            _runtimeSkin.AddSkin(slotSkin);
            //    }
            //}

            // 3) 추가 attach 스킨 (이펙트/악세사리 등)
            for (int i = 0; i < skinList.Count; ++i)
            {
                var name = skinList[i];
                if (string.IsNullOrEmpty(name))
                    continue;

                var extraSkin = data.FindSkin(name);
                if (extraSkin != null)
                    _runtimeSkin.AddSkin(extraSkin);
            }

            // 실제 스켈레톤에 반영
            skeleton.SetSkin(_runtimeSkin);
            skeleton.SetSlotsToSetupPose();
            skeleton.SetBonesToSetupPose();
            _skeletonAnimation.AnimationState.Apply(skeleton);
        }

        #endregion
    }
}
