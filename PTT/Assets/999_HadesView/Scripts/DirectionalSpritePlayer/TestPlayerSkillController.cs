using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestPlayerSkillController : MonoBehaviour
    {
        private static readonly HashSet<TestDirectionalMonsterController> SkillHitBuffer = new HashSet<TestDirectionalMonsterController>();
        private static TestSkillData s_defaultByungRyeokIlSeomSkill;

        [SerializeField] private TestDirectionalPlayerController _playerController;
        [SerializeField] private TestDirectionalSpriteAnimator _spriteAnimator;
        [SerializeField] private List<TestSkillData> _skills = new List<TestSkillData>();
        [SerializeField] private GameObject[] _pathParticlePrefabs = new GameObject[0];
        [SerializeField] private bool _drawSkillGizmos = true;
        [SerializeField] private float _skillGizmoDuration = 2.0f;

        private TestSkillData _activeSkill;
        private TestDirectionalMonsterController _lockedTarget;
        private Vector3 _activeSkillDirection = Vector3.down;
        private bool _activeSkillTriggered;
        private Vector3 _lastSkillGizmoStart;
        private Vector3 _lastSkillGizmoEnd;
        private float _lastSkillGizmoRadius;
        private float _lastSkillGizmoExpireTime;

        public bool IsPlayingSkill
        {
            get
            {
                RefreshInterruptedSkillState();
                return _activeSkill != null;
            }
        }

        private void Reset()
        {
            EnsureDependencies();
        }

        private void Awake()
        {
            EnsureDependencies();
            EnsureDefaultSkills();
        }

        public bool TryUseFirstSkill()
        {
            RefreshInterruptedSkillState();
            if (IsPlayingSkill)
                return false;

            TestSkillData firstSkill = GetFirstSkill();
            if (firstSkill == null)
                return false;

            return UseSkill(firstSkill);
        }

        public bool UseSkill(TestSkillData skillData)
        {
            RefreshInterruptedSkillState();
            if (skillData == null || _playerController == null || _playerController.IsDead || IsPlayingSkill)
                return false;

            TestDirectionalMonsterController targetMonster = ResolveSkillTarget(skillData);
            if (targetMonster == null)
                return false;

            _activeSkill = skillData;
            _lockedTarget = targetMonster;
            _activeSkillDirection = targetMonster.transform.position - _playerController.transform.position;
            _activeSkillDirection.z = 0.0f;
            if (_activeSkillDirection.sqrMagnitude <= 0.0001f)
                _activeSkillDirection = Vector3.down;

            _activeSkillDirection.Normalize();
            _playerController.SetFacingDirection(_activeSkillDirection);
            _activeSkillTriggered = false;
            SkillHitBuffer.Clear();
            _spriteAnimator.Play(skillData.PlaybackState, skillData.AnimationKey, _activeSkillDirection, true);
            return true;
        }

        public void CancelSkill()
        {
            _activeSkill = null;
            _lockedTarget = null;
            _activeSkillTriggered = false;
            SkillHitBuffer.Clear();
        }

        public void TickSkillAnimation()
        {
            RefreshInterruptedSkillState();
            if (_activeSkill == null)
                return;

            _spriteAnimator.Play(_activeSkill.PlaybackState, _activeSkill.AnimationKey, _activeSkillDirection);
        }

        public void HandleAnimatorStatePlaybackCompleted(CharacterState completedState)
        {
            RefreshInterruptedSkillState();
            if (_activeSkill == null)
                return;

            if (completedState != _activeSkill.PlaybackState)
                return;

            CancelSkill();
        }

        public void HandleAnimatorStateFrameTriggered(CharacterState state, int frameIndex)
        {
            RefreshInterruptedSkillState();
            if (_activeSkill == null || state != _activeSkill.PlaybackState)
                return;

            ExecuteActiveSkillHit();
        }

        private void EnsureDependencies()
        {
            if (_playerController == null)
                _playerController = GetComponent<TestDirectionalPlayerController>();

            if (_spriteAnimator == null)
                _spriteAnimator = GetComponent<TestDirectionalSpriteAnimator>();
        }

        private void EnsureDefaultSkills()
        {
            bool hasUsableSkill = false;
            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i] == null)
                    continue;

                hasUsableSkill = true;
                break;
            }

            if (hasUsableSkill)
                return;

            _skills.Clear();
            _skills.Add(GetOrCreateDefaultByungRyeokIlSeomSkill());
        }

        private TestSkillData GetFirstSkill()
        {
            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i] != null)
                    return _skills[i];
            }

            return GetOrCreateDefaultByungRyeokIlSeomSkill();
        }

        private TestDirectionalMonsterController ResolveSkillTarget(TestSkillData skillData)
        {
            TestDirectionalMonsterController currentTarget = _playerController.CurrentTarget;
            float queryRadius = Mathf.Max(skillData.Range, skillData.DashDistance);
            var monsters = TestDirectionalMonsterController.QueryMonstersInRadius(_playerController.transform.position, queryRadius);

            Vector3 baseDirection = Vector3.zero;
            if (IsValidSkillTarget(currentTarget))
            {
                baseDirection = currentTarget.transform.position - _playerController.transform.position;
                baseDirection.z = 0.0f;
            }

            if (baseDirection.sqrMagnitude <= 0.0001f)
            {
                TestDirectionalMonsterController nearestTarget = FindNearestTarget(monsters, queryRadius);
                if (nearestTarget == null)
                    return null;

                baseDirection = nearestTarget.transform.position - _playerController.transform.position;
                baseDirection.z = 0.0f;
            }

            return FindFarthestTargetInDirection(monsters, baseDirection, queryRadius, skillData.Angle);
        }

        private void ExecuteActiveSkillHit()
        {
            if (_activeSkill == null)
                return;

            if (_activeSkill.ExecutionType == TestSkillExecutionType.DashSlash)
            {
                if (_activeSkillTriggered == false)
                {
                    _activeSkillTriggered = true;
                    if (IsValidSkillTarget(_lockedTarget))
                    {
                        _activeSkillDirection = _lockedTarget.transform.position - _playerController.transform.position;
                        _activeSkillDirection.z = 0.0f;
                    }
                    else
                    {
                        _lockedTarget = ResolveSkillTarget(_activeSkill);
                        if (IsValidSkillTarget(_lockedTarget))
                        {
                            _activeSkillDirection = _lockedTarget.transform.position - _playerController.transform.position;
                            _activeSkillDirection.z = 0.0f;
                        }
                    }

                    if (_activeSkillDirection.sqrMagnitude <= 0.0001f)
                    {
                        CancelSkill();
                        return;
                    }

                    _activeSkillDirection.Normalize();
                    PerformBlinkSlash(_activeSkill);
                }

                return;
            }

            HitMonstersInSector(_activeSkill.Damage, _activeSkill.Range, _activeSkill.Angle);
        }

        private void PerformBlinkSlash(TestSkillData skillData)
        {
            Vector3 startPosition = _playerController.transform.position;
            TestDirectionalMonsterController blinkTarget = IsValidSkillTarget(_lockedTarget)
                ? _lockedTarget
                : FindBlinkSlashTarget(skillData, _activeSkillDirection);
            Vector3 destination = ResolveBlinkSlashDestination(skillData, _activeSkillDirection, blinkTarget);
            destination.z = startPosition.z;
            float hitRadius = Mathf.Max(skillData.DashHitRadius, _playerController.BodyRadius);

            destination = ClampDestinationToWall(startPosition, destination);

            HitBlinkSlashTargets(startPosition, destination, skillData);
            CacheSkillGizmo(startPosition, destination, hitRadius);
            SetupAndPlayPathParticles(startPosition, destination, _activeSkillDirection);

            _playerController.transform.position = destination;
            _playerController.ResolveWallsAfterTeleport();
            _playerController.SetFacingDirection(_activeSkillDirection);
        }

        private Vector3 ResolveBlinkSlashDestination(TestSkillData skillData, Vector3 direction, TestDirectionalMonsterController blinkTarget)
        {
            if (blinkTarget == null)
                return _playerController.transform.position + direction * skillData.DashDistance;

            Vector3 targetPosition = blinkTarget.transform.position;
            targetPosition.z = 0.0f;
            Vector3 behindOffset = direction.normalized * (blinkTarget.BodyRadius + _playerController.BodyRadius + 0.1f);
            return targetPosition + behindOffset;
        }

        private Vector3 ClampDestinationToWall(Vector3 startPos, Vector3 destination)
        {
            Vector2 dir2D = (Vector2)(destination - startPos);
            float distance = dir2D.magnitude;
            if (distance <= 0.0001f)
                return destination;

            dir2D /= distance;
            float bodyRadius = _playerController.BodyRadius;
            RaycastHit2D hit = Physics2D.CircleCast(startPos, bodyRadius, dir2D, distance, _playerController.WallLayerMask);
            if (hit.collider == null)
                return destination;

            float safeDistance = Mathf.Max(0f, hit.distance);
            Vector3 clamped = startPos + new Vector3(dir2D.x, dir2D.y, 0f) * safeDistance;
            clamped.z = destination.z;
            return clamped;
        }

        private void SetupAndPlayPathParticles(Vector3 startPos, Vector3 endPos, Vector3 direction)
        {
            if (_pathParticlePrefabs == null || _pathParticlePrefabs.Length == 0)
                return;

            Vector3 midPoint = (startPos + endPos) * 0.5f;
            float dashLength = (endPos - startPos).magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            for (int i = 0; i < _pathParticlePrefabs.Length; i++)
            {
                if (_pathParticlePrefabs[i] == null)
                    continue;

                TestParticlePool.Instance.PlayWithSizeY(_pathParticlePrefabs[i], endPos, rotation, dashLength);
            }
        }

        private TestDirectionalMonsterController FindBlinkSlashTarget(TestSkillData skillData, Vector3 direction)
        {
            if (IsValidSkillTarget(_lockedTarget))
                return _lockedTarget;

            float queryRadius = Mathf.Max(skillData.Range, skillData.DashDistance);
            var monsters = TestDirectionalMonsterController.QueryMonstersInRadius(_playerController.transform.position, queryRadius);
            return FindFarthestTargetInDirection(monsters, direction, queryRadius, skillData.Angle);
        }

        private TestDirectionalMonsterController FindNearestTarget(List<TestDirectionalMonsterController> monsters, float maxRange)
        {
            TestDirectionalMonsterController nearestTarget = null;
            float nearestSqrDistance = maxRange * maxRange;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidSkillTarget(monster) == false)
                    continue;

                float sqrDistance = (monster.transform.position - _playerController.transform.position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestTarget = monster;
                }
            }

            return nearestTarget;
        }

        private TestDirectionalMonsterController FindFarthestTargetInDirection(
            List<TestDirectionalMonsterController> monsters,
            Vector3 baseDirection,
            float maxRange,
            float allowedAngle)
        {
            if (baseDirection.sqrMagnitude <= 0.0001f)
                return null;

            TestDirectionalMonsterController farthestTarget = null;
            float farthestDistance = 0.0f;
            Vector2 forward = new Vector2(baseDirection.x, baseDirection.y).normalized;
            float halfAngle = allowedAngle * 0.5f;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidSkillTarget(monster) == false)
                    continue;

                Vector3 toTarget = monster.transform.position - _playerController.transform.position;
                toTarget.z = 0.0f;
                float distance = toTarget.magnitude;
                if (distance <= 0.0001f || distance > maxRange)
                    continue;

                float angle = Vector2.Angle(forward, new Vector2(toTarget.x, toTarget.y));
                if (angle > halfAngle)
                    continue;

                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestTarget = monster;
                }
            }

            return farthestTarget;
        }

        private void HitBlinkSlashTargets(Vector3 startPosition, Vector3 destination, TestSkillData skillData)
        {
            HitMonstersAlongSegment(startPosition, destination, Mathf.Max(skillData.DashHitRadius, _playerController.BodyRadius), skillData.Damage);
        }

        private void HitMonstersAlongSegment(Vector3 startPosition, Vector3 endPosition, float hitRadius, int damage)
        {
            startPosition.z = 0.0f;
            endPosition.z = 0.0f;
            IReadOnlyList<TestDirectionalMonsterController> monsters = TestDirectionalMonsterManager.Instance.Monsters;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidSkillTarget(monster) == false || SkillHitBuffer.Contains(monster))
                    continue;

                Vector3 monsterPosition = monster.transform.position;
                monsterPosition.z = 0.0f;
                float sqrDistance = DistanceToSegmentSquared(monsterPosition, startPosition, endPosition);
                float combinedRadius = hitRadius + monster.BodyRadius;
                if (sqrDistance > combinedRadius * combinedRadius)
                    continue;

                if (IsBlockedByWall(startPosition, monsterPosition))
                    continue;

                SkillHitBuffer.Add(monster);
                monster.TakeDamage(damage, _spriteAnimator.CurrentDirection);
            }
        }

        private bool IsBlockedByWall(Vector3 startPosition, Vector3 targetPosition)
        {
            startPosition.z = 0.0f;
            targetPosition.z = 0.0f;

            Vector2 delta = (Vector2)(targetPosition - startPosition);
            if (delta.sqrMagnitude <= 0.0001f)
                return false;

            RaycastHit2D hit = Physics2D.Linecast(startPosition, targetPosition, _playerController.WallLayerMask);
            return hit.collider != null;
        }

        private void HitMonstersInSector(int damage, float range, float angle)
        {
            var monsters = TestDirectionalMonsterController.QueryMonstersInRadius(_playerController.transform.position, range + _playerController.BodyRadius);
            Vector2 forward = TestDirectionalSpriteAnimator.DirectionToVector(_spriteAnimator.CurrentDirection);
            float halfAngle = angle * 0.5f;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidSkillTarget(monster) == false)
                    continue;

                Vector3 toTarget = monster.transform.position - _playerController.transform.position;
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

        private static bool IsValidSkillTarget(TestDirectionalMonsterController target)
        {
            return target != null && target.isActiveAndEnabled && target.gameObject.activeInHierarchy && target.IsDead == false;
        }

        private static float DistanceToSegmentSquared(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
        {
            point.z = 0.0f;
            segmentStart.z = 0.0f;
            segmentEnd.z = 0.0f;
            Vector3 segment = segmentEnd - segmentStart;
            float segmentLengthSqr = segment.sqrMagnitude;
            if (segmentLengthSqr <= 0.0001f)
                return (point - segmentStart).sqrMagnitude;

            float t = Mathf.Clamp01(Vector3.Dot(point - segmentStart, segment) / segmentLengthSqr);
            Vector3 closestPoint = segmentStart + segment * t;
            return (point - closestPoint).sqrMagnitude;
        }

        private static TestSkillData GetOrCreateDefaultByungRyeokIlSeomSkill()
        {
            if (s_defaultByungRyeokIlSeomSkill != null)
                return s_defaultByungRyeokIlSeomSkill;

            s_defaultByungRyeokIlSeomSkill = ScriptableObject.CreateInstance<TestSkillData>();
            s_defaultByungRyeokIlSeomSkill.name = "Runtime_ByungRyeokIlSeom";
            return s_defaultByungRyeokIlSeomSkill;
        }

        private void OnDrawGizmos()
        {
            if (_drawSkillGizmos == false)
                return;

            if (Application.isPlaying == false)
                return;

            if (Time.time > _lastSkillGizmoExpireTime || _lastSkillGizmoRadius <= 0.0f)
                return;

            DrawCapsuleGizmo(_lastSkillGizmoStart, _lastSkillGizmoEnd, _lastSkillGizmoRadius, new Color(0.15f, 0.9f, 1.0f, 0.9f), new Color(0.15f, 0.9f, 1.0f, 0.2f));
        }

        private void RefreshInterruptedSkillState()
        {
            if (_activeSkill == null || _spriteAnimator == null)
                return;

            if (_spriteAnimator.CurrentState == _activeSkill.PlaybackState)
                return;

            CancelSkill();
        }

        private void CacheSkillGizmo(Vector3 start, Vector3 end, float radius)
        {
            _lastSkillGizmoStart = start;
            _lastSkillGizmoEnd = end;
            _lastSkillGizmoRadius = radius;
            _lastSkillGizmoExpireTime = Time.time + Mathf.Max(0.0f, _skillGizmoDuration);
        }

        private static void DrawCapsuleGizmo(Vector3 start, Vector3 end, float radius, Color lineColor, Color fillColor)
        {
            const int arcSegments = 12;

            Vector3 segment = end - start;
            Vector3 forward = segment.sqrMagnitude > 0.0001f ? segment.normalized : Vector3.right;
            Vector3 perpendicular = new Vector3(-forward.y, forward.x, 0.0f);

            Vector3 startLeft = start + perpendicular * radius;
            Vector3 startRight = start - perpendicular * radius;
            Vector3 endLeft = end + perpendicular * radius;
            Vector3 endRight = end - perpendicular * radius;

            Gizmos.color = fillColor;
            Gizmos.DrawLine(startLeft, endLeft);
            Gizmos.DrawLine(startRight, endRight);

            Gizmos.color = lineColor;
            Gizmos.DrawLine(startLeft, endLeft);
            Gizmos.DrawLine(startRight, endRight);
            Gizmos.DrawLine(startLeft, startRight);
            Gizmos.DrawLine(endLeft, endRight);

            DrawArc(start, perpendicular, forward, radius, arcSegments, lineColor);
            DrawArc(end, -perpendicular, -forward, radius, arcSegments, lineColor);
        }

        private static void DrawArc(Vector3 center, Vector3 from, Vector3 to, float radius, int segments, Color color)
        {
            Gizmos.color = color;
            Vector3 previousPoint = center + from.normalized * radius;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 direction = Vector3.Slerp(from.normalized, to.normalized, t);
                Vector3 currentPoint = center + direction * radius;
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
}
