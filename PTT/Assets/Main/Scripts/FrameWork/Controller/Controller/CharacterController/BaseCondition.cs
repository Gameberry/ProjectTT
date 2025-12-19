using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Common;

namespace GameBerry
{
    public abstract class BaseCondition
    {
        public Enum_ConditionType ConditionType { get; private set; }
        public float Duration { get; protected set; }

        protected float _elapsed = 0f;
        public CharacterControllerBase Owner;
        protected ConditionData _conditionData;

        protected BaseCondition(Enum_ConditionType type)
        {
            ConditionType = type;
        }

        public virtual void Initialize(ConditionData conditionData)
        {
            _conditionData = conditionData;
            Duration = conditionData.Duration;
            _elapsed = 0f;
        }

        public virtual void OnApply() { }
        public virtual void OnRemove() { }

        public virtual void OnUpdate(float deltaTime)
        {
            _elapsed += deltaTime;
        }

        public bool IsFinished => _elapsed >= Duration;

        // 컨트롤 관련 플래그 (합산용)
        public virtual bool BlocksMove => false;
        public virtual bool BlocksAttack => false;
        public virtual bool BlocksSkill => false;

        // 스탯 추가 배율
        public virtual float AttackInc => 0f;
        public virtual float HpInc => 0f;
        public virtual float DefenseInc => 0f;
        public virtual float MoveSpeedInc => 0f;
        public virtual float AttackSpeedInc => 0f;

        /// <summary>RefreshDuration 용 (스턴 등)</summary>
        public virtual void Refresh(float duration)
        {
            Duration = duration;
            _elapsed = 0f;
        }

        /// <summary>AccumulateValue 용 </summary>
        public virtual void Merge(ConditionData conditionData)
        {
            // 기본은 아무것도 안 함
        }
    }

    public class InvincibleCondition : BaseCondition
    {
        public InvincibleCondition() : base(Enum_ConditionType.Invincible) { }
    }

    public class StunCondition : BaseCondition
    {
        public StunCondition() : base(Enum_ConditionType.Stun) { }

        public override bool BlocksMove => true;
        public override bool BlocksAttack => true;
        public override bool BlocksSkill => true;
    }

    public class SnareCondition : BaseCondition
    {
        public SnareCondition() : base(Enum_ConditionType.Snare) { }

        public override bool BlocksMove => true;
    }

    public class SlowCondition : BaseCondition
    {
        private float _moveRate;
        private float _attackRate;

        public SlowCondition() : base(Enum_ConditionType.Slow) { }

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);
            _moveRate = Mathf.Clamp(conditionData.Param1, 0f, 1f);
            _attackRate = Mathf.Clamp(conditionData.Param2, 0f, 1f);
        }

        public override float MoveSpeedInc => _moveRate;
        public override float AttackSpeedInc => _attackRate;
    }

    public class KnockbackCondition : BaseCondition
    {
        private Vector2 _direction;      // 넉백 방향 (정규화)
        private float _distance;         // 총 넉백 거리
        private float _baseDuration;     // 요청된 기본 지속 시간

        private Rigidbody2D _rb;
        private Vector2 _startPos;       // 넉백 시작 위치
        private Vector2 _prevPos;        // 직전 프레임에서의 목표 위치 (delta 계산용)

        /// <summary>넉백 방향 (정규화된 벡터)</summary>
        public Vector2 Direction => _direction;

        /// <summary>넉백 총 거리</summary>
        public float Distance => _distance;

        /// <summary>설정된 기본 지속 시간 (Merge 전에 요청된 값)</summary>
        public float BaseDuration => _baseDuration;

        public KnockbackCondition() : base(Enum_ConditionType.Knockback) { }

        public override bool BlocksMove => true;
        public override bool BlocksAttack => true;
        public override bool BlocksSkill => true;

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);

            _rb = Owner.MyRigidbody2D;

            Vector2 ownerPos = Owner.transform.position;

            Vector2 direction = ownerPos - conditionData.EffectPos;

            if (direction.sqrMagnitude > 0.0001f)
                _direction = direction.normalized;
            else
                _direction = Vector2.zero;

            if (_rb != null)
            {
                _startPos = _rb.position;
                _prevPos = _startPos;
            }
            else
            {
                _startPos = Owner.transform.position;
                _prevPos = _startPos;
            }

            _distance = Mathf.Max(0f, conditionData.Param1);

            _baseDuration = Mathf.Max(0.0001f, conditionData.Duration); // 0이면 나눗셈 터지니 최소값
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (_direction == Vector2.zero || _distance <= 0f)
                return;

            if (Duration <= 0f)
                return;

            float t = Mathf.Clamp01(_elapsed / Duration); // 0~1
            // Ease-Out 적용 (Quadratic Ease-Out)
            // t가 처음엔 빨리, 끝으로 갈수록 천천히 증가하는 느낌
            float easedT = 1f - (1f - t) * (1f - t);

            float currentDist = _distance * easedT;
            Vector2 targetPos = _startPos + _direction * currentDist;

            Vector2 delta = targetPos - _prevPos;
            _prevPos = targetPos;

            if (_rb != null)
            {
                Vector2 newpos = _rb.position + delta;

                Vector2 minpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Min;
                Vector2 maxpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Max;

                if (newpos.x < minpos.x)
                    newpos.x = minpos.x;
                else if (newpos.x > maxpos.x)
                    newpos.x = maxpos.x;

                if (newpos.y < minpos.y)
                    newpos.y = minpos.y;
                else if (newpos.y > maxpos.y)
                    newpos.y = maxpos.y;

                _rb.MovePosition(newpos);
            }
            else
            {
                Vector2 charpos = Owner.transform.position;
                Vector2 newpos = charpos + delta;

                Vector2 minpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Min;
                Vector2 maxpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Max;

                if (newpos.x < minpos.x)
                    newpos.x = minpos.x;
                else if (newpos.x > maxpos.x)
                    newpos.x = maxpos.x;

                if (newpos.y < minpos.y)
                    newpos.y = minpos.y;
                else if (newpos.y > maxpos.y)
                    newpos.y = maxpos.y;

                Owner.transform.position = newpos;
            }
        }

        /// <summary>
        /// 이미 넉백 중일 때, 추가 넉백이 들어온 경우:
        /// - 거리: 추가 거리만큼 더 멀리
        /// - 시간: 추가 duration만큼 더 오래 넉백
        /// </summary>
        public override void Merge(ConditionData conditionData)
        {

            // --- 방향/거리 누적 방식 (Additive) ---

            // 기존 넉백 벡터
            Vector2 oldVec = _direction * _distance;

            Vector2 ownerPos = Owner.transform.position;
            Vector2 direction = ownerPos - conditionData.EffectPos;


            // 새 넉백 벡터
            Vector2 newVec = direction * conditionData.Param1;

            // 합산
            Vector2 merged = oldVec + newVec;

            // 거리 갱신
            _distance = merged.magnitude;

            // 방향 갱신
            if (_distance > 0.0001f)
                _direction = merged.normalized;

            // 시간 누적 (원하면 Replace or Max로 변경해도 됨)
            Duration += conditionData.Duration;
        }
    }


    public class AttackUpCondition : BaseCondition
    {
        private float _rate;

        public AttackUpCondition() : base(Enum_ConditionType.AttackUp) { }

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);

            _rate = conditionData.Param1;
        }

        public override float AttackInc => _rate;
    }

    public class HpUpCondition : BaseCondition
    {
        private float _rate;

        public HpUpCondition() : base(Enum_ConditionType.HpUp) { }

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);

            _rate = conditionData.Param1;
        }

        public override float HpInc => _rate;
    }

    public class DefenseUpCondition : BaseCondition
    {
        private float _rate;

        public DefenseUpCondition() : base(Enum_ConditionType.DefenseUp) { }

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);

            _rate = conditionData.Param1;
        }

        public override float DefenseInc => _rate;
    }

    public class MoveSpeedUpCondition : BaseCondition
    {
        private float _rate;

        public MoveSpeedUpCondition() : base(Enum_ConditionType.MoveSpeedUp) { }

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);

            _rate = conditionData.Param1;
        }

        public override float MoveSpeedInc => _rate;
    }

    public class AttackSpeedUpCondition : BaseCondition
    {
        private float _rate;

        public AttackSpeedUpCondition() : base(Enum_ConditionType.AttackSpeedUp) { }

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);

            _rate = conditionData.Param1;
        }

        public override float AttackSpeedInc => _rate;
    }
}