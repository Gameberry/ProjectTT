using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class UIGoodsDropDirectionElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_myImage;

        [SerializeField]
        RectTransform m_myRect;

        private System.Action<UIGoodsDropDirectionElement> m_callback;


        //------------------------------------------------------------------------------------
        public void Init(System.Action<UIGoodsDropDirectionElement> action)
        {
            m_callback = action;
        }
        ////------------------------------------------------------------------------------------
        //public void Explosion(Sprite sprite, Vector3 from, Vector3 to, float explo_range, Ease fromEase, float fromDuration, Ease toEase, float toDuration)
        //{
        //    if (m_myImage != null)
        //        m_myImage.sprite = sprite;

        //    Vector3 convertFrom = from;
        //    Vector2 randomFrom = Random.insideUnitCircle * explo_range;
        //    convertFrom.x += randomFrom.x;
        //    convertFrom.y += randomFrom.y;

        //    float delay = Random.Range(0.0f, 0.3f);

        //    transform.position = from;
        //    Sequence sequence = DOTween.Sequence();
        //    sequence.Append(transform.DOMove(convertFrom, fromDuration).SetEase(fromEase));
        //    sequence.Append(transform.DOMove(to, toDuration).SetEase(toEase).SetDelay(delay));
        //    sequence.AppendCallback(endDirection);
        //}
        //------------------------------------------------------------------------------------
        public async UniTask Explosion(Sprite sprite, Vector3 from, Vector3 to, float explo_range, Ease fromEase, float fromDuration, Ease toEase, float toDuration)
        {
            if (m_myImage != null)
                m_myImage.sprite = sprite;

            Vector3 convertFrom = from;
            Vector2 randomFrom = Random.insideUnitCircle * explo_range;
            convertFrom.x += randomFrom.x;
            convertFrom.y += randomFrom.y;

            int delay = Random.Range(0, 300);

            transform.position = from;
            await transform.DOMove(convertFrom, fromDuration).SetEase(fromEase);
            await UniTask.Delay(delay);
            await transform.DOMove(to, toDuration).SetEase(toEase);
            //await transform.DOMove(to, toDuration).SetEase(toEase).SetDelay(delay);

            endDirection();
        }
        //------------------------------------------------------------------------------------
        private void endDirection()
        {
            gameObject.SetActive(false);

            m_callback?.Invoke(this);
        }
        //------------------------------------------------------------------------------------
    }
}