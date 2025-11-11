using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillProjectilAction : MonoBehaviour
    {
        protected Vector3 _targetPosition;
        protected SkillProjectilePlayer _skillProjectilePlayer;
        public System.Action<SkillProjectilAction> _stopCallBack;
        protected SkillManageInfo _skillManageInfo;

        public void SetSkillTarget(Vector3 pos)
        {
            _targetPosition = pos;
        }

        public void SetSkillProjectilePlayer(SkillProjectilePlayer skillProjectilePlayer)
        {
            _skillProjectilePlayer = skillProjectilePlayer;
        }

        public void SetSkillManageInfo(SkillManageInfo skillManageInfo)
        {
            _skillManageInfo = skillManageInfo;
        }

        public void AddStopCallback(System.Action<SkillProjectilAction> action)
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