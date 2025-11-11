using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class SkillCustomAction_JumpAttack : SkillCustomAction
    {
        [SerializeField]
        private string _jumpReadyAni;

        [SerializeField]
        private string _jumpAni;

        [SerializeField]
        private string _jumpEndAni;

        [SerializeField]
        private float _jumpHeight = 1.0f;

        [SerializeField]
        private float _jumpSpeed = 1.0f;

        [SerializeField]
        private AnimationCurve _jumpAnimationCurve_Height;

        [SerializeField]
        private AnimationCurve _jumpAnimationCurve_XZ;

        [SerializeField]
        private ParticleSystem _jumpReadyParticle;

        private JumpState _jumpState = JumpState.Max;

        private Vector3 startPos;
        private Vector3 goalPos;
        private Vector3 positionGap;

        private float startTime;
        private float duration;
        private float endTime;

        [SerializeField]
        private Transform _shadowObject;

        private enum JumpState
        {
            JumpReady,
            Jump,
            JumpEnd,

            Max,
        }

        public override void Play()
        {
            _jumpState = JumpState.JumpReady;
            if (_skillCustomActionPlayer != null)
                _skillCustomActionPlayer.CharacterControllerBase.PlayAnimation(_jumpReadyAni, false);

            if (_jumpReadyParticle != null)
            {
                _jumpReadyParticle.gameObject.SetActive(true);
                _jumpReadyParticle.Stop();
                _jumpReadyParticle.Play();
            }

            Managers.SoundManager.Instance.PlaySound("2_1");
        }

        private void PlayJump()
        {
            _jumpState = JumpState.Jump;

            if (_skillCustomActionPlayer != null)
            {
                CharacterControllerBase myController = _skillCustomActionPlayer.CharacterControllerBase;
                CharacterControllerBase target = myController.AttackTarget;

                if (target == null)
                {
                    PlayJumpEnd();
                }
                else
                {
                    if (_shadowObject != null)
                        _shadowObject.gameObject.SetActive(false);

                    _skillCustomActionPlayer.CharacterControllerBase.PlayAnimation(_jumpAni, true);

                    goalPos = target.transform.position;

                    goalPos.y = InGamePositionContainer.Instance.GetArrrStadardPos().transform.position.y;

                    startPos = myController.transform.position;

                    positionGap = goalPos - startPos;

                    float distance = Managers.AggroManager.Instance.GetDistance(myController, target);
                    duration = distance * _jumpSpeed;
                    startTime = Time.time;
                    endTime = startTime + duration;
                }

            }
        }

        private void PlayJumpEnd()
        {
            _jumpState = JumpState.JumpEnd;

            CharacterControllerBase characterControllerBase = _skillCustomActionPlayer.CharacterControllerBase;

            _skillCustomActionPlayer.CharacterControllerBase.PlayAnimation(_jumpEndAni, true);
            characterControllerBase.transform.position = goalPos;
            characterControllerBase.PlaySkill(_skillManageInfo);

            Managers.SoundManager.Instance.PlaySound("2_2");
        }

        public override void AniActionCallBack(AnimationAction aniaction)
        {
            switch (_jumpState)
            {
                case JumpState.JumpReady:
                    {
                        if (aniaction == AnimationAction.AniEnd)
                        {
                            PlayJump();
                        }
                        break;
                    }
                case JumpState.Jump:
                    {
                        


                        break;
                    }
                case JumpState.JumpEnd:
                    {
                        if (aniaction == AnimationAction.AniEnd)
                        {
                            _skillCustomActionPlayer.CharacterControllerBase.ChangeCharacterState(CharacterState.Idle);
                            Release();
                        }
                        
                        break;
                    }
            }
        }

        private void Update()
        {
            if (_jumpState == JumpState.Jump)
            {
                if (Time.time > endTime)
                {
                    PlayJumpEnd();
                    return;
                }

                float ratio = (Time.time - startTime) / duration;

                Vector3 newpos = startPos + (positionGap * _jumpAnimationCurve_XZ.Evaluate(ratio));
                newpos.y += _jumpAnimationCurve_Height.Evaluate(ratio) * _jumpHeight;

                _skillCustomActionPlayer.CharacterControllerBase.MyRigidbody2D.MovePosition(newpos);
            }
        }

        public override void Release()
        {
            _jumpState = JumpState.Max;

            if (_shadowObject != null)
                _shadowObject.gameObject.SetActive(true);
        }
    }
}