using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UISynergyCombineStackElement : MonoBehaviour
    {
        [SerializeField]
        private Image _gambleSynergyIcon;

        [SerializeField]
        private TMP_Text _gambleSynergyStack;

        [SerializeField]
        private Image _readyCheckImage;

        public void SetSynergyStack(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType, int stack)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_ARR_GambleSynergyType);
            if (gambleCardSprite != null)
            {
                if (_gambleSynergyIcon != null)
                    _gambleSynergyIcon.sprite = gambleCardSprite.SynergyIcon;
            }

            int currentStack = Managers.SynergyManager.Instance.GetSynergyStack(v2Enum_ARR_GambleSynergyType);
            bool ready = stack <= currentStack;

            if (_gambleSynergyStack != null)
            { 
                _gambleSynergyStack.SetText("{0}/{1}", currentStack, stack);
                _gambleSynergyStack.color = ready == true ? Color.green : Color.red;
            }

            if (_readyCheckImage != null)
                _readyCheckImage.gameObject.SetActive(ready);
        }

        public void SetDescendSynergyStack(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType, int stack)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_ARR_GambleSynergyType);
            if (gambleCardSprite != null)
            {
                if (_gambleSynergyIcon != null)
                    _gambleSynergyIcon.sprite = gambleCardSprite.SynergyIcon;
            }

            if (_gambleSynergyStack != null)
            {
                _gambleSynergyStack.SetText("{0}", stack);
                _gambleSynergyStack.color = Color.white;
            }

            if (_readyCheckImage != null)
                _readyCheckImage.gameObject.SetActive(false);
        }

        public void SetDescendSynergyStack(V2Enum_ARR_SynergyType v2Enum_ARR_GambleSynergyType)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_ARR_GambleSynergyType);
            if (gambleCardSprite != null)
            {
                if (_gambleSynergyIcon != null)
                    _gambleSynergyIcon.sprite = gambleCardSprite.SynergyIcon;
            }

            if (_gambleSynergyStack != null)
                _gambleSynergyStack.gameObject.SetActive(false);

            if (_readyCheckImage != null)
                _readyCheckImage.gameObject.SetActive(false);
        }
    }
}