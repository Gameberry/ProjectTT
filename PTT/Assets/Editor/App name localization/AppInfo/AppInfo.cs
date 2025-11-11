using UnityEngine;



public class AppInfo : MonoBehaviour
{

    public string localization = "";
    public string app_name = "";
    public string idfa_key = "";

    public AppInfo(string Localization, string App_name, string Idfa_key)
    {
        localization = Localization;
        app_name = App_name;
        idfa_key = Idfa_key;
    }

}
