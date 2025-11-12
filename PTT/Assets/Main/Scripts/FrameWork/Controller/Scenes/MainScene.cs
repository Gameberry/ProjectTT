using GameBerry.Managers;
using Cysharp.Threading.Tasks;

namespace GameBerry.Scene
{
    // 첫Scene
    // 패치와 클라이언트 선 로드를 해준다.
    public class MainScene : IScene
    {
        protected override void OnLoadStart()
        {
            UI.UIManager.DialogEnter<UI.AppLoadingDialog>();

            SetResourceLoadComplete();
        }

        protected override void OnLoadComplete()
        {
            LateChange().Forget();
        }

		public async UniTask LateChange()
        {
            await UniTask.Yield();

            UI.UIManager.Instance.Unload(nameof(UI.AppLoadingDialog));
            SceneManager.Instance.Load(Constants.SceneName.Lobby, true);
        }
	}
}