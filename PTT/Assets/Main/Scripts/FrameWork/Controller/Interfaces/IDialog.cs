using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
	public class IDialog : MonoBehaviour
	{
		protected RectTransform _rt;
		protected string _name;
		public GameObject dialogView;

        public bool isEnter { get { return _isEnter; } }

        protected bool _isEnter = false;

		public bool UseBackBtn = false;

		[SerializeField]
		private List<Button> _exitBtn;

		private UIDialogAnimaion _uIDialogAnimaion;

		void Awake()
		{
			if (dialogView == null)
				throw new System.NullReferenceException(string.Format("{0} dialogView Null", this.name));
		}

		public void Load()
		{
			_name = GetType().Name;
			_rt = GetComponent<RectTransform>();
			_uIDialogAnimaion = GetComponent<UIDialogAnimaion>();
			_uIDialogAnimaion?.Init();

			int sibling = EnumExtensions.ParseToInt<UISibling>(_name);
			UIManager.Instance.SetSibling(_rt, sibling);

			dialogView.SetActive(false);


			if (_exitBtn != null)
			{
				for (int i = 0; i < _exitBtn.Count; ++i)
				{
					if (_exitBtn[i] != null)
						_exitBtn[i].onClick.AddListener(Exit);
				}
			}

			OnLoad();
		}

        public void Load_Element()
        {
            _name = GetType().Name;
            _rt = GetComponent<RectTransform>();
			_uIDialogAnimaion = GetComponent<UIDialogAnimaion>();

			dialogView.SetActive(false);

			if (_exitBtn != null)
			{
				for (int i = 0; i < _exitBtn.Count; ++i)
				{
					if (_exitBtn[i] != null)
						_exitBtn[i].onClick.AddListener(Exit);
				}
			}

			OnLoad();
        }

        protected virtual void OnLoad()
		{
		}

		public void Unload()
		{
			OnExit();
			OnUnload();
		}

		protected virtual void OnUnload()
		{
		}

		public void ElementEnter()
		{
			Enter();
		}

		public void Enter()
		{
			if (dialogView != null)
			{
				if (dialogView.activeSelf)
					return;
				dialogView.SetActive(true);
			}

			_isEnter = true;
			OnEnter();

			if (_uIDialogAnimaion != null)
				_uIDialogAnimaion.PlayOpening(() =>
				{
					if (UseBackBtn == true)
						Managers.AOSBackBtnManager.Instance.EnterBackBtnAction(this);
				});
			else
			{
				if (UseBackBtn == true)
					Managers.AOSBackBtnManager.Instance.EnterBackBtnAction(this);
			}
		}

		public virtual void BackKeyCall()
		{
			Exit();
		}

		public void ElementExit()
		{
			Exit();
		}

		public void Exit()
		{
			if (_uIDialogAnimaion != null)
				_uIDialogAnimaion.PlayClosing(() =>
				{
					if (dialogView != null)
						dialogView.SetActive(false);

					_isEnter = false;
					OnExit();
				});
			else
			{
				if (dialogView != null)
					dialogView.SetActive(false);

				_isEnter = false;
				OnExit();
			}
		}

		protected virtual void OnDestroy()
        {
            Unload();
        }

		protected virtual void OnEnter()
		{
		}

		protected virtual void OnExit()
		{
		}
    }
}
