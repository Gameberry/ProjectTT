using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace GameBerry.UI
{
    public class InGameGuideInteractorDialog : IDialog
    {
        [SerializeField]
        private GameObject GuideQuestClicker;

        private UIGuideQuestClicker m_uIGuideQuestClicker = null;
        private UIGuideInteractor m_targetInteractor;
        private bool m_visible = false;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.VisibleGuideInteractorFocusMsg>(VisibleGuideInteractorFocus);

            GameObject clone = Instantiate(GuideQuestClicker, dialogView.transform);

            if (clone != null)
            {
                m_uIGuideQuestClicker = clone.GetComponent<UIGuideQuestClicker>();
                //if (m_uIGuideQuestClicker != null)
                //    m_uIGuideQuestClicker.SetSprite(StaticResource.Instance.GetIcon("guide/Guide_hand00/sprite"), StaticResource.Instance.GetIcon("guide/Guide_hand_E1_00/sprite"));

                clone.SetActive(false);
            }

            //AssetBundleLoader.Instance.Load<GameObject>("FX_Resources", "UI_QuestClick", o =>
            //{
            //    GameObject obj = o as GameObject;

            //    GameObject clone = Instantiate(obj, dialogView.transform);

            //    if (clone != null)
            //    {
            //        m_uIGuideQuestClicker = clone.GetComponent<UIGuideQuestClicker>();
            //        if (m_uIGuideQuestClicker != null)
            //            m_uIGuideQuestClicker.SetSprite(StaticResource.Instance.GetIcon("guide/Guide_hand00/sprite"), StaticResource.Instance.GetIcon("guide/Guide_hand_E1_00/sprite"));

            //        clone.SetActive(false);
            //    }
            //});
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.VisibleGuideInteractorFocusMsg>(VisibleGuideInteractorFocus);
        }
        //------------------------------------------------------------------------------------
        private void LateUpdate()
        {
            if (m_visible == false || m_targetInteractor == null || m_uIGuideQuestClicker == null)
                return;

            m_uIGuideQuestClicker.gameObject.transform.position = m_targetInteractor.gameObject.transform.position;
        }
        //------------------------------------------------------------------------------------
        private void VisibleGuideInteractorFocus(GameBerry.Event.VisibleGuideInteractorFocusMsg msg)
        {
            if (m_uIGuideQuestClicker == null)
                return;

            m_targetInteractor = msg.uIGuideInteractor;
            m_visible = msg.visible;

            if (m_visible == true)
            {
                if (m_targetInteractor == null)
                {
                    m_uIGuideQuestClicker.gameObject.SetActive(false);
                    return;
                }

                m_uIGuideQuestClicker.transform.SetParent(m_targetInteractor.FocusParent != null ? m_targetInteractor.FocusParent : dialogView.transform);
                m_uIGuideQuestClicker.gameObject.SetActive(true);
                m_uIGuideQuestClicker.transform.ResetLocal();
                m_uIGuideQuestClicker.SetHandAngle(m_targetInteractor.FocusAngle);
            }
            else
            {
                m_uIGuideQuestClicker.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
    }
}