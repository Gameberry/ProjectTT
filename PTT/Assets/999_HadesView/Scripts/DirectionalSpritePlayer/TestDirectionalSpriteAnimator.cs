using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public enum EightDirection
    {
        North = 0,
        NorthEast = 1,
        East = 2,
        SouthEast = 3,
        South = 4,
        SouthWest = 5,
        West = 6,
        NorthWest = 7,
    }

    [Serializable]
    public class DirectionalSpriteAnimationClip
    {
        public EightDirection Direction = EightDirection.South;
        public Sprite[] Frames = Array.Empty<Sprite>();
    }

    [Serializable]
    public class CharacterStateDirectionalAnimationSet
    {
        public CharacterState State = CharacterState.Idle;
        public float FramesPerSecond = 6.0f;
        public bool Loop = true;
        [ArrayElementTitle("Direction")]
        public List<DirectionalSpriteAnimationClip> DirectionClips = new List<DirectionalSpriteAnimationClip>();
    }

    public class TestDirectionalSpriteAnimator : MonoBehaviour
    {
        public event Action<CharacterState> StatePlaybackCompleted;

        [Header("References")]
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [SerializeField] private int _sortingOrder = 100;
        [SerializeField] private bool _generatePlaceholderAnimations = true;
        [SerializeField] private bool _mirrorLeftDirections = true;
        [SerializeField] private bool _autoReturnToIdleOnAttackComplete = true;

        [Header("Animation Data")] [ArrayElementTitle("State")]
        [SerializeField] private List<CharacterStateDirectionalAnimationSet> _stateAnimations = new List<CharacterStateDirectionalAnimationSet>();

        private readonly Dictionary<CharacterState, Dictionary<EightDirection, DirectionalSpriteAnimationClip>> _animationLookup
            = new Dictionary<CharacterState, Dictionary<EightDirection, DirectionalSpriteAnimationClip>>();
        private readonly Dictionary<CharacterState, CharacterStateDirectionalAnimationSet> _stateSetLookup
            = new Dictionary<CharacterState, CharacterStateDirectionalAnimationSet>();

        private CharacterState _currentState = CharacterState.None;
        private EightDirection _currentDirection = EightDirection.South;
        private DirectionalSpriteAnimationClip _currentClip;
        private CharacterStateDirectionalAnimationSet _currentStateSet;
        private int _currentFrameIndex;
        private float _frameTimer;
        private bool _isInitialized;
        private bool _currentFlipX;

        public CharacterState CurrentState => _currentState;
        public EightDirection CurrentDirection => _currentDirection;
        public bool AutoReturnToIdleOnAttackComplete
        {
            get => _autoReturnToIdleOnAttackComplete;
            set => _autoReturnToIdleOnAttackComplete = value;
        }

        private void Reset()
        {
            EnsureVisualObjects();
            EnsureStateEntries();
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnValidate()
        {
            EnsureVisualObjects();
            EnsureStateEntries();
            ApplyVisualSettings();
        }

        private void Update()
        {
            if (_currentClip == null || _currentClip.Frames == null || _currentClip.Frames.Length == 0)
                return;

            float framesPerSecond = GetCurrentFramesPerSecond();
            if (framesPerSecond <= 0.0f)
                return;

            _frameTimer += Time.deltaTime;
            float frameDuration = 1.0f / framesPerSecond;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                if (AdvanceFrame())
                    return;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            EnsureVisualObjects();
            EnsureStateEntries();

            if (_generatePlaceholderAnimations && HasAssignedSprites() == false)
                BuildPlaceholderAnimations();

            RebuildLookup();
            ApplyVisualSettings();
            Play(CharacterState.Idle, Vector3.down, true);
        }

        public void Play(CharacterState state, Vector3 moveDirection, bool forceRestart = false)
        {
            if (_isInitialized == false)
                Initialize();

            if (state == CharacterState.None || state == CharacterState.Max)
                state = CharacterState.Idle;

            EightDirection nextDirection = ResolveDirection(moveDirection, _currentDirection);
            if (forceRestart == false && _currentState == state && _currentDirection == nextDirection)
                return;

            if (TryGetClip(state, nextDirection, out DirectionalSpriteAnimationClip clip) == false)
                return;

            bool preserveFrameProgress = forceRestart == false && _currentState == state;
            int preservedFrameIndex = _currentFrameIndex;
            float preservedFrameTimer = _frameTimer;

            _currentState = state;
            _currentDirection = nextDirection;
            _currentClip = clip;
            _currentStateSet = GetStateAnimationSet(state);

            if (preserveFrameProgress && _currentClip != null && _currentClip.Frames != null && _currentClip.Frames.Length > 0)
            {
                _currentFrameIndex = Mathf.Clamp(preservedFrameIndex, 0, _currentClip.Frames.Length - 1);
                _frameTimer = preservedFrameTimer;
            }
            else
            {
                _currentFrameIndex = 0;
                _frameTimer = 0.0f;
            }

            ApplyCurrentFrame();
        }

        public void SetDirection(Vector3 moveDirection, bool forceRefresh = false)
        {
            Play(_currentState == CharacterState.None ? CharacterState.Idle : _currentState, moveDirection, forceRefresh);
        }

        public static EightDirection ResolveDirection(Vector3 moveDirection, EightDirection fallback)
        {
            Vector2 planar = new Vector2(moveDirection.x, moveDirection.y);
            if (planar.sqrMagnitude <= 0.0001f)
                return fallback;

            float angle = Mathf.Atan2(planar.x, planar.y) * Mathf.Rad2Deg;
            if (angle < 0.0f)
                angle += 360.0f;

            int sector = Mathf.RoundToInt(angle / 45.0f) % 8;
            return (EightDirection)sector;
        }

        private bool AdvanceFrame()
        {
            if (_currentClip == null || _currentClip.Frames == null || _currentClip.Frames.Length == 0)
                return true;

            int lastFrameIndex = _currentClip.Frames.Length - 1;

            if (GetCurrentLoop())
            {
                _currentFrameIndex = (_currentFrameIndex + 1) % _currentClip.Frames.Length;
                ApplyCurrentFrame();
                return false;
            }

            if (_currentFrameIndex < lastFrameIndex)
            {
                _currentFrameIndex++;
                ApplyCurrentFrame();
                return false;
            }

            HandleNonLoopAnimationFinished();
            return true;
        }

        private void ApplyCurrentFrame()
        {
            if (_spriteRenderer == null || _currentClip == null || _currentClip.Frames == null || _currentClip.Frames.Length == 0)
                return;

            _spriteRenderer.flipX = _currentFlipX;
            _spriteRenderer.sprite = _currentClip.Frames[Mathf.Clamp(_currentFrameIndex, 0, _currentClip.Frames.Length - 1)];
        }

        private void HandleNonLoopAnimationFinished()
        {
            if (GetCurrentLoop())
                return;

            CharacterState completedState = _currentState;
            if (completedState == CharacterState.Attack)
            {
                StatePlaybackCompleted?.Invoke(completedState);

                if (_autoReturnToIdleOnAttackComplete)
                    Play(CharacterState.Idle, DirectionToMoveVector(_currentDirection), true);
            }
        }

        private bool TryGetClip(CharacterState state, EightDirection direction, out DirectionalSpriteAnimationClip clip)
        {
            clip = null;
            _currentFlipX = false;

            if (_animationLookup.Count == 0)
                RebuildLookup();

            if (_animationLookup.TryGetValue(state, out Dictionary<EightDirection, DirectionalSpriteAnimationClip> directionMap) == false)
                return false;

            if (TryResolveDirectionalClip(directionMap, direction, out clip, out bool flipX))
            {
                _currentFlipX = flipX;
                return clip != null;
            }

            if (directionMap.TryGetValue(EightDirection.South, out clip))
                return clip != null;

            foreach (DirectionalSpriteAnimationClip value in directionMap.Values)
            {
                if (value == null)
                    continue;

                clip = value;
                return true;
            }

            return false;
        }

        private bool TryResolveDirectionalClip(
            Dictionary<EightDirection, DirectionalSpriteAnimationClip> directionMap,
            EightDirection direction,
            out DirectionalSpriteAnimationClip clip,
            out bool flipX)
        {
            clip = null;
            flipX = false;

            if (directionMap.TryGetValue(direction, out clip) && IsUsableClip(clip))
                return true;

            if (_mirrorLeftDirections == false || IsLeftDirection(direction) == false)
                return false;

            EightDirection mirroredDirection = GetMirroredDirection(direction);
            if (directionMap.TryGetValue(mirroredDirection, out clip) == false || IsUsableClip(clip) == false)
                return false;

            flipX = true;
            return true;
        }

        private void RebuildLookup()
        {
            _animationLookup.Clear();
            _stateSetLookup.Clear();

            for (int i = 0; i < _stateAnimations.Count; i++)
            {
                CharacterStateDirectionalAnimationSet stateSet = _stateAnimations[i];
                if (stateSet == null)
                    continue;

                _stateSetLookup[stateSet.State] = stateSet;

                if (_animationLookup.TryGetValue(stateSet.State, out Dictionary<EightDirection, DirectionalSpriteAnimationClip> directionMap) == false)
                {
                    directionMap = new Dictionary<EightDirection, DirectionalSpriteAnimationClip>();
                    _animationLookup.Add(stateSet.State, directionMap);
                }

                for (int j = 0; j < stateSet.DirectionClips.Count; j++)
                {
                    DirectionalSpriteAnimationClip clip = stateSet.DirectionClips[j];
                    if (clip == null)
                        continue;

                    directionMap[clip.Direction] = clip;
                }
            }
        }

        private bool HasAssignedSprites()
        {
            for (int i = 0; i < _stateAnimations.Count; i++)
            {
                CharacterStateDirectionalAnimationSet stateSet = _stateAnimations[i];
                if (stateSet == null)
                    continue;

                for (int j = 0; j < stateSet.DirectionClips.Count; j++)
                {
                    DirectionalSpriteAnimationClip clip = stateSet.DirectionClips[j];
                    if (clip == null || clip.Frames == null)
                        continue;

                    for (int k = 0; k < clip.Frames.Length; k++)
                    {
                        if (clip.Frames[k] != null)
                            return true;
                    }
                }
            }

            return false;
        }

        private void EnsureVisualObjects()
        {
            if (_visualRoot == null)
            {
                Transform child = transform.Find("VisualRoot");
                if (child == null)
                {
                    GameObject visualObject = new GameObject("VisualRoot");
                    child = visualObject.transform;
                    child.SetParent(transform, false);
                }

                _visualRoot = child;
            }

            if (_spriteRenderer == null && _visualRoot != null)
            {
                _spriteRenderer = _visualRoot.GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null)
                    _spriteRenderer = _visualRoot.gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private void ApplyVisualSettings()
        {
            if (_visualRoot != null)
            {
                _visualRoot.localPosition = Vector3.zero;
                _visualRoot.localRotation = Quaternion.identity;
                _visualRoot.localScale = Vector3.one;
            }

            if (_spriteRenderer != null)
                _spriteRenderer.sortingOrder = _sortingOrder;
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
            CharacterStateDirectionalAnimationSet stateSet = _stateAnimations.Find(data => data != null && data.State == state);
            if (stateSet == null)
            {
                stateSet = new CharacterStateDirectionalAnimationSet
                {
                    State = state,
                    FramesPerSecond = GetDefaultFramesPerSecond(state),
                    Loop = defaultLoop,
                };
                _stateAnimations.Add(stateSet);
            }
            else if (stateSet.FramesPerSecond <= 0.0f)
            {
                stateSet.FramesPerSecond = GetDefaultFramesPerSecond(state);
            }

            EightDirection[] requiredDirections = GetRequiredDirections();
            for (int i = 0; i < requiredDirections.Length; i++)
            {
                EightDirection direction = requiredDirections[i];
                DirectionalSpriteAnimationClip clip = stateSet.DirectionClips.Find(data => data != null && data.Direction == direction);
                if (clip != null)
                    continue;

                stateSet.DirectionClips.Add(new DirectionalSpriteAnimationClip
                {
                    Direction = direction,
                });
            }
        }

        private EightDirection[] GetRequiredDirections()
        {
            if (_mirrorLeftDirections)
            {
                return new[]
                {
                    EightDirection.North,
                    EightDirection.NorthEast,
                    EightDirection.East,
                    EightDirection.SouthEast,
                    EightDirection.South,
                };
            }

            return new[]
            {
                EightDirection.North,
                EightDirection.NorthEast,
                EightDirection.East,
                EightDirection.SouthEast,
                EightDirection.South,
                EightDirection.SouthWest,
                EightDirection.West,
                EightDirection.NorthWest,
            };
        }

        private void BuildPlaceholderAnimations()
        {
            for (int i = 0; i < _stateAnimations.Count; i++)
            {
                CharacterStateDirectionalAnimationSet stateSet = _stateAnimations[i];
                if (stateSet == null)
                    continue;

                for (int j = 0; j < stateSet.DirectionClips.Count; j++)
                {
                    DirectionalSpriteAnimationClip clip = stateSet.DirectionClips[j];
                    if (clip == null || (clip.Frames != null && clip.Frames.Length > 0 && clip.Frames[0] != null))
                        continue;

                    clip.Frames = CreatePlaceholderFrames(stateSet.State, clip.Direction);
                }
            }
        }

        private CharacterStateDirectionalAnimationSet GetStateAnimationSet(CharacterState state)
        {
            if (_stateSetLookup.Count == 0)
                RebuildLookup();

            _stateSetLookup.TryGetValue(state, out CharacterStateDirectionalAnimationSet stateSet);
            return stateSet;
        }

        private float GetCurrentFramesPerSecond()
        {
            if (_currentStateSet != null && _currentStateSet.FramesPerSecond > 0.0f)
                return _currentStateSet.FramesPerSecond;

            return GetDefaultFramesPerSecond(_currentState);
        }

        private bool GetCurrentLoop()
        {
            if (_currentStateSet != null)
                return _currentStateSet.Loop;

            return GetDefaultLoop(_currentState);
        }

        private static float GetDefaultFramesPerSecond(CharacterState state)
        {
            return state == CharacterState.Run ? 10.0f : 6.0f;
        }

        private static bool GetDefaultLoop(CharacterState state)
        {
            return state == CharacterState.Idle || state == CharacterState.Run;
        }

        private static bool ShouldLoop(CharacterState state)
        {
            return state == CharacterState.Idle || state == CharacterState.Run;
        }

        private static Sprite[] CreatePlaceholderFrames(CharacterState state, EightDirection direction)
        {
            int frameCount = GetPlaceholderFrameCount(state);
            Sprite[] frames = new Sprite[frameCount];

            for (int i = 0; i < frameCount; i++)
                frames[i] = CreatePlaceholderSprite(state, direction, i, frameCount);

            return frames;
        }

        private static int GetPlaceholderFrameCount(CharacterState state)
        {
            switch (state)
            {
                case CharacterState.Run:
                    return 4;
                case CharacterState.Attack:
                case CharacterState.Skill:
                case CharacterState.Tran:
                    return 3;
                case CharacterState.Dead:
                    return 1;
                default:
                    return 2;
            }
        }

        private static Sprite CreatePlaceholderSprite(CharacterState state, EightDirection direction, int frameIndex, int frameCount)
        {
            const int size = 48;
            const float pixelsPerUnit = 32.0f;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"Test_{state}_{direction}_{frameIndex}"
            };

            Color32[] pixels = new Color32[size * size];
            Color bodyColor = GetStateColor(state);
            Color outlineColor = bodyColor * 0.55f;
            outlineColor.a = 1.0f;
            Color accentColor = Color.Lerp(bodyColor, Color.white, 0.45f);

            Vector2 dir = DirectionToVector(direction);
            Vector2 perpendicular = new Vector2(-dir.y, dir.x);
            float step = frameCount <= 1 ? 0.0f : frameIndex / (float)(frameCount - 1);
            float bob = state == CharacterState.Run ? Mathf.Sin(step * Mathf.PI * 2.0f) * 2.0f : 0.0f;

            Vector2 bodyCenter = new Vector2(size * 0.5f, 20.0f + bob);
            Vector2 headCenter = bodyCenter + dir * 7.0f + new Vector2(0.0f, 10.0f);
            Vector2 markerCenter = bodyCenter + dir * 11.0f + new Vector2(0.0f, 2.0f);
            Vector2 footCenterLeft = bodyCenter + perpendicular * 4.0f + new Vector2(0.0f, -10.0f);
            Vector2 footCenterRight = bodyCenter - perpendicular * 4.0f + new Vector2(0.0f, -10.0f);

            if (state == CharacterState.Run)
            {
                float stride = Mathf.Sin(step * Mathf.PI * 2.0f) * 3.0f;
                footCenterLeft += dir * stride;
                footCenterRight -= dir * stride;
            }
            else if (state == CharacterState.Attack || state == CharacterState.Skill)
            {
                markerCenter += dir * Mathf.Lerp(0.0f, 6.0f, step);
            }
            else if (state == CharacterState.Hit)
            {
                bodyCenter -= dir * 2.0f;
                headCenter -= dir * 2.0f;
            }
            else if (state == CharacterState.Dead)
            {
                bodyCenter = new Vector2(size * 0.5f, 13.0f);
                headCenter = bodyCenter + perpendicular * 6.0f;
                markerCenter = bodyCenter - perpendicular * 5.0f;
                footCenterLeft = bodyCenter + dir * 6.0f;
                footCenterRight = bodyCenter - dir * 6.0f;
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = x + y * size;
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    Color color = Color.clear;

                    if (InsideEllipse(point, new Vector2(size * 0.5f, 8.0f), 11.0f, 4.0f))
                        color = new Color(0.0f, 0.0f, 0.0f, 0.18f);

                    if (InsideEllipse(point, bodyCenter, 9.0f, 11.0f))
                        color = bodyColor;

                    if (InsideCircle(point, headCenter, 5.0f))
                        color = accentColor;

                    if (InsideCircle(point, footCenterLeft, 2.8f) || InsideCircle(point, footCenterRight, 2.8f))
                        color = outlineColor;

                    if (InsideTriangle(point, markerCenter + dir * 4.0f, markerCenter - perpendicular * 3.0f, markerCenter + perpendicular * 3.0f))
                        color = Color.white;

                    if (color.a > 0.0f && (InsideOutline(point, bodyCenter, 9.0f, 11.0f, 1.2f) || InsideCircleOutline(point, headCenter, 5.0f, 1.0f)))
                        color = outlineColor;

                    pixels[index] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            Rect rect = new Rect(0.0f, 0.0f, size, size);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.1f), pixelsPerUnit);
        }

        private static Color GetStateColor(CharacterState state)
        {
            switch (state)
            {
                case CharacterState.Run:
                    return new Color(0.22f, 0.77f, 0.42f);
                case CharacterState.Attack:
                    return new Color(0.95f, 0.52f, 0.12f);
                case CharacterState.Hit:
                    return new Color(0.86f, 0.24f, 0.24f);
                case CharacterState.Dead:
                    return new Color(0.40f, 0.40f, 0.40f);
                case CharacterState.Skill:
                    return new Color(0.18f, 0.72f, 0.90f);
                case CharacterState.Tran:
                    return new Color(0.95f, 0.82f, 0.22f);
                default:
                    return new Color(0.28f, 0.56f, 0.92f);
            }
        }

        private static Vector2 DirectionToVector(EightDirection direction)
        {
            switch (direction)
            {
                case EightDirection.North:
                    return new Vector2(0.0f, 1.0f);
                case EightDirection.NorthEast:
                    return new Vector2(1.0f, 1.0f).normalized;
                case EightDirection.East:
                    return new Vector2(1.0f, 0.0f);
                case EightDirection.SouthEast:
                    return new Vector2(1.0f, -1.0f).normalized;
                case EightDirection.South:
                    return new Vector2(0.0f, -1.0f);
                case EightDirection.SouthWest:
                    return new Vector2(-1.0f, -1.0f).normalized;
                case EightDirection.West:
                    return new Vector2(-1.0f, 0.0f);
                default:
                    return new Vector2(-1.0f, 1.0f).normalized;
            }
        }

        private static Vector3 DirectionToMoveVector(EightDirection direction)
        {
            Vector2 planarDirection = DirectionToVector(direction);
            return new Vector3(planarDirection.x, planarDirection.y, 0.0f);
        }

        private static bool IsLeftDirection(EightDirection direction)
        {
            return direction == EightDirection.West
                || direction == EightDirection.NorthWest
                || direction == EightDirection.SouthWest;
        }

        private static bool IsUsableClip(DirectionalSpriteAnimationClip clip)
        {
            if (clip == null || clip.Frames == null || clip.Frames.Length == 0)
                return false;

            for (int i = 0; i < clip.Frames.Length; i++)
            {
                if (clip.Frames[i] != null)
                    return true;
            }

            return false;
        }

        private static EightDirection GetMirroredDirection(EightDirection direction)
        {
            switch (direction)
            {
                case EightDirection.West:
                    return EightDirection.East;
                case EightDirection.NorthWest:
                    return EightDirection.NorthEast;
                case EightDirection.SouthWest:
                    return EightDirection.SouthEast;
                default:
                    return direction;
            }
        }

        private static bool InsideEllipse(Vector2 point, Vector2 center, float radiusX, float radiusY)
        {
            float dx = (point.x - center.x) / radiusX;
            float dy = (point.y - center.y) / radiusY;
            return dx * dx + dy * dy <= 1.0f;
        }

        private static bool InsideOutline(Vector2 point, Vector2 center, float radiusX, float radiusY, float thickness)
        {
            return InsideEllipse(point, center, radiusX, radiusY) && InsideEllipse(point, center, radiusX - thickness, radiusY - thickness) == false;
        }

        private static bool InsideCircle(Vector2 point, Vector2 center, float radius)
        {
            return (point - center).sqrMagnitude <= radius * radius;
        }

        private static bool InsideCircleOutline(Vector2 point, Vector2 center, float radius, float thickness)
        {
            float distance = (point - center).magnitude;
            return distance <= radius && distance >= radius - thickness;
        }

        private static bool InsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float denominator = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
            if (Mathf.Abs(denominator) <= Mathf.Epsilon)
                return false;

            float alpha = ((b.y - c.y) * (point.x - c.x) + (c.x - b.x) * (point.y - c.y)) / denominator;
            float beta = ((c.y - a.y) * (point.x - c.x) + (a.x - c.x) * (point.y - c.y)) / denominator;
            float gamma = 1.0f - alpha - beta;

            return alpha >= 0.0f && beta >= 0.0f && gamma >= 0.0f;
        }
    }
}
