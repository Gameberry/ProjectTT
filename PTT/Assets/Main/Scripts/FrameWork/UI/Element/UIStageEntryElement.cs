using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIStageEntryElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private GameObject _selectedRoot;
        [SerializeField] private GameObject _lockedRoot;
        [SerializeField] private GameObject _currentRoot;
        [SerializeField] private Button _button;

        private int _chapter;
        private int _stage;
        private bool _interactable;
        private Action<int, int> _onClick;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        public void Bind(int chapter, int stage, bool interactable, bool selected, bool isCurrent, Action<int, int> onClick)
        {
            _chapter = chapter;
            _stage = stage;
            _interactable = interactable;
            _onClick = onClick;

            if (_titleText != null)
                _titleText.SetText($"{chapter}-{stage}");

            if (_selectedRoot != null)
                _selectedRoot.SetActive(selected);
            if (_lockedRoot != null)
                _lockedRoot.SetActive(!interactable);
            if (_currentRoot != null)
                _currentRoot.SetActive(isCurrent);
            if (_button != null)
                _button.interactable = true;
        }

        private void OnClick()
        {
            _onClick?.Invoke(_chapter, _stage);
        }
    }
}
