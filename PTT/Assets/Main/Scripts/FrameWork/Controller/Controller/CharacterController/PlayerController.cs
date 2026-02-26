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
        [Header("Monster Attack Slot")]
        [SerializeField]
        private int _frontRowSlotCount = 6;
        [SerializeField]
        private float _frontRowRadius = 1.2f;
        [SerializeField]
        private int _secondRowSlotCount = 8;
        [SerializeField]
        private float _secondRowRadius = 2.0f;
        [SerializeField]
        private float _overflowWaitRadiusOffset = 1.0f;

        [Header("Monster Attack Slot Debug")]
        [SerializeField]
        private bool _drawAttackSlotGizmos = true;
        [SerializeField]
        private float _attackSlotGizmoSphereRadius = 0.12f;
        private struct AttackSlotReservation
        {
            public int RingIndex;
            public int SlotIndex;
        }
        private readonly Dictionary<MonsterController, AttackSlotReservation> _monsterSlotReservations = new Dictionary<MonsterController, AttackSlotReservation>();
        private readonly Dictionary<int, MonsterController> _occupiedSlots = new Dictionary<int, MonsterController>();

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
            ClearAttackSlotReservations();

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
                    if(_currentSkillAction != null)
                    {
                        if (characterState == CharacterState.Skill)
                            _currentSkillAction.Release();

                        _currentSkillAction = null;
                    }
                    
                    if (string.IsNullOrEmpty(_currentAttackData.AnimationName) == false)
                    { 
                        ChangeState(characterState, false);
                        PlayAnimation_AniName(_currentAttackData.AnimationName);
                    }
                    else
                        ChangeState(characterState);

                    if (characterState == CharacterState.Skill)
                        _currentSkillAction = _skillPlayer.PlaySkill(selectAttackData.GetAttackStruct(this, SkillManager.Instance.GetSkillLevel(selectAttackData.SkillId)), AttackTarget);

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

                    if (_currentAttackData != null)
                    {
                        _nextSkillData = null;
                        StartCoolDown(_currentAttackData.SkillId);
                    }
                }
                else if (eventName.Contains("End"))
                {
                    ReleaseAttack();
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected override void ReleaseAttack()
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
        public bool TryReserveAttackSlot(MonsterController monster, out Vector3 slotPosition, out int ringIndex)
        {
            slotPosition = transform.position;
            ringIndex = -1;
            if (monster == null)
                return false;
            CleanupInvalidSlotReservations();
            if (_monsterSlotReservations.TryGetValue(monster, out AttackSlotReservation reservedSlot))
            {
                ringIndex = reservedSlot.RingIndex;
                slotPosition = GetSlotWorldPosition(reservedSlot.RingIndex, reservedSlot.SlotIndex);
                return true;
            }
            if (TryFindNearestEmptySlot(monster.transform.position, 0, out int frontSlotIndex))
            {
                RegisterSlot(monster, 0, frontSlotIndex);
                ringIndex = 0;
                slotPosition = GetSlotWorldPosition(0, frontSlotIndex);
                return true;
            }
            if (TryFindNearestEmptySlot(monster.transform.position, 1, out int secondSlotIndex))
            {
                RegisterSlot(monster, 1, secondSlotIndex);
                ringIndex = 1;
                slotPosition = GetSlotWorldPosition(1, secondSlotIndex);
                return true;
            }
            return false;
        }
        //------------------------------------------------------------------------------------
        public bool TryGetReservedAttackSlotPosition(MonsterController monster, out Vector3 slotPosition, out int ringIndex)
        {
            slotPosition = transform.position;
            ringIndex = -1;
            if (monster == null)
                return false;
            CleanupInvalidSlotReservations();
            if (_monsterSlotReservations.TryGetValue(monster, out AttackSlotReservation reservation) == false)
                return false;
            ringIndex = reservation.RingIndex;
            slotPosition = GetSlotWorldPosition(reservation.RingIndex, reservation.SlotIndex);
            return true;
        }
        //------------------------------------------------------------------------------------
        public bool TryReassignAttackSlot(MonsterController monster, out Vector3 slotPosition, out int ringIndex)
        {
            ReleaseAttackSlot(monster);
            return TryReserveAttackSlot(monster, out slotPosition, out ringIndex);
        }
        //------------------------------------------------------------------------------------
        public void ReleaseAttackSlot(MonsterController monster)
        {
            if (monster == null)
                return;
            if (_monsterSlotReservations.TryGetValue(monster, out AttackSlotReservation reservation))
            {
                _monsterSlotReservations.Remove(monster);
                _occupiedSlots.Remove(GetSlotKey(reservation.RingIndex, reservation.SlotIndex));
            }
        }
        //------------------------------------------------------------------------------------
        public Vector3 GetOverflowWaitPosition(MonsterController monster)
        {
            int seed = monster == null ? 0 : Mathf.Abs(monster.GetInstanceID());
            float angle = (seed % 360) * Mathf.Deg2Rad;
            float waitRadius = Mathf.Max(_secondRowRadius + Mathf.Max(_overflowWaitRadiusOffset, 0.5f), _frontRowRadius + 1.0f);
            return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * waitRadius;
        }
        //------------------------------------------------------------------------------------
        private void RegisterSlot(MonsterController monster, int ringIndex, int slotIndex)
        {
            AttackSlotReservation reservation = new AttackSlotReservation
            {
                RingIndex = ringIndex,
                SlotIndex = slotIndex
            };
            _monsterSlotReservations[monster] = reservation;
            _occupiedSlots[GetSlotKey(ringIndex, slotIndex)] = monster;
        }
        //------------------------------------------------------------------------------------
        private bool TryFindNearestEmptySlot(Vector3 monsterPosition, int ringIndex, out int slotIndex)
        {
            slotIndex = -1;
            int slotCount = GetSlotCount(ringIndex);
            if (slotCount <= 0)
                return false;
            float bestDistanceSqr = float.MaxValue;
            for (int i = 0; i < slotCount; ++i)
            {
                int slotKey = GetSlotKey(ringIndex, i);
                if (_occupiedSlots.ContainsKey(slotKey))
                    continue;
                Vector3 slotWorldPosition = GetSlotWorldPosition(ringIndex, i);
                float distanceSqr = (monsterPosition - slotWorldPosition).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    slotIndex = i;
                }
            }
            return slotIndex >= 0;
        }
        //------------------------------------------------------------------------------------
        private Vector3 GetSlotWorldPosition(int ringIndex, int slotIndex)
        {
            int slotCount = GetSlotCount(ringIndex);
            if (slotCount <= 0)
                return transform.position;
            float radius = GetSlotRadius(ringIndex);
            float angle = (Mathf.PI * 2f / slotCount) * slotIndex;
            return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }
        //------------------------------------------------------------------------------------
        private int GetSlotCount(int ringIndex)
        {
            return ringIndex == 0 ? Mathf.Max(0, _frontRowSlotCount) : Mathf.Max(0, _secondRowSlotCount);
        }
        //------------------------------------------------------------------------------------
        private float GetSlotRadius(int ringIndex)
        {
            return ringIndex == 0 ? Mathf.Max(0.5f, _frontRowRadius) : Mathf.Max(_frontRowRadius + 0.2f, _secondRowRadius);
        }
        //------------------------------------------------------------------------------------
        private int GetSlotKey(int ringIndex, int slotIndex)
        {
            return ringIndex * 1000 + slotIndex;
        }
        //------------------------------------------------------------------------------------
        private void CleanupInvalidSlotReservations()
        {
            if (_monsterSlotReservations.Count == 0)
                return;
            List<MonsterController> invalidMonsters = null;
            foreach (var kvp in _monsterSlotReservations)
            {
                MonsterController monster = kvp.Key;
                if (monster != null && monster.IsDead == false)
                    continue;
                if (invalidMonsters == null)
                    invalidMonsters = new List<MonsterController>();
                invalidMonsters.Add(monster);
                _occupiedSlots.Remove(GetSlotKey(kvp.Value.RingIndex, kvp.Value.SlotIndex));
            }
            if (invalidMonsters == null)
                return;
            for (int i = 0; i < invalidMonsters.Count; ++i)
            {
                MonsterController monster = invalidMonsters[i];
                if (object.ReferenceEquals(monster, null) == false)
                    _monsterSlotReservations.Remove(monster);
            }
        }
        //------------------------------------------------------------------------------------
        private void ClearAttackSlotReservations()
        {
            _monsterSlotReservations.Clear();
            _occupiedSlots.Clear();
        }
        //------------------------------------------------------------------------------------
        private void OnDrawGizmosSelected()
        {
            if (_drawAttackSlotGizmos == false)
                return;
            DrawAttackSlotGizmosByRing(0, new Color(0.2f, 0.9f, 0.4f, 0.9f), new Color(0.95f, 0.3f, 0.2f, 0.95f));
            DrawAttackSlotGizmosByRing(1, new Color(0.25f, 0.6f, 1f, 0.9f), new Color(1f, 0.75f, 0.15f, 0.95f));
            float waitRadius = Mathf.Max(_secondRowRadius + Mathf.Max(_overflowWaitRadiusOffset, 0.5f), _frontRowRadius + 1.0f);
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, waitRadius);
        }
        //------------------------------------------------------------------------------------
        private void DrawAttackSlotGizmosByRing(int ringIndex, Color emptyColor, Color occupiedColor)
        {
            int slotCount = GetSlotCount(ringIndex);
            if (slotCount <= 0)
                return;
            float sphereRadius = Mathf.Max(0.03f, _attackSlotGizmoSphereRadius);
            float ringRadius = GetSlotRadius(ringIndex);
            Gizmos.color = new Color(emptyColor.r, emptyColor.g, emptyColor.b, 0.35f);
            Gizmos.DrawWireSphere(transform.position, ringRadius);
            for (int i = 0; i < slotCount; ++i)
            {
                Vector3 slotPos = GetSlotWorldPosition(ringIndex, i);
                bool isOccupied = _occupiedSlots.ContainsKey(GetSlotKey(ringIndex, i));
                Gizmos.color = isOccupied ? occupiedColor : emptyColor;
                Gizmos.DrawSphere(slotPos, sphereRadius);
            }
        }
        //------------------------------------------------------------------------------------
    }
}



