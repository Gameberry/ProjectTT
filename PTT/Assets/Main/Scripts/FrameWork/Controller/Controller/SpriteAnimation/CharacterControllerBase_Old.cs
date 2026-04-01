// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace GameBerry.Old
// {
//     public enum CharacterState : byte
//     {
//         None = 0,
//         Idle,
//         Run,
//         Attack,
//         Hit,
//         Dead,

//         Skill,

//         Max,
//     }

//     public enum ActorType
//     {
//         None = 0,
//         Knight, // 기사
//         Ally, // 동료
//         Monster, // 몬스터
//         Stage, // 스테이지
//         Party, // 파티
//         EquipmentSet, // 장비세트
//         PhantomKnight, // 붉은기사
//         PvPFriend, // 우리팀 PvP객체
//         PvPFoe, // 상대 PvP객체
//     }

//     public class CharacterControllerBase_Old : MonoBehaviour
//     {
//         public StageGenerateDirections LookDirection { get { return m_lookDirection; } }
//         [SerializeField]
//         protected StageGenerateDirections m_lookDirection = StageGenerateDirections.Right;

//         [SerializeField]
//         protected ActorType m_myActorType = ActorType.None;

//         public ActorType MyActorType { get { return m_myActorType; } }

//         [SerializeField]
//         protected V2Enum_ElementType m_myelement = V2Enum_ElementType.None;

//         public CharacterState m_characterState = CharacterState.None;
//         public CharacterState CharacterState { get { return m_characterState; } }

//         private Dictionary<ParticlePoolElement, ParticlePoolElement> m_aliveParticle = new Dictionary<ParticlePoolElement, ParticlePoolElement>();

//         public bool IsDead { get { return m_characterState == CharacterState.Dead; } }

//         [SerializeField]
//         public CharacterAniController m_charAnicontroller;

//         [SerializeField]
//         protected SpriteRenderer m_bodyRenderer;

//         [SerializeField]
//         protected UICharacterState m_uiCharacterState;

//         [SerializeField]
//         protected Transform m_varianceTransform;

//         [SerializeField]
//         protected SkillHitReceiver m_skillHitReceiver;

//         public SkillHitReceiver MySkillHitReceiver { get { return m_skillHitReceiver; } }

//         [SerializeField]
//         protected CCStater m_cCStater;

//         public CCStater MyCCStater { get { return m_cCStater; } }

//         [SerializeField]
//         protected Rigidbody2D m_rigidbody2D;
//         public Rigidbody2D MyRigidbody2D { get { return m_rigidbody2D; } }

//         [SerializeField]
//         protected Transform m_characterSpriteRoot;
//         public Transform CharacterSpriteRoot { get { return m_characterSpriteRoot; } }


//         [SerializeField]
//         protected string m_groupIndex;
//         public string GroupIndex { get { return m_groupIndex; } }

//         [SerializeField]
//         protected int m_variationNumber;
//         public int VariationNumber { get { return m_variationNumber; } }

//         protected CharacterControllerBase m_attackTarget;
//         public CharacterControllerBase AttackTarget { get { return m_attackTarget; } }

//         [SerializeField]
//         protected double m_maxHP = 0.0;
//         public double MaxHP { get { return m_maxHP; } }

//         [SerializeField]
//         protected double m_currentHP = 0.0;
//         public double CurrentHP { get { return m_currentHP; } }

//         protected double m_myDamage = 0.0;
//         public double MyDamage { get { return m_myDamage; } }


//         protected int m_addSortingRenderer = 0;
//         public int AddSortingRenderer { get { return m_addSortingRenderer; } }

//         [SerializeField]
//         protected float m_aniControllerSpeed = 1.0f;
//         public float AniControllerSpeed { get { return m_aniControllerSpeed; } }

//         protected bool m_isTurnAttack = false;
//         public bool IsTurnAttack { get { return m_isTurnAttack; } }

//         // Hit
//         protected float m_hitRecoveryStartTime = 0.0f;
//         protected float m_hitRecoveryTime = 0.2f;

//         protected bool UseHpAsHitCount = false;

//         //------------------------------------------------------------------------------------
//         public virtual void Init()
//         { 

//         }
//         //------------------------------------------------------------------------------------
//         public virtual double OnDamage(V2SkillAttackData damage)
//         {
//             if (m_characterState == CharacterState.Dead)
//                 return 0.0;

//             if (m_currentHP <= 0)
//                 return 0.0;

//             if (m_cCStater != null)
//             {
//                 if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Invincible) == true)
//                 {
//                     Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.Block, 0.0, m_varianceTransform);
//                     m_cCStater.PlayApplyCC(damage.v2CCDatas);
//                     return 0.0;
//                 }
//             }

//             CreatureStatController attacker = damage.attacker;

//             double resultdamage = 0.0;

//             if (damage.v2DamageDatas != null)
//             {
                
//                 for (int i = 0; i < damage.v2DamageDatas.Count; ++i)
//                 {
//                     if (UseHpAsHitCount == true)
//                     {
//                         resultdamage = 1.0;

//                         if (MyActorType == ActorType.Monster)
//                         {
//                             if (Managers.GameSettingManager.Instance.Cheat_OnPunch() == true)
//                                 resultdamage = m_maxHP * 2.0;
//                         }

//                         if (resultdamage > 0.0)
//                         {
//                             if (damage.actorType == ActorType.Monster)
//                                 Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.Monster, resultdamage, m_varianceTransform, V2Enum_ElementType.None);
//                             else
//                                 Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.NomalDamage, resultdamage, m_varianceTransform, V2Enum_ElementType.None);

//                             DeCreaseHP(resultdamage);

//                             PlayOnDamageDirection();
//                         }
//                     }
//                     else
//                     {
//                         V2DamageData v2DamageData = damage.v2DamageDatas[i];



//                         bool IsElementAttack = v2DamageData.DamageElement != V2Enum_ElementType.None;

//                         // 최종공격력
//                         double finalAttackOper = v2DamageData.AttackValue * v2DamageData.FinalValue;

//                         // 최종속성공격력 추가
//                         if (IsElementAttack == true)
//                             finalAttackOper = finalAttackOper + v2DamageData.ElementValue;

//                         // 스킬계수
//                         double skillValueOper = v2DamageData.SkillValue;
//                         double splashValue = 1.0;

//                         if (damage.v2DamageDatas[i].characterSkillDamageData != null)
//                         {
//                             if (damage.v2DamageDatas[i].characterSkillDamageData.DamageSplashFactor != 1.0f)
//                             {
//                                 for (int j = 0; j < damage.v2DamageDatas[i].characterSkillDamageData.HitTargetCount; ++j)
//                                 {
//                                     splashValue *= damage.v2DamageDatas[i].characterSkillDamageData.DamageSplashFactor;
//                                 }

//                                 damage.v2DamageDatas[i].characterSkillDamageData.HitTargetCount++;
//                             }
//                         }

//                         skillValueOper = v2DamageData.SkillValue * splashValue;

//                         // 관통계수
//                         double PenetrationOper = 1.0;

//                         double finalDefence = GetOutPutDefence();
//                         PenetrationOper = 1.0 + ((v2DamageData.PenetrationValue - finalDefence) / finalDefence);
//                         if (PenetrationOper > 1.0)
//                             PenetrationOper = 1.0;
//                         else if (PenetrationOper < Define.PenetrationFactorMin)
//                             PenetrationOper = Define.PenetrationFactorMin;

//                         // 명중계수
//                         double AccuracyOper = 1.0;
//                         double myEvasion = GetOutPutMyStat(V2Enum_Stat.Evasion);
//                         if (v2DamageData.AccuracyValue < myEvasion)
//                         {
//                             double randomAccuracy = v2DamageData.AccuracyValue / myEvasion;

//                             if (Random.Range(0.0f, 1.0f) > randomAccuracy)
//                             {
//                                 AccuracyOper = randomAccuracy * randomAccuracy;
//                             }
//                         }

//                         // 속성계수
//                         double elementValueOper = 1.0;

//                         if (IsElementAttack == true)
//                         {
//                             if (m_myelement != V2Enum_ElementType.None)
//                             {
//                                 //elementValueOper = InGameUtil.IsCounterElement(m_myelement, v2DamageData.DamageElement) == true ? Define.ElementDamageFactorBenefit : Define.ElementDamageFactorBase;
//                                 elementValueOper = InGameUtil.ElementCountDamageRatio(m_myelement, v2DamageData.DamageElement);
//                             }
//                         }
//                         else
//                             elementValueOper = m_myelement == V2Enum_ElementType.None ? Define.ElementDamageFactorBase : Define.NormalDamageElementFactorPenalty;

//                         // 저항계수
//                         double ResistanceOper = 1.0;

//                         double myResistanceValue = GetOutPutMyStat(V2Enum_Stat.Resistance);
//                         if (m_myActorType == ActorType.PvPFriend
//                             || m_myActorType == ActorType.PvPFoe)
//                             myResistanceValue = myResistanceValue * Define.AllyArenaStatModForResistance;

//                         //ResistanceOper = 2.0 + ((finalAttackOper - myResistanceValue) / myResistanceValue);
//                         ResistanceOper = ((finalAttackOper - myResistanceValue) / finalAttackOper);
//                         if (ResistanceOper >= 1.0)
//                             ResistanceOper = 1.0;
//                         else if (ResistanceOper < Define.ResistanceFactorMin)
//                             ResistanceOper = Define.ResistanceFactorMin;


//                         double finalDamage = 0.0;
//                         if (IsElementAttack == true)
//                             finalDamage = finalAttackOper * skillValueOper * elementValueOper * ResistanceOper;
//                         else
//                             finalDamage = finalAttackOper * skillValueOper * PenetrationOper * AccuracyOper * elementValueOper;

//                         double criticalOperDamage = finalDamage;

//                         V2Enum_Stat criticalType = V2Enum_Stat.Attack;


//                         if (attacker != null)
//                         {
//                             criticalType = attacker.GetCriticalKind();

//                             switch (criticalType)
//                             {
//                                 case V2Enum_Stat.CriticalChance:
//                                     {
//                                         criticalOperDamage = finalDamage * (1.0 + (attacker.GetCriticalDamageValue(criticalType) * Define.PerStatRecoverValue));
//                                         break;
//                                     }
//                                 case V2Enum_Stat.SuperCriticalChance:
//                                     {
//                                         criticalOperDamage = finalDamage * (1.0 + (attacker.GetCriticalDamageValue(criticalType) * Define.PerStatRecoverValue)) * Define.CriticalFactorSuper;
//                                         break;
//                                     }
//                                 case V2Enum_Stat.HyperCriticalChance:
//                                     {
//                                         criticalOperDamage = finalDamage * (1.0 + (attacker.GetCriticalDamageValue(criticalType) * Define.PerStatRecoverValue)) * Define.CriticalFactorHyper;
//                                         break;
//                                     }
//                             }
//                         }

//                         resultdamage = criticalOperDamage;

//                         if (damage.actorType == ActorType.Monster)
//                             resultdamage *= 1.0 - (Managers.CharacterStatManager.Instance.GetOutPutStatValue(V2Enum_Stat.MonsterAttackDecrease) * Define.PerStatRecoverValue);

//                         resultdamage = System.Math.Round(resultdamage);

//                         if (Managers.GameSettingManager.Instance.Cheat_DamageLog() == true)
//                         {
//                             string DamageLog = string.Empty;

//                             DamageLog = string.Format("[{0}({1})] -> [{2}({3})]  Damage : {4} \n\n", damage.actorType, v2DamageData.DamageElement, m_myActorType, m_myelement, resultdamage);

//                             if (IsElementAttack == true)
//                             {
//                                 DamageLog += (string.Format("    FinalDamageValue : {0} ->  FinalAttack:{1} * SkillValue:{2} * ElementValue:{3} * ResistanceValue:{4}\n\n", finalDamage, finalAttackOper, skillValueOper, elementValueOper, ResistanceOper));
//                                 DamageLog += (string.Format("      FinalAttack : {0} ->  (AttackValue:{1} + ElementValue:{2})\n\n", finalAttackOper, v2DamageData.AttackValue * v2DamageData.FinalValue, v2DamageData.ElementValue));
//                                 DamageLog += (string.Format("      SkillValue : {0} ->  SkillValue:{1} * SplashValue:{2}\n\n", skillValueOper, v2DamageData.SkillValue, splashValue));
//                                 DamageLog += (string.Format("      ElementValue : {0}\n\n", elementValueOper));
//                                 DamageLog += (string.Format("      ResistanceValue : {0} ->  FinalAttack:{1} , Resistance:{2}\n\n", ResistanceOper, finalAttackOper, myResistanceValue));
//                             }
//                             else
//                             {
//                                 DamageLog += (string.Format("    FinalDamageValue : {0} ->  FinalAttack:{1} * SkillValue:{2} * PenetrationValue:{3} * AccuracyValue:{4} * ElementValue:{5}\n\n", finalDamage, v2DamageData.AttackValue * v2DamageData.FinalValue, skillValueOper, PenetrationOper, AccuracyOper, elementValueOper));
//                                 DamageLog += (string.Format("      FinalAttack : {0} ->  AttackValue:{1}\n\n", finalAttackOper, v2DamageData.AttackValue * v2DamageData.FinalValue));
//                                 DamageLog += (string.Format("      SkillValue : {0} ->  SkillValue:{1} * SplashValue:{2}\n\n", skillValueOper, v2DamageData.SkillValue, splashValue));
//                                 DamageLog += (string.Format("      PenetrationValue : {0} ->  PenetrationValue:{1} , FinalDefence:{2}\n\n", PenetrationOper, v2DamageData.PenetrationValue, finalDefence));
//                                 DamageLog += (string.Format("      AccuracyValue : {0} ->  AccuracyValue:{1} , Evasion:{2}\n\n", AccuracyOper, v2DamageData.AccuracyValue, myEvasion));
//                                 DamageLog += (string.Format("      ElementValue : {0}\n\n", elementValueOper));
//                             }


//                             if (criticalType != V2Enum_Stat.Attack)
//                             {
//                                 switch (criticalType)
//                                 {
//                                     case V2Enum_Stat.CriticalChance:
//                                         {
//                                             DamageLog += (string.Format("      Damage : {0} ->  CriticalType:{1} , CriticalValue:{2}\n\n", criticalOperDamage, criticalType, (1.0 + (attacker.GetCriticalDamageValue(criticalType) * Define.PerStatRecoverValue))));
//                                             break;
//                                         }
//                                     case V2Enum_Stat.SuperCriticalChance:
//                                         {
//                                             DamageLog += (string.Format("      Damage : {0} ->  CriticalType:{1} , CriticalValue:{2} * CriticalFactorSuper:{3}\n\n", criticalOperDamage, criticalType, (1.0 + (attacker.GetCriticalDamageValue(criticalType) * Define.PerStatRecoverValue)), Define.CriticalFactorSuper));
//                                             break;
//                                         }
//                                     case V2Enum_Stat.HyperCriticalChance:
//                                         {
//                                             DamageLog += (string.Format("      Damage : {0} ->  CriticalType:{1} , CriticalValue:{2} * CriticalFactorHyper:{3}\n\n", criticalOperDamage, criticalType, (1.0 + (attacker.GetCriticalDamageValue(criticalType) * Define.PerStatRecoverValue)), Define.CriticalFactorHyper));
//                                             break;
//                                         }
//                                 }

//                             }
//                             else
//                                 DamageLog += (string.Format("      Damage : {0} ->  CriticalType:{1}\n\n", criticalOperDamage, criticalType));

//                             if (damage.actorType == ActorType.Monster)
//                             {
//                                 DamageLog += (string.Format("      MonsterDecreaseDamage : {0} ->  MonsterAttackDecrease:{1}\n\n", criticalOperDamage * 1.0 - (Managers.CharacterStatManager.Instance.GetOutPutStatValue(V2Enum_Stat.MonsterAttackDecrease) * Define.PerStatRecoverValue), 1.0 - (Managers.CharacterStatManager.Instance.GetOutPutStatValue(V2Enum_Stat.MonsterAttackDecrease) * Define.PerStatRecoverValue)));
//                             }

//                             DamageLog += (string.Format("    System.Math.Round(Damage) : {0}\n\n", resultdamage));

//                             Debug.LogWarning(DamageLog);
//                         }



//                         if (MyActorType == ActorType.Monster)
//                         {
//                             if (Managers.GameSettingManager.Instance.Cheat_OnPunch() == true)
//                                 resultdamage = m_maxHP * 2.0;
//                         }

//                         if (resultdamage > 0.0)
//                         {
//                             if (damage.actorType == ActorType.Monster)
//                                 Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.Monster, resultdamage, m_varianceTransform, v2DamageData.DamageElement);
//                             else
//                                 Managers.HPMPVarianceManager.Instance.ShowVarianceText((HpMpVarianceType)criticalType, resultdamage, m_varianceTransform, v2DamageData.DamageElement);

//                             DeCreaseHP(resultdamage);

//                             PlayOnDamageDirection();
//                         }
//                     }


//                 }
//             }

//             if (m_currentHP <= 0)
//                 ChangeState(CharacterState.Dead);

//             if (m_characterState == CharacterState.Dead)
//                 return resultdamage;

//             if (MyActorType == ActorType.Knight
//                 && Managers.BerserkerManager.isAlive == true
//                 && Managers.BerserkerManager.Instance.PlayingBerserkerMode() == true)
//             {
//                 // 흠.... 나중에 뭔가 추가될 수 있으니
//             }
//             else
//             {
//                 if (damage.v2CCDatas != null)
//                 {
//                     for (int i = 0; i < damage.v2CCDatas.Count; ++i)
//                     {
//                         if (m_cCStater != null)
//                         {
//                             m_cCStater.PlayApplyCC(damage.v2CCDatas[i]);
//                         }

//                         PlayOnDamageAfterCC(damage.v2CCDatas[i]);
//                     }
//                 }
//             }

//             return resultdamage;
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void PlayOnDamageDirection()
//         { 

//         }
//         //------------------------------------------------------------------------------------
//         public virtual Vector2 GetProjectileAddPos()
//         {
//             if (m_bodyRenderer == null)
//                 return Vector2.zero;

//             Vector2 addpos = m_bodyRenderer.bounds.size * 0.1f;

//             addpos.x = Random.Range(addpos.x * -0.1f, addpos.x);
//             addpos.y = Random.Range(addpos.y * -0.1f, addpos.y) + (m_bodyRenderer.bounds.size.y * 0.3f);

//             return addpos;
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void PlayOnDamageAfterCC(V2CCData v2CCData)
//         {
//             if (v2CCData.CCTypeEnum == V2Enum_CrowdControlType.Stun
//                 || v2CCData.CCTypeEnum == V2Enum_CrowdControlType.Knockback
//                 || v2CCData.CCTypeEnum == V2Enum_CrowdControlType.Fling)
//             {
//                 ChangeState(CharacterState.Hit);
//                 SetHitRecoveryTime(v2CCData.CCTime);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected void SetHitRecoveryTime(float hitTime)
//         {
//             if ((m_hitRecoveryStartTime + m_hitRecoveryTime) - Time.time < hitTime)
//             {
//                 m_hitRecoveryStartTime = Time.time;
//                 m_hitRecoveryTime = hitTime;
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void InCreaseHP(double hp)
//         {
//             SetHP(m_currentHP + hp);
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void DeCreaseHP(double hp)
//         {
//             if (UseHpAsHitCount == true)
//                 SetHP(m_currentHP - 1.0);
//             else
//                 SetHP(m_currentHP - hp);
//         }
//         //------------------------------------------------------------------------------------
//         protected void SetHP(double hp)
//         {
//             m_currentHP = hp;

//             if (m_currentHP < 0)
//                 m_currentHP = 0;

//             if (m_currentHP > m_maxHP)
//                 m_currentHP = m_maxHP;

//             if (m_maxHP == 0)
//                 return;

//             double hpratio = m_currentHP / m_maxHP;

//             if (m_uiCharacterState != null)
//                 m_uiCharacterState.SetHPBar(hpratio);
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void ChangeState(CharacterState state)
//         { 

//         }
//         //------------------------------------------------------------------------------------
//         private void Update()
//         {
//             Updated();

//             if (m_characterState != CharacterState.Dead 
//                 && m_characterState != CharacterState.None)
//             {
//                 InCreaseHP(GetOutPutMyStat(V2Enum_Stat.HpRecovery) * Time.deltaTime);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void Updated()
//         { 

//         }
//         //------------------------------------------------------------------------------------
//         public void AddPlayParticle(ParticlePoolElement particlePoolElement)
//         {
//             if (particlePoolElement != null)
//             {
//                 if (m_aliveParticle.ContainsKey(particlePoolElement) == false)
//                 {
//                     particlePoolElement.SetSimulationSpeed(m_aniControllerSpeed);

//                     m_aliveParticle.Add(particlePoolElement, particlePoolElement);
//                     particlePoolElement.ParticleStop += EndParticle;
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public void EndParticle(ParticlePoolElement particlePoolElement)
//         {
//             if (particlePoolElement == null)
//                 return;

//             if (m_aliveParticle.ContainsKey(particlePoolElement) == true)
//             { 
//                 m_aliveParticle.Remove(particlePoolElement);
//                 particlePoolElement.ParticleStop -= EndParticle;
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected void ChangeWeaponParticle(CharacterGearData equipData)
//         {
//             if (m_charAnicontroller == null)
//                 return;

//             if (m_charAnicontroller.m_weaponEffect != null)
//             {
//                 Destroy(m_charAnicontroller.m_weaponEffect.gameObject);
//                 m_charAnicontroller.m_weaponEffect = null;
//             }

//             GameObject obj = Managers.CharacterGearManager.Instance.GetWeaponParticle(equipData);
//             if (obj != null)
//             {
//                 if (m_charAnicontroller.CharAniPart_Dic.ContainsKey(AnimationPart.Weapon) == true)
//                 {
//                     SpriteAniPart spriteAniPart = m_charAnicontroller.CharAniPart_Dic[AnimationPart.Weapon];
//                     GameObject clone = Instantiate(obj, spriteAniPart.transform);
//                     clone.transform.ResetLocal();
//                     WeaponParticleSortingOrder weaponParticleSortingOrder = clone.AddComponent<WeaponParticleSortingOrder>();
//                     weaponParticleSortingOrder.Init(spriteAniPart.Renderer);
//                     m_charAnicontroller.m_weaponEffect = weaponParticleSortingOrder;
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public void ReleaseParticle()
//         {
//             foreach (KeyValuePair<ParticlePoolElement, ParticlePoolElement> pair in m_aliveParticle)
//             {
//                 pair.Value.ParticleStop -= EndParticle;
//                 pair.Value.StopParticle();
//             }

//             m_aliveParticle.Clear();
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void ReleaseSkill()
//         {

//         }
//         //------------------------------------------------------------------------------------
//         public virtual void SendDamage(CharacterSkillData characterSkillData)
//         {

//         }
//         //------------------------------------------------------------------------------------
//         public virtual void HPRecoverPer(double ratio)
//         {
//             if (m_characterState == CharacterState.Dead || m_characterState == CharacterState.None)
//                 return;

//             double increaseValue = m_maxHP * ratio * Define.PercentageRecoverValue;

//             increaseValue = System.Math.Round(increaseValue);

//             Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.RecoveryHP, increaseValue, m_varianceTransform.position);

//             InCreaseHP(increaseValue);
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void HPRecoverPer(V2CCData v2CCData)
//         {
//             if (m_characterState == CharacterState.Dead || m_characterState == CharacterState.None)
//                 return;

//             if (v2CCData.CCTime <= 0)
//             {
//                 HPRecoverPer(v2CCData.CCValue);
//             }
//             else
//             {
//                 if (m_cCStater != null)
//                 {
//                     m_cCStater.PlayApplyCC(v2CCData);
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void HPDeCreasePer_CurrentHP(double ratio)
//         {
//             if (m_characterState == CharacterState.Dead)
//                 return;

//             double decreaseValue = m_currentHP * ratio * Define.PercentageRecoverValue;

//             decreaseValue = System.Math.Round(decreaseValue);

//             Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.NomalDamage, decreaseValue, m_varianceTransform.position);

//             DeCreaseHP(decreaseValue);

//             if (m_currentHP <= 0)
//             {
//                 ChangeState(CharacterState.Dead);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void CCDOT(double value)
//         {
//             if (m_characterState == CharacterState.Dead)
//                 return;

//             double decreaseValue = m_maxHP * value * Define.PercentageRecoverValue;

//             decreaseValue = System.Math.Round(decreaseValue);

//             Managers.HPMPVarianceManager.Instance.ShowVarianceText(HpMpVarianceType.NomalDamage, decreaseValue, m_varianceTransform.position);

//             DeCreaseHP(decreaseValue);

//             if (m_currentHP <= 0)
//             {
//                 ChangeState(CharacterState.Dead);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void CCHOT(double value)
//         {
//             HPRecoverPer(value);
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void EndCC(V2Enum_CrowdControlType cCType)
//         {

//         }
//         //------------------------------------------------------------------------------------
//         public virtual V2SkillAttackData GetCharacterDamageData(CharacterSkillData skillData)
//         {
//             V2SkillAttackData v2SkillAttackData;
//             v2SkillAttackData.v2DamageDatas = null;
//             v2SkillAttackData.v2CCDatas = null;
//             v2SkillAttackData.attacker = null;
//             v2SkillAttackData.actorType = ActorType.None;

//             return v2SkillAttackData;
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void AniCallBackRecv(string anicallback)
//         { 

//         }
//         //------------------------------------------------------------------------------------
//         public void ChangeCharacterLookAtDirection(StageGenerateDirections direction)
//         {
//             m_lookDirection = direction;
//             Vector3 rotate = transform.eulerAngles;

//             float selectRatote = 0.0f;

//             if (m_lookDirection == StageGenerateDirections.Left)
//                 selectRatote = 180.0f;

//             rotate.y = selectRatote;

//             transform.eulerAngles = rotate;

//             if (m_uiCharacterState != null)
//             {
//                 rotate = m_uiCharacterState.transform.localEulerAngles;
//                 rotate.y = selectRatote;

//                 m_uiCharacterState.transform.localEulerAngles = rotate;
//             }

//             if (m_cCStater != null)
//             {
//                 m_cCStater.SetPainterRotation(selectRatote);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public void ChangeCharacterLookAtDirection_Target(Transform targetTrans)
//         {
//             Vector2 direction = targetTrans.transform.position - transform.position;
//             direction.Normalize();

//             ChangeCharacterLookAtDirection(direction.x < 0 ? StageGenerateDirections.Left : StageGenerateDirections.Right);
//         }
//         //------------------------------------------------------------------------------------
//         public virtual double GetOutPutMyStat(V2Enum_Stat v2Enum_Stat)
//         {
//             return 0.0;
//         }
//         //------------------------------------------------------------------------------------
//         protected double GetOutPutDefence()
//         {
//             double orivalue = GetOutPutMyStat(V2Enum_Stat.Defence);
//             double pervalue = GetOutPutMyStat(V2Enum_Stat.DefenceIncrease) * Define.PerStatRecoverValue;

//             double returnvalue = orivalue * (1.0 + pervalue);

//             return returnvalue;
//         }
//         //------------------------------------------------------------------------------------
//         public virtual float GetOutputAttackRange()
//         {
//             return 1.0f;
//         }
//         //------------------------------------------------------------------------------------
//         public void SetAddSortingRenderer(int layer)
//         {
//             m_addSortingRenderer = layer;
//         }
//         //------------------------------------------------------------------------------------
//     }
// }

