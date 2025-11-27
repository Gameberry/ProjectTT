using UnityEngine;
using UnityEngine.UI;

namespace GameBerry
{
    public class UINumberBtn : MonoBehaviour
    {
        public int Num;

        public OnCallBack_Int AddListener;

        void Start()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    AddListener?.Invoke(Num);
                });
            }
        }
    }
}