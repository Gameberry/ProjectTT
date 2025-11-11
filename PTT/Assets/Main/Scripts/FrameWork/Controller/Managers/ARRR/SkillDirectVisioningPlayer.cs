using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class SkillDirectVisioningData
    {
        public int Index;

        public Transform ProjectileRoot;
        public SkillDirectVisioningAction DirectVisioning;

        public bool LinkParent = false;

        private Queue<SkillDirectVisioningAction> _projectilePool = new Queue<SkillDirectVisioningAction>();

        public SkillDirectVisioningAction GetParticle()
        {
            if (DirectVisioning == null)
                return null;

            SkillDirectVisioningAction skillProjectilAction;
            if (_projectilePool.Count > 0)
            {
                skillProjectilAction = _projectilePool.Dequeue();
            }
            else
            {
                GameObject clone = Object.Instantiate(DirectVisioning.gameObject, ProjectileRoot);
                clone.transform.localPosition = DirectVisioning.gameObject.transform.localPosition;
                skillProjectilAction = clone.GetComponent<SkillDirectVisioningAction>();

                skillProjectilAction.Init();
                skillProjectilAction.AddStopCallback(PoolParticle);
            }

            skillProjectilAction.gameObject.SetActive(true);

            if (LinkParent == false)
                skillProjectilAction.transform.SetParent(null);

            return skillProjectilAction;
        }

        private void PoolParticle(SkillDirectVisioningAction skillProjectilAction)
        {
            if (skillProjectilAction == null)
                return;

            skillProjectilAction.transform.SetParent(ProjectileRoot);
            skillProjectilAction.gameObject.SetActive(false);
            _projectilePool.Enqueue(skillProjectilAction);

        }
    }

    public class SkillDirectVisioningPlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillDirectVisioningData> _skillParticleDatas = new List<SkillDirectVisioningData>();

        private Dictionary<int, SkillDirectVisioningData> _skillParticleDatas_Dic = new Dictionary<int, SkillDirectVisioningData>();

        public void PlayDirectVisioning(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            if (skillManageInfo == null)
                return;

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;
            if (skillBaseData == null)
                return;

            SkillDirectVisioningData skillParticleData = _skillParticleDatas.Find(x => x.Index == skillBaseData.ResourceIndex);

            if (skillParticleData == null)
                return;

            //SkillProjectilAction particleSystem = skillParticleData.Particle;
            SkillDirectVisioningAction particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.SetSkillTarget(target);
                particleSystem.SetSkillProjectilePlayer(this);
                particleSystem.SetSkillManageInfo(skillManageInfo);
                particleSystem.Play();
            }
        }

        public void Release()
        {

        }
    }
}