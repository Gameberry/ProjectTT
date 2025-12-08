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

		private IDialogAnimation _iDialogAnimation;

		void Awake()
		{
			if (dialogView == null)
				throw new System.NullReferenceException(string.Format("{0} dialogView Null", this.name));

			_name = GetType().Name;
			_rt = GetComponent<RectTransform>();
			InitAnimation();

			if (_exitBtn != null)
			{
				for (int i = 0; i < _exitBtn.Count; ++i)
				{
					if (_exitBtn[i] != null)
						_exitBtn[i].onClick.AddListener(Exit);
				}
			}
		}

		public void Load()
		{
			int sibling = EnumExtensions.ParseToInt<UISibling>(_name);
			UIManager.Instance.SetSibling(_rt, sibling);

			dialogView.SetActive(false);

			OnLoad();
		}

        public void Load_Element()
        {
			dialogView.SetActive(false);

			OnLoad();
        }

		private void InitAnimation()
		{
			_iDialogAnimation = GetComponent<IDialogAnimation>();
			_iDialogAnimation?.OnInAnimationsFinish.AddListener(EnterFinish);
			_iDialogAnimation?.OnOutAnimationsFinish.AddListener(ExitFinish);
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

			if (_iDialogAnimation != null)
				_iDialogAnimation.PlayInAnimation();
			else
			{
				EnterFinish();
			}
		}

		private void EnterFinish()
		{
			if (UseBackBtn == true)
				Managers.AOSBackBtnManager.Instance.EnterBackBtnAction(this);
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
			_isEnter = false;

			if (_iDialogAnimation != null)
				_iDialogAnimation.PlayOutAnimation();
			else
			{
				ExitFinish();
			}
		}

		private void ExitFinish()
		{
			if (dialogView != null)
				dialogView.SetActive(false);

			OnExit();
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
