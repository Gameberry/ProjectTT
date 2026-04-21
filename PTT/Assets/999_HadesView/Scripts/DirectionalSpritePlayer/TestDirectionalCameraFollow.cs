using UnityEngine;

namespace GameBerry.TestScene
{
    [RequireComponent(typeof(Camera))]
    public class TestDirectionalCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private string _targetName = "TestDirectionalPlayer";
        [SerializeField] private Vector3 _followOffset = new Vector3(0.0f, 0.0f, -10.0f);
        [SerializeField] private Vector3 _followEulerAngles = Vector3.zero;
        [SerializeField] private float _followSmooth = 12.0f;
        [SerializeField] private bool _configureCameraOnAwake = true;
        [SerializeField] private float _orthographicSize = 5.0f;
        [SerializeField] private int _cameraDepth = 100;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            if (_configureCameraOnAwake)
            {
                _camera.orthographic = true;
                _camera.orthographicSize = _orthographicSize;
                _camera.depth = _cameraDepth;
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = new Color(0.78f, 0.86f, 0.68f, 1.0f);
            }

            if (_target == null)
            {
                GameObject targetObject = GameObject.Find(_targetName);
                if (targetObject != null)
                    _target = targetObject.transform;
            }

            transform.rotation = Quaternion.Euler(_followEulerAngles);
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            Vector3 desiredPosition = _target.position + _followOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1.0f - Mathf.Exp(-_followSmooth * Time.deltaTime));
            transform.rotation = Quaternion.Euler(_followEulerAngles);
        }

        private void SnapToTarget()
        {
            if (_target == null)
                return;

            transform.position = _target.position + _followOffset;
        }
    }
}
