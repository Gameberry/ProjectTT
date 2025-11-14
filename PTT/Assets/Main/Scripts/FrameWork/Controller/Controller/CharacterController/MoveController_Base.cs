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

                if (_characterControllerBase.MyRigidbody2D != null)
                {

                    Vector3 direction = _characterControllerBase.GetMoveDirection();

                    if (direction.x != 0)
                        _characterControllerBase.ChangeCharacterLookAtDirection(direction.x < 0
                            ? Enum_LookDirection.Left
                            : Enum_LookDirection.Right);

                    if (operMoveSpeed > Define.MaxEffectiveMoveSpeedValue)
                        operMoveSpeed = Define.MaxEffectiveMoveSpeedValue;

                    Vector3 newpos = _characterControllerBase.MyRigidbody2D.position + direction * operMoveSpeed * Time.deltaTime;

                    //float limitLine = Managers.BattleSceneManager.Instance.GetCreatureLimitLine();

                    //if (limitLine > 0.0f)
                    //{
                    //    if (newpos.x > limitLine)
                    //        newpos.x = limitLine;
                    //}

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