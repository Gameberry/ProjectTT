using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UITermsDescElement : MonoBehaviour
    {
        [SerializeField]
        private Button showPolicy;

        private void Awake()
        {
            if (showPolicy != null)
                showPolicy.onClick.AddListener(Managers.SceneManager.Instance.GoTerms);
        }
    }
}