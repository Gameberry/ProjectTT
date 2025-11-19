using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillAction_Melee : SkillAction
    {

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

            _playHit = Time.time + _attackData.MeleeAttackDelay;
            transform.rotation = Quaternion.FromToRotation(Vector3.left, dirvec);
            _endTime = Time.time + _attackData.MeleeAttackDelay + _hitDuraion;
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
                _attakParticle?.Play();
                _characterControllerBase.PlaySkill(_attackData, transform.position, _target);
            }
        }
        //------------------------------------------------------------------------------------
        private void ReleaseObj()
        {
            Release();
        }
        //------------------------------------------------------------------------------------
    }
}