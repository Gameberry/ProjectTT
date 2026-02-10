using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillAction : MonoBehaviour
    {
        protected Vector3 _targetPosition;
        protected SkillPlayer _skillProjectilePlayer;
        public System.Action<SkillAction> _stopCallBack;
        protected AttackStruct _attackData;
        protected CharacterControllerBase _target;

        public void SetSkillTarget(Vector3 pos)
        {
            _targetPosition = pos;
        }

        public void SetSkillProjectilePlayer(SkillPlayer skillProjectilePlayer)
        {
            _skillProjectilePlayer = skillProjectilePlayer;
        }

        public void SetSkillTarget(CharacterControllerBase characterControllerBase)
        {
            _target = characterControllerBase;
        }


        public void SetSkillManageInfo(AttackStruct attackData)
        {
            _attackData = attackData;
        }

        public void AddStopCallback(System.Action<SkillAction> action)
        {
            _stopCallBack = action;
        }

        public virtual void Play()
        {

        }

        public virtual void Release()
        {
            _stopCallBack?.Invoke(this);
        }
    }
}