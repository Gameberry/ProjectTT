using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    public class SkillAction_Dash : SkillAction
    {
        [Header("Spine")]
        [SerializeField] private string _dashAnimationName = "Skill_dash";
        [SerializeField] private int _trackIndex = 0;
        [SerializeField] private string _eventDashStart = "Dash_Start";
        [SerializeField] private string _eventDashEnd = "Dash_End";

        [Header("Dash")]
        [SerializeField] private float _stopDistance = 0.1f;         // 타겟에 딱 붙지 않게 유지할 거리
        [SerializeField] private bool _clampToMapRange = true;

        [Header("VFX / Attack")]
        [SerializeField] private ParticleSystem _attakParticle;

        private SkeletonAnimation _skeletonAnim;

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

            // 캐릭터에 SkeletonAnimation이 어디에 붙는지 프로젝트마다 다름
            _skeletonAnim = caster.GetSkeletonAnimation();
            if (_skeletonAnim == null)
            {
                ReleaseOnce();
                return;
            }

            BindSpineEvents();

            _released = false;
            _dashActive = false;
        }

        private void BindSpineEvents()
        {
            UnbindSpineEvents();
            _skeletonAnim.AnimationState.Event += OnSpineEvent;
            _skeletonAnim.AnimationState.Complete += OnSpineComplete;
        }

        private void UnbindSpineEvents()
        {
            if (_skeletonAnim == null) return;
            _skeletonAnim.AnimationState.Event -= OnSpineEvent;
            _skeletonAnim.AnimationState.Complete -= OnSpineComplete;
        }

        private void OnSpineEvent(TrackEntry entry, Spine.Event e)
        {
            if (_released || e?.Data == null) return;

            string name = e.Data.Name;

            if (name == _eventDashStart)
            {
                BeginDashOnEventStart();   // 여기서 actualInterval 계산 후 코루틴 시작
            }
            else if (name == _eventDashEnd)
            {
                EndDashOnEventEnd();       // 여기서 스냅 + 마무리
            }
        }

        private void OnSpineComplete(TrackEntry entry)
        {
            // 이벤트 누락 대비: 애니 끝났는데 release 안 됐으면 정리
            if (_released) return;

            if (_dashActive)
            {
                StopMoveRoutine();
                SnapToEndPos();
            }

            FinishDashAndAttack();
            ReleaseOnce();
        }

        private void BeginDashOnEventStart()
        {
            if (_released) return;
            if (_dashActive) return;

            var caster = _skillProjectilePlayer.CharacterControllerBase;

            // 타겟 위치 캐시
            _targetPosCached = (_target != null) ? _target.transform.position : _targetPosition;
            _targetPosCached.y = 0f;

            _startPos = GetCasterPos();
            _startPos.y = 0f;

            Vector3 toTarget = _targetPosCached - _startPos;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                // 방향이 애매하면 forward로 (y=0 평면)
                toTarget = caster.transform.forward;
                toTarget.y = 0f;
            }

            Vector3 dir = toTarget.normalized;

            float stopDist = Mathf.Max(0f, _stopDistance);
            float distToTarget = Vector3.Distance(_startPos, _targetPosCached);

            // 이미 충분히 가까우면 이동 없이 endPos=startPos
            if (distToTarget <= stopDist + 0.0001f)
            {
                _endPos = _startPos;
                _dashActive = true;
                return;
            }

            _endPos = _targetPosCached - dir * stopDist;

            if (_clampToMapRange)
                ClampToMapRange(ref _endPos);

            // dash_start ~ dash_end 실제 간격(초) 가져오기 (TimeScale 반영)
            float dashMoveDuration = GetActualEventIntervalSeconds(
                _skeletonAnim, _trackIndex, _eventDashStart, _eventDashEnd,
                fallbackSeconds: 0.2f
            );

            _dashActive = true;

            StopMoveRoutine();
            _moveRoutine = StartCoroutine(CoMoveDash(dashMoveDuration));
        }

        private IEnumerator CoMoveDash(float duration)
        {
            duration = Mathf.Max(0.0001f, duration);

            float elapsed = 0f;
            Vector3 start = _startPos;
            Vector3 end = _endPos;

            while (!_released && _dashActive)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                // Ease-Out (Quadratic)
                float easedT = 1f - (1f - t) * (1f - t);

                Vector3 nextPos = Vector3.LerpUnclamped(start, end, easedT);

                if (_clampToMapRange)
                    ClampToMapRange(ref nextPos);

                SetCasterPos(nextPos);

                // duration은 이벤트 간격이라 보통 여기서 정확히 끝나지만,
                // 실제 종료는 dash_end에서 스냅으로 “확정”한다.
                if (t >= 1f)
                {
                    // endPos 유지
                    SetCasterPos(end);
                }

                yield return null;
            }
        }

        private void EndDashOnEventEnd()
        {
            if (_released) return;

            _dashActive = false;

            StopMoveRoutine();

            // dash_end 시점에 “확정 도착”
            SnapToEndPos();

            FinishDashAndAttack();
            ReleaseOnce();
        }

        private void SnapToEndPos()
        {
            Vector3 pos = _endPos;
            if (_clampToMapRange)
                ClampToMapRange(ref pos);

            SetCasterPos(pos);
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

            // 원치 않으면 주석 처리
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
            if (_rb != null) return _rb.position;
            return caster.transform.position;
        }

        private void SetCasterPos(Vector3 pos)
        {
            var caster = _skillProjectilePlayer.CharacterControllerBase;

            // y는 원래 값 유지(너 프로젝트 평면이 x/z인 경우가 많아서)
            pos.y = caster.transform.position.y;

            if (_rb != null) _rb.MovePosition(pos);
            else caster.transform.position = pos;
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

        /// <summary>
        /// 현재 track에 재생 중인 애니에서 start/end 이벤트의 로컬 시간 차이를 찾고,
        /// entry.TimeScale + state.TimeScale을 반영해서 실제 초(actual seconds)로 변환해 반환.
        /// 못 찾으면 fallback 반환.
        /// </summary>
        private static float GetActualEventIntervalSeconds(
            SkeletonAnimation skeletonAnim,
            int trackIndex,
            string startEventName,
            string endEventName,
            float fallbackSeconds)
        {
            if (skeletonAnim == null) return fallbackSeconds;

            TrackEntry entry = skeletonAnim.AnimationState.GetCurrent(trackIndex);
            if (entry?.Animation == null) return fallbackSeconds;

            float localStart = -1f;
            float localEnd = -1f;

            var timelines = entry.Animation.Timelines;
            if (timelines != null)
            {
                for (int i = 0; i < timelines.Count; i++)
                {
                    if (timelines.Items[i] is not EventTimeline et)
                        continue;

                    float[] times = et.Frames;
                    Spine.Event[] events = et.Events;

                    for (int k = 0; k < events.Length; k++)
                    {
                        var ev = events[k];
                        if (ev?.Data == null) continue;

                        string n = ev.Data.Name;
                        if (n == startEventName) localStart = times[k];
                        else if (n == endEventName) localEnd = times[k];
                    }
                }
            }

            if (localStart < 0f || localEnd < 0f) return fallbackSeconds;

            float localInterval = Mathf.Max(0f, localEnd - localStart);

            float stateScale = skeletonAnim.AnimationState.TimeScale;
            float entryScale = entry.TimeScale;

            float effectiveScale = stateScale * entryScale;
            if (effectiveScale <= 0.0001f) effectiveScale = 1f;

            float actualInterval = localInterval / effectiveScale;
            if (actualInterval <= 0.0001f) actualInterval = fallbackSeconds;

            return actualInterval;
        }

        private void ReleaseOnce()
        {
            if (_released) return;
            _released = true;

            _dashActive = false;
            StopMoveRoutine();

            UnbindSpineEvents();

            base.Release();
        }

        public override void Release()
        {
            ReleaseOnce();
        }
    }
}
