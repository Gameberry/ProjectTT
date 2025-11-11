using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoleRedirector : MonoBehaviour, IPointerClickHandler
{
    public Button target;

    public void OnPointerClick(PointerEventData eventData)
    {
        target.onClick.Invoke();
    }
}