using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillRepeatAttackAction_Normal : SkillRepeatAttackAction
    {
        [SerializeField]
        private Transform _shootObj;

        [SerializeField]
        private ParticleSystem m_shootObj_Particle;

        private NoneTargetProjectileState CurrentShootState = NoneTargetProjectileState.None;


        private Vector3 _goalPos = Vector3.zero;
        private Vector3 _startPos = Vector3.zero;

        private CharacterControllerBase _characterControllerBase;

        private int _totalRepeatCount = 0;
        private int _repeatCount = 0;
        private float _repeatDelay = 0;

        private float _nextHitTime = 0;

        public override void Play()
        {
            if (_skillManageInfo == null
                || _skillManageInfo.SkillBaseData == null
                || _skillManageInfo.SkillBaseData.SkillDamageIndex == null
                || _skillProjectilePlayer == null
                || _skillProjectilePlayer.CharacterControllerBase == null
                || _skillProjectilePlayer.CharacterControllerBase.AttackTarget == null)
            {
                Release();
                return;
            }

            CurrentShootState = NoneTargetProjectileState.None;

            _characterControllerBase = _skillProjectilePlayer.CharacterControllerBase;

            gameObject.layer = LayerMask.NameToLayer(Managers.SkillManager.Instance.GetLayerTrigger(_characterControllerBase.IFFType));

            _goalPos = _targetPosition;

            transform.position = _goalPos;

            _totalRepeatCount = _skillManageInfo.GetRepeatCount();
            _repeatCount = 0;
            _repeatDelay = _skillManageInfo.GetRepeatDelay();

            _nextHitTime = 0;

            PlayProjectile();
        }
        //------------------------------------------------------------------------------------
        public void PlayProjectile()
        {
            ChangeState(NoneTargetProjectileState.Shoot);
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (CurrentShootState == NoneTargetProjectileState.Hit)
            {
                ReleaseObj();

                return;
            }

            if (CurrentShootState == NoneTargetProjectileState.Shoot)
            {
                if (_nextHitTime <= Time.time)
                {
                    _characterControllerBase.PlaySkill(_skillManageInfo, transform.position);
                    _repeatCount++;

                    if (_totalRepeatCount <= _repeatCount)
                    {
                        ChangeState(NoneTargetProjectileState.Hit);
                        return;
                    }
                    else
                    {
                        _nextHitTime = Time.time + _repeatDelay;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ChangeState(NoneTargetProjectileState state)
        {
            if (CurrentShootState == state)
                return;

            CurrentShootState = state;

            if (m_shootObj_Particle != null)
            {
                m_shootObj_Particle.gameObject.SetActive(state == NoneTargetProjectileState.Shoot);
                if (state == NoneTargetProjectileState.Shoot)
                {
                    m_shootObj_Particle.Stop();
                    m_shootObj_Particle.Play();
                }
            }

            if (_shootObj != null)
            {
                _shootObj.gameObject.SetActive(state == NoneTargetProjectileState.Shoot);
            }
        }
        //------------------------------------------------------------------------------------
        public void ForceRelease()
        {

        }
        //------------------------------------------------------------------------------------
        private void ReleaseObj()
        {
            Release();
        }
        //------------------------------------------------------------------------------------
    }
}
