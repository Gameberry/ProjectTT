using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    
    public class UnityUpdateManager : MonoSingleton<UnityUpdateManager>
    {
        public delegate void UpdateDelegate();

        public event UpdateDelegate UpdateFunc;
        public event UpdateDelegate LateUpdateFunc;

        public event UpdateDelegate FirstFixedUpdateFunc;
        public event UpdateDelegate SecondFixedUpdateFunc;

        public event UpdateDelegate LateFixedUpdateFunc;

        public event UpdateDelegate UpdateCoroutineFunc_1Sec;

        private WaitForSeconds m_waitForSeconds_1Sec = new WaitForSeconds(1.0f);


        public event UpdateDelegate UpdateCoroutineFunc_HalfSec;

        private WaitForSeconds m_waitForSeconds_HalfSec = new WaitForSeconds(0.5f);

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            StartCoroutine(UpdateCoroutine());
            StartCoroutine(UpdateHalfSecCoroutine());
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            UpdateFunc?.Invoke();
        }
        //------------------------------------------------------------------------------------
        private void LateUpdate()
        {
            LateUpdateFunc?.Invoke();
        }
        //------------------------------------------------------------------------------------
        private void FixedUpdate()
        {
            FirstFixedUpdateFunc?.Invoke();
            SecondFixedUpdateFunc?.Invoke();

            LateFixedUpdateFunc?.Invoke();
        }
        //------------------------------------------------------------------------------------
        private IEnumerator UpdateCoroutine()
        {
            while (isAlive == true)
            {
                UpdateCoroutineFunc_1Sec?.Invoke();
                yield return m_waitForSeconds_1Sec;
            }
        }
        //------------------------------------------------------------------------------------
        private IEnumerator UpdateHalfSecCoroutine()
        {
            while (isAlive == true)
            {
                UpdateCoroutineFunc_HalfSec?.Invoke();
                yield return m_waitForSeconds_HalfSec;
            }
        }
        //------------------------------------------------------------------------------------
    }
}