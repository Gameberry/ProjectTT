using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Managers;
using Spine;
using Spine.Unity;
using GameBerry.Chart;
using UnityEngine.TextCore.Text;
using UnityEngine.AdaptivePerformance.Provider;

namespace GameBerry
{
    /// <summary>
    /// 캐릭터의 기본 컨트롤러 클래스
    /// 플레이어와 NPC 캐릭터의 공통 기능을 담당합니다.
    /// - 스켈레톤 애니메이션 (Spine) 관리
    /// - 스킬 시스템 구현
    /// - 체력 및 스탯 관리
    /// - 조건 상태 (상태이상) 처리
    /// </summary>
    public class CharacterControllerBase : MonoBehaviour
    {
        //------------------------------------------------------------------------------------
        // [ 기본 상태 정보 ]
        //------------------------------------------------------------------------------------
        /// <summary> 캐릭터가 바라보는 방향 (좌측/우측) </summary>
        public Enum_LookDirection LookDirection { get { return _lookDirection; } }
        [SerializeField]
        protected Enum_LookDirection _lookDirection = Enum_LookDirection.Right;

        /// <summary> 캐릭터의 빌보드 (카메라 방향 유지) 컨트롤러 </summary>
        [SerializeField]
        private CharacterBillboardController _characterBillboardController;

        /// <summary> 캐릭터의 상태이상(버프/디버프) 관리 컨트롤러 </summary>
        [SerializeField]
        private CharacterConditionController _conditionController;
        public CharacterConditionController CharacterConditionController { get { return _conditionController; } }


        /// <summary> 캐릭터의 팀 정보 (아군/적군/중립) </summary>
        [SerializeField]
        protected IFFType _iFFType = IFFType.IFF_None;

        public IFFType IFFType { get { return _iFFType; } }

        /// <summary> 캐릭터의 현재 상태 (대기/이동/공격/피격/사망 등) </summary>
        [SerializeField]
        protected CharacterState _characterState = CharacterState.None;
        public CharacterState CharacterState { get { return _characterState; } }

        /// <summary> Spine 애니메이션 핸들러 </summary>
        [SerializeField] protected SkeletonAnimationHandler _mySkeletonAnimationHandler;

        /// <summary> 현재 적용 중인 Spine 모델 데이터 </summary>
        [SerializeField] protected SpineModelData _currentSpineModelData;

        /// <summary> UI에 표시되는 캐릭터 상태 </summary>
        [SerializeField]
        protected UICharacterState _uiCharacterState;

        /// <summary> 캐릭터 사망 여부 </summary>
        public bool IsDead { get { return CharacterState == CharacterState.Dead; } }


        /// <summary> 캐릭터의 물리 엔진 컴포넌트 </summary>
        [SerializeField]
        protected Rigidbody _rigidbody;
        public Rigidbody MyRigidbody { get { return _rigidbody; } }

        /// <summary> 현재 공격 대상 캐릭터 </summary>
        [SerializeField]
        protected CharacterControllerBase _attackTarget;
        public CharacterControllerBase AttackTarget { get { return _attackTarget; } }



        //------------------------------------------------------------------------------------
        // [ 스킬 시스템 필드 ]
        //------------------------------------------------------------------------------------
        #region Skill System Fields
        /// <summary> 스킬 실행 플레이어 </summary>
        [SerializeField]
        protected SkillPlayer _skillPlayer;
        protected virtual Enum_SkillActorType SkillActorType => Enum_SkillActorType.Player;
        /// <summary> 다음에 사용 예정인 스킬 정보 </summary>
        protected SkillInfo _nextSkillData = null;

        /// <summary> 게임의 스킬 데이터 차트 </summary>
        private SkillChart _skillChart;

        /// <summary> 캐릭터가 장착한 스킬 ID 목록 </summary>
        private List<int> _equippedSkillIds = new List<int>();




        /// <summary> 스킬 쿨타임 정보를 저장하는 내부 클래스 </summary>
        private class SkillCooldownInfo
        {
            /// <summary> 스킬 ID </summary>
            public int skillId;
            /// <summary> 쿨타임 유형 (시간/공격 횟수) </summary>
            public Enum_SkillCooldownType cooldownType;
            /// <summary> 스킬 재사용 가능 시간 </summary>
            public float nextAvailableTime;
            /// <summary> 남은 공격 횟수 (공격 횟수 기반 쿨타임인 경우) </summary>
            public int remainingAttackCount;
            /// <summary> 쿨타임 값 </summary>
            public float cooldownValue;

            /// <summary> 스킬 사용 준비 완료 여부 </summary>
            public bool IsReady()
            {
                if (cooldownType == Enum_SkillCooldownType.Time)
                    return Time.time >= nextAvailableTime;
                else if (cooldownType == Enum_SkillCooldownType.AttackCount)
                    return remainingAttackCount <= 0;

                return true;
            }

            /// <summary> 쿨타임 시작 </summary>
            public void StartCooldown()
            {
                if (cooldownType == Enum_SkillCooldownType.Time)
                    nextAvailableTime = Time.time + cooldownValue;
                else if (cooldownType == Enum_SkillCooldownType.AttackCount)
                    remainingAttackCount = Mathf.CeilToInt(cooldownValue);
            }

            /// <summary> 공격 시 호출 (공격 횟수 감소) </summary>
            public void OnAttack()
            {
                if (cooldownType == Enum_SkillCooldownType.AttackCount && remainingAttackCount > 0)
                    remainingAttackCount--;
            }
        }

        /// <summary> 스킬별 쿨타임 정보 딕셔너리 </summary>
        private Dictionary<int, SkillCooldownInfo> _skillCooldowns = new Dictionary<int, SkillCooldownInfo>();

        /// <summary> 자동 스킬 사용 여부 </summary>
        public bool AutoUseSkills { get; set; } = true;
        /// <summary> 기본 스킬 사거리 </summary>
        public float DefaultSkillRange { get; set; } = 3f;

        /// <summary> 스킬 사용 이벤트 (스킬 ID를 파라미터로 전달) </summary>
        public event System.Action<int> OnSkillUsed;

        #endregion

        //------------------------------------------------------------------------------------
        // [ 스탯 및 체력 ]
        //------------------------------------------------------------------------------------
#if UNITY_EDITOR
        [SerializeField]
#endif
        /// <summary> 캐릭터의 모든 스탯을 관리하는 오퍼레이터 </summary>
        protected CharacterStatOperator _characterStatOperator = new CharacterStatOperator();
        public CharacterStatOperator CharacterStatOperator { get { return _characterStatOperator; } }


        /// <summary> 캐릭터의 최대 체력 </summary>
        [SerializeField]
        protected double _maxHP = 0.0;
        public double MaxHP { get { return _maxHP; } }

        /// <summary> 캐릭터의 현재 체력 </summary>
        [SerializeField]
        protected double _currentHP = 0.0;
        public double CurrentHP { get { return _currentHP; } }
        public event System.Action<double, double> OnHpChanged;

        /// <summary> 애니메이션 컨트롤러의 재생 속도 (1.0 = 기본 속도) </summary>
        [SerializeField]
        protected float _aniControllerSpeed = 1.0f;
        public float AniControllerSpeed
        {
            get { return _aniControllerSpeed; }
            set { _aniControllerSpeed = value; }
        }

        /// <summary> 캐릭터의 공격력 </summary>
        protected double _characterAttack = 1.0f;
        /// <summary> 캐릭터의 방어력 </summary>
        protected double _characterDefense = 1.0f;
        /// <summary> 캐릭터의 공격 속도 </summary>
        protected float _characterAttackSpeed = 1.0f;
        /// <summary> 캐릭터의 이동 속도 </summary>
        protected float _characterMoveSpeed = 1.0f;

        /// <summary> 난수 생성기 </summary>
        private System.Random _random = new System.Random();

        //------------------------------------------------------------------------------------
        // [ 행동 제어 플래그 ]
        //------------------------------------------------------------------------------------
        /// <summary> 이동 차단 여부 </summary>
        public bool _blockMove { get; private set; }
        /// <summary> 공격 차단 여부 </summary>
        protected bool _blockAttack { get; private set; }
        /// <summary> 스킬 사용 차단 여부 </summary>
        protected bool _blockSkill { get; private set; }

        /// <summary> 모든 버프/디버프를 적용한 최종 공격력 </summary>
        public double FinalAttack => _characterAttack;
        /// <summary> 모든 버프/디버프를 적용한 최종 방어력 </summary>
        public double FinalDefense => _characterDefense;
        /// <summary> 모든 버프/디버프를 적용한 최종 이동 속도 </summary>
        public float FinalMoveSpeed => _characterMoveSpeed;
        /// <summary> 모든 버프/디버프를 적용한 최종 공격 속도 </summary>
        public float FinalAttackSpeed => _characterAttackSpeed;

        //------------------------------------------------------------------------------------
        // [ 생명주기 ]
        //------------------------------------------------------------------------------------
        /// <summary> Unity 초기화 시 호출 </summary>
        private void Awake()
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.AnimationEvent += SpineAnimationEvent;

            _conditionController = gameObject.AddComponent<CharacterConditionController>();
        }
        //------------------------------------------------------------------------------------
        /// <summary> 캐릭터 초기화 (파생 클래스에서 오버라이드) </summary>
        public virtual void Init()
        {

        }
        //------------------------------------------------------------------------------------
        /// <summary> 캐릭터 해제 (파생 클래스에서 오버라이드) </summary>
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
            _equippedSkillIds = SkillManager.Instance.GetEquippedSkillIds(SkillActorType);
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
        /// 쿨타임 정보 초기화 (기존 쿨타임 정보 보존)
        /// </summary>
        private void InitializeSkillCooldowns()
        {
            // 제거된 스킬의 쿨타임 정보 삭제
            var skillIdsToRemove = new List<int>();
            foreach (var skillId in _skillCooldowns.Keys)
            {
                if (!_equippedSkillIds.Contains(skillId))
                    skillIdsToRemove.Add(skillId);
            }

            foreach (var skillId in skillIdsToRemove)
            {
                _skillCooldowns.Remove(skillId);
            }

            // 새로 추가된 스킬만 쿨타임 정보 생성 (기존 스킬은 유지)
            foreach (int skillId in _equippedSkillIds)
            {
                if (skillId <= 0)
                    continue;

                // 이미 쿨타임 정보가 있으면 건너뛰기 (기존 쿨타임 보존!)
                if (_skillCooldowns.ContainsKey(skillId))
                    continue;

                SkillInfo skillInfo = _skillChart?.GetActive(skillId, SkillActorType);
                if (skillInfo == null)
                    continue;

                // 새로운 스킬만 쿨타임 정보 생성
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

            //if (_nextSkillData != null)
            //    return;

            // 장착된 스킬 중 사용 가능한 것 찾기
            foreach (var kvp in _skillCooldowns)
            {
                int skillId = kvp.Key;
                var cooldownInfo = kvp.Value;

                if (!cooldownInfo.IsReady())
                    continue;

                SkillInfo skillInfo = _skillChart?.GetActive(skillId, SkillActorType);
                if (skillInfo == null)
                    continue;

                // 거리 체크
                float distance = MathDatas.GetDistance(transform.position.x, transform.position.z, target.transform.position.x, target.transform.position.z);
                if (distance > skillInfo.AttackRange)
                    continue;

                _nextSkillData = skillInfo;

                //// 스킬 사용
                //UseSkill(skillId, target);
                //break; // 한 프레임에 하나씩만
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 사용 (메인 메서드)
        /// </summary>
        /// <param name="skillId">사용할 스킬의 ID</param>
        /// <param name="target">대상 캐릭터</param>
        /// <returns>스킬 사용 성공 여부</returns>
        public bool UseSkill(int skillId, CharacterControllerBase target)
        {
            if (IsDead || target == null || target.IsDead)
                return false;

            // 쿨타임 체크
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return false;

            if (!cooldownInfo.IsReady())
                return false;

            SkillInfo skillInfo = _skillChart?.GetActive(skillId, SkillActorType);
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
        /// <summary>
        /// 스킬 쿨타임 시작 (외부에서 쿨타임 강제 시작)
        /// </summary>
        /// <param name="skillId">쿨타임을 시작할 스킬의 ID</param>
        public void StartCoolDown(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return;

            cooldownInfo.StartCooldown();

            OnSkillUsed?.Invoke(skillId);
        }
        /// <summary>
        /// 스킬 공격 실행
        /// </summary>
        /// <param name="skillInfo">스킬 정보</param>
        /// <param name="target">대상 캐릭터</param>
        /// <param name="attackMultiplier">공격 배수 (레벨별 계산됨)</param>
        /// <param name="hitCount">명중 횟수</param>
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
        /// <summary>
        /// 스킬의 ConditionData 효과 적용 (상태이상 등)
        /// </summary>
        /// <param name="target">대상 캐릭터</param>
        /// <param name="conditionIndexes">적용할 상태이상 인덱스 목록</param>
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
        /// <summary>
        /// 공격 횟수 기반 쿨타임 감소
        /// 희망 스킬은 캐릭터가 공격할 때마다 쿨타임 카운트가 감소합니다.
        /// </summary>
        public void OnSkillOwnerAttack()
        {
            foreach (var cooldownInfo in _skillCooldowns.Values)
            {
                cooldownInfo.OnAttack();
            }
        }
        /// <summary>
        /// 특정 스킬이 사용 가능한지 체크 (쿨타임 확인)
        /// </summary>
        /// <param name="skillId">확인할 스킬의 ID</param>
        /// <returns>스킬 사용 가능 여부</returns>
        public bool CanUseSkill(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return false;

            return cooldownInfo.IsReady();
        }
        /// <summary>
        /// 남은 쿨타임 시간 (초)
        /// </summary>
        /// <param name="skillId">확인할 스킬의 ID</param>
        /// <returns>남은 쿨타임 시간 (시간 기반 쿨타임이 아니면 0)</returns>
        public float GetRemainingSkillCooldownTime(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return 0f;

            if (cooldownInfo.cooldownType != Enum_SkillCooldownType.Time)
                return 0f;

            return Mathf.Max(0f, cooldownInfo.nextAvailableTime - Time.time);
        }
        /// <summary>
        /// 남은 쿨타임 공격 횟수
        /// </summary>
        /// <param name="skillId">확인할 스킬의 ID</param>
        /// <returns>남은 쿨타임 공격 횟수 (공격 횟수 기반 쿨타임이 아니면 0)</returns>
        public int GetRemainingSkillCooldownAttackCount(int skillId)
        {
            if (!_skillCooldowns.TryGetValue(skillId, out var cooldownInfo))
                return 0;

            if (cooldownInfo.cooldownType != Enum_SkillCooldownType.AttackCount)
                return 0;

            return Mathf.Max(0, cooldownInfo.remainingAttackCount);
        }
        /// <summary>
        /// 모든 스킬 쿨타임 강제 초기화
        /// (치트/디버그/테스트용)
        /// </summary>
        public void ResetAllSkillCooldowns()
        {
            foreach (var cooldownInfo in _skillCooldowns.Values)
            {
                cooldownInfo.nextAvailableTime = 0f;
                cooldownInfo.remainingAttackCount = 0;
            }
        }
        /// <summary>
        /// 장착된 스킬 목록 반환
        /// </summary>
        /// <returns>장착된 스킬ID 목록의 복사본</returns>
        public List<int> GetEquippedSkillIds()
        {
            return new List<int>(_equippedSkillIds);
        }
        /// <summary>
        /// 패시브 스킬 효과 적용 (플레이어 Init()에서 호출)
        /// 패시브 스킬의 상태이상 효과를 캐릭터에 적용합니다 (영구 효과).
        /// </summary>
        protected void ApplyPassiveSkills()
        {
            // 플레이어만 SkillManager에서 패시브 스킬 가져옴
            if (this is PlayerController)
            {
                var passiveSkills = SkillManager.Instance.GetOwnedPassiveSkills(SkillActorType);

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
        // [ 행동 제어 및 모델 관리 ]
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 캐릭터의 행동 차단 설정 (이동/공격/스킬)
        /// </summary>
        /// <param name="move">이동 차단 여부</param>
        /// <param name="attack">공격 차단 여부</param>
        /// <param name="skill">스킬 사용 차단 여부</param>
        public void SetControlLocks(bool move, bool attack, bool skill)
        {
            _blockMove = move;
            _blockAttack = attack;
            _blockSkill = skill;

            if (IsDead == true)
                return;

            if (_blockMove == true && _blockAttack == true && _blockSkill == true)
            {
                ReleaseAttack();
                ChangeState(CharacterState.Hit);
            }
            else if (_blockMove == false || _blockAttack == false || _blockSkill == false)
            {
                if (CharacterState == CharacterState.Hit)
                    ChangeState(CharacterState.Idle);
            }
            else if (_blockAttack == true)
            {
                if (CharacterState != CharacterState.Attack)
                    ReleaseAttack();
            }
            else if (_blockSkill == true)
            {
                if (CharacterState == CharacterState.Skill)
                    ReleaseAttack();
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void ReleaseAttack()
        {
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Spine 애니메이션 모델 데이터 설정
        /// </summary>
        /// <param name="spineModelData">설정할 모델 데이터</param>
        public void SetSpineModelData(SpineModelData spineModelData)
        {
            if (spineModelData == null)
                return;

            _currentSpineModelData = spineModelData;

            _mySkeletonAnimationHandler?.SetSpineModel(_currentSpineModelData);
        }
        /// <summary>
        /// Spine 스킨 설정
        /// </summary>
        /// <param name="skin">설정할 스킨</param>
        public void SetSpineSkin(Skin skin)
        {
            _mySkeletonAnimationHandler?.SetSkin(skin);
        }
        /// <summary>
        /// Spine 애니메이션 색상 변경
        /// </summary>
        /// <param name="color">적용할 색상</param>
        public void ChangeSpineColor(Color color)
        {
            _mySkeletonAnimationHandler?.SetColor(color);
        }
        //------------------------------------------------------------------------------------
        public SkeletonAnimation GetSkeletonAnimation()
        {
            return _mySkeletonAnimationHandler?._skeletonAnimation;
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
                float hitChance = CalcHitRate(damage.Hitter.CharacterStatOperator, CharacterStatOperator);

                bool hasHit = false;

                for (int i = 0; i < damage.SkillInfo.HitCount; ++i)
                {
                    bool ishit = Random.Range(0.0f, 1.0f) <= hitChance;
                    if (ishit == false)
                    {
                        CombatTextSpawner.Instance.ShowMiss(transform, SkillManager.Instance.GetIcon(damage.SkillInfo.SkillId));
                        continue;
                    }

                    hasHit = true;

                    double setdamage = damage.SkillInfo.GetFinalAttackMultiplier(damage.AttackLevel) * damage.Hitter.FinalAttack;

                    bool critical = damage.Hitter.ApplyCritical();
                    if (critical == true)
                        setdamage = setdamage * damage.Hitter.GetOutPutMyStat(Enum_Stat.CritDmg_Inc);

                    setdamage *= damage.Hitter.GetMinMaxRatio();

                    setdamage = System.Math.Truncate(setdamage);

                    if (_iFFType == IFFType.IFF_Foe)
                    {
                        CombatTextSpawner.Instance.ShowDamage(transform, setdamage, critical, SkillManager.Instance.GetIcon(damage.SkillInfo.SkillId));

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
                    if (hasHit)
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
        public float CalcHitRate(CharacterStatOperator attacker, CharacterStatOperator defender)
        {
            double accuracy = attacker.GetOutPutMyStat(Enum_Stat.Accuracy);
            double evasion = defender.GetOutPutMyStat(Enum_Stat.Evasion);

            float bonus = (float)(accuracy / (accuracy + evasion + 1.0));

            return Mathf.Clamp(bonus, 0.05f, 0.99f);
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
                CharacterControllerBase myConditionReceiver = GetMyConditionReceiver(attackData);
                if (myConditionReceiver != null)
                    myConditionReceiver.PlayCharacterCondition(attackData.SkillInfo.GetMyConditionIndexes(), pos);
            }

            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, null);
        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(AttackStruct attackData, Vector3 pos, CharacterControllerBase fixSkillHitReceiver)
        {
            if (attackData.SkillInfo != null)
            {
                CharacterControllerBase myConditionReceiver = GetMyConditionReceiver(attackData);
                if (myConditionReceiver != null)
                    myConditionReceiver.PlayCharacterCondition(attackData.SkillInfo.GetMyConditionIndexes(), pos);
            }

            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, fixSkillHitReceiver);
        }
        //------------------------------------------------------------------------------------
        protected virtual CharacterControllerBase GetMyConditionReceiver(AttackStruct attackData)
        {
            return this;
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
        /// <summary>
        /// 캐릭터 게임시작 콜백 (오버라이드 권장)
        /// </summary>
        protected virtual void OnPlay()
        {

        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Spine 애니메이션 이벤트 핸들러 (오버라이드 권장)
        /// </summary>
        /// <param name="aniName">재생된 애니메이션 이름</param>
        /// <param name="eventName">발생한 이벤트 이름</param>
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

            if (Input.GetKey(KeyCode.T))
            {
                if(_iFFType == IFFType.IFF_Foe)
                {
                    Damage(MaxHP * 2);
                }
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
        /// <summary>
        /// 체력 증가 처리 (overridable)
        /// </summary>
        /// <param name="hp">증가할 체력</param>
        protected virtual void InCreaseHP(double hp)
        {
            SetHP(_currentHP + hp);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 체력 감소 처리 (데미지 적용)
        /// 사기 모드에서 플레이어 무적 처리
        /// </summary>
        /// <param name="hp">감소할 체력</param>
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
        /// <summary>
        /// 캐릭터의 현재 체력 설정 (0~MaxHP 범위로 정규화)
        /// UI 체력바 업데이트
        /// </summary>
        /// <param name="hp">설정할 체력</param>
        protected void SetHP(double hp)
        {
            _currentHP = hp;

            if (_currentHP < 0)
                _currentHP = 0;

            if (_currentHP > _maxHP)
                _currentHP = _maxHP;

            double hpratio = 0;
            if (_maxHP > 0)
                hpratio = _currentHP / _maxHP;

            _uiCharacterState?.SetHPBar(hpratio);
            OnHpChanged?.Invoke(_currentHP, _maxHP);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 캐릭터 상태 변경 (외부 호출용)
        /// </summary>
        /// <param name="state">변경할 상태</param>
        public void ChangeCharacterState(CharacterState state)
        { // �ܺο��� ���� ��
            ChangeState(state);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 캐릭터 상태 변경 (내부 구현)
        /// 애니메이션 속도 조정 및 상태 이벤트 발생
        /// </summary>
        /// <param name="state">변경할 상태</param>
        /// <param name="playAni">애니메이션 재생 여부</param>
        protected virtual void ChangeState(CharacterState state, bool playAni = true)
        {
            if (state == CharacterState.Run && _blockMove == true)
                state = CharacterState.Idle;

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
        /// <summary>
        /// 캐릭터 사망 콜백 (overridable)
        /// Aggro 목록에서 제거됨
        /// </summary>
        protected virtual void OnDead()
        {
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 캐릭터 상태에 따른 애니메이션 재생 (overridable)
        /// </summary>
        /// <param name="state">재생할 상태별 애니메이션</param>
        protected virtual void PlayAnimation(CharacterState state)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.PlayAnimation_Once(state, true);
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 특정 애니메이션 이름으로 재생
        /// </summary>
        /// <param name=\"aniName\">재생할 애니메이션 이름 (Spine 내 정의된 이름)</param>
        /// <param name=\"loop\">반복 여부 (기본값: true)</param>
        public void PlayAnimation_AniName(string aniName, bool loop = true)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.PlayAnimation_Once(aniName, loop);
            }
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 파생 클래스에서 구현할 커스텀 업데이트 (overridable)
        /// </summary>
        protected virtual void Updated()
        {

        }
        //------------------------------------------------------------------------------------
        // [ 캐릭터 방향 및 타겟 관리 ]
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 캐릭터 바라보는 방향 변경 (좌측/우측)
        /// </summary>
        /// <param name=\"direction\">변경할 방향</param>
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
        /// <summary>
        /// 특정 트랜스폼을 바라보는 방향으로 변경
        /// </summary>
        /// <param name=\"targetTrans\">바라볼 목표 위치</param>
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
        /// <summary>
        /// 캐릭터의 속성, 능력치 요정 계산 (특정 스킬 사용)
        /// Attack, Defense, MoveSpeed, AttackSpeed 를 모두 계산
        /// </summary>
        public virtual double GetOutPutMyStat(Enum_Stat v2Enum_Stat)
        {
            return _characterStatOperator.GetOutPutMyStat(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 모든 스킬 및 버프/디버프를 반영한 최종 스킬 계산
        /// 체력 또는 목표 체력 비율에 따라 최대 체력 설정
        /// </summary>
        /// <param name="setFullHp">전체 체력 설정 여부</param>
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

            if (CharacterState == CharacterState.Attack
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

            SetHP(_currentHP);
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

