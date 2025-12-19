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
                Enum_ConditionType.HpUp => new HpUpCondition(),
                Enum_ConditionType.DefenseUp => new DefenseUpCondition(),
                Enum_ConditionType.MoveSpeedUp => new MoveSpeedUpCondition(),
                Enum_ConditionType.AttackSpeedUp => new AttackSpeedUpCondition(),

                Enum_ConditionType.ComboBuff_AttackSpeedUp => new ComboBuff_AttackSpeedUpCondition(),
                Enum_ConditionType.ComboBuff_AttackUp => new ComboBuff_AttackUpCondition(),
                Enum_ConditionType.ComboBuff_CriticalChangeUp => new ComboBuff_CriticalChangeUpCondition(),

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

        public float AttackInc { get; private set; } = 0f;
        public float HpInc { get; private set; } = 0f;
        public float DefenseInc { get; private set; } = 0f;
        public float MoveSpeedInc { get; private set; } = 0f;
        public float AttackSpeedInc { get; private set; } = 0f;
        public float CritChanceAdd { get; private set; } = 0f;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            _owner = GetComponent<CharacterControllerBase>();
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            float dt = Time.deltaTime;
            _removeList.Clear();

            bool needRefresh = false;

            foreach (var cond in _conditions)
            {
                if (cond.Duration < 0)
                    continue;

                cond.OnUpdate(dt);
                if (cond.IsFinished)
                { 
                    _removeList.Add(cond);
                    needRefresh = true;
                }
            }

            foreach (var cond in _removeList)
            {
                cond.OnRemove();
                CharacterConditionPool.PoolCondition(cond);
                _conditions.Remove(cond);
            }

            if (needRefresh == true)
                RecalcAll();
        }
        //------------------------------------------------------------------------------------
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
                    
                case ConditionStackPolicy.Refresh:
                    {
                        var existing = FindFirst(conditionData.Type);

                        if (existing != null)
                        {
                            // 기존 것 갱신
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
        //------------------------------------------------------------------------------------
        private void AddNewCondition(ConditionData conditionData)
        {
            BaseCondition newCond = CharacterConditionPool.GetCondition(conditionData.Type);
            newCond.Owner = _owner;
            newCond.Initialize(conditionData);
            _conditions.Add(newCond);
            newCond.OnApply();
        }
        //------------------------------------------------------------------------------------
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
        //------------------------------------------------------------------------------------
        public bool HasCondition(Enum_ConditionType type)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].ConditionType == type)
                    return true;
            }
            return false;
        }
        //------------------------------------------------------------------------------------
        private BaseCondition FindFirst(Enum_ConditionType type)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].ConditionType == type)
                    return _conditions[i];
            }
            return null;
        }
        //------------------------------------------------------------------------------------
        private void RecalcAll()
        {
            RecalcControlLocks();
            RecalcStatInc();

            // Character 쪽으로 결과 전달 (Character 쪽에 구현해둔다고 가정)
            _owner.SetControlLocks(IsMoveBlocked, IsAttackBlocked, IsSkillBlocked);

            _owner.CharacterStatOperator.SetBuffValue(V2Enum_Stat.Attack_Inc, AttackInc);
            _owner.CharacterStatOperator.SetBuffValue(V2Enum_Stat.Hp_Inc, HpInc);
            _owner.CharacterStatOperator.SetBuffValue(V2Enum_Stat.Defence_Inc, DefenseInc);
            _owner.CharacterStatOperator.SetBuffValue(V2Enum_Stat.MoveSpeed_Inc, MoveSpeedInc);
            _owner.CharacterStatOperator.SetBuffValue(V2Enum_Stat.AttackSpeed_Inc, AttackSpeedInc);
            _owner.CharacterStatOperator.SetBuffValue(V2Enum_Stat.CritChance, CritChanceAdd);

            _owner.RefreshStat();
        }
        //------------------------------------------------------------------------------------
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
        //------------------------------------------------------------------------------------
        private void RecalcStatInc()
        {
            float atkInc = 0f;
            float hpInc = 0f;
            float defInc = 0f;
            float moveInc = 0f;
            float aspdInc = 0f;
            float crichanceAdd = 0f;

            foreach (var cond in _conditions)
            {
                atkInc += cond.AttackInc;
                hpInc += cond.HpInc;
                defInc += cond.DefenseInc;
                moveInc += cond.MoveSpeedInc;
                aspdInc += cond.AttackSpeedInc;
                crichanceAdd += cond.CritChanceAdd;
            }

            AttackInc = atkInc;
            HpInc = hpInc;
            DefenseInc = defInc;
            MoveSpeedInc = moveInc;
            AttackSpeedInc = aspdInc;
            CritChanceAdd = crichanceAdd;
        }
        //------------------------------------------------------------------------------------
    }
}