using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIBlockUserElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_name;

        [SerializeField]
        private Button m_unBlockBtn;

        private int m_isBlockUid = -1;

        public void Init()
        {
            if (m_unBlockBtn != null)
                m_unBlockBtn.onClick.AddListener(OnClick_UnBlockBtn);
        }

        public void SetBlockUser(int uid, string username)
        {
            m_isBlockUid = uid;

            if (m_name != null)
                m_name.text = username;
        }

        private void OnClick_UnBlockBtn()
        {
            if (m_isBlockUid == -1)
                return;

            //Managers.ChatDataManager.Instance.API_Player_Chat_UnBlockRequest(m_isBlockUid);
        }
    }
}