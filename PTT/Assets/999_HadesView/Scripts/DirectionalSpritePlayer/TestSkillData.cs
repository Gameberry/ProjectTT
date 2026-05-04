using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public abstract class TestSkillData : ScriptableObject
    {
        public const string ByungRyeokIlSeomSkillId = "skill.byungryeokilseom";

        [SerializeField] private string _skillId = ByungRyeokIlSeomSkillId;
        [SerializeField] private string _animationKey;
        [SerializeField] private CharacterState _playbackState = CharacterState.Skill;
        [SerializeField] private int _damage = 24;
        [SerializeField] private float _range = 2.5f;
        [SerializeField] private float _angle = 120.0f;
        [SerializeField] private bool _lockMovementDuringPlayback = true;

        public string SkillId => _skillId;
        public string AnimationKey => AnimationPlaybackKey.NormalizeAnimationKey(_animationKey);
        public CharacterState PlaybackState => _playbackState;
        public int Damage => _damage;
        public float Range => _range;
        public float Angle => _angle;
        public bool LockMovementDuringPlayback => _lockMovementDuringPlayback;

        // 타겟 탐색 반경. 대쉬 계열 스킬은 DashDistance를 포함해 override.
        public virtual float TargetQueryRadius => _range;

        // 반환값이 false이면 컨트롤러가 스킬을 취소함.
        public abstract bool ExecuteHit(TestSkillExecutionContext ctx);

        public static bool IsValidTarget(TestDirectionalMonsterController target)
        {
            return target != null && target.isActiveAndEnabled && target.gameObject.activeInHierarchy && target.IsDead == false;
        }

        protected static void HitMonstersInSector(TestSkillExecutionContext ctx, int damage, float range, float angle)
        {
            TestDirectionalPlayerController player = ctx.PlayerController;
            var monsters = TestDirectionalMonsterController.QueryMonstersInRadius(
                player.transform.position, range + player.BodyRadius);
            Vector2 forward = TestDirectionalSpriteAnimator.DirectionToVector(ctx.SpriteAnimator.CurrentDirection);
            float halfAngle = angle * 0.5f;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidTarget(monster) == false)
                    continue;

                Vector3 toTarget = monster.transform.position - player.transform.position;
                toTarget.z = 0f;
                float distance = toTarget.magnitude;
                if (distance > range || distance <= 0.0001f)
                    continue;

                if (Vector2.Angle(forward, new Vector2(toTarget.x, toTarget.y)) > halfAngle)
                    continue;

                monster.TakeDamage(damage, ctx.SpriteAnimator.CurrentDirection);
            }
        }

        protected static void HitMonstersAlongSegment(
            TestSkillExecutionContext ctx, Vector3 start, Vector3 end, float hitRadius, int damage)
        {
            start.z = 0f;
            end.z = 0f;
            IReadOnlyList<TestDirectionalMonsterController> monsters = TestDirectionalMonsterManager.Instance.Monsters;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidTarget(monster) == false || ctx.HitBuffer.Contains(monster))
                    continue;

                Vector3 monsterPos = monster.transform.position;
                monsterPos.z = 0f;
                float combined = hitRadius + monster.BodyRadius;
                if (DistanceToSegmentSquared(monsterPos, start, end) > combined * combined)
                    continue;

                if (IsBlockedByWall(ctx.PlayerController.WallLayerMask, start, monsterPos))
                    continue;

                ctx.HitBuffer.Add(monster);
                monster.TakeDamage(damage, ctx.SpriteAnimator.CurrentDirection);
            }
        }

        protected static Vector3 ClampDestinationToWall(
            TestSkillExecutionContext ctx, Vector3 startPos, Vector3 destination)
        {
            const float wallSkin = 0.05f;
            TestDirectionalPlayerController player = ctx.PlayerController;
            Vector2 dir2D = (Vector2)(destination - startPos);
            float distance = dir2D.magnitude;
            if (distance <= 0.0001f)
                return destination;

            dir2D /= distance;
            RaycastHit2D hit = Physics2D.CircleCast(startPos, player.BodyRadius, dir2D, distance, player.WallLayerMask);
            float safeDistance = hit.collider != null ? Mathf.Max(0f, hit.distance - wallSkin) : distance;
            return BacktrackToSafePosition(player, dir2D, startPos, safeDistance, destination.z);
        }

        private static Vector3 BacktrackToSafePosition(
            TestDirectionalPlayerController player, Vector2 direction, Vector3 startPos, float distance, float z)
        {
            const float wallSkin = 0.05f;
            float step = Mathf.Max(wallSkin, player.BodyRadius * 0.25f);
            var filter = new ContactFilter2D
            {
                layerMask = player.WallLayerMask,
                useLayerMask = true,
                useTriggers = false
            };
            var buffer = new Collider2D[8];

            for (int i = 0; i < 16; i++)
            {
                Vector3 candidate = startPos + new Vector3(direction.x, direction.y, 0f) * distance;
                candidate.z = z;

                if (Physics2D.OverlapCircle((Vector2)candidate, player.BodyRadius, filter, buffer) == 0)
                    return candidate;

                distance -= step;
                if (distance <= 0f)
                    break;
            }

            Vector3 fallback = startPos;
            fallback.z = z;
            return fallback;
        }

        private static bool IsBlockedByWall(LayerMask wallMask, Vector3 from, Vector3 to)
        {
            from.z = 0f;
            to.z = 0f;
            if (((Vector2)(to - from)).sqrMagnitude <= 0.0001f)
                return false;

            return Physics2D.Linecast(from, to, wallMask).collider != null;
        }

        private static float DistanceToSegmentSquared(Vector3 point, Vector3 segStart, Vector3 segEnd)
        {
            point.z = 0f;
            segStart.z = 0f;
            segEnd.z = 0f;
            Vector3 seg = segEnd - segStart;
            float lenSqr = seg.sqrMagnitude;
            if (lenSqr <= 0.0001f)
                return (point - segStart).sqrMagnitude;

            float t = Mathf.Clamp01(Vector3.Dot(point - segStart, seg) / lenSqr);
            return (point - (segStart + seg * t)).sqrMagnitude;
        }
    }
}
