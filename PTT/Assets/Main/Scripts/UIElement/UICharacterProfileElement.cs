using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UICharacterProfileElement : InfiniteScrollItem
    {
        [SerializeField]
        private Button elementbtn;

        [SerializeField]
        private Image image;

        [SerializeField]
        private Transform selectMark;

        [SerializeField]
        private Image selectMark_Image;

        [SerializeField]
        private Image disable_Image;

        private CharacterProfileData characterProfileData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (elementbtn != null)
                elementbtn.onClick.AddListener(OnSelect);
        }
        //------------------------------------------------------------------------------------
        public override void UpdateData(InfiniteScrollData scrollData)
        {
            CharacterProfileData characterProfileData = scrollData as CharacterProfileData;
            if (characterProfileData == null)
                return;

            if (characterProfileData == Managers.PlayerDataManager.Instance.GetCurrentProfile())
                SetFocus();
            else
                SetReleaseFocus();

            disable_Image?.gameObject.SetActive(!characterProfileData.isEnable);

            if (image != null)
                image.sprite = characterProfileData.sprite;
        }
        //------------------------------------------------------------------------------------
        public /*override */void SetFocus()
        {
            SetSelect(true);
        }
        //------------------------------------------------------------------------------------
        public /*override */void SetReleaseFocus()
        {
            SetSelect(false);
        }
        //------------------------------------------------------------------------------------
        public void SetSelect(bool select)
        {
            if (selectMark_Image != null)
                selectMark_Image.gameObject.SetActive(select);

            if (selectMark != null)
            {
                selectMark.gameObject.SetActive(select);
            }
        }
        //------------------------------------------------------------------------------------
    }
}