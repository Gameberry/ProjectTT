using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.UI;

namespace GameBerry.TestScene
{
    public class TestMapSelectionUI : IDialog
    {
        [Header("References")]
        [SerializeField] private RectTransform _buttonContainer;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _closeButton;

        private readonly List<Button> _mapButtons = new List<Button>();
        private TestMapFlowController _currentFlowController;

        protected override void OnLoad()
        {
            base.OnLoad();
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(Exit);
            }
        }

        public void Show(TestMapFlowController flowController)
        {
            if (_rt == null)
                Load_Element();

            _currentFlowController = flowController;
            RefreshButtons();
            Enter();
        }

        public void Hide()
        {
            Exit();
        }

        private void RefreshButtons()
        {
            if (_currentFlowController == null || _buttonContainer == null)
                return;

            for (int i = 0; i < _mapButtons.Count; i++)
            {
                if (_mapButtons[i] != null)
                    Destroy(_mapButtons[i].gameObject);
            }

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
            // Note: For a more robust IDialog implementation, using a prefab for these buttons would be better.
            // For now, we maintain the dynamic creation logic but parent it to the serialized container.
            GameObject buttonObject = new GameObject(mapDefinition.DisplayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(_buttonContainer, false);
            
            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = unlocked
                ? new Color(0.18f, 0.35f, 0.22f, 0.95f)
                : new Color(0.22f, 0.22f, 0.22f, 0.85f);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0.0f, 128.0f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = buttonImage.color;
            colors.highlightedColor = new Color(0.23f, 0.33f, 0.44f, 1.0f);
            colors.pressedColor = new Color(0.12f, 0.2f, 0.28f, 1.0f);
            colors.disabledColor = new Color(0.18f, 0.18f, 0.18f, 0.75f);
            button.colors = colors;

            // Name Text
            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(buttonObject.transform, false);
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = mapDefinition.DisplayName;
            nameText.fontSize = 28;
            nameText.alignment = TextAlignmentOptions.TopLeft;
            nameText.color = unlocked ? Color.white : new Color(0.72f, 0.72f, 0.72f, 1.0f);
            
            RectTransform nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0.0f, 1.0f);
            nameRect.anchorMax = new Vector2(1.0f, 1.0f);
            nameRect.pivot = new Vector2(0.5f, 1.0f);
            nameRect.anchoredPosition = new Vector2(0.0f, -14.0f);
            nameRect.sizeDelta = new Vector2(-32.0f, 36.0f);

            // Description Text
            GameObject descObj = new GameObject("Description", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(buttonObject.transform, false);
            TextMeshProUGUI descriptionText = descObj.GetComponent<TextMeshProUGUI>();
            descriptionText.text = description;
            descriptionText.fontSize = 18;
            descriptionText.alignment = TextAlignmentOptions.TopLeft;
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
    }
}
