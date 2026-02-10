using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Managers;
using Spine;
using Spine.Unity;
using GameBerry.Chart;

namespace GameBerry
{
    public class CharacterControllerBase : MonoBehaviour
    {
        public Enum_LookDirection LookDirection { get { return _lookDirection; } }
        [SerializeField]
        protected Enum_LookDirection _lookDirection = Enum_LookDirection.Right;

        [SerializeField]
        private Vector3 _element_leftRot;
        [SerializeField]
        private Vector3 _element_rightRot;

        [SerializeField]
        private CharacterBillboardController _characterBillboardController;

        [SerializeField]
        private CharacterConditionController _conditionController;
        public CharacterConditionController CharacterConditionController { get { return _conditionController; } }
        

        [SerializeField]
        protected IFFType _iFFType = IFFType.IFF_None;

        public IFFType IFFType { get { return _iFFType; } }

        [SerializeField]
        protected CharacterState _characterState = CharacterState.None;
        public CharacterState CharacterState { get { return _characterState; } }

        [SerializeField] protected SkeletonAnimationHandler _mySkeletonAnimationHandler;

        [SerializeField] protected SpineModelData _currentSpineModelData;

        [SerializeField]
        protected UICharacterState _uiCharacterState;

        public bool IsDead { get { return CharacterState == CharacterState.Dead; } }


        [SerializeField]
        protected Rigidbody _rigidbody;
        public Rigidbody MyRigidbody { get { return _rigidbody; } }

        [SerializeField]
        protected CharacterControllerBase _attackTarget;
        public CharacterControllerBase AttackTarget { get { return _attackTarget; } }



        #region Skill System Fields
        [SerializeField]
        protected SkillPlayer _skillPlayer;
        protected SkillInfo _nextSkillData = null;

        private SkillChart _skillChart;

        // 장착된 스킬 ID 목록
        private List<int> _equippedSkillIds = new List<int>();




        // 쿨타임 정보
        private class SkillCooldownInfo
        {
            public int skillId;
            public Enum_SkillCooldownType cooldownType;
            public float nextAvailableTime;
            public int remainingAttackCount;
            public float cooldownValue;

            public bool IsReady()
            {
                if (cooldownType == Enum_SkillCooldownType.Time)
                    return Time.time >= nextAvailableTime;
                else if (cooldownType == Enum_SkillCooldownType.AttackCount)
                    return remainingAttackCount <= 0;

                return true;
            }

            public void StartCooldown()
            {
                if (cooldownType == Enum_SkillCooldownType.Time)
                    nextAvailableTime = Time.time + cooldownValue;
                else if (cooldownType == Enum_SkillCooldownType.AttackCount)
                    remainingAttackCount = Mathf.CeilToInt(cooldownValue);
            }

            public void OnAttack()
            {
                if (cooldownType == Enum_SkillCooldownType.AttackCount && remainingAttackCount > 0)
                    remainingAttackCount--;
            }
        }

        private Dictionary<int, SkillCooldownInfo> _skillCooldowns = new Dictionary<int, SkillCooldownInfo>();

        // 스킬 설정
        public bool AutoUseSkills { get; set; } = true;
        public float DefaultSkillRange { get; set; } = 3f;

        // 스킬 사용 이벤트
        public event System.Action<int> OnSkillUsed;

        #endregion



#if UNITY_EDITOR
        [SerializeField]
#endif
        protected CharacterStatOperator _characterStatOperator = new CharacterStatOperator();
        public CharacterStatOperator CharacterStatOperator { get { return _characterStatOperator; } }


        [SerializeField]
        protected double _maxHP = 0.0;
        public double MaxHP { get { return _maxHP; } }


        [SerializeField]
        protected double _currentHP = 0.0;
        public double CurrentHP { get { return _currentHP; } }


        [SerializeField]
        protected float _aniControllerSpeed = 1.0f;
        public float AniControllerSpeed
        {
            get { return _aniControllerSpeed; }
            set { _aniControllerSpeed = value; }
        }

        protected double _characterAttack = 1.0f;
        protected double _characterDefense = 1.0f;
        protected float _characterAttackSpeed = 1.0f;
        protected float _characterMoveSpeed = 1.0f;

        public float Temp_Accuracy = 1f;

        private System.Random _random = new System.Random();

        public bool _blockMove { get; private set; }
        protected bool _blockAttack { get; private set; }
        protected bool _blockSkill { get; private set; }

        public double FinalAttack => _characterAttack;
        public double FinalDefense => _characterDefense;
        public float FinalMoveSpeed => _characterMoveSpeed;
        public float FinalAttackSpeed => _characterAttackSpeed;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.AnimationEvent += SpineAnimationEvent;

            _conditionController = gameObject.AddComponent<CharacterConditionController>();
        }
        //------------------------------------------------------------------------------------
        public virtual void Init()
        {

        }
        //------------------------------------------------------------------------------------
        public virtual void Release()
        { 

        }
        //------------------------------------------------------------------------------------
        #region Skill System
        /// <summary>
        /// 스킬 시스템 초기화 (Init()에서 호출)
        /// </summary>
        protected void InitializeSkillSystem()
        {
            _skillChart = GameChart.Get<SkillChart>();

            // 플레이어인 경우 SkillManager에서 장착된 스킬 로드
            if (this is PlayerController)
            {
                LoadPlayerEquippedSkills();
                SkillManager.Instance.OnSkillSlotChanged += LoadPlayerEquippedSkills;
            }

            _nextSkillData = null;

            InitializeSkillCooldowns();
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 시스템 해제 (Release()에서 호출)
        /// </summary>
        protected void ReleaseSkillSystem()
        {
            _nextSkillData = null;

            if (SkillManager.Instance != null)
                SkillManager.Instance.OnSkillSlotChanged -= LoadPlayerEquippedSkills;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 플레이어의 장착된 스킬 목록 로드
        /// </summary>
        private void LoadPlayerEquippedSkills()
        {
            _equippedSkillIds = SkillManager.Instance.GetEquippedSkillIds();
            InitializeSkillCooldowns();
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// AI 캐릭터용 스킬 설정 (외부에서 호출)
        /// </summary>
        public void SetEquippedSkills(List<int> skillIds)
        {
            _equippedSkillIds = new List<int>(skillIds);
            InitializeSkillCooldowns();
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 쿨타임 정보 초기화
        /// </summary>
        private void InitializeSkillCooldowns()
        {
            _skillCooldowns.Clear();

            foreach (int skillId in _equippedSkillIds)
            {
                if (skillId <= 0)
                    continue;

                SkillInfo skillInfo = _skillChart?.GetActive(skillId);
                if (skillInfo == null)
                    continue;

                _skillCooldowns[skillId] = new SkillCooldownInfo
                {
                    skillId = skillId,
                    cooldownType = skillInfo.CooldownType,
                    cooldownValue = skillInfo.CooldownValue,
                    nextAvailableTime = 0f,
                    remainingAttackCount = 0
                };
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 시스템 업데이트 (Updated()에서 호출)
        /// </summary>
        protected void UpdateSkillSystem()
        {
            if (CharacterState == CharacterState.Dead)
                return;

            if (!AutoUseSkills)
                return;

            // 스킬 사용 차단 상태 체크
            if (_blockSkill)
                return;

            CharacterControllerBase target = AttackTarget;
            if (target == null || target.IsDead)
                return;

            if (_nextSkillData != null)
                return;

            // 장착된 스킬 중 사용 가능한 것 찾기
            foreach (var kvp in _skillCooldowns)
            {
                int skillId = kvp.Key;
                var cooldownInfo = kvp.Value;

                if (!cooldownInfo.IsReady())
                    continue;

                SkillInfo skillInfo = _skillChart?.GetActive(skillId);
                if (skillInfo == null)
                    continue;

                _nextSkillData = skillInfo;

                //// 거리 체크
                //float distance = MathDatas.GetDistance(transform.position, target.transform.position);
                //if (distance > DefaultSkillRange)
                //    continue;

                //// 스킬 사용
                //UseSkill(skillId, target);
                //break; // 한 프레임에 하나씩만
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 사용 (메인 메서드)
        /// </summary>
        public bool UseSkill(int skillId, CharacterControllerBase target)
        {
            if (IsDead || target == null || target.IsDead)
                return false;

            // 쿨타임 체크
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return false;

            if (!cooldownInfo.IsReady())
                return false;

            SkillInfo skillInfo = _skillChart?.GetActive(skillId);
            if (skillInfo == null)
                return false;

            // 스킬 레벨 가져오기 (플레이어만, AI는 레벨 1)
            int skillLevel = 1;
            if (this is PlayerController)
                skillLevel = SkillManager.Instance.GetSkillLevel(skillId);

            // 스킬 데미지 계산
            double attackMultiplier = skillInfo.GetFinalAttackMultiplier(skillLevel);
            int hitCount = skillInfo.HitCount;

            // ConditionData 적용
            var conditionIndexes = skillInfo.GetEnemyConditionIndexes();
            ApplySkillConditions(target, conditionIndexes);

            // 스킬 실행
            ExecuteSkillAttack(skillInfo, target, attackMultiplier, hitCount);

            // 쿨타임 시작
            cooldownInfo.StartCooldown();

            // 이벤트 발생
            OnSkillUsed?.Invoke(skillId);

            return true;
        }
        //------------------------------------------------------------------------------------
        public void StartCoolDown(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return;

            cooldownInfo.StartCooldown();

            OnSkillUsed?.Invoke(skillId);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 공격 실행
        /// </summary>
        private void ExecuteSkillAttack(SkillInfo skillInfo, CharacterControllerBase target, double attackMultiplier, int hitCount)
        {
            // AttackData 생성
            AttackData skillAttackData = new AttackData
            {
                Hitter = this,
                AttackRange = DefaultSkillRange,
                AttackDuration = 0.5f, // TODO: 스킬별 애니메이션 시간
                CustomParam = $"Skill_{skillInfo.SkillId}"
            };

            // TODO: SkillPlayer를 통해 스킬 실행
            // 실제 프로젝트에 맞게 구현

            Debug.Log($"[Skill] {name} used skill {skillInfo.SkillId} with {attackMultiplier:P0} on {target.name}");
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬의 ConditionData 효과 적용
        /// </summary>
        private void ApplySkillConditions(CharacterControllerBase target, IReadOnlyList<int> conditionIndexes)
        {
            if (target.CharacterConditionController == null)
                return;

            var conditionDataList = StaticResource.Instance.GetConditionData();

            foreach (int conditionIdx in conditionIndexes)
            {
                ConditionData conditionData = conditionDataList.GetData(conditionIdx);
                if (conditionData != null)
                {
                    target.CharacterConditionController.AddCondition(conditionData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 공격 횟수 기반 쿨타임 감소
        /// </summary>
        public void OnSkillOwnerAttack()
        {
            foreach (var cooldownInfo in _skillCooldowns.Values)
            {
                cooldownInfo.OnAttack();
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 특정 스킬이 사용 가능한지 체크
        /// </summary>
        public bool CanUseSkill(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return false;

            return cooldownInfo.IsReady();
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 남은 쿨타임 시간 (초)
        /// </summary>
        public float GetRemainingSkillCooldownTime(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return 0f;

            if (cooldownInfo.cooldownType != Enum_SkillCooldownType.Time)
                return 0f;

            return Mathf.Max(0f, cooldownInfo.nextAvailableTime - Time.time);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 남은 쿨타임 공격 횟수
        /// </summary>
        public int GetRemainingSkillCooldownAttackCount(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return 0;

            if (cooldownInfo.cooldownType != Enum_SkillCooldownType.AttackCount)
                return 0;

            return Mathf.Max(0, cooldownInfo.remainingAttackCount);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 쿨타임 강제 초기화 (치트/디버그용)
        /// </summary>
        public void ResetAllSkillCooldowns()
        {
            foreach (var cooldownInfo in _skillCooldowns.Values)
            {
                cooldownInfo.nextAvailableTime = 0f;
                cooldownInfo.remainingAttackCount = 0;
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 장착된 스킬 목록 반환
        /// </summary>
        public List<int> GetEquippedSkillIds()
        {
            return new List<int>(_equippedSkillIds);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 패시브 스킬 효과 적용 (플레이어 Init()에서 호출)
        /// </summary>
        protected void ApplyPassiveSkills()
        {
            // 플레이어만 SkillManager에서 패시브 스킬 가져옴
            if (this is PlayerController)
            {
                var passiveSkills = SkillManager.Instance.GetOwnedPassiveSkills();

                foreach (var passiveSkill in passiveSkills)
                {
                    var conditionIndexes = passiveSkill.GetEnemyConditionIndexes();

                    foreach (int conditionIdx in conditionIndexes)
                    {
                        var conditionData = StaticResource.Instance.GetConditionData().GetData(conditionIdx);
                        if (conditionData != null)
                        {
                            conditionData.Duration = -1f; // 영구 효과
                            CharacterConditionController?.AddCondition(conditionData);
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        public void SetControlLocks(bool move, bool attack, bool skill)
        {
            _blockMove = move;
            _blockAttack = attack;
            _blockSkill = skill;
        }
        //------------------------------------------------------------------------------------
        public void SetSpineModelData(SpineModelData spineModelData)
        {
            if (spineModelData == null)
                return;

            _currentSpineModelData = spineModelData;

            _mySkeletonAnimationHandler?.SetSpineModel(_currentSpineModelData);
        }
        //------------------------------------------------------------------------------------
        public void SetSpineSkin(Skin skin)
        {
            _mySkeletonAnimationHandler?.SetSkin(skin);
        }
        //------------------------------------------------------------------------------------
        public void ChangeSpineColor(Color color)
        {
            _mySkeletonAnimationHandler?.SetColor(color);
        }
        //------------------------------------------------------------------------------------
        public void RefreshCheatStat()
        {// 데이터가 없어서...
            List<StatViewer> TempPlayerStat = _iFFType == IFFType.IFF_Friend ? StaticResource.Instance.GetBattleModeStaticData().TempPlayerStat : StaticResource.Instance.GetBattleModeStaticData().TempMonsterStat;
            for (int i = 0; i < TempPlayerStat.Count; ++i)
            {
                _characterStatOperator.SetDefaultStat(TempPlayerStat[i].v2Enum_Stat, TempPlayerStat[i].value);
            }

            _characterStatOperator.RefreshOutputStatValue();
            RefreshStat(true);
        }
        //------------------------------------------------------------------------------------
        public void Damage(double damage)
        {
            if (IsDead == true)
                return;

            if (_conditionController.HasCondition(Enum_ConditionType.Invincible))
            {
                Debug.Log("무적으로 인해 데미지 안입음");
                return;
            }

            DeCreaseHP(damage);


            if (CurrentHP <= 0)
                ChangeState(CharacterState.Dead);
            else
            {
                OnDamage();
            }
        }
        //------------------------------------------------------------------------------------
        public void Damage(AttackStruct damage)
        {
            if (IsDead == true)
                return;

            if (damage.Hitter != null && damage.Hitter.IsDead == false && damage.SkillInfo != null)
            {
                for (int i = 0; i < damage.SkillInfo.HitCount; ++i)
                {
                    bool ishit = Random.Range(0.0f, 1.0f) <= damage.Hitter.Temp_Accuracy;
                    if (ishit == false)
                    {
                        CombatTextSpawner.Instance.ShowMiss(transform);
                        return;
                    }

                    double setdamage = damage.SkillInfo.GetFinalAttackMultiplier(damage.AttackLevel) * damage.Hitter.FinalAttack;

                    bool critical = damage.Hitter.ApplyCritical();
                    if (critical == true)
                        setdamage = setdamage * damage.Hitter.GetOutPutMyStat(Enum_Stat.CritDmg_Inc);

                    setdamage *= damage.Hitter.GetMinMaxRatio();

                    setdamage = System.Math.Truncate(setdamage);

                    if (_iFFType == IFFType.IFF_Foe)
                    {
                        CombatTextSpawner.Instance.ShowDamage(transform, setdamage, critical);

                        if (StaticResource.Instance.GetBattleModeStaticData().CriticalAttackShake == true)
                        {
                            Managers.BattleSceneManager.Instance.PlayCameraShake(
                                StaticResource.Instance.GetBattleModeStaticData().CriticalAttackShake_strengthOverride,
                                StaticResource.Instance.GetBattleModeStaticData().CriticalAttackShake_durationOverride);
                        }
                    }

                    Damage(setdamage);
                }

                if (IsDead == false)
                {
                    if (_attackTarget == null)
                        _attackTarget = damage.Hitter;
                    PlayCharacterCondition(damage.SkillInfo.GetEnemyConditionIndexes(), damage.Hitter.transform.position);
                }
                else
                {
                    damage.Hitter.OnKillCharacter(this);
                }

                damage.Hitter.OnHitCharacter(this);
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnDamage()
        { 

        }
        //------------------------------------------------------------------------------------
        public virtual void OnKillCharacter(CharacterControllerBase characterControllerBase)
        { 

        }
        //------------------------------------------------------------------------------------
        public virtual void OnHitCharacter(CharacterControllerBase characterControllerBase)
        {

        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(AttackStruct attackData, Vector3 pos)
        {
            if (attackData.SkillInfo != null)
            {
                PlayCharacterCondition(attackData.SkillInfo.GetMyConditionIndexes(), pos);
            }

            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, null);
        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(AttackStruct attackData, Vector3 pos, CharacterControllerBase fixSkillHitReceiver)
        {
            if (attackData.SkillInfo != null)
            {
                PlayCharacterCondition(attackData.SkillInfo.GetMyConditionIndexes(), pos);
            }

            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, fixSkillHitReceiver);
        }
        //------------------------------------------------------------------------------------
        private void PlayCharacterCondition(IReadOnlyList<int> index, Vector3 attackpos)
        {
            for (int i = 0; i < index.Count; ++i)
            {
                PlayCharacterCondition(index[i], attackpos);
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayCharacterCondition(int index, Vector3 attackpos)
        {
            ConditionData conditionData = StaticResource.Instance.GetConditionData().GetData(index);
            conditionData.EffectPos = attackpos;

            PlayCharacterCondition(conditionData);
        }
        //------------------------------------------------------------------------------------
        public void PlayCharacterCondition(ConditionData conditionData)
        {
            if (conditionData == null)
                return;

            _conditionController?.AddCondition(conditionData);
        }
        //------------------------------------------------------------------------------------
        public void RemoveConditionsByType(Enum_ConditionType enum_ConditionType)
        {
            _conditionController?.RemoveConditionsByType(enum_ConditionType);
        }
        //------------------------------------------------------------------------------------
        public void Play()
        {
            Managers.AggroManager.Instance.AddIFFCharacterAggro(this);
            _blockMove = false;
            _blockAttack = false;
            _blockSkill = false;
            OnPlay();
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnPlay()
        {

        }
        //------------------------------------------------------------------------------------
        protected virtual void SpineAnimationEvent(string aniName, string eventName)
        {

        }
        //------------------------------------------------------------------------------------
        public virtual Vector3 GetMoveDirection()
        { // MoveController_Base���� �ַ� ȣ��
            // ������ ���̽�ƽ���� ������ ���� ���� �־ �����Լ��� ����

            if (AttackTarget == null)
                return Vector3.zero;

            return (AttackTarget.transform.position - transform.position).normalized;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                RefreshCheatStat();
            }

            Updated();

            if (_characterState != CharacterState.Dead
                && _characterState != CharacterState.None)
            {
                double recoveryvalue = GetOutPutMyStat(Enum_Stat.HpRecovery);

                InCreaseHP(recoveryvalue * MaxHP * Time.deltaTime);
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void InCreaseHP(double hp)
        {
            SetHP(_currentHP + hp);
        }
        //------------------------------------------------------------------------------------
        protected void DeCreaseHP(double hp)
        {
            if (IFFType == IFFType.IFF_Friend)
            {
                if (Managers.GameSettingManager.Instance.Cheat_NoDamage() == true)
                {
                    return;
                }
            }

            double decreaseValue = hp;

            if (decreaseValue <= 0)
                return;

            double beforeHp = _currentHP;

            SetHP(_currentHP - decreaseValue);
        }
        //------------------------------------------------------------------------------------
        protected void SetHP(double hp)
        {
            _currentHP = hp;

            if (_currentHP < 0)
                _currentHP = 0;

            if (_currentHP > _maxHP)
                _currentHP = _maxHP;

            if (_maxHP == 0)
                return;

            double hpratio = _currentHP / _maxHP;

            _uiCharacterState?.SetHPBar(hpratio);
        }
        //------------------------------------------------------------------------------------
        public void ChangeCharacterState(CharacterState state)
        { // �ܺο��� ���� ��
            ChangeState(state);
        }
        //------------------------------------------------------------------------------------
        protected virtual void ChangeState(CharacterState state, bool playAni = true)
        {
            if (_characterState == state)
                return;

            _characterState = state;

            switch (state)
            {
                case CharacterState.Attack:
                case CharacterState.Skill:
                    {
                        _mySkeletonAnimationHandler?.SetAnimationSpeed(FinalAttackSpeed);
                        break;
                    }
                case CharacterState.Run:
                    {
                        _mySkeletonAnimationHandler?.SetAnimationSpeed(_characterMoveSpeed);
                        break;
                    }
                default:
                    {
                        _mySkeletonAnimationHandler?.SetAnimationSpeed(1);
                        break;
                    }
            }

            PlayAnimation(state);
            if (state == CharacterState.Dead)
            {
                Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);
                OnDead();
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnDead()
        { 
        }
        //------------------------------------------------------------------------------------
        protected virtual void PlayAnimation(CharacterState state)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.PlayAnimation_Once(state, true);
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayAnimation_AniName(string aniName, bool loop = true)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.PlayAnimation_Once(aniName, loop);
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void Updated()
        {

        }
        //------------------------------------------------------------------------------------
        public void ChangeCharacterLookAtDirection(Enum_LookDirection direction)
        {
            if (direction == _lookDirection)
                return;

            _lookDirection = direction;
            Vector3 rotate = transform.eulerAngles;

            float selectRatote = 0.0f;

            if (_lookDirection == Enum_LookDirection.Right)
                selectRatote = 180.0f;

            rotate.y = selectRatote;

            transform.eulerAngles = rotate;

            _characterBillboardController?.RefreshBillboard();
        }
        //------------------------------------------------------------------------------------
        public void ChangeCharacterLookAtDirection_Target(Transform targetTrans)
        {
            Vector3 direction = targetTrans.transform.position - transform.position;
            direction.Normalize();

            ChangeCharacterLookAtDirection(direction.x < 0 ? Enum_LookDirection.Left : Enum_LookDirection.Right);
        }
        //------------------------------------------------------------------------------------
        public void SetNewTarget()
        {
            _attackTarget = Managers.AggroManager.Instance.GetIFFTargetCharacter(this);
        }
        //------------------------------------------------------------------------------------
        public virtual double GetOutPutMyStat(Enum_Stat v2Enum_Stat)
        {
            return _characterStatOperator.GetOutPutMyStat(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public void RefreshStat(bool setFullHp = false)
        {
            _characterStatOperator.RefreshOutputStatValue();

            _characterAttack = GetOutPutMyStat(Enum_Stat.Attack);
            _characterAttack += _characterAttack * GetOutPutMyStat(Enum_Stat.Attack_Inc);
            _characterAttack += _characterAttack * GetOutPutMyStat(Enum_Stat.FinalDamage);

            _characterDefense = GetOutPutMyStat(Enum_Stat.Defence);
            _characterDefense += _characterDefense * GetOutPutMyStat(Enum_Stat.Defence_Inc);

            _characterMoveSpeed = (float)(GetOutPutMyStat(Enum_Stat.MoveSpeed));
            _characterMoveSpeed += _characterMoveSpeed * (float)(GetOutPutMyStat(Enum_Stat.MoveSpeed_Inc));

            _characterAttackSpeed = (float)(GetOutPutMyStat(Enum_Stat.AttackSpeed));
            _characterAttackSpeed += _characterAttackSpeed * (float)(GetOutPutMyStat(Enum_Stat.AttackSpeed_Inc));

            if(CharacterState == CharacterState.Attack
                || CharacterState == CharacterState.Skill)
                _mySkeletonAnimationHandler?.SetAnimationSpeed(FinalAttackSpeed);
            else if (CharacterState == CharacterState.Run)
                _mySkeletonAnimationHandler?.SetAnimationSpeed(_characterMoveSpeed);

            double currHpRatio = 0;

            if (_maxHP <= 0)
                currHpRatio = 0;
            else
                currHpRatio = _currentHP / _maxHP;

            double hp = GetOutPutMyStat(Enum_Stat.HP);

            _maxHP = hp + (hp * GetOutPutMyStat(Enum_Stat.Hp_Inc));

            if (setFullHp == true)
                _currentHP = _maxHP;
            else
                _currentHP = _maxHP * currHpRatio;

            
        }
        //------------------------------------------------------------------------------------
        public bool ApplyCritical()
        {
            return Random.Range(0.0f, 1.0f) <= (float)GetOutPutMyStat(Enum_Stat.CritChance);
        }
        //------------------------------------------------------------------------------------
        public double GetMinMaxRatio()
        {
            double min = GetOutPutMyStat(Enum_Stat.MinDamagePer);
            double max = GetOutPutMyStat(Enum_Stat.MaxDamagePer);

            if (min >= max)
                return min;

            double value = min + ((max - min) * _random.NextDouble());

            return value;
        }
        //------------------------------------------------------------------------------------
    }
}

