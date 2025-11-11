using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class SelectGradeElement
    {
        public V2Enum_Grade V2Enum_Grade = V2Enum_Grade.Max;
        public Button Btn;
        public Image check;
        public TMP_Text text;

        [HideInInspector]
        public Toggle toggle;

        public System.Action<V2Enum_Grade> m_callback;

        private bool prevState = false;

        public void Init(System.Action<V2Enum_Grade> action)
        {
            if (Btn != null)
                Btn.onClick.AddListener(OnValueChange);

            m_callback = action;
        }

        public void OnValueChange()
        {
            m_callback?.Invoke(V2Enum_Grade);
        }

        public void SetText(string str)
        {
            if (text != null)
                text.SetText(str);
        }
    }

    public class InGameGradeSelectDialog : IDialog
    {
        [SerializeField]
        private List<SelectGradeElement> m_selectGradeElements;

        [SerializeField]
        private Button m_playAllCombine;

        [SerializeField]
        private List<Button> m_exitBtn;

        protected System.Action<List<V2Enum_Grade>> action;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }

            if (m_playAllCombine != null)
                m_playAllCombine.onClick.AddListener(OnClick_PlayAllCombine);

            Message.AddListener<GameBerry.Event.SetGradeSelectPopupMsg>(SetGradeSelectPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetGradeSelectPopupMsg>(SetGradeSelectPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            for (int i = 0; i < m_selectGradeElements.Count; ++i)
            {
                if (m_selectGradeElements[i].toggle != null)
                    m_selectGradeElements[i].toggle.isOn = false;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetGradeSelectPopup(GameBerry.Event.SetGradeSelectPopupMsg msg)
        {
            action = msg.SelectedCallBack;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlayAllCombine()
        {
            List<V2Enum_Grade> v2Enum_Grades = new List<V2Enum_Grade>();

            for (int i = 0; i < m_selectGradeElements.Count; ++i)
            {
                if (m_selectGradeElements[i].toggle != null)
                {
                    if (m_selectGradeElements[i].toggle.isOn == true)
                        v2Enum_Grades.Add(m_selectGradeElements[i].V2Enum_Grade);
                }
            }

            action?.Invoke(v2Enum_Grades);

            OnClick_ExitBtn();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            ElementExit();
        }
        //------------------------------------------------------------------------------------
    }
}