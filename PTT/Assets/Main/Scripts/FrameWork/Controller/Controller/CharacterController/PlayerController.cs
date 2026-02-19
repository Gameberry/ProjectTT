using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using GameBerry.Chart;

namespace GameBerry
{
    // 임시 네이밍 클래스. 클래스 이름 바꿀 수 있다면 바꾸자
    public class PlayerController : CharacterControllerBase
    {
        [SerializeField]
        private ComboController _comboController;

        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
        private float _attackTimming = 0.2f;

        [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField]
        private SkillInfo _defaultAttackData = new SkillInfo();

        [SerializeField]
        private List<string> _attackAnimations = new List<string>();

        [SerializeField]
        private int _attackAniSelectIndex = -1;

        private bool _comboTrigger = false;

        private SkillInfo _currentAttackData = null;
        private SkillAction _currentSkillAction = null;

        public bool _refreshAggro = false;

        // 조이스틱 넣기 전에 임시 변수
        private bool _useCustomDirVec = false;

        private Vector3 _customDieVec = Vector3.zero;
        // 조이스틱 넣기 전에 임시 변수

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            Message.AddListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);

            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
            RefreshCheatStat();

            PlayerManager.Instance.RefreshStat();
            EquipmentManager.Instance.RefreshStat();
            EngravingManager.Instance.RefreshStat();

            _comboController = new ComboController();
            _comboController.Init(this);
            _comboController.SetVisibleComboUI();

            _currentSpineModelData = Managers.SkinManager.Instance.GetPlayerSpineModelData();
            SetSpineModelData(_currentSpineModelData);
            RefreshPlayerSkin(null);

            // ============================================================
            // 스킬 시스템 초기화 (3줄 추가!)
            // ============================================================
            InitializeSkillSystem();      // CharacterControllerBase의 메서드
            ApplyPassiveSkills();          // CharacterControllerBase의 메서드
            AutoUseSkills = true;          // 자동 스킬 사용 활성화
            // ============================================================
        }
        //------------------------------------------------------------------------------------
        public override void Release()
        {
            Message.RemoveListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);

            _comboController?.Release();

            // ============================================================
            // 스킬 시스템 해제 (1줄 추가!)
            // ============================================================
            ReleaseSkillSystem();          // CharacterControllerBase의 메서드
            // ============================================================
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
        public override void OnHitCharacter(CharacterControllerBase characterControllerBase)
        {
            if (characterControllerBase != null && _comboTrigger == false)
            {
                _comboController?.AddCombo();
                _comboTrigger = true;
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            _attackAniSelectIndex = 0;

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

            _comboTrigger = false;

            // ============================================================
            // 스킬 시스템 업데이트 (1줄 추가!)
            // 이제 스킬이 자동으로 사용됨!
            // ============================================================
            UpdateSkillSystem();           // CharacterControllerBase의 메서드
            // ============================================================



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

                if (_currentAttackData != null && _nextSkillData != null)
                    _currentAttackData = _nextSkillData; // 혹시라도 평타 들어있으면 임의로 바로 바꾸기

                if (_currentAttackData == null)
                    SetAttackData();

                if (_currentAttackData == null)
                    return;

                float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                if (distance <= _currentAttackData.AttackRange)
                {
                    SkillInfo selectAttackData = _currentAttackData;

                    _attackTimming = Time.time + 10f;

                    CharacterState characterState = _currentAttackData == _nextSkillData ? CharacterState.Skill : CharacterState.Attack;

                    if (string.IsNullOrEmpty(_currentAttackData.AnimationName) == false)
                    { 
                        ChangeState(characterState, false);
                        PlayAnimation_AniName(_currentAttackData.AnimationName);
                    }
                    else
                        ChangeState(characterState);

                    if (characterState == CharacterState.Skill)
                        _skillPlayer.PlaySkill(selectAttackData.GetAttackStruct(this, SkillManager.Instance.GetSkillLevel(selectAttackData.SkillId)), AttackTarget);

                    ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                }
            }
            //else if (CharacterState == CharacterState.Attack)
            //{
            //    if (_attackTimming <= Time.time)
            //    {
            //        _currentAttackData = null;

            //        ChangeState(CharacterState.Idle);
            //        if (_refreshAggro == true)
            //            SetNewTarget();
            //        //if (AttackTarget != null)
            //        //{
            //        //    if (AttackTarget.IsDead)
            //        //        ChangeState(CharacterState.Idle);
            //        //    else
            //        //        _attackTimming = Time.time + _attackData.Cooltime;
            //        //}

            //    }
            //}
        }
        //------------------------------------------------------------------------------------
        protected override void SpineAnimationEvent(string aniName, string eventName)
        {
            if (CharacterState == CharacterState.Attack)
            {
                if (eventName.Contains("AniAction"))
                {
                    SkillInfo selectAttackData = _currentAttackData;

                    selectAttackData.CustomParam = eventName;

                    if (AttackTarget == null || AttackTarget.IsDead)
                        SetNewTarget();

                    if (AttackTarget == null || AttackTarget.IsDead)
                    {
                        ReleaseAttack();
                        return;
                    }

                    if (_currentAttackData != null && AttackTarget != null)
                    {
                        float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                        if (distance > _currentAttackData.AttackRange && _blockAttack == false)
                        {
                            ReleaseAttack();

                            return;
                        }
                    }

                    ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                    _skillPlayer.PlaySkill(selectAttackData.GetAttackStruct(this), AttackTarget);

                    // ============================================================
                    // 공격 시 쿨타임 감소 (1줄 추가!)
                    // ============================================================
                    OnSkillOwnerAttack();      // CharacterControllerBase의 메서드
                    // ============================================================

                    if (_refreshAggro == true)
                        SetNewTarget();

                    if (AttackTarget == null || AttackTarget.IsDead)
                        SetNewTarget();

                    if (_nextSkillData != null)
                    {
                        ReleaseAttack();
                        return;
                    }

                    if (_currentAttackData != null && AttackTarget != null)
                    {
                        float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                        if (distance > _currentAttackData.AttackRange && _blockAttack == false)
                        {
                            ReleaseAttack();
                        }
                    }

                }
                else if (eventName.Contains("End"))
                    ReleaseAttack();
            }
            else if (CharacterState == CharacterState.Skill)
            {
                if (eventName.Contains("AniAction"))
                {
                    SkillInfo selectAttackData = _currentAttackData;

                    selectAttackData.CustomParam = eventName;

                    if (AttackTarget == null || AttackTarget.IsDead)
                        SetNewTarget();

                    if (AttackTarget == null || AttackTarget.IsDead)
                    {
                        ReleaseAttack();
                        return;
                    }

                    if (_currentAttackData != null && AttackTarget != null)
                    {
                        float distance = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                        if (distance > _currentAttackData.AttackRange && _blockSkill == false)
                        {
                            ReleaseAttack();

                            return;
                        }
                    }

                    ChangeCharacterLookAtDirection_Target(AttackTarget.transform);
                    PlaySkill(selectAttackData.GetAttackStruct(this, SkillManager.Instance.GetSkillLevel(selectAttackData.SkillId)), transform.position, AttackTarget);
                }
                else if (eventName.Contains("End"))
                {
                    if (_currentAttackData != null)
                    {
                        _nextSkillData = null;
                        StartCoolDown(_currentAttackData.SkillId);
                    }

                    ReleaseAttack();
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void ReleaseAttack()
        {
            _attackTimming = 0f;
            _currentAttackData = null;

            if (_currentSkillAction != null)
                _currentSkillAction.Release();

            _currentSkillAction = null;

            ChangeState(CharacterState.Idle);
            if (_refreshAggro == true)
                SetNewTarget();
        }
        //------------------------------------------------------------------------------------
        public void SetAttackData()
        {
            if (_nextSkillData != null && _blockSkill == false)
            {
                _currentAttackData = _nextSkillData;
            }
            else
            {
                if (_blockAttack == true)
                    return;
                _currentAttackData = _defaultAttackData;

                if (_attackAnimations.Count <= _attackAniSelectIndex)
                    _attackAniSelectIndex = 0;

                _defaultAttackData.AnimationName = _attackAnimations[_attackAniSelectIndex];

                _attackAniSelectIndex++;
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// UI 버튼에서 수동으로 스킬 사용
        /// </summary>
        public void UseSkillManually(int slotIndex)
        {
            int skillId = SkillManager.Instance.GetEquippedSkillId(slotIndex);
            if (skillId > 0 && AttackTarget != null)
            {
                UseSkill(skillId, AttackTarget); // CharacterControllerBase의 메서드
            }
        }
        //------------------------------------------------------------------------------------
    }
}