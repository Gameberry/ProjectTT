using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class MonsterController : CharacterControllerBase
    {
        public override void Init()
        {
            _characterStatOperator = new CharacterStatOperator(this);
            MoveController_Base creatureBaseMove = gameObject.AddComponent<MoveController_Base>();
            creatureBaseMove.SetCharacterController(this);
            _characterStatOperator.RefreshOutputStatValue();
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
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
            _characterState = state;
        }
    }
}