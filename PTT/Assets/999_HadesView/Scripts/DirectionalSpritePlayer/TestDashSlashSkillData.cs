using UnityEngine;

namespace GameBerry.TestScene
{
    [CreateAssetMenu(fileName = "DashSlashSkillData", menuName = "GameBerry/Test Scene/Skills/Dash Slash Skill")]
    public class TestDashSlashSkillData : TestSkillData
    {
        [SerializeField] private float _dashDistance = 2.6f;
        [SerializeField] private float _dashDuration = 0.16f;
        [SerializeField] private float _dashHitRadius = 0.6f;
        [SerializeField] private GameObject[] _pathParticlePrefabs = new GameObject[0];

        public float DashDistance => _dashDistance;
        public float DashDuration => _dashDuration;
        public float DashHitRadius => _dashHitRadius;

        public override float TargetQueryRadius => Mathf.Max(Range, _dashDistance);

        public override bool ExecuteHit(TestSkillExecutionContext ctx)
        {
            if (ctx.IsTriggered)
                return true;

            ctx.IsTriggered = true;

            if (ctx.SkillDirection.sqrMagnitude <= 0.0001f)
                return false;

            PerformDashSlash(ctx);
            return true;
        }

        private void PerformDashSlash(TestSkillExecutionContext ctx)
        {
            TestDirectionalPlayerController player = ctx.PlayerController;
            Vector3 startPos = player.transform.position;
            Vector3 destination = ResolveDestination(ctx, startPos);
            destination.z = startPos.z;
            destination = ClampDestinationToWall(ctx, startPos, destination);

            float hitRadius = Mathf.Max(_dashHitRadius, player.BodyRadius);
            HitMonstersAlongSegment(ctx, startPos, destination, hitRadius, Damage);
            ctx.CacheGizmo(startPos, destination, hitRadius, 2f);
            PlayPathParticles(startPos, destination, ctx.SkillDirection);

            player.transform.position = destination;
            player.SetFacingDirection(ctx.SkillDirection);
        }

        private Vector3 ResolveDestination(TestSkillExecutionContext ctx, Vector3 startPos)
        {
            TestDirectionalMonsterController target = ctx.LockedTarget;
            if (IsValidTarget(target) == false)
                return startPos + ctx.SkillDirection * _dashDistance;

            Vector3 targetPos = target.transform.position;
            targetPos.z = 0f;
            Vector3 behindOffset = ctx.SkillDirection * (target.BodyRadius + ctx.PlayerController.BodyRadius + 0.1f);
            return targetPos + behindOffset;
        }

        private void PlayPathParticles(Vector3 startPos, Vector3 endPos, Vector3 direction)
        {
            if (_pathParticlePrefabs == null || _pathParticlePrefabs.Length == 0)
                return;

            float dashLength = (endPos - startPos).magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            for (int i = 0; i < _pathParticlePrefabs.Length; i++)
            {
                if (_pathParticlePrefabs[i] != null)
                    TestParticlePool.Instance.PlayWithSizeY(_pathParticlePrefabs[i], endPos, rotation, dashLength);
            }
        }
    }
}
