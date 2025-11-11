using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIGuideInteractor : MonoBehaviour
    {
        public V2Enum_EventType MyGuideType = V2Enum_EventType.Max;

        // 닫기버튼 같은건 일부러 -1을 쓴다
        public int MyStepID = -1;

        public Managers.GuideInteratorActionType GuideInteratorActionType = Managers.GuideInteratorActionType.None;

        public bool IsEnableShow = false;

        public bool FocusOn = true;
        public float FocusAngle = 0.0f;

        public int JumpIdx;

        public bool ShowHighlightFollower = false;

        // 인스펙터창에서 셋팅한건 true, 로직에서 셋팅한건 false
        public bool IsAutoSetting = true;

        public Transform FocusParent = null;

        private void Awake()
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OnClick_Interactor);

            UIPushBtn uIPushBtn = GetComponent<UIPushBtn>();
            if (uIPushBtn != null)
            {
                uIPushBtn.SetOnClick(OnClick_Interactor);
                uIPushBtn.SetOnPush(OnClick_Interactor);
            }

            if (IsAutoSetting == true)
                ConnectInteractor();
        }

        public void ConnectInteractor()
        {
            Managers.GuideInteractorManager.Instance.ConnecctGuideInteractor(this);
        }

        public void OnClick_Interactor()
        {
            Managers.GuideInteractorManager.Instance.OnClick_GuideInteractor(this);
        }
    }
}