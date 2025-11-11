using System;

public class CoinConverter
{
    public static string Convert(double valueToConvert)
    {
        string converted;
        double log10 = Math.Log10(valueToConvert);
        if (log10 >= 78) { converted = ((valueToConvert / 1e78)).ToString("N3") + "Z"; }
        else if (log10 >= 75) { converted = ((valueToConvert / 1e75)).ToString("N3") + "Y"; }
        else if (log10 >= 72) { converted = ((valueToConvert / 1e72)).ToString("N3") + "X"; }
        else if (log10 >= 69) { converted = ((valueToConvert / 1e69)).ToString("N3") + "W"; }
        else if (log10 >= 66) { converted = ((valueToConvert / 1e66)).ToString("N3") + "V"; }
        else if (log10 >= 63) { converted = ((valueToConvert / 1e63)).ToString("N3") + "U"; }
        else if (log10 >= 60) { converted = ((valueToConvert / 1e60)).ToString("N3") + "T"; }
        else if (log10 >= 57) { converted = ((valueToConvert / 1e57)).ToString("N3") + "S"; }
        else if (log10 >= 54) { converted = ((valueToConvert / 1e54)).ToString("N3") + "R"; }
        else if (log10 >= 51) { converted = ((valueToConvert / 1e51)).ToString("N3") + "Q"; }
        else if (log10 >= 48) { converted = ((valueToConvert / 1e48)).ToString("N3") + "P"; }
        else if (log10 >= 45) { converted = ((valueToConvert / 1e45)).ToString("N3") + "O"; }
        else if (log10 >= 42) { converted = ((valueToConvert / 1e42)).ToString("N3") + "N"; }
        else if (log10 >= 39) { converted = ((valueToConvert / 1e39)).ToString("N3") + "M"; }
        else if (log10 >= 36) { converted = ((valueToConvert / 1e36)).ToString("N3") + "L"; }
        else if (log10 >= 33) { converted = ((valueToConvert / 1e33)).ToString("N3") + "K"; }
        else if (log10 >= 30) { converted = ((valueToConvert / 1e30)).ToString("N3") + "J"; }
        else if (log10 >= 27) { converted = ((valueToConvert / 1e27)).ToString("N3") + "I"; }
        else if (log10 >= 24) { converted = ((valueToConvert / 1e24)).ToString("N3") + "H"; }
        else if (log10 >= 21) { converted = ((valueToConvert / 1e21)).ToString("N3") + "G"; }
        else if (log10 >= 18) { converted = ((valueToConvert / 1e18)).ToString("N3") + "F"; }
        else if (log10 >= 15) { converted = ((valueToConvert / 1e15)).ToString("N3") + "E"; }
        else if (log10 >= 12) { converted = ((valueToConvert / 1e12)).ToString("N3") + "D"; }
        else if (log10 >= 9) { converted = ((valueToConvert / 1e9)).ToString("N3") + "C"; }
        else if (log10 >= 6) { converted = ((valueToConvert / 1e6)).ToString("N3") + "B"; }
        else if (log10 >= 3) { converted = ((valueToConvert / 1e3)).ToString("N3") + "A"; }
        else
        {

            //converted = "" + valueToConvert.ToString("N");
            converted = string.Format("{0:0.###}", valueToConvert);
            //converted = converted.Replace(".000", "");
        }
        //converted = converted.Replace(".000", "");
        return converted;
    }

}

#region #표기수정 버전
//public static string Convert(double valueToConvert)
//{
//    string converted;
//    double log10 = Math.Log10(valueToConvert);
//    if (log10 >= 306) { converted = ((valueToConvert / 1e306)).ToString("N") + " Uc"; }
//    else if (log10 >= 303) { converted = ((valueToConvert / 1e303)).ToString("N") + " C"; }
//    else if (log10 >= 300) { converted = ((valueToConvert / 1e300)).ToString("N") + " NoNog"; }
//    else if (log10 >= 297) { converted = ((valueToConvert / 1e297)).ToString("N") + " OcNog"; }
//    else if (log10 >= 294) { converted = ((valueToConvert / 1e294)).ToString("N") + " SpNog"; }
//    else if (log10 >= 291) { converted = ((valueToConvert / 1e291)).ToString("N") + " SxNog"; }
//    else if (log10 >= 288) { converted = ((valueToConvert / 1e288)).ToString("N") + " QiNog"; }
//    else if (log10 >= 285) { converted = ((valueToConvert / 1e285)).ToString("N") + " QaNog"; }
//    else if (log10 >= 282) { converted = ((valueToConvert / 1e282)).ToString("N") + " TNog"; }
//    else if (log10 >= 279) { converted = ((valueToConvert / 1e279)).ToString("N") + " DNog"; }
//    else if (log10 >= 276) { converted = ((valueToConvert / 1e276)).ToString("N") + " UNog"; }
//    else if (log10 >= 273) { converted = ((valueToConvert / 1e273)).ToString("N") + " Nog"; }
//    else if (log10 >= 270) { converted = ((valueToConvert / 1e270)).ToString("N") + " NoOcg"; }
//    else if (log10 >= 267) { converted = ((valueToConvert / 1e267)).ToString("N") + " OcOcg"; }
//    else if (log10 >= 264) { converted = ((valueToConvert / 1e264)).ToString("N") + " SpOcg"; }
//    else if (log10 >= 261) { converted = ((valueToConvert / 1e261)).ToString("N") + " SxOcg"; }
//    else if (log10 >= 258) { converted = ((valueToConvert / 1e258)).ToString("N") + " QiOcg"; }
//    else if (log10 >= 255) { converted = ((valueToConvert / 1e255)).ToString("N") + " QaOcg"; }
//    else if (log10 >= 252) { converted = ((valueToConvert / 1e252)).ToString("N") + " TOcg"; }
//    else if (log10 >= 249) { converted = ((valueToConvert / 1e249)).ToString("N") + " DOcg"; }
//    else if (log10 >= 246) { converted = ((valueToConvert / 1e246)).ToString("N") + " UOcg"; }
//    else if (log10 >= 243) { converted = ((valueToConvert / 1e243)).ToString("N") + " Ocg"; }
//    else if (log10 >= 240) { converted = ((valueToConvert / 1e240)).ToString("N") + " NoSpg"; }
//    else if (log10 >= 237) { converted = ((valueToConvert / 1e237)).ToString("N") + " OcSpg"; }
//    else if (log10 >= 234) { converted = ((valueToConvert / 1e234)).ToString("N") + " SpSpg"; }
//    else if (log10 >= 231) { converted = ((valueToConvert / 1e231)).ToString("N") + " SxSpg"; }
//    else if (log10 >= 228) { converted = ((valueToConvert / 1e228)).ToString("N") + " QiSpg"; }
//    else if (log10 >= 225) { converted = ((valueToConvert / 1e225)).ToString("N") + " QaSpg"; }
//    else if (log10 >= 222) { converted = ((valueToConvert / 1e222)).ToString("N") + " TSpg"; }
//    else if (log10 >= 219) { converted = ((valueToConvert / 1e219)).ToString("N") + " DSpg"; }
//    else if (log10 >= 216) { converted = ((valueToConvert / 1e216)).ToString("N") + " USpg"; }
//    else if (log10 >= 213) { converted = ((valueToConvert / 1e213)).ToString("N") + " Spg"; }
//    else if (log10 >= 210) { converted = ((valueToConvert / 1e210)).ToString("N") + " NoSxg"; }
//    else if (log10 >= 207) { converted = ((valueToConvert / 1e207)).ToString("N") + " OcSxg"; }
//    else if (log10 >= 204) { converted = ((valueToConvert / 1e204)).ToString("N") + " SpSxg"; }
//    else if (log10 >= 201) { converted = ((valueToConvert / 1e201)).ToString("N") + " SxSxg"; }
//    else if (log10 >= 198) { converted = ((valueToConvert / 1e198)).ToString("N") + " QiSxg"; }
//    else if (log10 >= 195) { converted = ((valueToConvert / 1e195)).ToString("N") + " QaSxg"; }
//    else if (log10 >= 192) { converted = ((valueToConvert / 1e192)).ToString("N") + " TSxg"; }
//    else if (log10 >= 189) { converted = ((valueToConvert / 1e189)).ToString("N") + " DSxg"; }
//    else if (log10 >= 186) { converted = ((valueToConvert / 1e186)).ToString("N") + " USxg"; }
//    else if (log10 >= 183) { converted = ((valueToConvert / 1e183)).ToString("N") + " Sxg"; }
//    else if (log10 >= 180) { converted = ((valueToConvert / 1e180)).ToString("N") + " NoQig"; }
//    else if (log10 >= 177) { converted = ((valueToConvert / 1e177)).ToString("N") + " OcQig"; }
//    else if (log10 >= 174) { converted = ((valueToConvert / 1e174)).ToString("N") + " SpQig"; }
//    else if (log10 >= 171) { converted = ((valueToConvert / 1e171)).ToString("N") + " SxQig"; }
//    else if (log10 >= 168) { converted = ((valueToConvert / 1e168)).ToString("N") + " QiQig"; }
//    else if (log10 >= 165) { converted = ((valueToConvert / 1e165)).ToString("N") + " QaQig"; }
//    else if (log10 >= 162) { converted = ((valueToConvert / 1e162)).ToString("N") + " TQig"; }
//    else if (log10 >= 159) { converted = ((valueToConvert / 1e159)).ToString("N") + " DQig"; }
//    else if (log10 >= 156) { converted = ((valueToConvert / 1e156)).ToString("N") + " UQig"; }
//    else if (log10 >= 153) { converted = ((valueToConvert / 1e153)).ToString("N") + " Qig"; }
//    else if (log10 >= 150) { converted = ((valueToConvert / 1e150)).ToString("N") + " Noqag"; }
//    else if (log10 >= 147) { converted = ((valueToConvert / 1e147)).ToString("N") + " Ocqag"; }
//    else if (log10 >= 144) { converted = ((valueToConvert / 1e144)).ToString("N") + " Spqag"; }
//    else if (log10 >= 141) { converted = ((valueToConvert / 1e141)).ToString("N") + " Sxqag"; }
//    else if (log10 >= 138) { converted = ((valueToConvert / 1e138)).ToString("N") + " Qiqag"; }
//    else if (log10 >= 135) { converted = ((valueToConvert / 1e135)).ToString("N") + " Qaqag"; }
//    else if (log10 >= 132) { converted = ((valueToConvert / 1e132)).ToString("N") + " Tqag"; }
//    else if (log10 >= 129) { converted = ((valueToConvert / 1e129)).ToString("N") + " Dqag"; }
//    else if (log10 >= 126) { converted = ((valueToConvert / 1e126)).ToString("N") + " Uqag"; }
//    else if (log10 >= 123) { converted = ((valueToConvert / 1e123)).ToString("N") + " Qag"; }
//    else if (log10 >= 120) { converted = ((valueToConvert / 1e120)).ToString("N") + " Notg"; }
//    else if (log10 >= 117) { converted = ((valueToConvert / 1e117)).ToString("N") + " Octg"; }
//    else if (log10 >= 114) { converted = ((valueToConvert / 1e114)).ToString("N") + " Sptg"; }
//    else if (log10 >= 111) { converted = ((valueToConvert / 1e111)).ToString("N") + " Sxtg"; }
//    else if (log10 >= 108) { converted = ((valueToConvert / 1e108)).ToString("N") + " Qitg"; }
//    else if (log10 >= 105) { converted = ((valueToConvert / 1e105)).ToString("N") + " Qatg"; }
//    else if (log10 >= 102) { converted = ((valueToConvert / 1e102)).ToString("N") + " Ttg"; }
//    else if (log10 >= 99) { converted = ((valueToConvert / 1e99)).ToString("N") + " Dtg"; }
//    else if (log10 >= 96) { converted = ((valueToConvert / 1e96)).ToString("N") + " Utg"; }
//    else if (log10 >= 93) { converted = ((valueToConvert / 1e93)).ToString("N") + " Tg"; }
//    else if (log10 >= 90) { converted = ((valueToConvert / 1e90)).ToString("N") + " NVi"; }
//    else if (log10 >= 87) { converted = ((valueToConvert / 1e87)).ToString("N") + " OVi"; }
//    else if (log10 >= 84) { converted = ((valueToConvert / 1e84)).ToString("N") + " SpVi"; }
//    else if (log10 >= 81) { converted = ((valueToConvert / 1e81)).ToString("N") + " SxVi"; }
//    else if (log10 >= 78) { converted = ((valueToConvert / 1e78)).ToString("N") + " QiVi"; }
//    else if (log10 >= 75) { converted = ((valueToConvert / 1e75)).ToString("N") + " QaVi"; }
//    else if (log10 >= 72) { converted = ((valueToConvert / 1e72)).ToString("N") + " TVi"; }
//    else if (log10 >= 69) { converted = ((valueToConvert / 1e69)).ToString("N") + " DVi"; }
//    else if (log10 >= 66) { converted = ((valueToConvert / 1e66)).ToString("N") + " UVi"; }
//    else if (log10 >= 63) { converted = ((valueToConvert / 1e63)).ToString("N") + " Vi"; }
//    else if (log10 >= 60) { converted = ((valueToConvert / 1e60)).ToString("N") + " NDc"; }
//    else if (log10 >= 57) { converted = ((valueToConvert / 1e57)).ToString("N") + " OcDc"; }
//    else if (log10 >= 54) { converted = ((valueToConvert / 1e54)).ToString("N") + " SpDc"; }
//    else if (log10 >= 51) { converted = ((valueToConvert / 1e51)).ToString("N") + " SxDc"; }
//    else if (log10 >= 48) { converted = ((valueToConvert / 1e48)).ToString("N") + " QiDc"; }
//    else if (log10 >= 45) { converted = ((valueToConvert / 1e45)).ToString("N") + " QaDc"; }
//    else if (log10 >= 42) { converted = ((valueToConvert / 1e42)).ToString("N") + " TDc"; }
//    else if (log10 >= 39) { converted = ((valueToConvert / 1e39)).ToString("N") + " DDc"; }
//    else if (log10 >= 36) { converted = ((valueToConvert / 1e36)).ToString("N") + " UDc"; }
//    else if (log10 >= 33) { converted = ((valueToConvert / 1e33)).ToString("N") + " Dc"; }
//    else if (log10 >= 30) { converted = ((valueToConvert / 1e30)).ToString("N") + " N"; }
//    else if (log10 >= 27) { converted = ((valueToConvert / 1e27)).ToString("N") + " Oc"; }
//    else if (log10 >= 24) { converted = ((valueToConvert / 1e24)).ToString("N") + " Sp"; }
//    else if (log10 >= 21) { converted = ((valueToConvert / 1e21)).ToString("N") + " Sx"; }
//    else if (log10 >= 18) { converted = ((valueToConvert / 1e18)).ToString("N") + " Qi"; }
//    else if (log10 >= 15) { converted = ((valueToConvert / 1e15)).ToString("N") + " Qa"; }
//    else if (log10 >= 12) { converted = ((valueToConvert / 1e12)).ToString("N") + " T"; }
//    else if (log10 >= 9) { converted = ((valueToConvert / 1e9)).ToString("N") + " B"; }
//    else if (log10 >= 6) { converted = ((valueToConvert / 1e6)).ToString("N") + " M"; }
//    else if (log10 >= 3) { converted = ((valueToConvert / 1000)).ToString("N") + " K"; }
//    else
//    {

//        converted = "" + valueToConvert.ToString("N");
//    }

//    converted = converted.Replace(".00", "");
//    return converted;
//}
#endregion

#region  # 원본소스

//public static string Convert(double valueToConvert)
//{
//    string converted;

//    if (Math.Log10(valueToConvert) >= 306) { converted = ((valueToConvert / 1e306)).ToString("N") + " Uncentillion"; }
//    else if (Math.Log10(valueToConvert) >= 303) { converted = ((valueToConvert / 1e303)).ToString("N") + " Centillion"; }
//    else if (Math.Log10(valueToConvert) >= 300) { converted = ((valueToConvert / 1e300)).ToString("N") + " Novemnonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 297) { converted = ((valueToConvert / 1e297)).ToString("N") + " Onctononagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 294) { converted = ((valueToConvert / 1e294)).ToString("N") + " Septnonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 291) { converted = ((valueToConvert / 1e291)).ToString("N") + " Sexnonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 288) { converted = ((valueToConvert / 1e288)).ToString("N") + " Quinnonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 285) { converted = ((valueToConvert / 1e285)).ToString("N") + " Quattuornonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 282) { converted = ((valueToConvert / 1e282)).ToString("N") + " Trenonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 279) { converted = ((valueToConvert / 1e279)).ToString("N") + " Duononagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 276) { converted = ((valueToConvert / 1e276)).ToString("N") + " Unnonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 273) { converted = ((valueToConvert / 1e273)).ToString("N") + " Nonagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 270) { converted = ((valueToConvert / 1e270)).ToString("N") + " Novemoctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 267) { converted = ((valueToConvert / 1e267)).ToString("N") + " Octooctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 264) { converted = ((valueToConvert / 1e264)).ToString("N") + " Septoctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 261) { converted = ((valueToConvert / 1e261)).ToString("N") + " Sexoctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 258) { converted = ((valueToConvert / 1e258)).ToString("N") + " Quinoctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 255) { converted = ((valueToConvert / 1e255)).ToString("N") + " Quattuoroctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 252) { converted = ((valueToConvert / 1e252)).ToString("N") + " Treoctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 249) { converted = ((valueToConvert / 1e249)).ToString("N") + " Duooctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 246) { converted = ((valueToConvert / 1e246)).ToString("N") + " Unoctogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 243) { converted = ((valueToConvert / 1e243)).ToString("N") + " Octogintillion"; }
//    else if (Math.Log10(valueToConvert) >= 240) { converted = ((valueToConvert / 1e240)).ToString("N") + " Novemseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 237) { converted = ((valueToConvert / 1e237)).ToString("N") + " Octoseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 234) { converted = ((valueToConvert / 1e234)).ToString("N") + " Septseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 231) { converted = ((valueToConvert / 1e231)).ToString("N") + " Sexseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 228) { converted = ((valueToConvert / 1e228)).ToString("N") + " Quinseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 225) { converted = ((valueToConvert / 1e225)).ToString("N") + " Quattuorseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 222) { converted = ((valueToConvert / 1e222)).ToString("N") + " Treseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 219) { converted = ((valueToConvert / 1e219)).ToString("N") + " Duoseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 216) { converted = ((valueToConvert / 1e216)).ToString("N") + " Unseptuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 213) { converted = ((valueToConvert / 1e213)).ToString("N") + " Septuagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 210) { converted = ((valueToConvert / 1e210)).ToString("N") + " Novemsexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 207) { converted = ((valueToConvert / 1e207)).ToString("N") + " Octosexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 204) { converted = ((valueToConvert / 1e204)).ToString("N") + " Septsexaginillion"; }
//    else if (Math.Log10(valueToConvert) >= 201) { converted = ((valueToConvert / 1e201)).ToString("N") + " Sexsexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 198) { converted = ((valueToConvert / 1e198)).ToString("N") + " Quinsexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 195) { converted = ((valueToConvert / 1e195)).ToString("N") + " Quattuorsexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 192) { converted = ((valueToConvert / 1e192)).ToString("N") + " Tresexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 189) { converted = ((valueToConvert / 1e198)).ToString("N") + " Duosexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 186) { converted = ((valueToConvert / 1e186)).ToString("N") + " Unsexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 183) { converted = ((valueToConvert / 1e183)).ToString("N") + " Sexagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 180) { converted = ((valueToConvert / 1e180)).ToString("N") + " Novemquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 177) { converted = ((valueToConvert / 1e177)).ToString("N") + " Octoquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 174) { converted = ((valueToConvert / 1e174)).ToString("N") + " Septquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 171) { converted = ((valueToConvert / 1e171)).ToString("N") + " Sexquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 168) { converted = ((valueToConvert / 1e168)).ToString("N") + " Quinquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 165) { converted = ((valueToConvert / 1e165)).ToString("N") + " Quattuorquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 162) { converted = ((valueToConvert / 1e162)).ToString("N") + " Trequinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 159) { converted = ((valueToConvert / 1e159)).ToString("N") + " Duoquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 156) { converted = ((valueToConvert / 1e156)).ToString("N") + " Unquinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 153) { converted = ((valueToConvert / 1e153)).ToString("N") + " Quinquagintillion"; }
//    else if (Math.Log10(valueToConvert) >= 150) { converted = ((valueToConvert / 1e150)).ToString("N") + " Novemquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 147) { converted = ((valueToConvert / 1e147)).ToString("N") + " Octoquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 144) { converted = ((valueToConvert / 1e144)).ToString("N") + " Septquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 141) { converted = ((valueToConvert / 1e141)).ToString("N") + " Sexquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 138) { converted = ((valueToConvert / 1e138)).ToString("N") + " Quinquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 135) { converted = ((valueToConvert / 1e135)).ToString("N") + " Quattuorquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 132) { converted = ((valueToConvert / 1e132)).ToString("N") + " Trequadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 129) { converted = ((valueToConvert / 1e129)).ToString("N") + " Duoquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 126) { converted = ((valueToConvert / 1e126)).ToString("N") + " Unquadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 123) { converted = ((valueToConvert / 1e123)).ToString("N") + " Quadragintillion"; }
//    else if (Math.Log10(valueToConvert) >= 120) { converted = ((valueToConvert / 1e120)).ToString("N") + " Novemtrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 117) { converted = ((valueToConvert / 1e117)).ToString("N") + " Octotrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 114) { converted = ((valueToConvert / 1e114)).ToString("N") + " Septentrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 111) { converted = ((valueToConvert / 1e111)).ToString("N") + " Sextrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 108) { converted = ((valueToConvert / 1e108)).ToString("N") + " Quintrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 105) { converted = ((valueToConvert / 1e105)).ToString("N") + " Quattuortrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 102) { converted = ((valueToConvert / 1e102)).ToString("N") + " Tretrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 99) { converted = ((valueToConvert / 1e99)).ToString("N") + " Duotrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 96) { converted = ((valueToConvert / 1e96)).ToString("N") + " Untrigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 93) { converted = ((valueToConvert / 1e93)).ToString("N") + " Trigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 90) { converted = ((valueToConvert / 1e90)).ToString("N") + " Novemvigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 87) { converted = ((valueToConvert / 1e87)).ToString("N") + " Octovigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 84) { converted = ((valueToConvert / 1e84)).ToString("N") + " Septenvigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 81) { converted = ((valueToConvert / 1e81)).ToString("N") + " Sexvigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 78) { converted = ((valueToConvert / 1e78)).ToString("N") + " Quinvigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 75) { converted = ((valueToConvert / 1e75)).ToString("N") + " Quattuorvigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 72) { converted = ((valueToConvert / 1e72)).ToString("N") + " Tresvigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 69) { converted = ((valueToConvert / 1e69)).ToString("N") + " Duovigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 66) { converted = ((valueToConvert / 1e66)).ToString("N") + " Unvigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 63) { converted = ((valueToConvert / 1e63)).ToString("N") + " Vigintillion"; }
//    else if (Math.Log10(valueToConvert) >= 60) { converted = ((valueToConvert / 1e60)).ToString("N") + " Novemdecillion"; }
//    else if (Math.Log10(valueToConvert) >= 57) { converted = ((valueToConvert / 1e57)).ToString("N") + " Octodecillion"; }
//    else if (Math.Log10(valueToConvert) >= 54) { converted = ((valueToConvert / 1e54)).ToString("N") + " Septendecillion"; }
//    else if (Math.Log10(valueToConvert) >= 51) { converted = ((valueToConvert / 1e51)).ToString("N") + " Sexdecillion"; }
//    else if (Math.Log10(valueToConvert) >= 48) { converted = ((valueToConvert / 1e48)).ToString("N") + " Quindecillion"; }
//    else if (Math.Log10(valueToConvert) >= 45) { converted = ((valueToConvert / 1e45)).ToString("N") + " Quattuordecillion"; }
//    else if (Math.Log10(valueToConvert) >= 42) { converted = ((valueToConvert / 1e42)).ToString("N") + " Tredecillion"; }
//    else if (Math.Log10(valueToConvert) >= 39) { converted = ((valueToConvert / 1e39)).ToString("N") + " Duodecillion"; }
//    else if (Math.Log10(valueToConvert) >= 36) { converted = ((valueToConvert / 1e36)).ToString("N") + " Undecillion"; }
//    else if (Math.Log10(valueToConvert) >= 33) { converted = ((valueToConvert / 1e33)).ToString("N") + " Decillion"; }
//    else if (Math.Log10(valueToConvert) >= 30) { converted = ((valueToConvert / 1e30)).ToString("N") + " Nonillion"; }
//    else if (Math.Log10(valueToConvert) >= 27) { converted = ((valueToConvert / 1e27)).ToString("N") + " Octillion"; }
//    else if (Math.Log10(valueToConvert) >= 24) { converted = ((valueToConvert / 1e24)).ToString("N") + " Septillion"; }
//    else if (Math.Log10(valueToConvert) >= 21) { converted = ((valueToConvert / 1e21)).ToString("N") + " Sextillion"; }
//    else if (Math.Log10(valueToConvert) >= 18) { converted = ((valueToConvert / 1e18)).ToString("N") + " Quintillion"; }
//    else if (Math.Log10(valueToConvert) >= 15) { converted = ((valueToConvert / 1e15)).ToString("N") + " Quadrillion"; }
//    else if (Math.Log10(valueToConvert) >= 12) { converted = ((valueToConvert / 1e12)).ToString("N") + " Trillion"; }
//    else if (Math.Log10(valueToConvert) >= 9) { converted = ((valueToConvert / 1e9)).ToString("N") + " Billion"; }
//    else if (Math.Log10(valueToConvert) >= 6) { converted = ((valueToConvert / 1e6)).ToString("N") + " Million"; }
//    else if (Math.Log10(valueToConvert) >= 3) { converted = ((valueToConvert / 1000)).ToString("N") + " Thousand"; }

//    else
//    {

//        converted = "" + valueToConvert.ToString("N");
//    }

//    return converted;
//}

#endregion