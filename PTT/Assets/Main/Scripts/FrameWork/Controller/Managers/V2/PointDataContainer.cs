using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public static class PointDataContainer
    {
        public static string DiaAmount = "0";
        public static double DiaAmountRecord = 0;

        public static ObscuredDouble AccumUseDia = 0;

        public static Dictionary<int, ObscuredDouble> m_pointAmount = new Dictionary<int, ObscuredDouble>();

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        public static string GetSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in m_pointAmount)
            {
                SerializeString.Append(pair.Key - 199010000);
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
                m_pointAmount.Add(arrcontent[0].ToInt() + 199010000, arrcontent[1].ToDouble());
            }
        }
    }
}
