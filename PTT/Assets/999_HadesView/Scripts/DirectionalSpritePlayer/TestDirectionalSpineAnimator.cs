using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace GameBerry.TestScene
{
    [Serializable]
    public class DirectionalSpineAnimationClip
    {
        public EightDirection Direction = EightDirection.SouthEast;
        public string AnimationName;
    }

    [Serializable]
    public class DirectionalSpineAnimationEvent
    {
        public string EventName = "AniAction";
        public bool TriggerHit = true;
        public AudioClip Sound;
        [ArrayElementTitle("Direction")]
        public List<FrameParticleEvent> Particles = new List<FrameParticleEvent>();
        public Transform Root;
    }

    [Serializable]
    public class CharacterStateDirectionalSpineAnimationSet
    {
        public CharacterState State = CharacterState.Idle;
        public string AnimationKey = "Default";
        public string AnimationName = "Idle";
        public bool Loop = true;
        public bool TriggerHitOnlyOnce = true;
        public bool UseNormalizedHitFallback = true;
        [Range(0.0f, 1.0f)] public float NormalizedHitTime = 0.35f;
        [ArrayElementTitle("EventName")]
        public List<DirectionalSpineAnimationEvent> EventBindings = new List<DirectionalSpineAnimationEvent>();
        //[ArrayElementTitle("Direction")]
        public List<DirectionalSpineAnimationClip> DirectionClips = new List<DirectionalSpineAnimationClip>();
    }

    public class TestDirectionalSpineAnimator : TestDirectionalAnimator
    {
        [Header("References")]
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private SkeletonAnimation _skeletonAnimation;
        [SerializeField] private MeshRenderer _meshRenderer;

        [SerializeField] private int _sortingOrder = 100;
        [SerializeField] private int _trackIndex;
        [SerializeField] private bool _mirrorLeftDirections = true;
        [SerializeField] private bool _autoReturnToIdleOnAttackComplete = true;
        [SerializeField] private bool _validateAnimationNames = true;
        [SerializeField] private bool _logPlaybackWarnings = true;
        [SerializeField] private bool _logPlaybackDetails = true;
        [SerializeField] private float _fallbackPlaybackDuration = 0.75f;
        [SerializeField] private float _eventFrameRate = 30.0f;

        [Header("Animation Data")]
        [ArrayElementTitle("State")]
        [SerializeField] private List<CharacterStateDirectionalSpineAnimationSet> _stateAnimations
            = new List<CharacterStateDirectionalSpineAnimationSet>();

        private readonly Dictionary<AnimationPlaybackKey, CharacterStateDirectionalSpineAnimationSet> _stateSetLookup
            = new Dictionary<AnimationPlaybackKey, CharacterStateDirectionalSpineAnimationSet>();
        private readonly Dictionary<AnimationPlaybackKey, Dictionary<EightDirection, DirectionalSpineAnimationClip>> _directionLookup
            = new Dictionary<AnimationPlaybackKey, Dictionary<EightDirection, DirectionalSpineAnimationClip>>();

        private CharacterState _currentState = CharacterState.None;
        private string _currentAnimationKey = "Default";
        private string _currentAnimationName = string.Empty;
        private EightDirection _currentDirection = EightDirection.SouthEast;
        private CharacterStateDirectionalSpineAnimationSet _currentStateSet;
        private TrackEntry _currentTrackEntry;
        private bool _isInitialized;
        private bool _eventsBound;
        private bool _hitTriggeredThisPlayback;
        private float _baseSkeletonScaleX = 1.0f;
        private readonly HashSet<string> _playbackWarnings = new HashSet<string>();

        public override CharacterState CurrentState => _currentState;
        public override string CurrentAnimationKey => _currentAnimationKey;
        public override EightDirection CurrentDirection => _currentDirection;
        public override float CurrentPlaybackDuration => GetPlaybackDuration(_currentTrackEntry);
        public override bool AutoReturnToIdleOnAttackComplete
        {
            get => _autoReturnToIdleOnAttackComplete;
            set => _autoReturnToIdleOnAttackComplete = value;
        }

        private void Reset()
        {
            EnsureReferences();
            EnsureStateEntries();
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnValidate()
        {
            EnsureReferences();
            EnsureStateEntries();
            NormalizeAnimationKeys();
            ApplyRendererSettings();
        }

        private void Update()
        {
            RaiseNormalizedHitFallbackIfNeeded();
        }

        private void OnDestroy()
        {
            UnbindSpineEvents();
        }

        public override void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            EnsureReferences();
            EnsureStateEntries();
            NormalizeAnimationKeys();
            RebuildLookup();
            ApplyRendererSettings();

            if (_skeletonAnimation == null)
                return;

            _skeletonAnimation.Initialize(false);
            CacheBaseSkeletonScale();
            BindSpineEvents();
            Play(CharacterState.Idle, "Default", Vector3.down, true);
        }

        public override void Play(CharacterState state, Vector3 moveDirection, bool forceRestart = false)
        {
            Play(state, "Default", moveDirection, forceRestart);
        }

        public override void Play(CharacterState state, string animationKey, Vector3 moveDirection, bool forceRestart = false)
        {
            if (_isInitialized == false)
                Initialize();

            if (state == CharacterState.None || state == CharacterState.Max)
                state = CharacterState.Idle;

            string normalizedAnimationKey = AnimationPlaybackKey.NormalizeAnimationKey(animationKey);
            EightDirection nextDirection = ResolveDirection(moveDirection, _currentDirection);
            bool logPlayback = ShouldLogPlayback(state, normalizedAnimationKey, nextDirection, forceRestart);

            if (logPlayback)
            {
                LogPlayback($"Play request. state={state}, key={normalizedAnimationKey}, move={FormatVector(moveDirection)}, direction={nextDirection}, forceRestart={forceRestart}, currentState={_currentState}, currentKey={_currentAnimationKey}, currentAnim='{_currentAnimationName}'");
            }

            if (TryGetAnimationName(state, normalizedAnimationKey, nextDirection, out string animationName, out bool flipX) == false)
            {
                LogPlaybackWarning($"No animation found for state '{state}', key '{normalizedAnimationKey}', direction '{nextDirection}'.");
                return;
            }

            if (logPlayback)
                LogPlayback($"Resolved animation='{animationName}', flipX={flipX}, stateSet={DescribeStateSet(GetStateAnimationSet(state, normalizedAnimationKey))}");

            if (_skeletonAnimation == null || _skeletonAnimation.AnimationState == null)
            {
                LogPlaybackWarning("SkeletonAnimation or AnimationState is missing.");
                return;
            }

            bool samePlayback = _currentState == state
                && _currentAnimationKey == normalizedAnimationKey
                && _currentAnimationName == animationName;

            if (forceRestart == false
                && samePlayback
                && _currentDirection == nextDirection)
            {
                if (logPlayback && forceRestart)
                    LogPlayback($"Play skipped: same playback animation='{animationName}'.");

                return;
            }

            CharacterState previousState = _currentState;
            string previousAnimationKey = _currentAnimationKey;
            string previousAnimationName = _currentAnimationName;
            EightDirection previousDirection = _currentDirection;
            CharacterStateDirectionalSpineAnimationSet previousStateSet = _currentStateSet;
            TrackEntry previousTrackEntry = _currentTrackEntry;

            ApplyDirectionFlip(flipX);

            TrackEntry currentEntry = _skeletonAnimation.AnimationState.GetCurrent(_trackIndex);
            if (forceRestart == false && samePlayback && currentEntry != null)
            {
                _currentState = state;
                _currentAnimationKey = normalizedAnimationKey;
                _currentAnimationName = animationName;
                _currentDirection = nextDirection;
                _currentStateSet = GetStateAnimationSet(state, normalizedAnimationKey);
                _currentTrackEntry = currentEntry;
                return;
            }

            _hitTriggeredThisPlayback = false;
            _currentState = state;
            _currentAnimationKey = normalizedAnimationKey;
            _currentAnimationName = animationName;
            _currentDirection = nextDirection;
            _currentStateSet = GetStateAnimationSet(state, normalizedAnimationKey);

            try
            {
                bool loop = GetCurrentLoop();
                LogPlayback($"SetAnimation start. track={_trackIndex}, animation='{animationName}', loop={loop}, state={state}, key={normalizedAnimationKey}, direction={nextDirection}");
                _currentTrackEntry = _skeletonAnimation.AnimationState.SetAnimation(_trackIndex, animationName, loop);
                LogPlayback($"SetAnimation ok. animation='{_currentTrackEntry?.Animation?.Name}', duration={_currentTrackEntry?.Animation?.Duration:0.###}");
            }
            catch (Exception exception)
            {
                _currentState = previousState;
                _currentAnimationKey = previousAnimationKey;
                _currentAnimationName = previousAnimationName;
                _currentDirection = previousDirection;
                _currentStateSet = previousStateSet;
                _currentTrackEntry = previousTrackEntry;
                Debug.LogError($"{nameof(TestDirectionalSpineAnimator)} failed to play Spine animation '{animationName}'. {exception}", this);
            }
        }

        public override void SetDirection(Vector3 moveDirection, bool forceRefresh = false)
        {
            CharacterState state = _currentState == CharacterState.None ? CharacterState.Idle : _currentState;
            Play(state, _currentAnimationKey, moveDirection, forceRefresh);
        }

        private void BindSpineEvents()
        {
            if (_skeletonAnimation == null || _skeletonAnimation.AnimationState == null)
                return;

            UnbindSpineEvents();
            _skeletonAnimation.AnimationState.Event += HandleSpineEvent;
            _skeletonAnimation.AnimationState.Complete += HandleSpineComplete;
            _eventsBound = true;
        }

        private void UnbindSpineEvents()
        {
            if (_eventsBound == false || _skeletonAnimation == null || _skeletonAnimation.state == null)
                return;

            _skeletonAnimation.state.Event -= HandleSpineEvent;
            _skeletonAnimation.state.Complete -= HandleSpineComplete;
            _eventsBound = false;
        }

        private void HandleSpineEvent(TrackEntry entry, Spine.Event spineEvent)
        {
            if (IsCurrentEntry(entry) == false || spineEvent?.Data == null)
                return;

            string eventName = spineEvent.Data.Name;
            LogPlayback($"SpineEvent '{eventName}'. animation='{entry.Animation?.Name}', state={_currentState}, key={_currentAnimationKey}, time={entry.AnimationTime:0.###}");
            DirectionalSpineAnimationEvent eventBinding = FindEventBinding(_currentStateSet, eventName);
            if (eventBinding == null)
            {
                LogPlayback($"SpineEvent '{eventName}' has no explicit binding. defaultHit={IsDefaultHitEvent(eventName)}");
                if (IsDefaultHitEvent(eventName))
                    RaiseHitFrame(TimeToFrameIndex(entry.AnimationTime));

                return;
            }

            int frameIndex = TimeToFrameIndex(entry.AnimationTime);
            if (eventBinding.TriggerHit)
                RaiseHitFrame(frameIndex);

            RaiseFrameEventTriggered(new AnimationFrameEventTrigger(
                _currentState,
                _currentAnimationKey,
                frameIndex,
                CreateFrameEventData(eventBinding, frameIndex)));
        }

        private void HandleSpineComplete(TrackEntry entry)
        {
            if (IsCurrentEntry(entry) == false || GetCurrentLoop())
                return;

            CharacterState completedState = _currentState;
            string completedAnimationKey = _currentAnimationKey;
            LogPlayback($"SpineComplete. animation='{entry.Animation?.Name}', state={completedState}, key={completedAnimationKey}");
            RaiseStatePlaybackCompleted(completedState);

            if (completedState == CharacterState.Attack)
            {
                if (_autoReturnToIdleOnAttackComplete)
                    Play(CharacterState.Idle, "Default", DirectionToMoveVector(_currentDirection), true);
            }
            else if (completedState == CharacterState.Hit)
            {
                Play(CharacterState.Idle, "Default", DirectionToMoveVector(_currentDirection), true);
            }
            else if (completedState == CharacterState.Skill && completedAnimationKey == _currentAnimationKey)
            {
                Play(CharacterState.Idle, "Default", DirectionToMoveVector(_currentDirection), true);
            }
        }

        private void RaiseNormalizedHitFallbackIfNeeded()
        {
            if (_currentTrackEntry == null || _currentStateSet == null || _hitTriggeredThisPlayback)
                return;

            if (_currentStateSet.UseNormalizedHitFallback == false || _currentStateSet.NormalizedHitTime < 0.0f)
                return;

            Spine.Animation animation = _currentTrackEntry.Animation;
            if (animation == null || animation.Duration <= 0.0001f)
                return;

            float normalizedTime = Mathf.Clamp01(_currentTrackEntry.AnimationTime / animation.Duration);
            if (normalizedTime < _currentStateSet.NormalizedHitTime)
                return;

            RaiseHitFrame(TimeToFrameIndex(_currentTrackEntry.AnimationTime));
        }

        private void RaiseHitFrame(int frameIndex)
        {
            if (_currentStateSet != null && _currentStateSet.TriggerHitOnlyOnce && _hitTriggeredThisPlayback)
                return;

            _hitTriggeredThisPlayback = true;
            LogPlayback($"RaiseHitFrame. state={_currentState}, key={_currentAnimationKey}, frame={frameIndex}, animation='{_currentAnimationName}'");
            RaiseStateFrameTriggered(_currentState, frameIndex);
        }

        private bool TryGetAnimationName(
            CharacterState state,
            string animationKey,
            EightDirection direction,
            out string animationName,
            out bool flipX)
        {
            animationName = null;
            flipX = false;

            CharacterStateDirectionalSpineAnimationSet stateSet = GetStateAnimationSet(state, animationKey);
            if (stateSet == null)
                return false;

            string candidate = null;
            Dictionary<EightDirection, DirectionalSpineAnimationClip> directionMap = GetDirectionMap(state, animationKey);
            if (directionMap != null)
            {
                if (TryResolveDirectionalAnimation(directionMap, direction, out candidate, out flipX)
                    && TryValidateAnimationName(candidate, state, animationKey, out animationName))
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(candidate) == false)
                    LogPlaybackWarning($"Directional clip candidate '{candidate}' was not found in SkeletonData. state={state}, key={animationKey}, direction={direction}");

                if (TryResolveFallbackDirectionalAnimation(directionMap, out candidate)
                    && TryValidateAnimationName(candidate, state, animationKey, out animationName))
                {
                    flipX = false;
                    return true;
                }

                if (string.IsNullOrWhiteSpace(candidate) == false)
                    LogPlaybackWarning($"Fallback directional clip candidate '{candidate}' was not found in SkeletonData. state={state}, key={animationKey}, direction={direction}");
            }

            candidate = stateSet.AnimationName;
            if (string.IsNullOrWhiteSpace(candidate))
                candidate = animationKey != "Default" ? animationKey : state.ToString();

            flipX = _mirrorLeftDirections && IsLeftDirection(direction);
            bool resolvedDefault = TryValidateAnimationName(candidate, state, animationKey, out animationName);
            if (resolvedDefault == false)
                LogPlaybackWarning($"State default animation candidate '{candidate}' was not found. state={state}, key={animationKey}, direction={direction}");

            return resolvedDefault;
        }

        private bool TryResolveDirectionalAnimation(
            Dictionary<EightDirection, DirectionalSpineAnimationClip> directionMap,
            EightDirection direction,
            out string animationName,
            out bool flipX)
        {
            animationName = null;
            flipX = false;

            if (directionMap.TryGetValue(direction, out DirectionalSpineAnimationClip clip)
                && IsUsableClip(clip))
            {
                animationName = clip.AnimationName;
                return true;
            }

            if (_mirrorLeftDirections == false || IsLeftDirection(direction) == false)
                return false;

            EightDirection mirroredDirection = GetMirroredDirection(direction);
            if (directionMap.TryGetValue(mirroredDirection, out clip) == false || IsUsableClip(clip) == false)
                return false;

            animationName = clip.AnimationName;
            flipX = true;
            return true;
        }

        private bool TryResolveFallbackDirectionalAnimation(
            Dictionary<EightDirection, DirectionalSpineAnimationClip> directionMap,
            out string animationName)
        {
            animationName = null;

            if (directionMap.TryGetValue(EightDirection.SouthEast, out DirectionalSpineAnimationClip clip)
                && IsUsableClip(clip))
            {
                animationName = clip.AnimationName;
                return true;
            }

            if (directionMap.TryGetValue(EightDirection.NorthEast, out clip)
                && IsUsableClip(clip))
            {
                animationName = clip.AnimationName;
                return true;
            }

            foreach (DirectionalSpineAnimationClip value in directionMap.Values)
            {
                if (IsUsableClip(value) == false)
                    continue;

                animationName = value.AnimationName;
                return true;
            }

            return false;
        }

        private bool TryValidateAnimationName(
            string candidate,
            CharacterState state,
            string animationKey,
            out string animationName)
        {
            animationName = null;

            if (string.IsNullOrWhiteSpace(candidate) == false && (_validateAnimationNames == false || FindAnimation(candidate) != null))
            {
                animationName = candidate.Trim();
                return true;
            }

            if (animationKey != "Default" && FindAnimation(animationKey) != null)
            {
                animationName = animationKey;
                return true;
            }

            string stateName = state.ToString();
            if (FindAnimation(stateName) != null)
            {
                animationName = stateName;
                return true;
            }

            if (_validateAnimationNames == false && string.IsNullOrWhiteSpace(candidate) == false)
            {
                animationName = candidate.Trim();
                return true;
            }

            return false;
        }

        private void LogPlaybackWarning(string message)
        {
            if (_logPlaybackWarnings == false || string.IsNullOrWhiteSpace(message))
                return;

            if (_playbackWarnings.Add(message) == false)
                return;

            Debug.LogWarning($"{nameof(TestDirectionalSpineAnimator)}: {message}", this);
        }

        private void LogPlayback(string message)
        {
            if (_logPlaybackDetails == false || string.IsNullOrWhiteSpace(message))
                return;

            Debug.Log($"{nameof(TestDirectionalSpineAnimator)}[{name} frame={Time.frameCount}]: {message}", this);
        }

        private bool ShouldLogPlayback(CharacterState state, string animationKey, EightDirection direction, bool forceRestart)
        {
            if (_logPlaybackDetails == false)
                return false;

            if (forceRestart == false
                && _currentState == state
                && _currentAnimationKey == animationKey
                && _currentDirection == direction)
            {
                return false;
            }

            return forceRestart
                || animationKey != "Default"
                || state == CharacterState.Attack
                || state == CharacterState.Hit
                || state == CharacterState.Skill
                || state == CharacterState.Dead;
        }

        private static string DescribeStateSet(CharacterStateDirectionalSpineAnimationSet stateSet)
        {
            if (stateSet == null)
                return "null";

            int directionCount = stateSet.DirectionClips != null ? stateSet.DirectionClips.Count : 0;
            int eventCount = stateSet.EventBindings != null ? stateSet.EventBindings.Count : 0;
            return $"{stateSet.State}/{AnimationPlaybackKey.NormalizeAnimationKey(stateSet.AnimationKey)}(animation='{stateSet.AnimationName}', loop={stateSet.Loop}, directions={directionCount}, events={eventCount}, normalizedFallback={stateSet.UseNormalizedHitFallback})";
        }

        private static string FormatVector(Vector3 vector)
        {
            return $"({vector.x:0.###}, {vector.y:0.###}, {vector.z:0.###})";
        }

        private CharacterStateDirectionalSpineAnimationSet GetStateAnimationSet(CharacterState state, string animationKey)
        {
            if (_stateSetLookup.Count == 0)
                RebuildLookup();

            AnimationPlaybackKey playbackKey = new AnimationPlaybackKey(state, animationKey);
            if (_stateSetLookup.TryGetValue(playbackKey, out CharacterStateDirectionalSpineAnimationSet stateSet))
                return stateSet;

            if (playbackKey.AnimationKey != "Default")
                _stateSetLookup.TryGetValue(new AnimationPlaybackKey(state, "Default"), out stateSet);

            return stateSet;
        }

        private Dictionary<EightDirection, DirectionalSpineAnimationClip> GetDirectionMap(CharacterState state, string animationKey)
        {
            AnimationPlaybackKey playbackKey = new AnimationPlaybackKey(state, animationKey);
            if (_directionLookup.TryGetValue(playbackKey, out Dictionary<EightDirection, DirectionalSpineAnimationClip> directionMap))
                return directionMap;

            if (playbackKey.AnimationKey != "Default")
                _directionLookup.TryGetValue(new AnimationPlaybackKey(state, "Default"), out directionMap);

            return directionMap;
        }

        private void RebuildLookup()
        {
            _stateSetLookup.Clear();
            _directionLookup.Clear();

            for (int i = 0; i < _stateAnimations.Count; i++)
            {
                CharacterStateDirectionalSpineAnimationSet stateSet = _stateAnimations[i];
                if (stateSet == null)
                    continue;

                stateSet.AnimationKey = AnimationPlaybackKey.NormalizeAnimationKey(stateSet.AnimationKey);
                AnimationPlaybackKey playbackKey = new AnimationPlaybackKey(stateSet.State, stateSet.AnimationKey);
                _stateSetLookup[playbackKey] = stateSet;

                if (stateSet.DirectionClips == null || stateSet.DirectionClips.Count == 0)
                    continue;

                Dictionary<EightDirection, DirectionalSpineAnimationClip> directionMap
                    = new Dictionary<EightDirection, DirectionalSpineAnimationClip>();
                _directionLookup[playbackKey] = directionMap;

                for (int j = 0; j < stateSet.DirectionClips.Count; j++)
                {
                    DirectionalSpineAnimationClip clip = stateSet.DirectionClips[j];
                    if (clip == null)
                        continue;

                    directionMap[clip.Direction] = clip;
                }
            }
        }

        private void EnsureReferences()
        {
            if (_skeletonAnimation == null)
                _skeletonAnimation = GetComponent<SkeletonAnimation>();

            if (_skeletonAnimation == null)
                _skeletonAnimation = GetComponentInChildren<SkeletonAnimation>(true);

            if (_visualRoot == null && _skeletonAnimation != null)
                _visualRoot = _skeletonAnimation.transform;

            if (_meshRenderer == null && _skeletonAnimation != null)
                _meshRenderer = _skeletonAnimation.GetComponent<MeshRenderer>();
        }

        private void ApplyRendererSettings()
        {
            if (_meshRenderer != null)
                _meshRenderer.sortingOrder = _sortingOrder;
        }

        private void CacheBaseSkeletonScale()
        {
            if (_skeletonAnimation == null || _skeletonAnimation.Skeleton == null)
                return;

            float scaleX = _skeletonAnimation.Skeleton.ScaleX;
            _baseSkeletonScaleX = Mathf.Abs(scaleX) <= 0.0001f ? 1.0f : Mathf.Abs(scaleX);
        }

        private void ApplyDirectionFlip(bool flipX)
        {
            if (_skeletonAnimation == null || _skeletonAnimation.Skeleton == null)
                return;

            _skeletonAnimation.Skeleton.ScaleX = flipX ? -_baseSkeletonScaleX : _baseSkeletonScaleX;
        }

        private void EnsureStateEntries()
        {
            EnsureStateEntry(CharacterState.Idle, true);
            EnsureStateEntry(CharacterState.Run, true);
            EnsureStateEntry(CharacterState.Attack, false);
            EnsureStateEntry(CharacterState.Hit, false);
            EnsureStateEntry(CharacterState.Dead, false);
            EnsureStateEntry(CharacterState.Skill, false);
            EnsureStateEntry(CharacterState.Tran, false);
        }

        private void EnsureStateEntry(CharacterState state, bool defaultLoop)
        {
            CharacterStateDirectionalSpineAnimationSet stateSet = _stateAnimations.Find(
                data => data != null
                     && data.State == state
                     && AnimationPlaybackKey.NormalizeAnimationKey(data.AnimationKey) == "Default");
            if (stateSet == null)
            {
                stateSet = new CharacterStateDirectionalSpineAnimationSet
                {
                    State = state,
                    AnimationKey = "Default",
                    AnimationName = GetDefaultAnimationName(state),
                    Loop = defaultLoop,
                    UseNormalizedHitFallback = IsDefaultHit(state),
                    NormalizedHitTime = IsDefaultHit(state) ? 0.35f : -1.0f,
                };
                _stateAnimations.Add(stateSet);
            }
            else
            {
                stateSet.AnimationKey = AnimationPlaybackKey.NormalizeAnimationKey(stateSet.AnimationKey);
                if (string.IsNullOrWhiteSpace(stateSet.AnimationName))
                    stateSet.AnimationName = GetDefaultAnimationName(state);

                if (IsDefaultHit(state) == false)
                {
                    stateSet.UseNormalizedHitFallback = false;
                    stateSet.NormalizedHitTime = -1.0f;
                }
            }

            EnsureDefaultEventBindings(stateSet);
        }

        private void NormalizeAnimationKeys()
        {
            for (int i = 0; i < _stateAnimations.Count; i++)
            {
                CharacterStateDirectionalSpineAnimationSet stateSet = _stateAnimations[i];
                if (stateSet == null)
                    continue;

                stateSet.AnimationKey = AnimationPlaybackKey.NormalizeAnimationKey(stateSet.AnimationKey);
                EnsureDefaultEventBindings(stateSet);
            }
        }

        private void EnsureDefaultEventBindings(CharacterStateDirectionalSpineAnimationSet stateSet)
        {
            if (stateSet == null || IsDefaultHit(stateSet.State) == false)
                return;

            if (stateSet.EventBindings == null)
                stateSet.EventBindings = new List<DirectionalSpineAnimationEvent>();

            for (int i = 0; i < stateSet.EventBindings.Count; i++)
            {
                DirectionalSpineAnimationEvent eventBinding = stateSet.EventBindings[i];
                if (eventBinding != null && string.Equals(eventBinding.EventName, "AniAction", StringComparison.OrdinalIgnoreCase))
                    return;
            }

            stateSet.EventBindings.Add(new DirectionalSpineAnimationEvent
            {
                EventName = "AniAction",
                TriggerHit = true,
            });
        }

        private bool GetCurrentLoop()
        {
            return _currentStateSet != null ? _currentStateSet.Loop : GetDefaultLoop(_currentState);
        }

        private Spine.Animation FindAnimation(string animationName)
        {
            if (_skeletonAnimation == null || string.IsNullOrWhiteSpace(animationName))
                return null;

            if (_skeletonAnimation.Skeleton != null && _skeletonAnimation.Skeleton.Data != null)
                return _skeletonAnimation.Skeleton.Data.FindAnimation(animationName.Trim());

            return _skeletonAnimation.skeletonDataAsset != null
                ? _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true)?.FindAnimation(animationName.Trim())
                : null;
        }

        private DirectionalSpineAnimationEvent FindEventBinding(
            CharacterStateDirectionalSpineAnimationSet stateSet,
            string eventName)
        {
            if (stateSet?.EventBindings == null || string.IsNullOrWhiteSpace(eventName))
                return null;

            for (int i = 0; i < stateSet.EventBindings.Count; i++)
            {
                DirectionalSpineAnimationEvent eventBinding = stateSet.EventBindings[i];
                if (eventBinding == null || string.IsNullOrWhiteSpace(eventBinding.EventName))
                    continue;

                if (string.Equals(eventBinding.EventName.Trim(), eventName, StringComparison.OrdinalIgnoreCase))
                    return eventBinding;
            }

            return null;
        }

        private bool IsCurrentEntry(TrackEntry entry)
        {
            if (entry == null || entry.TrackIndex != _trackIndex)
                return false;

            if (_currentTrackEntry != null && entry == _currentTrackEntry)
                return true;

            return entry.Animation != null && entry.Animation.Name == _currentAnimationName;
        }

        private float GetPlaybackDuration(TrackEntry entry)
        {
            Spine.Animation animation = entry?.Animation ?? FindAnimation(_currentAnimationName);
            if (animation == null || animation.Duration <= 0.0001f)
                return _fallbackPlaybackDuration;

            float stateScale = _skeletonAnimation != null && _skeletonAnimation.AnimationState != null
                ? _skeletonAnimation.AnimationState.TimeScale
                : 1.0f;
            float entryScale = entry != null ? entry.TimeScale : 1.0f;
            float effectiveScale = Mathf.Max(0.0001f, stateScale * entryScale);
            return animation.Duration / effectiveScale;
        }

        private int TimeToFrameIndex(float animationTime)
        {
            return Mathf.Max(0, Mathf.FloorToInt(animationTime * Mathf.Max(1.0f, _eventFrameRate)));
        }

        private static AnimationFrameEventData CreateFrameEventData(DirectionalSpineAnimationEvent eventBinding, int frameIndex)
        {
            return new AnimationFrameEventData
            {
                FrameIndex = frameIndex,
                TriggerHit = eventBinding.TriggerHit,
                Sound = eventBinding.Sound,
                Particles = eventBinding.Particles,
                Root = eventBinding.Root,
            };
        }

        private static bool IsUsableClip(DirectionalSpineAnimationClip clip)
        {
            return clip != null && string.IsNullOrWhiteSpace(clip.AnimationName) == false;
        }

        private static bool IsDefaultHit(CharacterState state)
        {
            return state == CharacterState.Attack || state == CharacterState.Skill;
        }

        private static bool IsDefaultHitEvent(string eventName)
        {
            return string.Equals(eventName, "AniAction", StringComparison.OrdinalIgnoreCase)
                || string.Equals(eventName, "Hit", StringComparison.OrdinalIgnoreCase);
        }

        private static bool GetDefaultLoop(CharacterState state)
        {
            return state == CharacterState.Idle || state == CharacterState.Run;
        }

        private static string GetDefaultAnimationName(CharacterState state)
        {
            switch (state)
            {
                case CharacterState.None:
                case CharacterState.Max:
                    return CharacterState.Idle.ToString();
                default:
                    return state.ToString();
            }
        }
    }
}
