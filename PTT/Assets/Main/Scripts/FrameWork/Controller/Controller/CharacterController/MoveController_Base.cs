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

                    Vector2 direction = _characterControllerBase.GetMoveDirection();

                    if (direction.x != 0)
                        _characterControllerBase.ChangeCharacterLookAtDirection(direction.x < 0
                            ? Enum_LookDirection.Left
                            : Enum_LookDirection.Right);

                    if (operMoveSpeed > Define.MaxEffectiveMoveSpeedValue)
                        operMoveSpeed = Define.MaxEffectiveMoveSpeedValue;

                    Vector2 newpos = _characterControllerBase.MyRigidbody2D.position + direction * operMoveSpeed * Time.deltaTime;

                    Vector2 minpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Min;
                    Vector2 maxpos = StaticResource.Instance.GetBattleModeStaticData().MapRange_Max;

                    if (newpos.x < minpos.x)
                        newpos.x = minpos.x;
                    else if (newpos.x > maxpos.x)
                        newpos.x = maxpos.x;

                    if (newpos.y < minpos.y)
                        newpos.y = minpos.y;
                    else if (newpos.y > maxpos.y)
                        newpos.y = maxpos.y;

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