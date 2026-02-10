using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    [System.Serializable]
    public class SkillObjData
    {
        public int Index;

        public Transform ProjectileRoot;
        public SkillAction Projectile;

        private Queue<SkillAction> _projectilePool = new Queue<SkillAction>();

        public SkillAction GetParticle()
        {
            if (Projectile == null)
                return null;

            SkillAction skillProjectilAction;
            if (_projectilePool.Count > 0)
            {
                skillProjectilAction = _projectilePool.Dequeue();
            }
            else
            {
                GameObject clone = Object.Instantiate(Projectile.gameObject, ProjectileRoot);
                clone.transform.localPosition = Projectile.gameObject.transform.localPosition;
                skillProjectilAction = clone.GetComponent<SkillAction>();

                skillProjectilAction.AddStopCallback(PoolParticle);
            }

            skillProjectilAction.gameObject.SetActive(true);
            skillProjectilAction.transform.position = ProjectileRoot.transform.position;

            skillProjectilAction.transform.SetParent(null);

            return skillProjectilAction;
        }

        private void PoolParticle(SkillAction skillProjectilAction)
        {
            if (skillProjectilAction == null)
                return;

            skillProjectilAction.transform.SetParent(ProjectileRoot);
            skillProjectilAction.gameObject.SetActive(false);
            _projectilePool.Enqueue(skillProjectilAction);

        }
    }

    public class SkillPlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillObjData> _skillParticleDatas = new List<SkillObjData>();

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        public void PlaySkill(AttackStruct attackData, CharacterControllerBase target)
        {
            SkillObjData skillParticleData = _skillParticleDatas.Find(x => x.Index == attackData.SkillInfo.ResourceIndex);

            if (skillParticleData == null)
                return;

            //SkillProjectilAction particleSystem = skillParticleData.Particle;
            SkillAction particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.SetSkillTarget(target.transform.position);
                particleSystem.SetSkillTarget(target);
                particleSystem.SetSkillProjectilePlayer(this);
                particleSystem.SetSkillManageInfo(attackData);
                particleSystem.Play();
            }
        }

        public void PlaySkill(AttackStruct attackData, Vector3 pos)
        {
            SkillObjData skillParticleData = _skillParticleDatas.Find(x => x.Index == attackData.SkillInfo.ResourceIndex);

            if (skillParticleData == null)
                return;

            //SkillProjectilAction particleSystem = skillParticleData.Particle;
            SkillAction particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.SetSkillTarget(pos);
                particleSystem.SetSkillTarget(null);
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