using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class ScreenSpaceFollower : MonoBehaviour
    {
        public Transform Target { get; private set; }
        public Vector3 WorldOffset { get; set; }

        [SerializeField] private bool hideWhenBehindCamera = true;

        private RectTransform _rect;
        private Camera _battlecam;
        private Camera _uicam;

        void Awake()
        {
            _rect = (RectTransform)transform;
            _battlecam = Managers.BattleSceneManager.Instance.GetBattleSceneCamera().BattleCamera;
            _uicam = UI.UIManager.Instance.screenCanvasCamera;
            enabled = false;
        }

        public void SetTarget(Transform target, Vector3 offset)
        {
            Target = target;
            WorldOffset = offset;
            enabled = (target != null);
        }

        public void ClearTarget()
        {
            Target = null;
            enabled = false;
        }

        void LateUpdate()
        {
            if (Target == null) return;

            Vector3 sp = _battlecam.WorldToScreenPoint(Target.position + WorldOffset);
            Vector3 TextPos = _uicam.ScreenToWorldPoint(sp);

            if (hideWhenBehindCamera && sp.z < 0f)
            {
                if (_rect.gameObject.activeSelf) _rect.gameObject.SetActive(false);
                return;
            }

            if (!_rect.gameObject.activeSelf) _rect.gameObject.SetActive(true);
            _rect.position = TextPos;
        }
    }
}