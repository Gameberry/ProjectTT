// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace GameBerry.Old
// {
//     public class MonsterController_Old : CreatureControllerBase
//     {
//         [SerializeField]
//         protected SpriteRenderer m_shadowSprite;

//         [SerializeField]
//         protected List<ParticleSystem> m_berserkerModeDamageParticle;

//         protected MonsterData m_myMonsterData = null;

//         protected string m_spawnID = string.Empty;

//         protected CreatureStatController m_creatureStatController;

//         public bool m_isBossMonster = false;

//         public int m_localID = -1;

//         protected ParticlePoolElement m_bossParticle = null;

//         private Transform m_limitLine = null;
//         private Transform m_limitLine_L = null;

//         protected SkillBaseData m_nextPlaySkillData;
//         protected float m_playSkillTime;

//         //------------------------------------------------------------------------------------
//         public override void Init()
//         {
//             if (m_charAnicontroller != null)
//             {
//                 m_charAnicontroller.Init(transform);
//                 m_charAnicontroller.ConnectAniActionState(AniActionCallBack);
//                 m_originMaterial = m_charAnicontroller.GetMaterial();
//             }

//             if (m_uiCharacterState != null)
//                 m_uiCharacterState.Init();

//             if (m_bodyRenderer != null)
//                 m_originColor = m_bodyRenderer.color;

//             m_limitLine = InGamePositionContainer.Instance.GetMonsterLimitLine();
//             m_limitLine_L = InGamePositionContainer.Instance.GetMonsterLimitLine_L();

//             m_creatureStatController = new CreatureStatController();
//             m_creatureStatController.Init(ActorType.Monster);

//             //ReleaseHitDirection();
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void SetPlayerController(PlayerController playerController)
//         {
//             //m_attackTarget = playerController;
//         }
//         //------------------------------------------------------------------------------------
//         public void SetMonster<T>(MonsterData data, List<T> overrideStat, string spawnid, int addsortinglayer, bool isboss, int localid) where T : CreatureBaseStatElement
//         {
//             CreatureLevel = 1;
//             CreatureStar = 0;

//             m_myMonsterData = data;

//             m_myelement = data.ElementType;

//             m_groupIndex = data.GroupIndex;
//             m_variationNumber = data.VariationNumber;

//             m_mySearchType = data.TargetSearchType;

//             m_spawnID = spawnid;

//             for (int i = 0; i < m_myMonsterData.StatValue.Count; ++i)
//             {
//                 m_creatureStatController.SetDefaultStatValue(m_myMonsterData.StatValue[i].BaseStat, m_myMonsterData.StatValue[i].BaseValue);
//             }

//             UseHpAsHitCount = data.isHpAsHitCount;

//             if (data.isHpAsHitCount == false)
//             {
//                 for (int i = 0; i < overrideStat.Count; ++i)
//                 {
//                     m_creatureStatController.SetDefaultStatValue(overrideStat[i].BaseStat, overrideStat[i].BaseValue);
//                 }
//             }
            
//             m_creatureAttackSpeed = m_creatureStatController.GetOutPutAttackSpeed();
//             m_creatureMoveSpeed = m_creatureStatController.GetOutputMoveSpeed();

//             m_maxHP = m_creatureStatController.GetOutPutHP();

//             m_maxHP -= m_maxHP * Managers.CharacterStatManager.Instance.GetOutPutStatValue(V2Enum_Stat.MonsterHpDecrease) * Define.PerStatRecoverValue;

//             m_currentHP = m_maxHP;


//             m_myDamage = m_creatureStatController.GetOutPutATK();

//             m_myDamage -= m_myDamage * Managers.CharacterStatManager.Instance.GetOutPutStatValue(V2Enum_Stat.MonsterAttackDecrease) * Define.PerStatRecoverValue;

//             SetHP(m_maxHP);

//             m_addSortingRenderer = addsortinglayer;

//             m_isBossMonster = isboss;

//             if (m_uiCharacterState != null)
//                 m_uiCharacterState.gameObject.SetActive(false);

//             SetElement(V2Enum_ElementType.None);

//             m_localID = localid;

//             m_charAnicontroller.SetAnimationSpriteLibrary();

//             CreatureSkillRelease();

//             m_attackData = m_myMonsterData.MonsterAttackData;
//             SetCreatureSkill(m_attackData, ref m_attackActionScript);

//             m_skillData = m_myMonsterData.MonsterSkillData;
//             SetCreatureSkill(m_skillData, ref m_skillActionScript);

//             if (m_isBossMonster == true)
//             {
//                 Vector3 bosssize = Managers.DungeonManager.Instance.BossScale.ToVector3();

//                 SetCreatureSizeControll(bosssize * data.BaseScale);
//             }
//             else
//             {
//                 SetCreatureSizeControll(Vector3.one * data.BaseScale);
//             }

//             if (m_uiCharacterState != null)
//                 m_uiCharacterState.gameObject.SetActive(false);
//         }
//         //------------------------------------------------------------------------------------
//         public void SetMonster(MonsterData data, double level, string spawnid, int addsortinglayer, bool isboss, int localid)
//         {
//             CreatureLevel = level;
//             CreatureStar = 0;

//             m_myMonsterData = data;

//             m_groupIndex = data.GroupIndex;
//             m_variationNumber = data.VariationNumber;

//             m_mySearchType = data.TargetSearchType;

//             m_spawnID = spawnid;

//             for (int i = 0; i < m_myMonsterData.StatValue.Count; ++i)
//             {
//                 m_myMonsterData.StatValue[i].BaseValue = m_myMonsterData.StatValue[i].OverrideStatBaseValue + (m_myMonsterData.StatValue[i].OverrideStatAddValue * level);
//                 m_creatureStatController.SetDefaultStatValue(m_myMonsterData.StatValue[i].BaseStat, m_myMonsterData.StatValue[i].BaseValue);
//             }

//             UseHpAsHitCount = data.isHpAsHitCount;

//             m_creatureAttackSpeed = m_creatureStatController.GetOutPutAttackSpeed();
//             m_creatureMoveSpeed = m_creatureStatController.GetOutputMoveSpeed();

//             m_maxHP = m_creatureStatController.GetOutPutHP();

//             m_maxHP -= m_maxHP * Managers.CharacterStatManager.Instance.GetOutPutStatValue(V2Enum_Stat.MonsterHpDecrease) * Define.PerStatRecoverValue;

//             m_currentHP = m_maxHP;


//             m_myDamage = m_creatureStatController.GetOutPutATK();

//             m_myDamage -= m_myDamage * Managers.CharacterStatManager.Instance.GetOutPutStatValue(V2Enum_Stat.MonsterAttackDecrease) * Define.PerStatRecoverValue;

//             SetHP(m_maxHP);

//             m_addSortingRenderer = addsortinglayer;

//             m_isBossMonster = isboss;

//             if (m_uiCharacterState != null)
//                 m_uiCharacterState.gameObject.SetActive(false);

//             SetElement(V2Enum_ElementType.None);

//             m_localID = localid;

//             m_charAnicontroller.SetAnimationSpriteLibrary();

//             CreatureSkillRelease();

//             m_attackData = m_myMonsterData.MonsterAttackData;
//             SetCreatureSkill(m_attackData, ref m_attackActionScript);

//             m_skillData = m_myMonsterData.MonsterSkillData;
//             SetCreatureSkill(m_skillData, ref m_skillActionScript);

//             if (m_isBossMonster == true)
//             {
//                 Vector3 bosssize = Managers.DungeonManager.Instance.BossScale.ToVector3();

//                 SetCreatureSizeControll(bosssize * data.BaseScale);
//             }
//             else
//             {
//                 SetCreatureSizeControll(Vector3.one * data.BaseScale);
//             }

//             if (m_uiCharacterState != null)
//                 m_uiCharacterState.gameObject.SetActive(false);
//         }
//         //------------------------------------------------------------------------------------
//         public void SetElement(V2Enum_ElementType elementOverrideType)
//         {
//             m_myelement = elementOverrideType;

//             if (m_myelement != V2Enum_ElementType.None)
//             {
//                 if (m_shadowSprite != null)
//                     m_shadowSprite.enabled = false;

//                 if (m_bossParticle != null)
//                 {
//                     m_bossParticle.StopParticle();
//                 }

//                 m_bossParticle = null;

//                 m_bossParticle = ParticleManager.Instance.GetBossElementParticle(m_myelement);
//                 if (m_bossParticle != null)
//                 {
//                     m_bossParticle.transform.SetParent(m_shadowSprite.transform);
//                     m_bossParticle.transform.ResetLocal();
//                     m_bossParticle.gameObject.SetActive(true);
//                     m_bossParticle.PlayParticle();
//                 }
//             }
//             else
//             {
//                 if (m_bossParticle != null)
//                 {
//                     m_bossParticle.StopParticle();
//                 }

//                 m_bossParticle = null;

//                 if (m_shadowSprite != null)
//                     m_shadowSprite.enabled = true;
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void PlayMonster()
//         { // 몬스터가 알아서 초기의 상태로 ㄱㄱ싱
//             Color color = m_bodyRenderer.color;
//             color.a = 1.0f;
//             m_bodyRenderer.color = color;

//             if (m_bodyRenderer.material != m_originMaterial)
//                 m_bodyRenderer.material = m_originMaterial;

//             m_bodyRenderer.transform.localEulerAngles = Vector3.zero;

//             ChangeState(CharacterState.Idle);

//             //if (m_bodyRenderer != null)
//             //{
//             //    Vector3 pos = Vector3.zero;
//             //    pos.y += m_bodyRenderer.bounds.size.y * 0.5f;
//             //    m_characterSpriteRoot.localPosition = pos;

//             //    Vector2 size = m_bodyRenderer.bounds.size * 0.5f;
//             //    size.x *= 0.5f;
//             //    boxCollider.size = size;
//             //    Vector2 offset = Vector2.zero;
//             //    offset.y = m_bodyRenderer.bounds.size.y * 0.5f * 0.5f;
//             //    boxCollider.offset = offset;
//             //}

//             SetFootPos();

//             if (m_skillHitReceiver != null)
//                 m_skillHitReceiver.EnableColliders(true);

//             if (m_skillData != null)
//                 m_playSkillTime = Time.time + m_skillData.CoolTime;
//             else
//                 m_playSkillTime = -1;

//             m_attackTarget = Managers.AggroManager.Instance.GetTargetCharacter(m_mySearchType, this);
//         }
//         //------------------------------------------------------------------------------------
//         protected override void Updated()
//         {
//             if (m_characterState != CharacterState.Dead)
//             {
//                 if (m_characterState == CharacterState.Idle || m_characterState == CharacterState.Run)
//                 {

//                     if (m_cCStater != null)
//                     {
//                         if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Blind))
//                         {
//                             m_aniControllerSpeed = 1.0f;
//                             ChangeState(CharacterState.Idle);
//                             return;
//                         }
//                     }

//                     if (m_attackTarget == null || m_attackTarget.IsDead == true)
//                     {
//                         m_attackTarget = Managers.AggroManager.Instance.GetTargetCharacter(m_mySearchType, this);
//                     }

//                     if (m_attackTarget == null || m_attackTarget.IsDead == true)
//                     { 
//                         ChangeState(CharacterState.Idle);
//                         return;
//                     }

//                     if (m_skillData != null && m_playSkillTime <= Time.time)
//                     {
//                         if (m_skillActionScript == null)
//                             m_nextPlaySkillData = m_skillData;
//                         else if (m_skillActionScript != null && m_skillActionScript.IsReady() == true)
//                         {
//                             m_nextPlaySkillData = m_skillData;
//                         }
//                         else
//                             m_nextPlaySkillData = m_attackData;
//                     }
//                     else
//                         m_nextPlaySkillData = m_attackData;

//                     if (m_nextPlaySkillData != null)
//                     {
//                         float distance = MathDatas.GetDistance(transform.position.x, transform.position.y, m_attackTarget.transform.position.x, m_attackTarget.transform.position.y);

//                         if (distance > m_nextPlaySkillData.TargetCheckScale * GetOutputAttackRange())
//                             ChangeState(CharacterState.Run);
//                         else
//                         {
//                             if (m_nextPlaySkillData == m_attackData)
//                             {
//                                 ChangeState(CharacterState.Attack);
//                             }
//                             else if (m_nextPlaySkillData == m_skillData)
//                             {
//                                 ChangeState(CharacterState.Skill);
//                             }
//                         }
//                     }
//                 }
//                 else if (m_characterState == CharacterState.Attack)
//                 {
//                 }
//                 else if (m_characterState == CharacterState.Hit)
//                 {
//                     if (Time.time > m_hitRecoveryStartTime + m_hitRecoveryTime)
//                     {
//                         ChangeState(CharacterState.Idle);
//                         //ReleaseHitDirection();
//                     }
//                     //else
//                     //{
//                     //    if (m_hitRecoveryTime != 0.0f)
//                     //    {
//                     //        //float ratio = (Time.time - m_hitRecoveryStartTime) / m_hitRecoveryTime;
//                     //        //EnableHitEffect(true, Contents.InGameContent.MonsterHitColorGradient_Static.Evaluate(ratio));

//                     //        if (m_cCStater != null)
//                     //        {
//                     //            if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Fling) == false
//                     //                && m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Knockback) == false)
//                     //            {
//                     //                if (Managers.MonsterManager.Instance.MonsterKnockBackDuration > 0.0f && (Time.time - m_hitRecoveryStartTime) <= Managers.MonsterManager.Instance.MonsterKnockBackDuration)
//                     //                {
//                     //                    float ratio = (Time.time - m_hitRecoveryStartTime) / Managers.MonsterManager.Instance.MonsterKnockBackDuration;

//                     //                    float xposGab = Managers.MonsterManager.Instance.TestKnockbackForce;
//                     //                    if (m_lookDirection == StageGenerateDirections.Right)
//                     //                        xposGab *= -1.0f;

//                     //                    Vector2 pos = m_hitOriginPos;
//                     //                    pos.x += xposGab * Managers.MonsterManager.Instance.MonsterKnockBackCurve.Evaluate(ratio);
//                     //                    transform.position = pos;
//                     //                }
//                     //            }
//                     //        }
//                     //    }
                        
//                     //}
//                 }
//             }
//             else
//             {
//                 if (Time.time > m_creatureDeadTime + m_releaseWaitTime)
//                 {
//                     ReleaseMonster();
//                 }
//                 else
//                 {
//                     if (Time.time < m_creatureDeadTime + m_deadDirectionTime)
//                     {
//                         float ratio = (Time.time - m_creatureDeadTime) / m_deadDirectionTime;
//                         EnableHitEffect(true, Contents.InGameContent.MonsterDeadColorGradient_Static.Evaluate(ratio));

//                     }
//                     else
//                     {
//                         EnableHitEffect(true, Contents.InGameContent.MonsterDeadColorGradient_Static.Evaluate(1.0f));
//                     }
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         private void LateUpdate()
//         {
//             LateUpdated();
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void LateUpdated()
//         {
//             if (transform.position.x > m_limitLine.position.x)
//             {
//                 Vector3 limitpos = transform.position;
//                 limitpos.x = m_limitLine.position.x;
//                 transform.position = limitpos;
//             }
//             else if (transform.position.x < m_limitLine_L.position.x)
//             {
//                 Vector3 limitpos = transform.position;
//                 limitpos.x = m_limitLine_L.position.x;
//                 transform.position = limitpos;
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void AniActionCallBack(AnimationAction aniaction)
//         {
//             if (m_characterState == CharacterState.Attack || m_characterState == CharacterState.Skill)
//             {
//                 CreatureSkillActionBase m_nextPlaySkillActionScript = m_characterState == CharacterState.Skill ? m_skillActionScript : m_attackActionScript;

//                 if (aniaction == AnimationAction.AniStartAndAction || aniaction == AnimationAction.AniAction)
//                 {
//                     if (m_nextPlaySkillData.SkillEffectDatas.ContainsKey(V2Enum_EffectType.CurrentHpRecharge) == true)
//                     { // 힐은 여기서 처리한다.
//                         for (int i = 0; i < m_nextPlaySkillData.SkillEffectDatas[V2Enum_EffectType.CurrentHpRecharge].Count; ++i)
//                         {
//                             CharacterSkillCurrentHpRechargeData m_characterSkillCurrentHpRechargeData = m_nextPlaySkillData.SkillEffectDatas[V2Enum_EffectType.CurrentHpRecharge][i] as CharacterSkillCurrentHpRechargeData;
//                             double healValue = (m_characterSkillCurrentHpRechargeData.HpRechargeBase + (m_characterSkillCurrentHpRechargeData.HpRechargePerLevel));

//                             V2CCData v2CCData;
//                             v2CCData.CCTypeEnum = V2Enum_CrowdControlType.HOT;
//                             v2CCData.CCTime = m_characterSkillCurrentHpRechargeData.Duration;
//                             v2CCData.CCValue = healValue;

//                             v2CCData.AttackerPos = transform.position;

//                             if (m_characterSkillCurrentHpRechargeData.HealTargetType == V2Enum_HealTargetType.All)
//                             {
//                                 Managers.MonsterManager.Instance.AllMonsyerHPRecoverPer(v2CCData);
//                             }
//                             else
//                             {
//                                 HPRecoverPer(v2CCData);
//                             }
//                         }
//                     }
//                 }

//                 if (m_nextPlaySkillActionScript != null)
//                 {
//                     if (aniaction == AnimationAction.AniStart
//                         || aniaction == AnimationAction.AniStartAndAction)
//                     {
//                         V2SkillAttackData v2SkillAttackData = m_creatureStatController.GetV2SkillAttackData(m_nextPlaySkillData, 1, transform.position, m_myActorType, m_myelement);
//                         m_nextPlaySkillActionScript.SetSkillAttackData(v2SkillAttackData);
//                         m_nextPlaySkillActionScript.SetActionCount(m_nextPlaySkillData.ActionCountBase);
//                     }

//                     m_nextPlaySkillActionScript.AniActionCallBack(aniaction);

//                     if (aniaction == AnimationAction.AniEnd)
//                     {
//                         m_attackTarget = Managers.AggroManager.Instance.GetTargetCharacter(m_mySearchType, this);

//                         ChangeState(CharacterState.Run);
//                         Updated();
//                         return;
//                     }
//                     return;
//                 }
//                 else
//                 {
//                     if (aniaction == AnimationAction.AniStartAndAction
//                             || aniaction == AnimationAction.AniAction)
//                     {
//                         V2SkillAttackData damageData = m_creatureStatController.GetV2SkillAttackData(m_nextPlaySkillData, 1, transform.position, m_myActorType, m_myelement);

//                         if (m_nextPlaySkillData.TargetSearchType == V2Enum_TargetSearchType.Ally)
//                         {
//                             Managers.MonsterManager.Instance.AllMonsyerDamage(damageData);
//                         }
//                         else if (m_nextPlaySkillData.TargetSearchType == V2Enum_TargetSearchType.Character)
//                         {
//                             Managers.PlayerManager.Instance.PlayerOnDamage(damageData);
//                         }
//                         else
//                             Managers.SkillTriggerManager.Instance.RecvDamageDate(m_nextPlaySkillData, damageData, this, m_myActorType);
//                     }

//                     if (aniaction == AnimationAction.AniEnd)
//                     {
//                         m_attackTarget = Managers.AggroManager.Instance.GetTargetCharacter(m_mySearchType, this);

//                         ChangeState(CharacterState.Run);
//                         Updated();
//                         return;
//                     }
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public override void HPRecoverPer(double ratio)
//         {
//             base.HPRecoverPer(ratio);

//             if (m_isBossMonster == true)
//             {
//                 Managers.MonsterManager.Instance.SendBossMonsterHP(m_currentHP, m_maxHP);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected override void PlayOnDamageDirection()
//         {
//             if (m_isBossMonster == false)
//             {
//                 if (m_uiCharacterState != null)
//                     m_uiCharacterState.gameObject.SetActive(true);

                
//             }
//             else
//             { 
//                 Managers.MonsterManager.Instance.SendBossMonsterHP(m_currentHP, m_maxHP);

//                 if (m_currentHP > 0)
//                 {
//                     SetHitRecoveryTime(Managers.MonsterManager.Instance.HitRecoveryTime);

//                     if (m_hitDirectionCoroutine != null)
//                         StopCoroutine(m_hitDirectionCoroutine);

//                     m_hitDirectionCoroutine = StartCoroutine(HitColorEffect());
//                 }
//             }

//             int Randomint = Random.Range(0, 4);
//             if (Randomint == 0)
//                 Managers.SoundManager.Instance.PlaySound("fx_combat_hit_1_6");
//             else if (Randomint == 1)
//                 Managers.SoundManager.Instance.PlaySound("fx_combat_hit_1_7");
//             else if (Randomint == 2)
//                 Managers.SoundManager.Instance.PlaySound("fx_combat_hit_1_9");
//             else if (Randomint == 3)
//                 Managers.SoundManager.Instance.PlaySound("fx_combat_hit_1_5");

//             if (Managers.BerserkerManager.isAlive == true)
//             {
//                 if (Managers.BerserkerManager.Instance.PlayingBerserkerMode() == true)
//                 {
//                     for (int i = 0; i < m_berserkerModeDamageParticle.Count; ++i)
//                     {
//                         m_berserkerModeDamageParticle[i].Stop();
//                         m_berserkerModeDamageParticle[i].Play();
//                     }
//                 }
//                 else
//                 {
//                     for (int i = 0; i < m_damageParticle.Count; ++i)
//                     {
//                         m_damageParticle[i].Stop();
//                         m_damageParticle[i].Play();
//                     }
//                 }
//             }
//             else
//             {
//                 for (int i = 0; i < m_damageParticle.Count; ++i)
//                 {
//                     m_damageParticle[i].Stop();
//                     m_damageParticle[i].Play();
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public override void EndCC(V2Enum_CrowdControlType cCType)
//         {
//             if (cCType == V2Enum_CrowdControlType.Snare)
//             {
//                 if (m_characterState == CharacterState.Run)
//                 {
//                     PlayAnimation(CharacterState.Run);
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected virtual void ReleaseMonster()
//         { // 진짜 객체가 끝났을 때 호출

//             if (m_charAniMaterial != null)
//             {
//                 //Managers.CharacterMaterialManager.Instance.SliceMaterial_Pool(m_charAniMaterial);
//                 Managers.CharacterMaterialManager.Instance.ColorMaterial_Pool(m_charAniMaterial);
//                 if (m_charAniMaterial != null)
//                 {
//                     m_charAniMaterial.SetColor("_Color", Color.white);
//                     m_charAniMaterial.SetFloat("_GlowRange", 0.0f);
//                     m_charAniMaterial.SetFloat("_GlowPower", 0.0f);
//                     m_charAniMaterial.SetFloat("_SliceRange", 0.0f);
//                 }
//             }

//             if (m_bodyRenderer != null)
//             {
//                 m_bodyRenderer.material = m_originMaterial;
//             }

//             //if (m_isBossMonster == true)
//             {
//                 if (m_myMonsterData != null)
//                 {
//                     if (m_myelement != V2Enum_ElementType.None)
//                     {
//                         if (m_bossParticle != null)
//                         {
//                             m_bossParticle.StopParticle();
//                         }

//                         m_bossParticle = null;
//                     }
//                 }
//             }

//             CreatureSkillRelease();
//             CreatureSkillRemove();
//             ReleaseSkill();
//             CallDead();
//         }
//         //------------------------------------------------------------------------------------
//         protected override void CallDead()
//         {
//             Managers.MonsterManager.Instance.ReleaseMonster(this);
//         }
//         //------------------------------------------------------------------------------------
//         public void ForceReleaseMonster()
//         { // 로직상 안전을 이유로 죽여야 할 때

//             if (m_characterState != CharacterState.Dead)
//             { // 죽을 땐 알아서 해제되니 냅두고 나머지 상태들만 처리한다.

//                 m_characterState = CharacterState.Dead;

//                 m_creatureDeadTime = 0.0f;

//                 if (m_skillHitReceiver != null)
//                     m_skillHitReceiver.EnableColliders(false);

//                 CreatureSkillRelease();
//                 CreatureSkillRemove();
//                 ReleaseSkill();
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected override void ChangeState(CharacterState state)
//         {
//             if (m_characterState == state)
//                 return;

//             if (m_characterState == CharacterState.Hit)
//             {
//                 ReleaseHitDirection();
//             }

//             if (m_characterState == CharacterState.Skill)
//             {
//                 if (m_characterState == CharacterState.Skill)
//                     m_playSkillTime = Time.time + m_skillData.CoolTime;
//             }

//             m_aniControllerSpeed = 1.0f;

//             if (state == CharacterState.None)
//                 return;

//             switch (state)
//             {
//                 case CharacterState.Idle:
//                     {
//                         break;
//                     }
//                 case CharacterState.Run:
//                     {
//                         m_aniControllerSpeed = m_creatureMoveSpeed;


//                         if (m_cCStater != null)
//                         {
//                             if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Slow))
//                             {
//                                 m_aniControllerSpeed = (1.0f - (float)(m_cCStater.GetCCValue(V2Enum_CrowdControlType.Slow) * Define.PercentageRecoverValue));
//                             }
//                         }

//                         break;
//                     }
//                 case CharacterState.Attack:
//                     {
//                         m_aniControllerSpeed = m_creatureStatController.GetOutPutAttackSpeed();
                        
//                         if (m_nextPlaySkillData == null)
//                             m_nextPlaySkillData = m_attackData;

//                         if (m_nextPlaySkillData != null)
//                         {
//                             if (m_nextPlaySkillData.UseAniSpeed == 1)
//                                 m_aniControllerSpeed = m_nextPlaySkillData.AniSpeed;
//                         }

//                         if (m_cCStater != null)
//                         {
//                             if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Slow))
//                             {
//                                 m_aniControllerSpeed = (1.0f - (float)(m_cCStater.GetCCValue(V2Enum_CrowdControlType.Slow) * Define.PercentageRecoverValue));
//                             }
//                         }

//                         if (m_attackTarget == null)
//                         {
//                             ChangeCharacterLookAtDirection(StageGenerateDirections.Left);
//                         }
//                         else
//                         {
//                             ChangeCharacterLookAtDirection_Target(m_attackTarget.transform);
//                         }
                        
//                         break;
//                     }
//                 case CharacterState.Skill:
//                     {
//                         m_aniControllerSpeed = m_creatureStatController.GetOutPutAttackSpeed();

//                         if (m_nextPlaySkillData == null)
//                             m_nextPlaySkillData = m_skillData;

//                         if (m_nextPlaySkillData != null)
//                         {
//                             if (m_nextPlaySkillData.UseAniSpeed == 1)
//                                 m_aniControllerSpeed = m_nextPlaySkillData.AniSpeed;
//                         }

//                         if (m_cCStater != null)
//                         {
//                             if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Slow))
//                             {
//                                 m_aniControllerSpeed = (1.0f - (float)(m_cCStater.GetCCValue(V2Enum_CrowdControlType.Slow) * Define.PercentageRecoverValue));
//                             }
//                         }

//                         if (m_attackTarget == null)
//                         {
//                             ChangeCharacterLookAtDirection(StageGenerateDirections.Left);
//                         }
//                         else
//                         {
//                             ChangeCharacterLookAtDirection_Target(m_attackTarget.transform);
//                         }

//                         break;
//                     }
//                 case CharacterState.Hit:
//                     {
//                         CreatureSkillRelease();

//                         //SetHitRecoveryTime(Managers.MonsterManager.Instance.HitRecoveryTime);

//                         //if (m_characterState != CharacterState.Hit)
//                         //{
//                         //    m_charAniMaterial = Managers.CharacterMaterialManager.Instance.GetColorMaterial();

//                         //    if (m_bodyRenderer != null)
//                         //    {
//                         //        m_bodyRenderer.material = m_charAniMaterial;
//                         //    }
//                         //}

//                         break;
//                     }
//                 case CharacterState.Dead:
//                     {
//                         if (m_hitDirectionCoroutine != null)
//                         {
//                             ReleaseHitDirection();
//                         }

//                         Managers.MonsterManager.Instance.DeadMonster(m_spawnID, m_isBossMonster);
//                         //m_monsterDeadTime = Time.time * 100.0f;
//                         m_creatureDeadTime = Time.time;
//                         m_releaseWaitTime = Managers.MonsterManager.Instance.DeadTime;
//                         m_deadDirectionTime = Managers.MonsterManager.Instance.DeadDirectionTime;

//                         for (int i = 0; i < m_deadParticle.Count; ++i)
//                         {
//                             m_deadParticle[i].Stop();
//                             m_deadParticle[i].Play();
//                         }

//                         if (m_skillHitReceiver != null)
//                             m_skillHitReceiver.EnableColliders(false);

//                         //m_charAniMaterial = Managers.CharacterMaterialManager.Instance.GetSliceMaterial();
//                         m_charAniMaterial = Managers.CharacterMaterialManager.Instance.GetColorMaterial();
//                         if (m_bodyRenderer != null)
//                         {
//                             m_bodyRenderer.material = m_charAniMaterial;
//                         }

//                         //if (m_deadDirectionCoroutine != null)
//                         //    StopCoroutine(m_deadDirectionCoroutine);
//                         //m_deadDirectionCoroutine = StartCoroutine(MonsterDeadMonster());

//                         if (m_cCStater != null)
//                             m_cCStater.ReleaseAllCC();

//                         ReleaseParticle();

//                         m_charAnicontroller.HideBodyOrderSprite();

//                         break;
//                     }
//                 default:
//                     {
//                         m_aniControllerSpeed = 1.0f;
//                         break;
//                     }
//             }

//             m_characterState = state;

//             PlayAnimation(m_characterState);
//         }
//         //------------------------------------------------------------------------------------
//         public override double GetOutPutMyStat(V2Enum_Stat v2Enum_Stat)
//         {
//             return m_creatureStatController.GetOutPutStatValue(v2Enum_Stat);
//         }
//         //------------------------------------------------------------------------------------
//     }
// }