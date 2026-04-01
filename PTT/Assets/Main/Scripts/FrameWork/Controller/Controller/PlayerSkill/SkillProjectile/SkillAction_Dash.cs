using System.Collections;
using UnityEngine;

namespace GameBerry
{
    public class SkillAction_Dash : SkillAction
    {
        [Header("Dash")]
        [SerializeField] private float _stopDistance = 0.1f;
        [SerializeField] private bool _clampToMapRange = true;
        [SerializeField] private float _dashDuration = 0.2f;

        [Header("VFX / Attack")]
        [SerializeField] private ParticleSystem _attakParticle;

        private Coroutine _moveRoutine;
        private bool _dashActive;
        private bool _released;

        private Rigidbody _rb;

        private Vector3 _startPos;
        private Vector3 _endPos;
        private Vector3 _targetPosCached;

        public override void Play()
        {
            if (_skillProjectilePlayer == null || _skillProjectilePlayer.CharacterControllerBase == null)
            {
                ReleaseOnce();
                return;
            }

            var caster = _skillProjectilePlayer.CharacterControllerBase;
            _rb = caster.MyRigidbody;

            _released = false;
            _dashActive = false;

            BeginDash();
        }

        private void BeginDash()
        {
            if (_released || _dashActive)
                return;

            var caster = _skillProjectilePlayer.CharacterControllerBase;

            _targetPosCached = (_target != null) ? _target.transform.position : _targetPosition;
            _targetPosCached.y = 0f;

            _startPos = GetCasterPos();
            _startPos.y = 0f;

            Vector3 toTarget = _targetPosCached - _startPos;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                toTarget = caster.transform.forward;
                toTarget.y = 0f;
            }

            Vector3 dir = toTarget.normalized;

            float stopDist = Mathf.Max(0f, _stopDistance);
            float distToTarget = Vector3.Distance(_startPos, _targetPosCached);

            if (distToTarget <= stopDist + 0.0001f)
            {
                _endPos = _startPos;
                _dashActive = true;
                FinishDashAndAttack();
                ReleaseOnce();
                return;
            }

            _endPos = _targetPosCached - dir * stopDist;

            if (_clampToMapRange)
                ClampToMapRange(ref _endPos);

            _dashActive = true;
            StopMoveRoutine();
            _moveRoutine = StartCoroutine(CoMoveDash(Mathf.Max(0.01f, _dashDuration)));
        }

        private IEnumerator CoMoveDash(float duration)
        {
            float elapsed = 0f;
            Vector3 start = _startPos;
            Vector3 end = _endPos;

            while (!_released && _dashActive)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = 1f - (1f - t) * (1f - t);

                Vector3 nextPos = Vector3.LerpUnclamped(start, end, easedT);

                if (_clampToMapRange)
                    ClampToMapRange(ref nextPos);

                SetCasterPos(nextPos);

                if (t >= 1f)
                {
                    SetCasterPos(end);
                    _dashActive = false;
                    FinishDashAndAttack();
                    ReleaseOnce();
                    yield break;
                }

                yield return null;
            }
        }

        private void FinishDashAndAttack()
        {
            var caster = _skillProjectilePlayer.CharacterControllerBase;

            if (_attakParticle != null)
            {
                _attakParticle.transform.position = caster.transform.position;
                _attakParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _attakParticle.Play();
            }

            caster.PlaySkill(_attackData, caster.transform.position);
        }

        private void StopMoveRoutine()
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }
        }

        private Vector3 GetCasterPos()
        {
            var caster = _skillProjectilePlayer.CharacterControllerBase;
            if (_rb != null)
                return _rb.position;

            return caster.transform.position;
        }

        private void SetCasterPos(Vector3 pos)
        {
            var caster = _skillProjectilePlayer.CharacterControllerBase;
            pos.y = caster.transform.position.y;

            if (_rb != null)
                _rb.MovePosition(pos);
            else
                caster.transform.position = pos;
        }

        private void ClampToMapRange(ref Vector3 pos)
        {
            var data = StaticResource.Instance.GetBattleModeStaticData();
            Vector3 minpos = data.MapRange_Min;
            Vector3 maxpos = data.MapRange_Max;

            if (pos.x < minpos.x) pos.x = minpos.x;
            else if (pos.x > maxpos.x) pos.x = maxpos.x;

            if (pos.z < minpos.z) pos.z = minpos.z;
            else if (pos.z > maxpos.z) pos.z = maxpos.z;
        }

        private void ReleaseOnce()
        {
            if (_released)
                return;

            _released = true;
            _dashActive = false;
            StopMoveRoutine();
            base.Release();
        }

        public override void Release()
        {
            ReleaseOnce();
        }
    }
}
