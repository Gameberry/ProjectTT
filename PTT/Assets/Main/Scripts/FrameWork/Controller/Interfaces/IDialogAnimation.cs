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
            new Keyframe[2] { new Keyframe(0.0f, 0.0f, 0.5f, 0.5f), new Keyframe(1.0f, 1.0f, 0.5f, 0.5f) });
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
            if (ani == null)
                return 0.0f;

            if (ani.UseAnimation == false)
                return 0.0f;

            return ani.StartDelay + ani.Duration;
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class IDialogAnimation : MonoBehaviour
    {
        [HideInInspector]
        public Transform AnimationTarget;

        [HideInInspector]
        public bool useInAnimation;
        [HideInInspector]
        public bool useOutAnimation;

        public bool IsDoingInAnimation { get { return _doingInAnimation; } }
        private bool _doingInAnimation;

        public bool IsDoingOutAnimation { get { return _doingOutAnimation; } }
        private bool _doingOutAnimation;

        [HideInInspector]
        public IDialogAnimations InAnimation = new IDialogAnimations();
        [HideInInspector]
        public IDialogAnimations OutAnimation = new IDialogAnimations();

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
            _rectTransform = AnimationTarget == null ? GetComponent<RectTransform>() : AnimationTarget.GetComponent<RectTransform>();
            _startPos = _rectTransform.anchoredPosition3D;
            _startRotate = _rectTransform.eulerAngles;
            _startScale = _rectTransform.localScale;
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

            if (useInAnimation)
                InAnimation.SetTotalDuration();

            if (InAnimation.TotalDuration <= 0.0f)
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

            // 각 애니메이션 병렬 실행
            if (InAnimation.MoveAni.UseAnimation)
                _ = PlayMoveAsync(InAnimation.MoveAni.StartDelay,
                    InAnimation.MoveAni.Duration,
                    InAnimation.MoveAni.GetTargetPosition(_rectTransform, _startPos),
                    _startPos,
                    InAnimation.MoveAni.Linear ? null : InAnimation.MoveAni.AnimationCurve,
                    token);

            if (InAnimation.RotateAni.UseAnimation)
                _ = PlayRotateAsync(InAnimation.RotateAni.StartDelay,
                    InAnimation.RotateAni.Duration,
                    InAnimation.RotateAni.Rotate,
                    _startRotate,
                    InAnimation.RotateAni.Linear ? null : InAnimation.RotateAni.AnimationCurve,
                    token);

            if (InAnimation.ScaleAni.UseAnimation)
                _ = PlayScaleAsync(InAnimation.ScaleAni.StartDelay,
                    InAnimation.ScaleAni.Duration,
                    InAnimation.ScaleAni.Scale,
                    _startScale,
                    InAnimation.ScaleAni.Linear ? null : InAnimation.ScaleAni.AnimationCurve,
                    token);

            if (InAnimation.FadeAni.UseAnimation)
                _ = PlayFadeAsync(InAnimation.FadeAni.StartDelay,
                    InAnimation.FadeAni.Duration,
                    InAnimation.FadeAni.StartAlpha,
                    InAnimation.FadeAni.EndAlpha,
                    InAnimation.FadeAni.Linear ? null : InAnimation.FadeAni.AnimationCurve,
                    token);

            try
            {
                // 총 시간만큼 기다렸다가 끝 이벤트 호출 (기존 Update 로직 대체)
                int waitMs = (int)(InAnimation.TotalDuration * 1000f);
                await UniTask.Delay(waitMs, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                // 다른 애니메이션으로 교체된 경우
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

            if (useOutAnimation)
                OutAnimation.SetTotalDuration();

            if (OutAnimation.TotalDuration <= 0.0f)
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

            if (OutAnimation.MoveAni.UseAnimation)
                _ = PlayMoveAsync(OutAnimation.MoveAni.StartDelay,
                    OutAnimation.MoveAni.Duration,
                    _startPos,
                    OutAnimation.MoveAni.GetTargetPosition(_rectTransform, _startPos),
                    OutAnimation.MoveAni.Linear ? null : OutAnimation.MoveAni.AnimationCurve,
                    token);

            if (OutAnimation.RotateAni.UseAnimation)
                _ = PlayRotateAsync(OutAnimation.RotateAni.StartDelay,
                    OutAnimation.RotateAni.Duration,
                    _startRotate,
                    OutAnimation.RotateAni.Rotate,
                    OutAnimation.RotateAni.Linear ? null : OutAnimation.RotateAni.AnimationCurve,
                    token);

            if (OutAnimation.ScaleAni.UseAnimation)
                _ = PlayScaleAsync(OutAnimation.ScaleAni.StartDelay,
                    OutAnimation.ScaleAni.Duration,
                    _startScale,
                    OutAnimation.ScaleAni.Scale,
                    OutAnimation.ScaleAni.Linear ? null : OutAnimation.ScaleAni.AnimationCurve,
                    token);

            if (OutAnimation.FadeAni.UseAnimation)
                _ = PlayFadeAsync(OutAnimation.FadeAni.StartDelay,
                    OutAnimation.FadeAni.Duration,
                    OutAnimation.FadeAni.StartAlpha,
                    OutAnimation.FadeAni.EndAlpha,
                    OutAnimation.FadeAni.Linear ? null : OutAnimation.FadeAni.AnimationCurve,
                    token);

            try
            {
                int waitMs = (int)(OutAnimation.TotalDuration * 1000f);
                await UniTask.Delay(waitMs, cancellationToken: token);
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
        private async UniTask PlayMoveAsync(float delay, float duration, Vector3 startpos, Vector3 endpos,
            AnimationCurve animationcurve, CancellationToken token)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            float starttime = Time.time;
            float endtime = starttime + duration;

            Vector3 posGap = startpos - endpos;

            while (Time.time <= endtime)
            {
                if (token.IsCancellationRequested)
                    return;

                float ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _rectTransform.anchoredPosition3D = startpos - (posGap * ratio);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _rectTransform.anchoredPosition3D = endpos;
        }

        //------------------------------------------------------------------------------------
        private async UniTask PlayRotateAsync(float delay, float duration, Vector3 startrotate, Vector3 endrotate,
            AnimationCurve animationcurve, CancellationToken token)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            float starttime = Time.time;
            float endtime = starttime + duration;

            Vector3 rotateGap = startrotate - endrotate;

            while (Time.time <= endtime)
            {
                if (token.IsCancellationRequested)
                    return;

                float ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _rectTransform.eulerAngles = startrotate - (rotateGap * ratio);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _rectTransform.eulerAngles = endrotate;
        }

        //------------------------------------------------------------------------------------
        private async UniTask PlayScaleAsync(float delay, float duration, Vector3 startscale, Vector3 endscale,
            AnimationCurve animationcurve, CancellationToken token)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            float starttime = Time.time;
            float endtime = starttime + duration;

            Vector3 scaleGap = startscale - endscale;

            while (Time.time <= endtime)
            {
                if (token.IsCancellationRequested)
                    return;

                float ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _rectTransform.localScale = startscale - (scaleGap * ratio);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _rectTransform.localScale = endscale;
        }

        //------------------------------------------------------------------------------------
        private async UniTask PlayFadeAsync(float delay, float duration, float startfade, float endfade,
            AnimationCurve animationcurve, CancellationToken token)
        {
            _canvasGroup.alpha = startfade;

            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            float starttime = Time.time;
            float endtime = starttime + duration;

            float fadeGap = endfade - startfade;

            while (Time.time <= endtime)
            {
                if (token.IsCancellationRequested)
                    return;

                float ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _canvasGroup.alpha = startfade + (fadeGap * ratio);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _canvasGroup.alpha = endfade;
        }
        //------------------------------------------------------------------------------------
    }
}
