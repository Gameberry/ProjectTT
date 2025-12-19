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

        [SerializeField]
        private ComboController _comboController;

        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
        private float _attackTimming = 0.2f;

        [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField]
        private List<AttackData> _attackData = new List<AttackData>();

        [SerializeField]
        private AttackData _criticalFakeData = new AttackData();

        [SerializeField]
        private int _dataSelectIndex = -1;

        [SerializeField]
        private bool _setRandom = false;

        [SerializeField]
        private float _tempCritical = 0.5f;

        [SerializeField]
        private List<AttackData> _skillDatas = new List<AttackData>();

        // 평타
        private AttackData _currentAttackData = null;

        public bool _refreshAggro = false;

        // 조이스틱 넣기 전에 임시 변수
        private bool _useCustomDirVec = false;

        private Vector2 _customDieVec = Vector3.zero;
        // 조이스틱 넣기 전에 임시 변수

        [Header("카메라 흔들기")]
        [SerializeField] private bool NormalAttackShake = false;
        [SerializeField] private float NormalAttackShake_strengthOverride = 0.1f;
        [SerializeField] private float NormalAttackShake_durationOverride = 0.08f;

        [SerializeField] private bool CriticalAttackShake = true;
        [SerializeField] private float CriticalAttackShake_strengthOverride = 0.5f;
        [SerializeField] private float CriticalAttackShake_durationOverride = 0.25f;



        //------------------------------------------------------------------------------------
        public override void Init()
        {
            Message.AddListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);

            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
            RefreshCheatStat();

            _comboController = new ComboController();
            _comboController.Init(this);
            _comboController.SetVisibleComboUI();

            _currentSpineModelData = Managers.SkinManager.Instance.GetPlayerSpineModelData();
            SetSpineModelData(_currentSpineModelData);
            RefreshPlayerSkin(null);
        }
        //------------------------------------------------------------------------------------
        public override void Release()
        {
            Message.RemoveListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);

            _comboController?.Release();
        }
        //------------------------------------------------------------------------------------
        private void RefreshPlayerSkin(Event.RefreshPlayerSkinMsg msg)
        {
            SetSpineSkin(Managers.SkinManager.Instance.GetRuntimeSkin());
        }
        //------------------------------------------------------------------------------------
        public override void OnKillCharacter(CharacterControllerBase characterControllerBase)
        {
            //_comboController?.AddCombo();
        }
        //------------------------------------------------------------------------------------
        public override void HitResult(AttackData attackData)
        {
            if (attackData != null && attackData.HitEnemy.Count > 0)
                _comboController?.AddCombo();
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            for (int i = 0; i < _attackData.Count; ++i)
            {
                _attackData[i].Hitter = this;
            }

            _criticalFakeData.Hitter = this;

            for (int i = 0; i < _skillDatas.Count; ++i)
            {
                _skillDatas[i].Hitter = this;
                _skillDatas[i].NextPlayTime = Time.time + _skillDatas[i].Cooltime;
            }

            _dataSelectIndex = 0;

            ChangeState(CharacterState.Idle);
        }
        //------------------------------------------------------------------------------------
        public override Vector2 GetMoveDirection()
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

                if (_currentAttackData == null)
                    SetAttackData();

                float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                if (distance <= _currentAttackData.AttackRange && _blockAttack == false)
                {
                    AttackData selectAttackData = _currentAttackData;

                    float attackduration = selectAttackData.AttackDuration / FinalAttackSpeed;
                    _attackTimming = Time.time + attackduration;

                    if (string.IsNullOrEmpty(selectAttackData.CustomAni) == false)
                    { 
                        ChangeState(CharacterState.Attack, false);
                        PlayAnimation_AniName(selectAttackData.CustomAni);
                    }
                    else
                        ChangeState(CharacterState.Attack);

                    
                    ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                }
            }
            else if (CharacterState == CharacterState.Attack)
            {
                if (_attackTimming <= Time.time)
                {
                    _currentAttackData = null;

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
        protected override void SpineAnimationEvent(string aniName, string eventName)
        {
            if (CharacterState == CharacterState.Attack)
            {
                if (eventName.Contains("AniAction"))
                {
                    bool critical = Random.Range(0.0f, 1.0f) <= _tempCritical;

                    AttackData selectAttackData = critical ? _criticalFakeData : _currentAttackData;

                    selectAttackData.CustomParam = eventName;

                    if (_refreshAggro == true)
                        SetNewTarget();

                    if(AttackTarget == null || AttackTarget.IsDead)
                        SetNewTarget();

                    if (AttackTarget == null || AttackTarget.IsDead)
                    { 
                        _attackTimming = 0f;
                        return;
                    }

                    selectAttackData.HitEnemy.Clear();

                    ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                    _skillPlayer.PlaySkill(selectAttackData, AttackTarget);

                    if (critical == true)
                    {
                        if (CriticalAttackShake == true)
                        {
                            Managers.BattleSceneManager.Instance.PlayCameraShake(
                                CriticalAttackShake_strengthOverride,
                                CriticalAttackShake_durationOverride);
                        }
                    }
                    else
                    {
                        if (NormalAttackShake == true)
                        {
                            Managers.BattleSceneManager.Instance.PlayCameraShake(
                                NormalAttackShake_strengthOverride,
                                NormalAttackShake_durationOverride);
                        }
                    }


                    if (_currentAttackData != null && AttackTarget != null)
                    {
                        float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                        if (distance > _currentAttackData.AttackRange + 1.0f && _blockAttack == false)
                        {
                            _attackTimming = 0f;
                        }
                    }
                    
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetAttackData()
        {
            AttackData selectAttackData = _attackData.GetRandom();

            if (_setRandom == false)
            {
                if (_attackData.Count <= _dataSelectIndex)
                    _dataSelectIndex = 0;

                selectAttackData = _attackData[_dataSelectIndex];

                _dataSelectIndex++;
            }

            _currentAttackData = selectAttackData;
        }
        //------------------------------------------------------------------------------------
    }
}