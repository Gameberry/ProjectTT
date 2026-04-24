using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestLobbyPortal : MonoBehaviour
    {
        [SerializeField] private float _activationRadius = 1.25f;
        [SerializeField] private bool _autoOpenOnEnter = true;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;
        [SerializeField] private bool _drawGizmo = true;

        private TestDirectionalPlayerController _player;
        private bool _wasInsideRange;

        private void Update()
        {
            EnsurePlayer();
            if (_player == null)
                return;

            bool isInsideRange = Vector2.Distance(transform.position, _player.transform.position) <= _activationRadius;
            if (isInsideRange == false)
            {
                _wasInsideRange = false;
                return;
            }

            if (_autoOpenOnEnter && _wasInsideRange == false)
            {
                _wasInsideRange = true;
                TestMapFlowController.Instance.OpenMapSelection();
                return;
            }

            _wasInsideRange = true;
            if (Input.GetKeyDown(_interactKey))
                TestMapFlowController.Instance.OpenMapSelection();
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

            Gizmos.color = new Color(0.3f, 0.9f, 1.0f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, _activationRadius);
        }
    }
}
