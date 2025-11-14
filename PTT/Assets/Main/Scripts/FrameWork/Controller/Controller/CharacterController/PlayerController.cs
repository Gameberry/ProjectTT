using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    // 임시 네이밍 클래스. 클래스 이름 바꿀 수 있다면 바꾸자
    public class PlayerController : CharacterControllerBase
    {
        // 조이스틱 넣기 전에 임시 변수
        private bool _useCustomDirVec = false;

        private Vector3 _customDieVec = Vector3.zero;
        // 조이스틱 넣기 전에 임시 변수
        //------------------------------------------------------------------------------------
        public override void Init()
        {
            _characterStatOperator = new CharacterStatOperator(this);
            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
            _characterStatOperator.RefreshOutputStatValue();
        }
        //------------------------------------------------------------------------------------
        public override Vector3 GetMoveDirection()
        { // MoveController_Base에서 주로 호출
            // 유저는 조이스틱으로 방향을 정할 때가 있어서 가상함수로 만듬

            if (_useCustomDirVec == true)
                return _customDieVec.normalized;

            return base.GetMoveDirection();
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {

#if DEV_DEFINE
            _useCustomDirVec = false;
            _customDieVec = Vector3.zero;

            

            if (Input.GetKey(KeyCode.W))
            {
                _useCustomDirVec = true;
                _customDieVec.z = 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                _useCustomDirVec = true;
                _customDieVec.z = -1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                _useCustomDirVec = true;
                _customDieVec.x = 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                _useCustomDirVec = true;
                _customDieVec.x = -1;
            }
#endif

            if (CharacterState != CharacterState.Dead)
            {
                if (CharacterState == CharacterState.Idle || CharacterState == CharacterState.Run)
                {
                    if (_attackTarget == null || _attackTarget.IsDead == true)
                    {
                        SetNewTarget();
                    }

                    if (_attackTarget != null)
                    {
                        ChangeState(CharacterState.Run);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected override void ChangeState(CharacterState state)
        {
            //if (CharacterState == state)
            //    return;


            //if (CharacterState == CharacterState.Hit)
            //{
            //    ReleaseHitDirection();
            //}

            //_aniControllerSpeed = 1.0f;

            //if (_selectSkillCustomAction != null)
            //{
            //    _selectSkillCustomAction.Release();
            //    _selectSkillCustomAction = null;
            //}

            //switch (state)
            //{
            //    case CharacterState.Idle:
            //        {
            //            break;
            //        }
            //    case CharacterState.Run:
            //        {
            //            _aniControllerSpeed = _characterMoveSpeed;
            //            break;
            //        }
            //    case CharacterState.Attack:
            //        {
            //            _aniControllerSpeed = _characterAttackSpeed;

            //            _skillActiveController.UseSkill(_selectPlaySkillData);

            //            if (_skillEffectController != null)
            //            {
            //                if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
            //                {
            //                    _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
            //                }
            //            }

            //            if (_attackTarget == null)
            //            {
            //                ChangeCharacterLookAtDirection(Enum_LookDirection.Right);
            //            }
            //            else
            //            {
            //                ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
            //            }

            //            break;
            //        }
            //    case CharacterState.Skill:
            //        {
            //            _aniControllerSpeed = _characterAttackSpeed;

            //            _skillActiveController.UseSkill(_selectPlaySkillData);

            //            if (_skillEffectController != null)
            //            {
            //                if (_skillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
            //                {
            //                    _aniControllerSpeed = (1.0f - (float)(_skillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));
            //                }
            //            }

            //            if (_attackTarget == null)
            //            {
            //                ChangeCharacterLookAtDirection(Enum_LookDirection.Right);
            //            }
            //            else
            //            {
            //                ChangeCharacterLookAtDirection_Target(_attackTarget.transform);
            //            }

            //            if (_skillCustomActionPlayer != null)
            //                _selectSkillCustomAction = _skillCustomActionPlayer.PlayAction(_selectPlaySkillData);

            //            //_playSkillTime = Time.time + GetSkillCoolTime();

            //            break;
            //        }
            //    case CharacterState.Hit:
            //        {
            //            break;
            //        }
            //    case CharacterState.Dead:
            //        {
            //            if (_hitDirectionCoroutine != null)
            //            {
            //                ReleaseHitDirection();
            //            }

            //            _creatureDeadTime = Time.time;
            //            _deadDirectionTime = StaticResource.Instance.GetDeadDirectionTime();

            //            //for (int i = 0; i < _deadParticle.Count; ++i)
            //            //{
            //            //    _deadParticle[i].Stop();
            //            //    _deadParticle[i].Play();
            //            //}

            //            if (_skillEffectController != null)
            //                _skillEffectController.ReleaseAllCC();



            //            ReleaseParticle();
            //            _skillActiveController.ReleaseSkill();

            //            if (_skillHitReceiver != null)
            //                _skillHitReceiver.EnableColliders(false);

            //            Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);

            //            Managers.BattleSceneManager.Instance.CallDeadARRR(this);

            //            break;
            //        }
            //    default:
            //        {
            //            _aniControllerSpeed = 1.0f;
            //            break;
            //        }
            //}

            //SetAniSpeed();

            _characterState = state;

            //if (CharacterState == CharacterState.Dead)
            //    PlayAnimation(CharacterState.Hit);
            //else if (CharacterState == CharacterState.Attack)
            //{
            //    PlayAnimation(GetAttackAnimation(), false);
            //    _attackMotion = !_attackMotion;
            //}
            //else if (CharacterState == CharacterState.Skill)
            //{
            //    if (_selectSkillCustomAction == null)
            //    {
            //        if (_selectPlaySkillData.GetAniName() != string.Empty)
            //            PlayAnimation(_selectPlaySkillData.GetAniName(), true);
            //        else
            //            PlayAnimation(_attackMotion1, true);
            //    }
            //    else
            //        _selectSkillCustomAction.Play();
            //}
            //else
            //    PlayAnimation(_characterState);

        }
        //------------------------------------------------------------------------------------

    }
}