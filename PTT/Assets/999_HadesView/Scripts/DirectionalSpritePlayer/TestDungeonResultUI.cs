using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameBerry.TestScene
{
    public class TestDungeonResultUI : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _panelRoot;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _descriptionText;
        private TMP_FontAsset _font;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        private void Awake()
        {
            EnsureUi();
            Hide();
        }

        public void ShowFail(string description)
        {
            Show("FAIL", description, new Color(1.0f, 0.35f, 0.35f, 1.0f), new Color(0.07f, 0.02f, 0.02f, 0.82f));
        }

        public void ShowClear(string description)
        {
            Show("CLEAR", description, new Color(0.45f, 1.0f, 0.55f, 1.0f), new Color(0.02f, 0.08f, 0.03f, 0.82f));
        }

        private void Show(string title, string description, Color titleColor, Color panelColor)
        {
            EnsureUi();
            Image panelImage = _panelRoot.GetComponent<Image>();
            if (panelImage != null)
                panelImage.color = panelColor;

            _titleText.text = title;
            _titleText.color = titleColor;
            _descriptionText.text = description;
            _panelRoot.SetActive(true);
        }

        public void Hide()
        {
            EnsureUi();
            _panelRoot.SetActive(false);
        }

        private void EnsureUi()
        {
            EnsureEventSystem();
            EnsureFont();

            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
                if (_canvas == null)
                    _canvas = gameObject.AddComponent<Canvas>();

                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 4500;
            }

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            if (GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
            }

            if (_panelRoot != null)
                return;

            _panelRoot = CreateUiObject("ResultPanel", transform);
            Image panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0.07f, 0.02f, 0.02f, 0.82f);

            RectTransform panelRect = _panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            _titleText = CreateText("Title", _panelRoot.transform, "FAIL", 96, TextAlignmentOptions.Center);
            RectTransform titleRect = _titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0.0f, 36.0f);
            titleRect.sizeDelta = new Vector2(900.0f, 120.0f);
            _titleText.color = new Color(1.0f, 0.35f, 0.35f, 1.0f);

            _descriptionText = CreateText("Description", _panelRoot.transform, "You will return to the lobby.", 28, TextAlignmentOptions.Center);
            RectTransform descRect = _descriptionText.rectTransform;
            descRect.anchorMin = new Vector2(0.5f, 0.5f);
            descRect.anchorMax = new Vector2(0.5f, 0.5f);
            descRect.pivot = new Vector2(0.5f, 0.5f);
            descRect.anchoredPosition = new Vector2(0.0f, -52.0f);
            descRect.sizeDelta = new Vector2(960.0f, 80.0f);
            _descriptionText.color = new Color(1.0f, 0.92f, 0.92f, 1.0f);
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            TextMeshProUGUI uiText = textObject.AddComponent<TextMeshProUGUI>();
            uiText.font = _font;
            uiText.text = text;
            uiText.fontSize = fontSize;
            uiText.alignment = alignment;
            uiText.enableWordWrapping = true;
            uiText.overflowMode = TextOverflowModes.Overflow;
            return uiText;
        }

        private GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject uiObject = new GameObject(name, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void EnsureFont()
        {
            if (_font != null)
                return;

            _font = TMP_Settings.defaultFontAsset;
            if (_font == null)
                _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }
}
