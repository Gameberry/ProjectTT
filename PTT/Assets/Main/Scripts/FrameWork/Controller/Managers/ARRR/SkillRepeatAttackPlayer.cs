using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    [System.Serializable]
    public class SkillRepeatAttackData
    {
        public int Index;

        public Transform ProjectileRoot;
        public SkillRepeatAttackAction Projectile;

        private Queue<SkillRepeatAttackAction> _projectilePool = new Queue<SkillRepeatAttackAction>();

        public SkillRepeatAttackAction GetParticle()
        {
            if (Projectile == null)
                return null;

            SkillRepeatAttackAction skillProjectilAction;
            if (_projectilePool.Count > 0)
            {
                skillProjectilAction = _projectilePool.Dequeue();
            }
            else
            {
                GameObject clone = Object.Instantiate(Projectile.gameObject, ProjectileRoot);
                clone.transform.localPosition = Projectile.gameObject.transform.localPosition;
                skillProjectilAction = clone.GetComponent<SkillRepeatAttackAction>();

                skillProjectilAction.AddStopCallback(PoolParticle);
            }

            skillProjectilAction.gameObject.SetActive(true);
            skillProjectilAction.transform.position = ProjectileRoot.transform.position;

            skillProjectilAction.transform.SetParent(null);

            return skillProjectilAction;
        }

        private void PoolParticle(SkillRepeatAttackAction skillProjectilAction)
        {
            if (skillProjectilAction == null)
                return;

            skillProjectilAction.transform.SetParent(ProjectileRoot);
            skillProjectilAction.gameObject.SetActive(false);
            _projectilePool.Enqueue(skillProjectilAction);

        }
    }

    public class SkillRepeatAttackPlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillRepeatAttackData> _skillParticleDatas = new List<SkillRepeatAttackData>();

        private Dictionary<int, SkillRepeatAttackData> _skillParticleDatas_Dic = new Dictionary<int, SkillRepeatAttackData>();


        public void PlayProjectile(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            PlayProjectile(skillManageInfo, target.transform.position);
        }

        public void PlayProjectile(SkillManageInfo skillManageInfo, Vector3 pos)
        {
            if (skillManageInfo == null)
                return;

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;
            if (skillBaseData == null)
                return;

            SkillRepeatAttackData skillParticleData = _skillParticleDatas.Find(x => x.Index == skillBaseData.ResourceIndex);

            if (skillParticleData == null)
                return;

            //SkillProjectilAction particleSystem = skillParticleData.Particle;
            SkillRepeatAttackAction particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.SetSkillTarget(pos);
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