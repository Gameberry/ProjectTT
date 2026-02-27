using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Common;

namespace GameBerry.UI
{
    public class GlobalPopupDialog : IDialog
    {
        private ObjectPool<Popup_OkCancel> m_okCancelPool = null;
        private ObjectPool<Popup_Input> m_inputPool = null;

        [SerializeField]
        private GameObject m_okCancelObj = null;
        [SerializeField]
        private GameObject m_inputObj = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            //AssetBundleLoader.Instance.Load<GameObject>("ContentResources/GlobalContent", "Popup_OkCancel", o =>
            //{
            //    m_okCancelObj = o as GameObject;
            //});

            //AssetBundleLoader.Instance.Load<GameObject>("ContentResources/GlobalContent", "Popup_Input", o =>
            //{
            //    m_inputObj = o as GameObject;
            //});

            m_okCancelPool = new ObjectPool<Popup_OkCancel>();
            m_inputPool = new ObjectPool<Popup_Input>();

            CreateOkCancelPopup();
            CreateInputPopup();

            Message.AddListener<GameBerry.Event.ShowPopup_OkMsg>(ShowPopup_Ok);
            Message.AddListener<GameBerry.Event.ShowPopup_OkCancelMsg>(ShowPopup_OkCancel);
            Message.AddListener<GameBerry.Event.ShowPopup_InputMsg>(ShowPopup_Input);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ShowPopup_OkMsg>(ShowPopup_Ok);
            Message.RemoveListener<GameBerry.Event.ShowPopup_OkCancelMsg>(ShowPopup_OkCancel);
            Message.RemoveListener<GameBerry.Event.ShowPopup_InputMsg>(ShowPopup_Input);
        }
        //------------------------------------------------------------------------------------
        private void ShowPopup_Ok(GameBerry.Event.ShowPopup_OkMsg msg)
        {
            Popup_OkCancel popup = m_okCancelPool.GetObject() ?? CreateOkCancelPopup();
            popup.ElementEnter();
            popup.Show(msg.titletext, msg.contenttext, msg.okAction, msg.toDayHide, PopOkCancelObject);
        }
        //------------------------------------------------------------------------------------
        private void ShowPopup_OkCancel(GameBerry.Event.ShowPopup_OkCancelMsg msg)
        {
            Popup_OkCancel popup = m_okCancelPool.GetObject() ?? CreateOkCancelPopup();
            popup.ElementEnter();
            popup.Show(
                msg.titletext, 
                msg.contenttext,

                msg.useCustomOKBtn,
                msg.OKBtnTitle,
                msg.OKBtnIcon,
                msg.OKBtnContent,

                msg.okAction, 
                msg.cancelAction, 
                msg.toDayHide, 
                PopOkCancelObject);
        }
        //------------------------------------------------------------------------------------
        private void ShowPopup_Input(GameBerry.Event.ShowPopup_InputMsg msg)
        {
            Popup_Input popup = m_inputPool.GetObject() ?? CreateInputPopup();
            popup.gameObject.SetActive(true);

            popup.Show(msg.titletext, msg.defaultstr, msg.placeholder, msg.okAction, msg.cancelAction, msg.toDayHide, PopObject);
        }
        //------------------------------------------------------------------------------------
        private Popup_OkCancel CreateOkCancelPopup()
        {
            GameObject obj = Instantiate(m_okCancelObj, transform);

            Popup_OkCancel pop = obj.GetComponent<Popup_OkCancel>();
            pop.Init();
            m_okCancelPool.PoolObject(pop);
            obj.SetActive(false);

            return pop;
        }
        //------------------------------------------------------------------------------------
        private Popup_Input CreateInputPopup()
        {
            GameObject obj = Instantiate(m_inputObj, transform);

            Popup_Input pop = obj.GetComponent<Popup_Input>();
            pop.Init();
            m_inputPool.PoolObject(pop);
            obj.SetActive(false);

            return pop;
        }
        //------------------------------------------------------------------------------------
        private void PopOkCancelObject(Popup_OkCancel popup)
        {
            popup.ElementExit();
            m_okCancelPool.PoolObject(popup);
        }
        //------------------------------------------------------------------------------------
        private void PopObject(Popup_Input popup)
        {
            popup.gameObject.SetActive(false);
            m_inputPool.PoolObject(popup);
        }
        //------------------------------------------------------------------------------------
    }
}