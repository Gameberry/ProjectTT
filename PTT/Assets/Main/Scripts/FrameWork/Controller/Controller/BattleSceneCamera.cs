using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameBerry
{
    public class BattleSceneCamera : MonoSingleton<BattleSceneCamera>
    {
        [SerializeField]
        private Camera _myCamera;

        public Camera BattleCamera { get { return _myCamera; } }

        [SerializeField]
        private InGamePositionContainer _inGamePositionContainer;

        [SerializeField]
        private float _cameraDefaultSize = 5.0f;

        [SerializeField]
        private float _cameraMaxSize;

        [SerializeField]
        private float _cameraSizeSmoothValue = 1.0f;

        [SerializeField]
        private float _creatureDefaultPosGap = 7.8f;

        [SerializeField]
        private float _creatureMaxPosGap = 10.0f;

        [SerializeField]
        private float _focusMove_X = 0.0f;

        [SerializeField]
        private float _cameraSmoothTime = 0.1f;



        Vector3 velocity = Vector3.zero;

        private void LateUpdate()
        {
            if (Managers.BattleSceneManager.Instance.CurrentBattleScene == null)
                return;

            if (Managers.BattleSceneManager.Instance.CurrentBattleScene.IsPlay == false)
                return;

            if (Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers == null)
                return;

            float xpos = Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.transform.position.x;

            Vector3 cameraPos = Vector3.zero;
            cameraPos.x = xpos + _focusMove_X;

            transform.position = Vector3.SmoothDamp(transform.position, cameraPos, ref velocity, _cameraSmoothTime);


        }
    }
}