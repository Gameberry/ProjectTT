using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameBerry.UI
{
    [RequireComponent(typeof(EventTrigger))]
    public class UIPushBtn : MonoBehaviour
    {
        private EventTrigger m_eventTrigger;
        private EventTrigger.Entry m_pointDownEntry;
        private EventTrigger.Entry m_pointUpEntry;

        private float m_pointDownStartTime = 0.0f;
        private float m_pointDownDelay = 0.5f;
        private float m_pointDownTurm = 0.01f;
        private float m_pointDownTurmTimer = 0.0f;

        private bool m_isPointDown = false;
        private bool m_isOnPush = false;

        private System.Action m_OnPushStartCallBack;
        private System.Action m_OnPushCallBack;
        private System.Action m_OnPushEndCallBack;
        private System.Action m_OnClickCallBack;

        private Image m_btnImage;
        private Sprite m_normalSprite;

        [SerializeField]
        private Sprite m_pressSprite;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            Managers.UnityUpdateManager.Instance.UpdateFunc += UpdateFunc;
        }
        //------------------------------------------------------------------------------------
        void Start()
        {
            m_eventTrigger = GetComponent<EventTrigger>();
            m_btnImage = GetComponent<Image>();

            if (m_btnImage != null)
                m_normalSprite = m_btnImage.sprite;

            m_pointDownEntry = new EventTrigger.Entry();
            m_pointDownEntry.eventID = EventTriggerType.PointerDown;
            m_pointDownEntry.callback.AddListener(OnPointDown_UpGrade);

            m_pointUpEntry = new EventTrigger.Entry();
            m_pointUpEntry.eventID = EventTriggerType.PointerUp;
            m_pointUpEntry.callback.AddListener(OnPointUp_UpGrade);

            m_eventTrigger.triggers.Add(m_pointDownEntry);
            m_eventTrigger.triggers.Add(m_pointUpEntry);
        }
        //------------------------------------------------------------------------------------
        public int pushcount = 0;
        private void UpdateFunc()
        {
            if (m_isPointDown == true)
            {
                if (m_pointDownStartTime + m_pointDownDelay < Time.time)
                {
                    if (m_isOnPush == false)
                    {
                        m_isOnPush = true;
                        pushcount = 0;
                        OnPushStart();
                    }

                    if (m_pointDownTurmTimer > m_pointDownTurm)
                    {
                        OnPush();
                        pushcount++;
                        
                        m_pointDownTurmTimer = 0.0f;
                    }
                    else
                    {
                        m_pointDownTurmTimer += Time.deltaTime;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetDownDelay(float delay)
        {
            m_pointDownDelay = delay;
        }
        //------------------------------------------------------------------------------------
        private void OnDisable()
        {
            if (m_isPointDown == true)
            {
                OnPointUp_UpGrade(null);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetOnPushStart(System.Action action)
        {
            m_OnPushStartCallBack += action;
        }
        //------------------------------------------------------------------------------------
        public void SetOnPush(System.Action action)
        {
            m_OnPushCallBack += action;
        }
        //------------------------------------------------------------------------------------
        public void SetOnPushEnd(System.Action action)
        {
            m_OnPushEndCallBack += action;
        }
        //------------------------------------------------------------------------------------
        public void SetOnClick(System.Action action)
        {
            m_OnClickCallBack += action;
        }
        //------------------------------------------------------------------------------------
        private void OnPointDown_UpGrade(BaseEventData baseEventData)
        {
            m_isPointDown = true;
            m_pointDownStartTime = Time.time;
            m_pointDownTurmTimer = 0.0f;
        }
        //------------------------------------------------------------------------------------
        private void OnPointUp_UpGrade(BaseEventData baseEventData)
        {
            m_isPointDown = false;
            m_isOnPush = false;
            if (m_pointDownStartTime + m_pointDownDelay > Time.time)
                OnClick();
            else
                OnPushEnd();
        }
        //------------------------------------------------------------------------------------
        private void OnPushStart()
        {
            m_OnPushStartCallBack?.Invoke();

            if (m_btnImage != null && m_pressSprite != null)
                m_btnImage.sprite = m_pressSprite;
        }
        //------------------------------------------------------------------------------------
        private void OnPush()
        {
            if (m_OnPushCallBack != null)
            { 
                m_OnPushCallBack.Invoke();

                int addpush = pushcount;
                addpush = pushcount / 10;

                for (int i = 0; i < addpush; ++i)
                {
                    m_OnPushCallBack.Invoke();
                }
            }

            if (Managers.SoundManager.isAlive == true)
            {
                Managers.SoundManager.Instance.PlaySound("Button 5");
            }
        }
        //------------------------------------------------------------------------------------
        private void OnPushEnd()
        {
            m_OnPushEndCallBack?.Invoke();

            if (Managers.SoundManager.isAlive == true)
            {
                Managers.SoundManager.Instance.PlaySound("Button 5");
            }

            if (m_btnImage != null && m_normalSprite != null)
                m_btnImage.sprite = m_normalSprite;
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            m_OnClickCallBack?.Invoke();

            if (Managers.SoundManager.isAlive == true)
            {
                Managers.SoundManager.Instance.PlaySound("Button 5");
            }
        }
        //------------------------------------------------------------------------------------
    }
}

