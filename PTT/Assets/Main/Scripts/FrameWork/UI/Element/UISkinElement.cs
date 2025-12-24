using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISkinElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        private System.Action<Chart.SkinInfo> _action;
        public Chart.SkinInfo _skinInfo = null;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<Chart.SkinInfo> action)
        {
            _action = action;

            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void SetSkinInfo(Chart.SkinInfo skinInfo)
        {
            _skinInfo = skinInfo;

            if (skinInfo == null)
            {
                if (_label != null)
                {
                    _label.text = "Reset";
                    _label.color = Color.white;
                }
            }
            else
            {
                Table.SkinData skinData = Managers.SkinManager.Instance.GetSkinData(skinInfo.ItemId);

                if (_label != null)
                { 
                    _label.text = _skinInfo.SkinName;
                    _label.color = (skinData == null || skinData.unlocked == false) ? Color.red : Color.white;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            _action?.Invoke(_skinInfo);
        }
        //------------------------------------------------------------------------------------
    }
}
