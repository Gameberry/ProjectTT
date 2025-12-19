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
        private float attackRangeDefault = 1.5f;

        [SerializeField]
        private float attackRange = 1.0f;

        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
        [SerializeField]
        private float _attackTimming = 1.0f;

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        private BattleSceneMap_Aggro _battleSceneMap_Aggro;

        private Vector2 _spawnPos;

        public override void Init()
        {
            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
        }
        //------------------------------------------------------------------------------------
        public void SetMonster(BattleSceneMap_Aggro battleSceneMap_Aggro, Vector2 spawnPos, int modelIndex)
        { // 현재는 모델 인덱스만 받고 있다. 나중에 구조화 해야함
            RefreshCheatStat();

            _currentSpineModelData = StaticResource.Instance.GetCreatureSpineModelData(modelIndex);
            SetSpineModelData(_currentSpineModelData);

            attackRange = attackRangeDefault + Random.Range(0.1f, 0.5f);

            _battleSceneMap_Aggro = battleSceneMap_Aggro;
            _spawnPos = spawnPos;
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
        private async UniTask OnDamageDirection()
        {
            ChangeSpineColor(StaticResource.Instance.GetBattleModeStaticData().MonsterHitColor);
            await UniTask.WaitForSeconds(StaticResource.Instance.GetBattleModeStaticData().MonsterHitDuration, false, PlayerLoopTiming.Update, disableCancellation.Token);
            ChangeSpineColor(Color.white);
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            ChangeState(CharacterState.Idle);
        }
        //------------------------------------------------------------------------------------
        public override Vector2 GetMoveDirection()
        {
            if (_attackTarget == null)
                return (_spawnPos - new Vector2(transform.position.x, transform.position.y)).normalized;
            else
                return base.GetMoveDirection();
        }
        //------------------------------------------------------------------------------------
        protected override void OnDead()
        {
            _battleSceneMap_Aggro.OnDeadMonster(this);
            Managers.BattleSceneManager.Instance.DeadMonster(this);
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
            if (CharacterState != CharacterState.Dead)
            {
                if (CharacterState == CharacterState.Idle)
                {
                    if (_attackTarget != null)
                    {
                        ChangeState(CharacterState.Run);
                    }
                    else
                    {
                        float distance = MathDatas.GetDistance(transform.position, _spawnPos);
                        if (distance > StaticResource.Instance.GetBattleModeStaticData().MonsterReturnRadius && _blockAttack == false)
                        {
                            ChangeState(CharacterState.Run);
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
                            _attackTimming = Time.time + FinalAttackSpeed;
                        }
                    }
                    else
                    {
                        float distance = MathDatas.GetDistance(transform.position, _spawnPos);
                        if (distance < StaticResource.Instance.GetBattleModeStaticData().MonsterReturnRadius && _blockAttack == false)
                        {
                            ChangeState(CharacterState.Idle);
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
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}