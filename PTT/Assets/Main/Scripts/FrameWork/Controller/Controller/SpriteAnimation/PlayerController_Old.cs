// using System.Collections.Generic;
// using UnityEngine;

// namespace GameBerry.Old
// {
//     public class PlayerController_Old : CharacterControllerBase
//     {
//         [SerializeField]
//         private SkillController m_skillController;

//         private CharacterSkillData m_selectSkillData = null;

//         private Vector3 m_originPos = Vector3.one;

//         private CharacterState m_characterReservationState = CharacterState.None;

//         private bool m_isDirectionRun = false;
//         [SerializeField]
//         private float m_directionRunMoveSpeed = 4.0f;

//         private float m_directionStartTime = 0.0f;

//         private V2CCData berserkerCleansing;
//         private V2CCData berserkerInvincible;

//         private V2SkillAttackData berserkerModeAttackData;

//         private Transform m_limitLine = null;
//         private Transform m_limitLine_L = null;

//         //------------------------------------------------------------------------------------
//         public override void Init()
//         {
//             m_groupIndex = Define.PlayerSpriteResourceName;
//             m_variationNumber = Managers.CharacterSkinManager.Instance.GetSkinBodyNumber();

//             Managers.AggroManager.Instance.AddCharacterAggro(this);

//             if (m_uiCharacterState != null)
//                 m_uiCharacterState.Init();

//             if (m_charAnicontroller != null)
//             {
//                 m_charAnicontroller.Init(transform);
//                 m_charAnicontroller.ConnectAniActionState(AniActionCallBack);
//                 m_charAnicontroller.SetAnimationSpriteLibrary();
//             }

//             if (m_skillController != null)
//                 m_skillController.Init();

//             m_limitLine = InGamePositionContainer.Instance.GetMonsterLimitLine();
//             m_limitLine_L = InGamePositionContainer.Instance.GetMonsterLimitLine_L();

//             // 체력 셋팅
//             m_maxHP = Managers.CharacterStatManager.Instance.GetOutPutHP();

//             m_currentHP = m_maxHP;

//             Managers.CharacterStatManager.Instance.AddStatRefrashEvent(V2Enum_Stat.MoveSpeed, RefreshMoveSpeed);

//             Managers.CharacterStatManager.Instance.AddStatRefrashEvent(V2Enum_Stat.Hp, RefreshHP);
//             Managers.CharacterStatManager.Instance.AddStatRefrashEvent(V2Enum_Stat.HpIncrease, RefreshHP);
//             m_addSortingRenderer = 1000;

//             Managers.CharacterGearManager.Instance.ChangeWeapon += ChangeWeaponParticle;

//             CharacterSkinData characterSkinData = Managers.CharacterSkinManager.Instance.GetCurrentSlotSkin(V2Enum_Skin.SkinWeapon);
//             if (characterSkinData == null)
//                 ChangeWeaponParticle(Managers.CharacterGearManager.Instance.GetCurrentSlotGear(V2Enum_Goods.Weapon));
//             else
//                 ChangeWeaponParticle(characterSkinData.MyFakeGearData);

//             if (Managers.BerserkerManager.isAlive == true)
//             {
//                 Managers.BerserkerManager.Instance.StartBerserkerModeEvent += StartBerserkerMode;
//                 Managers.BerserkerManager.Instance.EndBerserkerModeEvent += EndBerserkerMode;
//             }
//         }
//         //------------------------------------------------------------------------------------
//         private void StartBerserkerMode()
//         {
//             BerserkerModeData berserkerModeData = Managers.BerserkerManager.Instance.GetCurrentBerserkerModeData();

//             if (berserkerModeData == null)
//                 return;

//             berserkerCleansing.CCTypeEnum = V2Enum_CrowdControlType.Cleansing;
//             berserkerCleansing.CCTime = 1000.0f;
//             berserkerCleansing.CCValue = 1000.0f;
//             berserkerCleansing.AttackerPos = transform.position;

//             berserkerInvincible.CCTypeEnum = V2Enum_CrowdControlType.Invincible;
//             berserkerInvincible.CCTime = berserkerModeData.InvincibleDuration;
//             berserkerInvincible.CCValue = berserkerModeData.InvincibleDuration;
//             berserkerInvincible.AttackerPos = transform.position;

//             if (m_cCStater != null)
//             {
//                 m_cCStater.PlayApplyCC(berserkerCleansing);
//                 m_cCStater.PlayApplyCC(berserkerInvincible);
//             }
            
//             m_variationNumber = 99;

//             if (m_charAnicontroller != null)
//             {
//                 m_charAnicontroller.SetAnimationSpriteLibrary();
//             }

//             transform.position = InGamePositionContainer.Instance.GetPlayerBerserkerModePos().position;

//             ChangeCharacterLookAtDirection(StageGenerateDirections.Right);

//             ChangeState(CharacterState.Idle);
//             PlayAnimation(CharacterState.Skill, "BerserkerMode");

//             berserkerModeAttackData = Managers.CharacterStatManager.Instance.GetV2SkillAttackData(Managers.CharacterSkillManager.Instance.GetBasicAttackSkillData());
//             m_aniControllerSpeed = Managers.CharacterStatManager.Instance.GetOutPutAttackSpeed();
//         }
//         //------------------------------------------------------------------------------------
//         private void EndBerserkerMode()
//         {
//             m_variationNumber = Managers.CharacterSkinManager.Instance.GetSkinBodyNumber();

//             if (m_charAnicontroller != null)
//             {
//                 m_charAnicontroller.SetAnimationSpriteLibrary();
//             }

//             ChangeState(CharacterState.Run);
//         }
//         //------------------------------------------------------------------------------------
//         public void RefreshBodySkin()
//         {
//             if (m_variationNumber == 99)
//                 return;

//             m_variationNumber = Managers.CharacterSkinManager.Instance.GetSkinBodyNumber();

//             if (m_charAnicontroller != null)
//             {
//                 m_charAnicontroller.SetAnimationSpriteLibrary();
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public bool CanCoolTime()
//         {
//             if (m_characterState == CharacterState.Dead
//                 || m_characterState == CharacterState.None
//                 || m_characterState == CharacterState.Idle)
//                 return false;

//             return true;
//         }
//         //------------------------------------------------------------------------------------
//         public void StartHunting()
//         {  // 사냥을 시작해야 할 때 호출
//             m_isDirectionRun = false;
//             m_characterReservationState = CharacterState.None;
//             ChangeState(CharacterState.Run);

//             if (Managers.BerserkerManager.Instance.GetAutoBerserkerMode() == true)
//                 Managers.BerserkerManager.Instance.StartBerserkerMode();
//         }
//         //------------------------------------------------------------------------------------
//         public virtual void ResetPlayer()
//         { // 주로 다른던전에 들어갈 때 직후나 던전 리트라이를 할 때 한다.
//             transform.position = InGamePositionContainer.Instance.GetPlayerStadardPos().position;
//             m_characterReservationState = CharacterState.None;

//             ChangeState(CharacterState.Idle);

//             SetMaxHP(Managers.CharacterStatManager.Instance.GetOutPutHP());

//             SetHP(m_maxHP);
//         }
//         //------------------------------------------------------------------------------------
//         public void PlayDirectionRun()
//         {
//             m_isDirectionRun = true;
//             m_directionStartTime = Time.time;

//             ChangeState(CharacterState.Idle);
//             PlayAnimation(CharacterState.Run);
//         }
//         //------------------------------------------------------------------------------------
//         public void StopPlayer()
//         { // 캐릭터를 멈춘다. 완전 리셋되는건 아니고 그냥 멈추기만 한다.
//             m_characterReservationState = CharacterState.None;
//             ChangeState(CharacterState.Idle);
//         }
//         //------------------------------------------------------------------------------------
//         public void ActionAndStopPlayer()
//         {
//             m_characterReservationState = CharacterState.Idle;
//         }
//         //------------------------------------------------------------------------------------
//         protected override void Updated()
//         {
//             float ypos = transform.position.y;
//             m_addSortingRenderer = 1000 + (int)(-10.0f * ypos);

// #if DEV_DEFINE
//             if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop)
//             {
//                 if (Input.GetKey(KeyCode.LeftArrow))
//                 {
//                     ChangeState(CharacterState.Run);

//                     ChangeCharacterLookAtDirection(StageGenerateDirections.Left);

//                     Vector3 pos = transform.position;
//                     pos.x += GetPlayerMoveSpeed() * Time.deltaTime * -1.0f;
//                     transform.position = pos;

//                     return;
//                 }
//                 else if (Input.GetKey(KeyCode.RightArrow))
//                 {
//                     ChangeState(CharacterState.Run);

//                     ChangeCharacterLookAtDirection(StageGenerateDirections.Right);

//                     Vector3 pos = transform.position;
//                     pos.x += GetPlayerMoveSpeed() * Time.deltaTime * 1.0f;
//                     transform.position = pos;

//                     return;
//                 }
//             }
// #endif


//             if (m_attackTarget == null || m_attackTarget.IsDead == true)
//             {
//                 m_attackTarget = null;

//                 if (Managers.MonsterManager.Instance.AllMonsterDead == true)
//                 {
//                     ActionAndStopPlayer();
//                 }
//                 else
//                 {
//                     float mindis = float.MaxValue;

//                     foreach (KeyValuePair<string, MonsterController> pair in Managers.MonsterManager.Instance.SpawnedMonster_Dic)
//                     {
//                         float distance = MathDatas.GetDistance(transform.position.x, transform.position.y, pair.Value.transform.position.x, pair.Value.transform.position.y);
//                         distance = pair.Value.transform.position.x - transform.position.x;
//                         //distance = MathDatas.GetDistance(transform.position, pair.Value.transform.position);
//                         if (mindis > distance)
//                             m_attackTarget = pair.Value;

//                         mindis = distance;
//                     }
//                 }
//             }

//             SelectState();

//             switch (m_characterState)
//             {
//                 case CharacterState.Run:
//                     {
//                         if (m_attackTarget == null)
//                         {
//                             ChangeCharacterLookAtDirection(StageGenerateDirections.Right);
//                         }
//                         else
//                         {
//                             ChangeCharacterLookAtDirection_Target(m_attackTarget.transform);
//                         }

//                         break;
//                     }
//                 case CharacterState.Attack:
//                     {

//                         break;
//                     }
//                 case CharacterState.Hit:
//                     {
//                         break;
//                     }
//                 case CharacterState.Dead:
//                     {
//                         break;
//                     }
//                 case CharacterState.None:
//                     {
//                         break;
//                     }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         private void SelectState()
//         {
//             if (m_characterState == CharacterState.None || m_characterState == CharacterState.Dead)
//                 return;

//             if (m_isDirectionRun == true)
//                 return;

//             if (m_characterState == CharacterState.Hit)
//             {
//                 if (Time.time > m_hitRecoveryStartTime + m_hitRecoveryTime)
//                 {
//                     ChangeState(CharacterState.Run);
//                 }
//                 return;
//             }

//             if (m_characterState == CharacterState.Idle)
//                 return;

//             if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill != null)
//             {
//                 if (m_characterState == CharacterState.Attack && m_attackTarget != null)
//                 {
//                     if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TriggerType == V2Enum_TriggerType.Active)
//                     {
//                         float checkConditionRange = Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TargetCheckScale * GetOutputAttackRange();

//                         if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TargetCheckType == V2Enum_TargetCheckType.Circle)
//                             checkConditionRange *= 0.5f;

//                         float distance = MathDatas.GetDistance(transform.position.x, transform.position.y, m_attackTarget.transform.position.x, m_attackTarget.transform.position.y);

//                         if (distance < checkConditionRange)
//                         {
//                             DummySkillPlayer dummySkillPlayer = Managers.PlayerManager.Instance.GetDummySkillPlayer();

//                             if (dummySkillPlayer != null)
//                             {
//                                 PlayDummySkillPlayer(dummySkillPlayer);

//                                 ChangeState(CharacterState.None);
//                             }
//                         }
//                         else
//                             Managers.CharacterSkillSlotManager.Instance.SetNextActiveSkill();
//                     }
//                 }



//                 if (m_characterState != CharacterState.Attack
//                     && m_characterState != CharacterState.Skill)
//                 {
//                     if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TriggerType == V2Enum_TriggerType.Default && m_attackTarget != null)
//                     {
//                         float distance = MathDatas.GetDistance(transform.position.x, transform.position.y, m_attackTarget.transform.position.x, m_attackTarget.transform.position.y);

//                         if (distance < Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TargetCheckScale * GetOutputAttackRange())
//                         {
//                             m_selectSkillData = Managers.CharacterSkillSlotManager.Instance.NextActiveSkill;

//                             Managers.CharacterSkillSlotManager.Instance.UseSkill(m_selectSkillData);

//                             ChangeState(CharacterState.Attack);
//                             return;
//                         }
//                     }
//                     else if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TriggerType == V2Enum_TriggerType.Active)
//                     {
//                         bool doSkill = false;

//                         if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TargetCheckType == V2Enum_TargetCheckType.Self)
//                         {
//                             doSkill = true;
//                             Managers.SoundManager.Instance.PlaySound("fx_combat_attack_2_1");
//                         }
//                         else
//                         {
//                             float checkConditionRange = Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TargetCheckScale * GetOutputAttackRange();
//                             if (checkConditionRange < 0.0f)
//                             {
//                                 doSkill = true;
//                             }
//                             else
//                             {
//                                 if (m_attackTarget != null)
//                                 {
//                                     float distance = MathDatas.GetDistance(transform.position.x, transform.position.y, m_attackTarget.transform.position.x, m_attackTarget.transform.position.y);

//                                     if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TargetCheckType == V2Enum_TargetCheckType.Circle)
//                                         checkConditionRange *= 0.5f;

//                                     if (distance < checkConditionRange)
//                                     {
//                                         doSkill = true;
//                                     }
//                                 }
//                             }
//                         }

//                         if (doSkill == true)
//                         {
//                             m_selectSkillData = Managers.CharacterSkillSlotManager.Instance.NextActiveSkill;

//                             Managers.CharacterSkillSlotManager.Instance.UseSkill(m_selectSkillData);

//                             ChangeState(CharacterState.Skill);
//                             return;
//                         }
//                         else
//                             Managers.CharacterSkillSlotManager.Instance.SetNextActiveSkill();
//                     }
//                     else
//                     {
//                         ChangeState(CharacterState.Run);
//                         return;
//                     }
//                 }

//             }

//         }
//         //------------------------------------------------------------------------------------
//         private void LateUpdate()
//         { 
//             if (m_isDirectionRun == true)
//             {
//                 Transform mypos = InGamePositionContainer.Instance.GetPlayerStadardPos();

//                 if ((mypos.position - transform.position).magnitude < 0.3f)
//                 {
//                     transform.position = mypos.position;
//                     ChangeCharacterLookAtDirection(StageGenerateDirections.Right);
//                 }
//                 else if (Time.time - m_directionStartTime < Define.CharacterDirectionRunDuration)
//                 {
//                     float ratio = (Time.time - m_directionStartTime) / Define.CharacterDirectionRunDuration;

//                     Vector3 targetpos = mypos.position;
//                     Vector3 pos = transform.position;
//                     Vector3 posGab = targetpos - pos;

//                     transform.position += posGab * ratio;

//                     ChangeCharacterLookAtDirection_Target(mypos);
//                 }
//                 else
//                 {
//                     transform.position = mypos.position;
//                     ChangeCharacterLookAtDirection(StageGenerateDirections.Right);
//                 }

//                 //ChangeCharacterLookAtDirection_Target(mypos);
//                 //Vector3 DirectionVec = mypos.position - transform.position;
//                 //Vector3 pos = transform.position;
//                 //pos += DirectionVec * Time.deltaTime * m_directionRunMoveSpeed;

//                 //transform.position = pos;

//                 return;
//             }

//             if (Managers.BerserkerManager.Instance.PlayingBerserkerMode() == true)
//             {
//                 transform.position = InGamePositionContainer.Instance.GetPlayerBerserkerModePos().position;

//                 ChangeCharacterLookAtDirection(StageGenerateDirections.Right);
//             }

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
//         private void FixedUpdate()
//         {
//             if (m_characterState == CharacterState.Run)
//             {
//                 float movespeed = GetPlayerMoveSpeed();

//                 if (m_cCStater != null)
//                 {
//                     if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Snare))
//                     {
//                         movespeed = 0.0f;
//                         m_aniControllerSpeed = 1.0f;
//                         PlayAnimation(CharacterState.Idle);
//                     }
//                     else if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Slow))
//                     {
//                         movespeed = movespeed * (1.0f - (float)(m_cCStater.GetCCValue(V2Enum_CrowdControlType.Slow) * Define.PerStatRecoverValue));
//                         m_aniControllerSpeed = movespeed;
//                     }
//                     else
//                         m_aniControllerSpeed = movespeed;
//                 }
//                 else
//                     m_aniControllerSpeed = movespeed;

//                 Vector2 dirVec = m_lookDirection == StageGenerateDirections.Right ? Vector2.right : Vector2.left;

//                 if (m_attackTarget != null)
//                 {
//                     dirVec = m_attackTarget.transform.position - transform.position;
//                     dirVec.y *= 3.0f;
//                     dirVec.Normalize();
//                 }

//                 m_rigidbody2D.MovePosition(m_rigidbody2D.position + (movespeed * Time.deltaTime * dirVec));
//             }


//         }
//         //------------------------------------------------------------------------------------
//         private void PlayDummySkillPlayer(DummySkillPlayer dummySkillPlayer)
//         {
//             dummySkillPlayer.SetPlayer(this);
//             dummySkillPlayer.SetSkillController(m_skillController);
//             dummySkillPlayer.SetCharAniController(m_charAnicontroller);
//             dummySkillPlayer.PlayDummySkillPlayer();

//             if (m_skillController != null)
//                 m_skillController.ForceReleaseSkill();
//         }
//         //------------------------------------------------------------------------------------
//         private void AniActionCallBack(AnimationAction aniaction)
//         {
//             if (m_characterState == CharacterState.Skill
//                 || m_characterState == CharacterState.Attack)
//             {
//                 if (m_skillController != null)
//                     m_skillController.AniActionCallBack(aniaction);

//                 if (aniaction == AnimationAction.AniEnd)
//                     m_attackTarget = Managers.AggroManager.Instance.GetTargetMonster(Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TargetSearchType, this);
//             }
//             else
//             {
//                 if (m_characterState == CharacterState.Idle)
//                 {
//                     if (Managers.BerserkerManager.isAlive == true)
//                     {
//                         if (Managers.BerserkerManager.Instance.PlayingBerserkerMode() == true)
//                         {
//                             if (aniaction == AnimationAction.AniAction)
//                             {
//                                 Managers.SkillTriggerManager.Instance.RecvDamageDate(Managers.CharacterSkillManager.Instance.GetAllAttackSkillData(), berserkerModeAttackData, this, m_myActorType);

//                                 //foreach (KeyValuePair<string, MonsterController> pair in Managers.MonsterManager.Instance.SpawnedMonster_Dic)
//                                 //{
//                                 //    pair.Value.MySkillHitReceiver.RecvHitData(berserkerModeAttackData);
//                                 //}
//                                 return;
//                             }
//                         }
//                     }
//                 }

//                 if (m_characterState != CharacterState.Dead)
//                 {
//                     if (m_characterReservationState != CharacterState.None)
//                     {
//                         ChangeState(m_characterReservationState);
//                         m_characterReservationState = CharacterState.None;
//                     }
//                 }
//             }
//         }
//         //------------------------------------------------------------------------------------
//         public override void ReleaseSkill()
//         {
//             if (m_characterReservationState != CharacterState.None)
//             {
//                 ChangeState(m_characterReservationState);
//                 m_characterReservationState = CharacterState.None;
//             }
//             else
//                 ChangeState(CharacterState.Run);
//         }
//         //------------------------------------------------------------------------------------
//         public override void SendDamage(CharacterSkillData characterSkillData)
//         {
//             V2SkillAttackData damageData = Managers.CharacterStatManager.Instance.GetV2SkillAttackData(characterSkillData);

//             if (m_characterState == CharacterState.Attack && m_attackTarget != null)
//             {
//                 int Randomint = Random.Range(0, 3);
//                 if (Randomint == 0)
//                     Managers.SoundManager.Instance.PlaySound("fx_combat_attack_1_3");
//                 else if (Randomint == 1)
//                     Managers.SoundManager.Instance.PlaySound("fx_combat_attack_1_8");
//                 else if (Randomint == 2)
//                     Managers.SoundManager.Instance.PlaySound("fx_combat_attack_1_11");


//                 m_attackTarget.MySkillHitReceiver.RecvHitData(damageData);
//             }
//             else
//             {
//                 Managers.SkillTriggerManager.Instance.RecvDamageDate(characterSkillData, damageData, this, m_myActorType);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         protected override void InCreaseHP(double hp)
//         {
//             base.InCreaseHP(hp);
//         }
//         //------------------------------------------------------------------------------------
//         protected override void DeCreaseHP(double hp)
//         {
//             if (Managers.GameSettingManager.Instance.Cheat_NoDamage() == true)
//                 return;

//             base.DeCreaseHP(hp);
//         }
//         //------------------------------------------------------------------------------------
//         private void PlaySelfSkill(CharacterSkillData characterSkillData)
//         {
//             if (characterSkillData.SkillEffectDatas.ContainsKey(V2Enum_EffectType.CurrentHpRecharge) == true)
//             {
//                 List<SkillBase> characterSkillDataBaseList = characterSkillData.SkillEffectDatas[V2Enum_EffectType.CurrentHpRecharge];

//                 for (int index = 0; index < characterSkillDataBaseList.Count; ++index)
//                 {
//                     CharacterSkillCurrentHpRechargeData characterSkillCurrentHpRechargeData = characterSkillDataBaseList[index] as CharacterSkillCurrentHpRechargeData;
//                     if (characterSkillCurrentHpRechargeData == null)
//                         continue;

//                     double healValue = GetSkillCurrentHpRechargeValue(characterSkillData, characterSkillCurrentHpRechargeData);

//                     V2CCData v2CCData;
//                     v2CCData.CCTypeEnum = V2Enum_CrowdControlType.HOT;
//                     v2CCData.CCTime = characterSkillCurrentHpRechargeData.Duration;
//                     v2CCData.CCValue = healValue;

//                     v2CCData.AttackerPos = transform.position;

//                     if (characterSkillCurrentHpRechargeData.HealTargetType == V2Enum_HealTargetType.All)
//                     {
//                         HPRecoverPer(v2CCData);
//                         Managers.AllyControllerManager.Instance.AllAllyHPRecoverPer(v2CCData);
//                     }
//                     else
//                     {
//                         HPRecoverPer(v2CCData);
//                     }
//                 }
//             }

//             if (characterSkillData.SkillEffectDatas.ContainsKey(V2Enum_EffectType.CooltimeRecharge) == true)
//             {
//                 List<SkillBase> characterSkillDataBaseList = characterSkillData.SkillEffectDatas[V2Enum_EffectType.CooltimeRecharge];

//                 for (int index = 0; index < characterSkillDataBaseList.Count; ++index)
//                 {
//                     CharacterSkillCooltimeRechargeData characterSkillCooltimeRechargeData = characterSkillDataBaseList[index] as CharacterSkillCooltimeRechargeData;
//                     if (characterSkillCooltimeRechargeData == null)
//                         continue;

//                     DoCooltimeRecharge(characterSkillData, characterSkillCooltimeRechargeData);
//                 }
//             }

//             V2SkillAttackData v2SkillAttackData = Managers.CharacterStatManager.Instance.GetV2SkillAttackData(characterSkillData);
//             OnDamage(v2SkillAttackData);
//         }
//         //------------------------------------------------------------------------------------
//         private void SetMaxHP(double maxhp)
//         {
//             double currratio = m_currentHP / m_maxHP;
//             m_currentHP = maxhp * currratio;

//             m_maxHP = maxhp;
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
//         public override V2SkillAttackData GetCharacterDamageData(CharacterSkillData characterSkillData)
//         {
//             return Managers.CharacterStatManager.Instance.GetV2SkillAttackData(characterSkillData);
//         }
//         //------------------------------------------------------------------------------------
//         private double GetSkillCurrentHpRechargeValue(CharacterSkillData characterSkillData, CharacterSkillCurrentHpRechargeData characterSkillCurrentHpRechargeData)
//         {
//             return Managers.CharacterSkillManager.Instance.GetSkillCurrentHpRechargeValue(characterSkillData, characterSkillCurrentHpRechargeData);
//         }
//         //------------------------------------------------------------------------------------
//         private void DoCooltimeRecharge(CharacterSkillData characterSkillData, CharacterSkillCooltimeRechargeData characterSkillCooltimeRechargeData)
//         {
//             double cooltimeRecharge = Managers.CharacterSkillManager.Instance.GetSkillCooltimeRechargeValue(characterSkillData, characterSkillCooltimeRechargeData);

//             cooltimeRecharge *= Define.PercentageRecoverValue;

//             Managers.CharacterSkillSlotManager.Instance.DecreaseRemainCoolTime((float)cooltimeRecharge);
//         }
//         //------------------------------------------------------------------------------------
//         protected override void ChangeState(CharacterState characterState)
//         {
//             if (m_characterState == characterState)
//                 return;

//             if (Managers.BerserkerManager.isAlive == true && Managers.BerserkerManager.Instance.PlayingBerserkerMode() == true)
//             {
//                 if (characterState == CharacterState.Dead)
//                 {
//                     Managers.BerserkerManager.Instance.PlayerDead();
//                 }
//                 else
//                     return;
//             }

//             m_characterState = characterState;

//             m_aniControllerSpeed = 1.0f;

//             switch (m_characterState)
//             {
//                 case CharacterState.Idle:
//                     {
//                         break;
//                     }
//                 case CharacterState.Attack:
//                 case CharacterState.Skill:
//                     {
//                         if (m_selectSkillData.TriggerType == V2Enum_TriggerType.Active
//                             || m_selectSkillData.TriggerType == V2Enum_TriggerType.Default)
//                         {
//                             if (m_attackTarget != null)
//                             {
//                                 Vector2 direction = m_attackTarget.transform.position - transform.position;
//                                 direction.Normalize();

//                                 StageGenerateDirections nextdirection = direction.x < 0 ? StageGenerateDirections.Left : StageGenerateDirections.Right;

//                                 if (nextdirection != m_lookDirection)
//                                 {
//                                     m_isTurnAttack = true;
//                                 }
//                                 else
//                                 {
//                                     m_isTurnAttack = false;
//                                 }

//                                 ChangeCharacterLookAtDirection(nextdirection);
//                             }
//                         }

//                         if (m_selectSkillData.UseAniSpeed == 1)
//                         {
//                             m_aniControllerSpeed = m_selectSkillData.AniSpeed;
//                         }
//                         else
//                         {
//                             m_aniControllerSpeed = Managers.CharacterStatManager.Instance.GetOutPutAttackSpeed();
//                         }

//                         if (m_cCStater != null)
//                         {
//                             if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Slow))
//                             {
//                                 m_aniControllerSpeed = m_aniControllerSpeed * (1.0f - (float)(m_cCStater.GetCCValue(V2Enum_CrowdControlType.Slow) * Define.PercentageRecoverValue));
//                             }
//                         }

//                         if (m_selectSkillData.TargetCheckType == V2Enum_TargetCheckType.Self)
//                         {
//                             PlaySelfSkill(m_selectSkillData);
//                         }
//                         else
//                         {
//                             m_skillController.PlaySkill(m_selectSkillData);
//                         }
                        
//                         break;
//                     }
//                 case CharacterState.Run:
//                     {
//                         m_aniControllerSpeed = GetPlayerMoveSpeed();

//                         if (m_cCStater != null)
//                         {
//                             if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Slow))
//                             {
//                                 m_aniControllerSpeed = m_aniControllerSpeed * (1.0f - (float)(m_cCStater.GetCCValue(V2Enum_CrowdControlType.Slow) * Define.PercentageRecoverValue));
//                             }
//                         }

//                         break;
//                     }
//                 case CharacterState.Hit:
//                     {
//                         break;
//                     }
//                 case CharacterState.Dead:
//                     {
//                         m_characterReservationState = CharacterState.None;
//                         Managers.PlayerManager.Instance.PlayerDead();

//                         if (m_cCStater != null)
//                             m_cCStater.ReleaseAllCC();

//                         break;
//                     }
//             }

//             if (characterState == CharacterState.Skill)
//             {
//                 PlayAnimation(characterState, m_selectSkillData.AniStringKey);
//             }
//             else if (characterState == CharacterState.Dead)
//             {
//                 PlayAnimation(characterState, "Dead");
//             }
//             else if (characterState == CharacterState.Hit)
//             {
//                 if (m_cCStater.IsAppliedCC(V2Enum_CrowdControlType.Knockback) == true)
//                     PlayAnimation(characterState, "Dust");
//                 else
//                     PlayAnimation(characterState, "Hit");
//             }
//             else
//                 PlayAnimation(characterState);

//             Managers.AllyControllerManager.Instance.SetAllyState(characterState);
//         }
//         //------------------------------------------------------------------------------------
//         public void PlayDummySkill(CharacterSkillData skillData)
//         {
//             if (m_attackTarget == null)
//                 return;

//             if (Managers.CharacterSkillSlotManager.Instance.NextActiveSkill.TriggerType == V2Enum_TriggerType.Active)
//             {
//                 DummySkillPlayer dummySkillPlayer = Managers.PlayerManager.Instance.GetDummySkillPlayer();

//                 if (dummySkillPlayer != null)
//                 {
//                     dummySkillPlayer.SetPlayer(this);
//                     dummySkillPlayer.SetAniSpeed(1.0f);
//                     dummySkillPlayer.SetSKillData(skillData);
//                     dummySkillPlayer.PlayDummySkillPlayer();
//                 }
//             }

//             Managers.CharacterSkillSlotManager.Instance.UseSkill(skillData);
//         }
//         //------------------------------------------------------------------------------------
//         private void PlayAnimation(CharacterState state, string aniid = "")
//         {
//             if (m_charAnicontroller != null)
//             {
//                 m_charAnicontroller.PlayAnimation(state, aniid);
//             }
//         }
//         //------------------------------------------------------------------------------------
//         private float GetPlayerMoveSpeed()
//         {
//             return Managers.CharacterStatManager.Instance.GetOutputMoveSpeed();
//         }
//         //------------------------------------------------------------------------------------
//         private void RefreshMoveSpeed(double mystat)
//         {
//             if (m_characterState == CharacterState.Run)
//             {
//                 m_aniControllerSpeed = GetPlayerMoveSpeed();
//             }
//         }
//         //------------------------------------------------------------------------------------
//         private void RefreshHP(double mystat)
//         {
//             double percent = m_currentHP / m_maxHP;

//             double afterHP = Managers.CharacterStatManager.Instance.GetOutPutHP();

//             m_maxHP = afterHP;

//             SetHP(m_maxHP * percent);
//         }
//         //------------------------------------------------------------------------------------
//         public override double GetOutPutMyStat(V2Enum_Stat v2Enum_Stat)
//         {
//             return Managers.CharacterStatManager.Instance.GetOutPutStatValue(v2Enum_Stat);
//         }
//         //------------------------------------------------------------------------------------
//         public override float GetOutputAttackRange()
//         {
//             return Managers.CharacterStatManager.Instance.GetOutputAttackRange();
//         }
//         //------------------------------------------------------------------------------------
//     }
// }