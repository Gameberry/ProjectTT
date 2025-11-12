using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace GameBerry.UI
{
    public class InGameGuideNPCNoticeDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private TMP_Text m_guideNPCDialog;

        [SerializeField]
        private float m_dialogSpeed = 0.05f;

        private float checkDelay = 0.5f;
        private float checkInputTouchCount = 0.0f;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < m_exitBtn.Count; ++i)
            {
                if (m_exitBtn[i] != null)
                    m_exitBtn[i].onClick.AddListener(OnClick_Exit);
            }

            Message.AddListener<GameBerry.Event.SetGuideDialogMsg>(SetGuideDialog);

            Managers.UnityUpdateManager.Instance.UpdateFunc += UpdateFunc;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetGuideDialogMsg>(SetGuideDialog);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            checkInputTouchCount = Time.time + checkDelay;
        }
        //------------------------------------------------------------------------------------
        private void SetGuideDialog(GameBerry.Event.SetGuideDialogMsg msg)
        {
            if (msg.guideTutorialData == null)
                return;

            GuideTutorialData guideTutorialData = msg.guideTutorialData;

            if (m_guideNPCDialog != null)
            {
                m_guideNPCDialog.DOKill();

                m_guideNPCDialog.SetText(string.Empty);

                string dialog = string.Empty;

                if (guideTutorialData.TutorialStr.ContainsKey(Managers.LocalStringManager.Instance.GetLocalizeType()) == true)
                {
                    dialog = guideTutorialData.TutorialStr[Managers.LocalStringManager.Instance.GetLocalizeType()];
                }

                float duration = dialog.Length * m_dialogSpeed;
                m_guideNPCDialog.DOText(dialog, duration);
            }
        }
        //------------------------------------------------------------------------------------
        private void UpdateFunc()
        {
            if (isEnter == false)
                return;

            if (checkInputTouchCount > Time.time)
                return;

#if UNITY_EDITOR
            if(Input.GetMouseButtonDown(0))
                OnClick_Exit();
#else
            bool touchOn = Input.touchCount > 0;

            if (touchOn == true)
                OnClick_Exit();
#endif
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Exit()
        {
            UIManager.DialogExit<InGameGuideNPCNoticeDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}