using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using Spine;

namespace GameBerry
{
    public class LanternController : CharacterControllerBase
    {
        [Header("Follow")]
        [SerializeField] private Vector3 _baseFollowOffset = new Vector3(1.2f, 0.9f, 0.0f);
        [SerializeField] private float _followLerp = 12f;
        [SerializeField] private float _hoverAmplitude = 0.25f;
        [SerializeField] private float _hoverFrequency = 1.8f;
        [Header("Move State")]
        [SerializeField] private float _runStartDistance = 0.45f;
        [SerializeField] private float _runStopDistance = 0.25f;
        [Header("Soul Effect")]
        [SerializeField] private GameObject _soulOrbPrefab;
        [SerializeField] private float _soulVisibleDelayTime = 0.5f;
        [SerializeField] private float _soulWaitTime = 0.5f;
        [SerializeField] private float _soulTravelDuration = 0.35f;
        [SerializeField] private float _soulArcHeight = 0.8f;
        [SerializeField] private Transform _soulGoalTransform = null;

        protected override Enum_SkillActorType SkillActorType => Enum_SkillActorType.Lantern;

        private PlayerController _ownerPlayer;
        private int _lanternItemId;
        private SkillInfo _mainSkillInfo;
        private float _hoverSeed;
        private bool _isSkillSystemInitialized;
        private bool _isMoveRunState;

        public override void Init()
        {
            _hoverSeed = Random.Range(-10f, 10f);
            _maxHP = 0;
            _currentHP = 0;

            // Lantern has no HP and should never take damage.
            PlayCharacterCondition(new ConditionData
            {
                Type = Enum_ConditionType.Invincible,
                Duration = -1f,
                Rate = 1f
            });

            InitializeSkillSystem();
            _isSkillSystemInitialized = true;
            AutoUseSkills = true;
        }

        public override void Release()
        {
            UnbindOwnerAttack();

            if (_isSkillSystemInitialized)
            {
                ReleaseSkillSystem();
                _isSkillSystemInitialized = false;
            }
        }

        protected override void OnPlay()
        {
            ChangeState(CharacterState.Idle);
        }

        public void Setup(PlayerController ownerPlayer, int lanternItemId)
        {
            if (_ownerPlayer != ownerPlayer)
            {
                UnbindOwnerAttack();
                _ownerPlayer = ownerPlayer;
                BindOwnerAttack();
            }

            _lanternItemId = lanternItemId;
            _attackTarget = _ownerPlayer != null ? _ownerPlayer.AttackTarget : null;
            ChangeState(CharacterState.Idle);
            _isMoveRunState = false;

            RefreshSpineModel();
            PlayIdleAnimation();
            RefreshMainSkill();
            SnapToOwner();

            if (LanternManager.isAlive)
                LanternManager.Instance.RegisterActiveLanternController(this);
        }

        protected override void Updated()
        {
            if (_ownerPlayer == null || _ownerPlayer.IsDead)
                return;

            RefreshMoveState();
            FollowOwner();
            SyncTarget();

            if (_mainSkillInfo == null || _attackTarget == null || _attackTarget.IsDead)
                return;

            UpdateSkillSystem();
            TryUseMainSkill();
        }

        protected override CharacterControllerBase GetMyConditionReceiver(AttackStruct attackData)
        {
            return _ownerPlayer != null ? _ownerPlayer : this;
        }

        private void RefreshMainSkill()
        {
            _mainSkillInfo = null;

            if (LanternManager.isAlive == false || _lanternItemId <= 0)
            {
                SetEquippedSkills(new List<int>());
                return;
            }

            LanternInfo lanternInfo = LanternManager.Instance.GetLanternInfo(_lanternItemId);
            if (lanternInfo == null || lanternInfo.Skill <= 0)
            {
                SetEquippedSkills(new List<int>());
                return;
            }

            _mainSkillInfo = GameChart.Get<SkillChart>()?.GetActive(lanternInfo.Skill, Enum_SkillActorType.Lantern);
            if (_mainSkillInfo == null)
            {
                SetEquippedSkills(new List<int>());
                return;
            }

            SetEquippedSkills(new List<int> { _mainSkillInfo.SkillId });
        }

        private void RefreshSpineModel()
        {
            LanternInfo lanternInfo = LanternManager.isAlive ? LanternManager.Instance.GetLanternInfo(_lanternItemId) : null;
            if (lanternInfo == null || lanternInfo.SpineResourceId <= 0)
                return;

            SpineModelData modelData = StaticResource.Instance.GetCreatureSpineModelData(lanternInfo.SpineResourceId);
            if (modelData == null)
                return;

            SetSpineModelData(modelData);
            ApplyDefaultSkin(modelData);
        }

        private void SyncTarget()
        {
            _attackTarget = _ownerPlayer.AttackTarget;
            if (_attackTarget == null || _attackTarget.IsDead)
                _ownerPlayer.SetNewTarget();
            _attackTarget = _ownerPlayer.AttackTarget;
        }

        private void SnapToOwner()
        {
            if (_ownerPlayer == null)
                return;

            transform.position = GetDesiredPosition();
            ChangeCharacterLookAtDirection(_ownerPlayer.LookDirection);
        }

        private void FollowOwner()
        {
            Vector3 desired = GetDesiredPosition();
            float lerpT = Mathf.Clamp01(_followLerp * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, lerpT);
            ChangeCharacterLookAtDirection(_ownerPlayer.LookDirection);
        }

        private void RefreshMoveState()
        {
            Vector3 desired = GetDesiredPosition();
            float distance = Vector3.Distance(transform.position, desired);

            float runStart = Mathf.Max(0.01f, _runStartDistance);
            float runStop = Mathf.Clamp(_runStopDistance, 0.0f, runStart);

            // if (_isMoveRunState == false && distance >= runStart)
            // {
            //     _isMoveRunState = true;
            //     ChangeState(CharacterState.Run);
            // }
            // else if (_isMoveRunState && distance <= runStop)
            // {
            //     _isMoveRunState = false;
            //     ChangeState(CharacterState.Idle);
            // }
            if (distance >= runStart)
            {
                _characterMoveSpeed = 1.0f;
                ChangeState(CharacterState.Run);
            }
            else
            {
                ChangeState(CharacterState.Idle);
            }
        }

        private Vector3 GetDesiredPosition()
        {
            Vector3 offset = _baseFollowOffset;
            if (_ownerPlayer != null && _ownerPlayer.LookDirection == Enum_LookDirection.Left)
                offset.x = Mathf.Abs(offset.x);
            else
                offset.x = -Mathf.Abs(offset.x);

            float hover = Mathf.Sin((Time.time + _hoverSeed) * _hoverFrequency) * _hoverAmplitude;
            offset.y += hover;

            return _ownerPlayer.transform.position + offset;
        }

        private int GetLanternSkillLevel()
        {
            if (LanternManager.isAlive == false || _lanternItemId <= 0)
                return 1;

            return Mathf.Max(1, LanternManager.Instance.GetLanternLevel(_lanternItemId));
        }

        private CharacterControllerBase GetSkillHitter()
        {
            return _ownerPlayer != null ? _ownerPlayer : this;
        }

        private void TryUseMainSkill()
        {
            if (_nextSkillData == null || _attackTarget == null)
                return;

            SkillInfo castSkill = _nextSkillData;
            _nextSkillData = null;

            ChangeCharacterLookAtDirection_Target(_attackTarget.transform);

            //ChangeState(CharacterState.Attack);

            AttackStruct attackStruct = castSkill.GetAttackStruct(GetSkillHitter(), GetLanternSkillLevel());
            // if (_skillPlayer != null)
            //     _skillPlayer.PlaySkill(attackStruct, _attackTarget);
            // else
                PlaySkill(attackStruct, transform.position, _attackTarget);
            StartCoolDown(castSkill.SkillId);
        }

        private void BindOwnerAttack()
        {
            if (_ownerPlayer == null)
                return;

            _ownerPlayer.OnAttackPerformed -= HandleOwnerAttack;
            _ownerPlayer.OnAttackPerformed += HandleOwnerAttack;
        }

        private void UnbindOwnerAttack()
        {
            if (_ownerPlayer == null)
                return;

            _ownerPlayer.OnAttackPerformed -= HandleOwnerAttack;
        }

        private void HandleOwnerAttack()
        {
            OnSkillOwnerAttack();
        }

        public void PlaySoulAbsorbFrom(Vector3 sourceWorldPos)
        {
            if (isActiveAndEnabled == false)
                return;

            GameObject orb = CreateSoulOrb(sourceWorldPos);
            if (orb == null)
                return;

            StartCoroutine(CoMoveSoulOrb(orb.transform, sourceWorldPos));
        }

        private GameObject CreateSoulOrb(Vector3 sourceWorldPos)
        {
            if (_soulOrbPrefab != null)
            {
                GameObject clone = Instantiate(_soulOrbPrefab, sourceWorldPos, Quaternion.identity);
                //Destroy(clone, Mathf.Max(0.5f, _soulTravelDuration + 0.5f));
                return clone;
            }

            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallback.name = "LanternSoulOrb";
            fallback.transform.position = sourceWorldPos;
            fallback.transform.localScale = Vector3.one * 0.22f;

            Collider col = fallback.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            Renderer renderer = fallback.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.material.color = new Color(0.4f, 0.95f, 1f, 1f);
            }

            Destroy(fallback, Mathf.Max(0.5f, _soulTravelDuration + 0.5f));
            return fallback;
        }

        private IEnumerator CoMoveSoulOrb(Transform orb, Vector3 startPos)
        {

            orb.gameObject.SetActive(false);

            yield return new WaitForSeconds(_soulVisibleDelayTime);

            orb.gameObject.SetActive(true);
            orb.position = startPos;

            yield return new WaitForSeconds(_soulWaitTime);

            float totalDuration = _soulTravelDuration;
            float burstDuration = totalDuration * 0.45f;
            float absorbDuration = totalDuration - burstDuration;

            Vector3 goalPos = _soulGoalTransform != null ? _soulGoalTransform.position : transform.position;
            Vector3 dir = goalPos - startPos;
            float distance = dir.magnitude;
            if (distance > 0.0001f)
                dir /= distance;
            else
                dir = Vector3.up;

            Vector3 cross = new Vector3(-dir.y, dir.x, 0f).normalized;
            float randomCrossDir = Random.Range(0, 2) == 0 ? -1f : 1f;
            float convertDis = distance * 0.5f;

            float elapsed = 0f;
            while (elapsed < burstDuration && orb != null)
            {
                elapsed += Time.deltaTime;
                float ratio = Mathf.Clamp01(elapsed / burstDuration);

                Vector3 newDir = -dir * Mathf.Sin(Mathf.PI * ratio);
                Vector3 newCross = cross * Mathf.Sin(Mathf.PI * 0.5f * ratio) * randomCrossDir;
                Vector3 offset = convertDis * ((newDir * 0.8f) + newCross);

                Vector3 pos = startPos + offset;
                pos.y += Mathf.Sin(Mathf.PI * ratio) * (_soulArcHeight * 0.55f);
                orb.position = pos;
                orb.localScale = Vector3.one * Mathf.Lerp(1f, 0.82f, ratio);
                yield return null;
            }

            Vector3 absorbStart = orb != null ? orb.position : startPos;
            elapsed = 0f;
            while (elapsed < absorbDuration && orb != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / absorbDuration);
                float eased = EaseOutQuart(t);

                goalPos = _soulGoalTransform != null ? _soulGoalTransform.position : transform.position;
                Vector3 pos = Vector3.Lerp(absorbStart, goalPos, eased);
                pos.y += Mathf.Sin(Mathf.PI * t) * (_soulArcHeight * 0.35f);
                orb.position = pos;
                orb.localScale = Vector3.one * Mathf.Lerp(0.82f, 0.2f, eased);
                yield return null;
            }

            if (orb != null)
            {
                // Ensure the soul arrives exactly at lantern position.
                orb.position = _soulGoalTransform != null ? _soulGoalTransform.position : transform.position;
                orb.localScale = Vector3.one * 0.2f;
                yield return null;
                Destroy(orb.gameObject);
            }
        }

        private static float EaseOutQuart(float t)
        {
            t = Mathf.Clamp01(t);
            float inv = 1f - t;
            return 1f - (inv * inv * inv * inv);
        }

        private void PlayIdleAnimation()
        {
            if (_currentSpineModelData != null)
            {
                string idleAnimation = _currentSpineModelData.GetAnimationName(CharacterState.Idle);
                if (string.IsNullOrEmpty(idleAnimation) == false)
                {
                    PlayAnimation_AniName(idleAnimation, true);
                    return;
                }
            }

            PlayAnimation(CharacterState.Idle);
        }

        private void ApplyDefaultSkin(SpineModelData modelData)
        {
            if (modelData == null || modelData.SkeletonData == null)
                return;

            SkeletonData skeletonData = modelData.SkeletonData.GetSkeletonData(true);
            if (skeletonData == null)
                return;

            Skin skin = new Skin("lantern-default-runtime");
            bool hasAny = false;
            for (int i = 0; i < (int)Enum_SkinSlotType.Max; ++i)
            {
                List<string> skinNames = modelData.DefaultSkin((Enum_SkinSlotType)i);
                if (skinNames == null || skinNames.Count == 0)
                    continue;

                foreach (string skinName in skinNames)
                {
                    Skin part = skeletonData.FindSkin(skinName);
                    if (part == null)
                        continue;

                    skin.AddSkin(part);
                }

                hasAny = true;
            }

            if (hasAny)
                SetSpineSkin(skin);
        }

        private void OnDestroy()
        {
            if (LanternManager.isAlive)
                LanternManager.Instance.UnregisterActiveLanternController(this);
        }
    }
}
