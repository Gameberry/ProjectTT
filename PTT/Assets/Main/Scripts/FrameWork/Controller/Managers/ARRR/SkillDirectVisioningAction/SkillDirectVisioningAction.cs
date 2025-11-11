using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillDirectVisioningAction : MonoBehaviour
    {
        [SerializeField]
        protected CharacterControllerBase _targetController;
        protected SkillDirectVisioningPlayer _skillProjectilePlayer;
        public System.Action<SkillDirectVisioningAction> _stopCallBack;
        protected SkillManageInfo _skillManageInfo;

        public virtual void Init()
        { 

        }

        public void SetSkillTarget(CharacterControllerBase characterControllerBase)
        {
            _targetController = characterControllerBase;
        }

        public void SetSkillProjectilePlayer(SkillDirectVisioningPlayer skillProjectilePlayer)
        {
            _skillProjectilePlayer = skillProjectilePlayer;
        }

        public void SetSkillManageInfo(SkillManageInfo skillManageInfo)
        {
            _skillManageInfo = skillManageInfo;
        }

        public void AddStopCallback(System.Action<SkillDirectVisioningAction> action)
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