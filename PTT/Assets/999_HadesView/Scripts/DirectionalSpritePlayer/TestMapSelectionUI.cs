using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.TestScene
{
    public class TestMapSelectionUI : MonoBehaviour
    {
        private readonly List<Button> _mapButtons = new List<Button>();

        private Canvas _canvas;
        private GameObject _panelRoot;
        private RectTransform _buttonContainer;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _descriptionText;
        private TMP_FontAsset _font;
        private TestMapFlowController _currentFlowController;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        private void Awake()
        {
            EnsureUi();
            Hide();
        }

        public void Show(TestMapFlowController flowController)
        {
            _currentFlowController = flowController;
            EnsureUi();
            RefreshButtons();
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
                _canvas = GetComponentInParent<Canvas>();
                if (_canvas == null)
                    _canvas = FindObjectOfType<Canvas>();
            }

            if (_canvas == null)
            {
                Debug.LogWarning("[TestMapSelectionUI] No parent canvas found. Attach this component under a UI Canvas.");
                return;
            }

            RectTransform rootRect = transform as RectTransform;
            if (rootRect == null)
                rootRect = gameObject.AddComponent<RectTransform>();

            if (transform.parent != _canvas.transform)
                transform.SetParent(_canvas.transform, false);

            if (_panelRoot != null)
                return;

            _panelRoot = CreateUiObject("PanelRoot", transform);
            Image panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0.05f, 0.08f, 0.12f, 0.88f);

            RectTransform panelRect = _panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(860.0f, 620.0f);

            _titleText = CreateText("Title", _panelRoot.transform, "Select Map", 36, TextAlignmentOptions.Left);
            RectTransform titleRect = _titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.0f, 1.0f);
            titleRect.anchorMax = new Vector2(1.0f, 1.0f);
            titleRect.pivot = new Vector2(0.5f, 1.0f);
            titleRect.anchoredPosition = new Vector2(0.0f, -24.0f);
            titleRect.sizeDelta = new Vector2(-80.0f, 48.0f);

            _descriptionText = CreateText("Description", _panelRoot.transform, "Unlocked maps can be entered from the lobby portal.", 20, TextAlignmentOptions.TopLeft);
            RectTransform descRect = _descriptionText.rectTransform;
            descRect.anchorMin = new Vector2(0.0f, 1.0f);
            descRect.anchorMax = new Vector2(1.0f, 1.0f);
            descRect.pivot = new Vector2(0.5f, 1.0f);
            descRect.anchoredPosition = new Vector2(0.0f, -78.0f);
            descRect.sizeDelta = new Vector2(-80.0f, 52.0f);

            GameObject containerObject = CreateUiObject("ButtonContainer", _panelRoot.transform);
            _buttonContainer = containerObject.GetComponent<RectTransform>();
            _buttonContainer.anchorMin = new Vector2(0.0f, 0.0f);
            _buttonContainer.anchorMax = new Vector2(1.0f, 1.0f);
            _buttonContainer.offsetMin = new Vector2(40.0f, 40.0f);
            _buttonContainer.offsetMax = new Vector2(-40.0f, -150.0f);

            VerticalLayoutGroup layoutGroup = containerObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 220.0f;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);

            ContentSizeFitter fitter = containerObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            Button closeButton = CreateButton("CloseButton", _panelRoot.transform, "Close", Hide);
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1.0f, 1.0f);
            closeRect.anchorMax = new Vector2(1.0f, 1.0f);
            closeRect.pivot = new Vector2(1.0f, 1.0f);
            closeRect.anchoredPosition = new Vector2(-24.0f, -20.0f);
            closeRect.sizeDelta = new Vector2(120.0f, 44.0f);
        }

        private void RefreshButtons()
        {
            if (_currentFlowController == null)
                return;

            for (int i = 0; i < _mapButtons.Count; i++)
                Destroy(_mapButtons[i].gameObject);

            _mapButtons.Clear();

            IReadOnlyList<TestMapDefinition> maps = _currentFlowController.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                TestMapDefinition mapDefinition = maps[i];
                if (mapDefinition == null)
                    continue;

                bool unlocked = _currentFlowController.IsUnlocked(mapDefinition);
                bool cleared = _currentFlowController.IsCleared(mapDefinition);

                string description = $"Rooms: {mapDefinition.RoomCount}";
                if (mapDefinition.RequiredClearMap != null)
                    description += unlocked
                        ? $"\nUnlocked: cleared {mapDefinition.RequiredClearMap.DisplayName}"
                        : $"\nLocked: clear {mapDefinition.RequiredClearMap.DisplayName} first";

                if (cleared)
                    description += "\nStatus: Cleared";

                Button button = CreateMapButton(mapDefinition, unlocked, description);
                _mapButtons.Add(button);
            }
        }

        private Button CreateMapButton(TestMapDefinition mapDefinition, bool unlocked, string description)
        {
            Button button = CreateButton(mapDefinition.DisplayName, _buttonContainer, null, null);
            Image buttonImage = button.GetComponent<Image>();
            buttonImage.color = unlocked
                ? new Color(0.18f, 0.35f, 0.22f, 0.95f)
                : new Color(0.22f, 0.22f, 0.22f, 0.85f);

            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0.0f, 128.0f);

            for (int i = button.transform.childCount - 1; i >= 0; i--)
                Destroy(button.transform.GetChild(i).gameObject);

            TextMeshProUGUI nameText = CreateText("Name", button.transform, mapDefinition.DisplayName, 28, TextAlignmentOptions.TopLeft);
            nameText.color = unlocked ? Color.white : new Color(0.72f, 0.72f, 0.72f, 1.0f);
            RectTransform nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0.0f, 1.0f);
            nameRect.anchorMax = new Vector2(1.0f, 1.0f);
            nameRect.pivot = new Vector2(0.5f, 1.0f);
            nameRect.anchoredPosition = new Vector2(0.0f, -14.0f);
            nameRect.sizeDelta = new Vector2(-32.0f, 36.0f);

            TextMeshProUGUI descriptionText = CreateText("Description", button.transform, description, 18, TextAlignmentOptions.TopLeft);
            descriptionText.color = unlocked ? new Color(0.86f, 0.94f, 0.88f, 1.0f) : new Color(0.7f, 0.7f, 0.7f, 0.95f);
            RectTransform descRect = descriptionText.rectTransform;
            descRect.anchorMin = new Vector2(0.0f, 0.0f);
            descRect.anchorMax = new Vector2(1.0f, 1.0f);
            descRect.offsetMin = new Vector2(16.0f, 14.0f);
            descRect.offsetMax = new Vector2(-16.0f, -50.0f);

            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            if (unlocked)
                button.onClick.AddListener(() => _currentFlowController.TrySelectMap(mapDefinition));

            return button;
        }

        private Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.17f, 0.24f, 0.32f, 0.95f);

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.23f, 0.33f, 0.44f, 1.0f);
            colors.pressedColor = new Color(0.12f, 0.2f, 0.28f, 1.0f);
            colors.disabledColor = new Color(0.18f, 0.18f, 0.18f, 0.75f);
            button.colors = colors;

            if (onClick != null)
                button.onClick.AddListener(onClick);

            if (string.IsNullOrEmpty(label) == false)
            {
                TextMeshProUGUI buttonText = CreateText("Label", buttonObject.transform, label, 22, TextAlignmentOptions.Center);
                RectTransform textRect = buttonText.rectTransform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }

            return button;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            TextMeshProUGUI uiText = textObject.AddComponent<TextMeshProUGUI>();
            uiText.font = _font;
            uiText.text = text;
            uiText.fontSize = fontSize;
            uiText.alignment = alignment;
            uiText.color = Color.white;
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
