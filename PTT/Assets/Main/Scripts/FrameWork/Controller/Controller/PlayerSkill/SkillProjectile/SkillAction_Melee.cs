using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    public class SkillAction_Melee : SkillAction
    {
        [SerializeField]
        private ParticleSystem _attakParticle;

        private SkeletonAnimation _skeletonAnim;

        private CharacterControllerBase _characterControllerBase;

        private bool _released;

        public override void Play()
        {
            if (_skillProjectilePlayer == null || _skillProjectilePlayer.CharacterControllerBase == null)
            {
                ReleaseOnce();
                return;
            }

            if(_attakParticle != null)
                _attakParticle.gameObject.SetActive(false);
            
            var caster = _skillProjectilePlayer.CharacterControllerBase;

            // 캐릭터에 SkeletonAnimation이 어디에 붙는지 프로젝트마다 다름
            _skeletonAnim = caster.GetSkeletonAnimation();
            if (_skeletonAnim == null)
            {
                ReleaseOnce();
                return;
            }


            BindSpineEvents();

            _released = false;

        }
        //------------------------------------------------------------------------------------
        private void BindSpineEvents()
        {
            UnbindSpineEvents();
            _skeletonAnim.AnimationState.Event += OnSpineEvent;
            _skeletonAnim.AnimationState.Complete += OnSpineComplete;
        }
        //------------------------------------------------------------------------------------
        private void UnbindSpineEvents()
        {
            if (_skeletonAnim == null) return;
            _skeletonAnim.AnimationState.Event -= OnSpineEvent;
            _skeletonAnim.AnimationState.Complete -= OnSpineComplete;
        }
        //------------------------------------------------------------------------------------
        private void OnSpineEvent(TrackEntry entry, Spine.Event e)
        {
            if (_released || e?.Data == null) return;

            string name = e.Data.Name;

            if (name.Contains("AniAction"))
            {
                if(_attakParticle != null)
                {
                    _attakParticle?.gameObject.SetActive(true);
                    _attakParticle?.Play();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnSpineComplete(TrackEntry entry)
        {
            // 이벤트 누락 대비: 애니 끝났는데 release 안 됐으면 정리
            if (_released) return;

            ReleaseOnce();
        }
        //------------------------------------------------------------------------------------
        private void ReleaseOnce()
        {
            if (_released) return;
            _released = true;

            UnbindSpineEvents();
            //_attakParticle?.gameObject.SetActive(false);
            base.Release();
        }
        //------------------------------------------------------------------------------------
        public override void Release()
        {
            ReleaseOnce();
        }
        //------------------------------------------------------------------------------------
    }
}