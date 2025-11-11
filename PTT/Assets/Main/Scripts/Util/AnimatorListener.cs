using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace GameBerry
{
    public class AnimatorListener : MonoBehaviour
    {
        public event OnCallBack OnCallBack;
        public event OnCallBack_String OnCallBack_String;

        public void CallBack()
        {
            OnCallBack?.Invoke();
        }

        public void CallBack_String(string str)
        {
            OnCallBack_String?.Invoke(str);
        }
    }
}