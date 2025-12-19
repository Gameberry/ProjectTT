using UnityEngine;

namespace GameBerry
{
    public class CameraShake : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] private float defaultDuration = 0.15f;
        [SerializeField] private float defaultStrength = 0.2f;
        [SerializeField] private float defaultFrequency = 25f;
        [SerializeField] private AnimationCurve dampingCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Axis Control")]
        [SerializeField] private bool shakeX = true;
        [SerializeField] private bool shakeY = true;
        [SerializeField] private bool shakeZ = false;

        private Vector3 originPos;
        private float timer;
        private float duration;
        private float strength;
        private float frequency;
        private bool isShaking;

        private void Awake()
        {
            originPos = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (!isShaking)
                return;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float damper = dampingCurve.Evaluate(t);

            float offsetX = shakeX ? Random.Range(-1f, 1f) : 0f;
            float offsetY = shakeY ? Random.Range(-1f, 1f) : 0f;
            float offsetZ = shakeZ ? Random.Range(-1f, 1f) : 0f;

            Vector3 offset = new Vector3(offsetX, offsetY, offsetZ)
                             * strength * damper;

            transform.localPosition = originPos + offset;

            if (timer >= duration)
            {
                StopShake();
            }
        }

        public void Shake(
            float strengthOverride = -1f,
            float durationOverride = -1f,
            float frequencyOverride = -1f)
        {
            strength = strengthOverride > 0 ? strengthOverride : defaultStrength;
            duration = durationOverride > 0 ? durationOverride : defaultDuration;
            frequency = frequencyOverride > 0 ? frequencyOverride : defaultFrequency;

            timer = 0f;
            isShaking = true;
        }

        public void StopShake()
        {
            isShaking = false;
            transform.localPosition = originPos;
        }
    }
}
