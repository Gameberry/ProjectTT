using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Chart;
using GameBerry.Managers;
using UnityEngine;

namespace GameBerry
{
    public class BattleScene_Stage : BattleSceneBase
    {
        private CancellationTokenSource _spawnCancellation = new CancellationTokenSource();
        private readonly List<MonsterController> _bossBattleMonsters = new List<MonsterController>();

        protected override void OnSetBattleScene()
        {
            ResetSpawnCancellation();
            ReleaseSpawnedObjects();

            ResourceLoader.Instance.Load<GameObject>("BattleScene/PlayerController", o =>
            {
                GameObject clone = Object.Instantiate(o, Managers.BattleSceneManager.Instance.transform) as GameObject;
                if (clone == null)
                    return;

                PlayerController = clone.GetComponent<CharacterControllerBase>();
                if (PlayerController == null)
                    return;

                ApplyPlayerSpawnPosition(PlayerController.transform);
                PlayerController.Init();
                PlayBattleScene();
            });
        }

        protected override void OnPlayBattleScene()
        {
            if (PlayerController == null)
                return;

            if (StageManager.Instance.IsStageBossBattle)
                SpawnBossBattle();
            else
                PlayFieldStageLoop().Forget();

            PlayerController.Play();
            GetPlayerController()?.SetNewTarget();
        }

        private async UniTaskVoid PlayFieldStageLoop()
        {
            SpawnFieldMonsters();

            while (IsPlay)
            {
                BattleModeStaticDataAsset data = StaticResource.Instance.GetBattleModeStaticData();
                float waitTime = data != null ? Mathf.Max(0f, data.SpawnTurm) : 0f;

                try
                {
                    await UniTask.WaitForSeconds(waitTime, false, PlayerLoopTiming.Update, _spawnCancellation.Token);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }

                if (IsPlay == false)
                    break;

                SpawnFieldMonsters();
            }
        }

        private void SpawnFieldMonsters()
        {
            BattleModeStaticDataAsset data = StaticResource.Instance.GetBattleModeStaticData();
            if (data == null)
                return;

            Managers.BattleSceneManager.Instance.SpawnMonster(data.SpawnCount);
            GetPlayerController()?.SetNewTarget();
        }

        private void SpawnBossBattle()
        {
            if (StageManager.Instance.TryGetCurrentStageInfo(out StageInfo stageInfo) == false)
                return;

            BattleModeStaticDataAsset data = StaticResource.Instance.GetBattleModeStaticData();
            if (data == null)
                return;

            Vector3 bossPosition = data.StageBossMonsterSpawnPosition;

            if (stageInfo.BossMonster > 0)
                SpawnBossBattleMonster(stageInfo.BossMonster, stageInfo.BossMonsterModel, bossPosition);

            if (stageInfo.BossSubMonster > 0 && stageInfo.BossSubMonsterCount > 0)
            {
                float spawnRadius = data.StageBossSubMonsterSpawnRadius > 0f
                    ? data.StageBossSubMonsterSpawnRadius
                    : data.SpawnrouteRadius;
                float minSeparation = data.StageBossSubMonsterMinSeparation > 0f
                    ? data.StageBossSubMonsterMinSeparation
                    : data.MonsterMinSeparation;

                List<Vector3> spawnPositions = PointSpawnPlacer2D.GeneratePositionsGuaranteed(
                    bossPosition,
                    stageInfo.BossSubMonsterCount,
                    Mathf.Max(0.1f, spawnRadius),
                    Mathf.Max(0.1f, minSeparation));

                for (int i = 0; i < spawnPositions.Count; ++i)
                {
                    Vector3 spawnPosition = EnsureBossSubMonsterOffset(bossPosition, spawnPositions[i], minSeparation, i, stageInfo.BossSubMonsterCount);
                    SpawnBossBattleMonster(stageInfo.BossSubMonster, stageInfo.BossSubMonsterModel, spawnPosition);
                }
            }
        }

        private void SpawnBossBattleMonster(int monsterIndex, int modelIndex, Vector3 spawnPosition)
        {
            MonsterController monsterController = Managers.MonsterManager.Instance.GetMonster();
            if (monsterController == null)
                return;

            monsterController.gameObject.SetActive(true);
            monsterController.transform.position = spawnPosition;
            monsterController.SetMonster(null, spawnPosition, monsterIndex, modelIndex, OnDeadBossBattleMonster);
            monsterController.SetAggro(GetPlayerController());
            monsterController.Play();

            _bossBattleMonsters.Add(monsterController);
        }

        private void ApplyPlayerSpawnPosition(Transform playerTransform)
        {
            BattleModeStaticDataAsset data = StaticResource.Instance.GetBattleModeStaticData();
            if (data == null || playerTransform == null)
                return;

            playerTransform.position = StageManager.Instance.IsStageBossBattle
                ? data.StageBossPlayerSpawnPosition
                : Vector3.zero;
        }

        private PlayerController GetPlayerController()
        {
            return PlayerController as PlayerController;
        }

        private Vector3 EnsureBossSubMonsterOffset(Vector3 bossPosition, Vector3 spawnPosition, float minDistance, int index, int totalCount)
        {
            Vector3 offset = spawnPosition - bossPosition;
            if (offset.sqrMagnitude >= minDistance * minDistance)
                return spawnPosition;

            float angle = totalCount > 0 ? (Mathf.PI * 2f / totalCount) * index : 0f;
            Vector3 fallbackDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            if (fallbackDirection.sqrMagnitude <= Mathf.Epsilon)
                fallbackDirection = Vector3.right;

            return bossPosition + fallbackDirection.normalized * Mathf.Max(0.1f, minDistance);
        }

        protected override void OnReleaseBattleScene()
        {
            ResetSpawnCancellation();
            ReleaseSpawnedObjects();
        }

        public override void DeadPlayer(PlayerController playerController)
        {
        }

        public override void DeadMonster(MonsterController monsterController)
        {
            PlayerManager.Instance.AddExp(10);
        }

        private void OnDeadBossBattleMonster(MonsterController monsterController)
        {
            _bossBattleMonsters.Remove(monsterController);
            DeadMonster(monsterController);
        }

        private void ReleaseSpawnedObjects()
        {
            _bossBattleMonsters.Clear();

            if (BattleSceneManager.isAlive)
                BattleSceneManager.Instance.ReleaseAllMonsters();

            if (MonsterManager.isAlive)
                MonsterManager.Instance.ReleaseAllMonsters();

            if (PlayerController != null)
            {
                PlayerController.Release();
                Object.Destroy(PlayerController.gameObject);
                PlayerController = null;
            }
        }

        private void ResetSpawnCancellation()
        {
            if (_spawnCancellation != null)
            {
                _spawnCancellation.Cancel();
                _spawnCancellation.Dispose();
            }

            _spawnCancellation = new CancellationTokenSource();
        }
    }
}
