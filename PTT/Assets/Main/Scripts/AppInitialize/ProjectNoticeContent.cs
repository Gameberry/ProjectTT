using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;

namespace GameBerry
{
    public class ProjectNoticeContent : MonoSingleton<ProjectNoticeContent>
    {
        [SerializeField]
        private ProjectNoticeCheckDialog projectNoticeCheckDialog;

        [SerializeField]
        private ProjectNoticeNeedUpdateDialog projectNoticeNeedUpdateDialog;

        [SerializeField]
        private ProjectNoticeExitGameDialog projectNoticeExitGameDialog;

        [SerializeField]
        private ProjectNoticeGooglePlayServiceUpdateDialog projectNoticeGooglePlayServiceUpdateDialog;


        public void ShowCheckDialog(string checktime = "", bool enablebackBtn = true)
        {
            if (projectNoticeCheckDialog != null)
            {
                projectNoticeCheckDialog.gameObject.SetActive(true);
                projectNoticeCheckDialog.SetCheckTime(checktime);
            }

            //if (enablebackBtn == true)
            //{
            //    Managers.AOSBackBtnManager.Instance.EnterBackBtnAction(() =>
            //    {
            //        if (projectNoticeCheckDialog != null)
            //            projectNoticeCheckDialog.gameObject.SetActive(false);
            //    });
            //}
        }
        //------------------------------------------------------------------------------------
        public void ShowNeedUpdateDialog(bool enablebackBtn = true)
        {
            if (projectNoticeNeedUpdateDialog != null)
                projectNoticeNeedUpdateDialog.gameObject.SetActive(true);

            //if (enablebackBtn == true)
            //{
            //    Managers.AOSBackBtnManager.Instance.EnterBackBtnAction(() =>
            //    {
            //        if (projectNoticeNeedUpdateDialog != null)
            //            projectNoticeNeedUpdateDialog.gameObject.SetActive(false);
            //    });
            //}
        }
        //------------------------------------------------------------------------------------
        public void ShowExitGameDialog()
        {
            if (projectNoticeExitGameDialog != null)
                projectNoticeExitGameDialog.ElementEnter();
                //projectNoticeExitGameDialog.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        public void HideExitGameDialog()
        {
            if (projectNoticeExitGameDialog != null)
                projectNoticeExitGameDialog.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        public void ShowGooglePlayServiceUpdate()
        {
            if (projectNoticeGooglePlayServiceUpdateDialog != null)
                projectNoticeGooglePlayServiceUpdateDialog.ElementEnter();
        }
        //------------------------------------------------------------------------------------
        public void Show_TermsView()
        {
            Managers.SceneManager.Instance.GoTerms();
        }
        //------------------------------------------------------------------------------------
        public void Show_PrivacyView()
        {
            Managers.SceneManager.Instance.GoPrivacy();
        }
        //------------------------------------------------------------------------------------
    }
}