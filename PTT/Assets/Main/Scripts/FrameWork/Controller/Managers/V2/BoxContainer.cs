using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class BoxContainer
    {
        public static Dictionary<int, ObscuredDouble> m_boxAmount = new Dictionary<int, ObscuredDouble>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static bool needReddot = false;

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_boxAmount)
            {
                SerializeString.Append(pair.Key - 174010000);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value);
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
                double amount = arrcontent[1].ToDouble();

                m_boxAmount.Add(arrcontent[0].ToInt() + 174010000, amount);

                if (amount > 0)
                    needReddot = true;
            }
        }
    }
}