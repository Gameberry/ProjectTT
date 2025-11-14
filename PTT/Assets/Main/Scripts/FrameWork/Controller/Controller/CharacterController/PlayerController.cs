using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    // 임시 네이밍 클래스. 클래스 이름 바꿀 수 있다면 바꾸자
    public class PlayerController : CharacterControllerBase
    {
        [SerializeField]
        private SkillProjectilePlayer _projectilePlayer;

        [SerializeField]
        private float attackRange = 3.0f;

        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
        [SerializeField]
        private float _attackTimming = 0.2f;

        [SerializeField]
        private AttackData _attackData = new AttackData();

        [SerializeField]
        private List<AttackData> _skillDatas = new List<AttackData>();

        // 조이스틱 넣기 전에 임시 변수
        private bool _useCustomDirVec = false;

        private Vector3 _customDieVec = Vector3.zero;
        // 조이스틱 넣기 전에 임시 변수
        //------------------------------------------------------------------------------------
        public override void Init()
        {
            Managers.AggroManager.Instance.AddIFFCharacterAggro(this);
            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
            RefreshCheatStat();

            _currentSpineModelData = StaticResource.Instance.GetCreatureSpineModelData(0);
            SetSpineModelData(_currentSpineModelData);
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            _attackData.Hitter = this;

            for (int i = 0; i < _skillDatas.Count; ++i)
            {
                _skillDatas[i].Hitter = this;
                _skillDatas[i].NextPlayTime = Time.time + _skillDatas[i].Cooltime;
            }

            ChangeState(CharacterState.Idle);
        }
        //------------------------------------------------------------------------------------
        public override Vector3 GetMoveDirection()
        { // MoveController_Base에서 주로 호출
            // 유저는 조이스틱으로 방향을 정할 때가 있어서 가상함수로 만듬

            if (_useCustomDirVec == true)
                return _customDieVec.normalized;

            return base.GetMoveDirection();
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {

#if DEV_DEFINE
            _useCustomDirVec = false;
            _customDieVec = Vector3.zero;



            if (Input.GetKey(KeyCode.W))
            {
                _useCustomDirVec = true;
                _customDieVec.z = 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                _useCustomDirVec = true;
                _customDieVec.z = -1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                _useCustomDirVec = true;
                _customDieVec.x = 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                _useCustomDirVec = true;
                _customDieVec.x = -1;
            }

            if (_useCustomDirVec == true)
            {
                _attackTarget = null;
                ChangeState(CharacterState.Run);
                return;
            }
#endif

            if (CharacterState != CharacterState.Dead)
            {
                for (int i = 0; i < _skillDatas.Count; ++i)
                {
                    AttackData attackData = _skillDatas[i];
                    if (attackData.NextPlayTime <= Time.time)
                    {
                        if (_attackTarget != null && _attackTarget.IsDead != true)
                        { 
                            float distance = MathDatas.GetDistance(transform.position.x, transform.position.z, _attackTarget.transform.position.x, _attackTarget.transform.position.z);
                            if (distance <= attackData.AttackRange)
                            {
                                _projectilePlayer.PlayProjectile(attackData, _attackTarget);
                                attackData.NextPlayTime = Time.time + attackData.Cooltime;
                            }
                        }
                    }
                }

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
                        _attackTimming = Time.time + _attackData.Cooltime;
                    }
                }
                else if (CharacterState == CharacterState.Attack)
                {
                    if (_attackTimming <= Time.time)
                    {
                        if (AttackTarget != null)
                        {
                            ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                            _projectilePlayer.PlayProjectile(_attackData, AttackTarget);
                            AttackTarget.OnDamage(MyDamage);
                            if (AttackTarget.IsDead)
                                ChangeState(CharacterState.Idle);
                            else
                                _attackTimming = Time.time + _attackData.Cooltime;
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}