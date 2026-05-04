using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestRoomPortal : MonoBehaviour
    {
        [SerializeField] private float _activationRadius = 1.25f;
        [SerializeField] private bool _activeOnStart = false;
        [SerializeField] private bool _drawGizmo = true;
        [SerializeField] private GameObject _visualRoot;

        private TestDirectionalPlayerController _player;
        private bool _isPortalActive;
        private bool _wasInsideRange;
        private bool _mustExitBeforeEntering;

        public bool IsPortalActive => _isPortalActive;

        private void Awake()
        {
            SetPortalActive(_activeOnStart);
        }

        private void Update()
        {
            if (_isPortalActive == false)
                return;

            EnsurePlayer();
            if (_player == null)
                return;

            bool isInsideRange = Vector2.Distance(transform.position, _player.transform.position) <= _activationRadius;
            if (isInsideRange == false)
            {
                _wasInsideRange = false;
                _mustExitBeforeEntering = false;
                return;
            }

            if (_mustExitBeforeEntering)
            {
                _wasInsideRange = true;
                return;
            }

            if (_wasInsideRange == false)
            {
                _wasInsideRange = true;
                TestMapFlowController.Instance.NotifyPortalEntered(this);
            }
        }

        public void SetPortalActive(bool active)
        {
            _isPortalActive = active;

            if (active)
            {
                bool isInsideRange = IsPlayerInsideRange();
                _wasInsideRange = isInsideRange;
                _mustExitBeforeEntering = isInsideRange;
            }
            else
            {
                _wasInsideRange = false;
                _mustExitBeforeEntering = false;
            }

            if (_visualRoot != null)
                _visualRoot.SetActive(active);
        }

        private bool IsPlayerInsideRange()
        {
            EnsurePlayer();
            if (_player == null)
                return false;

            return Vector2.Distance(transform.position, _player.transform.position) <= _activationRadius;
        }

        private void EnsurePlayer()
        {
            if (_player != null)
                return;

            _player = FindObjectOfType<TestDirectionalPlayerController>();
        }

        private void OnDrawGizmosSelected()
        {
            if (_drawGizmo == false)
                return;

            Gizmos.color = _isPortalActive
                ? new Color(0.2f, 1.0f, 0.9f, 0.85f)
                : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _activationRadius);
        }
    }
}
