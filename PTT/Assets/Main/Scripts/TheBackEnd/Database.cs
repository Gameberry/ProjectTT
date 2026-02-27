using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BACKND.Database;
using LitJson;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using GameBerry.Managers;

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
                DBClient = new Client(SceneManager.Instance.BuildEnvironmentData.Database);

                if (DBClient == null)
                {
                    Debug.LogError("[Database] Client creation failed.");
                    return false;
                }

                // 3. 데이터베이스 초기화
                await DBClient.Initialize();
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
