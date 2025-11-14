using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    [System.Serializable]
    public class SkillProjectileData
    {
        public int Index;

        public Transform ProjectileRoot;
        public SkillProjectilAction Projectile;

        private Queue<SkillProjectilAction> _projectilePool = new Queue<SkillProjectilAction>();

        public SkillProjectilAction GetParticle()
        {
            if (Projectile == null)
                return null;

            SkillProjectilAction skillProjectilAction;
            if (_projectilePool.Count > 0)
            {
                skillProjectilAction = _projectilePool.Dequeue();
            }
            else
            {
                GameObject clone = Object.Instantiate(Projectile.gameObject, ProjectileRoot);
                clone.transform.localPosition = Projectile.gameObject.transform.localPosition;
                skillProjectilAction = clone.GetComponent<SkillProjectilAction>();

                skillProjectilAction.AddStopCallback(PoolParticle);
            }

            skillProjectilAction.gameObject.SetActive(true);
            skillProjectilAction.transform.position = ProjectileRoot.transform.position;

            skillProjectilAction.transform.SetParent(null);

            return skillProjectilAction;
        }

        private void PoolParticle(SkillProjectilAction skillProjectilAction)
        {
            if (skillProjectilAction == null)
                return;

            skillProjectilAction.transform.SetParent(ProjectileRoot);
            skillProjectilAction.gameObject.SetActive(false);
            _projectilePool.Enqueue(skillProjectilAction);

        }
    }

    public class SkillProjectilePlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillProjectileData> _skillParticleDatas = new List<SkillProjectileData>();

        private Dictionary<int, SkillProjectileData> _skillParticleDatas_Dic = new Dictionary<int, SkillProjectileData>();

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        public void PlayProjectile(AttackData damage, CharacterControllerBase target)
        {
            PlayProjectile(damage, target.transform.position);
        }

        public void PlayProjectile(AttackData attackData, Vector3 pos)
        {
            SkillProjectileData skillParticleData = _skillParticleDatas.Find(x => x.Index == attackData.ResourceIndex);

            if (skillParticleData == null)
                return;

            //SkillProjectilAction particleSystem = skillParticleData.Particle;
            SkillProjectilAction particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.SetSkillTarget(pos);
                particleSystem.SetSkillProjectilePlayer(this);
                particleSystem.SetSkillManageInfo(attackData);
                particleSystem.Play();
            }
        }
        //------------------------------------------------------------------------------------
        public void Release()
        {
            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == false)
            {
                disableCancellation.Cancel();
                disableCancellation.Dispose();
            }
        }
    }
}