using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestDirectionalMonsterController : MonoBehaviour
    {
        [SerializeField] private TestDirectionalSpriteAnimator _spriteAnimator;
        [SerializeField] private Transform _target;
        [SerializeField] private string _targetObjectName = "TestDirectionalPlayer";
        [SerializeField] private float _moveSpeed = 2.0f;
        [SerializeField] private float _detectRange = 6.0f;
        [SerializeField] private float _attackRange = 1.25f;
        [SerializeField] private bool _drawRangeGizmos = true;

        private Vector3 _lastMoveDirection = Vector3.down;
        private bool _isAttacking;

        private void Reset()
        {
            EnsureDependencies();
        }

        private void Awake()
        {
            EnsureDependencies();
            TryFindTarget();

            if (_spriteAnimator != null)
            {
                _spriteAnimator.AutoReturnToIdleOnAttackComplete = false;
                _spriteAnimator.StatePlaybackCompleted += HandleStatePlaybackCompleted;
                _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection, true);
            }
        }

        private void OnDestroy()
        {
            if (_spriteAnimator != null)
                _spriteAnimator.StatePlaybackCompleted -= HandleStatePlaybackCompleted;
        }

        private void Update()
        {
            if (_spriteAnimator == null)
                return;

            if (_target == null)
                TryFindTarget();

            Vector3 planarToTarget = GetPlanarDirectionToTarget();
            float sqrDistanceToTarget = planarToTarget.sqrMagnitude;
            float detectRangeSqr = _detectRange * _detectRange;
            float attackRangeSqr = _attackRange * _attackRange;

            if (_isAttacking)
            {
                _spriteAnimator.SetDirection(_lastMoveDirection);
                return;
            }

            if (_target == null || sqrDistanceToTarget > detectRangeSqr)
            {
                _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection);
                return;
            }

            if (sqrDistanceToTarget <= attackRangeSqr)
            {
                StartAttack(planarToTarget);
                return;
            }

            Vector3 moveDirection = planarToTarget.normalized;
            if (moveDirection.sqrMagnitude > 0.0001f)
                _lastMoveDirection = moveDirection;

            transform.position += moveDirection * (_moveSpeed * Time.deltaTime);
            _spriteAnimator.Play(CharacterState.Run, _lastMoveDirection);
        }

        private void EnsureDependencies()
        {
            if (_spriteAnimator == null)
                _spriteAnimator = GetComponent<TestDirectionalSpriteAnimator>();

            if (_spriteAnimator == null)
                _spriteAnimator = gameObject.AddComponent<TestDirectionalSpriteAnimator>();
        }

        private void TryFindTarget()
        {
            TestDirectionalPlayerController playerController = FindObjectOfType<TestDirectionalPlayerController>();
            if (playerController != null)
            {
                _target = playerController.transform;
                return;
            }

            GameObject targetObject = GameObject.Find(_targetObjectName);
            if (targetObject != null)
                _target = targetObject.transform;
        }

        private Vector3 GetPlanarDirectionToTarget()
        {
            if (_target == null)
                return Vector3.zero;

            Vector3 planarToTarget = _target.position - transform.position;
            planarToTarget.z = 0.0f;
            return planarToTarget;
        }

        private void StartAttack(Vector3 planarToTarget)
        {
            if (planarToTarget.sqrMagnitude > 0.0001f)
                _lastMoveDirection = planarToTarget.normalized;

            _isAttacking = true;
            _spriteAnimator.Play(CharacterState.Attack, _lastMoveDirection, true);
        }

        private void HandleStatePlaybackCompleted(CharacterState completedState)
        {
            if (completedState != CharacterState.Attack)
                return;

            _isAttacking = false;

            Vector3 planarToTarget = GetPlanarDirectionToTarget();
            float sqrDistanceToTarget = planarToTarget.sqrMagnitude;
            float detectRangeSqr = _detectRange * _detectRange;
            float attackRangeSqr = _attackRange * _attackRange;

            if (_target == null || sqrDistanceToTarget > detectRangeSqr)
            {
                _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection, true);
                return;
            }

            if (sqrDistanceToTarget > attackRangeSqr)
            {
                Vector3 moveDirection = planarToTarget.sqrMagnitude > 0.0001f ? planarToTarget.normalized : _lastMoveDirection;
                _lastMoveDirection = moveDirection;
                _spriteAnimator.Play(CharacterState.Run, moveDirection, true);
                return;
            }

            StartAttack(planarToTarget);
        }

        private void OnDrawGizmosSelected()
        {
            if (_drawRangeGizmos == false)
                return;

            Gizmos.color = new Color(0.2f, 0.8f, 1.0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _detectRange);

            Gizmos.color = new Color(1.0f, 0.35f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
