using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class MonsterController : CharacterControllerBase
    {
        [SerializeField]
        private float attackRange = 1.0f;

        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
        [SerializeField]
        private float _attackTimming = 1.0f;

        public override void Init()
        {
            Managers.AggroManager.Instance.AddIFFCharacterAggro(this);
            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
        }
        //------------------------------------------------------------------------------------
        public void SetMonster(int modelIndex)
        { // 현재는 모델 인덱스만 받고 있다. 나중에 구조화 해야함
            RefreshCheatStat();

            _currentSpineModelData = StaticResource.Instance.GetCreatureSpineModelData(modelIndex);
            SetSpineModelData(_currentSpineModelData);

            attackRange = Random.Range(0.9f, 1.1f);
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            ChangeState(CharacterState.Idle);
        }
        //------------------------------------------------------------------------------------
        protected override void OnDead()
        {
            Managers.BattleSceneManager.Instance.DeadMonster(this);
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
            if (CharacterState != CharacterState.Dead)
            {
                if (CharacterState == CharacterState.Idle || CharacterState == CharacterState.Run)
                {
                    if (_attackTarget == null || _attackTarget.IsDead == true)
                    {
                        SetNewTarget();
                    }

                    if (_attackTarget != null)
                    {
                        ChangeState(CharacterState.Run);
                    }
                    else
                    {
                        ChangeState(CharacterState.Idle);
                        return;
                    }

                    float distance = MathDatas.GetDistance(transform.position.x, transform.position.z, _attackTarget.transform.position.x, _attackTarget.transform.position.z);
                    if (distance <= attackRange)
                    {
                        ChangeState(CharacterState.Attack);
                        _attackTimming = Time.time + _characterAttackSpeed;
                    }
                }
                else if (CharacterState == CharacterState.Attack)
                {
                    if (_attackTimming <= Time.time)
                    {
                        if (AttackTarget != null)
                        {
                            ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                            AttackTarget.OnDamage(MyDamage);
                            ChangeState(CharacterState.Idle);
                        }   
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}