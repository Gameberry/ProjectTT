using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;

namespace GameBerry
{
    public static class CharacterConditionPool
    {

        public static Dictionary<Enum_ConditionType, ObjectPoolClass<BaseCondition>> _pool = new Dictionary<Enum_ConditionType, ObjectPoolClass<BaseCondition>>();

        public static BaseCondition GetCondition(Enum_ConditionType type)
        {
            ObjectPoolClass<BaseCondition> pool = null;
            if (_pool.ContainsKey(type) == false)
                pool = new ObjectPoolClass<BaseCondition>();
            else
                pool = _pool[type];

            return pool.GetObject() ?? CreateCondition(type);
        }

        public static void PoolCondition(BaseCondition baseCondition)
        {
            if (_pool.ContainsKey(baseCondition.ConditionType) == true)
                _pool[baseCondition.ConditionType].PoolObject(baseCondition);
        }

        private static BaseCondition CreateCondition(Enum_ConditionType type)
        {
            return type switch
            {
                Enum_ConditionType.Stun => new StunCondition(),
                Enum_ConditionType.Snare => new SnareCondition(),
                Enum_ConditionType.Slow => new SlowCondition(),
                Enum_ConditionType.Knockback => new KnockbackCondition(),
                Enum_ConditionType.Invincible => new InvincibleCondition(),

                Enum_ConditionType.AttackUp => new AttackUpCondition(),
                Enum_ConditionType.DefenseUp => new DefenseUpCondition(),
                Enum_ConditionType.MoveSpeedUp => new MoveSpeedUpCondition(),
                Enum_ConditionType.AttackSpeedUp => new AttackSpeedUpCondition(),

                _ => null
            };
        }
    }


    public class CharacterConditionController : MonoBehaviour
    {
        private readonly List<BaseCondition> _conditions = new();
        private readonly List<BaseCondition> _removeList = new();

        private CharacterControllerBase _owner;

        // 합산 결과 (Character에서 참조)
        public bool IsMoveBlocked = false;// { get; private set; }
        public bool IsAttackBlocked = false;// { get; private set; }
        public bool IsSkillBlocked = false;// { get; private set; }

        public float AttackMultiplier { get; private set; } = 1f;
        public float DefenseMultiplier { get; private set; } = 1f;
        public float MoveSpeedMultiplier { get; private set; } = 1f;
        public float AttackSpeedMultiplier { get; private set; } = 1f;

        private void Awake()
        {
            _owner = GetComponent<CharacterControllerBase>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _removeList.Clear();

            foreach (var cond in _conditions)
            {
                cond.OnUpdate(dt);
                if (cond.IsFinished)
                    _removeList.Add(cond);
            }

            foreach (var cond in _removeList)
            {
                cond.OnRemove();
                CharacterConditionPool.PoolCondition(cond);
                _conditions.Remove(cond);
            }

            RecalcAll();
        }

        // ------------ Public API ------------

        public void AddCondition(ConditionData conditionData)
        {
            //newCond.Initialize(_owner, duration);
            if (conditionData == null)
                return;

            if (Random.Range(0.0f, 1.0f) > conditionData.Rate)
                return;

            var meta = StaticResource.Instance.GetConditionData().GetMeta(conditionData.Type);
            if (meta == null)
                return;

            switch (meta.StackPolicy)
            {
                case ConditionStackPolicy.MultipleInstances:
                    {
                        // 그냥 새 인스턴스 추가
                        AddNewCondition(conditionData);
                        break;
                    }
                    
                case ConditionStackPolicy.RefreshDuration:
                    {
                        var existing = FindFirst(conditionData.Type);

                        if (existing != null)
                        {
                            // 기존 것 듀레이션만 교체 (최신/최대 등 규칙은 여기서 바꾸면 됨)
                            existing.Refresh(conditionData.Duration);
                        }
                        else
                        {
                            AddNewCondition(conditionData);
                        }
                        break;
                    }
                    
                case ConditionStackPolicy.MergyValue:
                    {
                        var existing = FindFirst(conditionData.Type);

                        if (existing != null)
                        {
                            // 기존 것 듀레이션만 교체 (최신/최대 등 규칙은 여기서 바꾸면 됨)
                            existing.Merge(conditionData);
                        }
                        else
                        {
                            AddNewCondition(conditionData);
                        }
                        break;
                    }
            }

            RecalcAll();
        }

        private void AddNewCondition(ConditionData conditionData)
        {
            BaseCondition newCond = CharacterConditionPool.GetCondition(conditionData.Type);
            newCond.Owner = _owner;
            newCond.Initialize(conditionData);
            _conditions.Add(newCond);
            newCond.OnApply();
        }

        public void RemoveConditionsByType(Enum_ConditionType type)
        {
            _removeList.Clear();

            foreach (var cond in _conditions)
            {
                if (cond.ConditionType == type)
                    _removeList.Add(cond);
            }

            foreach (var cond in _removeList)
            {
                cond.OnRemove();
                CharacterConditionPool.PoolCondition(cond);
                _conditions.Remove(cond);
            }

            RecalcAll();
        }

        public bool HasCondition(Enum_ConditionType type)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].ConditionType == type)
                    return true;
            }
            return false;
        }

        // ------------ 내부 헬퍼 ------------

        private BaseCondition FindFirst(Enum_ConditionType type)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].ConditionType == type)
                    return _conditions[i];
            }
            return null;
        }

        private void RecalcAll()
        {
            RecalcControlLocks();
            RecalcStatMultipliers();

            // Character 쪽으로 결과 전달 (Character 쪽에 구현해둔다고 가정)
            _owner.SetControlLocks(IsMoveBlocked, IsAttackBlocked, IsSkillBlocked);
            _owner.SetConditionStatMultipliers(
                AttackMultiplier,
                DefenseMultiplier,
                MoveSpeedMultiplier,
                AttackSpeedMultiplier
            );
        }

        private void RecalcControlLocks()
        {
            bool blockMove = false;
            bool blockAttack = false;
            bool blockSkill = false;

            foreach (var cond in _conditions)
            {
                if (cond.BlocksMove) blockMove = true;
                if (cond.BlocksAttack) blockAttack = true;
                if (cond.BlocksSkill) blockSkill = true;
            }

            IsMoveBlocked = blockMove;
            IsAttackBlocked = blockAttack;
            IsSkillBlocked = blockSkill;
        }

        private void RecalcStatMultipliers()
        {
            float atkMul = 1f;
            float defMul = 1f;
            float moveMul = 1f;
            float aspdMul = 1f;

            foreach (var cond in _conditions)
            {
                atkMul *= cond.AttackMultiplier;
                defMul *= cond.DefenseMultiplier;
                moveMul *= cond.MoveSpeedMultiplier;
                aspdMul *= cond.AttackSpeedMultiplier;
            }

            AttackMultiplier = atkMul;
            DefenseMultiplier = defMul;
            MoveSpeedMultiplier = moveMul;
            AttackSpeedMultiplier = aspdMul;
        }
    }
}