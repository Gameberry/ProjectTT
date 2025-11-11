using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class SkillDirectVisioningAction_Nightingale : SkillDirectVisioningAction
    {
        [SerializeField] 
        protected SkeletonAnimationHandler _mySkeletonAnimationHandler;

        [SerializeField]
        private string _healAni;

        [SerializeField]
        private float _showDuration = 0.2f;

        [SerializeField]
        private float _viewDuration = 1.1f;

        [SerializeField]
        private float _hideDuration = 0.2f;

        [SerializeField]
        private ParticleSystem _hideParticle;

        [SerializeField]
        private float _releaseTime = 1.0f;

        [SerializeField]
        private ParticleSystem _healParticle;

        [SerializeField]
        private Transform _shadowObject;

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        public override void Init()
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.AnimationEvent += SpineAnimationEvent;
                _mySkeletonAnimationHandler.AnimationList_Dic.Add(_healAni, null);
            }
        }
        //------------------------------------------------------------------------------------
        public override void Play()
        {
            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == true)
                disableCancellation = new CancellationTokenSource();

            _mySkeletonAnimationHandler.SetColor(Color.clear);
            _mySkeletonAnimationHandler.PlayAnimation_Once(_healAni, false);

            PlayNextWaveDelay().Forget();
        }

        private async UniTask PlayNextWaveDelay()
        {
            float duration = _showDuration;
            float starttime = Time.time;
            float endtime = Time.time + duration;

            while (Time.time < endtime)
            {
                float ratio = (Time.time - starttime) / duration;

                Color color = Color.white;
                color.a = ratio;
                _mySkeletonAnimationHandler.SetColor(color);

                await UniTask.Yield(disableCancellation.Token);
            }

            _mySkeletonAnimationHandler.SetColor(Color.white);
            await UniTask.Delay((int)(_viewDuration * 1000), false, PlayerLoopTiming.Update, disableCancellation.Token);


            if (_hideParticle != null)
            {
                _hideParticle.gameObject.SetActive(true);
                _hideParticle.Stop();
                _hideParticle.Play();
            }

            duration = _hideDuration;
            starttime = Time.time;
            endtime = Time.time + duration;

            while (Time.time < endtime)
            {
                float ratio = (Time.time - starttime) / duration;

                Color color = Color.white;
                color.a = 1.0f - ratio;
                _mySkeletonAnimationHandler.SetColor(color);

                await UniTask.Yield(disableCancellation.Token);
            }

            _mySkeletonAnimationHandler.SetColor(Color.clear);


            await UniTask.Delay((int)_releaseTime * 1000, false, PlayerLoopTiming.Update, disableCancellation.Token);

            Release();
        }
        //------------------------------------------------------------------------------------
        public void AniActionCallBack(AnimationAction aniaction)
        {
            //switch (_jumpState)
            //{
            //    case ThorState.Landing:
            //        {

            //            break;
            //        }
            //    case ThorState.ActionReady:
            //        {
            //            if (aniaction == AnimationAction.AniEnd)
            //            {
            //                PlayAttack();
            //            }


            //            break;
            //        }
            //    case ThorState.Attack:
            //        {
            //            if (aniaction == AnimationAction.AniEnd)
            //            {
            //                Release();
            //            }

            //            break;
            //        }
            //}
        }
        //------------------------------------------------------------------------------------
        public override void Release()
        {
            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == false)
            {
                disableCancellation.Cancel();
                disableCancellation.Dispose();
            }

            if (_shadowObject != null)
                _shadowObject.gameObject.SetActive(true);

            base.Release();
        }
        //------------------------------------------------------------------------------------
        private void SpineAnimationEvent(string aniName, string eventName)
        {
            if (eventName == "Start")
                AniActionCallBack(AnimationAction.AniStart);
            else if (eventName == "AniAction")
            {
                _skillProjectilePlayer.CharacterControllerBase.PlaySkill(_skillManageInfo);
                if (_healParticle != null)
                {
                    _healParticle.gameObject.SetActive(true);
                    _healParticle.Play();
                }
                AniActionCallBack(AnimationAction.AniAction);
            }
            else if (eventName == "End")
                AniActionCallBack(AnimationAction.AniEnd);
        }
        //------------------------------------------------------------------------------------
    }
}