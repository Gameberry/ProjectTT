using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Common;

namespace GameBerry
{
    public class MonsterController : CharacterControllerBase
    {
        [SerializeField]
        private List<string> idleAniNameList = new();

        [SerializeField]
        private List<string> AttackAniNameList = new();

        [SerializeField]
        private List<string> run1AniNameList = new();

        [SerializeField]
        private List<string> run2AniNameList = new();

        [SerializeField]
        private List<string> hitAniNameList = new();

        [SerializeField]
        private string idleAniName;

        [SerializeField]
        private string AttackAniName;

        [SerializeField]
        private string runAniName;

        [SerializeField]
        private string hitAniName;


        [SerializeField]
        private float attackRangeDefault = 1.5f;

        [SerializeField]
        private float attackRange = 1.0f;

        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
        [SerializeField]
        private float _attackTimming = 1.0f;

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        private BattleSceneMap_Aggro _battleSceneMap_Aggro;

        private Vector3 _spawnPos;

        public override void Init()
        {
            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
        }
        //------------------------------------------------------------------------------------
        public void SetMonster(BattleSceneMap_Aggro battleSceneMap_Aggro, Vector3 spawnPos, int modelIndex)
        { 
            RefreshCheatStat();

            _currentSpineModelData = StaticResource.Instance.GetCreatureSpineModelData(98);
            SetSpineModelData(_currentSpineModelData);
            _mySkeletonAnimationHandler._skeletonAnimation.initialSkinName = _currentSpineModelData.SkinList[Random.Range(4, _currentSpineModelData.SkinList.Count)];
            _mySkeletonAnimationHandler._skeletonAnimation.Initialize(true);

            _mySkeletonAnimationHandler._skeletonAnimation.skeleton.SetSlotsToSetupPose();
            _mySkeletonAnimationHandler._skeletonAnimation.skeleton.SetBonesToSetupPose();

            attackRange = attackRangeDefault + Random.Range(0.1f, 0.5f);

            if (_battleSceneMap_Aggro != null)
            {
                Debug.Log("sdf");
            }
            _battleSceneMap_Aggro = battleSceneMap_Aggro;
            _spawnPos = spawnPos;

            int idleattackidx = Random.Range(0, 2);

            idleAniName = idleAniNameList[Random.Range(0, idleAniNameList.Count)];
            AttackAniName = AttackAniNameList[Random.Range(0, AttackAniNameList.Count)];

            if (idleattackidx == 0)
                runAniName = run1AniNameList[Random.Range(0, run1AniNameList.Count)];
            else if (idleattackidx == 1)
                runAniName = run2AniNameList[Random.Range(0, run2AniNameList.Count)];

            hitAniName = idleAniName;
        }
        //------------------------------------------------------------------------------------
        public void SetAggro(PlayerController playerController)
        {
            if (playerController == null)
            {
                if (_attackTarget != null && _maxHP > CurrentHP)
                    return;
            }
            _attackTarget = playerController;
        }
        //------------------------------------------------------------------------------------
        protected override void OnDamage()
        {
            OnDamageDirection().Forget();
        }
        //------------------------------------------------------------------------------------
        private void TestAniPlay(string aniname, bool isloop = true)
        {
            PlayAnimation_AniName(aniname, isloop);
        }
        //------------------------------------------------------------------------------------
        private async UniTask OnDamageDirection()
        {
            ChangeState(CharacterState.Hit);
            TestAniPlay(hitAniName, false);
            ChangeSpineColor(StaticResource.Instance.GetBattleModeStaticData().MonsterHitColor);
            await UniTask.WaitForSeconds(StaticResource.Instance.GetBattleModeStaticData().MonsterHitDuration, false, PlayerLoopTiming.Update, disableCancellation.Token);
            ChangeSpineColor(Color.white);

            ChangeState(CharacterState.Idle);
            TestAniPlay(idleAniName);
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            ChangeState(CharacterState.Idle);
            TestAniPlay(idleAniName);
        }
        //------------------------------------------------------------------------------------
        public override Vector3 GetMoveDirection()
        {
            if (_attackTarget == null)
                return (_spawnPos - transform.position).normalized;
            else
                return base.GetMoveDirection();
        }
        //------------------------------------------------------------------------------------
        protected override void OnDead()
        {
            if (_battleSceneMap_Aggro != null)
            { 
                _battleSceneMap_Aggro.OnDeadMonster(this);
                _battleSceneMap_Aggro = null;
            }
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
            if (CharacterState != CharacterState.Dead && CharacterState != CharacterState.Hit)
            {
                if (CharacterState == CharacterState.Idle)
                {
                    if (_attackTarget != null)
                    {
                        ChangeState(CharacterState.Run);
                        TestAniPlay(runAniName);
                    }
                    else
                    {
                        float distance = MathDatas.GetDistance(transform.position, _spawnPos);
                        if (distance > StaticResource.Instance.GetBattleModeStaticData().MonsterReturnRadius && _blockAttack == false)
                        {
                            ChangeState(CharacterState.Run);
                            TestAniPlay(runAniName);
                        }
                    }
                }
                else if (CharacterState == CharacterState.Run)
                {
                    if (_attackTarget != null)
                    {
                        float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                        if (distance <= attackRange && _blockAttack == false)
                        {
                            ChangeState(CharacterState.Attack);
                            TestAniPlay(AttackAniName);
                            _attackTimming = Time.time + FinalAttackSpeed;
                        }
                    }
                    else
                    {
                        float distance = MathDatas.GetDistance(transform.position, _spawnPos);
                        if (distance < StaticResource.Instance.GetBattleModeStaticData().MonsterReturnRadius && _blockAttack == false)
                        {
                            ChangeState(CharacterState.Idle);
                            TestAniPlay(idleAniName);
                        }
                    }
                }
                else if (CharacterState == CharacterState.Attack)
                {
                    if (_attackTimming <= Time.time)
                    {
                        if (AttackTarget != null)
                        {
                            ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                            AttackTarget.Damage(FinalAttack);
                            ChangeState(CharacterState.Idle);
                            TestAniPlay(idleAniName);
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}