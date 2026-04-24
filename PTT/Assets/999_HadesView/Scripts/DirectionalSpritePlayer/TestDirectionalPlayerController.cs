using UnityEngine;
using System.Collections.Generic;
using System;

namespace GameBerry.TestScene
{
    public class TestDirectionalPlayerController : MonoBehaviour
    {
        private static readonly List<TestDirectionalMonsterController> QueryBuffer = new List<TestDirectionalMonsterController>(32);
        private static readonly List<TestDirectionalMonsterController> AutoBuffer = new List<TestDirectionalMonsterController>(32);

        [SerializeField] private TestDirectionalSpriteAnimator _spriteAnimator;
        [SerializeField] private TestPlayerSkillController _skillController;
        [SerializeField] private UICharacterState _hpBar;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _bodyRadius = 0.4f;
        [SerializeField] private int _maxHp = 100;
        [SerializeField] private int _attackDamage = 10;
        [SerializeField] private float _attackRange = 1.0f;
        [SerializeField] private float _attackAngle = 120.0f;
        [SerializeField] private LayerMask _wallLayerMask;
        [SerializeField] private bool _autoPlay = true;
        [SerializeField] private float _autoDetectRange = 6f;
        [SerializeField] private bool _supportWASD = true;
        [SerializeField] private bool _enablePreviewHotKeys = true;
        [SerializeField] private bool _drawBodyGizmo = true;
        [SerializeField] private bool _drawAttackGizmo = true;

        private Vector3 _lastMoveDirection = Vector3.down;
        private CharacterState _previewState = CharacterState.None;
        private Vector3 _cachedSteerDirection;
        private int _steerFrame = -999;
        private const int SteerInterval = 3;
        private TestDirectionalMonsterController _autoTarget;
        private Transform _autoMoveDestination;
        [SerializeField]
        private int _currentHp;
        private bool _isDead;

        public event Action Died;

        public float BodyRadius => _bodyRadius;
        public int CurrentHp => _currentHp;
        public bool IsDead => _isDead;
        public Vector3 FacingDirection => _lastMoveDirection;
        public TestDirectionalMonsterController CurrentTarget => IsValidAutoTarget(_autoTarget) ? _autoTarget : null;
        public bool IsSkillCasting => _previewState == CharacterState.Skill && _skillController != null && _skillController.IsPlayingSkill;

        private void Reset()
        {
            EnsureDependencies();
        }

        private void Awake()
        {
            EnsureDependencies();
            _currentHp = _maxHp;
            _spriteAnimator.StatePlaybackCompleted += HandleStatePlaybackCompleted;
            _spriteAnimator.StateFrameTriggered += HandleStateFrameTriggered;
            //RefreshHpBar();
            _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection, true);
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
            if (_isDead)
                return;

            ValidateAutoTarget();
            UpdatePreviewState();

            Vector3 moveDirection = ReadMoveInput();
            if (moveDirection.sqrMagnitude > 1.0f)
                moveDirection.Normalize();

            bool hasManualInput = moveDirection.sqrMagnitude > 0.0001f;
            if (hasManualInput && IsSkillCasting == false)
                _lastMoveDirection = moveDirection.normalized;

            if (hasManualInput && _previewState == CharacterState.Attack)
                _previewState = CharacterState.None;

            // 자동 이동/공격 계산 (IsPreviewLockedState 체크 전에 실행해야 같은 프레임에 공격 시작)
            Vector3 autoMove = Vector3.zero;
            if (!hasManualInput && _autoPlay && _previewState == CharacterState.None)
                autoMove = ComputeAutoMove();

            if (IsPreviewLockedState(_previewState))
            {
                ResolveMonsterOverlaps(false);
                if (_previewState == CharacterState.Skill && _skillController != null && _skillController.IsPlayingSkill)
                    _skillController.TickSkillAnimation();
                else
                    _spriteAnimator.Play(_previewState, _lastMoveDirection);
                return;
            }

            if (hasManualInput)
            {
                transform.position += moveDirection * (_moveSpeed * Time.deltaTime);
                ResolveMonsterOverlaps(false);
                _spriteAnimator.Play(CharacterState.Run, _lastMoveDirection);
            }
            else if (autoMove.sqrMagnitude > 0.0001f)
            {
                transform.position += autoMove;
                ResolveMonsterOverlaps(true);
                _spriteAnimator.Play(CharacterState.Run, _lastMoveDirection);
            }
            else
            {
                ResolveMonsterOverlaps(true);
                CharacterState idleLikeState = _previewState == CharacterState.None ? CharacterState.Idle : _previewState;
                _spriteAnimator.Play(idleLikeState, _lastMoveDirection);
            }
        }

        private Vector3 ComputeAutoMove()
        {
            if (_autoMoveDestination != null && _autoMoveDestination.gameObject.activeInHierarchy)
            {
                Vector3 toDestination = _autoMoveDestination.position - transform.position;
                toDestination.z = 0f;
                if (toDestination.sqrMagnitude > 0.0025f)
                {
                    _lastMoveDirection = toDestination.normalized;
                    return GetSteeringDirection(toDestination) * (_moveSpeed * Time.deltaTime);
                }
            }

            TestDirectionalMonsterManager.Instance.QueryMonsters((Vector2)transform.position, _autoDetectRange, AutoBuffer);

            _autoTarget = FindNearestInBuffer();
            if (_autoTarget == null)
                return Vector3.zero;

            Vector3 toMonster = _autoTarget.transform.position - transform.position;
            toMonster.z = 0f;

            if (toMonster.sqrMagnitude > 0.0001f)
                _lastMoveDirection = toMonster.normalized;

            if (toMonster.sqrMagnitude <= _attackRange * _attackRange)
            {
                _previewState = CharacterState.Attack;
                return Vector3.zero;
            }

            return GetSteeringDirection(toMonster) * (_moveSpeed * Time.deltaTime);
        }

        private Vector3 GetSteeringDirection(Vector3 planarToTarget)
        {
            Vector2 desiredDir = ((Vector2)planarToTarget).normalized;
            Vector2 origin = (Vector2)transform.position;
            float lookAhead = _bodyRadius * 3f;

            // 벽이 없으면 직진 (몬스터는 overlap 해소로 처리)
            bool wallBlocked = Physics2D.CircleCast(origin, _bodyRadius * 0.5f, desiredDir, lookAhead, _wallLayerMask).collider != null;
            if (!wallBlocked)
                return (Vector3)desiredDir;

            if (Time.frameCount - _steerFrame >= SteerInterval)
            {
                _steerFrame = Time.frameCount;
                _cachedSteerDirection = ComputeSteeringDirection(desiredDir, origin, lookAhead);
            }
            return _cachedSteerDirection;
        }

        private Vector3 ComputeSteeringDirection(Vector2 desiredDir, Vector2 origin, float lookAhead)
        {
            Vector2 result = Vector2.zero;
            for (int i = 0; i < 8; i++)
            {
                Vector2 dir = TestSteeringUtils.Directions8[i];
                if (Physics2D.CircleCast(origin, _bodyRadius * 0.5f, dir, lookAhead, _wallLayerMask).collider != null)
                    continue;
                result += dir * Mathf.Max(0f, Vector2.Dot(desiredDir, dir));
            }
            return result.sqrMagnitude > 0.0001f ? (Vector3)result.normalized : (Vector3)desiredDir;
        }

        private TestDirectionalMonsterController FindNearestInBuffer()
        {
            TestDirectionalMonsterController nearest = null;
            float nearestSqrDist = float.MaxValue;
            Vector3 myPos = transform.position;

            for (int i = 0; i < AutoBuffer.Count; i++)
            {
                TestDirectionalMonsterController m = AutoBuffer[i];
                if (IsValidAutoTarget(m) == false)
                    continue;

                float sqrDist = (m.transform.position - myPos).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = m;
                }
            }

            return nearest;
        }

        private void ValidateAutoTarget()
        {
            if (IsValidAutoTarget(_autoTarget))
                return;

            _autoTarget = null;
        }

        private static bool IsValidAutoTarget(TestDirectionalMonsterController target)
        {
            return target != null && target.isActiveAndEnabled && target.gameObject.activeInHierarchy && target.IsDead == false;
        }

        private void EnsureDependencies()
        {
            if (_spriteAnimator == null)
                _spriteAnimator = GetComponent<TestDirectionalSpriteAnimator>();

            if (_spriteAnimator == null)
                _spriteAnimator = gameObject.AddComponent<TestDirectionalSpriteAnimator>();

            if (_skillController == null)
                _skillController = GetComponent<TestPlayerSkillController>();

            if (_skillController == null)
                _skillController = gameObject.AddComponent<TestPlayerSkillController>();

            if (_hpBar == null)
                _hpBar = GetComponent<UICharacterState>();

            if (_hpBar == null)
                _hpBar = gameObject.AddComponent<UICharacterState>();
        }

        private void HandleStatePlaybackCompleted(CharacterState completedState)
        {
            if (completedState == CharacterState.Attack)
            {
                ValidateAutoTarget();
                _previewState = CharacterState.None;
            }

            if (completedState == CharacterState.Skill && _skillController != null)
            {
                _skillController.HandleAnimatorStatePlaybackCompleted(completedState);
                _previewState = CharacterState.None;
            }
        }

        private void HandleStateFrameTriggered(CharacterState state, int frameIndex)
        {
            if (_isDead)
                return;

            if (state == CharacterState.Attack)
                HitMonstersInSector(_attackDamage, _attackRange, _attackAngle);
            else if (state == CharacterState.Skill && _skillController != null)
                _skillController.HandleAnimatorStateFrameTriggered(state, frameIndex);
        }

        private Vector3 ReadMoveInput()
        {
            float horizontal = 0.0f;
            float vertical = 0.0f;

            if (Input.GetKey(KeyCode.LeftArrow) || (_supportWASD && Input.GetKey(KeyCode.A)))
                horizontal -= 1.0f;

            if (Input.GetKey(KeyCode.RightArrow) || (_supportWASD && Input.GetKey(KeyCode.D)))
                horizontal += 1.0f;

            if (Input.GetKey(KeyCode.UpArrow) || (_supportWASD && Input.GetKey(KeyCode.W)))
                vertical += 1.0f;

            if (Input.GetKey(KeyCode.DownArrow) || (_supportWASD && Input.GetKey(KeyCode.S)))
                vertical -= 1.0f;

            return new Vector3(horizontal, vertical, 0.0f);
        }

        private void UpdatePreviewState()
        {
            if (_enablePreviewHotKeys == false)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                _previewState = CharacterState.None;
                _skillController?.CancelSkill();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
                _previewState = CharacterState.Attack;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                _previewState = CharacterState.Hit;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (_skillController != null && _skillController.TryUseFirstSkill())
                    _previewState = CharacterState.Skill;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                _previewState = CharacterState.Tran;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                _previewState = CharacterState.Dead;
        }

        private static bool IsPreviewLockedState(CharacterState state)
        {
            return state == CharacterState.Attack
                || state == CharacterState.Hit
                || state == CharacterState.Skill
                || state == CharacterState.Tran
                || state == CharacterState.Dead;
        }

        private void ResolveMonsterOverlaps(bool allowAutoAttack)
        {
            Vector3 resolvedPosition = transform.position;
            float queryRadius = _bodyRadius + 1.0f;
            TestDirectionalMonsterManager.Instance.QueryMonsters(new Vector2(resolvedPosition.x, resolvedPosition.y), queryRadius, QueryBuffer);

            TestDirectionalMonsterController overlappingMonster = null;
            float nearestOverlapSqrDist = float.MaxValue;

            for (int i = 0; i < QueryBuffer.Count; i++)
            {
                TestDirectionalMonsterController monster = QueryBuffer[i];
                if (IsValidAutoTarget(monster) == false)
                    continue;

                Vector3 delta = resolvedPosition - monster.transform.position;
                delta.z = 0.0f;
                float minDistance = _bodyRadius + monster.BodyRadius;
                float sqrDistance = delta.sqrMagnitude;
                if (sqrDistance >= minDistance * minDistance)
                    continue;

                if (_autoPlay && sqrDistance < nearestOverlapSqrDist)
                {
                    nearestOverlapSqrDist = sqrDistance;
                    overlappingMonster = monster;
                }

                float distance = Mathf.Sqrt(sqrDistance);
                Vector3 pushDirection = distance > 0.0001f ? delta / distance : (Vector3)_lastMoveDirection.normalized;
                if (pushDirection.sqrMagnitude <= 0.0001f)
                    pushDirection = Vector3.right;

                float overlap = minDistance - distance;
                resolvedPosition += pushDirection * overlap;
                resolvedPosition.z = transform.position.z;
            }

            transform.position = resolvedPosition;
            transform.position = TestSteeringUtils.ResolveWallOverlaps(transform.position, _bodyRadius, _wallLayerMask);

            if (!_autoPlay || allowAutoAttack == false)
                return;

            if (overlappingMonster != null)
            {
                // 물리적으로 충돌한 몬스터는 공격 중이더라도 즉시 타겟 전환
                _autoTarget = overlappingMonster;
                Vector3 toMonster = overlappingMonster.transform.position - transform.position;
                toMonster.z = 0f;
                if (toMonster.sqrMagnitude > 0.0001f)
                    _lastMoveDirection = toMonster.normalized;
                _previewState = CharacterState.Attack;
            }
            else if (_previewState == CharacterState.None)
            {
                TryAutoAttackNearbyMonster();
            }
        }

        private void TryAutoAttackNearbyMonster()
        {
            TestDirectionalMonsterController closest = null;
            float closestSqrDist = _attackRange * _attackRange;
            for (int i = 0; i < QueryBuffer.Count; i++)
            {
                TestDirectionalMonsterController monster = QueryBuffer[i];
                if (IsValidAutoTarget(monster) == false)
                    continue;
                float sqrDist = (monster.transform.position - transform.position).sqrMagnitude;
                if (sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closest = monster;
                }
            }
            if (closest == null)
                return;

            _autoTarget = closest;
            Vector3 toMonster = closest.transform.position - transform.position;
            toMonster.z = 0f;
            if (toMonster.sqrMagnitude > 0.0001f)
                _lastMoveDirection = toMonster.normalized;
            _previewState = CharacterState.Attack;
        }

        private void OnDrawGizmos()
        {
            if (_autoTarget == null)
                return;

            if (IsValidAutoTarget(_autoTarget) == false)
                return;

            Vector3 targetPos = _autoTarget.transform.position;
            Gizmos.color = new Color(1.0f, 0.85f, 0.0f, 0.9f);
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.DrawWireSphere(targetPos, _autoTarget.BodyRadius + 0.08f);
        }

        private void OnDrawGizmosSelected()
        {
            if (_drawBodyGizmo)
            {
                Gizmos.color = new Color(0.3f, 1.0f, 0.3f, 0.75f);
                Gizmos.DrawWireSphere(transform.position, _bodyRadius);
            }

            if (_drawAttackGizmo)
                DrawAttackGizmo();
        }

        public void TakeDamage(int damage, EightDirection hitDirection)
        {
            if (_isDead)
                return;

            _currentHp = Mathf.Max(0, _currentHp - Mathf.Max(0, damage));
            RefreshHpBar();
            _hpBar?.ShowTemporarily();
            _skillController?.CancelSkill();
            _previewState = CharacterState.None;
            if (_currentHp > 0)
            {
                _spriteAnimator.Play(CharacterState.Hit, DirectionToWorldVector(hitDirection), true);
                return;
            }

            _isDead = true;
            _previewState = CharacterState.Dead;
            _spriteAnimator.Play(CharacterState.Dead, DirectionToWorldVector(hitDirection), true);
            Died?.Invoke();
            enabled = false;
        }

        public void ResetForSpawn(Vector3 worldPosition, bool restoreFullHp = true)
        {
            transform.position = worldPosition;
            if (restoreFullHp)
                _currentHp = _maxHp;

            _isDead = false;
            _previewState = CharacterState.None;
            _autoTarget = null;
            _autoMoveDestination = null;
            _cachedSteerDirection = Vector3.zero;
            _steerFrame = -999;
            _skillController?.CancelSkill();
            RefreshHpBar();
            _hpBar?.HideImmediate();
            SetFacingDirection(Vector3.down);
            enabled = true;

            if (_spriteAnimator != null)
                _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection, true);
        }

        private void RefreshHpBar()
        {
            if (_hpBar == null)
                return;

            float normalized = _maxHp > 0 ? _currentHp / (float)_maxHp : 0.0f;
            _hpBar.SetHPBar(normalized);
        }

        private void HitMonstersInSector(int damage, float range, float angle)
        {
            var monsters = TestDirectionalMonsterController.QueryMonstersInRadius(transform.position, range + _bodyRadius);
            Vector2 forward = TestDirectionalSpriteAnimator.DirectionToVector(_spriteAnimator.CurrentDirection);
            float halfAngle = angle * 0.5f;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidAutoTarget(monster) == false)
                    continue;

                Vector3 toTarget = monster.transform.position - transform.position;
                toTarget.z = 0.0f;
                float distance = toTarget.magnitude;
                if (distance > range || distance <= 0.0001f)
                    continue;

                float targetAngle = Vector2.Angle(forward, new Vector2(toTarget.x, toTarget.y));
                if (targetAngle > halfAngle)
                    continue;

                monster.TakeDamage(damage, _spriteAnimator.CurrentDirection);
            }
        }

        private static Vector3 DirectionToWorldVector(EightDirection direction)
        {
            Vector2 vector = TestDirectionalSpriteAnimator.DirectionToVector(direction);
            return new Vector3(vector.x, vector.y, 0.0f);
        }

        public void SetFacingDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            _lastMoveDirection = direction.normalized;
        }

        public void SetAutoMoveDestination(Transform destination)
        {
            _autoMoveDestination = destination;
        }

        public void ClearAutoMoveDestination()
        {
            _autoMoveDestination = null;
        }

        public void ResolveWallsAfterTeleport()
        {
            transform.position = TestSteeringUtils.ResolveWallOverlaps(transform.position, _bodyRadius, _wallLayerMask);
        }

        private void DrawAttackGizmo()
        {
            Vector3 origin = transform.position;
            Vector3 forward = _lastMoveDirection.sqrMagnitude > 0.0001f ? _lastMoveDirection.normalized : Vector3.down;
            DrawSectorGizmo(origin, forward, _attackRange, _attackAngle, new Color(0.2f, 1.0f, 0.4f, 0.35f), new Color(0.1f, 0.85f, 0.3f, 0.9f));
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
