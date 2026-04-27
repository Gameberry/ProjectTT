using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.TestScene
{
    public class TestDamageTextManager : MonoBehaviour
    {
        private static TestDamageTextManager _instance;

        private readonly Queue<DamageTextEntry> _pool = new Queue<DamageTextEntry>();
        private readonly List<DamageTextEntry> _activeEntries = new List<DamageTextEntry>();

        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private float _duration = 0.65f;
        [SerializeField] private float _riseDistance = 48.0f;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0.0f, 0.9f, 0.0f);
        [SerializeField] private Vector2 _screenOffset = new Vector2(0.0f, 70.0f);

        public static TestDamageTextManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<TestDamageTextManager>();
                return _instance;
            }
        }

        private void Reset()
        {
            EnsureReferences();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            EnsureReferences();
            EnsureFont();
        }

        private void Update()
        {
            Camera currentCamera = ResolveRenderCamera();
            for (int i = _activeEntries.Count - 1; i >= 0; i--)
            {
                DamageTextEntry entry = _activeEntries[i];
                if (entry == null)
                {
                    _activeEntries.RemoveAt(i);
                    continue;
                }

                float elapsed = Time.time - entry.StartTime;
                float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, _duration));
                if (normalized >= 1.0f)
                {
                    Release(entry);
                    _activeEntries.RemoveAt(i);
                    continue;
                }

                entry.UpdateVisual(normalized, _riseDistance, _screenOffset, _contentRoot, currentCamera);
            }
        }

        public void ShowDamage(Vector3 worldPosition, int damage, bool isCritical = false, bool isPlayerDamage = false)
        {
            if (Instance == null)
                return;

            EnsureReferences();
            EnsureFont();
            if (_contentRoot == null)
                return;

            DamageTextEntry entry = _pool.Count > 0 ? _pool.Dequeue() : CreateEntry();
            if (entry == null)
                return;

            Color color = isPlayerDamage
                ? new Color(1.0f, 0.45f, 0.45f, 1.0f)
                : new Color(1.0f, 0.95f, 0.6f, 1.0f);

            if (isCritical)
                color = new Color(1.0f, 0.72f, 0.22f, 1.0f);

            float fontSize = isCritical ? 44.0f : 34.0f;
            entry.Show(worldPosition + _worldOffset, damage.ToString(), color, fontSize, _contentRoot);
            _activeEntries.Add(entry);
        }

        private void EnsureReferences()
        {
            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();

            if (_contentRoot == null)
                _contentRoot = _canvas != null ? _canvas.transform as RectTransform : transform as RectTransform;

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null && _canvas != null && rectTransform.parent == _canvas.transform)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        private DamageTextEntry CreateEntry()
        {
            GameObject entryObject = new GameObject("DamageTextEntry", typeof(RectTransform));
            RectTransform rectTransform = entryObject.GetComponent<RectTransform>();
            rectTransform.SetParent(_contentRoot, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(220.0f, 80.0f);

            TextMeshProUGUI text = entryObject.AddComponent<TextMeshProUGUI>();
            text.font = _font;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.fontSize = 34.0f;
            text.outlineWidth = 0.18f;
            text.outlineColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);

            DamageTextEntry entry = entryObject.AddComponent<DamageTextEntry>();
            entry.Initialize(rectTransform, text);
            entryObject.SetActive(false);
            return entry;
        }

        private void Release(DamageTextEntry entry)
        {
            if (entry == null)
                return;

            entry.gameObject.SetActive(false);
            entry.RectTransform.SetParent(_contentRoot, false);
            _pool.Enqueue(entry);
        }

        private void EnsureFont()
        {
            if (_font != null)
                return;

            _font = TMP_Settings.defaultFontAsset;
            if (_font == null)
                _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        private Camera ResolveRenderCamera()
        {
            if (_canvas == null)
                return Camera.main;

            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
        }

        private sealed class DamageTextEntry : MonoBehaviour
        {
            private TextMeshProUGUI _text;
            private Color _baseColor;
            private Vector3 _worldPosition;

            public RectTransform RectTransform { get; private set; }
            public float StartTime { get; private set; }

            public void Initialize(RectTransform rectTransform, TextMeshProUGUI text)
            {
                RectTransform = rectTransform;
                _text = text;
            }

            public void Show(Vector3 worldPosition, string value, Color color, float fontSize, RectTransform parent)
            {
                _worldPosition = worldPosition;
                StartTime = Time.time;
                RectTransform.SetParent(parent, false);
                gameObject.SetActive(true);

                if (_text == null)
                    return;

                _baseColor = color;
                _text.text = value;
                _text.color = color;
                _text.fontSize = fontSize;
            }

            public void UpdateVisual(
                float normalized,
                float riseDistance,
                Vector2 screenOffset,
                RectTransform canvasRoot,
                Camera renderCamera)
            {
                if (RectTransform == null || canvasRoot == null)
                    return;

                Camera cameraForViewport = renderCamera != null ? renderCamera : Camera.main;
                if (cameraForViewport == null)
                    return;

                Vector3 viewportPoint = cameraForViewport.WorldToViewportPoint(_worldPosition);
                if (viewportPoint.z < 0.0f)
                {
                    gameObject.SetActive(false);
                    return;
                }

                Vector2 canvasSize = canvasRoot.rect.size;
                Vector2 localPoint = new Vector2(
                    (viewportPoint.x - 0.5f) * canvasSize.x,
                    (viewportPoint.y - 0.5f) * canvasSize.y);

                RectTransform.anchoredPosition = localPoint + screenOffset + Vector2.up * (riseDistance * normalized);

                if (_text == null)
                    return;

                Color currentColor = _baseColor;
                currentColor.a = 1.0f - normalized;
                _text.color = currentColor;
            }
        }
    }
}
