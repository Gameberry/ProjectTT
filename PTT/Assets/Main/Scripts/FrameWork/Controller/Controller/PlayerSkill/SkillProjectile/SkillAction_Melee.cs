using UnityEngine;

namespace GameBerry
{
    public class SkillAction_Melee : SkillAction
    {
        [SerializeField] private ParticleSystem _attakParticle;

        private bool _released;

        public override void Play()
        {
            if (_skillProjectilePlayer == null || _skillProjectilePlayer.CharacterControllerBase == null)
            {
                ReleaseOnce();
                return;
            }

            if (_attakParticle != null)
            {
                _attakParticle.gameObject.SetActive(true);
                _attakParticle.Play();
            }

            _released = false;
            ReleaseOnce();
        }

        private void ReleaseOnce()
        {
            if (_released)
                return;

            _released = true;
            base.Release();
        }

        public override void Release()
        {
            ReleaseOnce();
        }
    }
}
