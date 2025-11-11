using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace GameBerry
{
    public class UIIntButtonElement : MonoBehaviour
    {
        [SerializeField]
        private Button _btn;

        public Button Btn { get { return _btn; } }

        [SerializeField]
        private TMP_Text _text;

        public TMP_Text Text { get { return _text; } }


        System.Action<int> _action;
        private int _myInt;

        private void Awake()
        {
            if (_btn != null)
                _btn.onClick.AddListener(OnClick);
        }

        public void SetCallBack(System.Action<int> action)
        {
            _action = action;
        }

        public void SetInt(int myint)
        {
            _myInt = myint;
        }

        public void OnClick()
        {
            _action?.Invoke(_myInt);
        }
    }
}