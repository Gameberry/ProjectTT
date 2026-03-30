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
        private MonsterController _bossMonster;
        private bool _bossBattleCleared;
        private bool _bossBattleResolved;
        private float _bossBattleTimeRemaining;

        protected override void OnSetBattleScene()
        {
            ResetSpawnCancellation();
            ReleaseSpawnedObjects();
            _bossBattleCleared = false;
            _bossBattleResolved = false;
            _bossBattleTimeRemaining = 0f;

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

            ReleaseActiveMonsters();

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
            _bossMonster = null;
            _bossBattleCleared = false;
            _bossBattleResolved = false;
            _bossBattleTimeRemaining = Mathf.Max(0f, stageInfo.BossTime);
            StageManager.Instance.StartBossBattleTimer(_bossBattleTimeRemaining);

            if (stageInfo.BossMonster > 0)
                _bossMonster = SpawnBossBattleMonster(stageInfo.BossMonster, stageInfo.BossMonsterModel, bossPosition);

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

        private MonsterController SpawnBossBattleMonster(int monsterIndex, int modelIndex, Vector3 spawnPosition)
        {
            MonsterController monsterController = Managers.MonsterManager.Instance.GetMonster();
            if (monsterController == null)
                return null;

            monsterController.gameObject.SetActive(true);
            monsterController.transform.position = spawnPosition;
            monsterController.SetMonster(null, spawnPosition, monsterIndex, modelIndex, OnDeadBossBattleMonster);
            monsterController.SetAggro(GetPlayerController());
            monsterController.Play();

            _bossBattleMonsters.Add(monsterController);
            return monsterController;
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

        protected override void OnUpdated()
        {
            if (IsPlay == false || StageManager.Instance.IsStageBossBattle == false || _bossBattleResolved)
                return;

            if (_bossBattleTimeRemaining <= 0f)
                return;

            _bossBattleTimeRemaining = Mathf.Max(0f, _bossBattleTimeRemaining - Time.deltaTime);
            StageManager.Instance.UpdateBossBattleTimer(_bossBattleTimeRemaining);

            if (_bossBattleTimeRemaining <= 0f)
                FailBossBattle();
        }

        public override void DeadPlayer(PlayerController playerController)
        {
            if (StageManager.Instance.IsStageBossBattle == false)
                return;

            FailBossBattle();
        }

        public override void DeadMonster(MonsterController monsterController)
        {
            GiveStageKillRewards();
            //TryDropStageEquipment();
        }

        private void OnDeadBossBattleMonster(MonsterController monsterController)
        {
            _bossBattleMonsters.Remove(monsterController);
            DeadMonster(monsterController);

            if (_bossBattleResolved)
                return;

            if (monsterController == _bossMonster)
                CompleteBossBattle();
        }

        private void ReleaseSpawnedObjects()
        {
            ReleaseActiveMonsters();

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

        private void ReleaseActiveMonsters()
        {
            _bossBattleMonsters.Clear();
            _bossMonster = null;
            _bossBattleTimeRemaining = 0f;
            _bossBattleResolved = false;
            StageManager.Instance.StopBossBattleTimer();

            if (BattleSceneManager.isAlive)
                BattleSceneManager.Instance.ReleaseAllMonsters();

            if (MonsterManager.isAlive)
                MonsterManager.Instance.ReleaseAllMonsters();
        }

        private void TryDropStageEquipment()
        {
            if (StageManager.Instance.TryGetCurrentStageInfo(out StageInfo stageInfo) == false || stageInfo == null)
                return;

            if (stageInfo.EquipDropRate <= 0d || stageInfo.EquipList == null || stageInfo.EquipList.Length <= 0)
                return;

            if (Random.value > stageInfo.EquipDropRate)
                return;

            List<int> validEquipIds = new List<int>();
            for (int i = 0; i < stageInfo.EquipList.Length; ++i)
            {
                int equipItemId = stageInfo.EquipList[i];
                if (equipItemId > 0)
                    validEquipIds.Add(equipItemId);
            }

            if (validEquipIds.Count <= 0)
                return;

            int selectedEquipItemId = validEquipIds[Random.Range(0, validEquipIds.Count)];
            ItemManager.Instance.AddItem(selectedEquipItemId, 1);
        }

        private void GiveStageKillRewards()
        {
            if (StageManager.Instance.TryGetCurrentStageInfo(out StageInfo stageInfo) == false || stageInfo == null)
                return;

            if (stageInfo.Exp > 0)
                PlayerManager.Instance.AddExp(stageInfo.Exp);

            int goldItemId = GameChart.Get<PointChart>()?.GetByType(Enum_PointType.Gold)?.ItemId ?? 0;
            if (goldItemId > 0 && stageInfo.Gold > 0)
                ItemManager.Instance.AddItem(goldItemId, stageInfo.Gold);
        }

        private void CompleteBossBattle()
        {
            if (_bossBattleResolved)
                return;

            _bossBattleCleared = true;
            _bossBattleResolved = true;
            StageManager.Instance.StopBossBattleTimer();

            if (StageManager.Instance.TryAdvanceToNextStage())
            {
                StageManager.Instance.GetCurrentStage(out int nextChapter, out int nextStage);
                StageManager.Instance.PrepareFieldBattle(nextChapter, nextStage, false);
            }
            else
                StageManager.Instance.SetStageBattleMode(StageBattleMode.Field);

            ReleaseActiveMonsters();

            if (BattleSceneManager.isAlive)
                BattleSceneManager.Instance.ReloadCurrentBattleScene();
        }

        private void FailBossBattle()
        {
            if (_bossBattleResolved)
                return;

            _bossBattleCleared = false;
            _bossBattleResolved = true;
            StageManager.Instance.StopBossBattleTimer();

            StageManager.Instance.GetCurrentStage(out int chapter, out int stage);
            StageManager.Instance.PrepareFieldBattle(chapter, stage, false);

            ReleaseActiveMonsters();

            if (BattleSceneManager.isAlive)
                BattleSceneManager.Instance.ReloadCurrentBattleScene();
        }
    }
}
