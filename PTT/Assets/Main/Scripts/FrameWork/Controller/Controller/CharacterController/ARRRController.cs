using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class ARRRController : CharacterControllerBase
    {
//        private ARRRInfo _aRRRInfo;
//        public ARRRInfo ARRRInfo { get { return _aRRRInfo; } }

//        private List<Transform> _petTargetPosRoot = new List<Transform>();

//        private float _targetRefreshTimer = 0.0f;
//        [SerializeField]
//        private float _targetRefreshTurm = 0.5f;

//        private bool _attackMotion = false;

//        private string _attackMotion1 = "Attack_01";
//        private string _attackMotion2 = "Attack_02";

//        //------------------------------------------------------------------------------------
//        public override void Init()
//        {
//            _characterStatOperator = new CharacterStatOperator(this);

//            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
//            creatureBaseMove.SetCharacterController(this);

//            //SkillActiveController.SetSkillCoolTimeRate(3);
//        }
//        //------------------------------------------------------------------------------------
//        public override void OnRefreshBuff()
//        {
//            _characterStatOperator.RefreshOutputStatValue();

//            RefreshStat();
//        }
//        //------------------------------------------------------------------------------------
//        public void ReadyARRR()
//        {
//            _characterState = CharacterState.None;
//            PlayAnimation(CharacterState.Idle);


//            if (_hitDirectionCoroutine != null)
//            {
//                Debug.LogError("코루틴 안꺼짐");
//                ReleaseHitDirection();
//            }

//            ReleaseParticle();

//            Managers.AggroManager.Instance.AddIFFCharacterAggro(this);
//        }
//        //------------------------------------------------------------------------------------
//        public void DirectionRunARRR()
//        {
//            ChangeState(CharacterState.Run);
//        }
//        //------------------------------------------------------------------------------------
//        public void PlayCreature()
//        {
//            //if (_skillData != null)
//            //    _playSkillTime = Time.time + GetSkillCoolTime();
//            //else
//            //    _playSkillTime = -1;

//            if (_skillHitReceiver != null)
//                _skillHitReceiver.EnableColliders(true);

//            SetNewTarget();

//            ChangeState(CharacterState.Run);
//        }
//        //------------------------------------------------------------------------------------
//        public void ForceReleaseCreature()
//        {
//            _characterState = CharacterState.None;

//            if (_hitDirectionCoroutine != null)
//            {
//                ReleaseHitDirection();
//            }

//            EnableHitEffect(Color.white);

//            _creatureDeadTime = Time.time;
//            _deadDirectionTime = StaticResource.Instance.GetDeadDirectionTime();

//            if (_skillEffectController != null)
//                _skillEffectController.ReleaseAllCC();

//            ReleaseParticle();

//            if (_skillHitReceiver != null)
//                _skillHitReceiver.EnableColliders(false);

//            Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);

//            ReleaseCreature();
//        }
//        //------------------------------------------------------------------------------------
//        public void ReleaseCreature()
//        {
//            _skillActiveController.ReleaseSkill();

//            foreach (var pair in _petDictionary)
//            {
//                Managers.PetManager.Instance.PoolCreature(pair.Value);
//            }

//            _petDictionary.Clear();
//            _moveController_List.Clear();
//            ARRRSkillInfos.Clear();

//            ReleaseCharacterBase();
//        }
//        //------------------------------------------------------------------------------------
//        public void SetARRRInfo(ARRRInfo aRRRInfo)
//        {
//            _aRRRInfo = aRRRInfo;

//            RefreshARRRStat(_aRRRInfo.DefaultStatValue);

//            _currentSpineModelData = StaticResource.Instance.GetARRRSpineModelData();

//            if (_currentSpineModelData != null)
//            {
//                SetSpineModelData(_currentSpineModelData);

//                ReleaseAttachSkin();

//                foreach (var skinname in _currentSpineModelData.DefaultAttackSkin)
//                {
//                    AddAttachSkin(skinname);
//                }

//                AddAttachSkin(Managers.JobManager.Instance.GetWeaponSkin());

//                //if (_currentSpineModelData.SkinList.Count > aRRRInfo.SkinNumber)
//                //SetSkin(_currentSpineModelData.SkinList[aRRRInfo.SkinNumber]);
//                RefreshAttachSkin();
//            }

//            for (int i = 0; i < aRRRInfo.EquipPetInfo.Count; ++i)
//            {
//                AddPet(aRRRInfo.EquipPetInfo[i]);
//            }

//            for (int i = 0; i < _moveController_List.Count; ++i)
//            {
//                _moveController_List[i].SetRootPosition();
//            }

//            SkillBaseData defaultSkillBaseData = Managers.ARRRSkillManager.Instance.GetBasicAttackSkillData();

//            _defaultSearchType = defaultSkillBaseData.TargetCondition;
//            _minTraceRange = defaultSkillBaseData.AttackRange;

//            _skillActiveController.SetDefaultSkill(defaultSkillBaseData);

//            for (int i = 0; i < aRRRInfo.EquipSkillData.Count; ++i)
//            {
//                SkillInfo skillInfo = aRRRInfo.EquipSkillData[i];
//                SkillBaseData skillBaseData = Managers.ARRRSkillManager.Instance.ConvertSkillInfoToSkillBaseData(skillInfo);

//                _skillActiveController.AddSkillData(skillBaseData, true, skillInfo);

//                ARRRSkillInfos.Add(skillBaseData, skillInfo);
//            }

//            _skillActiveController.ReStartSkill();
//            _selectPlaySkillData = _skillActiveController.GetReadyActiveSkill();

//            ChangeSearchType();

//            RefreshPetPosition();

//            RefreshStat(true);

//            if (Managers.AdBuffManager.Instance.GetActiveBuffKind() == V2Enum_BuffEffectType.IncreaseStat
//                || Define.IsAdBuffAlways == true)
//            {
//                AdBuffActiveData adBuffActiveData = Managers.AdBuffManager.Instance.GetBuffData(V2Enum_BuffEffectType.IncreaseStat);
//                InCreaseBuffStat(V2Enum_Stat.Attack, adBuffActiveData.BuffValue * Define.PerSkillEffectRecoverValue);
//                InCreaseBuffStat(V2Enum_Stat.HP, adBuffActiveData.BuffValue * Define.PerSkillEffectRecoverValue);
//                InCreaseBuffStat(V2Enum_Stat.Defence, adBuffActiveData.BuffValue * Define.PerSkillEffectRecoverValue);
//            }
//        }
//        //------------------------------------------------------------------------------------
//        public void RefreshARRRStat(Dictionary<V2Enum_Stat, ObscuredDouble> DefaultStatValue)
//        {
//            _characterStatOperator.ForceReleaseStat();

//            foreach (var pair in _aRRRInfo.DefaultStatValue)
//            {
//                _characterStatOperator.SetDefaultStat(pair.Key, pair.Value);
//            }

//            _characterStatOperator.RefreshAllStatValue();

//            _characterMoveSpeed = (float)(GetOutPutMyStat(V2Enum_Stat.MoveSpeed));

//            RefreshStat();
//        }
//        //------------------------------------------------------------------------------------
//        private void LateUpdate()
//        {
//            MovePet();
//        }
//        //------------------------------------------------------------------------------------
//        private void AddPet(PetInfo petInfo)
//        {
//            if (petInfo == null)
//                return;

//            PetData petData = Managers.PetManager.Instance.GetPetData(petInfo.Id);
//            if (petData == null)
//                return;

//            if (_petDictionary.ContainsKey(petData) == true)
//                return;


//            PetController petController = Managers.PetManager.Instance.GetPetController();
//            petController.gameObject.SetActive(true);
//            petController.SetCharacterController(this);
//            petController.SetIndex(_petDictionary.Count);
//            petController.SetPetInfo(petInfo);
            
//            _petDictionary.Add(petData, petController);

//            _moveController_List.Add(petController.GetMoveController());

//            if (_petTargetPosRoot.Count < _petDictionary.Count)
//            {
//                int petTargetCount = _petTargetPosRoot.Count;

//                for (int i = petTargetCount; i < _petDictionary.Count; ++i)
//                {
//                    GameObject root = new GameObject();
//                    root.transform.SetParent(transform);
//                    _petTargetPosRoot.Add(root.transform);
//                }
//            }

//            petController.transform.position = transform.position;
//        }
//        //------------------------------------------------------------------------------------
//        public void AddPet(PetData petData, SkillInfo skillInfo = null)
//        {
//            if (_petDictionary.ContainsKey(petData) == true)
//                return;

//            PetController petController = Managers.PetManager.Instance.GetPetController();
//            petController.gameObject.SetActive(true);
//            petController.SetCharacterController(this);
//            petController.SetIndex(_petDictionary.Count);
//            petController.SetPetInfo(petData, skillInfo);

//            _petDictionary.Add(petData, petController);

//            _moveController_List.Add(petController.GetMoveController());

//            if (_petTargetPosRoot.Count < _petDictionary.Count)
//            {
//                int petTargetCount = _petTargetPosRoot.Count;

//                for (int i = petTargetCount; i < _petDictionary.Count; ++i)
//                {
//                    GameObject root = new GameObject();
//                    root.transform.SetParent(transform);
//                    _petTargetPosRoot.Add(root.transform);
//                }
//            }

//            petController.transform.position = transform.position;

//            RefreshPetPosition();
//        }
//        //------------------------------------------------------------------------------------
//        public void AddPetAfterSkill(PetData petData, MainSkillData gambleSkillData, SkillInfo skillInfo = null)
//        {
//            if (_petDictionary.ContainsKey(petData) == false)
//                return;

//            PetController petController = _petDictionary[petData];
//            if (petController != null)
//                petController.AddGambleSkill(gambleSkillData, Enum_SynergyType.Max, skillInfo);
//        }
//        //------------------------------------------------------------------------------------
//        private void RefreshPetPosition()
//        {
//            if (_moveController_List.Count == 0)
//                return;

//            if (_moveController_List.Count == 1)
//            {
//                if (_petTargetPosRoot.Count > 0)
//                {
//                    Vector3 pos = transform.position;
//                    float oper = _lookDirection == Enum_LookDirection.Right ? -1 : 1;

//                    pos.x += StaticResource.Instance.GetBattleModeStaticData().PetRadius * oper;
//                    _petTargetPosRoot[0].transform.position = pos;
//                    _moveController_List[0].SetRoot(_petTargetPosRoot[0]);
//                }
//            }
//            else
//            {
//                CalculateSectorPoints(
//                    StaticResource.Instance.GetBattleModeStaticData().PetRadius,
//                    StaticResource.Instance.GetBattleModeStaticData().PetStartAngle,
//                    StaticResource.Instance.GetBattleModeStaticData().PetSectorAngle);
//            }
//        }
//        //------------------------------------------------------------------------------------
//        public void CalculateSectorPoints(float radius, float startAngle, float sectorAngle)
//        {
//            // 각 점 간의 간격 각도 계산
//            float angleIncrement = sectorAngle / (_moveController_List.Count - 1);

//            for (int i = 0; i < _moveController_List.Count; i++)
//            {
//                // 현재 점의 각도 (라디안 단위로 변환)
//                float currentAngle = startAngle + (angleIncrement * i);
//                float radians = currentAngle * Mathf.Deg2Rad;

//                // 점의 좌표 계산
//                float x = radius * Mathf.Cos(radians);
//                float z = radius * Mathf.Sin(radians);

//                Vector3 pos = transform.position;
//                float oper = _lookDirection == Enum_LookDirection.Right ? -1 : 1;
//                pos.x -= x * oper;
//                pos.z -= z * oper;
//                if (_petTargetPosRoot.Count > i)
//                { 
//                    _petTargetPosRoot[i].transform.position = pos;
//                    _moveController_List[i].SetRoot(_petTargetPosRoot[i]);
//                }
//            }
//        }
//        //------------------------------------------------------------------------------------
//        public void MovePet()
//        {
//            for (int i = 0; i < _moveController_List.Count; i++)
//            {
//                _moveController_List[i].Move();
//            }
//        }
//        //------------------------------------------------------------------------------------
//        public float CoolTimeRate1 = 10.0f;
//        public float CoolTimeRate2 = 20.0f;
//        public float AfterSize = 1.5f;
//        //------------------------------------------------------------------------------------
//        protected override void Updated()
//        {

//#if DEV_DEFINE
//            if (Input.GetKeyUp(KeyCode.P))
//            {
//                AddPet(new PetInfo());
//                RefreshPetPosition();
//            }

//            if (Input.GetKeyUp(KeyCode.O))
//            {
//                RefreshPetPosition();
//            }

//            if (Input.GetKeyUp(KeyCode.J))
//            {
//                SkillActiveController.SetSkillCoolTimeRate(CoolTimeRate1);
//            }

//            if (Input.GetKeyUp(KeyCode.K))
//            {
//                SkillActiveController.SetSkillCoolTimeRate(CoolTimeRate2);
//                SetCreatureSizeControll(AfterSize.ToVector3());
//            }

//            if (Input.GetKey(KeyCode.LeftArrow))
//            {
//                ChangeState(CharacterState.Run);

//                ChangeCharacterLookAtDirection(Enum_LookDirection.Left);

//                Vector3 pos = transform.position;
//                pos.x += MyCharacterMoveSpeed * Time.deltaTime * -1.0f;
//                transform.position = pos;

//                return;
//            }
//            else if (Input.GetKey(KeyCode.RightArrow))
//            {
//                ChangeState(CharacterState.Run);

//                ChangeCharacterLookAtDirection(Enum_LookDirection.Right);

//                Vector3 pos = transform.position;
//                pos.x += MyCharacterMoveSpeed * Time.deltaTime * 1.0f;
//                transform.position = pos;

//                return;
//            }

//            if (Input.GetKey(KeyCode.UpArrow))
//            {
//                SetHP(MaxHP * 0.4);

//                return;
//            }
//            else if (Input.GetKey(KeyCode.DownArrow))
//            {
//                SetHP(MaxHP);

//                return;
//            }
//#endif

//            if (CharacterState != CharacterState.Dead)
//            {
//                if (CharacterState == CharacterState.Idle || CharacterState == CharacterState.Run)
//                {

//                    if (_skillEffectController != null)
//                    {
//                        if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Blind))
//                        {
//                            _aniControllerSpeed = 1.0f;
//                            ChangeState(CharacterState.Idle);
//                            return;
//                        }
//                    }

//                    if (_selectPlaySkillData != _skillActiveController.GetReadyActiveSkill())
//                    {
//                        _selectPlaySkillData = _skillActiveController.GetReadyActiveSkill();

//                        ChangeSearchType();
//                        SetNewTarget();
//                    }

//                    if (_targetRefreshTimer > Time.time)
//                    { 
//                        SetNewTarget();
//                        _targetRefreshTimer = Time.time + _targetRefreshTurm;
//                    }

//                    if (_attackTarget == null || _attackTarget.IsDead == true)
//                    {
//                        SetNewTarget();
//                        _targetRefreshTimer = Time.time + _targetRefreshTurm;
//                    }

//                    if (_attackTarget == null || _attackTarget.IsDead == true)
//                    {
//                        if (Managers.BattleSceneManager.Instance.BattleType == Enum_Dungeon.LobbyScene)
//                            ChangeState(CharacterState.Idle);
//                        else
//                            ChangeState(CharacterState.Run);
//                        return;
//                    }

//                    //if (_skillData != null && _playSkillTime <= Time.time)
//                    //{
//                    //    if (_skillActionScript == null)
//                    //        SetSelectSkillType(_skillData);
//                    //    else if (_skillActionScript != null && _skillActionScript.IsReady() == true)
//                    //    {
//                    //        SetSelectSkillType(_skillData);
//                    //    }
//                    //    else
//                    //        SetSelectSkillType(_attackData);
//                    //}
//                    //else
//                    //    SetSelectSkillType(_attackData);

//                    float distance = MathDatas.GetDistance(transform.position.x, transform.position.z, _attackTarget.transform.position.x, _attackTarget.transform.position.z);

//                    if (_selectPlaySkillData != null)
//                    {
//                        if (_selectPlaySkillData.SkillBaseData.TargetCheckType == Enum_TargetCheckType.Friendly)
//                        {
//                            ChangeState(CharacterState.Skill);
//                            return;
//                        }

//                        if (distance > _selectPlaySkillData.SkillBaseData.AttackRange * GetOutputAttackRange() * (_selectPlaySkillData.SkillBaseData.TargetAttackType == Enum_TargetAttackType.Circle ? 0.5f : 1.0f))
//                            ChangeState(CharacterState.Run);
//                        else
//                        {
//                            if (_selectPlaySkillData.SkillBaseData.TriggerType == Enum_TriggerType.Default)
//                                ChangeState(CharacterState.Attack);
//                            else if (_selectPlaySkillData.SkillBaseData.TriggerType == Enum_TriggerType.Active)
//                                ChangeState(CharacterState.Skill);

//                            //if (_selectPlaySkillData == _attackData)
//                            //{
//                            //    ChangeState(CharacterState.Attack);
//                            //}
//                            //else if (_selectPlaySkillData == _skillData)
//                            //{
//                            //    ChangeState(CharacterState.Skill);
//                            //}
//                        }
//                    }
//                    else
//                    {
//                        if (distance > _minTraceRange)
//                            ChangeState(CharacterState.Run);
//                        else
//                            ChangeState(CharacterState.Idle);
//                    }
                    
//                }
//                else if (CharacterState == CharacterState.Attack || CharacterState == CharacterState.Skill)
//                {
//                    //if (m_attackTarget == null || m_attackTarget.IsDead == true)
//                    //{
//                    //    ChangeState(CharacterState.Idle);
//                    //}
//                }
//                else if (CharacterState == CharacterState.Hit)
//                {
//                    if (Time.time > _hitRecoveryStartTime + _hitRecoveryTime)
//                    {
//                        ChangeState(CharacterState.Idle);
//                    }
//                }
//            }
//            else
//            {
//                if (Time.time > _creatureDeadTime + _deadDirectionTime)
//                {
//                    Managers.BattleSceneManager.Instance.CallReleaseARRR(this);
//                    ReleaseCreature();
//                }
//                else
//                { // 죽었을 때 연출은 여기서 해준다.
//                    float ratio = (Time.time - _creatureDeadTime) / _deadDirectionTime;
//                    EnableHitEffect(StaticResource.Instance.GetDeadColorGradient().Evaluate(ratio));
//                }
//            }
//        }
//        //------------------------------------------------------------------------------------
//        public void SetAniSpeed()
//        {
//            _mySkeletonAnimationHandler.SetAnimationSpeed(_aniControllerSpeed);
//        }
//        //------------------------------------------------------------------------------------
//        protected override void ChangeState(CharacterState state)
//        {
//            if (CharacterState == state)
//                return;


//            if (CharacterState == CharacterState.Hit)
//            {
//                ReleaseHitDirection();
//            }

//            _aniControllerSpeed = 1.0f;

//            if (_selectSkillCustomAction != null)
//            {
//                _selectSkillCustomAction.Release();
//                _selectSkillCustomAction = null;
//            }

//            switch (state)
//            {
//                case CharacterState.Idle:
//                    {
//                        break;
//                    }
//                case CharacterState.Run:
//                    {
//                        _aniControllerSpeed = _characterMoveSpeed;

//                        if (_skillEffectController != null)
//                        {
//                            if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
//                            {
//                                _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
//                            }
//                        }

//                        //m_aniControllerSpeed *= 0.5f;
//                        _attackMotion = false;

//                        break;
//                    }
//                case CharacterState.Attack:
//                    {
//                        _aniControllerSpeed = _characterAttackSpeed;

//                        _skillActiveController.UseSkill(_selectPlaySkillData);

//                        if (_skillEffectController != null)
//                        {
//                            if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
//                            {
//                                _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
//                            }
//                        }

//                        if (_attackTarget == null)
//                        {
//                            ChangeCharacterLookAtDirection(Enum_LookDirection.Right);
//                        }
//                        else
//                        {
//                            ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
//                        }

//                        break;
//                    }
//                case CharacterState.Skill:
//                    {
//                        _aniControllerSpeed = _characterAttackSpeed;

//                        _skillActiveController.UseSkill(_selectPlaySkillData);

//                        if (_skillEffectController != null)
//                        {
//                            if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
//                            {
//                                _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
//                            }
//                        }

//                        if (_attackTarget == null)
//                        {
//                            ChangeCharacterLookAtDirection(Enum_LookDirection.Right);
//                        }
//                        else
//                        {
//                            ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
//                        }

//                        if (_skillCustomActionPlayer != null)
//                            _selectSkillCustomAction = _skillCustomActionPlayer.PlayAction(_selectPlaySkillData);

//                        //_playSkillTime = Time.time + GetSkillCoolTime();

//                        break;
//                    }
//                case CharacterState.Hit:
//                    {
//                        SetHitRecoveryTime(StaticResource.Instance.GetHitRecoveryTime());

//                        break;
//                    }
//                case CharacterState.Dead:
//                    {
//                        if (_hitDirectionCoroutine != null)
//                        {
//                            ReleaseHitDirection();
//                        }

//                        _creatureDeadTime = Time.time;
//                        _deadDirectionTime = StaticResource.Instance.GetDeadDirectionTime();

//                        //for (int i = 0; i < _deadParticle.Count; ++i)
//                        //{
//                        //    _deadParticle[i].Stop();
//                        //    _deadParticle[i].Play();
//                        //}

//                        if (_skillEffectController != null)
//                            _skillEffectController.ReleaseAllCC();

                        

//                        ReleaseParticle();
//                        _skillActiveController.ReleaseSkill();

//                        if (_skillHitReceiver != null)
//                            _skillHitReceiver.EnableColliders(false);

//                        Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);

//                        Managers.BattleSceneManager.Instance.CallDeadARRR(this);

//                        break;
//                    }
//                default:
//                    {
//                        _aniControllerSpeed = 1.0f;
//                        break;
//                    }
//            }

//            SetAniSpeed();

//            _characterState = state;

//            if (CharacterState == CharacterState.Dead)
//                PlayAnimation(CharacterState.Hit);
//            else if (CharacterState == CharacterState.Attack)
//            {
//                PlayAnimation(GetAttackAnimation(), false);
//                _attackMotion = !_attackMotion;
//            }
//            else if (CharacterState == CharacterState.Skill)
//            {
//                if (_selectSkillCustomAction == null)
//                {
//                    if (_selectPlaySkillData.GetAniName() != string.Empty)
//                        PlayAnimation(_selectPlaySkillData.GetAniName(), true);
//                    else
//                        PlayAnimation(_attackMotion1, true);
//                }
//                else
//                    _selectSkillCustomAction.Play();
//            }
//            else
//                PlayAnimation(_characterState);

//        }
//        //------------------------------------------------------------------------------------
//        private string GetAttackAnimation()
//        {
//            return _attackMotion == false ? _attackMotion1 : _attackMotion2;
//        }
//        //------------------------------------------------------------------------------------
//        protected override void SpineAnimationEvent(string aniName, string eventName)
//        {
//            if (aniName.Contains("Attack") || aniName.Contains("Skill"))
//            {
//                if (eventName == "Start")
//                    AniActionCallBack(AnimationAction.AniStart);
//                else if (eventName == "AniAction")
//                    AniActionCallBack(AnimationAction.AniAction);
//                else if (eventName == "End")
//                    AniActionCallBack(AnimationAction.AniEnd);
//            }
//        }
//        //------------------------------------------------------------------------------------
//        private void AniActionCallBack(AnimationAction aniaction)
//        {
//            if (CharacterState == CharacterState.Attack || CharacterState == CharacterState.Skill)
//            {
//                if (_selectSkillCustomAction != null)
//                { 
//                    _selectSkillCustomAction.AniActionCallBack(aniaction);
//                    return;
//                }

//                //CreatureSkillActionBase m_nextPlaySkillActionScript = CharacterState == CharacterState.Skill ? _skillActionScript : _attackActionScript;

//                    //if (m_nextPlaySkillActionScript != null)
//                    //{
//                    //    if (aniaction == AnimationAction.AniStart
//                    //        || aniaction == AnimationAction.AniStartAndAction)
//                    //    {
//                    //        V2SkillAttackData damageData = Managers.CreatureManager.Instance.GetCreatureSkillAttackData(_selectPlaySkillData, this);
//                    //        m_nextPlaySkillActionScript.SetSkillAttackData(damageData);

//                    //        //if (mCharacterState == CharacterState.Skill)
//                    //        //    Managers.PlayerManager.Instance.PlayInGameBGFade(m_AllyCameraFade);
//                    //    }

//                    //    m_nextPlaySkillActionScript.AniActionCallBack(aniaction);

//                    //    if (aniaction == AnimationAction.AniEnd)
//                    //    {
//                    //        SetNewTarget();

//                    //        ChangeState(CharacterState.Run);
//                    //        Updated();
//                    //        return;
//                    //    }
//                    //    return;
//                    //}
//                    //else
//                {
//                    if (aniaction == AnimationAction.AniStartAndAction
//                            || aniaction == AnimationAction.AniAction)
//                    {
//                        PlaySkill(_selectPlaySkillData, AttackTarget);
//                    }

//                    if (aniaction == AnimationAction.AniEnd)
//                    {
//                        SetNewTarget();

//                        _characterState = CharacterState.Idle;
//                        Updated();
//                        return;
//                    }
//                }
//            }
//        }
        //------------------------------------------------------------------------------------
    }
}