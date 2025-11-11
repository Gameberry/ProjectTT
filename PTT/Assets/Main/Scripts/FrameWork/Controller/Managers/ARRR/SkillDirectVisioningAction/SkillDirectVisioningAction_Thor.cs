using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillDirectVisioningAction_Thor : SkillDirectVisioningAction
    {
        [SerializeField] protected SkeletonAnimationHandler _mySkeletonAnimationHandler;

        [SerializeField]
        private string _landingAni;

        [SerializeField]
        private float _landingAni_Speed = 1.0f;

        [SerializeField]
        private string _attackReadyAni;

        [SerializeField]
        private float _attackReadyAni_Speed = 1.0f;

        [SerializeField]
        private string _attackAni;

        [SerializeField]
        private float _attackAni_Speed = 1.0f;

        [SerializeField]
        private float _landingHeight = 1.0f;

        [SerializeField]
        private float _landingDuration = 1.0f;

        [SerializeField]
        private float _attackReadyDuration = 1.0f;

        [SerializeField]
        private float _attackReadyPlayRatio = 0.5f;

        [SerializeField]
        private AnimationCurve _landingCurve_Height;

        [SerializeField]
        private ParticleSystem _jumpReadyParticle;

        private ThorState _jumpState = ThorState.Max;

        public Vector3 startPos;
        public Vector3 goalPos;
        public Vector3 positionGap;

        private float startTime;
        private float duration;
        private float endTime;

        [SerializeField]
        private Transform _shadowObject;

        private enum ThorState
        {
            Landing,
            ActionReady,
            Attack,


            Max,
        }

        public override void Init()
        {
            if (_mySkeletonAnimationHandler != null)
            { 
                _mySkeletonAnimationHandler.AnimationEvent += SpineAnimationEvent;
                _mySkeletonAnimationHandler.AnimationList_Dic.Add(_landingAni, null);
                _mySkeletonAnimationHandler.AnimationList_Dic.Add(_attackReadyAni, null);
                _mySkeletonAnimationHandler.AnimationList_Dic.Add(_attackAni, null);
            }
        }
        //------------------------------------------------------------------------------------
        public override void Play()
        {
            Vector3 targetpos = _skillProjectilePlayer.CharacterControllerBase.transform.position;//_targetController.transform.position;

            Vector2 direction = Vector2.zero;

            if (_targetController != null)
            { 
                targetpos = _targetController.transform.position;
                direction = _targetController.transform.position - _skillProjectilePlayer.CharacterControllerBase.transform.position;
                direction.Normalize();
            }

            goalPos = targetpos;
            targetpos.y += _landingHeight;
            transform.position = targetpos;
            startPos = targetpos;

            positionGap = goalPos - startPos;

            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.SetAnimationSpeed(_attackReadyAni_Speed);
                _mySkeletonAnimationHandler.PlayAnimation_Once(_attackReadyAni, false);
            }

            if (_jumpReadyParticle != null)
            {
                _jumpReadyParticle.gameObject.SetActive(true);
                _jumpReadyParticle.Stop();
                _jumpReadyParticle.Play();
            }

            startTime = Time.time;
            duration = _landingDuration;
            endTime = startTime + duration;

            Vector3 rotate = transform.eulerAngles;
            if (direction.x < 0)
                rotate.y = 180.0f;
            else
                rotate.y = 0;

            transform.eulerAngles = rotate;

            _jumpState = ThorState.ActionReady;
        }
        //------------------------------------------------------------------------------------
        private void PlayActionReady()
        {
            if (_mySkeletonAnimationHandler != null)
            { 
                _mySkeletonAnimationHandler.SetAnimationSpeed(_attackReadyAni_Speed);
                _mySkeletonAnimationHandler.PlayAnimation_Once(_attackReadyAni, false);
            }

            _jumpState = ThorState.ActionReady;
        }
        //------------------------------------------------------------------------------------
        private void PlayAttack()
        {
            if (_mySkeletonAnimationHandler != null)
            {
                _mySkeletonAnimationHandler.SetAnimationSpeed(_attackAni_Speed);
                _mySkeletonAnimationHandler.PlayAnimation_Once(_attackAni, false);
            }

            _skillProjectilePlayer.CharacterControllerBase.PlaySkill(_skillManageInfo, transform.position);

            _jumpState = ThorState.Attack;
        }
        //------------------------------------------------------------------------------------
        public void AniActionCallBack(AnimationAction aniaction)
        {
            switch (_jumpState)
            {
                case ThorState.Landing:
                    {
                        
                        break;
                    }
                case ThorState.ActionReady:
                    {
                        if (aniaction == AnimationAction.AniEnd)
                        {
                            PlayAttack();
                        }


                        break;
                    }
                case ThorState.Attack:
                    {
                        if (aniaction == AnimationAction.AniEnd)
                        {
                            Release();
                        }

                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (_jumpState == ThorState.ActionReady)
            {
                //if (Time.time > endTime)
                //{
                //    PlayAttack();
                //    return;
                //}

                float ratio = (Time.time - startTime) / duration;

                //if (_jumpState == ThorState.Landing && ratio > _attackReadyPlayRatio)
                //    PlayActionReady();


                Vector3 newpos = startPos;
                newpos.y -= _landingCurve_Height.Evaluate(ratio) * _landingHeight;

                transform.position = newpos;
            }
        }
        //------------------------------------------------------------------------------------
        public override void Release()
        {
            _jumpState = ThorState.Max;

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
                AniActionCallBack(AnimationAction.AniAction);
            else if (eventName == "End")
                AniActionCallBack(AnimationAction.AniEnd);
        }
        //------------------------------------------------------------------------------------
    }
}