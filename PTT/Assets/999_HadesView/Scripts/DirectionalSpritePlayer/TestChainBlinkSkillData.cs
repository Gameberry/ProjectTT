using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    // 첫 타겟 즉시 타격 후, _stepDelay 간격으로 다음 타겟에 순차적으로 블링크 타격.
    [CreateAssetMenu(fileName = "ChainBlinkSkillData", menuName = "GameBerry/Test Scene/Skills/Chain Blink Skill")]
    public class TestChainBlinkSkillData : TestSkillData
    {
        [SerializeField] private int _maxTargets = 3;
        [SerializeField] private float _chainRange = 4f;
        [SerializeField] private float _stepDelay = 0.08f;
        [SerializeField] private GameObject[] _hitParticlePrefabs = new GameObject[0];

        public override float TargetQueryRadius => _chainRange;

        private sealed class ChainState
        {
            public readonly Queue<TestDirectionalMonsterController> Pending = new();
            public Vector3 CurrentPos;
            public Vector3 OriginPos;
            public Vector3 LastDir;
            public float Timer;
        }

        public override bool ExecuteHit(TestSkillExecutionContext ctx)
        {
            if (ctx.IsTriggered)
                return true;

            ctx.IsTriggered = true;

            if (IsValidTarget(ctx.LockedTarget) == false)
                return false;

            ChainState state = BuildChainState(ctx);
            if (state.Pending.Count == 0)
                return false;

            ExecuteStep(ctx, state);

            if (state.Pending.Count == 0)
                return true;

            state.Timer = _stepDelay;
            ctx.TickAction = (c) => TickChain(c, state);
            return true;
        }

        private ChainState BuildChainState(TestSkillExecutionContext ctx)
        {
            var state = new ChainState
            {
                CurrentPos = ctx.PlayerController.transform.position,
                OriginPos = ctx.PlayerController.transform.position,
                LastDir = ctx.SkillDirection,
            };

            var visited = new HashSet<TestDirectionalMonsterController>();
            TestDirectionalMonsterController current = ctx.LockedTarget;

            for (int i = 0; i < _maxTargets; i++)
            {
                if (IsValidTarget(current) == false)
                    break;

                state.Pending.Enqueue(current);
                visited.Add(current);

                Vector3 searchOrigin = current.transform.position;
                searchOrigin.z = 0f;
                current = FindNextChainTarget(searchOrigin, visited);
                if (current == null)
                    break;
            }

            return state;
        }

        private bool TickChain(TestSkillExecutionContext ctx, ChainState state)
        {
            state.Timer -= Time.deltaTime;
            if (state.Timer > 0f)
                return true;

            if (state.Pending.Count == 0)
                return false;

            state.Timer = _stepDelay;
            ExecuteStep(ctx, state);
            return state.Pending.Count > 0;
        }

        private void ExecuteStep(TestSkillExecutionContext ctx, ChainState state)
        {
            TestDirectionalMonsterController target = state.Pending.Dequeue();

            if (IsValidTarget(target) == false)
                return;

            if (IsBlockedByWall(GameLayers.MapBoundary, state.CurrentPos, target.transform.position))
            {
                state.Pending.Clear();
                return;
            }

            Vector3 toTarget = target.transform.position - state.CurrentPos;
            toTarget.z = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
                return;

            state.LastDir = toTarget.normalized;

            Vector3 behindOffset = state.LastDir * (target.BodyRadius + ctx.PlayerController.BodyRadius + 0.1f);
            Vector3 destination = target.transform.position + behindOffset;
            destination.z = state.OriginPos.z;
            destination = ClampDestinationToWall(ctx, state.CurrentPos, destination, GameLayers.MapBoundary);
            if (IsBlockedByWall(GameLayers.MapBoundary, state.CurrentPos, destination))
                destination = state.CurrentPos;

            if (ctx.HitBuffer.Contains(target) == false)
            {
                ctx.HitBuffer.Add(target);
                target.TakeDamage(Damage, ctx.SpriteAnimator.CurrentDirection);
            }

            PlayHitParticle(destination, state.LastDir);

            state.CurrentPos = destination;
            ctx.PlayerController.transform.position = destination;
            ctx.PlayerController.SetFacingDirection(state.LastDir);
            ctx.CacheGizmo(state.OriginPos, destination, ctx.PlayerController.BodyRadius, 2f);
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

                if (IsBlockedByWall(GameLayers.MapBoundary, fromPos, monster.transform.position))
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
