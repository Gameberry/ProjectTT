using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace GameBerry
{
    public class DirectionSpineController : MonoBehaviour
    {
        [SerializeField]
        private SkeletonAnimation m_skeletonAnimation;


        public void PlayAnimation(string aniname)
        {
            if (m_skeletonAnimation != null)
            {
                if (m_skeletonAnimation.skeleton != null)
                    m_skeletonAnimation.skeleton.SetToSetupPose();

                m_skeletonAnimation.AnimationState.SetAnimation(0, aniname, false);
            }
        }
    }
}