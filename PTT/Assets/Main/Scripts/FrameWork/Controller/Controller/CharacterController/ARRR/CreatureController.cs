using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class CreatureController : CreatureControllerBase
    {
        #region New Member Variables

        private float _randomRange = 0.0f;

        private int _characterIndex = -1;
        public int CharacterIndex { get { return _characterIndex; } }

        private bool _deadDirection_Swing = false;
        private Vector3 _swingDirVec = Vector3.zero;

        public int AniNumber = 0;

        #endregion

        public event CallMonsterHitState SendHP;

        #region NewFunc
        //------------------------------------------------------------------------------------
        public override void Init()
        {
            CreatureStatOperator creatureStatOperator = new CreatureStatOperator(this);
            creatureStatOperator.SetCreature(this);

            _characterStatOperator = creatureStatOperator;

            MoveController_Base creatureBaseMove =  gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
        }
        //------------------------------------------------------------------------------------
        public override void OnRefreshBuff()
        {
            _characterStatOperator.RefreshOutputStatValue();

            RefreshStat();
        }
        //------------------------------------------------------------------------------------
        public void SetCreatureData(CreatureData creatureData, int level, Dictionary<V2Enum_Stat, ObscuredDouble> addDefaultStat = null)
        {
            _isMonster = true;
            _monsterGradeType = creatureData.MonsterGradeType;

            _deadDirection_Swing = false;

            Vector3 rotate = transform.localEulerAngles;
            rotate.z = 0.0f;
            transform.localEulerAngles = rotate;

            _uiCharacterState.transform.localEulerAngles = rotate;

            CreatureLevel = level;

            _creatureData = creatureData;

            _selectPlaySkillData = null;

            _defaultSearchType = creatureData.BasicAttackData.TargetCondition;
            _minTraceRange = creatureData.BasicAttackData.AttackRange;

            _skillActiveController.SetDefaultSkill(creatureData.BasicAttackData);

            if (creatureData.ActiveSkillData != null)
                _skillActiveController.AddSkillData(creatureData.ActiveSkillData);

            _skillActiveController.ReStartSkill();
            _selectPlaySkillData = _skillActiveController.GetReadyActiveSkill();

            ChangeSearchType();

            if (_iFFType == IFFType.IFF_Foe)
                _randomRange = Random.Range(0.0f, 1.0f);
            else
                _randomRange = 0.0f;

            SetCreatureSizeControll(creatureData.Scale.ToVector3());
            //SetCreatureSizeControll(0.6f.ToVector3());

            _characterStatOperator.RefreshDefaultStat();

            if (addDefaultStat != null)
            {
                foreach (var pair in addDefaultStat)
                {
                    double statvalue = _characterStatOperator.GetDefaultValue(pair.Key);

                    statvalue += pair.Value;

                    _characterStatOperator.SetDefaultStat(pair.Key, statvalue);
                }
            }

            _characterStatOperator.RefreshOutputStatValue();


            RefreshStat(true);

            _currentSpineModelData = StaticResource.Instance.GetCreatureSpineModelData(creatureData.ResourceIndex);

            SetAnimNumber(creatureData.AnimNumber);

            if (_currentSpineModelData != null)
            {
                if (creatureData.ResourceSkin != "-1")
                {
                    SetSpineModelData(_currentSpineModelData);
                    if (_currentSpineModelData.SkinList.Count > 0)
                        SetSkin(_currentSpineModelData.SkinList[0]);
                    ReleaseAttachSkin();

                    if (creatureData.DefaultSkin != "-1")
                        AddAttachSkin(creatureData.DefaultSkin);
                    //AddAttachSkin(string.Format("0{0}", AniNumber));
                    if (creatureData.ResourceSkin != "-1")
                    {
                        AddAttachSkin(creatureData.ResourceSkin);
                    }

                    RefreshAttachSkin();
                }
                else if (creatureData.DefaultSkin == "-1")
                {
                    SetSpineModelData(_currentSpineModelData);
                    ReleaseAttachSkin();
                }
                else
                {
                    SetSpineModelData(_currentSpineModelData);
                    SetSkin(creatureData.DefaultSkin);
                }
            }

            SendHP?.Invoke(0, _currentHP, _maxHP);
            ChangeState(CharacterState.Idle);
        }
        //------------------------------------------------------------------------------------
        public void SetOverrideStat(List<OperatorOverrideStat> overrideStat)
        {
            if (overrideStat == null)
                return;

            foreach (var pair in overrideStat)
            {
                OperatorOverrideStat operatorOverrideStat = pair;
                _characterStatOperator.SetDefaultStat(operatorOverrideStat.BaseStat, operatorOverrideStat.OverrideStatBaseValue);
            }

            _characterStatOperator.RefreshOutputStatValue();


            RefreshStat(true);

            SendHP?.Invoke(0, _currentHP, _maxHP);
        }
        //------------------------------------------------------------------------------------
        public void SetIndex(int characterIndex)
        {
            _characterIndex = characterIndex;
        }
        //------------------------------------------------------------------------------------
        public void SetLevel(int level)
        {
            CreatureLevel = level;

            _characterStatOperator.RefreshAllStatValue();

            RefreshStat();
        }
        //------------------------------------------------------------------------------------
        protected override void SpineAnimationEvent(string aniName, string eventName)
        {
            //Debug.Log(string.Format("{0} {1}", aniName, eventName));

            if (aniName.Contains("Attack") || aniName.Contains("Skill"))
            {
                if (eventName == "Start")
                    AniActionCallBack(AnimationAction.AniStart);
                else if (eventName == "AniAction" || eventName.Contains("Attack"))
                    AniActionCallBack(AnimationAction.AniAction);
                else if (eventName == "End")
                    AniActionCallBack(AnimationAction.AniEnd);
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region OldFunc
        //------------------------------------------------------------------------------------
        private void SetSelectSkillType()
        {
            _selectPlaySkillData = _skillActiveController.GetReadyActiveSkill();

            ChangeSearchType();
        }
        //------------------------------------------------------------------------------------
        private void AniActionCallBack(AnimationAction aniaction)
        {
            if (CharacterState == CharacterState.Attack || CharacterState == CharacterState.Skill)
            {
                //CreatureSkillActionBase m_nextPlaySkillActionScript = CharacterState == CharacterState.Skill ? _skillActionScript : _attackActionScript;

                //if (m_nextPlaySkillActionScript != null)
                //{
                //    if (aniaction == AnimationAction.AniStart
                //        || aniaction == AnimationAction.AniStartAndAction)
                //    {
                //        V2SkillAttackData damageData = Managers.CreatureManager.Instance.GetCreatureSkillAttackData(_selectPlaySkillData, this);
                //        m_nextPlaySkillActionScript.SetSkillAttackData(damageData);

                //        //if (mCharacterState == CharacterState.Skill)
                //        //    Managers.PlayerManager.Instance.PlayInGameBGFade(m_AllyCameraFade);
                //    }

                //    m_nextPlaySkillActionScript.AniActionCallBack(aniaction);

                //    if (aniaction == AnimationAction.AniEnd)
                //    {
                //        SetNewTarget();

                //        ChangeState(CharacterState.Run);
                //        Updated();
                //        return;
                //    }
                //    return;
                //}
                //else
                {
                    if (aniaction == AnimationAction.AniStartAndAction
                            || aniaction == AnimationAction.AniAction)
                    {
                        if (_selectPlaySkillData != null && _selectPlaySkillData.SkillBaseData != null && _selectPlaySkillData.SkillBaseData.SkillDamageIndex != null)
                        {
                            if (_selectPlaySkillData.SkillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.Projectile)
                            {
                                PlayProjectile(_selectPlaySkillData, AttackTarget);
                            }
                            else if (_selectPlaySkillData.SkillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.Pierce)
                            {
                                PlayPierce(_selectPlaySkillData, _attackTarget);
                            }
                            else if (_selectPlaySkillData.SkillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.Void)
                            {
                                PlayVoid(_selectPlaySkillData, _attackTarget);
                            }
                            else if (_selectPlaySkillData.SkillBaseData.SkillDamageIndex.DamageType == Enum_DamageType.RepeatAttack)
                            {
                                PlayRepeatAttack(_selectPlaySkillData, _attackTarget);
                            }
                            else
                                PlaySkill(_selectPlaySkillData, AttackTarget);
                        }
                        else
                            PlaySkill(_selectPlaySkillData, AttackTarget);
                    }

                    if (aniaction == AnimationAction.AniEnd)
                    {
                        SetNewTarget();

                        ChangeState(CharacterState.Run);
                        Updated();
                        return;
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------
        public void SetAniSpeed()
        {
            _mySkeletonAnimationHandler.SetAnimationSpeed(_aniControllerSpeed);
        }
        //------------------------------------------------------------------------------------
        protected override void ChangeState(CharacterState state)
        {
            if (CharacterState == state)
                return;

            if (CharacterState == CharacterState.Hit)
            {
                ReleaseHitDirection();
            }

            _aniControllerSpeed = 1.0f;

            switch (state)
            {
                case CharacterState.Idle:
                    {
                        break;
                    }
                case CharacterState.Run:
                    {
                        _aniControllerSpeed = _characterMoveSpeed;

                        if (_skillEffectController != null)
                        {
                            if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
                            {
                                _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
                            }
                        }

                        //m_aniControllerSpeed *= 0.5f;

                        break;
                    }
                case CharacterState.Attack:
                    {
                        _aniControllerSpeed = _characterAttackSpeed;

                        _skillActiveController.UseSkill(_selectPlaySkillData);

                        if (_skillEffectController != null)
                        {
                            if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
                            {
                                _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
                            }
                        }

                        if (_attackTarget == null)
                        {
                            ChangeCharacterLookAtDirection(Enum_LookDirection.Right);
                        }
                        else
                        {
                            ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
                        }

                        break;
                    }
                case CharacterState.Skill:
                    {
                        _aniControllerSpeed = _characterAttackSpeed;

                        _skillActiveController.UseSkill(_selectPlaySkillData);

                        if (_skillEffectController != null)
                        {
                            if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
                            {
                                _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
                            }
                        }

                        if (_attackTarget == null)
                        {
                            ChangeCharacterLookAtDirection(Enum_LookDirection.Right);
                        }
                        else
                        {
                            ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
                        }

                        break;
                    }
                case CharacterState.Hit:
                    {
                        //CreatureSkillRelease();

                        SetHitRecoveryTime(StaticResource.Instance.GetHitRecoveryTime());

                        break;
                    }
                case CharacterState.Dead:
                    {
                        if (_hitDirectionCoroutine != null)
                        {
                            ReleaseHitDirection();
                        }

                        _creatureDeadTime = Time.time;
                        _deadDirectionTime = StaticResource.Instance.GetDeadDirectionTime();

                        

                        if (_skillEffectController != null)
                            _skillEffectController.ReleaseAllCC();

                        if (_iFFType == IFFType.IFF_Foe)
                        {
                            _deadDirection_Swing = Random.Range(0, 2) == 1;
                            //_deadDirection_Swing = true;

                            if (_deadDirection_Swing == true)
                            {
                                if (_lastHitter != null)
                                {
                                    _swingDirVec.x = _lastHitter.transform.position.x > transform.position.x ? -1 : 1;
                                }
                                else
                                {
                                    _swingDirVec.x = _lookDirection == Enum_LookDirection.Right ? -1 : 1;
                                }

                                _swingDirVec.y = Random.Range(0.3f, 0.6f);
                                _swingDirVec.z = Random.Range(-1.0f, -1.0f);

                                _swingDirVec.Normalize();
                            }
                            else
                            {
                                for (int i = 0; i < _deadParticle.Count; ++i)
                                {
                                    _deadParticle[i].Stop();
                                    _deadParticle[i].Play();
                                }
                            }
                        }

                        ReleaseParticle();
                        _skillActiveController.ReleaseSkill();

                        if (_skillHitReceiver != null)
                            _skillHitReceiver.EnableColliders(false);

                        Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);

                        Managers.BattleSceneManager.Instance.CallDeadCreature(this);

                        break;
                    }
                default:
                    {
                        _aniControllerSpeed = 1.0f;
                        break;
                    }
            }

            SetAniSpeed();

            _characterState = state;

            if (CharacterState == CharacterState.Attack || CharacterState == CharacterState.Skill)
            {
                if (_selectSkillCustomAction == null)
                {
                    if (_selectPlaySkillData.GetAniName() != string.Empty)
                        PlayAnimation(_selectPlaySkillData.GetAniName(), true);
                    else
                        PlayAnimation(CharacterState.Attack);
                }
                else
                    _selectSkillCustomAction.Play();
            }
            else
                PlayAnimation(_characterState);
        }
        //------------------------------------------------------------------------------------
        public void ReadyCreature()
        {
            _characterState = CharacterState.None;
            PlayAnimation(CharacterState.Idle);


            if (_hitDirectionCoroutine != null)
            {
                Debug.LogError("코루틴 안꺼짐");
                ReleaseHitDirection();
            }

            ReleaseParticle();

            SetHP(_maxHP);


            Managers.AggroManager.Instance.AddIFFCharacterAggro(this);
        }
        //------------------------------------------------------------------------------------
        public void PlayCreature()
        {
            if (_skillHitReceiver != null)
                _skillHitReceiver.EnableColliders(true);

            SetNewTarget();

            ChangeState(CharacterState.Run);
        }
        //------------------------------------------------------------------------------------
        public void ForceReleaseCreature()
        {
            _characterState = CharacterState.None;

            if (_hitDirectionCoroutine != null)
            {
                ReleaseHitDirection();
            }

            EnableHitEffect(Color.white);

            _creatureDeadTime = Time.time;
            _deadDirectionTime = StaticResource.Instance.GetDeadDirectionTime();

            for (int i = 0; i < _deadParticle.Count; ++i)
            {
                _deadParticle[i].Stop();
                _deadParticle[i].Play();
            }

            if (_skillEffectController != null)
                _skillEffectController.ReleaseAllCC();

            ReleaseParticle();
            //CreatureSkillRelease();

            if (_skillHitReceiver != null)
                _skillHitReceiver.EnableColliders(false);


            _skillActiveController.ReleaseSkill();
            //CreatureSkillRemove();

            Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);

            ReleaseCharacterBase();
        }
        //------------------------------------------------------------------------------------
        public void ReleaseCreature()
        {
            //CreatureSkillRemove();
            _skillActiveController.ReleaseSkill();
            Managers.BattleSceneManager.Instance.CallReleaseCreature(this);

            ReleaseCharacterBase();
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
            //if (_skillData != null)
            //{
            //    float cooltime = GetSkillCoolTime();
            //    if (cooltime == 0.0f)
            //    {
            //        _uiCharacterState.SetCoolTimeBar(1.0f);
            //    }
            //    else
            //    {
            //        float remaintime = cooltime - (_playSkillTime - Time.time);
            //        float ratio = (remaintime) / cooltime;
            //        _uiCharacterState.SetCoolTimeBar(ratio);
            //    }
            //}

            if (CharacterState != CharacterState.Dead)
            {
                if (CharacterState == CharacterState.Idle || CharacterState == CharacterState.Run)
                {

                    if (_skillEffectController != null)
                    {
                        if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Blind))
                        {
                            _aniControllerSpeed = 1.0f;
                            ChangeState(CharacterState.Idle);
                            return;
                        }
                    }

                    if (_selectPlaySkillData != _skillActiveController.GetReadyActiveSkill())
                    {
                        _selectPlaySkillData = _skillActiveController.GetReadyActiveSkill();

                        ChangeSearchType();
                        SetNewTarget();
                    }

                    if (_attackTarget == null || _attackTarget.IsDead == true)
                    {
                        SetNewTarget();
                    }

                    if (_attackTarget == null || _attackTarget.IsDead == true)
                    {
                        ChangeState(CharacterState.Idle);
                        return;
                    }

                    //if (_skillData != null && _playSkillTime <= Time.time)
                    //{
                    //    if (_skillActionScript == null)
                    //        SetSelectSkillType(_skillData);
                    //    else if (_skillActionScript != null && _skillActionScript.IsReady() == true)
                    //    {
                    //        SetSelectSkillType(_skillData);
                    //    }
                    //    else
                    //        SetSelectSkillType(_attackData);
                    //}
                    //else
                    //    SetSelectSkillType(_attackData);

                    float distance = MathDatas.GetDistance(transform.position.x, transform.position.z, _attackTarget.transform.position.x, _attackTarget.transform.position.z);

                    if (_selectPlaySkillData != null)
                    {
                        if (_selectPlaySkillData.SkillBaseData.TargetCheckType == Enum_TargetCheckType.Friendly)
                        {
                            ChangeState(CharacterState.Skill);
                            return;
                        }

                        if (distance > _selectPlaySkillData.SkillBaseData.AttackRange * GetOutputAttackRange() * (_selectPlaySkillData.SkillBaseData.TargetAttackType == Enum_TargetAttackType.Circle ? 0.5f : 1.0f))
                            ChangeState(CharacterState.Run);
                        else
                        {
                            ChangeState(CharacterState.Attack);

                            //if (_selectPlaySkillData == _attackData)
                            //{
                            //    ChangeState(CharacterState.Attack);
                            //}
                            //else if (_selectPlaySkillData == _skillData)
                            //{
                            //    ChangeState(CharacterState.Skill);
                            //}
                        }
                    }
                    else
                    {
                        //if (distance > _minTraceRange)
                        //    ChangeState(CharacterState.Run);
                        //else
                            ChangeState(CharacterState.Idle);
                    }
                }
                else if (CharacterState == CharacterState.Attack || CharacterState == CharacterState.Skill)
                {
                    //if (m_attackTarget == null || m_attackTarget.IsDead == true)
                    //{
                    //    ChangeState(CharacterState.Idle);
                    //}
                }
                else if (CharacterState == CharacterState.Hit)
                {
                    if (Time.time > _hitRecoveryStartTime + _hitRecoveryTime)
                    {
                        ChangeState(CharacterState.Idle);
                    }
                }
            }
            else
            {
                if (Time.time > _creatureDeadTime + _deadDirectionTime)
                {
                    ReleaseCreature();
                }
                else
                { // 죽었을 때 연출은 여기서 해준다.
                    float ratio = (Time.time - _creatureDeadTime) / _deadDirectionTime;
                    EnableHitEffect(StaticResource.Instance.GetDeadColorGradient().Evaluate(ratio));

                    if (_deadDirection_Swing == true)
                    { 
                        float force = StaticResource.Instance.GetDeadDirectionSwingForce();

                        _rigidbody2D.MovePosition(_rigidbody2D.position + _swingDirVec * force * Time.deltaTime);

                        Vector3 rotate = transform.localEulerAngles;
                        rotate.z += StaticResource.Instance.GetDeadDirectionSwingRotationSpeed() * (_swingDirVec.x > 0 ? 1.0f : -1.0f) * Time.deltaTime;
                        transform.localEulerAngles = rotate;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected override void PlayOnDamageDirection()
        {
            if (_currentHP <= 0)
                ChangeState(CharacterState.Hit);

            SendHP?.Invoke(0, _currentHP, _maxHP);

            for (int i = 0; i < _damageParticle.Count; ++i)
            {
                _damageParticle[i].Stop();
                _damageParticle[i].Play();
            }

            if (CharacterState == CharacterState.None)
                return;

            SetHitRecoveryTime(0.3f);

            if (_hitDirectionCoroutine != null)
                StopCoroutine(_hitDirectionCoroutine);

            _hitDirectionCoroutine = StartCoroutine(HitColorEffect());
        }
        //------------------------------------------------------------------------------------
        public override float GetOutputAttackRange()
        {
            return 1.0f + _randomRange;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}