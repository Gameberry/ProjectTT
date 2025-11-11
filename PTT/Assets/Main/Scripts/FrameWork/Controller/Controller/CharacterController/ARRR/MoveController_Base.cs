using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class MoveController_Base : MonoBehaviour, ICharacterMove
    {
        private CharacterControllerBase _characterControllerBase;

        public void SetCharacterController(CharacterControllerBase characterControllerBase)
        {
            _characterControllerBase = characterControllerBase;
        }

        public void Move()
        {
            if (_characterControllerBase.CharacterState == CharacterState.Run)
            {
                float operMoveSpeed = _characterControllerBase.MyCharacterMoveSpeed;

                if (_characterControllerBase.MySkillEffectController != null)
                {
                    if (_characterControllerBase.MySkillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Snare))
                    {
                        operMoveSpeed = 0.0f;
                        _characterControllerBase.AniControllerSpeed = 1.0f;
                        //PlayAnimation(CharacterState.Idle);
                    }
                    else if (_characterControllerBase.MySkillEffectController.IsAppliedCC(V2Enum_SkillEffectType.Slow))
                    {
                        operMoveSpeed = _characterControllerBase.AniControllerSpeed * (1.0f - (float)(_characterControllerBase.MySkillEffectController.GetCCValue(V2Enum_SkillEffectType.Slow) * Define.PercentageRecoverValue));

                        _characterControllerBase.AniControllerSpeed = operMoveSpeed;
                    }
                }

                if (_characterControllerBase.MyRigidbody2D != null)
                {

                    Vector3 direction = Vector3.zero;
                    if (_characterControllerBase.AttackTarget == null || _characterControllerBase.AttackTarget.IsDead == true)
                        direction = _characterControllerBase.IFFType == IFFType.IFF_Friend ? Vector3.right : Vector3.left;
                    else
                        direction = _characterControllerBase.AttackTarget.transform.position - transform.position;

                    direction.Normalize();

                    //direction.y *= 0.5f;

                    //direction.Normalize();

                    _characterControllerBase.ChangeCharacterLookAtDirection(direction.x < 0 ? Enum_ARR_LookDirection.Left : Enum_ARR_LookDirection.Right);

                    if (operMoveSpeed > Define.MaxEffectiveMoveSpeedValue)
                        operMoveSpeed = Define.MaxEffectiveMoveSpeedValue;

                    Vector3 newpos = _characterControllerBase.MyRigidbody2D.position + direction * operMoveSpeed * Time.deltaTime;

                    float limitLine = Managers.BattleSceneManager.Instance.GetCreatureLimitLine();

                    if (limitLine > 0.0f)
                    {
                        if (newpos.x > limitLine)
                            newpos.x = limitLine;
                    }

                    _characterControllerBase.MyRigidbody2D.MovePosition(newpos);
                }
            }
        }

        private void FixedUpdate()
        {
            Move();
        }
    }
}