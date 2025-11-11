using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Managers;

namespace GameBerry
{
    public enum CharacterState : byte
    {
        None = 0,
        Idle,
        Run,
        Attack,
        Hit,
        Dead,

        Skill,

        Max,
    }



    public class CharacterControllerBase : MonoBehaviour
    {
        public Enum_ARR_LookDirection LookDirection { get { return _lookDirection; } }
        [SerializeField]
        protected Enum_ARR_LookDirection _lookDirection = Enum_ARR_LookDirection.Right;

        [SerializeField]
        protected IFFType _iFFType = IFFType.IFF_None;

        public IFFType IFFType { get { return _iFFType; } }

        [SerializeField]
        protected CharacterState _characterState = CharacterState.None;
        public CharacterState CharacterState { get { return _characterState; } }

        protected float _minTraceRange = 1.0f;

        protected Enum_ARR_TargetConditionType _defaultSearchType = Enum_ARR_TargetConditionType.RangeAll;

        protected Enum_ARR_TargetConditionType _mySearchType;
        public Enum_ARR_TargetConditionType MySearchType { get { return _mySearchType; } }

        protected bool _isMonster = false;
        protected V2Enum_ARR_MonsterGradeType _monsterGradeType = V2Enum_ARR_MonsterGradeType.Normal;
        public V2Enum_ARR_MonsterGradeType MonsterGradeType { get { return _monsterGradeType; } }

        private Dictionary<ParticlePoolElement, ParticlePoolElement> _aliveParticle = new Dictionary<ParticlePoolElement, ParticlePoolElement>();

        public bool IsDead { get { return CharacterState == CharacterState.Dead; } }

        [SerializeField] protected SkeletonAnimationHandler _mySkeletonAnimationHandler;

        [SerializeField] protected SpineModelData _currentSpineModelData;

        [SerializeField]
        public CharacterAniController _charAnicontroller;

        [SerializeField]
        protected UICharacterState _uiCharacterState;

        [SerializeField]
        protected Transform _varianceTransform;

        [SerializeField]
        protected SkillHitReceiver _skillHitReceiver;

        public SkillHitReceiver MySkillHitReceiver { get { return _skillHitReceiver; } }

        [SerializeField]
        protected SkillEffectController _skillEffectController;

        public SkillEffectController MySkillEffectController { get { return _skillEffectController; } }

        [SerializeField]
        protected SkillParticlePlayer _skillParticlePlayer;

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
        protected List<Transform> _sizecontrollList;

        [SerializeField]
        protected double _maxHP = 0.0;
        public double MaxHP { get { return _maxHP; } }

        [SerializeField]
        protected double _shield = 0.0;
        public double Shield { get { return _shield; } }

        public double ShieldRatio = 0.0;


        [SerializeField]
        protected double _currentHP = 0.0;
        public double CurrentHP { get { return _currentHP; } }

        protected double _myDamage = 0.0;
        public double MyDamage { get { return _myDamage; } }

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

        // Hit
        protected float _hitRecoveryStartTime = 0.0f;
        protected float _hitRecoveryTime = 0.2f;

        private System.Random _random = new System.Random();
        private CodeStage.AntiCheat.ObscuredTypes.ObscuredDouble totalCCProbWeight = 100;

        protected CharacterControllerBase _lastHitter;

        protected Coroutine _hitDirectionCoroutine = null;

        protected SkillActiveController _skillActiveController = new SkillActiveController();
        public SkillActiveController SkillActiveController { get { return _skillActiveController; } }

        protected SkillManageInfo _selectPlaySkillData;
        protected SkillCustomAction _selectSkillCustomAction;

        [SerializeField]
        protected SkillCustomActionPlayer _skillCustomActionPlayer;

        [SerializeField]
        protected SkillProjectilePlayer _skillProjectilePlayer;

        [SerializeField]
        protected SkillDirectVisioningPlayer _skillDirectVisioningPlayer;

        [SerializeField]
        protected SkillPiercePlayer _skillPiercePlayer;

        [SerializeField]
        protected SkillVoidPlayer _skillVoidPlayer;

        [SerializeField]
        protected SkillRepeatAttackPlayer _skillRepeatAttackPlayer;


        protected Transform _myRoot;

        protected Dictionary<V2Enum_Stat, double> _myBuffValues = new Dictionary<V2Enum_Stat, double>();

        protected bool _needRefreshBuff = false;

        // Dead
        protected float _deadDirectionTime = 0.3f;

        protected float _creatureDeadTime = 0.0f;

        protected Queue<V2CCData> _reviveQueue = new Queue<V2CCData>();
        protected LinkedList<V2CCData> _killingMoneyQueue = new LinkedList<V2CCData>();
        protected LinkedList<V2CCData> _doubleInterestQueue = new LinkedList<V2CCData>();
        protected LinkedList<V2CCData> _increaseGasGambleProbQueue = new LinkedList<V2CCData>();
        protected LinkedList<V2CCData> _increaseGasHpHealQueue = new LinkedList<V2CCData>();
        protected LinkedList<V2CCData> _increaseJokerProbQueue = new LinkedList<V2CCData>();
        protected LinkedList<V2CCData> _decreaseGamblePriceQueue = new LinkedList<V2CCData>();
        protected LinkedList<ShieldData> _shieldQueue = new LinkedList<ShieldData>();

        protected Dictionary<V2Enum_Grade, LinkedList<SkillBaseData>> _gradeAfterSkill = new Dictionary<V2Enum_Grade, LinkedList<SkillBaseData>>();
        protected LinkedList<SkillBaseData> _allDamageAfterSkill = new LinkedList<SkillBaseData>();

        protected Dictionary<V2Enum_ARR_SynergyType, LinkedList<SkillBaseData>> _synergyAfterSkill = new Dictionary<V2Enum_ARR_SynergyType, LinkedList<SkillBaseData>>();

        protected Dictionary<V2Enum_ARR_SynergyType, LinkedList<SkillModuleData>> _synergySkillModule = new Dictionary<V2Enum_ARR_SynergyType, LinkedList<SkillModuleData>>();


        protected double _interestAmount = 0;
        protected Dictionary<V2Enum_SkillEffectType, double> _goldIncreaseValue = new Dictionary<V2Enum_SkillEffectType, double>();

        protected double _useGoldAmount = 0;
        protected Dictionary<V2Enum_SkillEffectType, double> _goldUseValue = new Dictionary<V2Enum_SkillEffectType, double>();

        protected double _increaseInterestRatet = 0;

        protected double _increaseSynergyCount = 0;

        protected double _damageReduce = 0;

        protected double _increaseHealEffect = 0;

        protected Dictionary<V2Enum_ARR_SynergyType, double> _synergyVampiricDmg = new Dictionary<V2Enum_ARR_SynergyType, double>();
        protected double _outputSynergyVampiricDmg = 0;

        protected Dictionary<V2Enum_ARR_SynergyType, double> _synergyDescendDmg = new Dictionary<V2Enum_ARR_SynergyType, double>();
        protected double _outputSynergyDescendDmg = 0;


        protected Dictionary<V2Enum_ARR_SynergyType, int> _synergyAmount = new Dictionary<V2Enum_ARR_SynergyType, int>();
        protected Dictionary<V2Enum_ARR_SynergyType, double> _synergyIncreaseAtt = new Dictionary<V2Enum_ARR_SynergyType, double>();
        protected Dictionary<V2Enum_ARR_SynergyType, double> _synergyIncreaseDef = new Dictionary<V2Enum_ARR_SynergyType, double>();
        protected Dictionary<V2Enum_ARR_SynergyType, double> _synergyIncreaseHp = new Dictionary<V2Enum_ARR_SynergyType, double>();





        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.AnimationEvent += SpineAnimationEvent;

            if (_skillActiveController != null)
                _skillActiveController.SetCharacterController(this);

            if (_skillEffectController != null)
                _skillEffectController.SetCharacterControllerBase(this);

            if (_skillCustomActionPlayer != null)
                _skillCustomActionPlayer.CharacterControllerBase = this;

            if (_skillProjectilePlayer != null)
                _skillProjectilePlayer.CharacterControllerBase = this;

            if (_skillDirectVisioningPlayer != null)
                _skillDirectVisioningPlayer.CharacterControllerBase = this;

            if (_skillPiercePlayer != null)
                _skillPiercePlayer.CharacterControllerBase = this;

            if (_skillVoidPlayer != null)
                _skillVoidPlayer.CharacterControllerBase = this;

            if (_skillRepeatAttackPlayer != null)
                _skillRepeatAttackPlayer.CharacterControllerBase = this;

            if (_uiCharacterState != null)
                _uiCharacterState.SetShieldBar(0);
        }
        //------------------------------------------------------------------------------------
        public virtual void Init()
        { 

        }
        //------------------------------------------------------------------------------------
        public void SetIFFType(IFFType iFFType)
        {
            _iFFType = iFFType;

            if (_iFFType == IFFType.IFF_Friend)
                gameObject.SetLayerInChildren(LayerMask.NameToLayer("IFF_Friend"));
            else if (_iFFType == IFFType.IFF_Foe)
                gameObject.SetLayerInChildren(LayerMask.NameToLayer("IFF_Foe"));

            if (_uiCharacterState != null)
                _uiCharacterState.SetBarColor(_iFFType == IFFType.IFF_Friend ? Color.green : Color.red);
        }
        //------------------------------------------------------------------------------------
        public void SetAnimNumber(int number)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.SetAnimNumber(number);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSpineModelData(SpineModelData spineModelData)
        {
            if (spineModelData == null)
                return;

            _currentSpineModelData = spineModelData;

            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.SetSpineModel(_currentSpineModelData);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSkin(string skinName)
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.SetSkin(skinName);
        }
        //------------------------------------------------------------------------------------
        public void AddAttachSkin(string skinattack)
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.AddAttachSkin(skinattack);
        }
        //------------------------------------------------------------------------------------
        public void ReleaseAttachSkin()
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.ReleaseAttachSkin();
        }
        //------------------------------------------------------------------------------------
        public void RefreshAttachSkin()
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.RefreshAttachSkin();
        }
        //------------------------------------------------------------------------------------
        protected virtual void SpineAnimationEvent(string aniName, string eventName)
        {

        }
        //------------------------------------------------------------------------------------
        public virtual void SetCreatureSizeControll(Vector3 size)
        {
            for (int i = 0; i < _sizecontrollList.Count; ++i)
            {
                _sizecontrollList[i].localScale = size;
            }
        }
        //------------------------------------------------------------------------------------
        public bool IsHalfOver()
        { 
            return MaxHP * 0.5 < CurrentHP;
        }
        //------------------------------------------------------------------------------------
        public virtual double OnDamage(V2SkillAttackData damage)
        {
            if (CharacterState == CharacterState.Dead)
                return 0.0;

            if (_currentHP <= 0)
                return 0.0;

            if (_skillEffectController != null)
            {
                if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Invincible) == true)
                {
                    Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.Block, 0.0, _varianceTransform);
                    _skillEffectController.PlayApplyCC(damage.v2CCDatas);
                    return 0.0;
                }
            }

            double resultdamage = 0.0;
            bool isHalfOver = IsHalfOver();

            _lastHitter = null;

            for(int hit = 0; hit < damage.HitCount; ++hit)
            {
                V2DamageData v2DamageData = damage.v2DamageDatas;

                if (v2DamageData.AttackValue > 0)
                {

                    // 최종공격력
                    double finalAttackOper = v2DamageData.AttackValue;

                    // 스킬계수
                    double skillValueOper = v2DamageData.SkillValue;

                    double finalDefence = GetOutPutMyStat(V2Enum_Stat.Defence);

                    finalAttackOper = finalAttackOper * (Define.DefenseStandard / (Define.DefenseStandard + finalDefence));

                    //double damageReductionValue = finalDefence / (finalDefence + Define.DamageReductionValue);

                    double finalDamage = finalAttackOper * skillValueOper;

                    //finalDamage = finalDamage - (finalDamage * damageReductionValue);

                    double criticalOperDamage = finalDamage;

                    V2Enum_Stat criticalType = V2Enum_Stat.Attack;


                    bool ApplyCritical = Random.Range(0.0f, 1000000.0f) <= damage.criticalChance;

                    double criticalDamageValue = GetOutPutMyStat(V2Enum_Stat.CritDmgIncrease);

                    if (ApplyCritical == true)
                    {
                        criticalOperDamage = finalDamage * (Define.CriticalDamageValue + (criticalDamageValue * Define.PerSkillEffectRecoverValue));
                        criticalType = V2Enum_Stat.CritChance;
                    }

                    resultdamage = criticalOperDamage;

                    resultdamage += resultdamage * damage.DamageBoost;

                    resultdamage = System.Math.Round(resultdamage);


                    if (IFFType == IFFType.IFF_Foe)
                    {
                        if (Managers.GameSettingManager.Instance.Cheat_OnPunch() == true)
                            resultdamage = _maxHP * 2.0;
                    }

                    resultdamage -= resultdamage * _damageReduce;

                    if (resultdamage > 0.0)
                    {
                        //if (damage.actorType == IFFType.Foe)
                        //    Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.Monster, resultdamage, m_varianceTransform, v2DamageData.DamageElement);
                        //else
                        //    Managers.HPMPVarianceManager.Instance.ShowVarianceText((HpMpVarianceType)criticalType, resultdamage, m_varianceTransform, v2DamageData.DamageElement);

                        _lastHitter = damage.characterControllerBase;

                        SkillBaseData skillBaseData = damage.skillBaseData;

                        int hitcount = 1;

                        _skillActiveController.AddHitCount(hitcount);

                        if (damage.characterControllerBase != null && damage.IsAfter == false)
                        { 
                            damage.characterControllerBase.SkillActiveController.AddAttackCount(hitcount);
                            if (_isMonster == true)
                            {
                                if (_monsterGradeType == V2Enum_ARR_MonsterGradeType.Normal)
                                    damage.characterControllerBase.SkillActiveController.AddNormalHitCount(hitcount);
                                else
                                    damage.characterControllerBase.SkillActiveController.AddBossHitCount(hitcount);
                            }

                            if(ApplyCritical == true && damage.IsMain == true)
                                damage.characterControllerBase.SkillActiveController.AddCriticalCount(hitcount);
                        }

                        //if (_iFFType == IFFType.IFF_Foe)
                            Managers.HPMPVarianceManager.Instance.ShowVarianceText((HpMpVarianceType)criticalType, resultdamage, _varianceTransform);

                        DeCreaseHP(resultdamage);

                        if (damage.VampiricDmgEffecter != null)
                        {
                            for (int i = 0; i < damage.VampiricDmgEffecter.Count; ++i)
                            {
                                if (IsTargetState(damage.VampiricDmgEffecter[i]) == false)
                                    continue;

                                double vam = resultdamage * damage.VampiricDmgEffecter[i].CCValue;

                                vam = System.Math.Round(vam);

                                if (damage.characterControllerBase != null)
                                    damage.characterControllerBase.ApplyVampiricDmg(vam);

                                break;
                            }
                        }

                        if (damage.characterControllerBase != null)
                            damage.characterControllerBase.ApplySynergyVampiricDmg(resultdamage);
                        

                        double addDamageResult = 0;

                        if (damage.AddDamageEffecter != null)
                        {
                            for (int i = 0; i < damage.AddDamageEffecter.Count; ++i)
                            {
                                if (IsTargetState(damage.AddDamageEffecter[i]) == false)
                                    continue;

                                double addDamage = resultdamage * damage.AddDamageEffecter[i].CCValue;

                                addDamage = System.Math.Round(addDamage);

                                Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.NomalDamage, addDamage, _varianceTransform);

                                DeCreaseHP(addDamage);

                                addDamageResult += addDamage;
                            }
                        }

                        resultdamage += addDamageResult;

                        if (damage.DeadEffecter != null)
                        {
                            for (int i = 0; i < damage.DeadEffecter.Count; ++i)
                            {
                                if (MaxHP * damage.DeadEffecter[i].CCValue < CurrentHP)
                                    continue;

                                if (IsTargetState(damage.DeadEffecter[i]) == false)
                                    continue;

                                double deadDamage = _maxHP * 2.0;

                                Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.Dead, deadDamage, _varianceTransform);

                                DeCreaseHP(deadDamage);
                                break;
                            }
                        }

                        PlayOnDamageDirection();
                    }
                }

                if (_currentHP <= 0)
                    break;
            }


            if (_currentHP <= 0)
            {
                // 죽었다

                bool revive = false;

                while (_reviveQueue.Count > 0)
                { // 그치만 부활
                    V2CCData v2CCData = _reviveQueue.Dequeue();
                    if (IsTargetState(v2CCData) == true)
                    {
                        Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.Revive, 0.0, _varianceTransform);
                        PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType.Revive);
                        SetHP(_maxHP * v2CCData.CCValue);
                        revive = true;

                        _skillActiveController.AddReviveCount(1);

                        break;
                    }
                }

                if (revive == false)
                {
                    ChangeState(CharacterState.Dead);
                    if (damage.characterControllerBase != null)
                        damage.characterControllerBase.ApplyKillMoney();
                }
            }

            if (CharacterState == CharacterState.Dead)
                return resultdamage;

            if (damage.v2CCDatas != null)
            {
                for (int i = 0; i < damage.v2CCDatas.Count; ++i)
                {
                    if (damage.v2CCDatas[i].CCTypeEnum == V2Enum_SkillEffectType.BurnDOT)
                    {
                        V2CCData v2CCData = new V2CCData();
                        v2CCData.CCTypeEnum = damage.v2CCDatas[i].CCTypeEnum;

                        v2CCData.TargetCondition = damage.v2CCDatas[i].TargetCondition;

                        v2CCData.CCTime = damage.v2CCDatas[i].CCTime;
                        v2CCData.CCValue = resultdamage * damage.v2CCDatas[i].CCValue;
                        v2CCData.CCProb = damage.v2CCDatas[i].CCProb;

                        v2CCData.AttackerPos = damage.v2CCDatas[i].AttackerPos;
                        ApplyCC(v2CCData);
                    }
                    else
                        ApplyCC(damage.v2CCDatas[i]);
                }
            }

            return resultdamage;
        }
        //------------------------------------------------------------------------------------
        public void ApplyCC(V2CCData v2CCData)
        {
            if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.ReduceSynergyFire
                || v2CCData.CCTypeEnum == V2Enum_SkillEffectType.ReduceSynergyGold
                || v2CCData.CCTypeEnum == V2Enum_SkillEffectType.ReduceSynergyWater
                || v2CCData.CCTypeEnum == V2Enum_SkillEffectType.ReduceSynergyThunder
                || v2CCData.CCTypeEnum == V2Enum_SkillEffectType.ReduceDescendExp)
                return;

            if (IsTargetState(v2CCData) == false)
                return;

            switch (v2CCData.CCTypeEnum)
            {
                case V2Enum_SkillEffectType.RandomSkillCooltimeDecrease:
                    {
                        _skillActiveController.SetRandomEquipSkillCoolTimeDecrease(v2CCData.CCValue);
                        PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType.RandomSkillCooltimeDecrease);
                        return;
                    }
                case V2Enum_SkillEffectType.Heal:
                    {
                        HealPerAttack(v2CCData.CCValue);
                        PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType.Heal);
                        return;
                    }
                case V2Enum_SkillEffectType.DecreaseAtt:
                case V2Enum_SkillEffectType.DecreaseArmor:
                    {
                        if (v2CCData.CCTime <= 0)
                        {
                            V2Enum_Stat v2Enum_Stat = ARRRStatOperator.ConvertCrowdControlTypeToStat(v2CCData.CCTypeEnum);

                            DeCreaseBuffStat(v2Enum_Stat, v2CCData.CCValue);
                            return;
                        }

                        break;
                    }
                case V2Enum_SkillEffectType.GoldGainTimer:
                    {
                        //Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.GoldGainTimer, 0.0, _varianceTransform);
                        Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), v2CCData.CCValue);
                        RefreshInterestAmount(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt()));
                        Managers.SynergyManager.Instance.ShowInterestText(v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.EarnInterest:
                    {
                        double gold = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt()) * (Define.InterestRate + _increaseInterestRatet);

                        gold = System.Math.Floor(gold);
                        if (gold < 1)
                            return;

                        gold = v2CCData.CCValue < gold ? v2CCData.CCValue : gold;

                        gold += GetDoubleInterest(gold);

                        Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), gold);
                        RefreshInterestAmount(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt()));
                        Managers.SynergyManager.Instance.ShowInterestText(gold);
                        return;
                    }
                case V2Enum_SkillEffectType.RandomSynergyGain:
                    {
                        int synergy = Random.Range(V2Enum_ARR_SynergyType.Red.Enum32ToInt(), V2Enum_ARR_SynergyType.Max.Enum32ToInt());

                        Managers.SynergyManager.Instance.AddSkillSynergy(synergy.IntToEnum32<V2Enum_ARR_SynergyType>(), (int)v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.GetSameSynergy:
                    {
                        Managers.SynergyManager.Instance.AddSkillSynergy(SkillActiveController.LastChoice_SynergyType, (int)v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.GetGas:
                    {
                        Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGas.Enum32ToInt(), v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.MinorJoker:
                    {
                        Managers.GambleManager.Instance.PlayMinorJokerSynergy((int)v2CCData.CCValue);
                        return;
                    }

                case V2Enum_SkillEffectType.GetSynergyFire:
                    {
                        Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.Red, (int)v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.GetSynergyGold:
                    {
                        Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.Yellow, (int)v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.GetSynergyWater:
                    {
                        Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.Blue, (int)v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.GetSynergyThunder:
                    {
                        Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.White, (int)v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.Shield:
                    {
                        AddShield(v2CCData);
                        return;
                    }
                case V2Enum_SkillEffectType.GetDescendGoods:
                    {
                        Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.SynergyMAXGoods, v2CCData.CCValue);
                        return;
                    }
                case V2Enum_SkillEffectType.SheildFireCount:
                    {
                        double shildValue = v2CCData.CCValue * Managers.SynergyManager.Instance.GetSynergyStack(V2Enum_ARR_SynergyType.Red);

                        V2CCData shilddata = v2CCData;
                        shilddata.CCValue = shildValue;

                        AddShield(shilddata);
                        return;
                    }
                case V2Enum_SkillEffectType.SheildGoldCount:
                    {
                        double shildValue = v2CCData.CCValue * Managers.SynergyManager.Instance.GetSynergyStack(V2Enum_ARR_SynergyType.Yellow);

                        V2CCData shilddata = v2CCData;
                        shilddata.CCValue = shildValue;

                        AddShield(shilddata);
                        return;
                    }
                case V2Enum_SkillEffectType.SheildWaterCount:
                    {
                        double shildValue = v2CCData.CCValue * Managers.SynergyManager.Instance.GetSynergyStack(V2Enum_ARR_SynergyType.Blue);

                        V2CCData shilddata = v2CCData;
                        shilddata.CCValue = shildValue;

                        AddShield(shilddata);
                        return;
                    }
                case V2Enum_SkillEffectType.SheildThunderCount:
                    {
                        double shildValue = v2CCData.CCValue * Managers.SynergyManager.Instance.GetSynergyStack(V2Enum_ARR_SynergyType.White);

                        V2CCData shilddata = v2CCData;
                        shilddata.CCValue = shildValue;

                        AddShield(shilddata);
                        return;
                    }
            }

            if (_skillEffectController != null)
            {
                _skillEffectController.PlayApplyCC(v2CCData);
            }

            PlayOnDamageAfterCC(v2CCData);
        }
        //------------------------------------------------------------------------------------
        private bool IsTargetState(V2CCData v2CCData)
        {
            if (v2CCData.CCProb < 100)
            {
                double randomNumber = _random.NextDouble() * totalCCProbWeight;
                if (randomNumber > v2CCData.CCProb)
                    return false;
            }

            switch (v2CCData.TargetCondition)
            {
                case Enum_ARR_TargetStateType.All:
                    return true;
                case Enum_ARR_TargetStateType.HPPercentOver:
                    {
                        return IsHalfOver() == true;
                    }
                case Enum_ARR_TargetStateType.HPPercentBelow:
                    {
                        return IsHalfOver() == false;
                    }
                case Enum_ARR_TargetStateType.BossMonster:
                    {
                        if (_isMonster == false)
                            return false;

                        return _monsterGradeType == V2Enum_ARR_MonsterGradeType.Boss || _monsterGradeType == V2Enum_ARR_MonsterGradeType.Named;
                    }
                case Enum_ARR_TargetStateType.NormalMonster:
                    {
                        if (_isMonster == false)
                            return false;

                        return _monsterGradeType == V2Enum_ARR_MonsterGradeType.Normal;
                    }
                default:
                    return true;
            }
        }
        //------------------------------------------------------------------------------------
        public void ApplyVampiricDmg(double heal)
        {
            heal += heal * _increaseHealEffect;

            heal = System.Math.Round(heal);

            Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.VampiricDmg, heal, _varianceTransform);
            InCreaseHP(heal);
        }
        //------------------------------------------------------------------------------------
        public void ApplySynergyVampiricDmg(double damage)
        {
            if (_outputSynergyVampiricDmg <= 0)
                return;

            double heal = damage * _outputSynergyVampiricDmg;
            heal += heal * _increaseHealEffect;

            heal = System.Math.Round(heal);

            Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.VampiricDmg, heal, _varianceTransform);
            InCreaseHP(heal);
        }
        //------------------------------------------------------------------------------------
        public void ApplyKillMoney()
        {
            if (_killingMoneyQueue.Count > 0)
            {
                var node = _killingMoneyQueue.First;
                while (node != null)
                {
                    if (IsTargetState(node.Value) == true)
                    {
                        //Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.KillingMoney, 0.0, _varianceTransform);
                        Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), node.Value.CCValue);
                        Managers.SynergyManager.Instance.ShowInterestText(node.Value.CCValue);
                        PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType.KillingMoney);
                    }

                    node = node.Next;
                }
            }

            _skillActiveController.AddKillCount(1);
        }
        //------------------------------------------------------------------------------------
        public double GetDoubleInterest(double gold)
        {
            double addGold = 0.0;

            if (_doubleInterestQueue.Count > 0)
            {
                var node = _doubleInterestQueue.First;
                while (node != null)
                {
                    if (IsTargetState(node.Value) == true)
                    {
                        Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.DoubleInterest, 0.0, _varianceTransform);
                        addGold += gold;
                        PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType.DoubleInterest);
                    }

                    node = node.Next;
                }
            }

            return addGold;
        }
        //------------------------------------------------------------------------------------
        public double GetIncreaseGasGambleProb()
        {
            double addGold = 0.0;

            if (_increaseGasGambleProbQueue.Count > 0)
            {
                var node = _increaseGasGambleProbQueue.First;
                while (node != null)
                {
                    addGold += node.Value.CCValue;
                    node = node.Next;
                }
            }

            return addGold * 100;
        }
        //------------------------------------------------------------------------------------
        public double GetIncreaseGasHpHeal()
        {
            double addGold = 0.0;

            if (_increaseGasHpHealQueue.Count > 0)
            {
                var node = _increaseGasHpHealQueue.First;
                while (node != null)
                {
                    addGold += node.Value.CCValue;
                    node = node.Next;
                }
            }

            return addGold * 100;
        }
        //------------------------------------------------------------------------------------
        public double GetIncreaseJokerProb()
        {
            double addGold = 0.0;

            if (_increaseJokerProbQueue.Count > 0)
            {
                var node = _increaseJokerProbQueue.First;
                while (node != null)
                {
                    addGold += node.Value.CCValue;
                    node = node.Next;
                }
            }

            return addGold * 1000000;
        }
        //------------------------------------------------------------------------------------
        public int GetIncreaseSynergyCount()
        {
            return _increaseSynergyCount.ToInt();
        }
        //------------------------------------------------------------------------------------
        public void RefreshShield()
        {
            if (MaxHP == 0)
                return;

            double shield = 0.0;

            if (_shieldQueue.Count > 0)
            {
                var node = _shieldQueue.First;
                while (node != null)
                {
                    shield += node.Value.RemainShield;
                    node = node.Next;
                }
            }

            _shield = shield;
            ShieldRatio = shield / MaxHP;

            if (_uiCharacterState != null)
                _uiCharacterState.SetShieldBar(ShieldRatio);
        }
        //------------------------------------------------------------------------------------
        public double GetDecreaseGamblePrice()
        {
            double addGold = 0.0;

            if (_decreaseGamblePriceQueue.Count > 0)
            {
                var node = _decreaseGamblePriceQueue.First;
                while (node != null)
                {
                    addGold += node.Value.CCValue;
                    node = node.Next;
                }
            }

            return addGold;
        }
        //------------------------------------------------------------------------------------
        public double GetOutputSynergyDescendDmg()
        {
            return _outputSynergyDescendDmg;
        }
        //------------------------------------------------------------------------------------
        public void RefreshInterestAmount(double interest)
        {
            if (_interestAmount != interest)
            {
                foreach (var pair in _goldIncreaseValue)
                {
                    DeCreaseGoldIncreaseBuffStat(pair.Key);
                }

                _interestAmount = interest;

                foreach (var pair in _goldIncreaseValue)
                {
                    InCreaseGoldIncreaseBuffStat(pair.Key);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshUseGoldAmount(double interest)
        {
            if (_useGoldAmount != interest)
            {
                foreach (var pair in _goldUseValue)
                {
                    DeCreaseGoldUseBuffStat(pair.Key);
                }

                _useGoldAmount += interest;

                foreach (var pair in _goldUseValue)
                {
                    InCreaseGoldUseBuffStat(pair.Key);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void PlayOnDamageDirection()
        { 

        }
        //------------------------------------------------------------------------------------
        public virtual Vector2 GetProjectileAddPos()
        {
            return Vector2.zero;

            //if (_bodyRenderer == null)
            //    return Vector2.zero;

            //Vector2 addpos = _bodyRenderer.bounds.size * 0.1f;

            //addpos.x = Random.Range(addpos.x * -0.1f, addpos.x);
            //addpos.y = Random.Range(addpos.y * -0.1f, addpos.y) + (_bodyRenderer.bounds.size.y * 0.3f);

            //return addpos;
        }
        //------------------------------------------------------------------------------------
        protected virtual void PlayOnDamageAfterCC(V2CCData v2CCData)
        {
            if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.Stun)
            {
                ChangeState(CharacterState.Hit);
                SetHitRecoveryTime(v2CCData.CCTime);
            }
        }
        //------------------------------------------------------------------------------------
        protected void SetHitRecoveryTime(float hitTime)
        {
            if ((_hitRecoveryStartTime + _hitRecoveryTime) - Time.time < hitTime)
            {
                _hitRecoveryStartTime = Time.time;
                _hitRecoveryTime = hitTime;
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


            if (_shieldQueue.Count > 0)
            {
                var node = _shieldQueue.First;
                while (node != null)
                {
                    ShieldData shieldData = node.Value;
                    double shieldValue = shieldData.RemainShield;

                    shieldValue -= decreaseValue;
                    if (shieldValue <= 0)
                    {
                        decreaseValue -= shieldData.RemainShield;
                        Managers.SkillManager.Instance.PoolShieldData(shieldData);
                        var nodeNext = node.Next;
                        _shieldQueue.Remove(node);
                        node = nodeNext;
                    }
                    else
                    {
                        shieldData.RemainShield -= decreaseValue;
                        decreaseValue = 0;
                        break;
                    }
                }

                RefreshShield();
            }

            if (decreaseValue <= 0)
                return;

            double beforeHp = _currentHP;

            SetHP(_currentHP - decreaseValue);

            if (_isMonster == false)
                return;

            if (MapContainer.PlayingStage == Define.TestDefence_Stage
                && MapContainer.PlayingWave + 1 >= Define.TestDefence_Wave)
            {
                if (_characterState != CharacterState.Dead && _currentHP > 0.0)
                {
                    if ((beforeHp / _maxHP) >= Define.TestDefence_HpRatio
                        && (_currentHP / _maxHP) < Define.TestDefence_HpRatio)
                    {
                        double defence = _characterStatOperator.GetDefaultValue(V2Enum_Stat.Defence);
                        _characterStatOperator.SetDefaultStat(V2Enum_Stat.Defence, defence + Define.TestDefence_Value);
                        _characterStatOperator.RefreshOutputStatValue(V2Enum_Stat.Defence);
                    }
                }
            }
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
        private void Update()
        {
            if (_skillActiveController != null)
                _skillActiveController.Updated();

            if (_needRefreshBuff == true)
            { 
                OnRefreshBuff();
                _needRefreshBuff = false;

                if (_iFFType == IFFType.IFF_Friend)
                    Managers.ARRRStatManager.Instance.RefreshBuffUI(V2Enum_Stat.Max);
            }

            if (_shieldQueue.Count > 0)
            {
                var node = _shieldQueue.First;
                bool refreshshield = false;

                while (node != null)
                {
                    ShieldData shieldData = node.Value;
                    if (shieldData.ShieldEndTime < Time.time)
                    {
                        Managers.SkillManager.Instance.PoolShieldData(shieldData);
                        var nodeNext = node.Next;
                        _shieldQueue.Remove(node);
                        node = nodeNext;
                        refreshshield = true;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }

                RefreshShield();

                if (refreshshield == true)
                    RefreshShield();
            }

            Updated();

            if (_characterState != CharacterState.Dead
                && _characterState != CharacterState.None)
            {
                double recoveryvalue = GetOutPutMyStat(V2Enum_Stat.HpRecovery);

                double ratio = recoveryvalue * Define.PerSkillEffectRecoverValue;


                InCreaseHP(ratio * MaxHP * Time.deltaTime);

                if (MapContainer.PlayingStage == Define.TestDamage_Stage)
                {
                    DeCreaseHP(Define.TestDamage_Value * Time.deltaTime);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected virtual void Updated()
        { 

        }
        //------------------------------------------------------------------------------------
        public void AddPlayParticle(ParticlePoolElement particlePoolElement)
        {
            if (particlePoolElement != null)
            {
                if (_aliveParticle.ContainsKey(particlePoolElement) == false)
                {
                    particlePoolElement.SetSimulationSpeed(_aniControllerSpeed);

                    _aliveParticle.Add(particlePoolElement, particlePoolElement);
                    particlePoolElement.ParticleStop += EndParticle;
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void EndParticle(ParticlePoolElement particlePoolElement)
        {
            if (particlePoolElement == null)
                return;

            if (_aliveParticle.ContainsKey(particlePoolElement) == true)
            { 
                _aliveParticle.Remove(particlePoolElement);
                particlePoolElement.ParticleStop -= EndParticle;
            }
        }
        //------------------------------------------------------------------------------------
        public void ReleaseParticle()
        {
            foreach (KeyValuePair<ParticlePoolElement, ParticlePoolElement> pair in _aliveParticle)
            {
                pair.Value.ParticleStop -= EndParticle;
                pair.Value.StopParticle();
            }

            _aliveParticle.Clear();
        }
        //------------------------------------------------------------------------------------
        public void ReleaseCharacterBase()
        {
            _reviveQueue.Clear();
            _killingMoneyQueue.Clear();
            _doubleInterestQueue.Clear();
            _increaseGasGambleProbQueue.Clear();
            _increaseGasHpHealQueue.Clear();
            _increaseJokerProbQueue.Clear();
            _decreaseGamblePriceQueue.Clear();
            _increaseInterestRatet = 0;
            _increaseSynergyCount = 0;
            _damageReduce = 0;
            _increaseHealEffect = 0;
            _synergyVampiricDmg.Clear();
            _outputSynergyVampiricDmg = 0;
            _outputSynergyDescendDmg = 0;

            _synergyAmount.Clear();
            _synergyIncreaseAtt.Clear();
            _synergyIncreaseDef.Clear();
            _synergyIncreaseHp.Clear();

            var node = _shieldQueue.First;
            while (node != null)
            {
                ShieldData shieldData = node.Value;

                Managers.SkillManager.Instance.PoolShieldData(shieldData);

                node = node.Next;
            }

            _shieldQueue.Clear();

            if (_uiCharacterState != null)
                _uiCharacterState.SetShieldBar(0);

            _shield = 0;
            ShieldRatio = 0;

            _gradeAfterSkill.Clear();
            _allDamageAfterSkill.Clear();
            _synergyAfterSkill.Clear();
            _synergySkillModule.Clear();
            _interestAmount = 0;
            _goldIncreaseValue.Clear();
            _useGoldAmount = 0;
            _goldUseValue.Clear();

            _myBuffValues.Clear();

            if (_skillCustomActionPlayer != null)
                _skillCustomActionPlayer.Release();

            if (_skillProjectilePlayer != null)
                _skillProjectilePlayer.Release();

            if (_skillDirectVisioningPlayer != null)
                _skillDirectVisioningPlayer.Release();

            if (_skillPiercePlayer != null)
                _skillPiercePlayer.Release();

            if (_skillVoidPlayer != null)
                _skillVoidPlayer.Release();

            if (_skillRepeatAttackPlayer != null)
                _skillRepeatAttackPlayer.Release();
        }
        //------------------------------------------------------------------------------------
        public virtual void HPRecoverPer(double ratio)
        {
            if (CharacterState == CharacterState.Dead)
                return;

            if (IFFType != IFFType.IFF_Friend)
            {
                if (CharacterState == CharacterState.None)
                    return;
            }
            

            double increaseValue = _maxHP * ratio * Define.PercentageRecoverValue;
            increaseValue += increaseValue * _increaseHealEffect;

            increaseValue = System.Math.Round(increaseValue);

            Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.RecoveryHP, increaseValue, _varianceTransform.position);

            InCreaseHP(increaseValue);
        }
        //------------------------------------------------------------------------------------
        public virtual void HPRecoverPer(V2CCData v2CCData)
        {
            if (CharacterState == CharacterState.Dead || CharacterState == CharacterState.None)
                return;

            if (v2CCData.CCTime <= 0)
            {
                HPRecoverPer(v2CCData.CCValue);
            }
            else
            {
                if (_skillEffectController != null)
                {
                    _skillEffectController.PlayApplyCC(v2CCData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public virtual void HPDeCreasePer_CurrentHP(double ratio)
        {
            if (CharacterState == CharacterState.Dead)
                return;

            double decreaseValue = _currentHP * ratio * Define.PercentageRecoverValue;

            decreaseValue = System.Math.Round(decreaseValue);

            Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.NomalDamage, decreaseValue, _varianceTransform.position);

            DeCreaseHP(decreaseValue);

            if (_currentHP <= 0)
            {
                ChangeState(CharacterState.Dead);
            }
        }
        //------------------------------------------------------------------------------------
        public virtual void CCDOT(double value)
        {
            if (CharacterState == CharacterState.Dead)
                return;

            double decreaseValue = _maxHP * value;

            decreaseValue = System.Math.Round(decreaseValue);

            Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.NomalDamage, decreaseValue, _varianceTransform.position);

            DeCreaseHP(decreaseValue);

            if (_currentHP <= 0)
            {
                ChangeState(CharacterState.Dead);
            }
        }
        //------------------------------------------------------------------------------------
        public virtual void CCBurn(double value)
        {
            if (CharacterState == CharacterState.Dead)
                return;

            double decreaseValue = value;

            decreaseValue = System.Math.Round(decreaseValue);

            Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.NomalDamage, decreaseValue, _varianceTransform.position);

            DeCreaseHP(decreaseValue);

            if (_currentHP <= 0)
            {
                ChangeState(CharacterState.Dead);
            }
        }
        //------------------------------------------------------------------------------------
        public virtual void CCHOT(double value)
        {
            HPRecoverPer(value);
        }
        //------------------------------------------------------------------------------------
        public virtual void HealPerAttack(double value)
        {
            double increaseValue = GetOutPutMyStat(V2Enum_Stat.Attack) * value;

            increaseValue += increaseValue * _increaseHealEffect;
            increaseValue = System.Math.Round(increaseValue);

            Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.RecoveryHP, increaseValue, _varianceTransform.position);
            InCreaseHP(increaseValue);
        }
        //------------------------------------------------------------------------------------
        public virtual void EndCC(V2Enum_SkillEffectType cCType)
        {

        }
        //------------------------------------------------------------------------------------
        public virtual void AniCallBackRecv(string anicallback)
        { 

        }
        //------------------------------------------------------------------------------------
        public void ChangeCharacterLookAtDirection(Enum_ARR_LookDirection direction)
        {
            if (direction == _lookDirection)
                return;

            _lookDirection = direction;
            Vector3 rotate = transform.eulerAngles;

            float selectRatote = 0.0f;

            if (_lookDirection == Enum_ARR_LookDirection.Left)
                selectRatote = 180.0f;

            rotate.y = selectRatote;

            transform.eulerAngles = rotate;

            if (_mySkeletonAnimationHandler != null)
            {
                Vector3 aniRot = _mySkeletonAnimationHandler.transform.eulerAngles;
                _mySkeletonAnimationHandler.transform.eulerAngles = aniRot;
            }

            if (_uiCharacterState != null)
            {
                rotate = _uiCharacterState.transform.localEulerAngles;
                rotate.y = selectRatote;

                _uiCharacterState.transform.localEulerAngles = rotate;
            }

            if (_skillEffectController != null)
            {
                _skillEffectController.SetPainterRotation(selectRatote);
            }
        }
        //------------------------------------------------------------------------------------
        public void ChangeCharacterLookAtDirection_Target(Transform targetTrans)
        {
            Vector2 direction = targetTrans.transform.position - transform.position;
            direction.Normalize();

            ChangeCharacterLookAtDirection(direction.x < 0 ? Enum_ARR_LookDirection.Left : Enum_ARR_LookDirection.Right);
        }
        //------------------------------------------------------------------------------------
        public virtual double GetOutPutMyStat(V2Enum_Stat v2Enum_Stat)
        {
            return _characterStatOperator.GetOutPutMyStat(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public virtual float GetOutputAttackRange()
        {
            return 1.0f;
        }
        //------------------------------------------------------------------------------------
        public void SetAttackTarget(CharacterControllerBase characterControllerBase)
        {
            _attackTarget = characterControllerBase;
        }
        //------------------------------------------------------------------------------------
        protected void SetNewTarget()
        {
            if (_selectPlaySkillData != null && _selectPlaySkillData.SkillBaseData != null && _selectPlaySkillData.SkillBaseData.SkillDamageIndex != null)
                _attackTarget = Managers.AggroManager.Instance.GetIFFTargetCharacter(_mySearchType, this, _selectPlaySkillData.GetTargetRange());
            else
                _attackTarget = Managers.AggroManager.Instance.GetIFFTargetCharacter(_mySearchType, this);
        }
        //------------------------------------------------------------------------------------
        protected virtual void PlayAnimation(CharacterState state)
        {
            if (state == CharacterState.Idle || state == CharacterState.Run)
                PlayAnimation(state, true);
            else if (state == CharacterState.Skill)
            {
                if (_currentSpineModelData.AnimationList.Find(x => x.stateName == "Skill") == null)
                    PlayAnimation(CharacterState.Attack, true);
                else
                    PlayAnimation(state, true);
            }
            else
                PlayAnimation(state, true);
        }
        //------------------------------------------------------------------------------------
        public void PlayAnimation(CharacterState animation, bool loop)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.PlayAnimation_Once(animation, loop);
            }

        }
        //------------------------------------------------------------------------------------
        public void PlayAnimation(string animation, bool loop)
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.PlayAnimation_Once(animation, loop);
                
                //if (loop == true)
                //{
                //    _mySkeletonAnimationHandler.PlayAnimation(animation);
                //}
                //else
                //    _mySkeletonAnimationHandler.PlayAnimation_Once(animation, loop);
            }

        }
        //------------------------------------------------------------------------------------
        public virtual void PlayAnimation(CharacterState state, string aniId)
        {

        }
        //------------------------------------------------------------------------------------
        protected void ReleaseHitDirection()
        {
            EnableHitEffect(Color.white);

            if (_hitDirectionCoroutine != null)
                StopCoroutine(_hitDirectionCoroutine);

            _hitDirectionCoroutine = null;
        }
        //------------------------------------------------------------------------------------
        protected IEnumerator HitColorEffect()
        {
            if (_hitRecoveryTime != 0.0f)
            {
                while (_hitRecoveryStartTime + _hitRecoveryTime >= Time.time)
                {

                    float ratio = (Time.time - _hitRecoveryStartTime) / _hitRecoveryTime;
                    EnableHitEffect(StaticResource.Instance.GetHitColorGradient().Evaluate(ratio));
                    yield return null;
                }
            }

            _hitDirectionCoroutine = null;
            ReleaseHitDirection();
        }
        //------------------------------------------------------------------------------------
        protected void EnableHitEffect(Color color)
        {
            if (_mySkeletonAnimationHandler != null)
                _mySkeletonAnimationHandler.SetColor(color);
        }
        //------------------------------------------------------------------------------------
        public void SetRootTransform(Transform myRoot)
        {
            _myRoot = myRoot;

            transform.position = _myRoot.position;
        }
        //------------------------------------------------------------------------------------
        public void SetRootPos()
        {
            if (_myRoot != null)
                transform.position = _myRoot.position;
        }
        //------------------------------------------------------------------------------------
        public void ChangeSearchType()
        {
            if (_selectPlaySkillData != null)
                _mySearchType = _selectPlaySkillData.SkillBaseData.TargetCondition;
            else
                _mySearchType = _defaultSearchType;
        }
        //------------------------------------------------------------------------------------
        public void AddRevive(V2CCData v2CCData)
        {
            _reviveQueue.Enqueue(v2CCData);
        }
        //------------------------------------------------------------------------------------
        public void AddKillingMoney(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _killingMoneyQueue.AddLast(v2CCData);
        }
        //------------------------------------------------------------------------------------
        public void AddDoubleInterest(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _doubleInterestQueue.AddLast(v2CCData);
        }
        //------------------------------------------------------------------------------------
        public void AddIncreaseGasGambleProb(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _increaseGasGambleProbQueue.AddLast(v2CCData);
        }
        //------------------------------------------------------------------------------------
        public void AddIncreaseGasHpHeal(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _increaseGasHpHealQueue.AddLast(v2CCData);
        }
        //------------------------------------------------------------------------------------
        public void AddIncreaseJokerProb(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _increaseJokerProbQueue.AddLast(v2CCData);
        }
        //------------------------------------------------------------------------------------
        public void AddDecreaseGamblePriceQueue(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _decreaseGamblePriceQueue.AddLast(v2CCData);
        }
        //------------------------------------------------------------------------------------
        public void AddIncreaseInterestRate(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _increaseInterestRatet += v2CCData.CCValue;
        }
        //------------------------------------------------------------------------------------
        public void AddIncreaseSynergyCount(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            _increaseSynergyCount += v2CCData.CCValue;
        }
        //------------------------------------------------------------------------------------
        public void IncreaseDamageReduce(double value)
        {
            if (_isMonster == true)
                return;

            _damageReduce += value;
        }
        //------------------------------------------------------------------------------------
        public void DecreaseDamageReduce(double value)
        {
            if (_isMonster == true)
                return;

            _damageReduce -= value;
        }
        //------------------------------------------------------------------------------------
        public void AddIncreaseHealEffect(double value)
        {
            if (_isMonster == true)
                return;

            _increaseHealEffect += value;
        }
        //------------------------------------------------------------------------------------
        public void AddSynergyVampiricDmg(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Max;

            if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.VampiricDmgFire)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.VampiricDmgGold)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Yellow;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.VampiricDmgWater)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Blue;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.VampiricDmgThunder)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.White;

            if (_synergyVampiricDmg.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyVampiricDmg.Add(v2Enum_ARR_SynergyType, 0);

            _synergyVampiricDmg[v2Enum_ARR_SynergyType] += v2CCData.CCValue;

            RefreshOutputSynergyVampiricDmg();
        }
        //------------------------------------------------------------------------------------
        private void RefreshOutputSynergyVampiricDmg()
        {
            _outputSynergyVampiricDmg = 0;

            foreach (var pair in _synergyVampiricDmg)
            {
                _outputSynergyVampiricDmg += pair.Value * Managers.SynergyManager.Instance.GetSynergyStack(pair.Key);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddSynergyDescendDmg(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Max;

            if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.DescendDmgFireCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.DescendDmgGoldCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Yellow;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.DescendDmgWaterCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Blue;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.DescendDmgThunderCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.White;

            if (_synergyDescendDmg.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyDescendDmg.Add(v2Enum_ARR_SynergyType, 0);

            _synergyDescendDmg[v2Enum_ARR_SynergyType] += v2CCData.CCValue;

            RefreshOutputSynergyDescendDmg();
        }
        //------------------------------------------------------------------------------------
        private void RefreshOutputSynergyDescendDmg()
        {
            _outputSynergyDescendDmg = 0;

            foreach (var pair in _synergyDescendDmg)
            {
                _outputSynergyDescendDmg += pair.Value * Managers.SynergyManager.Instance.GetSynergyStack(pair.Key);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddSynergyStatBuff_Att(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Max;

            if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseAttFireCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseAttGoldCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Yellow;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseAttWaterCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Blue;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseAttThunderCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.White;

            if (_synergyIncreaseAtt.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyIncreaseAtt.Add(v2Enum_ARR_SynergyType, 0);

            _synergyIncreaseAtt[v2Enum_ARR_SynergyType] += v2CCData.CCValue;
        }
        //------------------------------------------------------------------------------------
        public void AddSynergyStatBuff_Def(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Max;

            if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseDefFireCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseDefGoldCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Yellow;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseDefWaterCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Blue;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseDefThunderCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.White;

            if (_synergyIncreaseDef.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyIncreaseDef.Add(v2Enum_ARR_SynergyType, 0);

            _synergyIncreaseDef[v2Enum_ARR_SynergyType] += v2CCData.CCValue;
        }
        //------------------------------------------------------------------------------------
        public void AddSynergyStatBuff_Hp(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Max;

            if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseHpFireCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Red;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseHpGoldCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Yellow;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseHpWaterCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.Blue;
            else if (v2CCData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseHpThunderCount)
                v2Enum_ARR_SynergyType = V2Enum_ARR_SynergyType.White;

            if (_synergyIncreaseHp.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyIncreaseHp.Add(v2Enum_ARR_SynergyType, 0);

            _synergyIncreaseHp[v2Enum_ARR_SynergyType] += v2CCData.CCValue;
        }
        //------------------------------------------------------------------------------------
        public void AllRefreshSynergyStat()
        {
            for (int i = V2Enum_ARR_SynergyType.Red.Enum32ToInt(); i < V2Enum_ARR_SynergyType.Max.Enum32ToInt(); ++i)
            {
                V2Enum_ARR_SynergyType v2Enum_Stat = i.IntToEnum32<V2Enum_ARR_SynergyType>();
                InCreaseSynergyBuffStat(v2Enum_Stat);
            }
        }
        //------------------------------------------------------------------------------------
        public void InCreaseSynergyBuffStat(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            if (_synergyAmount.ContainsKey(v2Enum_ARR_SynergyType) == false)
                return;

            int amount = _synergyAmount[v2Enum_ARR_SynergyType];

            if (_synergyIncreaseAtt.ContainsKey(v2Enum_ARR_SynergyType) == true)
            {
                double effectValue = _synergyIncreaseAtt[v2Enum_ARR_SynergyType];
                double buffvalue = effectValue * amount;
                InCreaseBuffStat(V2Enum_Stat.Attack, buffvalue);
            }
            
            if (_synergyIncreaseDef.ContainsKey(v2Enum_ARR_SynergyType) == true)
            {
                double effectValue = _synergyIncreaseDef[v2Enum_ARR_SynergyType];
                double buffvalue = effectValue * amount;
                InCreaseBuffStat(V2Enum_Stat.Defence, buffvalue);
            }

            if (_synergyIncreaseHp.ContainsKey(v2Enum_ARR_SynergyType) == true)
            {
                double effectValue = _synergyIncreaseHp[v2Enum_ARR_SynergyType];
                double buffvalue = effectValue * amount;
                InCreaseBuffStat(V2Enum_Stat.HP, buffvalue);
            }
        }
        //------------------------------------------------------------------------------------
        public void DeCreaseGoldIncreaseBuffStat(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            if (_synergyAmount.ContainsKey(v2Enum_ARR_SynergyType) == false)
                return;

            int amount = _synergyAmount[v2Enum_ARR_SynergyType];

            if (_synergyIncreaseAtt.ContainsKey(v2Enum_ARR_SynergyType) == true)
            {
                double effectValue = _synergyIncreaseAtt[v2Enum_ARR_SynergyType];
                double buffvalue = effectValue * amount;
                DeCreaseBuffStat(V2Enum_Stat.Attack, buffvalue);
            }

            if (_synergyIncreaseDef.ContainsKey(v2Enum_ARR_SynergyType) == true)
            {
                double effectValue = _synergyIncreaseDef[v2Enum_ARR_SynergyType];
                double buffvalue = effectValue * amount;
                DeCreaseBuffStat(V2Enum_Stat.Defence, buffvalue);
            }

            if (_synergyIncreaseHp.ContainsKey(v2Enum_ARR_SynergyType) == true)
            {
                double effectValue = _synergyIncreaseHp[v2Enum_ARR_SynergyType];
                double buffvalue = effectValue * amount;
                DeCreaseBuffStat(V2Enum_Stat.HP, buffvalue);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshSynergy(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, int stack)
        {
            if (_synergyAmount.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyAmount.Add(v2Enum_ARR_SynergyType, 0);

            if (_synergyAmount[v2Enum_ARR_SynergyType] == stack)
                return;

            DeCreaseGoldIncreaseBuffStat(v2Enum_ARR_SynergyType);

            _synergyAmount[v2Enum_ARR_SynergyType] = stack;

            InCreaseSynergyBuffStat(v2Enum_ARR_SynergyType);

            // 흡혈은 이걸로 땡
            RefreshOutputSynergyVampiricDmg();

            // 강림 데미지 추가 강화 이걸로 끝
            RefreshOutputSynergyDescendDmg();
        }
        //------------------------------------------------------------------------------------
        public void AddShield(V2CCData v2CCData)
        {
            if (v2CCData.CCValue <= 0)
                return;

            if (v2CCData.CCTime <= 0)
                return;

            ShieldData shieldData = Managers.SkillManager.Instance.GetShieldData();
            shieldData.RemainShield = MaxHP * v2CCData.CCValue;
            shieldData.ShieldEndTime = Time.time + v2CCData.CCTime;

            _shieldQueue.AddLast(shieldData);

            RefreshShield();
        }
        //------------------------------------------------------------------------------------
        public void AddGoldIncrease(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            if (_goldIncreaseValue.ContainsKey(v2CCData.CCTypeEnum) == false)
            { 
                _goldIncreaseValue.Add(v2CCData.CCTypeEnum, v2CCData.CCValue);
                InCreaseGoldIncreaseBuffStat(v2CCData.CCTypeEnum);
            }
            else
            {
                DeCreaseGoldIncreaseBuffStat(v2CCData.CCTypeEnum);
                _goldIncreaseValue[v2CCData.CCTypeEnum] += v2CCData.CCValue;
            }
        }
        //------------------------------------------------------------------------------------
        public void InCreaseGoldIncreaseBuffStat(V2Enum_SkillEffectType v2Enum_SkillEffectType)
        {
            if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseAtt
                || v2Enum_SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseArmor)
            {
                if (_goldIncreaseValue.ContainsKey(v2Enum_SkillEffectType) == false)
                    return;

                double effectValue = _goldIncreaseValue[v2Enum_SkillEffectType];
                double buffvalue = effectValue * _interestAmount * 0.01;

                InCreaseBuffStat(v2Enum_SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseAtt ? V2Enum_Stat.Attack : V2Enum_Stat.Defence, buffvalue);
            }
        }
        //------------------------------------------------------------------------------------
        public void DeCreaseGoldIncreaseBuffStat(V2Enum_SkillEffectType v2Enum_SkillEffectType)
        {
            if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseAtt
                || v2Enum_SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseArmor)
            {
                if (_goldIncreaseValue.ContainsKey(v2Enum_SkillEffectType) == false)
                    return;

                double effectValue = _goldIncreaseValue[v2Enum_SkillEffectType];
                double buffvalue = effectValue * _interestAmount * 0.01;

                DeCreaseBuffStat(v2Enum_SkillEffectType == V2Enum_SkillEffectType.GoldIncreaseAtt ? V2Enum_Stat.Attack : V2Enum_Stat.Defence, buffvalue);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddGoldUse(V2CCData v2CCData)
        {
            if (_isMonster == true)
                return;

            if (_goldUseValue.ContainsKey(v2CCData.CCTypeEnum) == false)
            {
                _goldUseValue.Add(v2CCData.CCTypeEnum, v2CCData.CCValue);
                InCreaseGoldUseBuffStat(v2CCData.CCTypeEnum);
            }
            else
            {
                DeCreaseGoldUseBuffStat(v2CCData.CCTypeEnum);
                _goldUseValue[v2CCData.CCTypeEnum] += v2CCData.CCValue;
                InCreaseGoldUseBuffStat(v2CCData.CCTypeEnum);
            }
        }
        //------------------------------------------------------------------------------------
        public void InCreaseGoldUseBuffStat(V2Enum_SkillEffectType v2Enum_SkillEffectType)
        {
            if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.WasteGoldBuff)
            {
                if (_goldUseValue.ContainsKey(v2Enum_SkillEffectType) == false)
                    return;

                double effectValue = _goldUseValue[v2Enum_SkillEffectType];
                double buffvalue = effectValue * _useGoldAmount * 0.01;

                InCreaseBuffStat(V2Enum_Stat.Attack, buffvalue);
                InCreaseBuffStat(V2Enum_Stat.Defence, buffvalue);
            }
        }
        //------------------------------------------------------------------------------------
        public void DeCreaseGoldUseBuffStat(V2Enum_SkillEffectType v2Enum_SkillEffectType)
        {
            if (v2Enum_SkillEffectType == V2Enum_SkillEffectType.WasteGoldBuff)
            {
                if (_goldUseValue.ContainsKey(v2Enum_SkillEffectType) == false)
                    return;

                double effectValue = _goldUseValue[v2Enum_SkillEffectType];
                double buffvalue = effectValue * _useGoldAmount * 0.01;

                DeCreaseBuffStat(V2Enum_Stat.Attack, buffvalue);
                DeCreaseBuffStat(V2Enum_Stat.Defence, buffvalue);
            }
        }
        //------------------------------------------------------------------------------------
        public virtual void EnablePassiveBuffSkill(SkillManageInfo skillManageInfo)
        {
            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Self)
            {
                int skillLevel = skillManageInfo.GetSkillLevel();

                if (skillBaseData.SkillEffect != null && skillBaseData.SkillEffect.Count > 0)
                {
                    for (int effect = 0; effect < skillBaseData.SkillEffect.Count; ++effect)
                    {
                        SkillEffectData skillEffectData = skillBaseData.SkillEffect[effect];

                        V2Enum_Stat v2Enum_Stat = ARRRStatOperator.ConvertCrowdControlTypeToStat(skillEffectData.SkillEffectType);

                        if (v2Enum_Stat != V2Enum_Stat.Max)
                        {
                            double value = Managers.SkillManager.Instance.GetSkillEffectValue(skillEffectData, skillLevel);

                            InCreaseBuffStat(v2Enum_Stat, value);
                        }
                    }
                }
            }
            else
            {
                if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Friendly)
                    Managers.BattleSceneManager.Instance.SetBuff(skillManageInfo, _iFFType);
                else if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Enemy)
                {
                    IFFType iFFType = _iFFType == IFFType.IFF_Friend ? IFFType.IFF_Foe : IFFType.IFF_Friend;
                    Managers.BattleSceneManager.Instance.SetBuff(skillManageInfo, iFFType);
                }
            }

        }
        //------------------------------------------------------------------------------------
        public virtual void ReleasePassiveBuffSkill(SkillManageInfo skillManageInfo)
        {
            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Self)
            {
                int skillLevel = skillManageInfo.GetSkillLevel();

                if (skillBaseData.SkillEffect != null && skillBaseData.SkillEffect.Count > 0)
                {
                    for (int effect = 0; effect < skillBaseData.SkillEffect.Count; ++effect)
                    {
                        SkillEffectData skillEffectData = skillBaseData.SkillEffect[effect];

                        V2Enum_Stat v2Enum_Stat = ARRRStatOperator.ConvertCrowdControlTypeToStat(skillEffectData.SkillEffectType);

                        if (v2Enum_Stat != V2Enum_Stat.Max)
                        {
                            double value = Managers.SkillManager.Instance.GetSkillEffectValue(skillEffectData, skillLevel);

                            DeCreaseBuffStat(v2Enum_Stat, value);
                        }
                    }
                }
            }
            else
            {
                if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Friendly)
                    Managers.BattleSceneManager.Instance.ReleaseBuff(skillManageInfo, _iFFType);
                else if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Enemy)
                {
                    IFFType iFFType = _iFFType == IFFType.IFF_Friend ? IFFType.IFF_Foe : IFFType.IFF_Friend;
                    Managers.BattleSceneManager.Instance.ReleaseBuff(skillManageInfo, iFFType);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public virtual void PlaySkill(SkillManageInfo skillManageInfo, CharacterControllerBase target = null, bool isAfterSkill = false)
        { // 스킬 애니메이션 타이밍에 맞춰 발동
            PlaySkill(skillManageInfo, transform.position, target, isAfterSkill);
        }
        //------------------------------------------------------------------------------------
        public void PlaySkill(SkillManageInfo skillManageInfo, Vector3 actorPosition, CharacterControllerBase target = null, bool isAfterSkill = false)
        {
            if (skillManageInfo.SkillBaseData != null && skillManageInfo.SkillBaseData.SkillDamageIndex != null)
            {
                if (isAfterSkill == true)
                {
                    if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Projectile)
                    {
                        PlayProjectile(skillManageInfo, _attackTarget);
                        return;
                    }
                    else if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Pierce)
                    {
                        PlayPierce(skillManageInfo, _attackTarget);
                        return;
                    }
                    else if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Void)
                    {
                        PlayVoid(skillManageInfo, _attackTarget);
                        return;
                    }
                    else if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.RepeatAttack)
                    {
                        PlayRepeatAttack(skillManageInfo, _attackTarget);
                        return;
                    }
                }
                
            }
            

            V2SkillAttackData damageData = Managers.SkillManager.Instance.GetCreatureSkillAttackData(skillManageInfo, this, actorPosition);
            damageData.IsAfter = isAfterSkill;

            if (damageData.SelfEffecter != null)
            {
                for (int i = 0; i < damageData.SelfEffecter.Count; ++i)
                {
                    V2CCData v2CCData = damageData.SelfEffecter[i];

                    if (IsTargetState(v2CCData))
                    {
                        switch (v2CCData.CCTypeEnum)
                        {
                            case V2Enum_SkillEffectType.ResetUsedSkillCooltime:
                                {
                                    _skillActiveController.ResetSkillCoolTime(skillManageInfo);
                                    PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType.ResetUsedSkillCooltime);
                                    break;
                                }
                        }
                    }
                }
            }

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            if (skillBaseData.ResourceIndex == 5)
            {
                Managers.SoundManager.Instance.PlaySound(string.Format("5_{0}", skillManageInfo.OnTimePlayCount + 1));
            }
            else
                Managers.SoundManager.Instance.PlaySound(skillBaseData.SoundId);

            if (skillBaseData.SkillDamageIndex == null)
                PlayParticlePlayer(skillBaseData.ResourceIndex, skillManageInfo.OnTimePlayCount, null);
            else
            {
                if (skillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Direct)
                    PlayParticlePlayer(skillBaseData.ResourceIndex, skillManageInfo.OnTimePlayCount, null);
                else if(skillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Sunken)
                    PlayParticlePlayer(skillBaseData.ResourceIndex, skillManageInfo.OnTimePlayCount, target);
            }

            if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Self)
                OnDamage(damageData);
            else if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Friendly)
            {
                if (Managers.BattleSceneManager.Instance.CurrentBattleScene != null)
                    Managers.BattleSceneManager.Instance.CurrentBattleScene.TargetAllDamage(IFFType, damageData);
            }
            else
            {
                if (skillBaseData.TargetCount == 1)
                {
                    if (target != null)
                        target.OnDamage(damageData);
                    else
                        Managers.SkillTriggerManager.Instance.RecvDamageDate(skillManageInfo, damageData, this, actorPosition, null);
                }
                else
                {
                    if (target != null)
                        Managers.SkillTriggerManager.Instance.RecvDamageDate(skillManageInfo, damageData, this, actorPosition, target.MySkillHitReceiver);
                    else
                        Managers.SkillTriggerManager.Instance.RecvDamageDate(skillManageInfo, damageData, this, actorPosition, null);
                }
            }

            for (int i = 0; i < skillManageInfo.OtherSkillEffectDatas.Count; ++i)
            {
                SKillEffectInfo skillEffectData = skillManageInfo.OtherSkillEffectDatas[i];
                V2CCData v2CCData = Managers.SkillManager.Instance.GetV2CCData(skillManageInfo.GetSkillLevel(), skillEffectData, actorPosition);
                if (skillEffectData.TargetCheckType == Enum_ARR_TargetCheckType.Self)
                    ApplyCC(v2CCData);
                else if (skillEffectData.TargetCheckType == Enum_ARR_TargetCheckType.Friendly)
                {
                    if (Managers.BattleSceneManager.Instance.CurrentBattleScene != null)
                        Managers.BattleSceneManager.Instance.CurrentBattleScene.TargetAllEffect(IFFType, v2CCData);
                }
            }

            skillManageInfo.OnTimePlayCount++;

            if (isAfterSkill == true)
                return;

            for (int i = 0; i < skillManageInfo.AfterSkill.Count; ++i)
            {
                PlaySkill(skillManageInfo.AfterSkill[i], actorPosition, target, true);
            }
        }
        //------------------------------------------------------------------------------------
        public void TargetDirectDamage(SkillManageInfo skillManageInfo, CharacterControllerBase target, Vector3 actorPosition, bool isAfterSkill = false)
        {
            if (target == null)
                return;

            if (target.IsDead == true)
                return;

            if (skillManageInfo.SkillBaseData != null && skillManageInfo.SkillBaseData.SkillDamageIndex != null)
            {
                if (isAfterSkill == true)
                {
                    if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Projectile)
                    {
                        PlayProjectile(skillManageInfo, _attackTarget);
                        return;
                    }
                    else if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Pierce)
                    {
                        PlayPierce(skillManageInfo, _attackTarget);
                        return;
                    }
                    else if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Void)
                    {
                        PlayVoid(skillManageInfo, _attackTarget);
                        return;
                    }
                    else if (skillManageInfo.SkillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.RepeatAttack)
                    {
                        PlayRepeatAttack(skillManageInfo, _attackTarget);
                        return;
                    }
                }

            }


            V2SkillAttackData damageData = Managers.SkillManager.Instance.GetCreatureSkillAttackData(skillManageInfo, this, actorPosition);
            damageData.IsAfter = isAfterSkill;

            if (damageData.SelfEffecter != null)
            {
                for (int i = 0; i < damageData.SelfEffecter.Count; ++i)
                {
                    V2CCData v2CCData = damageData.SelfEffecter[i];

                    if (IsTargetState(v2CCData))
                    {
                        switch (v2CCData.CCTypeEnum)
                        {
                            case V2Enum_SkillEffectType.ResetUsedSkillCooltime:
                                {
                                    _skillActiveController.ResetSkillCoolTime(skillManageInfo);
                                    PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType.ResetUsedSkillCooltime);
                                    break;
                                }
                        }
                    }
                }
            }

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            if (skillBaseData.ResourceIndex == 5)
            {
                Managers.SoundManager.Instance.PlaySound(string.Format("5_{0}", skillManageInfo.OnTimePlayCount + 1));
            }
            else
                Managers.SoundManager.Instance.PlaySound(skillBaseData.SoundId);

            if (skillBaseData.SkillDamageIndex == null)
                PlayParticlePlayer(skillBaseData.ResourceIndex, skillManageInfo.OnTimePlayCount, null);
            else
            {
                if (skillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Direct)
                    PlayParticlePlayer(skillBaseData.ResourceIndex, skillManageInfo.OnTimePlayCount, null);
                else if (skillBaseData.SkillDamageIndex.DamageType == V2Enum_ARR_DamageType.Sunken)
                    PlayParticlePlayer(skillBaseData.ResourceIndex, skillManageInfo.OnTimePlayCount, target);
            }

            if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Self)
                OnDamage(damageData);
            else if (skillBaseData.TargetCheckType == Enum_ARR_TargetCheckType.Friendly)
            {
                if (Managers.BattleSceneManager.Instance.CurrentBattleScene != null)
                    Managers.BattleSceneManager.Instance.CurrentBattleScene.TargetAllDamage(IFFType, damageData);
            }
            else
            {
                if (target != null)
                    target.OnDamage(damageData);
            }

            for (int i = 0; i < skillManageInfo.OtherSkillEffectDatas.Count; ++i)
            {
                SKillEffectInfo skillEffectData = skillManageInfo.OtherSkillEffectDatas[i];
                V2CCData v2CCData = Managers.SkillManager.Instance.GetV2CCData(skillManageInfo.GetSkillLevel(), skillEffectData, actorPosition);
                if (skillEffectData.TargetCheckType == Enum_ARR_TargetCheckType.Self)
                    ApplyCC(v2CCData);
                else if (skillEffectData.TargetCheckType == Enum_ARR_TargetCheckType.Friendly)
                {
                    if (Managers.BattleSceneManager.Instance.CurrentBattleScene != null)
                        Managers.BattleSceneManager.Instance.CurrentBattleScene.TargetAllEffect(IFFType, v2CCData);
                }
            }

            skillManageInfo.OnTimePlayCount++;

            if (isAfterSkill == true)
                return;

            for (int i = 0; i < skillManageInfo.AfterSkill.Count; ++i)
            {
                TargetDirectDamage(skillManageInfo.AfterSkill[i], target, actorPosition, true);
            }
        }
        //------------------------------------------------------------------------------------
        protected void PlayParticlePlayer(int index, int order, CharacterControllerBase target)
        {
            if (_skillParticlePlayer != null)
                _skillParticlePlayer.PlayParticle(index, order, target);
        }
        //------------------------------------------------------------------------------------
        protected void PlayParticlePlayer_SkillEffect(V2Enum_SkillEffectType v2Enum_SkillEffectType, int order = 0)
        {
            if (_skillParticlePlayer != null)
                _skillParticlePlayer.PlayParticle(v2Enum_SkillEffectType.Enum32ToInt() + 10000, 0, null);
        }
        //------------------------------------------------------------------------------------
        public void PlayProjectile(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            if (_skillProjectilePlayer != null)
            {
                _skillProjectilePlayer.PlayProjectile(skillManageInfo, target);
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayDirectVisioning(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            if (_skillDirectVisioningPlayer != null)
            {
                _skillDirectVisioningPlayer.PlayDirectVisioning(skillManageInfo, target);
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayPierce(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            if (_skillPiercePlayer != null)
            {
                _skillPiercePlayer.PlayProjectile(skillManageInfo, target);
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayVoid(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            if (_skillVoidPlayer != null)
            {
                _skillVoidPlayer.PlayProjectile(skillManageInfo, target);
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayRepeatAttack(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            if (_skillRepeatAttackPlayer != null)
            {
                _skillRepeatAttackPlayer.PlayProjectile(skillManageInfo, target);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshBuff()
        {
            _needRefreshBuff = true;
        }
        //------------------------------------------------------------------------------------
        public virtual void OnRefreshBuff()
        {

        }
        //------------------------------------------------------------------------------------
        public double GetMyBuffValue(V2Enum_Stat v2Enum_Stat)
        {
            if (_myBuffValues.ContainsKey(v2Enum_Stat) == false)
                return 0.0;

            return _myBuffValues[v2Enum_Stat];
        }
        //------------------------------------------------------------------------------------
        public void InCreaseBuffStat(V2Enum_Stat v2Enum_Stat, double buffValue, bool immediately = false)
        {
            if (_myBuffValues.ContainsKey(v2Enum_Stat) == false)
                _myBuffValues.Add(v2Enum_Stat, 0.0);

            _myBuffValues[v2Enum_Stat] += buffValue;

            if (v2Enum_Stat == V2Enum_Stat.SkillCoolTimeDecrease)
                _skillActiveController.SetSkillCoolTimeRate((float)_myBuffValues[v2Enum_Stat]);

            if (immediately == false)
                RefreshBuff();
            else
            { 
                _characterStatOperator.RefreshOutputStatValue(v2Enum_Stat);

                if (_iFFType == IFFType.IFF_Friend)
                    Managers.ARRRStatManager.Instance.RefreshBuffUI(v2Enum_Stat);
            }
        }
        //------------------------------------------------------------------------------------
        public void DeCreaseBuffStat(V2Enum_Stat v2Enum_Stat, double buffValue, bool immediately = false)
        {
            if (_myBuffValues.ContainsKey(v2Enum_Stat) == false)
            {
                _myBuffValues.Add(v2Enum_Stat, 0.0);
            }
            
            double operValue = _myBuffValues[v2Enum_Stat];
            operValue -= buffValue;
            _myBuffValues[v2Enum_Stat] = operValue;

            if (v2Enum_Stat == V2Enum_Stat.SkillCoolTimeDecrease)
                _skillActiveController.SetSkillCoolTimeRate((float)_myBuffValues[v2Enum_Stat]);

            if (immediately == false)
                RefreshBuff();
            else
            { 
                _characterStatOperator.RefreshOutputStatValue(v2Enum_Stat);

                if (_iFFType == IFFType.IFF_Friend)
                    Managers.ARRRStatManager.Instance.RefreshBuffUI(v2Enum_Stat);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshStat(bool setFullHp = false)
        {
            _characterMoveSpeed = (float)(GetOutPutMyStat(V2Enum_Stat.MoveSpeed));

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
        public void AddGambleSkill(MainSkillData gambleSkillData, V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, SkillInfo skillInfo = null)
        {
            switch (gambleSkillData.MainSkillType)
            {
                case V2Enum_ARR_MainSkillType.AddSkill:
                    {
                        SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam1);
                        if (skillBaseData != null)
                        {
                            _skillActiveController.AddGambleSkillBaseData(gambleSkillData, v2Enum_ARR_SynergyType, skillInfo);

                            if (skillBaseData.SkillDamageIndex != null)
                            {
                                foreach (var node in _allDamageAfterSkill)
                                {
                                    _skillActiveController.AddAfterData(skillBaseData, node);
                                }
                            }

                            if (gambleSkillData.MainSkillGrade != V2Enum_Grade.Max)
                            {
                                if (_gradeAfterSkill.ContainsKey(gambleSkillData.MainSkillGrade) == true)
                                {
                                    foreach (var node in _gradeAfterSkill[gambleSkillData.MainSkillGrade])
                                    {
                                        _skillActiveController.AddAfterData(skillBaseData, node);
                                    }
                                }
                            }

                            if (v2Enum_ARR_SynergyType != V2Enum_ARR_SynergyType.Max)
                            {
                                if (_synergyAfterSkill.ContainsKey(v2Enum_ARR_SynergyType) == true)
                                {
                                    foreach (var node in _synergyAfterSkill[v2Enum_ARR_SynergyType])
                                    {
                                        _skillActiveController.AddAfterData(skillBaseData, node);
                                    }
                                }

                                if (_synergySkillModule.ContainsKey(v2Enum_ARR_SynergyType) == true)
                                {
                                    foreach (var node in _synergySkillModule[v2Enum_ARR_SynergyType])
                                    {
                                        _skillActiveController.AddModuleData(skillBaseData, node);
                                    }
                                }
                            }
                        }

                        break;
                    }
                case V2Enum_ARR_MainSkillType.SkillEnforge:
                    {
                        SkillModuleData skillModuleData = Managers.SkillManager.Instance.GetSkillModuleData(gambleSkillData.MainSkillTypeParam1);
                        if (skillModuleData == null)
                            return;

                        SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam2);
                        if (skillBaseData != null)
                            _skillActiveController.AddModuleData(skillBaseData, skillModuleData);

                        break;
                    }
                case V2Enum_ARR_MainSkillType.AfterSkill:
                    {
                        SkillBaseData AfterSkill = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam1);
                        if (AfterSkill == null)
                            return;

                        SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam2);
                        if (skillBaseData != null)
                            _skillActiveController.AddAfterData(skillBaseData, AfterSkill, skillInfo);

                        break;
                    }
                case V2Enum_ARR_MainSkillType.AfterGroupSkill:
                    {
                        SkillBaseData AfterSkill = Managers.SkillManager.Instance.GetSkillBaseData(gambleSkillData.MainSkillTypeParam1);
                        SkillManageInfo skillManageInfo = _skillActiveController.AddNewSkillManageInfo(AfterSkill, skillInfo);

                        switch (gambleSkillData.MainSkillTypeParam2)
                        {
                            case 11:
                                {
                                    for (int i = 0; i < _skillActiveController.OriginSkillList.Count; ++i)
                                    {
                                        SkillBaseData skillBaseData = _skillActiveController.OriginSkillList[i].SkillBaseData;
                                        _skillActiveController.AddAfterData(skillBaseData, AfterSkill, skillInfo);
                                    }

                                    break;
                                }
                            case 12:
                                {
                                    V2Enum_Grade v2Enum_Grade = gambleSkillData.MainSkillTypeParam3.GetDecrypted().IntToEnum32<V2Enum_Grade>();

                                    foreach (var pair in _skillActiveController.SkillLibrary)
                                    {
                                        if (pair.Value.GetSkillGrade() == v2Enum_Grade)
                                        {
                                            _skillActiveController.AddAfterData(pair.Key, AfterSkill, skillInfo);
                                        }
                                    }

                                    if (_gradeAfterSkill.ContainsKey(v2Enum_Grade) == false)
                                        _gradeAfterSkill.Add(v2Enum_Grade, new LinkedList<SkillBaseData>());

                                    _gradeAfterSkill[v2Enum_Grade].AddLast(AfterSkill);

                                    break;
                                }
                            case 13:
                                {
                                    foreach (var pair in _skillActiveController.SkillLibrary)
                                    {
                                        if (pair.Key.SkillDamageIndex != null)
                                        {
                                            _skillActiveController.AddAfterData(pair.Key, AfterSkill, skillInfo);
                                        }
                                    }

                                    _allDamageAfterSkill.AddLast(AfterSkill);

                                    break;
                                }
                            //case 14:
                            //    {
                            //        _skillActiveController.AddAfterData(_skillActiveController.DefaultSkill.SkillBaseData, AfterSkill, skillInfo);

                            //        break;
                            //    }

                            case 15:
                                {
                                    V2Enum_ARR_SynergyType synergy = gambleSkillData.MainSkillTypeParam3.GetDecrypted().IntToEnum32<V2Enum_ARR_SynergyType>();

                                    foreach (var pair in _skillActiveController.SkillLibrary)
                                    {
                                        if (pair.Value.SynergyType == synergy)
                                        {
                                            _skillActiveController.AddAfterData(pair.Key, AfterSkill, skillInfo);
                                        }
                                    }

                                    if (_synergyAfterSkill.ContainsKey(synergy) == false)
                                        _synergyAfterSkill.Add(synergy, new LinkedList<SkillBaseData>());

                                    _synergyAfterSkill[synergy].AddLast(AfterSkill);

                                    break;
                                }
                        }
                        break;
                    }
                case V2Enum_ARR_MainSkillType.SkillEnforgeGroupSkill:
                    {
                        SkillModuleData skillModuleData = Managers.SkillManager.Instance.GetSkillModuleData(gambleSkillData.MainSkillTypeParam1);
                        if (skillModuleData == null)
                            return;

                        switch (gambleSkillData.MainSkillTypeParam2)
                        {
                            case 15:
                                {
                                    V2Enum_ARR_SynergyType synergy = gambleSkillData.MainSkillTypeParam3.GetDecrypted().IntToEnum32<V2Enum_ARR_SynergyType>();

                                    foreach (var pair in _skillActiveController.SkillLibrary)
                                    {
                                        if (pair.Value.SynergyType == synergy)
                                        {
                                            _skillActiveController.AddModuleData(pair.Key, skillModuleData);
                                        }
                                    }

                                    if (_synergySkillModule.ContainsKey(synergy) == false)
                                        _synergySkillModule.Add(synergy, new LinkedList<SkillModuleData>());

                                    _synergySkillModule[synergy].AddLast(skillModuleData);

                                    break;
                                }
                        }

                        break;
                    }
            }

            _skillActiveController.AddGambleSkill(gambleSkillData);
        }
        //------------------------------------------------------------------------------------
    }
}

