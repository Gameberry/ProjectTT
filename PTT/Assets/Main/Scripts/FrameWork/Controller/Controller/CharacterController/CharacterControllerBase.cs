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
        protected Rigidbody _rigidbody2D;
        public Rigidbody MyRigidbody2D { get { return _rigidbody2D; } }

        [SerializeField]
        protected CharacterControllerBase _attackTarget;
        public CharacterControllerBase AttackTarget { get { return _attackTarget; } }

#if UNITY_EDITOR
        [SerializeField]
#endif
        protected CharacterStatOperator _characterStatOperator = new CharacterStatOperator();
        public CharacterStatOperator CharacterStatOperator { get { return _characterStatOperator; } }

        protected double _myDamage = 0.0;
        public double MyDamage { get { return _myDamage; } }

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

        protected float _characterAttackSpeed = 1.0f;
        protected float _characterMoveSpeed = 1.0f;
        public float MyCharacterMoveSpeed { get { return _characterMoveSpeed; } }


        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.AnimationEvent += SpineAnimationEvent;
        }
        //------------------------------------------------------------------------------------
        public virtual void Init()
        {

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
        public void OnDamage(double damage)
        {
            if (IsDead == true)
                return;

            DeCreaseHP(damage);
            if (CurrentHP <= 0)
                ChangeState(CharacterState.Dead);
            else
            { 

            }
        }
        //------------------------------------------------------------------------------------
        public void OnDamage(AttackData damage)
        {
            if (damage.Hitter != null && damage.Hitter.IsDead == false)
                OnDamage(damage.DamageRate * damage.Hitter.MyDamage);
        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(AttackData attackData, Vector3 pos)
        {
            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, null);
        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(AttackData attackData, Vector3 pos, CharacterControllerBase fixSkillHitReceiver)
        {
            SkillTriggerManager.Instance.EffectDamage(attackData, this, pos, fixSkillHitReceiver);
        }
        //------------------------------------------------------------------------------------
        public void Play()
        {
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
        protected virtual void ChangeState(CharacterState state)
        {
            if (_characterState == state)
                return;

            _characterState = state;
            PlayAnimation(state);
            if (state == CharacterState.Dead)
            {
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

            Vector3 elementRot = _element_leftRot;

            float selectRatote = 0.0f;

            if (_lookDirection == Enum_LookDirection.Right)
            {
                elementRot = _element_rightRot;
                selectRatote = 180.0f;
            }

            rotate.y = selectRatote;

            transform.eulerAngles = rotate;

            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.transform.localEulerAngles = elementRot;

            if (_uiCharacterState != null)
                _uiCharacterState.transform.localEulerAngles = elementRot;
        }
        //------------------------------------------------------------------------------------
        public void ChangeCharacterLookAtDirection_Target(Transform targetTrans)
        {
            Vector2 direction = targetTrans.transform.position - transform.position;
            direction.Normalize();

            ChangeCharacterLookAtDirection(direction.x < 0 ? Enum_LookDirection.Left : Enum_LookDirection.Right);
        }
        //------------------------------------------------------------------------------------
        protected void SetNewTarget()
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

            _myDamage = GetOutPutMyStat(V2Enum_Stat.Attack);
        }
        //------------------------------------------------------------------------------------
    }
}

