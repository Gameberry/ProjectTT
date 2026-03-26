using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Chart;
using GameBerry.Managers;
using UnityEngine;

namespace GameBerry
{
    public class BattleScene_GrowthDungeon : BattleSceneBase
    {
        private enum SpawnRole
        {
            Field,
            Support,
            Boss,
        }

        private readonly List<MonsterController> _activeMonsters = new List<MonsterController>();
        private readonly Dictionary<MonsterController, SpawnRole> _monsterRoles = new Dictionary<MonsterController, SpawnRole>();

        private CancellationTokenSource _contentCancellation = new CancellationTokenSource();
        private DungeonRuntimeInfo _currentInfo;
        private MonsterController _bossMonster;
        private bool _isResolved;
        private bool _bossSpawned;
        private float _remainingTime;
        private int _killCount;

        protected override void OnSetBattleScene()
        {
            ResetCancellation();
            ReleaseSpawnedObjects();
            _currentInfo = null;
            _bossMonster = null;
            _isResolved = false;
            _bossSpawned = false;
            _remainingTime = 0f;
            _killCount = 0;

            ResourceLoader.Instance.Load<GameObject>("BattleScene/PlayerController", o =>
            {
                GameObject clone = Object.Instantiate(o, Managers.BattleSceneManager.Instance.transform) as GameObject;
                if (clone == null)
                    return;

                PlayerController = clone.GetComponent<CharacterControllerBase>();
                if (PlayerController == null)
                    return;

                PlayerController.transform.position = Vector3.zero;
                PlayerController.Init();
                PlayBattleScene();
            });
        }

        protected override void OnPlayBattleScene()
        {
            if (PlayerController == null)
                return;

            Enum_Dungeon dungeonType = MyEnum_BattleType;
            int stage = GrowthDungeonManager.Instance.GetCurrentStage(dungeonType);
            if (GrowthDungeonManager.Instance.TryGetInfo(dungeonType, stage, out _currentInfo) == false || _currentInfo == null)
                return;

            _remainingTime = Mathf.Max(1f, _currentInfo.TimeLimit);
            _killCount = 0;
            _bossSpawned = false;
            _isResolved = false;

            PlayerController.Play();

            switch (_currentInfo.RuleType)
            {
                case Enum_GrowthDungeonRuleType.BossStun:
                    SpawnBossOnly();
                    RunBossStunLoop(_contentCancellation.Token).Forget();
                    break;

                case Enum_GrowthDungeonRuleType.KillCountWithTimeBonus:
                    StartFieldLoops(enableSupport: true, _contentCancellation.Token);
                    break;

                case Enum_GrowthDungeonRuleType.KillCountWithExplosion:
                    StartFieldLoops(enableSupport: true, _contentCancellation.Token);
                    break;

                case Enum_GrowthDungeonRuleType.StackBuffThenBoss:
                    StartFieldLoops(enableSupport: false, _contentCancellation.Token);
                    break;

                case Enum_GrowthDungeonRuleType.BossWeaknessCycle:
                    SpawnBossOnly();
                    RunBossWeaknessLoop(_contentCancellation.Token).Forget();
                    break;
            }
        }

        protected override void OnReleaseBattleScene()
        {
            ResetCancellation();
            ReleaseSpawnedObjects();
        }

        protected override void OnUpdated()
        {
            if (IsPlay == false || _currentInfo == null || _isResolved)
                return;

            _remainingTime = Mathf.Max(0f, _remainingTime - Time.deltaTime);
            if (_remainingTime <= 0f)
                FailDungeon();
        }

        public override void DeadPlayer(PlayerController playerController)
        {
            if (_isResolved)
                return;

            FailDungeon();
        }

        public override void DeadMonster(MonsterController monsterController)
        {
            HandleMonsterDeath(monsterController);
        }

        private void HandleMonsterDeath(MonsterController monsterController)
        {
            if (monsterController == null)
                return;

            if (_monsterRoles.TryGetValue(monsterController, out SpawnRole spawnRole) == false)
                return;

            _monsterRoles.Remove(monsterController);
            _activeMonsters.Remove(monsterController);

            if (_isResolved || _currentInfo == null)
                return;

            _killCount++;

            switch (_currentInfo.RuleType)
            {
                case Enum_GrowthDungeonRuleType.BossStun:
                    if (spawnRole == SpawnRole.Boss)
                        CompleteDungeon();
                    break;

                case Enum_GrowthDungeonRuleType.KillCountWithTimeBonus:
                    if (spawnRole == SpawnRole.Support)
                        _remainingTime += Mathf.Max(0f, _currentInfo.ExtraTimeOnSupportKill);

                    if (_killCount >= Mathf.Max(1, _currentInfo.TargetKillCount))
                        CompleteDungeon();
                    break;

                case Enum_GrowthDungeonRuleType.KillCountWithExplosion:
                    if (spawnRole == SpawnRole.Support)
                        TriggerExplosionKill(monsterController.transform.position, Mathf.Max(0.5f, _currentInfo.EffectRadius));

                    if (_killCount >= Mathf.Max(1, _currentInfo.TargetKillCount))
                        CompleteDungeon();
                    break;

                case Enum_GrowthDungeonRuleType.StackBuffThenBoss:
                    if (_bossSpawned == false)
                    {
                        ApplyTrainingBuff();

                        if (_killCount >= Mathf.Max(1, _currentInfo.BossSpawnKillCount))
                            SpawnTrainingBoss();
                    }
                    else if (spawnRole == SpawnRole.Boss)
                    {
                        CompleteDungeon();
                    }
                    break;

                case Enum_GrowthDungeonRuleType.BossWeaknessCycle:
                    if (spawnRole == SpawnRole.Boss)
                        CompleteDungeon();
                    break;
            }
        }

        private void StartFieldLoops(bool enableSupport, CancellationToken cancellationToken)
        {
            RunFieldSpawnLoop(cancellationToken).Forget();

            if (enableSupport && _currentInfo.SupportMonsterKey > 0)
                RunSupportSpawnLoop(cancellationToken).Forget();
        }

        private async UniTaskVoid RunFieldSpawnLoop(CancellationToken cancellationToken)
        {
            while (IsPlay && _isResolved == false)
            {
                SpawnFieldMonstersUpToCount();

                try
                {
                    await UniTask.Delay((int)(Mathf.Max(0.1f, _currentInfo.SpawnInterval) * 1000f), cancellationToken: cancellationToken);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async UniTaskVoid RunSupportSpawnLoop(CancellationToken cancellationToken)
        {
            while (IsPlay && _isResolved == false)
            {
                SpawnSupportMonstersUpToCount();

                try
                {
                    await UniTask.Delay((int)(Mathf.Max(0.1f, _currentInfo.SupportSpawnInterval) * 1000f), cancellationToken: cancellationToken);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async UniTaskVoid RunBossStunLoop(CancellationToken cancellationToken)
        {
            while (IsPlay && _isResolved == false && _bossMonster != null)
            {
                try
                {
                    await UniTask.Delay((int)(Mathf.Max(0.5f, _currentInfo.StunInterval) * 1000f), cancellationToken: cancellationToken);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }

                if (_isResolved || PlayerController == null)
                    break;

                PlayerController.PlayCharacterCondition(CreateCondition(Enum_ConditionType.Stun, Mathf.Max(0.2f, _currentInfo.StunDuration)));
            }
        }

        private async UniTaskVoid RunBossWeaknessLoop(CancellationToken cancellationToken)
        {
            while (IsPlay && _isResolved == false && _bossMonster != null)
            {
                _bossMonster.PlayCharacterCondition(CreateCondition(Enum_ConditionType.Invincible, Mathf.Max(0.5f, _currentInfo.BossInvincibleDuration)));

                try
                {
                    await UniTask.Delay((int)(Mathf.Max(0.5f, _currentInfo.BossInvincibleDuration) * 1000f), cancellationToken: cancellationToken);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }

                if (_bossMonster == null || _isResolved)
                    break;

                _bossMonster.RemoveConditionsByType(Enum_ConditionType.Invincible);

                try
                {
                    await UniTask.Delay((int)(Mathf.Max(0.5f, _currentInfo.BossWeakDuration) * 1000f), cancellationToken: cancellationToken);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }
            }
        }

        private void SpawnBossOnly()
        {
            _bossSpawned = true;
            ReleaseFieldAndSupportMonsters();
            _bossMonster = SpawnMonster(_currentInfo.BossMonsterKey, _currentInfo.BossMonsterModel, GetBossSpawnPosition(), SpawnRole.Boss);
        }

        private void SpawnTrainingBoss()
        {
            if (_bossSpawned)
                return;

            _bossSpawned = true;
            ReleaseFieldAndSupportMonsters();
            _bossMonster = SpawnMonster(_currentInfo.BossMonsterKey, _currentInfo.BossMonsterModel, GetBossSpawnPosition(), SpawnRole.Boss);
        }

        private void SpawnFieldMonstersUpToCount()
        {
            if (_currentInfo.FieldMonsterKey <= 0)
                return;

            int activeFieldCount = GetActiveCount(SpawnRole.Field);
            int desiredCount = Mathf.Max(1, _currentInfo.FieldMonsterCount);
            int spawnCount = desiredCount - activeFieldCount;
            if (spawnCount <= 0)
                return;

            List<Vector3> positions = PointSpawnPlacer2D.GeneratePositionsGuaranteed(
                Vector3.zero,
                spawnCount,
                StaticResource.Instance.GetBattleModeStaticData().SpawnrouteRadius,
                StaticResource.Instance.GetBattleModeStaticData().MonsterMinSeparation);

            for (int i = 0; i < positions.Count; ++i)
                SpawnMonster(_currentInfo.FieldMonsterKey, PickRandomModel(_currentInfo.FieldMonsterModel), positions[i], SpawnRole.Field);
        }

        private void SpawnSupportMonstersUpToCount()
        {
            if (_currentInfo.SupportMonsterKey <= 0)
                return;

            int desiredCount = Mathf.Max(1, _currentInfo.SupportMonsterCount);
            int activeSupportCount = GetActiveCount(SpawnRole.Support);
            int spawnCount = desiredCount - activeSupportCount;
            if (spawnCount <= 0)
                return;

            List<Vector3> positions = PointSpawnPlacer2D.GeneratePositionsGuaranteed(
                new Vector3(0f, 0f, 1.5f),
                spawnCount,
                StaticResource.Instance.GetBattleModeStaticData().SpawnrouteRadius,
                StaticResource.Instance.GetBattleModeStaticData().MonsterMinSeparation);

            for (int i = 0; i < positions.Count; ++i)
                SpawnMonster(_currentInfo.SupportMonsterKey, _currentInfo.SupportMonsterModel, positions[i], SpawnRole.Support);
        }

        private MonsterController SpawnMonster(int monsterKey, int modelIndex, Vector3 position, SpawnRole spawnRole)
        {
            if (monsterKey <= 0)
                return null;

            MonsterController monsterController = Managers.MonsterManager.Instance.GetMonster();
            if (monsterController == null)
                return null;

            monsterController.gameObject.SetActive(true);
            monsterController.transform.position = position;
            monsterController.SetMonster(null, position, monsterKey, modelIndex > 0 ? modelIndex : 1000, OnMonsterDead);
            monsterController.SetAggro(PlayerController as PlayerController);
            monsterController.Play();

            _activeMonsters.Add(monsterController);
            _monsterRoles[monsterController] = spawnRole;
            return monsterController;
        }

        private void OnMonsterDead(MonsterController monsterController)
        {
            DeadMonster(monsterController);
        }

        private void TriggerExplosionKill(Vector3 center, float radius)
        {
            List<MonsterController> targets = new List<MonsterController>();

            for (int i = 0; i < _activeMonsters.Count; ++i)
            {
                MonsterController monster = _activeMonsters[i];
                if (monster == null || monster == _bossMonster)
                    continue;

                if (_monsterRoles.TryGetValue(monster, out SpawnRole role) == false || role == SpawnRole.Support)
                    continue;

                Vector3 diff = monster.transform.position - center;
                if (diff.sqrMagnitude <= radius * radius)
                    targets.Add(monster);
            }

            for (int i = 0; i < targets.Count; ++i)
            {
                MonsterController target = targets[i];
                if (target != null && target.IsDead == false)
                    target.Damage(target.MaxHP * 2d);
            }
        }

        private void ApplyTrainingBuff()
        {
            if (PlayerController == null)
                return;

            if (_currentInfo.PlayerBuffAttackInc > 0f)
            {
                PlayerController.PlayCharacterCondition(CreateCondition(
                    Enum_ConditionType.AttackUp,
                    999f,
                    _currentInfo.PlayerBuffAttackInc));
            }

            if (_currentInfo.PlayerBuffMoveSpeedInc > 0f)
            {
                PlayerController.PlayCharacterCondition(CreateCondition(
                    Enum_ConditionType.MoveSpeedUp,
                    999f,
                    _currentInfo.PlayerBuffMoveSpeedInc));
            }
        }

        private void CompleteDungeon()
        {
            if (_isResolved)
                return;

            _isResolved = true;
            CleanupPlayerBuffs();
            GrowthDungeonManager.Instance.SetClearedStage(MyEnum_BattleType, GrowthDungeonManager.Instance.GetCurrentStage(MyEnum_BattleType), false);
            GrowthDungeonManager.Instance.TryGrantRewards(_currentInfo);

            if (GrowthDungeonManager.Instance.TryAdvanceToNextStage(MyEnum_BattleType, false) == false)
                GrowthDungeonManager.Instance.PrepareDungeon(MyEnum_BattleType, GrowthDungeonManager.Instance.GetCurrentStage(MyEnum_BattleType), false);

            ReleaseActiveMonsters();
            BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);
        }

        private void FailDungeon()
        {
            if (_isResolved)
                return;

            _isResolved = true;
            CleanupPlayerBuffs();
            ReleaseActiveMonsters();
            BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);
        }

        private void CleanupPlayerBuffs()
        {
            if (PlayerController == null)
                return;

            PlayerController.RemoveConditionsByType(Enum_ConditionType.AttackUp);
            PlayerController.RemoveConditionsByType(Enum_ConditionType.MoveSpeedUp);
            PlayerController.RemoveConditionsByType(Enum_ConditionType.Stun);
        }

        private void ReleaseSpawnedObjects()
        {
            CleanupPlayerBuffs();
            ReleaseActiveMonsters();

            if (PlayerController != null)
            {
                PlayerController.Release();
                Object.Destroy(PlayerController.gameObject);
                PlayerController = null;
            }
        }

        private void ReleaseActiveMonsters()
        {
            for (int i = _activeMonsters.Count - 1; i >= 0; --i)
            {
                MonsterController monster = _activeMonsters[i];
                if (monster == null || monster.IsDead)
                    continue;

                MonsterManager.Instance.PoolMonster(monster);
            }

            _activeMonsters.Clear();
            _monsterRoles.Clear();
            _bossMonster = null;
        }

        private void ReleaseFieldAndSupportMonsters()
        {
            for (int i = _activeMonsters.Count - 1; i >= 0; --i)
            {
                MonsterController monster = _activeMonsters[i];
                if (monster == null)
                    continue;

                if (_monsterRoles.TryGetValue(monster, out SpawnRole role) == false || role == SpawnRole.Boss)
                    continue;

                MonsterManager.Instance.PoolMonster(monster);
                _monsterRoles.Remove(monster);
                _activeMonsters.RemoveAt(i);
            }
        }

        private int GetActiveCount(SpawnRole spawnRole)
        {
            int count = 0;
            for (int i = 0; i < _activeMonsters.Count; ++i)
            {
                MonsterController monster = _activeMonsters[i];
                if (monster == null || monster.IsDead)
                    continue;

                if (_monsterRoles.TryGetValue(monster, out SpawnRole role) && role == spawnRole)
                    count++;
            }

            return count;
        }

        private int PickRandomModel(int[] models)
        {
            List<int> validModels = new List<int>();
            if (models != null)
            {
                for (int i = 0; i < models.Length; ++i)
                {
                    if (models[i] > 0)
                        validModels.Add(models[i]);
                }
            }

            if (validModels.Count <= 0)
                return 1000;

            return validModels[Random.Range(0, validModels.Count)];
        }

        private Vector3 GetBossSpawnPosition()
        {
            BattleModeStaticDataAsset data = StaticResource.Instance.GetBattleModeStaticData();
            if (data != null)
                return data.StageBossMonsterSpawnPosition;

            return new Vector3(2f, 0f, 0f);
        }

        private ConditionData CreateCondition(Enum_ConditionType type, float duration, float param1 = 0f)
        {
            return new ConditionData
            {
                Type = type,
                Duration = duration,
                Param1 = param1,
                EffectPos = PlayerController != null ? PlayerController.transform.position : Vector3.zero,
            };
        }

        private void ResetCancellation()
        {
            if (_contentCancellation != null)
            {
                _contentCancellation.Cancel();
                _contentCancellation.Dispose();
            }

            _contentCancellation = new CancellationTokenSource();
        }
    }
}
