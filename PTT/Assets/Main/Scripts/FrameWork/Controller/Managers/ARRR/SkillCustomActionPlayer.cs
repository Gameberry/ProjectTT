using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class SkillCustomActionData
    {
        public int Index;
        public SkillCustomAction SkillCustomAction;

        public SkillCustomAction GetSkillCustomAction()
        {
            return SkillCustomAction;
        }
    }

    public class SkillCustomActionPlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillCustomActionData> _skillParticleDatas = new List<SkillCustomActionData>();

        public SkillCustomAction PlayAction(SkillManageInfo skillManageInfo)
        {
            if (skillManageInfo == null)
                return null;

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;
            if (skillBaseData == null)
                return null;

            SkillCustomActionData skillParticleData = _skillParticleDatas.Find(x => x.Index == skillBaseData.ResourceIndex);
            if (skillParticleData == null)
                return null;

            SkillCustomAction skillCustomAction = skillParticleData.GetSkillCustomAction();
            skillCustomAction.SetSkillCustomActionPlayer(this);
            skillCustomAction.SetSkillManageInfo(skillManageInfo);

            return skillCustomAction;
        }

        public void Release()
        { 

        }
    }
}