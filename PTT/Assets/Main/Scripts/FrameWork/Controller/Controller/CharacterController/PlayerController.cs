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
        public event System.Action OnAttackPerformed;
        private Event.RefreshPlayerHpMsg _refreshPlayerHpMsg = new Event.RefreshPlayerHpMsg();
        [SerializeField]
        private ComboController _comboController;

        // 지금은 어택 애니도 뭐 없어서 일단 이정도로 구현
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
        private LanternController _lanternController;
        private int _lastMainLanternId = -1;
        private bool _isLanternPrefabLoading = false;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            Message.AddListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);
            OnHpChanged += SendRefreshPlayerHpMsg;

            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
            RefreshCheatStat();

            PlayerManager.Instance.RefreshStat();
            EquipmentManager.Instance.RefreshStat();
            EngravingManager.Instance.RefreshStat();
            LanternManager.Instance.RefreshStat();

            if (LanternManager.isAlive)
                LanternManager.Instance.OnLanternEquipChanged += HandleLanternEquipChanged;

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

            SendRefreshPlayerHpMsg(CurrentHP, MaxHP);
        }
        //------------------------------------------------------------------------------------
        public override void Release()
        {
            Message.RemoveListener<Event.RefreshPlayerSkinMsg>(RefreshPlayerSkin);
            OnHpChanged -= SendRefreshPlayerHpMsg;

            _comboController?.Release();

            // ============================================================
            // 스킬 시스템 해제 (1줄 추가!)
            // ============================================================
            ReleaseSkillSystem();          // CharacterControllerBase의 메서드

            if (LanternManager.isAlive)
                LanternManager.Instance.OnLanternEquipChanged -= HandleLanternEquipChanged;

            ReleaseLanternController();
            // ============================================================
        }
        //------------------------------------------------------------------------------------
        private void SendRefreshPlayerHpMsg(double currentHp, double maxHp)
        {
            _refreshPlayerHpMsg.CurrentHp = currentHp;
            _refreshPlayerHpMsg.MaxHp = maxHp;
            Message.Send(_refreshPlayerHpMsg);
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
            RefreshLanternController();

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
                    OnAttackPerformed?.Invoke();
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
        private void HandleLanternEquipChanged()
        {
            _lastMainLanternId = -1;
        }
        //------------------------------------------------------------------------------------
        private void RefreshLanternController()
        {
            int mainLanternId = LanternManager.isAlive ? LanternManager.Instance.GetMainLanternId() : 0;
            if (_lastMainLanternId == mainLanternId && _lanternController != null)
                return;

            _lastMainLanternId = mainLanternId;

            if (mainLanternId <= 0)
            {
                ReleaseLanternController();
                return;
            }

            if (_lanternController != null)
            {
                _lanternController.Setup(this, mainLanternId);
                return;
            }

            TryCreateLanternController(mainLanternId);
        }
        //------------------------------------------------------------------------------------
        private void TryCreateLanternController(int mainLanternId)
        {
            if (_isLanternPrefabLoading)
                return;

            if (LanternManager.isAlive == false)
                return;

            _isLanternPrefabLoading = true;

            Transform root = Managers.BattleSceneManager.Instance != null ? Managers.BattleSceneManager.Instance.transform : null;
            LanternManager.Instance.CreateLanternController(this, root, mainLanternId, controller =>
            {
                _isLanternPrefabLoading = false;

                if (controller == null)
                    return;

                _lanternController = controller;
            });
        }
        //------------------------------------------------------------------------------------
        private void ReleaseLanternController()
        {
            if (_lanternController == null)
                return;

            _lanternController.Release();
            Object.Destroy(_lanternController.gameObject);
            _lanternController = null;
        }
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



