using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    [System.Serializable]
    public class UIGambleCardGradeEffect
    {
        public V2Enum_Grade V2Enum_Grade;
        public List<Transform> Effects = new List<Transform>();
    }

    public class UIGambleCardElement : MonoBehaviour
    {
        [SerializeField]
        private Image _gambleCardType;

        [SerializeField]
        private RectTransform _gambleCardRotationTrans;


        [SerializeField]
        private ParticleSystem _gambleCardDestroy;

        [SerializeField]
        private List<UIGambleCardGradeEffect> _uIGambleCardGradeEffects;

        private Material _originMaterial;

        V2Enum_ARR_SynergyType _v2Enum_ARR_Card;
        V2Enum_Grade _V2Enum_Grade = V2Enum_Grade.Max;


        //------------------------------------------------------------------------------------
        public void Init()
        {
            if (_gambleCardType != null)
                _originMaterial = _gambleCardType.material;
        }
        //------------------------------------------------------------------------------------
        private void AllHideParticle()
        {
            for (int i = 0; i < _uIGambleCardGradeEffects.Count; ++i)
            {
                UIGambleCardGradeEffect uIGambleCardGradeEffect = _uIGambleCardGradeEffects[i];
                if (uIGambleCardGradeEffect != null && uIGambleCardGradeEffect.Effects != null)
                    uIGambleCardGradeEffect.Effects.AllSetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetCard(V2Enum_ARR_SynergyType v2Enum_ARR_Card)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_ARR_Card);

            _v2Enum_ARR_Card = v2Enum_ARR_Card;

            AllHideParticle();
            _V2Enum_Grade = V2Enum_Grade.Max;
        }
        //------------------------------------------------------------------------------------
        public void SetInitState()
        {
            if (_gambleCardType != null)
                _gambleCardType.gameObject.SetActive(false);

            if (_gambleCardRotationTrans != null)
                _gambleCardRotationTrans.eulerAngles = Vector3.zero;

            SetOriginMaterial();
        }
        //------------------------------------------------------------------------------------
        public void OpenCard()
        {
            if (_gambleCardType != null)
                _gambleCardType.gameObject.SetActive(true);

            //if (_gambleCardSymbolImage != null)
            //    _gambleCardSymbolImage.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        public void EnableCardType(bool enable)
        {
            if (_gambleCardType != null)
                _gambleCardType.gameObject.SetActive(enable);

            //if (_gambleCardSymbolImage != null)
            //    _gambleCardSymbolImage.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void PlayCardDestroy()
        {
            if (_gambleCardDestroy != null)
            { 
                //_gambleCardDestroy.gameObject.SetActive(true);
                _gambleCardDestroy.Stop();
                _gambleCardDestroy.Play();
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableGradeParticle(V2Enum_Grade V2Enum_Grade)
        {
            for (int i = 0; i < _uIGambleCardGradeEffects.Count; ++i)
            {
                if (_uIGambleCardGradeEffects[i] == null)
                    continue;

                if (_uIGambleCardGradeEffects[i].V2Enum_Grade == _V2Enum_Grade)
                {
                    if (_uIGambleCardGradeEffects[i].Effects != null)
                        _uIGambleCardGradeEffects[i].Effects.AllSetActive(false);
                }
                else if (_uIGambleCardGradeEffects[i].V2Enum_Grade == V2Enum_Grade)
                {
                    if (_uIGambleCardGradeEffects[i].Effects != null)
                        _uIGambleCardGradeEffects[i].Effects.AllSetActive(true);
                }
            }

            _V2Enum_Grade = V2Enum_Grade;
        }
        //------------------------------------------------------------------------------------
        public void SetOriginMaterial()
        {
            if (_gambleCardType != null)
                _gambleCardType.material = _originMaterial;
        }
        //------------------------------------------------------------------------------------
        public void ChangeSliceMaterial(Material material)
        {
            if (_gambleCardType != null)
                _gambleCardType.material = material;
        }
        //------------------------------------------------------------------------------------
    }
}