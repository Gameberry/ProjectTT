using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillCustomAction : MonoBehaviour
    {
        protected SkillCustomActionPlayer _skillCustomActionPlayer;
        protected SkillManageInfo _skillManageInfo;

        public void SetSkillCustomActionPlayer(SkillCustomActionPlayer skillCustomActionPlayer)
        {
            _skillCustomActionPlayer = skillCustomActionPlayer;
        }

        public void SetSkillManageInfo(SkillManageInfo skillManageInfo)
        {
            _skillManageInfo = skillManageInfo;
        }

        public virtual void Play()
        { 

        }

        public virtual void AniActionCallBack(AnimationAction aniaction)
        { 

        }

        public virtual void Release()
        {
        }
    }
}