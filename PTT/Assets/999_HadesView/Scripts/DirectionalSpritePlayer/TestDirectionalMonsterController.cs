using UnityEngine;
using System.Collections.Generic;

namespace GameBerry.TestScene
{
    public class TestDirectionalMonsterController : MonoBehaviour
    {
        private static readonly List<TestDirectionalMonsterController> QueryBuffer = new List<TestDirectionalMonsterController>(32);
        private static readonly List<TestDirectionalMonsterController> AttackQueryBuffer = new List<TestDirectionalMonsterController>(32);
        private static readonly Collider2D[] WallBuffer = new Collider2D[16];

        [SerializeField] private TestDirectionalSpriteAnimator _spriteAnimator;
        [SerializeField] private UICharacterState _hpBar;
        [SerializeField] private Transform _target;
        [SerializeField] private string _targetObjectName = "TestDirectionalPlayer";
        [SerializeField] private float _moveSpeed = 2.0f;
        [SerializeField] private float _detectRange = 6.0f;
        [SerializeField] private float _attackRange = 1.25f;
        [SerializeField] private float _bodyRadius = 0.35f;
        [SerializeField] private int _maxHp = 30;
        [SerializeField] private int _attackDamage = 5;
        [SerializeField] private float _attackAngle = 90.0f;
        [SerializeField] private LayerMask _wallLayerMask;
        [SerializeField] private bool _drawRangeGizmos = true;
        [SerializeField] private bool _drawAttackGizmo = true;

        private Vector3 _lastMoveDirection = Vector3.down;
        private bool _isAttacking;
        [SerializeField]
        private int _currentHp;
        private bool _isDead;

        public float BodyRadius => _bodyRadius;

        private void OnEnable()
        {
            TestDirectionalMonsterManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (TestDirectionalMonsterManager.HasInstance)
                TestDirectionalMonsterManager.Instance.Unregister(this);
        }

        private void Reset()
        {
            EnsureDependencies();
        }

        private void Awake()
        {
            EnsureDependencies();
            TryFindTarget();
            _currentHp = _maxHp;

            if (_spriteAnimator != null)
            {
                _spriteAnimator.AutoReturnToIdleOnAttackComplete = false;
                _spriteAnimator.StatePlaybackCompleted += HandleStatePlaybackCompleted;
                _spriteAnimator.StateFrameTriggered += HandleStateFrameTriggered;
                //RefreshHpBar();
                _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection, true);
            }
        }

        private void OnDestroy()
        {
            if (_spriteAnimator != null)
            {
                _spriteAnimator.StatePlaybackCompleted -= HandleStatePlaybackCompleted;
                _spriteAnimator.StateFrameTriggered -= HandleStateFrameTriggered;
            }
        }

        private void Update()
        {
            if (_spriteAnimator == null || _isDead)
                return;

            if (_target == null)
                TryFindTarget();

            Vector3 planarToTarget = GetPlanarDirectionToTarget();
            float sqrDistanceToTarget = planarToTarget.sqrMagnitude;
            float detectRangeSqr = _detectRange * _detectRange;
            float attackRangeSqr = _attackRange * _attackRange;

            if (_isAttacking)
            {
                ResolveBodyOverlaps();
                _spriteAnimator.SetDirection(_lastMoveDirection);
                return;
            }

            if (_target == null || sqrDistanceToTarget > detectRangeSqr)
            {
                ResolveBodyOverlaps();
                _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection);
                return;
            }

            if (sqrDistanceToTarget <= attackRangeSqr)
            {
                StartAttack(planarToTarget);
                ResolveBodyOverlaps();
                return;
            }

            Vector3 moveDirection = planarToTarget.normalized;
            if (moveDirection.sqrMagnitude > 0.0001f)
                _lastMoveDirection = moveDirection;

            transform.position += moveDirection * (_moveSpeed * Time.deltaTime);
            ResolveBodyOverlaps();
            _spriteAnimator.Play(CharacterState.Run, _lastMoveDirection);
        }

        private void EnsureDependencies()
        {
            if (_spriteAnimator == null)
                _spriteAnimator = GetComponent<TestDirectionalSpriteAnimator>();

            if (_spriteAnimator == null)
                _spriteAnimator = gameObject.AddComponent<TestDirectionalSpriteAnimator>();

            if (_hpBar == null)
                _hpBar = GetComponent<UICharacterState>();

            if (_hpBar == null)
                _hpBar = gameObject.AddComponent<UICharacterState>();
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

        private void ResolveBodyOverlaps()
        {
            Vector3 resolvedPosition = transform.position;
            resolvedPosition = ResolveOverlapWithMonsters(resolvedPosition);
            resolvedPosition = ResolveWallOverlaps(resolvedPosition);
            transform.position = resolvedPosition;
        }

        private Vector3 ResolveWallOverlaps(Vector3 position)
        {
            Vector2 pos2D = (Vector2)position;
            var filter = new ContactFilter2D { layerMask = _wallLayerMask, useLayerMask = true, useTriggers = false };
            int count = Physics2D.OverlapCircle(pos2D, _bodyRadius, filter, WallBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D wall = WallBuffer[i];
                if (wall == null)
                    continue;

                Vector2 closest = wall.ClosestPoint(pos2D);
                Vector2 delta = pos2D - closest;
                float distance = delta.magnitude;

                if (distance >= _bodyRadius)
                    continue;

                Vector2 pushDir = distance > 0.0001f ? delta / distance : Vector2.up;
                pos2D += pushDir * (_bodyRadius - distance);
            }

            return new Vector3(pos2D.x, pos2D.y, position.z);
        }

        private Vector3 ResolveOverlapWithMonsters(Vector3 currentPosition)
        {
            float queryRadius = _bodyRadius * 2.5f;
            TestDirectionalMonsterManager.Instance.QueryMonsters(new Vector2(currentPosition.x, currentPosition.y), queryRadius, QueryBuffer);

            for (int i = 0; i < QueryBuffer.Count; i++)
            {
                TestDirectionalMonsterController other = QueryBuffer[i];
                if (other == null || other == this)
                    continue;

                Vector3 delta = currentPosition - other.transform.position;
                delta.z = 0.0f;
                float minDistance = _bodyRadius + other.BodyRadius;
                float sqrDistance = delta.sqrMagnitude;
                if (sqrDistance >= minDistance * minDistance)
                    continue;

                float distance = Mathf.Sqrt(sqrDistance);
                Vector3 pushDirection = distance > 0.0001f ? delta / distance : (Vector3)_lastMoveDirection.normalized;
                if (pushDirection.sqrMagnitude <= 0.0001f)
                    pushDirection = Vector3.right;

                float overlap = minDistance - distance;
                currentPosition += pushDirection * overlap;
                currentPosition.z = transform.position.z;
            }

            return currentPosition;
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

        private void HandleStateFrameTriggered(CharacterState state, int frameIndex)
        {
            if (_isDead || state != CharacterState.Attack || _target == null)
                return;

            TestDirectionalPlayerController playerController = _target.GetComponent<TestDirectionalPlayerController>();
            if (playerController == null)
                return;

            Vector3 toPlayer = _target.position - transform.position;
            toPlayer.z = 0.0f;
            float distance = toPlayer.magnitude;
            if (distance > _attackRange || distance <= 0.0001f)
                return;

            Vector2 forward = TestDirectionalSpriteAnimator.DirectionToVector(_spriteAnimator.CurrentDirection);
            float angle = Vector2.Angle(forward, new Vector2(toPlayer.x, toPlayer.y));
            if (angle > _attackAngle * 0.5f)
                return;

            playerController.TakeDamage(_attackDamage, _spriteAnimator.CurrentDirection);
        }

        public void TakeDamage(int damage, EightDirection hitDirection)
        {
            if (_isDead)
                return;

            _currentHp = Mathf.Max(0, _currentHp - Mathf.Max(0, damage));
            RefreshHpBar();
            _hpBar?.ShowTemporarily();
            _lastMoveDirection = DirectionToWorldVector(hitDirection);
            _isAttacking = false;

            if (_currentHp > 0)
            {
                _spriteAnimator.Play(CharacterState.Hit, _lastMoveDirection, true);
                return;
            }

            _isDead = true;
            _isAttacking = false;
            _spriteAnimator.Play(CharacterState.Dead, _lastMoveDirection, true);
            gameObject.SetActive(false);
        }

        private void RefreshHpBar()
        {
            if (_hpBar == null)
                return;

            float normalized = _maxHp > 0 ? _currentHp / (float)_maxHp : 0.0f;
            _hpBar.SetHPBar(normalized);
        }

        public static List<TestDirectionalMonsterController> QueryMonstersInRadius(Vector3 worldPosition, float radius)
        {
            AttackQueryBuffer.Clear();
            TestDirectionalMonsterManager.Instance.QueryMonsters(new Vector2(worldPosition.x, worldPosition.y), radius, AttackQueryBuffer);
            return AttackQueryBuffer;
        }

        private static Vector3 DirectionToWorldVector(EightDirection direction)
        {
            Vector2 vector = TestDirectionalSpriteAnimator.DirectionToVector(direction);
            return new Vector3(vector.x, vector.y, 0.0f);
        }

        private void OnDrawGizmosSelected()
        {
            if (_drawRangeGizmos)
            {
                Gizmos.color = new Color(1.0f, 0.85f, 0.2f, 0.7f);
                Gizmos.DrawWireSphere(transform.position, _bodyRadius);

                Gizmos.color = new Color(0.2f, 0.8f, 1.0f, 0.6f);
                Gizmos.DrawWireSphere(transform.position, _detectRange);

                Gizmos.color = new Color(1.0f, 0.35f, 0.2f, 0.8f);
                Gizmos.DrawWireSphere(transform.position, _attackRange);
            }

            if (_drawAttackGizmo)
                DrawAttackGizmo();
        }

        private void DrawAttackGizmo()
        {
            Vector3 origin = transform.position;
            Vector3 forward = _lastMoveDirection.sqrMagnitude > 0.0001f ? _lastMoveDirection.normalized : Vector3.down;
            DrawSectorGizmo(origin, forward, _attackRange, _attackAngle, new Color(1.0f, 0.35f, 0.2f, 0.3f), new Color(1.0f, 0.45f, 0.1f, 0.9f));
        }

        private static void DrawSectorGizmo(Vector3 origin, Vector3 forward, float radius, float angle, Color fillColor, Color lineColor)
        {
            const int segments = 24;

            Quaternion startRotation = Quaternion.Euler(0.0f, 0.0f, angle * 0.5f);
            Vector3 startDirection = startRotation * forward.normalized;
            Vector3 previousPoint = origin + startDirection * radius;

            Gizmos.color = fillColor;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float currentAngle = Mathf.Lerp(angle * 0.5f, -angle * 0.5f, t);
                Vector3 currentDirection = Quaternion.Euler(0.0f, 0.0f, currentAngle) * forward.normalized;
                Vector3 currentPoint = origin + currentDirection * radius;
                Gizmos.DrawLine(origin, currentPoint);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            Gizmos.color = lineColor;
            previousPoint = origin + startDirection * radius;
            Gizmos.DrawLine(origin, previousPoint);
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float currentAngle = Mathf.Lerp(angle * 0.5f, -angle * 0.5f, t);
                Vector3 currentDirection = Quaternion.Euler(0.0f, 0.0f, currentAngle) * forward.normalized;
                Vector3 currentPoint = origin + currentDirection * radius;
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            Gizmos.DrawLine(origin, previousPoint);
        }
    }
}
