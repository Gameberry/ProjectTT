using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CreatureControllerBase : CharacterControllerBase
    {
        [SerializeField]
        protected List<ParticleSystem> _damageParticle;

        [SerializeField]
        protected List<ParticleSystem> _deadParticle;

        public int CreatureLevel = 1;

        protected CreatureData _creatureData;
        public CreatureData CreatureData { get { return _creatureData; } }

        //------------------------------------------------------------------------------------
        public override void SetCreatureSizeControll(Vector3 size)
        {
            base.SetCreatureSizeControll(size);
        }
        //------------------------------------------------------------------------------------
        public override void PlayAnimation(CharacterState state, string aniId)
        {
            if (_charAnicontroller != null)
            {
                _charAnicontroller.PlayAnimation(state, aniId);
            }
        }
        //------------------------------------------------------------------------------------
    }
}