using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    [System.Flags]
    public enum AnimationType
    {
        Pop = 1 << 0,
        Fade = 1 << 1,
        Toast = 1 << 2,
        Elevate = 1 << 3,
    }

    [RequireComponent(typeof(CanvasGroup))]
    // 애쉬엔베일에 있던 다이얼로그 간단한 애니메이션 함수 쓸만해서 가져옴
    public class UIDialogAnimaion : MonoBehaviour
    {
        [FormerlySerializedAs("openingAnimationType")] [SerializeField] private AnimationType _openingAnimationType;
        private const float OpeningAnimationDuration = 0.25f;

        [FormerlySerializedAs("closingAnimationType")] [SerializeField] private AnimationType _closingAnimationType;
        private const float ClosingAnimationDuration = 0.25f;

        public CanvasGroup _canvasGroup;
        
        [SerializeField] protected RectTransform _dialogView;

        private float _frameY;
        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        public void Init()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_dialogView)
            {
                _frameY = _dialogView.anchoredPosition.y;
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayOpening(Action onExit)
        {
            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == true)
                disableCancellation = new CancellationTokenSource();

            PlayOpeningAnimation(onExit).Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayOpeningAnimation(Action onExit)
        {
            _canvasGroup.interactable = false;

            var pop = _openingAnimationType.HasFlag(AnimationType.Pop);
            var fade = _openingAnimationType.HasFlag(AnimationType.Fade);
            var toast = _openingAnimationType.HasFlag(AnimationType.Toast);
            var elevate = _openingAnimationType.HasFlag(AnimationType.Elevate);

            var current = 0f;
            while (current < OpeningAnimationDuration)
            {
                var ratio = current / OpeningAnimationDuration;

                if (pop)
                {
                    var ease = EaseOutBack(ratio);
                    _dialogView.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, ease);
                }

                if (fade)
                {
                    _canvasGroup.alpha = ratio;
                }

                if (toast)
                {
                    var ease = EaseOutBack(ratio);
                    _dialogView.ModifyAnchoredPosition(y: _frameY - 200 * (1 - ease));
                }

                if (elevate)
                {
                    var ease = EaseOutSine(ratio);
                    _dialogView.ModifyAnchoredPosition(y: _frameY - 200 * (1 - ease));
                }

                current += Time.deltaTime;
                await UniTask.WaitForEndOfFrame(disableCancellation.Token);
            }

            if (pop)
            {
                _dialogView.localScale = Vector3.one;
            }

            if (fade)
            {
                _canvasGroup.alpha = 1f;
            }

            if (toast)
            {
                _dialogView.ModifyAnchoredPosition(y: _frameY);
            }

            if (elevate)
            {
                _dialogView.ModifyAnchoredPosition(y: _frameY);
            }

            _canvasGroup.interactable = true;
            onExit?.Invoke();
        }
        //------------------------------------------------------------------------------------
        public void PlayClosing(Action onExit)
        {
            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == true)
                disableCancellation = new CancellationTokenSource();

            PlayClosingAnimation(onExit).Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayClosingAnimation(Action onExit)
        {
            _canvasGroup.interactable = false;

            var pop = _closingAnimationType.HasFlag(AnimationType.Pop);
            var fade = _closingAnimationType.HasFlag(AnimationType.Fade);
            var toast = _closingAnimationType.HasFlag(AnimationType.Toast);
            var elevate = _closingAnimationType.HasFlag(AnimationType.Elevate);

            var current = 0f;
            while (current < ClosingAnimationDuration)
            {
                var ratio = current / ClosingAnimationDuration;

                if (pop)
                {
                    var ease = EaseInBack(ratio);
                    _dialogView.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, ease);
                }

                if (fade)
                {
                    _canvasGroup.alpha = 1 - ratio;
                }

                if (toast)
                {
                    var ease = EaseInBack(ratio);
                    _dialogView.ModifyAnchoredPosition(y: _frameY - 200 * ease);
                }

                if (elevate)
                {
                    var ease = EaseInSine(ratio);
                    _dialogView.ModifyAnchoredPosition(y: _frameY - 200 * ease);
                }

                current += Time.deltaTime;
                await UniTask.WaitForEndOfFrame(disableCancellation.Token);
            }

            if (pop)
            {
                _dialogView.localScale = Vector3.zero;
            }

            if (fade)
            {
                _canvasGroup.alpha = 0f;
            }

            if (toast)
            {
                _dialogView.ModifyAnchoredPosition(y: _frameY - 200);
            }

            if (elevate)
            {
                _dialogView.ModifyAnchoredPosition(y: _frameY - 200);
            }

            ResetAnimationValues();
            onExit?.Invoke();
        }
        //------------------------------------------------------------------------------------
        private void ResetAnimationValues()
        {
            _canvasGroup.alpha = 1f;
            if (_dialogView)
            {
                _dialogView.localScale = Vector3.one;
                _dialogView.anchoredPosition = new Vector2(_dialogView.anchoredPosition.x, _frameY);
            }
        }
        //------------------------------------------------------------------------------------
        public static float EaseInSine(float time)
        {
            return 1 - Mathf.Cos((time * Mathf.PI) / 2);
        }

        public static float EaseOutSine(float time)
        {
            return Mathf.Sin((time * Mathf.PI) / 2);
        }

        public static float EaseInBack(float time)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return c3 * time * time * time - c1 * time * time;
        }

        public static float EaseOutBack(float time)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return 1f + c3 * Mathf.Pow(time - 1f, 3) + c1 * Mathf.Pow(time - 1f, 2);
        }
    }
}