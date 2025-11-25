using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Managers;

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
        protected Rigidbody2D _rigidbody2D;
        public Rigidbody2D MyRigidbody2D { get { return _rigidbody2D; } }

        [SerializeField]
        protected CharacterControllerBase _attackTarget;
        public CharacterControllerBase AttackTarget { get { return _attackTarget; } }

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

        public bool _blockMove { get; private set; }
        protected bool _blockAttack { get; private set; }
        protected bool _blockSkill { get; private set; }

        private float _condAtkMul = 1f;
        private float _condDefMul = 1f;
        private float _condMoveMul = 1f;
        private float _condAttackSpdMul = 1f;

        public double FinalAttack => _characterAttack * _condAtkMul;
        public double FinalDefense => _characterDefense * _condDefMul;
        public float FinalMoveSpeed => _characterMoveSpeed * _condMoveMul;
        public float FinalAttackSpeed => _characterAttackSpeed * _condAttackSpdMul;

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
        public void SetControlLocks(bool move, bool attack, bool skill)
        {
            _blockMove = move;
            _blockAttack = attack;
            _blockSkill = skill;
        }
        //------------------------------------------------------------------------------------
        public void SetConditionStatMultipliers(float atkMul, float defMul, float moveMul, float aspdMul)
        {
            _condAtkMul = atkMul;
            _condDefMul = defMul;
            _condMoveMul = moveMul;
            _condAttackSpdMul = aspdMul;
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
        public void ChangeSpineColor(Color color)
        {
            _mySkeletonAnimationHandler?.SetColor(color);
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("RefreshCheatStat()")]
        public void RefreshCheatStat()
        {// 데이터가 없어서...
            List<StatViewer> TempPlayerStat = _iFFType == IFFType.IFF_Friend ? StaticResource.Instance.GetBattleModeStaticData().TempPlayerStat : StaticResource.Instance.GetBattleModeStaticData().TempMonsterStat;
            for (int i = 0; i < TempPlayerStat.Count; ++i)
            {
                _characterStatOperator.SetDefaultStat(TempPlayerStat[i].v2Enum_Stat, TempPlayerStat[i].value);
            }
            _characterStatOperator.RefreshDefaultStat();
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
        public void Damage(AttackData damage)
        {
            if (damage.Hitter != null && damage.Hitter.IsDead == false)
            { 
                Damage(damage.DamageRate * damage.Hitter.FinalAttack);
                if (IsDead == false)
                    PlayCharacterCondition(damage.EnemyConditionDatas, damage.Hitter.transform.position);
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void OnDamage()
        { 

        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(AttackData attackData, Vector3 pos)
        {
            if (attackData != null)
            {
                PlayCharacterCondition(attackData.MyConditionDatas, pos);
            }

            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, null);
        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(AttackData attackData, Vector3 pos, CharacterControllerBase fixSkillHitReceiver)
        {
            if (attackData != null)
            {
                PlayCharacterCondition(attackData.MyConditionDatas, pos);
            }

            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, fixSkillHitReceiver);
        }
        //------------------------------------------------------------------------------------
        private void PlayCharacterCondition(List<int> index, Vector2 attackpos)
        {
            for (int i = 0; i < index.Count; ++i)
            {
                PlayCharacterCondition(index[i], attackpos);
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayCharacterCondition(int index, Vector2 attackpos)
        {
            ConditionData conditionData = StaticResource.Instance.GetConditionData().GetData(index);
            conditionData.EffectPos = attackpos;

            PlayCharacterCondition(conditionData);
        }
        //------------------------------------------------------------------------------------
        private void PlayCharacterCondition(ConditionData conditionData)
        {
            if (conditionData == null)
                return;

            _conditionController?.AddCondition(conditionData);
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
                double recoveryvalue = GetOutPutMyStat(V2Enum_Stat.HpRecovery);

                double ratio = recoveryvalue * Define.PerSkillEffectRecoverValue;


                InCreaseHP(ratio * MaxHP * Time.deltaTime);
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

                Debug.LogError(string.Format("DeCreaseHP : {0}", hp));
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
        public void AddForce(Vector2 force, ForceMode2D forceMode2D)
        {
            _rigidbody2D.AddForce(force, forceMode2D);
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
        public void PlayAnimation_AniName(string aniName)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.PlayAnimation_Once(aniName, true);
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
            Vector2 direction = targetTrans.transform.position - transform.position;
            direction.Normalize();

            ChangeCharacterLookAtDirection(direction.x < 0 ? Enum_LookDirection.Left : Enum_LookDirection.Right);
        }
        //------------------------------------------------------------------------------------
        public void SetNewTarget()
        {
            _attackTarget = Managers.AggroManager.Instance.GetIFFTargetCharacter(this);
        }
        //------------------------------------------------------------------------------------
        public virtual double GetOutPutMyStat(V2Enum_Stat v2Enum_Stat)
        {
            return _characterStatOperator.GetOutPutMyStat(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public void RefreshStat(bool setFullHp = false)
        {
            _characterMoveSpeed = (float)(GetOutPutMyStat(V2Enum_Stat.MoveSpeed));
            _characterAttackSpeed = (float)(GetOutPutMyStat(V2Enum_Stat.AttackSpeed));

            double currHpRatio = 0;

            if (_maxHP <= 0)
                currHpRatio = 0;
            else
                currHpRatio = _currentHP / _maxHP;


            _maxHP = GetOutPutMyStat(V2Enum_Stat.HP);

            if (setFullHp == true)
                _currentHP = _maxHP;
            else
                _currentHP = _maxHP * currHpRatio;

            _characterAttack = GetOutPutMyStat(V2Enum_Stat.Attack);
            _characterDefense = GetOutPutMyStat(V2Enum_Stat.Defence);
        }
        //------------------------------------------------------------------------------------
    }
}

