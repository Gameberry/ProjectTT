using System.Collections.Generic;
using UnityEngine;
using GameBerry;
using BackndChat;
using Gpm.Ui;

namespace GameBerry
{
    public class ChatBlockUser : InfiniteScrollData
    {
        public string UID = string.Empty;
        public string NickName = string.Empty;
    }

    public static class ChatContainer
    {
        public static Dictionary<string, ChatBlockUser> BlockUserList = new Dictionary<string, ChatBlockUser>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in BlockUserList)
            {
                SerializeString.Append(pair.Key);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.NickName);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                if (arrcontent.Length >= 2)
                {
                    ChatBlockUser chatBlockUser = new ChatBlockUser();
                    chatBlockUser.UID = arrcontent[0];
                    chatBlockUser.NickName = arrcontent[1];
                    BlockUserList.Add(arrcontent[0], chatBlockUser);
                }
            }
        }
    }
}