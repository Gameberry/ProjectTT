using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class SkillDirectVisioningAction_MonkeyKing : SkillDirectVisioningAction
    {
        //[SerializeField] protected SkeletonAnimationHandler _mySkeletonAnimationHandler;

        [SerializeField]
        private AnimationCurve _runAniCurv;

        [SerializeField]
        private float _applyDamageRatio = 0.8f;

        [SerializeField]
        private ParticleSystem _onDamageParticle;

        [SerializeField]
        private float _duration = 0.5f;

        

        public Vector3 StartWorldPos;
        public Vector3 EndWorldPos;

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        public override void Play()
        {
            transform.eulerAngles = Vector3.zero;

            if (_onDamageParticle != null)
                _onDamageParticle.gameObject.SetActive(false);

            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == true)
                disableCancellation = new CancellationTokenSource();

            PlayNextWaveDelay().Forget();
        }

        private async UniTask PlayNextWaveDelay()
        {
            float starttime = Time.time;
            float endtime = Time.time + _duration;

            Vector3 targetpos = _skillProjectilePlayer.CharacterControllerBase.transform.position;//_targetController.transform.position;

            Vector3 startpos = targetpos + StartWorldPos;
            Vector3 endpos = targetpos + EndWorldPos;

            Vector3 posgab = endpos - startpos;

            bool applyDamage = false;

            while (Time.time < endtime)
            {
                float ratio = (Time.time - starttime) / _duration;

                Vector3 pos = startpos + (posgab * _runAniCurv.Evaluate(ratio));

                transform.position = pos;

                if (applyDamage == false)
                {
                    if (_applyDamageRatio < ratio)
                    {
                        _skillProjectilePlayer.CharacterControllerBase.PlaySkill(_skillManageInfo);

                        if (_onDamageParticle != null)
                            _onDamageParticle.gameObject.SetActive(true);
                        applyDamage = true;
                    }
                }

                await UniTask.Yield(disableCancellation.Token);
            }

            Release();
        }

        public override void Release()
        {
            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == false)
            {
                disableCancellation.Cancel();
                disableCancellation.Dispose();
            }

            if (_onDamageParticle != null)
                _onDamageParticle.gameObject.SetActive(false);

            base.Release();
        }
    }
}