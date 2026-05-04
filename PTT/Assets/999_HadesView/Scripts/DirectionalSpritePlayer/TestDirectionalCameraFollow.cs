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

        [Header("Camera")]
        [SerializeField] private bool _configureCameraOnAwake = true;
        [SerializeField] private float _orthographicSize = 5.0f;
        [SerializeField] private int _cameraDepth = 100;

        [Header("Map Clamp")]
        [SerializeField] private SpriteRenderer _mapSpriteRenderer;

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
            desiredPosition = ClampCameraPosition(desiredPosition);

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                1.0f - Mathf.Exp(-_followSmooth * Time.deltaTime)
            );

            transform.rotation = Quaternion.Euler(_followEulerAngles);
        }

        public void SetMapSpriteRenderer(SpriteRenderer mapSpriteRenderer)
        {
            _mapSpriteRenderer = mapSpriteRenderer;
        }

        private void SnapToTarget()
        {
            if (_target == null)
                return;

            Vector3 desiredPosition = _target.position + _followOffset;
            transform.position = ClampCameraPosition(desiredPosition);
        }

        private Vector3 ClampCameraPosition(Vector3 desiredPosition)
        {
            if (_mapSpriteRenderer == null)
                return desiredPosition;

            Bounds mapBounds = _mapSpriteRenderer.bounds;

            float cameraHalfHeight = _camera.orthographicSize;
            float cameraHalfWidth = cameraHalfHeight * _camera.aspect;

            float minX = mapBounds.min.x + cameraHalfWidth;
            float maxX = mapBounds.max.x - cameraHalfWidth;
            float minY = mapBounds.min.y + cameraHalfHeight;
            float maxY = mapBounds.max.y - cameraHalfHeight;

            Vector3 clampedPosition = desiredPosition;

            if (minX > maxX)
                clampedPosition.x = mapBounds.center.x;
            else
                clampedPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

            if (minY > maxY)
                clampedPosition.y = mapBounds.center.y;
            else
                clampedPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

            return clampedPosition;
        }
    }
}