using System.Collections;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Chart;

namespace GameBerry
{
    public class MonsterController : CharacterControllerBase
    {
        [SerializeField]
        private float attackRangeDefault = 1.5f;

        [SerializeField]
        private float attackRange = 1.0f;

        private CancellationTokenSource disableCancellation = new CancellationTokenSource();

        private BattleSceneMap_Aggro _battleSceneMap_Aggro;
        private System.Action<MonsterController> _onDeadCallback;
        private MonsterChart _monsterChart;

        private Vector3 _spawnPos;
        private bool _isDeadHandling = false;
        private Coroutine _delayedPoolCoroutine;
        private Collider[] _cachedColliders;

        private bool _isWandering = false;
        private Vector3 _wanderTargetPos;
        private float _nextWanderStartTime = 0f;
        private bool _isReturningToSpawn = false;
        private PlayerController _slotOwnerPlayer = null;
        private Vector3 _reservedAttackSlotPos = Vector3.zero;
        private int _reservedSlotRingIndex = -1;
        private bool _hasReservedAttackSlot = false;
        private float _nextSlotSyncTime = 0f;
        private float _nextFrontRowRetryTime = 0f;

        private const float AttackSlotReachThreshold = 0.25f;
        private const float SlotSyncInterval = 0.15f;
        private const float FrontRowRetryInterval = 0.5f;

        private void OnDisable()
        {
            ReleaseAttackSlotReservation();
        }

        private void ResetDamageCancellation()
        {
            if (disableCancellation != null)
            {
                disableCancellation.Cancel();
                disableCancellation.Dispose();
            }

            disableCancellation = new CancellationTokenSource();
        }

        private void CacheColliders()
        {
            if (_cachedColliders == null || _cachedColliders.Length == 0)
                _cachedColliders = GetComponentsInChildren<Collider>(true);
        }

        private void SetCollisionEnabled(bool enabled)
        {
            CacheColliders();
            for (int i = 0; i < _cachedColliders.Length; ++i)
            {
                if (_cachedColliders[i] != null)
                    _cachedColliders[i].enabled = enabled;
            }

            if (MyRigidbody != null)
            {
                MyRigidbody.linearVelocity = Vector3.zero;
                MyRigidbody.angularVelocity = Vector3.zero;
            }
        }

        public override void Init()
        {
            _monsterChart = GameChart.Get<MonsterChart>();

            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
        }
        //------------------------------------------------------------------------------------
        public void SetMonster(BattleSceneMap_Aggro battleSceneMap_Aggro, Vector3 spawnPos, int monsterIndex, int modelIndex, System.Action<MonsterController> onDeadCallback = null)
        {
            _isDeadHandling = false;
            SetCollisionEnabled(true);
            ChangeSpineColor(Color.white);
            ResetDamageCancellation();

            if (_delayedPoolCoroutine != null)
            {
                StopCoroutine(_delayedPoolCoroutine);
                _delayedPoolCoroutine = null;
            }

            ApplyMonsterStats(monsterIndex);
            string animationResourceKey = "GhostSword";
            if (_myCharacterAniController != null
                && _myCharacterAniController.TryGetRandomAnimationResourceKey(out string randomAnimationResourceKey))
            {
                animationResourceKey = randomAnimationResourceKey;
            }

            SetAnimationResourceKey(animationResourceKey);
            _battleSceneMap_Aggro = battleSceneMap_Aggro;
            _onDeadCallback = onDeadCallback;
            _spawnPos = spawnPos;
            _attackTarget = null;
            ReleaseAttackSlotReservation();
            _isWandering = false;
            _isReturningToSpawn = false;
            _wanderTargetPos = _spawnPos;
            ScheduleNextWander();
        }
        //------------------------------------------------------------------------------------
        public void PrepareForPool()
        {
            if (_delayedPoolCoroutine != null)
            {
                StopCoroutine(_delayedPoolCoroutine);
                _delayedPoolCoroutine = null;
            }

            ReleaseAttackSlotReservation();
            _battleSceneMap_Aggro = null;
            _onDeadCallback = null;
            _attackTarget = null;
            _isDeadHandling = false;
            _isWandering = false;
            _isReturningToSpawn = false;
            _wanderTargetPos = Vector3.zero;
            _spawnPos = Vector3.zero;

            if (Managers.AggroManager.isAlive)
                Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);

            ChangeSpineColor(Color.white);
            SetCollisionEnabled(false);
            ChangeState(CharacterState.Idle, false);
            disableCancellation.Cancel();
        }
        //------------------------------------------------------------------------------------
        private void ApplyMonsterStats(int monsterIndex)
        {
            if (_monsterChart == null)
                _monsterChart = GameChart.Get<MonsterChart>();

            if (_monsterChart != null && _monsterChart.TryGetInfo(monsterIndex, out MonsterInfo monsterInfo) && monsterInfo != null)
            {
                _characterStatOperator.ForceReleaseStat();

                var baseStats = monsterInfo.GetBaseStats();
                if (baseStats != null)
                {
                    foreach (var pair in baseStats)
                        _characterStatOperator.SetDefaultStat(pair.Key, pair.Value);
                }

                _characterStatOperator.RefreshOutputStatValue();
                RefreshStat(true);
                return;
            }

            RefreshCheatStat();
        }
        //------------------------------------------------------------------------------------
        public void SetAggro(PlayerController playerController)
        {
            if (playerController == null)
            {
                // Lost aggro while chasing: reset and return to spawn before wandering.
                if (_attackTarget != null)
                {
                    ReleaseAttackSlotReservation();
                    _attackTarget = null;
                    _isWandering = false;
                    _isReturningToSpawn = true;
                    SetHP(_maxHP);
                    return;
                }

                ReleaseAttackSlotReservation();
                _attackTarget = null;
                return;
            }

            _attackTarget = playerController;
            EnsureAttackSlotReservation(playerController);
            _isWandering = false;
            _isReturningToSpawn = false;
        }
        //------------------------------------------------------------------------------------
        protected override void OnDamage()
        {
            OnDamageDirection().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask OnDamageDirection()
        {
            if (_isDeadHandling || CharacterState == CharacterState.Dead)
                return;

            ChangeState(CharacterState.Hit);
            ChangeSpineColor(StaticResource.Instance.GetBattleModeStaticData().MonsterHitColor);

            try
            {
                await UniTask.WaitForSeconds(StaticResource.Instance.GetBattleModeStaticData().MonsterHitDuration, false, PlayerLoopTiming.Update, disableCancellation.Token);
            }
            catch (System.OperationCanceledException)
            {
                ChangeSpineColor(Color.white);
                return;
            }

            if (_isDeadHandling || CharacterState == CharacterState.Dead)
            {
                ChangeSpineColor(Color.white);
                return;
            }

            ChangeSpineColor(Color.white);
            ChangeState(CharacterState.Idle);
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlay()
        {
            _isWandering = false;
            _wanderTargetPos = _spawnPos;
            ScheduleNextWander();
            ChangeState(CharacterState.Idle);
        }
        //------------------------------------------------------------------------------------
        public override Vector3 GetMoveDirection()
        {
            if (_attackTarget == null)
            {
                if (_isWandering)
                    return (_wanderTargetPos - transform.position).normalized;

                return (_spawnPos - transform.position).normalized;
            }

            if (_hasReservedAttackSlot)
                return (_reservedAttackSlotPos - transform.position).normalized;

            return base.GetMoveDirection();
        }
        //------------------------------------------------------------------------------------
        protected override void OnDead()
        {
            if (LanternManager.isAlive)
                LanternManager.Instance.PlaySoulAbsorbEffect(transform.position);

            if (_isDeadHandling)
                return;

            _isDeadHandling = true;
            ReleaseAttackSlotReservation();
            SetCollisionEnabled(false);
            ChangeSpineColor(Color.white);
            disableCancellation.Cancel();

            if (_battleSceneMap_Aggro != null)
            {
                _battleSceneMap_Aggro.OnDeadMonster(this, false);
                _battleSceneMap_Aggro = null;
            }
            else
            {
                _onDeadCallback?.Invoke(this);
            }

            float deadDuration = 0f;
            if (StaticResource.Instance != null && StaticResource.Instance.GetBattleModeStaticData() != null)
                deadDuration = Mathf.Max(0f, StaticResource.Instance.GetBattleModeStaticData().MonsterDeadDuration);

            _delayedPoolCoroutine = StartCoroutine(Co_DelayedPool(deadDuration));
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
            if (_isDeadHandling)
                return;

            if (CharacterState == CharacterState.Dead || CharacterState == CharacterState.Hit)
                return;

            if (_attackTarget != null && _attackTarget.IsDead)
            {
                ReleaseAttackSlotReservation();
                _attackTarget = null;
                _isWandering = false;
                _isReturningToSpawn = true;
                ScheduleNextWander();
                ChangeState(CharacterState.Idle);
                return;
            }

            if (_attackTarget != null)
            {
                PlayerController attackTargetPlayer = _attackTarget as PlayerController;
                if (attackTargetPlayer != null)
                    EnsureAttackSlotReservation(attackTargetPlayer);
                else
                    ReleaseAttackSlotReservation();

                _isWandering = false;

                float distanceToTarget = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                if (distanceToTarget <= attackRange && CharacterState == CharacterState.Attack)
                {
                    // 사거리 내에서 공격 중이면 계속 공격
                    return;
                }

                if (_hasReservedAttackSlot)
                {
                    float distanceToSlot = MathDatas.GetDistance(transform.position, _reservedAttackSlotPos);
                    bool hasReachedSlot = distanceToSlot <= AttackSlotReachThreshold;

                    // 1) 사거리 미달이면 슬롯으로 이동(이미 도착했으면 대기)
                    if (distanceToTarget > attackRange)
                    {
                        if (hasReachedSlot)
                        {
                            if (CharacterState != CharacterState.Idle)
                                ChangeState(CharacterState.Idle);
                        }
                        else if (CharacterState != CharacterState.Run)
                        {
                            ChangeState(CharacterState.Run);
                        }

                        return;
                    }

                    // 2) 슬롯 도착 + 사거리 충분하면 공격
                    if (hasReachedSlot)
                    {
                        if (_blockAttack == false && CharacterState != CharacterState.Attack)
                            ChangeState(CharacterState.Attack);
                    }
                    else if (CharacterState != CharacterState.Run)
                    {
                        // 사거리는 되더라도 슬롯 미도착 상태면 슬롯까지 이동
                        ChangeState(CharacterState.Run);
                    }

                    return;
                }

                // 슬롯이 없으면 기존 추적 이동
                if (CharacterState != CharacterState.Run)
                    ChangeState(CharacterState.Run);

                return;
            }

            if (_isReturningToSpawn)
            {
                float distanceToSpawn = MathDatas.GetDistance(transform.position, _spawnPos);
                if (distanceToSpawn <= StaticResource.Instance.GetBattleModeStaticData().MonsterReturnRadius)
                {
                    _isReturningToSpawn = false;
                    ScheduleNextWander();
                    ChangeState(CharacterState.Idle);
                }
                else if (CharacterState != CharacterState.Run)
                {
                    ChangeState(CharacterState.Run);
                }

                return;
            }

            if (_isWandering)
            {
                float distanceToWanderTarget = MathDatas.GetDistance(transform.position, _wanderTargetPos);
                if (distanceToWanderTarget <= StaticResource.Instance.GetBattleModeStaticData().MonsterReturnRadius)
                {
                    _isWandering = false;
                    ScheduleNextWander();
                    ChangeState(CharacterState.Idle);
                }
                else if (CharacterState != CharacterState.Run)
                {
                    ChangeState(CharacterState.Run);
                }

                return;
            }

            if (Time.time >= _nextWanderStartTime)
            {
                StartWander();
                if (CharacterState != CharacterState.Run)
                    ChangeState(CharacterState.Run);
            }
            else if (CharacterState != CharacterState.Idle)
            {
                ChangeState(CharacterState.Idle);
            }
        }
        //------------------------------------------------------------------------------------
        private void ScheduleNextWander()
        {
            BattleModeStaticDataAsset data = StaticResource.Instance?.GetBattleModeStaticData();
            if (data == null)
            {
                _nextWanderStartTime = Time.time + 1f;
                return;
            }

            float minTime = Mathf.Max(0f, data.MonsterWanderIdleMinTime);
            float maxTime = Mathf.Max(minTime, data.MonsterWanderIdleMaxTime);
            _nextWanderStartTime = Time.time + Random.Range(minTime, maxTime);
        }
        //------------------------------------------------------------------------------------
        private void StartWander()
        {
            BattleModeStaticDataAsset data = StaticResource.Instance?.GetBattleModeStaticData();
            if (data == null)
            {
                _wanderTargetPos = _spawnPos;
                _isWandering = false;
                ScheduleNextWander();
                return;
            }
            float radius = Mathf.Max(0f, data.MonsterWanderRadius);

            if (radius <= 0f)
            {
                _wanderTargetPos = _spawnPos;
                _isWandering = false;
                ScheduleNextWander();
                return;
            }

            Vector2 randomCircle = Random.insideUnitCircle * radius;
            _wanderTargetPos = _spawnPos + new Vector3(randomCircle.x, 0f, randomCircle.y);
            _isWandering = true;
        }
        //------------------------------------------------------------------------------------
        protected override void SpineAnimationEvent(string aniName, string eventName)
        {
            if (CharacterState == CharacterState.Attack)
            {
                if (eventName.Contains("AniAction"))
                {
                    if (AttackTarget == null || AttackTarget.IsDead)
                    {
                        ChangeState(CharacterState.Idle);
                        return;
                    }

                    ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
                    
                    SkillInfo defaultAttackData = StaticResource.Instance.GetBattleModeStaticData().MonsterDefaultAttackData;
                    if (defaultAttackData != null)
                        AttackTarget.Damage(defaultAttackData.GetAttackStruct(this));
                    else
                        AttackTarget.Damage(FinalAttack);
                }
                else if (eventName.Contains("Start"))
                {
                    if (_attackTarget != null)
                        ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
                }
                else if (eventName.Contains("End"))
                {
                    if (_attackTarget != null)
                    {
                        float distanceToTarget = MathDatas.GetDistance(transform.position, _attackTarget.transform.position);
                        if (distanceToTarget > attackRange)
                        {
                            ChangeState(CharacterState.Idle);
                        }

                    }
                    else
                    {
                        ChangeState(CharacterState.Idle);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void EnsureAttackSlotReservation(PlayerController playerController)
        {
            if (playerController == null)
            {
                ReleaseAttackSlotReservation();
                return;
            }

            if (_slotOwnerPlayer != playerController)
                ReleaseAttackSlotReservation();

            if (_hasReservedAttackSlot == false)
            {
                TryReserveAttackSlot(playerController);
                return;
            }

            if (Time.time >= _nextSlotSyncTime)
            {
                if (playerController.TryGetReservedAttackSlotPosition(this, out Vector3 slotPosition, out int ringIndex))
                {
                    _reservedAttackSlotPos = slotPosition;
                    _reservedSlotRingIndex = ringIndex;
                }
                else
                {
                    _hasReservedAttackSlot = false;
                    TryReserveAttackSlot(playerController);
                }

                _nextSlotSyncTime = Time.time + SlotSyncInterval;
            }

            if (_reservedSlotRingIndex == 1 && Time.time >= _nextFrontRowRetryTime)
            {
                if (playerController.TryReassignAttackSlot(this, out Vector3 slotPosition, out int ringIndex))
                {
                    _reservedAttackSlotPos = slotPosition;
                    _reservedSlotRingIndex = ringIndex;
                    _hasReservedAttackSlot = true;
                    _slotOwnerPlayer = playerController;
                }

                _nextFrontRowRetryTime = Time.time + FrontRowRetryInterval;
            }
        }
        //------------------------------------------------------------------------------------
        private void TryReserveAttackSlot(PlayerController playerController)
        {
            if (playerController == null)
                return;

            if (playerController.TryReserveAttackSlot(this, out Vector3 slotPosition, out int ringIndex))
            {
                _slotOwnerPlayer = playerController;
                _reservedAttackSlotPos = slotPosition;
                _reservedSlotRingIndex = ringIndex;
                _hasReservedAttackSlot = true;
                _nextSlotSyncTime = Time.time + SlotSyncInterval;
                _nextFrontRowRetryTime = Time.time + FrontRowRetryInterval;
                return;
            }

            _slotOwnerPlayer = playerController;
            _reservedAttackSlotPos = playerController.GetOverflowWaitPosition(this);
            _reservedSlotRingIndex = -1;
            _hasReservedAttackSlot = true;
            _nextSlotSyncTime = Time.time + SlotSyncInterval;
            _nextFrontRowRetryTime = Time.time + FrontRowRetryInterval;
        }
        //------------------------------------------------------------------------------------
        private void ReleaseAttackSlotReservation()
        {
            if (_slotOwnerPlayer != null)
                _slotOwnerPlayer.ReleaseAttackSlot(this);

            _slotOwnerPlayer = null;
            _reservedAttackSlotPos = Vector3.zero;
            _reservedSlotRingIndex = -1;
            _hasReservedAttackSlot = false;
            _nextSlotSyncTime = 0f;
            _nextFrontRowRetryTime = 0f;
        }
        //------------------------------------------------------------------------------------
        private IEnumerator Co_DelayedPool(float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            Managers.MonsterManager.Instance.PoolMonster(this);
            _delayedPoolCoroutine = null;
        }
    }
}
