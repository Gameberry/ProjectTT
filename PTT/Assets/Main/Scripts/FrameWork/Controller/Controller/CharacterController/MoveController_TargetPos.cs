using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class MoveController_TargetPos : ICharacterMove
    {
        private CharacterControllerBase _myController;

        private Transform _root;

        private Vector3 _goalPos;

        public void SetCharacterController(CharacterControllerBase characterControllerBase)
        {
            _myController = characterControllerBase;
        }

        public void GoalPos(Vector3 goalPos)
        {
            _goalPos = goalPos;
        }

        public void SetRoot(Transform root)
        {
            _root = root;
        }

        public void SetRootPosition()
        {
            if (_root != null)
                _myController.transform.position = _root.position;
        }

        public void Move()
        {
            if (_root == null)
                return;

            Vector3 posGap = _root.transform.position - _myController.transform.position;

            _myController.transform.position += posGap * Time.deltaTime * _myController.MyCharacterMoveSpeed * StaticResource.Instance.GetBattleModeStaticData().PetMoveSpeed;
        }
    }
}