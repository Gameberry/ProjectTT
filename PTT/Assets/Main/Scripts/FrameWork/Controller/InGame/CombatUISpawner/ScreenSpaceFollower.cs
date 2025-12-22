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
        public Vector2 ScreenPixelOffset { get; set; }

        [SerializeField] private bool hideWhenBehindCamera = true;

        [Min(1)]
        [SerializeField] private int updateEveryNFrames = 2;

        [SerializeField] private float worldMoveEpsilon = 0.0004f;

        [Range(0.9f, 0.999999f)]
        [SerializeField] private float camRotDotThreshold = 0.99995f;

        private RectTransform _rect;

        private Camera _battlecam;
        private Camera _uicam;

        private int _frameCounter;

        private Vector3 _cachedScreenPos;
        private bool _cachedVisible;

        private Vector3 _lastTargetPos;
        private Vector3 _lastCamPos;
        private Vector3 _lastCamForward;

        private bool _dirtyThisFrame;

        void Awake()
        {
            _rect = (RectTransform)transform;

            _battlecam = Managers.BattleSceneManager.Instance.GetBattleSceneCamera().BattleCamera;
            _uicam = UI.UIManager.Instance.screenCanvasCamera;

            enabled = false;
        }

        public void SetTarget(Transform target, Vector3 worldOffset)
        {
            Target = target;
            WorldOffset = worldOffset;
            ScreenPixelOffset = Vector2.zero;

            _frameCounter = 0;
            _cachedScreenPos = Vector3.zero;
            _cachedVisible = true;

            if (Target != null)
                _lastTargetPos = Target.position;

            if (_battlecam != null)
            {
                _lastCamPos = _battlecam.transform.position;
                _lastCamForward = _battlecam.transform.forward;
            }

            _dirtyThisFrame = true;

            enabled = (target != null);
            ForceUpdate();
            ApplyCachedToUI();
        }

        public void ClearTarget()
        {
            Target = null;
            enabled = false;
        }

        public void MarkDirty()
        {
            _dirtyThisFrame = true;
        }

        void LateUpdate()
        {
            if (Target == null) return;

            bool needUpdate =
                _dirtyThisFrame ||
                HasMovedSignificantly() ||
                ShouldUpdateByFrameSkip();

            if (needUpdate)
            {
                _frameCounter = 0;
                ForceUpdate();
            }

            ApplyCachedToUI();
            _dirtyThisFrame = false;
        }

        bool ShouldUpdateByFrameSkip()
        {
            _frameCounter++;
            if (_frameCounter >= updateEveryNFrames)
            {
                _frameCounter = 0;
                return true;
            }
            return false;
        }

        bool HasMovedSignificantly()
        {
            Vector3 tp = Target.position;
            Vector3 dpT = tp - _lastTargetPos;
            float movedTargetSqr = dpT.sqrMagnitude;

            Transform ct = _battlecam.transform;
            Vector3 cp = ct.position;
            Vector3 dpC = cp - _lastCamPos;
            float movedCamSqr = dpC.sqrMagnitude;

            Vector3 fwd = ct.forward;
            float dot = Vector3.Dot(fwd, _lastCamForward);

            bool moved =
                movedTargetSqr > worldMoveEpsilon ||
                movedCamSqr > worldMoveEpsilon ||
                dot < camRotDotThreshold;

            if (moved)
            {
                _lastTargetPos = tp;
                _lastCamPos = cp;
                _lastCamForward = fwd;
            }

            return moved;
        }

        void ForceUpdate()
        {
            Vector3 sp = _battlecam.WorldToScreenPoint(Target.position + WorldOffset);
            Vector3 TextPos = _uicam.ScreenToWorldPoint(sp);
            bool visible = !(hideWhenBehindCamera && sp.z < 0f);

            TextPos.x += ScreenPixelOffset.x;
            TextPos.y += ScreenPixelOffset.y;

            _cachedScreenPos = TextPos;
            _cachedVisible = visible;
        }

        void ApplyCachedToUI()
        {
            if (!_cachedVisible)
            {
                if (_rect.gameObject.activeSelf) _rect.gameObject.SetActive(false);
                return;
            }

            if (!_rect.gameObject.activeSelf) _rect.gameObject.SetActive(true);
            _rect.position = _cachedScreenPos;
        }
    }
}