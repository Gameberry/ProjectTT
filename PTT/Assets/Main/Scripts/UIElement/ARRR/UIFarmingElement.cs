using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIFarmingElement : MonoBehaviour
    {
        [SerializeField]
        private Image _farmingBG;

        [SerializeField]
        private Image _farmingIcon;

        [SerializeField]
        private TMP_Text _farmingText;

        private CanvasGroup _canvasGroup;

        FarmingData _farmingData = null;
        System.Action<FarmingData> _endCallback = null;

        [SerializeField]
        private float _directionWaitTime = 1.0f;
        private float _alphaDirectionTime = 1.0f;

        private Coroutine _directionCoroutine = null;

        private WaitForSeconds waitForSeconds = null;
        //------------------------------------------------------------------------------------
        public void Init(System.Action<FarmingData> callback)
        {
            waitForSeconds = new WaitForSeconds(_directionWaitTime);
            _canvasGroup = GetComponent<CanvasGroup>();
            _endCallback = callback;
        }
        //------------------------------------------------------------------------------------
        public void PlayDirection(FarmingData data)
        {
            if (_directionCoroutine != null)
            {
                OnDirectionEnd();
                StopCoroutine(_directionCoroutine);
            }

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1.0f;

            _farmingData = data;

            //if (_farmingBG != null)
            //    _farmingBG.color = data.RewardBGColor;

            if (_farmingIcon != null)
                _farmingIcon.sprite = _farmingData.RewardIcon;

            if (_farmingText != null)
                _farmingText.text = _farmingData.RewardCountText;

            _directionCoroutine = StartCoroutine(ElementDirection());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator ElementDirection()
        {
            if (waitForSeconds != null)
                yield return waitForSeconds;

            float starttime = Time.time;

            while (starttime + _alphaDirectionTime > Time.time)
            {
                if (_canvasGroup != null && _alphaDirectionTime > 0.0f)
                {
                    float alpha = _canvasGroup.alpha;
                    alpha = (_alphaDirectionTime - (Time.time - starttime)) / _alphaDirectionTime;
                    _canvasGroup.alpha = alpha;
                }

                yield return null;
            }

            if (_canvasGroup != null)
                _canvasGroup.alpha = 0.0f;

            _directionCoroutine = null;
            OnDirectionEnd();
        }
        //------------------------------------------------------------------------------------
        private void OnDirectionEnd()
        {
            if (_endCallback != null)
                _endCallback(_farmingData);
        }
        //------------------------------------------------------------------------------------
        public void ForceRelease()
        {
            if (_directionCoroutine != null)
            {
                if (_canvasGroup != null)
                    _canvasGroup.alpha = 0.0f;

                OnDirectionEnd();
                StopCoroutine(_directionCoroutine);
            }
        }
        //------------------------------------------------------------------------------------
    }
}