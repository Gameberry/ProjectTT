using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIStageChapterTabElement : MonoBehaviour
    {
        [SerializeField] private Image _chapterImage;
        [SerializeField] private TMP_Text _chapterText;
        [SerializeField] private GameObject _selectedRoot;
        [SerializeField] private GameObject _dimmedRoot;
        [SerializeField] private Button _button;

        private int _chapter;
        private Action<int> _onClick;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        public void Bind(int chapter, bool unlocked, bool selected, Action<int> onClick)
        {
            _chapter = chapter;
            _onClick = onClick;

            if( _chapterImage != null)
                _chapterImage.sprite = StageManager.Instance.GetChapterIcon(chapter);

            if (_chapterText != null)
                _chapterText.SetText($"{chapter} Chapter");

            if (_selectedRoot != null)
                _selectedRoot.SetActive(selected);

            if (_dimmedRoot != null)
                _dimmedRoot.SetActive(unlocked == false);
        }

        private void OnClick()
        {
            _onClick?.Invoke(_chapter);
        }
    }
}
