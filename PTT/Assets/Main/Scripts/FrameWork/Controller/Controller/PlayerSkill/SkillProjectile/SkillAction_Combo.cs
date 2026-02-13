using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class MeleeParticle
    {
        public string CustomParam;
        public ParticleSystem ParticleSystem;
    }

    public class SkillAction_Combo : SkillAction
    {
        [System.Serializable]
        public class MeleeParticle
        {
            public string CustomParam;
            public ParticleSystem ParticleSystem;
        }

        [SerializeField]
        private List<MeleeParticle> _attackParticles = new List<MeleeParticle>();


        [SerializeField]
        private ParticleSystem _attakParticle;

        [SerializeField]
        private float _hitDuraion = 1.0f;

        private float _endTime;

        private float _playHit;

        private CharacterControllerBase _characterControllerBase;

        private bool _onHit = false;

        public override void Play()
        {
            if (_skillProjectilePlayer == null
                || _skillProjectilePlayer.CharacterControllerBase == null
                || _skillProjectilePlayer.CharacterControllerBase.AttackTarget == null)
            {
                Release();
                return;
            }

            _characterControllerBase = _skillProjectilePlayer.CharacterControllerBase;

            Vector3 TargetPos = _target.transform.position;

            Vector3 MyPos = _characterControllerBase.transform.position;

            Vector3 dirvec = TargetPos - MyPos;
            dirvec.Normalize();

            _onHit = false;

            //_playHit = Time.time + _attackData.MeleeAttackDelay;
            _playHit = Time.time;
            //transform.rotation = Quaternion.FromToRotation(Vector3.left, dirvec);

            Enum_LookDirection stageGenerateDirections = MyPos.x < TargetPos.x ? Enum_LookDirection.Right : Enum_LookDirection.Left;

            MeleeParticle meleeParticle = _attackParticles.Find(x => x.CustomParam == _attackData.SkillInfo.CustomParam);

            if (meleeParticle != null)
                _attakParticle = meleeParticle.ParticleSystem;

            if (stageGenerateDirections == Enum_LookDirection.Left)
            {
                Vector3 rotate = transform.eulerAngles;
                rotate.y = 0.0f;
                transform.eulerAngles = rotate;
            }
            else if (stageGenerateDirections == Enum_LookDirection.Right)
            {
                Vector3 rotate = transform.eulerAngles;
                rotate.y = 180.0f;
                transform.eulerAngles = rotate;
            }
            //_endTime = Time.time + _attackData.MeleeAttackDelay + _hitDuraion;
            _endTime = Time.time + _hitDuraion;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (_endTime < Time.time)
            {
                ReleaseObj();
            }

            if (_onHit == false && _playHit < Time.time)
            {
                _onHit = true;
                _attakParticle?.gameObject.SetActive(true);
                _attakParticle?.Play();
                _characterControllerBase.PlaySkill(_attackData, transform.position, _target);
            }
        }
        //------------------------------------------------------------------------------------
        private void ReleaseObj()
        {
            _attakParticle?.gameObject.SetActive(false);
            Release();
        }
        //------------------------------------------------------------------------------------
    }
}