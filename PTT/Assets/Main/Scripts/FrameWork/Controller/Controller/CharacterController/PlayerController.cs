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
        private SkillPlayer _skillPlayer;


        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
        private float _attackTimming = 0.2f;

        [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField]
        private List<AttackData> _attackData = new List<AttackData>();

        [SerializeField]
        private List<AttackData> _criticalAttackData = new List<AttackData>();

        [SerializeField]
        private int _dataSelectIndex = -1;

        [SerializeField]
        private bool _setRandom = false;

        [SerializeField]
        private float _tempCritical = 0.5f;

        [SerializeField]
        private List<AttackData> _skillDatas = new List<AttackData>();

        // 조이스틱 넣기 전에 임시 변수
        private bool _useCustomDirVec = false;

        private Vector3 _customDieVec = Vector3.zero;

        public bool _refreshAggro = false;

        // 조이스틱 넣기 전에 임시 변수
        //------------------------------------------------------------------------------------
        public override void Init()
        {
            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
            RefreshCheatStat();

            _currentSpineModelData = Managers.SkinManager.Instance.GetPlayerSpineModelData();
            SetSpineModelData(_currentSpineModelData);

            Managers.SkinManager.Instance.SetTempPlayerSpineHandler(_mySkeletonAnimationHandler);
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            for (int i = 0; i < _attackData.Count; ++i)
            {
                _attackData[i].Hitter = this;
            }

            for (int i = 0; i < _criticalAttackData.Count; ++i)
            {
                _criticalAttackData[i].Hitter = this;
            }

            for (int i = 0; i < _skillDatas.Count; ++i)
            {
                _skillDatas[i].Hitter = this;
                _skillDatas[i].NextPlayTime = Time.time + _skillDatas[i].Cooltime;
            }

            _dataSelectIndex = 0;

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
            if (CharacterState == CharacterState.Dead)
                return;

            if (_blockSkill == false)
            {
                for (int i = 0; i < _skillDatas.Count; ++i)
                {
                    AttackData attackData = _skillDatas[i];
                    if (attackData.NextPlayTime <= Time.time)
                    {
                        if (_attackTarget != null && _attackTarget.IsDead != true)
                        {
                            float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                            if (distance <= attackData.AttackRange)
                            {
                                _skillPlayer.PlaySkill(attackData, _attackTarget);
                                attackData.NextPlayTime = Time.time + attackData.Cooltime;
                            }
                        }
                    }
                }
            }
            

#if DEV_DEFINE
            _useCustomDirVec = false;
            _customDieVec = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                _useCustomDirVec = true;
                _customDieVec.y = 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                _useCustomDirVec = true;
                _customDieVec.y = -1;
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

                float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                if (distance <= _attackRange && _blockAttack == false)
                {
                    List<AttackData> attackDatas = Random.Range(0.0f, 1.0f) <= _tempCritical ? _criticalAttackData : _attackData;

                    AttackData selectAttackData = attackDatas.GetRandom();

                    if (_setRandom == false)
                    {
                        if (attackDatas.Count <= _dataSelectIndex)
                            _dataSelectIndex = 0;
                        
                        selectAttackData = attackDatas[_dataSelectIndex];

                        _dataSelectIndex++;
                    }
                    

                    float attackduration = selectAttackData.AttackDuration / FinalAttackSpeed;
                    float attackdelay = attackduration * selectAttackData.AttackDamageNormalTime;
                    _attackTimming = Time.time + attackduration;

                    if (string.IsNullOrEmpty(selectAttackData.CustomAni) == false)
                    { 
                        ChangeState(CharacterState.Attack, false);
                        PlayAnimation_AniName(selectAttackData.CustomAni);
                    }
                    else
                        ChangeState(CharacterState.Attack);

                    
                    ChangeCharacterLookAtDirection_Target(AttackTarget.transform);

                    selectAttackData.MeleeAttackDelay = attackdelay;
                    _skillPlayer.PlaySkill(selectAttackData, AttackTarget);
                }
            }
            else if (CharacterState == CharacterState.Attack)
            {
                if (_attackTimming <= Time.time)
                {
                    ChangeState(CharacterState.Idle);
                    if (_refreshAggro == true)
                        SetNewTarget();
                    //if (AttackTarget != null)
                    //{
                    //    if (AttackTarget.IsDead)
                    //        ChangeState(CharacterState.Idle);
                    //    else
                    //        _attackTimming = Time.time + _attackData.Cooltime;
                    //}
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}