using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
//using Google.Play.Review;
#endif

namespace GameBerry
{
    public class ProjectReviewManager : MonoSingleton<ProjectReviewManager>
    {
        public void ShowReview()
        {
//#if UNITY_EDITOR
//            Debug.Log("BuyReview");
//#elif UNITY_ANDROID
//            StartCoroutine(ReviewShow());
//#elif UNITY_IOS
//            UnityEngine.iOS.Device.RequestStoreReview();
//#endif
        }

//#if UNITY_ANDROID
//        private IEnumerator ReviewShow()
//        {
//            ReviewManager _reviewManager = new ReviewManager();
//            var requestFlowOperation = _reviewManager.RequestReviewFlow();
//            yield return requestFlowOperation;
//            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
//            {
//                Debug.LogError("requestFlowOperation.Error : " + requestFlowOperation.Error);
//                // Log error. For example, using requestFlowOperation.Error.ToString().
//                yield break;
//            }

//            PlayReviewInfo _playReviewInfo = requestFlowOperation.GetResult();
//            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
//            yield return launchFlowOperation;
//            _playReviewInfo = null; // Reset the object
//            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
//            {
//                Debug.Log("launchFlowOperation.Error : " + launchFlowOperation.Error);
//                // Log error. For example, using requestFlowOperation.Error.ToString().
//                yield break;
//            }

//            Debug.Log("Open Review Over!");
//        }
//#endif

    }
}