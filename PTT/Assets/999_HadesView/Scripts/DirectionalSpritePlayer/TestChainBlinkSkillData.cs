using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    // 첫 타겟에 도착 후 주변 미타격 적을 찾아 연속 이동하며 최대 _maxTargets명까지 연쇄 타격.
    [CreateAssetMenu(fileName = "ChainBlinkSkillData", menuName = "GameBerry/Test Scene/Skills/Chain Blink Skill")]
    public class TestChainBlinkSkillData : TestSkillData
    {
        [SerializeField] private int _maxTargets = 3;
        [SerializeField] private float _chainRange = 4f;
        [SerializeField] private float _dashHitRadius = 0.5f;
        [SerializeField] private GameObject[] _hitParticlePrefabs = new GameObject[0];

        public override float TargetQueryRadius => _chainRange;

        public override bool ExecuteHit(TestSkillExecutionContext ctx)
        {
            if (ctx.IsTriggered)
                return true;

            ctx.IsTriggered = true;

            if (IsValidTarget(ctx.LockedTarget) == false)
                return false;

            ExecuteChainBlink(ctx);
            return true;
        }

        private void ExecuteChainBlink(TestSkillExecutionContext ctx)
        {
            var visited = new HashSet<TestDirectionalMonsterController>();
            Vector3 originPos = ctx.PlayerController.transform.position;
            Vector3 currentPos = originPos;
            TestDirectionalMonsterController currentTarget = ctx.LockedTarget;
            visited.Add(currentTarget);

            Vector3 lastDir = ctx.SkillDirection;

            for (int i = 0; i < _maxTargets; i++)
            {
                if (IsValidTarget(currentTarget) == false)
                    break;

                Vector3 toTarget = currentTarget.transform.position - currentPos;
                toTarget.z = 0f;
                if (toTarget.sqrMagnitude <= 0.0001f)
                    break;

                lastDir = toTarget.normalized;

                Vector3 behindOffset = lastDir * (currentTarget.BodyRadius + ctx.PlayerController.BodyRadius + 0.1f);
                Vector3 destination = currentTarget.transform.position + behindOffset;
                destination.z = originPos.z;
                destination = ClampDestinationToWall(ctx, currentPos, destination);

                float hitRadius = Mathf.Max(_dashHitRadius, ctx.PlayerController.BodyRadius);
                HitMonstersAlongSegment(ctx, currentPos, destination, hitRadius, Damage);
                PlayHitParticle(destination, lastDir);

                currentPos = destination;

                TestDirectionalMonsterController next = FindNextChainTarget(currentPos, visited);
                if (next == null)
                    break;

                visited.Add(next);
                currentTarget = next;
            }

            ctx.PlayerController.transform.position = currentPos;
            ctx.PlayerController.SetFacingDirection(lastDir);
            ctx.CacheGizmo(originPos, currentPos, Mathf.Max(_dashHitRadius, ctx.PlayerController.BodyRadius), 2f);
        }

        private TestDirectionalMonsterController FindNextChainTarget(
            Vector3 fromPos, HashSet<TestDirectionalMonsterController> excluded)
        {
            var monsters = TestDirectionalMonsterController.QueryMonstersInRadius(fromPos, _chainRange);
            TestDirectionalMonsterController nearest = null;
            float nearestSqrDist = _chainRange * _chainRange;

            for (int i = 0; i < monsters.Count; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (IsValidTarget(monster) == false || excluded.Contains(monster))
                    continue;

                float sqrDist = (monster.transform.position - fromPos).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = monster;
                }
            }

            return nearest;
        }

        private void PlayHitParticle(Vector3 position, Vector3 direction)
        {
            if (_hitParticlePrefabs == null || _hitParticlePrefabs.Length == 0)
                return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            for (int i = 0; i < _hitParticlePrefabs.Length; i++)
            {
                if (_hitParticlePrefabs[i] != null)
                    TestParticlePool.Instance.Play(_hitParticlePrefabs[i], position, rotation);
            }
        }
    }
}
