using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class Popup_OkCancel : IDialog
    {
        [SerializeField]
        private Transform m_contents;

        [SerializeField]
        private TMP_Text m_titleText = null;

        [SerializeField]
        private TMP_Text m_contentText = null;

        [SerializeField]
        private TMP_Text m_okBtnDefaultText = null;

        [SerializeField]
        private Transform m_okBtnCustomGroup = null;

        [SerializeField]
        private TMP_Text m_okBtnCustomTitle = null;

        [SerializeField]
        private Image m_okBtnCustomIcon = null;

        [SerializeField]
        private TMP_Text m_okBtnCustomContent = null;

        [SerializeField]
        private Button m_okBtn = null;

        [SerializeField]
        private TMP_Text m_okText = null;

        [SerializeField]
        private Button m_cancelBtn = null;

        [SerializeField]
        private TMP_Text m_cancelText = null;

        [SerializeField]
        private Toggle m_todayHideToggle;

        [SerializeField]
        private Transform m_todayHideToggleGroup;

        private System.Action m_okAction = null;

        private System.Action m_cancelAction = null;

        private System.Action<bool> m_toDayHide = null;

        private System.Action<Popup_OkCancel> m_endHideAction = null;

        private Vector3 originOkBtnPos = Vector3.zero;

        private Coroutine m_directionCoroutine = null;

        private List<Graphic> m_colorDirList = new List<Graphic>();

        private Vector3 m_minSize = 0.9f.ToVector3();

        private Vector3 m_maxSize = 1.0f.ToVector3();

        //------------------------------------------------------------------------------------
        public void Init()
        {
            if (m_okBtn)
            {
                originOkBtnPos = m_okBtn.transform.localPosition;
                m_okBtn.onClick.AddListener(OnClick_OkBtn);
            }

            if (m_cancelBtn)
                m_cancelBtn.onClick.AddListener(OnClick_cancelBtn);

            if (m_contents != null)
                m_colorDirList = m_contents.GetComponentsInAllChildren<Graphic>();
        }
        //------------------------------------------------------------------------------------
        public override void BackKeyCall()
        {
            if (m_cancelAction != null)
                m_cancelAction();

            Hide();
        }
        //------------------------------------------------------------------------------------
        public void Show(string titletext, string contenttext, System.Action okAction, System.Action<bool> toDayHide, System.Action<Popup_OkCancel> endHideAction)
        {
            if (m_titleText != null)
                m_titleText.text = titletext;

            if (m_contentText != null)
                m_contentText.text = contenttext;

            if (m_okBtnDefaultText != null)
                m_okBtnDefaultText.gameObject.SetActive(true);

            if (m_okBtnCustomGroup != null)
                m_okBtnCustomGroup.gameObject.SetActive(false);

            m_okAction = okAction;
            m_toDayHide = toDayHide;
            m_endHideAction = endHideAction;

            if (m_okBtn != null)
            {
                m_okBtn.gameObject.SetActive(true);
                Vector3 pos = m_okBtn.transform.localPosition;
                pos.x = 0.0f;
                m_okBtn.transform.localPosition = pos;
            }

            if (m_cancelBtn != null)
                m_cancelBtn.gameObject.SetActive(false);

            if (m_todayHideToggleGroup != null)
                m_todayHideToggleGroup.gameObject.SetActive(m_toDayHide != null);

            if (m_todayHideToggle != null)
                m_todayHideToggle.isOn = false;

            if (m_directionCoroutine == null)
                m_directionCoroutine = StartCoroutine(ShowDir());
        }
        //------------------------------------------------------------------------------------
        public void Show(
            string titletext, 
            string contenttext, 

            bool usecustomokbtn,
            string okbtntitle,
            Sprite okbtnicon,
            string okbtncontent,

            System.Action okAction, 
            System.Action cancelAction,
            System.Action<bool> toDayHide,
            System.Action<Popup_OkCancel> endHideAction)
        {
            if (m_titleText != null)
                m_titleText.text = titletext;

            if (m_contentText != null)
                m_contentText.text = contenttext;

            if (m_okBtnDefaultText != null)
                m_okBtnDefaultText.gameObject.SetActive(usecustomokbtn == false);

            if (m_okBtnCustomGroup != null)
                m_okBtnCustomGroup.gameObject.SetActive(usecustomokbtn == true);

            if (usecustomokbtn == true)
            {
                if (m_okBtnCustomTitle != null)
                    m_okBtnCustomTitle.text = okbtntitle;

                if (m_okBtnCustomIcon != null)
                    m_okBtnCustomIcon.sprite = okbtnicon;

                if (m_okBtnCustomContent != null)
                    m_okBtnCustomContent.text = okbtncontent;
            }

            m_okAction = okAction;
            m_cancelAction = cancelAction;
            m_toDayHide = toDayHide;
            m_endHideAction = endHideAction;

            if (m_okBtn != null)
            {
                m_okBtn.gameObject.SetActive(true);
                m_okBtn.transform.localPosition = originOkBtnPos;
            }

            if (m_cancelBtn != null)
                m_cancelBtn.gameObject.SetActive(true);

            if (m_todayHideToggleGroup != null)
                m_todayHideToggleGroup.gameObject.SetActive(m_toDayHide != null);

            if (m_todayHideToggle != null)
                m_todayHideToggle.isOn = false;

            if (m_directionCoroutine == null)
                m_directionCoroutine = StartCoroutine(ShowDir());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator ShowDir()
        {
            if (m_contents == null)
                yield break;

            float duration = 0.1f;
            float starttime = Time.time;
            float endtime = starttime + duration;

            float ratio = 0.0f;

            m_contents.localScale = m_minSize;

            Vector3 sizeGab = m_maxSize - m_minSize;

            Color color = new Color();

            for (int i = 0; i < m_colorDirList.Count; ++i)
            {
                color = m_colorDirList[i].color;
                color.a = 0.0f;
                m_colorDirList[i].color = color;
            }

            while (Time.time <= endtime)
            {
                ratio = MathDatas.Sin((90.0f * ((Time.time - starttime) / duration)));

                m_contents.localScale = m_minSize + (sizeGab * ratio);

                for (int i = 0; i < m_colorDirList.Count; ++i)
                {
                    color = m_colorDirList[i].color;
                    color.a = ratio;
                    m_colorDirList[i].color = color;
                }

                yield return null;
            }

            m_contents.localScale = m_maxSize;

            for (int i = 0; i < m_colorDirList.Count; ++i)
            {
                color = m_colorDirList[i].color;
                color.a = 1.0f;
                m_colorDirList[i].color = color;
            }

            m_directionCoroutine = null;
        }
        //------------------------------------------------------------------------------------
        private void Hide()
        {
            m_okAction = null;
            m_cancelAction = null;

            if (m_toDayHide != null)
            {
                if (m_todayHideToggle != null)
                    m_toDayHide(m_todayHideToggle.isOn);
                else
                    m_toDayHide(false);

                m_toDayHide = null;
            }

            if (m_directionCoroutine == null)
                m_directionCoroutine = StartCoroutine(HideDir());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator HideDir()
        {
            if (m_contents == null)
                yield break;

            float duration = 0.05f;
            float starttime = Time.time;
            float endtime = starttime + duration;

            float ratio = 0.0f;

            m_contents.localScale = m_maxSize;

            Vector3 sizeGab = m_maxSize;

            Color color = new Color();

            for (int i = 0; i < m_colorDirList.Count; ++i)
            {
                color = m_colorDirList[i].color;
                color.a = 1.0f;
                m_colorDirList[i].color = color;
            }

            while (Time.time <= endtime)
            {
                ratio = MathDatas.Sin((90.0f * ((Time.time - starttime) / duration)));

                m_contents.localScale = m_maxSize - (sizeGab * ratio);

                for (int i = 0; i < m_colorDirList.Count; ++i)
                {
                    color = m_colorDirList[i].color;
                    color.a = 1.0f - ratio;
                    m_colorDirList[i].color = color;
                }

                yield return null;
            }

            m_contents.localScale = 0.0f.ToVector3();

            for (int i = 0; i < m_colorDirList.Count; ++i)
            {
                color = m_colorDirList[i].color;
                color.a = 0.0f;
                m_colorDirList[i].color = color;
            }

            if (m_endHideAction != null)
                m_endHideAction(this);

            m_endHideAction = null;

            m_directionCoroutine = null;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_OkBtn()
        {
            if (m_okAction != null)
                m_okAction();

            Hide();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_cancelBtn()
        {
            if (m_cancelAction != null)
                m_cancelAction();

            Hide();
        }
        //------------------------------------------------------------------------------------
    }
}