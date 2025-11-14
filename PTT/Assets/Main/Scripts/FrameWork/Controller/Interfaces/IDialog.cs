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

		public event OnCallBack exitCallBack;
		protected bool ignoreExit = false;

		[SerializeField]
		private List<Button> _exitBtn;

		void Awake()
		{
			if (dialogView == null)
				throw new System.NullReferenceException(string.Format("{0} dialogView Null", this.name));
		}

		public void Load()
		{
			_name = GetType().Name;
			_rt = GetComponent<RectTransform>();

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

		public void Enter()
		{
			if (UIManager.Instance.IgnoreEnterDialog == true)
				return;

			if (dialogView != null)
			{
				if (dialogView.activeSelf)
					return;
				dialogView.SetActive(true);
			}

			_isEnter = true;
			OnEnter();

			if (UseBackBtn == true)
				Managers.AOSBackBtnManager.Instance.EnterBackBtnAction(this);
		}

		public void ElementEnter()
		{
            Enter();
		}

		public virtual void BackKeyCall()
		{
			Exit();
		}

        public void Exit()
        {
			if (ignoreExit == true)
				return;

			if (dialogView != null)
				dialogView.SetActive(false);

			_isEnter = false;
			exitCallBack?.Invoke();
			OnExit();
		}

		public void ElementExit()
		{
			Exit();
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
