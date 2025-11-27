using System.Collections;
using UnityEngine;

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
        private float _endTime_InAnimation;

        public bool IsDoingOutAnimation { get { return _doingOutAnimation; } }
        private bool _doingOutAnimation;
        private float _endTime_OutAnimation;

        [HideInInspector]
        public IDialogAnimations _InAnimation = new IDialogAnimations();
        [HideInInspector]
        public IDialogAnimations _OutAnimation = new IDialogAnimations();

        private RectTransform _rectTransform;

        private Vector3 _startPos;
        private Vector3 _startRotate;
        private Vector3 _startScale;
        private CanvasGroup _canvasGroup;

        private Coroutine _moveCoroutine;
        private Coroutine _rotateCoroutine;
        private Coroutine _scaleCoroutine;
        private Coroutine _fadeCoroutine;

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
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (_doingInAnimation == true)
            {
                if (_endTime_InAnimation <= Time.time)
                {
                    if (OnInAnimationsFinish != null)
                        OnInAnimationsFinish.Invoke();

                    _doingInAnimation = false;
                }
            }

            if (_doingOutAnimation == true)
            {
                if (_endTime_OutAnimation <= Time.time)
                {
                    if (OnOutAnimationsFinish != null)
                        OnOutAnimationsFinish.Invoke();

                    _doingOutAnimation = false;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void StopCoroutine_All()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            if (_rotateCoroutine != null)
            {
                StopCoroutine(_rotateCoroutine);
                _rotateCoroutine = null;
            }

            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
                _scaleCoroutine = null;
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayInAnimation()
        {
            StopCoroutine_All();

            if (_doingOutAnimation == true)
            {
                _doingOutAnimation = false;

                if (OnOutAnimationsFinish != null)
                    OnOutAnimationsFinish.Invoke();
            }

            if (OnInAnimationsStart != null)
                OnInAnimationsStart.Invoke();

            if (useInAnimation == true)
                _InAnimation.SetTotalDuration();

            if (_InAnimation.TotalDuration == 0.0f)
            {
                if (OnInAnimationsFinish != null)
                    OnInAnimationsFinish.Invoke();

                return;
            }
            else
            {
                _endTime_InAnimation = Time.time + _InAnimation.TotalDuration;
                _doingInAnimation = true;
            }

            if (gameObject.activeInHierarchy == false)
                return;

            if (_InAnimation.MoveAni.UseAnimation)
                _moveCoroutine = StartCoroutine(PlayMove(_InAnimation.MoveAni.StartDelay,
                    _InAnimation.MoveAni.Duration,
                    _InAnimation.MoveAni.GetTargetPosition(_rectTransform, _startPos),
                    _startPos,
                    _InAnimation.MoveAni.Linear == true ? null : _InAnimation.MoveAni.AnimationCurve));

            if (_InAnimation.RotateAni.UseAnimation)
                _rotateCoroutine = StartCoroutine(PlayRotate(_InAnimation.RotateAni.StartDelay,
                    _InAnimation.RotateAni.Duration,
                    _InAnimation.RotateAni.Rotate,
                    _startRotate,
                    _InAnimation.RotateAni.Linear == true ? null : _InAnimation.RotateAni.AnimationCurve));

            if (_InAnimation.ScaleAni.UseAnimation)
                _scaleCoroutine = StartCoroutine(PlayScale(_InAnimation.ScaleAni.StartDelay,
                    _InAnimation.ScaleAni.Duration,
                    _InAnimation.ScaleAni.Scale,
                    _startScale,
                    _InAnimation.ScaleAni.Linear == true ? null : _InAnimation.ScaleAni.AnimationCurve));

            if (_InAnimation.FadeAni.UseAnimation)
                _fadeCoroutine = StartCoroutine(PlayFade(_InAnimation.FadeAni.StartDelay,
                    _InAnimation.FadeAni.Duration,
                    _InAnimation.FadeAni.StartAlpha,
                    _InAnimation.FadeAni.EndAlpha,
                    _InAnimation.FadeAni.Linear == true ? null : _InAnimation.FadeAni.AnimationCurve));
        }
        //------------------------------------------------------------------------------------
        public void PlayOutAnimation()
        {
            StopCoroutine_All();

            if (_doingInAnimation == true)
            {
                _doingInAnimation = false;

                if (OnInAnimationsFinish != null)
                    OnInAnimationsFinish.Invoke();
            }

            if (OnOutAnimationsStart != null)
                OnOutAnimationsStart.Invoke();

            if (useOutAnimation == true)
                _OutAnimation.SetTotalDuration();

            if (_OutAnimation.TotalDuration == 0.0f)
            {
                if (OnOutAnimationsFinish != null)
                    OnOutAnimationsFinish.Invoke();

                return;
            }
            else
            {
                _endTime_OutAnimation = Time.time + _OutAnimation.TotalDuration;
                _doingOutAnimation = true;
            }

            if (gameObject.activeInHierarchy == false)
                return;

            if (_OutAnimation.MoveAni.UseAnimation)
                _moveCoroutine = StartCoroutine(PlayMove(_OutAnimation.MoveAni.StartDelay,
                    _OutAnimation.MoveAni.Duration,
                    _startPos,
                    _OutAnimation.MoveAni.GetTargetPosition(_rectTransform, _startPos),
                    _OutAnimation.MoveAni.Linear == true ? null : _OutAnimation.MoveAni.AnimationCurve));

            if (_OutAnimation.RotateAni.UseAnimation)
                _rotateCoroutine = StartCoroutine(PlayRotate(_OutAnimation.RotateAni.StartDelay,
                    _OutAnimation.RotateAni.Duration,
                    _startRotate,
                    _OutAnimation.RotateAni.Rotate,
                    _OutAnimation.RotateAni.Linear == true ? null : _OutAnimation.RotateAni.AnimationCurve));

            if (_OutAnimation.ScaleAni.UseAnimation)
                _scaleCoroutine = StartCoroutine(PlayScale(_OutAnimation.ScaleAni.StartDelay,
                    _OutAnimation.ScaleAni.Duration,
                    _startScale,
                    _OutAnimation.ScaleAni.Scale,
                    _OutAnimation.ScaleAni.Linear == true ? null : _OutAnimation.ScaleAni.AnimationCurve));

            if (_OutAnimation.FadeAni.UseAnimation)
                _fadeCoroutine = StartCoroutine(PlayFade(_OutAnimation.FadeAni.StartDelay,
                    _OutAnimation.FadeAni.Duration,
                    _OutAnimation.FadeAni.StartAlpha,
                    _OutAnimation.FadeAni.EndAlpha,
                    _OutAnimation.FadeAni.Linear == true ? null : _OutAnimation.FadeAni.AnimationCurve));
        }
        //------------------------------------------------------------------------------------
        IEnumerator PlayMove(float delay, float duration, Vector3 startpos, Vector3 endpos, AnimationCurve animationcurve)
        {
            float starttime = Time.time;
            float endtime = starttime + delay;

            while (Time.time <= endtime)
                yield return null;

            starttime = Time.time;
            endtime = starttime + duration;

            Vector3 posGap = startpos - endpos;

            float ratio = 0.0f;
            while (Time.time <= endtime)
            {
                ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _rectTransform.anchoredPosition3D = startpos - (posGap * ratio);

                yield return null;
            }

            _rectTransform.anchoredPosition3D = endpos;
            _moveCoroutine = null;
        }
        //------------------------------------------------------------------------------------
        IEnumerator PlayRotate(float delay, float duration, Vector3 startrotate, Vector3 endrotate, AnimationCurve animationcurve)
        {
            float starttime = Time.time;
            float endtime = starttime + delay;

            while (Time.time <= endtime)
                yield return null;

            starttime = Time.time;
            endtime = starttime + duration;

            Vector3 rotateGap = startrotate - endrotate;

            float ratio = 0.0f;
            while (Time.time <= endtime)
            {
                ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _rectTransform.eulerAngles = startrotate - (rotateGap * ratio);

                yield return null;
            }

            _rectTransform.eulerAngles = endrotate;
            _rotateCoroutine = null;
        }
        //------------------------------------------------------------------------------------
        IEnumerator PlayScale(float delay, float duration, Vector3 startscale, Vector3 endscale, AnimationCurve animationcurve)
        {
            float starttime = Time.time;
            float endtime = starttime + delay;

            while (Time.time <= endtime)
                yield return null;

            starttime = Time.time;
            endtime = starttime + duration;

            Vector3 scaleGap = startscale - endscale;

            float ratio = 0.0f;
            while (Time.time <= endtime)
            {
                ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _rectTransform.localScale = startscale - (scaleGap * ratio);

                yield return null;
            }

            _rectTransform.localScale = endscale;
            _scaleCoroutine = null;
        }
        //------------------------------------------------------------------------------------
        IEnumerator PlayFade(float delay, float duration, float startfade, float endfade, AnimationCurve animationcurve)
        {
            _canvasGroup.alpha = startfade;

            float starttime = Time.time;
            float endtime = starttime + delay;

            while (Time.time <= endtime)
                yield return null;

            starttime = Time.time;
            endtime = starttime + duration;

            float fadeGap = endfade - startfade;

            float ratio = 0.0f;
            while (Time.time <= endtime)
            {
                ratio = (Time.time - starttime) / duration;
                if (animationcurve != null)
                    ratio = animationcurve.Evaluate(ratio);

                _canvasGroup.alpha = startfade + (fadeGap * ratio);

                yield return null;
            }

            _canvasGroup.alpha = endfade;
            _fadeCoroutine = null;
        }
        //------------------------------------------------------------------------------------
    }
}