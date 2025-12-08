using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    [System.Serializable]
    public class BaseAnimationStruct
    {
        public bool UseAnimation;

        public float StartDelay;
        public float Duration = 0.25f;

        public bool Linear = true;
        public AnimationCurve AnimationCurve = new AnimationCurve(
            new Keyframe[2]
            {
                new Keyframe(0.0f, 0.0f, 0.5f, 0.5f),
                new Keyframe(1.0f, 1.0f, 0.5f, 0.5f)
            });
    }

    [System.Serializable]
    public class MoveAniStruct : BaseAnimationStruct
    {
        public enum MoveDirection
        {
            Left = 0,
            Right = 1,
            Top = 2,
            Bottom = 3,
            TopLeft = 4,
            TopCenter = 5,
            TopRight = 6,
            MiddleLeft = 7,
            MiddleCenter = 8,
            MiddleRight = 9,
            BottomLeft = 10,
            BottomCenter = 11,
            BottomRight = 12,
            CustomPosition = 13
        }

        public MoveDirection MoveFrom = MoveDirection.Left;
        public Vector3 CustomPosition = Vector3.zero;

        public Vector3 GetTargetPosition(RectTransform target, Vector3 startPosition)
        {
            Rect rootCanvasRect = target.rect;
            float xOffset = rootCanvasRect.width / 2 + target.rect.width * target.pivot.x;
            float yOffset = rootCanvasRect.height / 2 + target.rect.height * target.pivot.y;

            switch (MoveFrom)
            {
                case MoveDirection.Left: return new Vector3(-xOffset, startPosition.y, startPosition.z);
                case MoveDirection.Right: return new Vector3(xOffset, startPosition.y, startPosition.z);
                case MoveDirection.Top: return new Vector3(startPosition.x, yOffset, startPosition.z);
                case MoveDirection.Bottom: return new Vector3(startPosition.x, -yOffset, startPosition.z);
                case MoveDirection.TopLeft: return new Vector3(-xOffset, yOffset, startPosition.z);
                case MoveDirection.TopCenter: return new Vector3(0, yOffset, startPosition.z);
                case MoveDirection.TopRight: return new Vector3(xOffset, yOffset, startPosition.z);
                case MoveDirection.MiddleLeft: return new Vector3(-xOffset, 0, startPosition.z);
                case MoveDirection.MiddleCenter: return new Vector3(0, 0, startPosition.z);
                case MoveDirection.MiddleRight: return new Vector3(xOffset, 0, startPosition.z);
                case MoveDirection.BottomLeft: return new Vector3(-xOffset, -yOffset, startPosition.z);
                case MoveDirection.BottomCenter: return new Vector3(0, -yOffset, startPosition.z);
                case MoveDirection.BottomRight: return new Vector3(xOffset, -yOffset, startPosition.z);
                case MoveDirection.CustomPosition: return CustomPosition;
                default: return Vector3.zero;
            }
        }
    }

    [System.Serializable]
    public class RotateAniStruct : BaseAnimationStruct
    {
        public Vector3 Rotate = Vector3.zero;
    }

    [System.Serializable]
    public class ScaleAniStruct : BaseAnimationStruct
    {
        public Vector3 Scale = Vector3.one;
    }

    [System.Serializable]
    public class FadeAniStruct : BaseAnimationStruct
    {
        public float StartAlpha;
        public float EndAlpha;
    }

    [System.Serializable]
    public class IDialogAnimations
    {
        public MoveAniStruct MoveAni = new MoveAniStruct();
        public RotateAniStruct RotateAni = new RotateAniStruct();
        public ScaleAniStruct ScaleAni = new ScaleAniStruct();
        public FadeAniStruct FadeAni = new FadeAniStruct();

        public float TotalDuration { get; private set; }

        public void SetTotalDuration()
        {
            TotalDuration = GetTotalAnimationDuration();
        }

        private float GetTotalAnimationDuration()
        {
            float totaltime = 0.0f;

            if (totaltime < GetTotalDuration(MoveAni))
                totaltime = GetTotalDuration(MoveAni);

            if (totaltime < GetTotalDuration(RotateAni))
                totaltime = GetTotalDuration(RotateAni);

            if (totaltime < GetTotalDuration(ScaleAni))
                totaltime = GetTotalDuration(ScaleAni);

            if (totaltime < GetTotalDuration(FadeAni))
                totaltime = GetTotalDuration(FadeAni);

            return totaltime;
        }

        private float GetTotalDuration(BaseAnimationStruct ani)
        {
            if (ani == null || ani.UseAnimation == false)
                return 0.0f;

            return ani.StartDelay + ani.Duration;
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class IDialogAnimation : MonoBehaviour
    {
        [HideInInspector] public Transform AnimationTarget;

        [HideInInspector] public bool useInAnimation;
        [HideInInspector] public bool useOutAnimation;

        public bool IsDoingInAnimation { get { return _doingInAnimation; } }
        public bool IsDoingOutAnimation { get { return _doingOutAnimation; } }

        private bool _doingInAnimation;
        private bool _doingOutAnimation;

        [HideInInspector] public IDialogAnimations InAnimation = new IDialogAnimations();
        [HideInInspector] public IDialogAnimations OutAnimation = new IDialogAnimations();

        private RectTransform _rectTransform;
        private Vector3 _startPos;
        private Vector3 _startRotate;
        private Vector3 _startScale;
        private CanvasGroup _canvasGroup;

        // UniTask용 취소 토큰
        private CancellationTokenSource _animationCts;

        public UnityEngine.Events.UnityEvent OnInAnimationsStart;
        public UnityEngine.Events.UnityEvent OnInAnimationsFinish;
        public UnityEngine.Events.UnityEvent OnOutAnimationsStart;
        public UnityEngine.Events.UnityEvent OnOutAnimationsFinish;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            // 에디터/런타임 둘 다에서 활성화 시점 기준으로 기준값 갱신
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (AnimationTarget == null)
                _rectTransform = GetComponent<RectTransform>();
            else
                _rectTransform = AnimationTarget.GetComponent<RectTransform>();

            if (_rectTransform != null)
            {
                _startPos = _rectTransform.anchoredPosition3D;
                _startRotate = _rectTransform.eulerAngles;
                _startScale = _rectTransform.localScale;
            }

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            CancelAnimations();
        }

        //------------------------------------------------------------------------------------
        private void CancelAnimations()
        {
            if (_animationCts != null)
            {
                _animationCts.Cancel();
                _animationCts.Dispose();
                _animationCts = null;
            }
        }

        private CancellationToken CreateNewToken()
        {
            CancelAnimations();
            _animationCts = new CancellationTokenSource();
            return _animationCts.Token;
        }

        //------------------------------------------------------------------------------------
        // 기존 시그니처 유지용(외부에서 그대로 호출 가능)
        public void PlayInAnimation()
        {
            PlayInAnimationAsync().Forget();
        }

        public void PlayOutAnimation()
        {
            PlayOutAnimationAsync().Forget();
        }

        //------------------------------------------------------------------------------------
        public async UniTask PlayInAnimationAsync()
        {
            var token = CreateNewToken();

            // Out 애니메이션 도중에 In 이 호출되면 정리
            if (_doingOutAnimation)
            {
                _doingOutAnimation = false;
                OnOutAnimationsFinish?.Invoke();
            }

            OnInAnimationsStart?.Invoke();

            if (!useInAnimation)
            {
                OnInAnimationsFinish?.Invoke();
                return;
            }

            _doingInAnimation = true;

            if (!gameObject.activeInHierarchy)
            {
                _doingInAnimation = false;
                return;
            }

            try
            {
                await RunAnimationsAsync(InAnimation, isIn: true, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested)
                return;

            _doingInAnimation = false;
            OnInAnimationsFinish?.Invoke();
        }

        //------------------------------------------------------------------------------------
        public async UniTask PlayOutAnimationAsync()
        {
            var token = CreateNewToken();

            if (_doingInAnimation)
            {
                _doingInAnimation = false;
                OnInAnimationsFinish?.Invoke();
            }

            OnOutAnimationsStart?.Invoke();

            if (!useOutAnimation)
            {
                OnOutAnimationsFinish?.Invoke();
                return;
            }

            _doingOutAnimation = true;

            if (!gameObject.activeInHierarchy)
            {
                _doingOutAnimation = false;
                return;
            }

            try
            {
                await RunAnimationsAsync(OutAnimation, isIn: false, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested)
                return;

            _doingOutAnimation = false;
            OnOutAnimationsFinish?.Invoke();
        }

        //------------------------------------------------------------------------------------
        /// <summary>
        /// Move/Rotate/Scale/Fade를 한 번에 처리하는 메인 애니메이션 루프
        /// </summary>
        private async UniTask RunAnimationsAsync(IDialogAnimations anim, bool isIn, CancellationToken token)
        {
            if (_rectTransform == null || _canvasGroup == null)
                CacheComponents();

            anim.SetTotalDuration();
            float total = anim.TotalDuration;
            if (total <= 0f)
                return;

            float startTime = Time.time;

            // -------- Move 사전 계산 --------
            Vector3 moveFrom = _startPos;
            Vector3 moveTo = _startPos;
            bool useMove = anim.MoveAni != null && anim.MoveAni.UseAnimation;

            if (useMove)
            {
                var offPos = anim.MoveAni.GetTargetPosition(_rectTransform, _startPos);
                if (isIn)
                {
                    // In : off → start
                    moveFrom = offPos;
                    moveTo = _startPos;
                }
                else
                {
                    // Out : start → off
                    moveFrom = _startPos;
                    moveTo = offPos;
                }
            }

            // -------- Rotate 사전 계산 --------
            Vector3 rotFrom = _startRotate;
            Vector3 rotTo = _startRotate;
            bool useRotate = anim.RotateAni != null && anim.RotateAni.UseAnimation;

            if (useRotate)
            {
                if (isIn)
                {
                    rotFrom = anim.RotateAni.Rotate;
                    rotTo = _startRotate;
                }
                else
                {
                    rotFrom = _startRotate;
                    rotTo = anim.RotateAni.Rotate;
                }
            }

            // -------- Scale 사전 계산 --------
            Vector3 scaleFrom = _startScale;
            Vector3 scaleTo = _startScale;
            bool useScale = anim.ScaleAni != null && anim.ScaleAni.UseAnimation;

            if (useScale)
            {
                if (isIn)
                {
                    scaleFrom = anim.ScaleAni.Scale;
                    scaleTo = _startScale;
                }
                else
                {
                    scaleFrom = _startScale;
                    scaleTo = anim.ScaleAni.Scale;
                }
            }

            // -------- Fade 사전 계산 --------
            bool useFade = anim.FadeAni != null && anim.FadeAni.UseAnimation;
            float fadeFrom = _canvasGroup != null ? _canvasGroup.alpha : 1f;
            float fadeTo = fadeFrom;

            if (useFade)
            {
                fadeFrom = anim.FadeAni.StartAlpha;
                fadeTo = anim.FadeAni.EndAlpha;

                if (_canvasGroup != null)
                    _canvasGroup.alpha = fadeFrom;
            }

            // -------- 메인 루프 --------
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                float elapsed = Time.time - startTime;
                float t = Mathf.Clamp(elapsed, 0f, total);

                // Move
                if (useMove)
                {
                    float r = GetAnimRatio(anim.MoveAni, t);
                    _rectTransform.anchoredPosition3D = Vector3.Lerp(moveFrom, moveTo, r);
                }

                // Rotate
                if (useRotate)
                {
                    float r = GetAnimRatio(anim.RotateAni, t);
                    _rectTransform.eulerAngles = Vector3.Lerp(rotFrom, rotTo, r);
                }

                // Scale
                if (useScale)
                {
                    float r = GetAnimRatio(anim.ScaleAni, t);
                    _rectTransform.localScale = Vector3.Lerp(scaleFrom, scaleTo, r);
                }

                // Fade
                if (useFade && _canvasGroup != null)
                {
                    float r = GetAnimRatio(anim.FadeAni, t);
                    _canvasGroup.alpha = Mathf.Lerp(fadeFrom, fadeTo, r);
                }

                if (elapsed >= total)
                    break;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            // 루프가 끝난 후, 정확히 최종 값 스냅
            if (useMove)
                _rectTransform.anchoredPosition3D = moveTo;

            if (useRotate)
                _rectTransform.eulerAngles = rotTo;

            if (useScale)
                _rectTransform.localScale = scaleTo;

            if (useFade && _canvasGroup != null)
                _canvasGroup.alpha = fadeTo;
        }

        //------------------------------------------------------------------------------------
        private float GetAnimRatio(BaseAnimationStruct ani, float time)
        {
            if (ani == null || !ani.UseAnimation)
                return 0f;

            float start = ani.StartDelay;
            float end = ani.StartDelay + ani.Duration;

            if (time <= start)
                return 0f;
            if (time >= end)
                return 1f;

            float t = (time - start) / ani.Duration;
            if (!ani.Linear && ani.AnimationCurve != null)
                t = ani.AnimationCurve.Evaluate(t);

            return Mathf.Clamp01(t);
        }
        //------------------------------------------------------------------------------------
    }
}
