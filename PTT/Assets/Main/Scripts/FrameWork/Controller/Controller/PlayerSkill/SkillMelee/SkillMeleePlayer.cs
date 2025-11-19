using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    [System.Serializable]
    public class SkillMeleeData
    {
        public int Index;
        public ParticleSystem particle;
    }

    public class SkillMeleePlayer : MonoBehaviour
    {
        public CharacterControllerBase CharacterControllerBase;

        [SerializeField]
        private List<SkillMeleeData> _skillParticleDatas = new List<SkillMeleeData>();

        private Dictionary<int, SkillMeleeData> _skillParticleDatas_Dic = new Dictionary<int, SkillMeleeData>();

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        public void PlaySkill(AttackData attackData, CharacterControllerBase target)
        {
            SkillMeleeData skillParticleData = _skillParticleDatas.Find(x => x.Index == attackData.ResourceIndex);

            if (skillParticleData == null)
                return;

            if (skillParticleData.particle != null)
            {
                ParticleSystem particle = skillParticleData.particle;

                Vector3 TargetPos = target.transform.position;

                Vector3 MyPos = transform.position;

                Vector3 dirvec = TargetPos - MyPos;
                dirvec.Normalize();

                particle.transform.rotation = Quaternion.FromToRotation(Vector3.right, dirvec);
                particle.Play();
            }

            CharacterControllerBase.PlaySkill(attackData, transform.position, target);
        }
    }
}