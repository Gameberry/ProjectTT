using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Common;

namespace GameBerry
{
    public class BattleScene_Stage : BattleSceneBase
    {
        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        //------------------------------------------------------------------------------------
        protected override void OnSetBattleScene()
        {
            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == true)
                disableCancellation = new CancellationTokenSource();

            ResourceLoader.Instance.Load<GameObject>("BattleScene/PlayerController", o =>
            {
                GameObject clone = Object.Instantiate(o, Managers.BattleSceneManager.Instance.transform) as GameObject;
                if (clone != null)
                {
                    PlayerController = clone.GetComponent<CharacterControllerBase>();
                    PlayerController.Init();
                }
            });

            PlayBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlayBattleScene()
        {
            SpawnMonster();
            PlayCardGamblePlayTutorial().Forget();

            PlayerController.Play();
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayCardGamblePlayTutorial()
        {
            while (IsPlay)
            {
                await UniTask.WaitForSeconds(StaticResource.Instance.GetBattleModeStaticData().SpawnTurm, false, PlayerLoopTiming.Update, disableCancellation.Token);

                SpawnMonster();
            }
        }
        //------------------------------------------------------------------------------------
        private void SetRandomStartPos(Transform trans)
        {
            Vector3 pos = trans.transform.position;

            Vector3 minpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Min;
            Vector3 maxpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Max;
            pos.x = Random.Range(minpos.x, maxpos.x);
            pos.z = Random.Range(minpos.z, maxpos.z);

            trans.transform.position = pos;
        }
        //------------------------------------------------------------------------------------
        private void SpawnMonster()
        {
            for (int i = 0; i < StaticResource.Instance.GetBattleModeStaticData().SpawnCount; ++i)
            {
                MonsterController monsterController = Managers.MonsterManager.Instance.GetMonster();
                monsterController.gameObject.SetActive(true);
                SetRandomStartPos(monsterController.transform);
                monsterController.SetMonster(StaticResource.Instance.GetBattleModeStaticData().MonsterModelIdxs.GetRandom());
                monsterController.Play();
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnReleaseBattleScene()
        {

        }
        //------------------------------------------------------------------------------------
        public override void DeadPlayer(PlayerController playerController)
        {

        }
        //------------------------------------------------------------------------------------
        public override void DeadMonster(MonsterController monsterController)
        {
            Managers.MonsterManager.Instance.PoolMonster(monsterController);
        }
        //------------------------------------------------------------------------------------
    }
}