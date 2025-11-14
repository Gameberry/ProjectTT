using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillProjectilAction_PositionAttack_SpawnPos : SkillProjectilAction
    {
        [SerializeField]
        private Transform _shootObj;

        [SerializeField]
        private ParticleSystem m_shootObj_Particle;

        [SerializeField]
        private ParticleSystem m_hitObj_Particle;

        [SerializeField]
        private float m_hitDuraion = 1.0f;

        [SerializeField]
        private Vector3 _addStartPos = Vector3.zero;

        private NoneTargetProjectileState CurrentShootState = NoneTargetProjectileState.None;

        [SerializeField]
        private float _speed;
        private float _endTime;

        private Vector3 _goalPos = Vector3.zero;
        private Vector3 _startPos = Vector3.zero;

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

            CurrentShootState = NoneTargetProjectileState.None;

            _characterControllerBase = _skillProjectilePlayer.CharacterControllerBase;

            _goalPos = _targetPosition;

            Enum_LookDirection stageGenerateDirections = _startPos.x < _goalPos.x ? Enum_LookDirection.Right : Enum_LookDirection.Left;

            _startPos = _goalPos + _addStartPos;

            transform.position = _startPos;

            //if (stageGenerateDirections == Enum_ARR_LookDirection.Right)
            //{
            //    Vector3 rotate = transform.eulerAngles;
            //    rotate.y = 0.0f;
            //    transform.eulerAngles = rotate;
            //}
            //else if (stageGenerateDirections == Enum_ARR_LookDirection.Left)
            //{
            //    Vector3 rotate = transform.eulerAngles;
            //    rotate.y = 180.0f;
            //    transform.eulerAngles = rotate;
            //}

            Vector3 TargetPos = _goalPos;

            Vector3 MyPos = transform.position;

            Vector3 dirvec = TargetPos - MyPos;
            dirvec.Normalize();

            if (m_shootObj_Particle != null)
                m_shootObj_Particle.transform.rotation = Quaternion.FromToRotation(Vector3.right, dirvec);

            if (_speed == -1)
            {
                _characterControllerBase.PlaySkill(_attackData, transform.position);
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
                if (_endTime < Time.time)
                {
                    ReleaseObj();
                }

                return;
            }

            if (CurrentShootState == NoneTargetProjectileState.Shoot)
            {
                if (MathDatas.GetDistance(transform.position, _goalPos) < 0.3f)
                {
                    _characterControllerBase.PlaySkill(_attackData, transform.position);

                    ChangeState(NoneTargetProjectileState.Hit);

                    return;
                }

                Vector3 TargetPos = _goalPos;

                Vector3 MyPos = transform.position;

                Vector3 dirvec = TargetPos - MyPos;
                dirvec.Normalize();

                if (m_shootObj_Particle != null)
                    m_shootObj_Particle.transform.rotation = Quaternion.FromToRotation(Vector3.right, dirvec);

                transform.position += dirvec * _speed * Time.deltaTime;
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


            if (m_hitObj_Particle != null)
            {
                m_hitObj_Particle.gameObject.SetActive(state == NoneTargetProjectileState.Hit);
                if (state == NoneTargetProjectileState.Hit)
                {
                    m_hitObj_Particle.Stop();
                    m_hitObj_Particle.Play();
                }
            }

            if (state == NoneTargetProjectileState.Hit)
            {
                _endTime = Time.time + m_hitDuraion;
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