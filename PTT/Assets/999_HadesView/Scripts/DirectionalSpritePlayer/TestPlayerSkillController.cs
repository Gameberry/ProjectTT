using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestPlayerSkillController : MonoBehaviour
    {
        private static readonly HashSet<TestDirectionalMonsterController> SkillHitBuffer = new HashSet<TestDirectionalMonsterController>();
        private static TestDashSlashSkillData s_defaultDashSlashSkill;

        [SerializeField] private TestDirectionalPlayerController _playerController;
        [SerializeField] private TestDirectionalSpriteAnimator _spriteAnimator;
        [SerializeField] private List<TestSkillData> _skills = new List<TestSkillData>();
        [SerializeField] private bool _drawSkillGizmos = true;
        [SerializeField] private float _skillGizmoDuration = 2.0f;

        private TestSkillData _activeSkill;
        private TestSkillExecutionContext _context;

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
            _context = new TestSkillExecutionContext(_playerController, _spriteAnimator, SkillHitBuffer);
            EnsureDefaultSkills();
        }

        public bool TryUseFirstSkill()
        {
            return TryUseSkillAtIndex(0);
        }

        public bool TryUseSkillAtIndex(int skillIndex)
        {
            RefreshInterruptedSkillState();
            if (IsPlayingSkill)
                return false;

            TestSkillData skill = GetSkillAtIndex(skillIndex);
            if (skill == null)
                return false;

            return UseSkill(skill);
        }

        public bool UseSkill(TestSkillData skillData)
        {
            RefreshInterruptedSkillState();
            if (skillData == null || _playerController == null || _playerController.IsDead || IsPlayingSkill)
                return false;

            TestDirectionalMonsterController target = ResolveSkillTarget(skillData);
            if (target == null)
                return false;

            _activeSkill = skillData;
            _context.Reset();
            _context.LockedTarget = target;

            Vector3 toTarget = target.transform.position - _playerController.transform.position;
            toTarget.z = 0f;
            _context.SkillDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector3.down;

            _playerController.SetFacingDirection(_context.SkillDirection);
            _spriteAnimator.Play(skillData.PlaybackState, skillData.AnimationKey, _context.SkillDirection, true);
            return true;
        }

        public void CancelSkill()
        {
            _activeSkill = null;
            _context?.Reset();
        }

        // 반환값: 스킬(체인 포함)이 아직 진행 중이면 true, 완전히 종료되면 false.
        public bool TickSkillAnimation()
        {
            RefreshInterruptedSkillState();
            if (_activeSkill == null)
                return false;

            if (_context.TickAction != null)
            {
                bool stillActive = _context.TickAction(_context);
                if (!stillActive)
                {
                    _context.TickAction = null;
                    CancelSkill();
                    return false;
                }
            }

            _spriteAnimator.Play(_activeSkill.PlaybackState, _activeSkill.AnimationKey, _context.SkillDirection);
            return true;
        }

        public void HandleAnimatorStatePlaybackCompleted(CharacterState completedState)
        {
            RefreshInterruptedSkillState();
            if (_activeSkill == null || completedState != _activeSkill.PlaybackState)
                return;

            if (_context.TickAction != null)
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

        private void ExecuteActiveSkillHit()
        {
            if (_activeSkill == null)
                return;

            RefreshContextTarget();

            bool success = _activeSkill.ExecuteHit(_context);
            if (!success)
                CancelSkill();
        }

        private void RefreshContextTarget()
        {
            if (TestSkillData.IsValidTarget(_context.LockedTarget))
            {
                Vector3 toTarget = _context.LockedTarget.transform.position - _playerController.transform.position;
                toTarget.z = 0f;
                if (toTarget.sqrMagnitude > 0.0001f)
                    _context.SkillDirection = toTarget.normalized;
                return;
            }

            TestDirectionalMonsterController newTarget = ResolveSkillTarget(_activeSkill);
            _context.LockedTarget = newTarget;
            if (TestSkillData.IsValidTarget(newTarget))
            {
                Vector3 toTarget = newTarget.transform.position - _playerController.transform.position;
                toTarget.z = 0f;
                if (toTarget.sqrMagnitude > 0.0001f)
                    _context.SkillDirection = toTarget.normalized;
            }
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
            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i] != null)
                    return;
            }

            _skills.Clear();
            _skills.Add(GetOrCreateDefaultDashSlashSkill());
        }

        private TestSkillData GetSkillAtIndex(int skillIndex)
        {
            if (skillIndex < 0)
                return null;

            if (skillIndex < _skills.Count)
                return _skills[skillIndex];

            return skillIndex == 0 ? GetOrCreateDefaultDashSlashSkill() : null;
        }

        private TestDirectionalMonsterController ResolveSkillTarget(TestSkillData skillData)
        {
            TestDirectionalMonsterController currentTarget = _playerController.CurrentTarget;
            float queryRadius = skillData.TargetQueryRadius;
            var monsters = TestDirectionalMonsterController.QueryMonstersInRadius(
                _playerController.transform.position, queryRadius);

            Vector3 baseDirection = Vector3.zero;
            if (TestSkillData.IsValidTarget(currentTarget))
            {
                baseDirection = currentTarget.transform.position - _playerController.transform.position;
                baseDirection.z = 0f;
            }

            if (baseDirection.sqrMagnitude <= 0.0001f)
            {
                TestDirectionalMonsterController nearest = FindNearestTarget(monsters, queryRadius);
                if (nearest == null)
                    return null;

                baseDirection = nearest.transform.position - _playerController.transform.position;
                baseDirection.z = 0f;
            }

            return FindFarthestTargetInDirection(monsters, baseDirection, queryRadius, skillData.Angle);
        }

        private TestDirectionalMonsterController FindNearestTarget(
            List<TestDirectionalMonsterController> monsters, float maxRange)
        {
            TestDirectionalMonsterController nearest = null;
            float nearestSqrDist = maxRange * maxRange;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (TestSkillData.IsValidTarget(monster) == false)
                    continue;

                float sqrDist = (monster.transform.position - _playerController.transform.position).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = monster;
                }
            }

            return nearest;
        }

        private TestDirectionalMonsterController FindFarthestTargetInDirection(
            List<TestDirectionalMonsterController> monsters,
            Vector3 baseDirection,
            float maxRange,
            float allowedAngle)
        {
            if (baseDirection.sqrMagnitude <= 0.0001f)
                return null;

            TestDirectionalMonsterController farthest = null;
            float farthestDistance = 0f;
            Vector2 forward = new Vector2(baseDirection.x, baseDirection.y).normalized;
            float halfAngle = allowedAngle * 0.5f;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (TestSkillData.IsValidTarget(monster) == false)
                    continue;

                Vector3 toTarget = monster.transform.position - _playerController.transform.position;
                toTarget.z = 0f;
                float distance = toTarget.magnitude;
                if (distance <= 0.0001f || distance > maxRange)
                    continue;

                if (Vector2.Angle(forward, new Vector2(toTarget.x, toTarget.y)) > halfAngle)
                    continue;

                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthest = monster;
                }
            }

            return farthest;
        }

        private void RefreshInterruptedSkillState()
        {
            if (_activeSkill == null || _spriteAnimator == null)
                return;

            if (_spriteAnimator.CurrentState == _activeSkill.PlaybackState)
                return;

            if (_context.TickAction != null)
                return;

            CancelSkill();
        }

        private static TestDashSlashSkillData GetOrCreateDefaultDashSlashSkill()
        {
            if (s_defaultDashSlashSkill != null)
                return s_defaultDashSlashSkill;

            s_defaultDashSlashSkill = ScriptableObject.CreateInstance<TestDashSlashSkillData>();
            s_defaultDashSlashSkill.name = "Runtime_ByungRyeokIlSeom";
            return s_defaultDashSlashSkill;
        }

        private void OnDrawGizmos()
        {
            if (_drawSkillGizmos == false || Application.isPlaying == false || _context == null)
                return;

            if (Time.time > _context.LastGizmoExpireTime || _context.LastGizmoRadius <= 0f)
                return;

            DrawCapsuleGizmo(
                _context.LastGizmoStart,
                _context.LastGizmoEnd,
                _context.LastGizmoRadius,
                new Color(0.15f, 0.9f, 1.0f, 0.9f),
                new Color(0.15f, 0.9f, 1.0f, 0.2f));
        }

        private static void DrawCapsuleGizmo(
            Vector3 start, Vector3 end, float radius, Color lineColor, Color fillColor)
        {
            const int arcSegments = 12;
            Vector3 segment = end - start;
            Vector3 forward = segment.sqrMagnitude > 0.0001f ? segment.normalized : Vector3.right;
            Vector3 perp = new Vector3(-forward.y, forward.x, 0f);

            Gizmos.color = fillColor;
            Gizmos.DrawLine(start + perp * radius, end + perp * radius);
            Gizmos.DrawLine(start - perp * radius, end - perp * radius);

            Gizmos.color = lineColor;
            Gizmos.DrawLine(start + perp * radius, end + perp * radius);
            Gizmos.DrawLine(start - perp * radius, end - perp * radius);
            Gizmos.DrawLine(start + perp * radius, start - perp * radius);
            Gizmos.DrawLine(end + perp * radius, end - perp * radius);

            DrawArc(start, perp, forward, radius, arcSegments, lineColor);
            DrawArc(end, -perp, -forward, radius, arcSegments, lineColor);
        }

        private static void DrawArc(
            Vector3 center, Vector3 from, Vector3 to, float radius, int segments, Color color)
        {
            Gizmos.color = color;
            Vector3 prev = center + from.normalized * radius;
            for (int i = 1; i <= segments; i++)
            {
                Vector3 dir = Vector3.Slerp(from.normalized, to.normalized, i / (float)segments);
                Vector3 curr = center + dir * radius;
                Gizmos.DrawLine(prev, curr);
                prev = curr;
            }
        }
    }
}
