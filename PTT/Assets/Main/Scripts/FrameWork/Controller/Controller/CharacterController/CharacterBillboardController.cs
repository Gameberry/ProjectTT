using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CharacterBillboardController : MonoBehaviour
    {
        [SerializeField]
        private CharacterControllerBase characterControllerBase;

        [Header("빌보드 대상")]
        [SerializeField] private List<Transform> applyXBillboard;
        [SerializeField] private List<Transform> fullBillboard;

        [Header("카메라")]
        [SerializeField] private Camera targetCamera;

        private void Awake()
        {
            if (targetCamera == null)
            {
                BattleSceneCamera battleSceneCamera = Managers.BattleSceneManager.Instance.GetBattleSceneCamera();
                if (battleSceneCamera != null)
                    targetCamera = battleSceneCamera.BattleCamera;
                else
                    targetCamera = Camera.main;
            }

            characterControllerBase = GetComponent<CharacterControllerBase>();
        }

        private void Start()
        {
            RefreshBillboard();
        }

        /// <summary>
        /// 필요할 때만 호출해서 Spine/HPBar 방향을 갱신
        /// </summary>
        public void RefreshBillboard()
        {
            if (targetCamera == null)
                return;

            // Orthographic 카메라 → 모든 오브젝트에 대해 방향은 항상 -forward
            Vector3 lookDir = targetCamera.transform.forward;

            if (lookDir.sqrMagnitude < 0.0001f)
                return;

            for (int i = 0; i < fullBillboard.Count; ++i)
            { 
                if(fullBillboard[i] != null)
                    fullBillboard[i].rotation = Quaternion.LookRotation(lookDir);
            }


            if (characterControllerBase == null)
                return;

            lookDir = characterControllerBase.LookDirection == Enum_LookDirection.Left ? targetCamera.transform.forward : -targetCamera.transform.forward;


            if (lookDir.sqrMagnitude < 0.0001f)
                return;

            for (int i = 0; i < applyXBillboard.Count; ++i)
            {
                if (applyXBillboard[i] != null)
                    applyXBillboard[i].rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }
}