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


        private CharacterControllerBase _characterControllerBase;

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

            _attakParticle?.Play();
            transform.rotation = Quaternion.FromToRotation(Vector3.left, dirvec);

            _characterControllerBase.PlaySkill(_attackData, transform.position, _target);

            _endTime = Time.time + _hitDuraion;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (_endTime < Time.time)
            {
                ReleaseObj();
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