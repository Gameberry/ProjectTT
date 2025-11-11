using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public interface ICharacterMove
    {
        void SetCharacterController(CharacterControllerBase characterControllerBase);

        void Move();
    }
}