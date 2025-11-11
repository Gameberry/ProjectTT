using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public class UIBackKeyExtensionElement : IDialog
    {
        public System.Action m_callback = null;

        public override void BackKeyCall()
        {
            if (m_callback != null)
                m_callback();

            base.BackKeyCall();
        }
    }
}