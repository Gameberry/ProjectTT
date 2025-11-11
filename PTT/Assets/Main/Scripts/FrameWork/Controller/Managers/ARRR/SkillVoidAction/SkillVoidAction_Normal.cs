using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillVoidAction_Normal : SkillVoidAction
    {
        [SerializeField]
        private Transform _shootObj;

        [SerializeField]
        private ParticleSystem m_shootObj_Particle;

        private NoneTargetProjectileState CurrentShootState = NoneTargetProjectileState.None;


        private float _speed;
        private float _endTime;

        private Vector3 _goalPos = Vector3.zero;
        private Vector3 _startPos = Vector3.zero;

        private float _movePos = 1;

        private CharacterControllerBase _characterControllerBase;

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

            _speed = _skillManageInfo.SkillBaseData.SkillDamageIndex.DamageParam;
            _goalPos = _targetPosition;

            _startPos = transform.position;

            _movePos = _goalPos.x >= _skillProjectilePlayer.CharacterControllerBase.transform.position.x ? 1.0f : -1.0f;

            Vector3 rotate = transform.eulerAngles;

            float selectRatote = 0.0f;

            if (_movePos == -1.0f)
                selectRatote = 180.0f;

            rotate.y = selectRatote;

            transform.eulerAngles = rotate;

            _repeatDelay = _skillManageInfo.GetRepeatDelay();

            _endTime = Time.time + _skillManageInfo.GetDuration();

            _nextHitTime = 0;

            if (_speed == -1)
            {
                _characterControllerBase.PlaySkill(_skillManageInfo, _goalPos);
                transform.position = _goalPos;
                ChangeState(NoneTargetProjectileState.Hit);
            }
            else
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
                if (_endTime < Time.time)
                {
                    ChangeState(NoneTargetProjectileState.Hit);
                    return;
                }

                if (_nextHitTime <= Time.time)
                {
                    _characterControllerBase.PlaySkill(_skillManageInfo, transform.position);
                    _nextHitTime = Time.time + _repeatDelay;
                }

                Vector3 pos = transform.position;
                pos.x += _movePos * _speed * Time.deltaTime;
                transform.position = pos;
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