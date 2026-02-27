using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIAuctionListItemElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private TMP_Text _actionText;
        [SerializeField] private Button _actionButton;

        private Auction _boundAuction;
        private Action<Auction> _onClickAction;

        private void Awake()
        {
            if (_actionButton != null)
                _actionButton.onClick.AddListener(OnClickAction);
        }

        public void Bind(Auction auction, string infoText, string actionText, bool interactable, Action<Auction> onClickAction)
        {
            _boundAuction = auction;
            _onClickAction = onClickAction;

            if (_infoText != null)
                _infoText.SetText(infoText ?? string.Empty);

            if (_actionText != null)
                _actionText.SetText(actionText ?? string.Empty);

            if (_actionButton != null)
            {
                _actionButton.interactable = interactable;
                _actionButton.gameObject.SetActive(string.IsNullOrEmpty(actionText) == false);
            }
        }

        private void OnClickAction()
        {
            _onClickAction?.Invoke(_boundAuction);
        }
    }
}
