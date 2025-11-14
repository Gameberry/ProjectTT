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
        protected CharacterStatOperator _characterStatOperator;
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
        protected virtual void SpineAnimationEvent(string aniName, string eventName)
        {

        }
        //------------------------------------------------------------------------------------
        public virtual Vector3 GetMoveDirection()
        { // MoveController_Base에서 주로 호출
            // 유저는 조이스틱으로 방향을 정할 때가 있어서 가상함수로 만듬

            if (AttackTarget == null)
                return Vector3.zero;

            return (AttackTarget.transform.position - transform.position).normalized;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
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

            if (_uiCharacterState != null)
                _uiCharacterState.SetHPBar(hpratio);
        }
        //------------------------------------------------------------------------------------
        public void ChangeCharacterState(CharacterState state)
        { // 외부에서 들어올 때
            ChangeState(state);
        }
        //------------------------------------------------------------------------------------
        protected virtual void ChangeState(CharacterState state)
        {

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
    }
}

