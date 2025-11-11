using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillHitReceiver : MonoBehaviour
    {
        [SerializeField]
        private CharacterControllerBase _characterController;

        public  CharacterControllerBase CharacterController { get { return _characterController; } }

        [SerializeField]
        private Collider _characterCollider;

        public Collider CharacterCollider { get { return _characterCollider; } }

        //------------------------------------------------------------------------------------
        public void RecvHitData(V2SkillAttackData damage)
        {
            if (_characterController != null)
            {
                _characterController.OnDamage(damage);
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableColliders(bool enable)
        {
            if (_characterCollider != null)
                _characterCollider.enabled = enable;
        }
        //------------------------------------------------------------------------------------
    }
}