using UnityEngine;
using UnityEngine.UI;

namespace GameBerry
{
    public class UINumberBtn : MonoBehaviour
    {
        public int Num;

        public OnCallBack_Int AddListener;

        // 선택 상태 시 색 바꾸고 싶으면 여기서 처리
        [SerializeField] private GameObject _selectedIndicator;

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

        public void SetSelected(bool selected)
        {
            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(selected);
        }
    }
}