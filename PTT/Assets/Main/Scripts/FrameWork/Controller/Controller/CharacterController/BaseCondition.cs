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

        // 스탯 배율 (기본 1.0)
        public virtual float AttackMultiplier => 1f;
        public virtual float DefenseMultiplier => 1f;
        public virtual float MoveSpeedMultiplier => 1f;
        public virtual float AttackSpeedMultiplier => 1f;

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

    public class InvincibleCondition : BaseCondition
    {
        public InvincibleCondition() : base(Enum_ConditionType.Invincible) { }

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

        public override float MoveSpeedMultiplier => _moveRate;
        public override float AttackSpeedMultiplier => _attackRate;
    }


    public class KnockbackCondition : BaseCondition
    {
        private Vector2 _direction;
        private float _force;

        public Vector2 Direction => _direction;
        public float Force => _force;

        public KnockbackCondition() : base(Enum_ConditionType.Knockback) { }

        public override bool BlocksMove => true;
        public override bool BlocksAttack => true;
        public override bool BlocksSkill => true;

        public override void Initialize(ConditionData conditionData)
        {
            base.Initialize(conditionData);

            Vector2 ownerPos = Owner.transform.position;

            Vector2 direction = ownerPos - conditionData.EffectPos;

            if (direction.sqrMagnitude > 0.0001f)
                _direction = direction.normalized;
            else
                _direction = Vector2.zero;

            _force = Mathf.Max(0f, conditionData.Param1);
        }

        public override void OnApply()
        {
            if (_direction == Vector2.zero || _force <= 0f)
                return;

            // 필요에 따라 이 부분은 커스텀
            Owner.AddForce(_direction * _force, ForceMode2D.Impulse);
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

        public override float AttackMultiplier => _rate;
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

        public override float DefenseMultiplier => _rate;
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

        public override float MoveSpeedMultiplier => _rate;
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

        public override float AttackSpeedMultiplier => _rate;
    }
}