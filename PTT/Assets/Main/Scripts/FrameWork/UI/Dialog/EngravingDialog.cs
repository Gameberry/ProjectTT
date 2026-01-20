using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Table;

namespace GameBerry.UI
{
    /// <summary>
    /// 각인 메인 다이얼로그
    /// 10개 스테이지 패널 표시, 각인 실행, 확률 팝업 연결
    /// </summary>
    public class EngravingDialog : IDialog
    {
        [Header("Stage Container")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _stageContainer;
        [SerializeField] private UIEngravingStageElement _stagePrefab;

        private UIEngravingStageElement[] _stageElements;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            InitializeStages();
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            EngravingManager.Instance.OnEngravingChanged += Refresh;
            Refresh();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (EngravingManager.Instance != null)
                EngravingManager.Instance.OnEngravingChanged -= Refresh;
        }
        //------------------------------------------------------------------------------------
        private void InitializeStages()
        {
            _stageElements = new UIEngravingStageElement[EngravingTable.MaxStage];

            for (int i = 0; i < EngravingTable.MaxStage; i++)
            {
                int stage = i + 1;
                var element = Instantiate(_stagePrefab, _stageContainer);
                element.Init(stage, OnEngraveClicked, OnProbabilityClicked);
                _stageElements[i] = element;
            }
        }
        //------------------------------------------------------------------------------------
        private void Refresh()
        {
            for (int i = 0; i < EngravingTable.MaxStage; i++)
            {
                int stage = i + 1;
                var stageData = EngravingManager.Instance.GetEngraving(stage);
                _stageElements[i].UpdatePanel(stageData);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnEngraveClicked(int stage)
        {
            int result = EngravingManager.Instance.Roll(stage);

            if (result > 0)
            {
                StartCoroutine(ShowResultRoutine(stage, 0.1f));
            }
        }
        //------------------------------------------------------------------------------------
        private void OnProbabilityClicked(int stage)
        {
            UIManager.Instance.DialogEnter<EngravingProbabilityDialog>();
            EngravingProbabilityDialog engravingProbabilityDialog = UIManager.Get<EngravingProbabilityDialog>();
            if (engravingProbabilityDialog != null)
                engravingProbabilityDialog.Init(stage);
        }
        //------------------------------------------------------------------------------------
        private IEnumerator ShowResultRoutine(int stage, float delay)
        {
            yield return new WaitForSeconds(delay);

            Refresh();

            var stageData = EngravingManager.Instance.GetEngraving(stage);
            bool isMatching = stageData != null && stageData.HasMatchingStats();

            // TODO: 결과 팝업 표시
            // UISystem.Instance.Open<EngravingResultDialog>().Init(stageData, isMatching);

            if (isMatching && stage < EngravingTable.MaxStage)
            {
                ScrollToStage(stage + 1);
            }
        }
        //------------------------------------------------------------------------------------
        private void ScrollToStage(int stage)
        {
            if (_scrollRect == null || _stageElements == null)
                return;

            int index = stage - 1;
            if (index < 0 || index >= _stageElements.Length)
                return;

            var targetRect = _stageElements[index].GetComponent<RectTransform>();
            var contentRect = _stageContainer.GetComponent<RectTransform>();

            if (targetRect == null || contentRect == null)
                return;

            Canvas.ForceUpdateCanvases();

            float targetX = -targetRect.anchoredPosition.x;
            float viewportWidth = _scrollRect.viewport.rect.width;
            float contentWidth = contentRect.rect.width;

            float normalizedPosition = Mathf.Clamp01(targetX / (contentWidth - viewportWidth));
            _scrollRect.horizontalNormalizedPosition = normalizedPosition;
        }
        //------------------------------------------------------------------------------------
    }
}
