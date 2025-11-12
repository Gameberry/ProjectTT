using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace GameBerry.UI
{
    public class GlobalPowerSavingDialog : IDialog
    {
        [SerializeField]
        private Image m_batteryPer_BGImage;

        [SerializeField]
        private TMP_Text m_batteryPer_Text;

        [SerializeField]
        private TMP_Text m_nowTime;

        [SerializeField]
        private TMP_Text m_today;

        [SerializeField]
        private TMP_Text m_currentFarmStage;

        [SerializeField]
        private Transform m_monSprite_Idle;

        [SerializeField]
        private Transform m_monSprite_Hit;

        [SerializeField]
        private UIPushBtn m_sliderPushBtn;

        [SerializeField]
        private Transform m_sliderIcon;

        [SerializeField]
        private RectTransform m_targetRectTr;

        [SerializeField]
        private Transform m_sliderStartPos;

        [SerializeField]
        private Transform m_sliderEndPos;

        private Coroutine m_refreshTimer = null;

        private WaitForSeconds m_refreshTimerTime = new WaitForSeconds(1.0f);

        private bool m_blockRefreshUILoop = false;


        private Coroutine m_playSlice = null;

        private bool m_doSlice = false;

        [SerializeField]
        private float m_unLockCheckDistance = 5.0f;

        private Camera m_myUICamera = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_sliderPushBtn != null)
            {
                m_sliderPushBtn.SetOnPushStart(StartSlice);
                m_sliderPushBtn.SetOnPushEnd(EndSlice);
                m_sliderPushBtn.SetDownDelay(0.0f);
            }

            m_myUICamera = UIManager.Instance.screenCanvasCamera;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            m_blockRefreshUILoop = true;
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            //UICharacterAniManager.Instance.SetAniResourceName(
            //    Define.PlayerSpriteResourceName
            //    , Managers.CharacterSkinManager.Instance.GetSkinBodyNumber()
            //    , ActorType.Knight);
            //UICharacterAniManager.Instance.PlayIdleAni();

            if (m_monSprite_Idle != null)
                m_monSprite_Idle.gameObject.SetActive(true);

            if (m_monSprite_Hit != null)
                m_monSprite_Hit.gameObject.SetActive(false);

            if (m_refreshTimer != null)
                StopCoroutine(m_refreshTimer);

            m_blockRefreshUILoop = false;

            m_refreshTimer = StartCoroutine(RefreshUICoroutine());

            m_doSlice = false;

            m_sliderIcon.transform.localPosition = m_sliderStartPos.transform.localPosition;
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (m_refreshTimer != null)
                StopCoroutine(m_refreshTimer);

            m_blockRefreshUILoop = true;
        }
        //------------------------------------------------------------------------------------
        private IEnumerator RefreshUICoroutine()
        {

            while (m_blockRefreshUILoop == false)
            {
                RefreshUI();
                yield return m_refreshTimerTime;
            }
            
            m_refreshTimer = null;
        }
        //------------------------------------------------------------------------------------
        private void RefreshUI()
        {
            float batterylevel = SystemInfo.batteryLevel;

            if (m_batteryPer_Text != null)
            {
                float batteryper = 1.0f;
                if (SystemInfo.batteryStatus != BatteryStatus.Unknown)
                {
                    batteryper = SystemInfo.batteryLevel;
                }

                if (m_batteryPer_BGImage != null)
                {
                    m_batteryPer_BGImage.fillAmount = batteryper;

                    Color color = Color.green;

                    if (batteryper < 0.3f)
                        color = Color.red;
                    else if(batteryper < 0.7f)
                        color = Color.yellow;

                    m_batteryPer_BGImage.color = color;
                }

                batteryper *= 100.0f;

                if (batteryper > 100.0f)
                    batteryper = 100.0f;
                else if (batteryper < 0.0f)
                    batteryper = 0.0f;

                batteryper = (float)Math.Truncate(batteryper);

                m_batteryPer_Text.text = string.Format("{0}%", batteryper);
            }

            DateTime dateTime = DateTime.Now;

            if (m_nowTime != null)
            {
                string ampm = dateTime.Hour >= 12 ? "pm" : "am";

                int hour = dateTime.Hour >= 12 ? dateTime.Hour - 12 : dateTime.Hour;
                if (hour == 0)
                    hour = 12;

                m_nowTime.text = string.Format("{0} {1:D2}:{2:D2}", ampm, hour, dateTime.Minute);
            }

            if (m_today != null)
            {
                m_today.text = string.Format("{0}/{1}/{2}", dateTime.Month, dateTime.Day, dateTime.Year);
            }
        }
        //------------------------------------------------------------------------------------
        private void StartSlice()
        {
            m_doSlice = true;

            if (m_playSlice != null)
                StopCoroutine(m_playSlice);

            m_playSlice = StartCoroutine(PlaySlice());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator PlaySlice()
        {
            Vector2 screenPoint;

            while (m_doSlice == true)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_targetRectTr, Input.mousePosition, m_myUICamera, out screenPoint);

                Vector2 mylocalpos = m_sliderIcon.localPosition;
                mylocalpos.x = screenPoint.x;

                if (mylocalpos.x < m_sliderStartPos.localPosition.x)
                    mylocalpos.x = m_sliderStartPos.localPosition.x;
                else if (mylocalpos.x > m_sliderEndPos.localPosition.x)
                    mylocalpos.x = m_sliderEndPos.localPosition.x;

                m_sliderIcon.localPosition = mylocalpos;

                float remainDistance = m_sliderEndPos.localPosition.x - m_sliderIcon.localPosition.x;

                if (m_monSprite_Idle != null)
                    m_monSprite_Idle.gameObject.SetActive(m_unLockCheckDistance < remainDistance);

                if (m_monSprite_Hit != null)
                    m_monSprite_Hit.gameObject.SetActive(m_unLockCheckDistance >= remainDistance);

                yield return null;
            }

            m_playSlice = null;
        }
        //------------------------------------------------------------------------------------
        private void EndSlice()
        {
            m_doSlice = false;

            if (m_playSlice != null)
                StopCoroutine(m_playSlice);

            float remainDistance = m_sliderEndPos.localPosition.x - m_sliderIcon.localPosition.x;

            if (m_unLockCheckDistance >= remainDistance)
            {
                UIManager.DialogExit<GlobalPowerSavingDialog>();
            }
            else
            {
                m_sliderIcon.transform.localPosition = m_sliderStartPos.transform.localPosition;

                if (m_monSprite_Idle != null)
                    m_monSprite_Idle.gameObject.SetActive(true);

                if (m_monSprite_Hit != null)
                    m_monSprite_Hit.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
    }
}