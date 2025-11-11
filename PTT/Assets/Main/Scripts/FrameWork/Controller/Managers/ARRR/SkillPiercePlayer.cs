using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    [System.Serializable]
    public class SkillPierceData
    {
        public int Index;

        public Transform ProjectileRoot;
        public SkillPierceAction Projectile;

        private Queue<SkillPierceAction> _projectilePool = new Queue<SkillPierceAction>();

        public SkillPierceAction GetParticle()
        {
            if (Projectile == null)
                return null;

            SkillPierceAction skillProjectilAction;
            if (_projectilePool.Count > 0)
            {
                skillProjectilAction = _projectilePool.Dequeue();
            }
            else
            {
                GameObject clone = Object.Instantiate(Projectile.gameObject, ProjectileRoot);
                clone.transform.localPosition = Projectile.gameObject.transform.localPosition;
                skillProjectilAction = clone.GetComponent<SkillPierceAction>();

                skillProjectilAction.AddStopCallback(PoolParticle);
            }

            skillProjectilAction.gameObject.SetActive(true);
            skillProjectilAction.transform.position = ProjectileRoot.transform.position;

            skillProjectilAction.transform.SetParent(null);

            return skillProjectilAction;
        }

        private void PoolParticle(SkillPierceAction skillProjectilAction)
        {
            if (skillProjectilAction == null)
                return;

            skillProjectilAction.transform.SetParent(ProjectileRoot);
            skillProjectilAction.gameObject.SetActive(false);
            _projectilePool.Enqueue(skillProjectilAction);

        }
    }

    public class SkillPiercePlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillPierceData> _skillParticleDatas = new List<SkillPierceData>();

        private Dictionary<int, SkillPierceData> _skillParticleDatas_Dic = new Dictionary<int, SkillPierceData>();

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        public void PlayProjectile(SkillManageInfo skillManageInfo, CharacterControllerBase target)
        {
            if (skillManageInfo.GetRepeatCount() > 1 || skillManageInfo.GetRepeatDelay() > 0)
            {
                bool iscanceled = disableCancellation.IsCancellationRequested;
                if (iscanceled == true)
                    disableCancellation = new CancellationTokenSource();

                PlayNextWaveDelay(skillManageInfo, target.transform.position).Forget();
            }
            else
                PlayProjectile(skillManageInfo, target.transform.position);
        }

        public void PlayProjectile(SkillManageInfo skillManageInfo, Vector3 pos)
        {
            if (skillManageInfo == null)
                return;

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;
            if (skillBaseData == null)
                return;

            SkillPierceData skillParticleData = _skillParticleDatas.Find(x => x.Index == skillBaseData.ResourceIndex);

            if (skillParticleData == null)
                return;

            //SkillProjectilAction particleSystem = skillParticleData.Particle;
            SkillPierceAction particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.SetSkillTarget(pos);
                particleSystem.SetSkillProjectilePlayer(this);
                particleSystem.SetSkillManageInfo(skillManageInfo);
                particleSystem.Play();
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay(SkillManageInfo skillManageInfo, Vector3 pos)
        {
            int RepeatCount = skillManageInfo.GetRepeatCount();
            float Delay = skillManageInfo.GetRepeatDelay();

            for (int i = 0; i < RepeatCount; ++i)
            {
                PlayProjectile(skillManageInfo, pos);
                await UniTask.WaitForSeconds(Delay, false, PlayerLoopTiming.Update, disableCancellation.Token);
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
        //------------------------------------------------------------------------------------
    }
}