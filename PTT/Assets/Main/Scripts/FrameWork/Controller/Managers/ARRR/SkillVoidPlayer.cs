using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    [System.Serializable]
    public class SkillVoidData
    {
        public int Index;

        public Transform ProjectileRoot;
        public SkillVoidAction Projectile;

        private Queue<SkillVoidAction> _projectilePool = new Queue<SkillVoidAction>();

        public SkillVoidAction GetParticle()
        {
            if (Projectile == null)
                return null;

            SkillVoidAction skillProjectilAction;
            if (_projectilePool.Count > 0)
            {
                skillProjectilAction = _projectilePool.Dequeue();
            }
            else
            {
                GameObject clone = Object.Instantiate(Projectile.gameObject, ProjectileRoot);
                clone.transform.localPosition = Projectile.gameObject.transform.localPosition;
                skillProjectilAction = clone.GetComponent<SkillVoidAction>();

                skillProjectilAction.AddStopCallback(PoolParticle);
            }

            skillProjectilAction.gameObject.SetActive(true);
            skillProjectilAction.transform.position = ProjectileRoot.transform.position;

            skillProjectilAction.transform.SetParent(null);

            return skillProjectilAction;
        }

        private void PoolParticle(SkillVoidAction skillProjectilAction)
        {
            if (skillProjectilAction == null)
                return;

            skillProjectilAction.transform.SetParent(ProjectileRoot);
            skillProjectilAction.gameObject.SetActive(false);
            _projectilePool.Enqueue(skillProjectilAction);

        }
    }

    public class SkillVoidPlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillVoidData> _skillParticleDatas = new List<SkillVoidData>();

        private Dictionary<int, SkillVoidData> _skillParticleDatas_Dic = new Dictionary<int, SkillVoidData>();

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

            SkillVoidData skillParticleData = _skillParticleDatas.Find(x => x.Index == skillBaseData.ResourceIndex);

            if (skillParticleData == null)
                return;

            //SkillProjectilAction particleSystem = skillParticleData.Particle;
            SkillVoidAction particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.SetSkillTarget(pos);
                particleSystem.SetSkillProjectilePlayer(this);
                particleSystem.SetSkillManageInfo(skillManageInfo);
                particleSystem.Play();
            }
        }
        //------------------------------------------------------------------------------------
        public void Release()
        {

        }
        //------------------------------------------------------------------------------------
    }
}