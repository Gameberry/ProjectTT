using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class DungeonSelectDialog : IDialog
    {
        private struct DungeonButtonView
        {
            public Enum_Dungeon DungeonType;
            public Button Button;
            public Image Background;
            public TMP_Text TitleText;
            public TMP_Text TicketText;
        }

        private readonly List<DungeonButtonView> _dungeonButtons = new List<DungeonButtonView>();

        private Button _closeButton;
        private Button _prevStageButton;
        private Button _nextStageButton;
        private Button _enterButton;

        private TMP_Text _titleText;
        private TMP_Text _descriptionText;
        private TMP_Text _stageText;
        private TMP_Text _unlockText;
        private TMP_Text _ticketText;
        private TMP_Text _rewardText;
        private TMP_Text _statusText;
        private TMP_Text _enterButtonText;

        private Enum_Dungeon _selectedDungeon = Enum_Dungeon.GrowthWeapon;
        private int _selectedStage = 1;

        protected override void OnLoad()
        {
            BuildUiIfNeeded();

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Exit);
            if (_prevStageButton != null)
                _prevStageButton.onClick.AddListener(OnClickPrevStage);
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(OnClickNextStage);
            if (_enterButton != null)
                _enterButton.onClick.AddListener(OnClickEnter);
        }

        protected override void OnEnter()
        {
            if (GrowthDungeonManager.isAlive)
                GrowthDungeonManager.Instance.OnGrowthDungeonProgressChanged += OnGrowthDungeonProgressChanged;
            if (ItemManager.isAlive)
                ItemManager.Instance.OnPointChanged += OnPointChanged;

            IReadOnlyList<Enum_Dungeon> dungeonTypes = GrowthDungeonManager.Instance.GetDungeonTypes();
            if (dungeonTypes != null && dungeonTypes.Count > 0)
                _selectedDungeon = dungeonTypes[0];

            SyncSelectedStageToProgress();
            RefreshAll();
        }

        protected override void OnExit()
        {
            if (GrowthDungeonManager.isAlive)
                GrowthDungeonManager.Instance.OnGrowthDungeonProgressChanged -= OnGrowthDungeonProgressChanged;
            if (ItemManager.isAlive)
                ItemManager.Instance.OnPointChanged -= OnPointChanged;

            if (_statusText != null)
                _statusText.SetText(string.Empty);
        }

        private void OnGrowthDungeonProgressChanged(Enum_Dungeon dungeonType)
        {
            if (dungeonType != _selectedDungeon)
                return;

            ClampSelectedStage();
            RefreshAll();
        }

        private void OnPointChanged()
        {
            RefreshAll();
        }

        private void OnClickDungeon(Enum_Dungeon dungeonType)
        {
            _selectedDungeon = dungeonType;
            SyncSelectedStageToProgress();

            if (_statusText != null)
                _statusText.SetText(string.Empty);

            RefreshAll();
        }

        private void OnClickPrevStage()
        {
            int maxUnlocked = GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon);
            if (maxUnlocked <= 1)
                return;

            _selectedStage = Mathf.Clamp(_selectedStage - 1, 1, maxUnlocked);
            RefreshAll();
        }

        private void OnClickNextStage()
        {
            int maxUnlocked = GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon);
            if (maxUnlocked <= 1)
                return;

            _selectedStage = Mathf.Clamp(_selectedStage + 1, 1, maxUnlocked);
            RefreshAll();
        }

        private void OnClickEnter()
        {
            if (GrowthDungeonManager.Instance.TryEnterDungeon(_selectedDungeon, _selectedStage, true) == false)
            {
                if (_statusText != null)
                    _statusText.SetText(BuildFailReason());

                RefreshAll();
                return;
            }

            if (BattleSceneManager.isAlive)
            {
                if (BattleSceneManager.Instance.BattleType == _selectedDungeon)
                    BattleSceneManager.Instance.ReloadCurrentBattleScene();
                else
                    BattleSceneManager.Instance.ChangeBattleScene(_selectedDungeon);
            }

            Exit();
        }

        private string BuildFailReason()
        {
            if (GrowthDungeonManager.Instance.CanEnter(_selectedDungeon, _selectedStage) == false)
                return "아직 입장할 수 없는 단계입니다.";

            if (GrowthDungeonManager.Instance.GetEntryTicketItemId(_selectedDungeon) <= 0)
                return "입장권 Point가 아직 설정되지 않았습니다.";

            return "입장권이 부족합니다.";
        }

        private void SyncSelectedStageToProgress()
        {
            _selectedStage = GrowthDungeonManager.Instance.GetCurrentStage(_selectedDungeon);
            ClampSelectedStage();
        }

        private void ClampSelectedStage()
        {
            int maxUnlocked = Mathf.Max(1, GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon));
            _selectedStage = Mathf.Clamp(_selectedStage, 1, maxUnlocked);
        }

        private void RefreshAll()
        {
            RefreshDungeonButtons();
            RefreshDetail();
        }

        private void RefreshDungeonButtons()
        {
            for (int i = 0; i < _dungeonButtons.Count; ++i)
            {
                DungeonButtonView view = _dungeonButtons[i];
                bool selected = view.DungeonType == _selectedDungeon;
                long ticketCount = GrowthDungeonManager.Instance.GetEntryTicketCount(view.DungeonType);

                if (view.Background != null)
                    view.Background.color = selected
                        ? new Color(0.16f, 0.57f, 0.78f, 1f)
                        : new Color(0.18f, 0.18f, 0.2f, 1f);

                if (view.TitleText != null)
                    view.TitleText.SetText(GrowthDungeonManager.Instance.GetDungeonDisplayName(view.DungeonType));

                if (view.TicketText != null)
                {
                    Enum_PointType ticketPointType = GrowthDungeonManager.Instance.GetEntryTicketPointType(view.DungeonType);
                    string ticketName = GrowthDungeonManager.Instance.GetPointDisplayName(ticketPointType);
                    view.TicketText.SetText($"{ticketName}  {ticketCount:N0}");
                }
            }
        }

        private void RefreshDetail()
        {
            ClampSelectedStage();

            if (_titleText != null)
                _titleText.SetText(GrowthDungeonManager.Instance.GetDungeonDisplayName(_selectedDungeon));

            if (_descriptionText != null)
                _descriptionText.SetText(GrowthDungeonManager.Instance.GetDungeonShortDescription(_selectedDungeon));

            int maxUnlocked = GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon);
            int maxConfigured = GrowthDungeonManager.Instance.GetMaxConfiguredStage(_selectedDungeon);
            int ticketCost = GrowthDungeonManager.Instance.GetEntryTicketCost(_selectedDungeon, _selectedStage);
            long ticketCount = GrowthDungeonManager.Instance.GetEntryTicketCount(_selectedDungeon);

            if (_stageText != null)
                _stageText.SetText($"단계  {_selectedStage}");

            if (_unlockText != null)
                _unlockText.SetText($"해금 단계  {maxUnlocked} / {maxConfigured}");

            if (_ticketText != null)
            {
                Enum_PointType ticketPointType = GrowthDungeonManager.Instance.GetEntryTicketPointType(_selectedDungeon);
                string ticketName = GrowthDungeonManager.Instance.GetPointDisplayName(ticketPointType);
                _ticketText.SetText($"입장권  {ticketName}  {ticketCount:N0} / 소모 {ticketCost}");
            }

            if (_prevStageButton != null)
                _prevStageButton.interactable = _selectedStage > 1;
            if (_nextStageButton != null)
                _nextStageButton.interactable = _selectedStage < maxUnlocked;

            if (_rewardText != null)
                _rewardText.SetText(BuildRewardText());

            bool canEnter = GrowthDungeonManager.Instance.CanEnter(_selectedDungeon, _selectedStage) &&
                            GrowthDungeonManager.Instance.CanAffordEntryTicket(_selectedDungeon, _selectedStage);

            if (_enterButton != null)
                _enterButton.interactable = canEnter;

            if (_enterButtonText != null)
                _enterButtonText.SetText(canEnter ? "입장" : "입장 불가");
        }

        private string BuildRewardText()
        {
            if (GrowthDungeonManager.Instance.TryGetInfo(_selectedDungeon, _selectedStage, out DungeonRuntimeInfo info) == false || info == null)
                return "보상 정보가 없습니다.";

            StringBuilder builder = new StringBuilder();

            if (info.RewardExp > 0)
                builder.AppendLine($"EXP  {info.RewardExp:N0}");

            AppendPointReward(builder, info.RewardPointType1, info.RewardPointAmount1);
            AppendPointReward(builder, info.RewardPointType2, info.RewardPointAmount2);

            if (info.RewardEquipmentCount > 0)
            {
                builder.Append("장비  ");
                builder.Append(info.RewardEquipmentCount);
                builder.Append("개");

                if (info.RewardEquipmentLevelMin > 0 || info.RewardEquipmentLevelMax > 0)
                {
                    builder.Append("  (Lv.");
                    builder.Append(info.RewardEquipmentLevelMin);
                    builder.Append("-");
                    builder.Append(Mathf.Max(info.RewardEquipmentLevelMin, info.RewardEquipmentLevelMax));
                    builder.Append(")");
                }

                builder.AppendLine();
            }

            if (builder.Length <= 0)
                builder.Append("보상 정보가 없습니다.");

            return builder.ToString().TrimEnd();
        }

        private void AppendPointReward(StringBuilder builder, Enum_PointType pointType, int amount)
        {
            if (pointType == Enum_PointType.Max || amount <= 0)
                return;

            builder.Append(GrowthDungeonManager.Instance.GetPointDisplayName(pointType));
            builder.Append("  ");
            builder.Append(amount.ToString("N0"));
            builder.AppendLine();
        }

        private void BuildUiIfNeeded()
        {
            if (_closeButton != null || dialogView == null)
                return;

            Sprite sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            TMP_FontAsset fontAsset = TMP_Settings.defaultFontAsset;

            RectTransform dialogRoot = dialogView.GetComponent<RectTransform>();
            dialogRoot.anchorMin = Vector2.zero;
            dialogRoot.anchorMax = Vector2.one;
            dialogRoot.offsetMin = Vector2.zero;
            dialogRoot.offsetMax = Vector2.zero;

            Image dim = AddImage(dialogView, "Dim", sprite, new Color(0f, 0f, 0f, 0.72f));
            Stretch(dim.rectTransform);

            Image panel = AddImage(dialogView, "Panel", sprite, new Color(0.12f, 0.12f, 0.14f, 0.98f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(980f, 640f));

            _titleText = AddText(panel.gameObject, "Title", fontAsset, 34, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            SetRect(_titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(300f, 48f));
            _titleText.SetText("성장 던전");

            _closeButton = AddButton(panel.gameObject, "CloseButton", sprite, new Color(0.3f, 0.12f, 0.12f, 1f));
            SetRect(_closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(44f, 44f));
            TMP_Text closeText = AddText(_closeButton.gameObject, "Text", fontAsset, 24, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            Stretch(closeText.rectTransform);
            closeText.SetText("X");

            Image leftPanel = AddImage(panel.gameObject, "DungeonListPanel", sprite, new Color(0.1f, 0.1f, 0.12f, 1f));
            SetRect(leftPanel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -76f), new Vector2(260f, 532f));
            VerticalLayoutGroup leftLayout = leftPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 10f;
            leftLayout.padding = new RectOffset(12, 12, 12, 12);
            leftLayout.childControlHeight = false;
            leftLayout.childControlWidth = true;
            leftLayout.childForceExpandHeight = false;
            leftLayout.childForceExpandWidth = true;

            Image rightPanel = AddImage(panel.gameObject, "DetailPanel", sprite, new Color(0.15f, 0.15f, 0.17f, 1f));
            SetRect(rightPanel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(300f, -76f), new Vector2(-24f, -24f));

            TMP_Text rightTitle = AddText(rightPanel.gameObject, "DetailTitle", fontAsset, 36, FontStyles.Bold, TextAlignmentOptions.TopLeft, Color.white);
            SetRect(rightTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(-48f, 42f));
            _titleText = rightTitle;

            _descriptionText = AddText(rightPanel.gameObject, "Description", fontAsset, 22, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.85f, 0.85f, 0.85f, 1f));
            SetRect(_descriptionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(24f, -76f), new Vector2(-48f, 64f));
            _descriptionText.enableWordWrapping = true;

            _prevStageButton = AddButton(rightPanel.gameObject, "PrevStageButton", sprite, new Color(0.18f, 0.36f, 0.52f, 1f));
            SetRect(_prevStageButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -166f), new Vector2(64f, 48f));
            TMP_Text prevText = AddText(_prevStageButton.gameObject, "Text", fontAsset, 28, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            Stretch(prevText.rectTransform);
            prevText.SetText("<");

            _stageText = AddText(rightPanel.gameObject, "StageText", fontAsset, 28, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            SetRect(_stageText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(104f, -166f), new Vector2(180f, 48f));

            _nextStageButton = AddButton(rightPanel.gameObject, "NextStageButton", sprite, new Color(0.18f, 0.36f, 0.52f, 1f));
            SetRect(_nextStageButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -166f), new Vector2(64f, 48f));
            TMP_Text nextText = AddText(_nextStageButton.gameObject, "Text", fontAsset, 28, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            Stretch(nextText.rectTransform);
            nextText.SetText(">");

            _unlockText = AddText(rightPanel.gameObject, "UnlockText", fontAsset, 22, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);
            SetRect(_unlockText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(24f, -226f), new Vector2(-48f, 34f));

            _ticketText = AddText(rightPanel.gameObject, "TicketText", fontAsset, 22, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.95f, 0.89f, 0.4f, 1f));
            SetRect(_ticketText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(24f, -266f), new Vector2(-48f, 34f));

            TMP_Text rewardTitle = AddText(rightPanel.gameObject, "RewardTitle", fontAsset, 24, FontStyles.Bold, TextAlignmentOptions.TopLeft, Color.white);
            SetRect(rewardTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(24f, -324f), new Vector2(-48f, 36f));
            rewardTitle.SetText("도전 클리어 보상");

            _rewardText = AddText(rightPanel.gameObject, "RewardText", fontAsset, 22, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);
            SetRect(_rewardText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(24f, -368f), new Vector2(-48f, -160f));
            _rewardText.enableWordWrapping = true;
            _rewardText.overflowMode = TextOverflowModes.Overflow;

            _statusText = AddText(rightPanel.gameObject, "StatusText", fontAsset, 20, FontStyles.Normal, TextAlignmentOptions.BottomLeft, new Color(1f, 0.52f, 0.52f, 1f));
            SetRect(_statusText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(24f, 88f), new Vector2(-220f, 28f));
            _statusText.SetText(string.Empty);

            _enterButton = AddButton(rightPanel.gameObject, "EnterButton", sprite, new Color(0.34f, 0.72f, 0.12f, 1f));
            SetRect(_enterButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(184f, 58f));
            _enterButtonText = AddText(_enterButton.gameObject, "Text", fontAsset, 24, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            Stretch(_enterButtonText.rectTransform);

            CreateDungeonButtons(leftPanel.gameObject, sprite, fontAsset);
        }

        private void CreateDungeonButtons(GameObject parent, Sprite sprite, TMP_FontAsset fontAsset)
        {
            IReadOnlyList<Enum_Dungeon> dungeonTypes = GrowthDungeonManager.Instance.GetDungeonTypes();
            if (dungeonTypes == null)
                return;

            for (int i = 0; i < dungeonTypes.Count; ++i)
            {
                Enum_Dungeon dungeonType = dungeonTypes[i];
                Button button = AddButton(parent, $"Dungeon_{dungeonType}", sprite, new Color(0.18f, 0.18f, 0.2f, 1f));
                LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 88f;
                layout.preferredHeight = 88f;

                Image background = button.GetComponent<Image>();

                TMP_Text title = AddText(button.gameObject, "Title", fontAsset, 24, FontStyles.Bold, TextAlignmentOptions.TopLeft, Color.white);
                SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(-32f, 30f));

                TMP_Text ticket = AddText(button.gameObject, "Ticket", fontAsset, 18, FontStyles.Normal, TextAlignmentOptions.BottomLeft, new Color(0.94f, 0.89f, 0.45f, 1f));
                SetRect(ticket.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(16f, 10f), new Vector2(-32f, 24f));

                button.onClick.AddListener(() => OnClickDungeon(dungeonType));

                _dungeonButtons.Add(new DungeonButtonView
                {
                    DungeonType = dungeonType,
                    Button = button,
                    Background = background,
                    TitleText = title,
                    TicketText = ticket,
                });
            }
        }

        private static Image AddImage(GameObject parent, string name, Sprite sprite, Color color)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent.transform, false);

            Image image = child.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        private static Button AddButton(GameObject parent, string name, Sprite sprite, Color color)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            child.transform.SetParent(parent.transform, false);

            Image image = child.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;

            Button button = child.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.08f;
            colors.pressedColor = color * 0.92f;
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f, 0.75f);
            button.colors = colors;
            button.targetGraphic = image;

            return button;
        }

        private static TMP_Text AddText(GameObject parent, string name, TMP_FontAsset fontAsset, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            child.transform.SetParent(parent.transform, false);

            TMP_Text text = child.GetComponent<TMP_Text>();
            text.font = fontAsset;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            SetRect(rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.localScale = Vector3.one;
        }
    }
}
