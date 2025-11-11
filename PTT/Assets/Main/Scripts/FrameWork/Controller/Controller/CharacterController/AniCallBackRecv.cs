using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class AniCallBackRecv : MonoBehaviour
    {
        [SerializeField]
        private CharacterControllerBase m_controller;

        public void AniBack(string str)
        {
            if (m_controller != null)
                m_controller.AniCallBackRecv(str);
        }
    }
}