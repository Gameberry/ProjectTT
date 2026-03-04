using System;
using UnityEngine;
using BACKND.Database;
using Cysharp.Threading.Tasks;

namespace GameBerry.DB
{
    public static class Database
    {
        public static Client DBClient;

        public static async UniTask<bool> InitializeDatabase()
        {
            try
            {
                // 2. 데이터베이스 클라이언트 생성
                // 뒤끝 콘솔 > 데이터베이스 관리 메뉴에서 발급받은 DB UUID를 입력하세요.
                //DBClient = new Client(SceneManager.Instance.BuildEnvironmentData.Database);

                DBClient = new Client("019bd957-3a9a-7a11-8fee-edd43cc1a6e5");

                if (DBClient == null)
                {
                    Debug.LogError("[Database] Client creation failed.");
                    return false;
                }

                // 3. 데이터베이스 초기화
                await DBClient.Initialize();

                Debug.Log($"DB UserUUID: {DBClient?.UserUUID}");
                Debug.Log($"Backend.UserInDate: {BackEnd.Backend.UserInDate}");


                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Initialization exception: {ex}");
                return false;
            }
        }
    }
}
