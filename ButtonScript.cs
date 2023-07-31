using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    public GameObject contentInput;
    public GameObject startIndexInput;
    public GameObject algorithmInput;
    public static string preContent = "";
    public static string preIndex = "";
    public static string preAlg = "";
    public GameObject script;
    public GameObject AndroidUI;
    public GameObject WinUI;
    static string helpFilePath;
    static string sumFilePath;
    static string p1FilePath;
    static string m1FilePath;
    static string algFolderPath;
    static bool triedCreation = false;
    public static bool isAndroid = false;
    public static bool isAndroidMode = false;
    public static bool isPlatformManual = false;
    public static void WriteFile(string path, string content)
    {
        if (!isAndroid)
        {
            File.Create(path).Close();
            File.WriteAllText(path, content);
        } else
        {
            StreamWriter writer = new(path);
            writer.Write(content);
            writer.Close();
        }
    }
    void Start()
    {
        try
        {
#if PLATFORM_ANDROID
            isAndroid = true;
#endif
            if (!isPlatformManual)
            {
#if PLATFORM_ANDROID
                isAndroidMode = true;
#endif
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
            algFolderPath = Application.persistentDataPath + "/algorithms";
            sumFilePath = Application.persistentDataPath + "/algorithms/sum.txt";
            p1FilePath = Application.persistentDataPath + "/algorithms/plus1.txt";
            m1FilePath = Application.persistentDataPath + "/algorithms/minus1.txt";
            helpFilePath = Application.persistentDataPath + "/langDescription.txt";

            if (!Directory.Exists(algFolderPath))
            {
                Directory.CreateDirectory(algFolderPath);
            }
            if (!File.Exists(sumFilePath))
            {
                byte[] byte64 = Convert.FromBase64String(sumBase64);
                string s1 = Encoding.UTF8.GetString(byte64);
                WriteFile(sumFilePath, s1);
            }
            if (!File.Exists(p1FilePath))
            {
                byte[] byte64 = Convert.FromBase64String(plus1B64);
                string s1 = Encoding.UTF8.GetString(byte64);
                WriteFile(p1FilePath, s1);
            }
            if (!File.Exists(m1FilePath))
            {
                byte[] byte64 = Convert.FromBase64String(minus1B64);
                string s1 = Encoding.UTF8.GetString(byte64);
                WriteFile(m1FilePath, s1);
            }
            byte[] byte64h = Convert.FromBase64String(helpBase64);
            string s1h = Encoding.UTF8.GetString(byte64h);
            WriteFile(helpFilePath, s1h);

            try
            {
                if (isAndroidMode)
                {
                    WinUI.SetActive(false);
                    AndroidUI.SetActive(true);
                }
                else
                {
                    WinUI.SetActive(true);
                    AndroidUI.SetActive(false);
                }
            }
            catch (NullReferenceException) { }
            catch (UnassignedReferenceException) { }
        }
        catch (DirectoryNotFoundException e1) {
            if (isAndroid)
            {
                Main.HandleError_NonAlg("디렉토리를 찾을 수 없습니다.", "권한이 없습니다. 앱 설정에서 미디어 권한을 허용해주세요.");
            }
            else
            {
                Main.HandleError_NonAlg(e1.ToString(), "internal");
            }
        }
        catch (Exception e)
        {
            if (!triedCreation) { triedCreation = true; Main.HandleError_NonAlg(e.ToString(), "internal"); }
            return;
        }

        try
        {
            algorithmInput.GetComponent<TMP_InputField>().text = preAlg;
            contentInput.GetComponent<TMP_InputField>().text = preContent;
            startIndexInput.GetComponent<TMP_InputField>().text = preIndex;
        }
        catch (UnassignedReferenceException) { /* StartScene 이외의 Scene에서는 에러가 발생합니다. */ }
    }

    public void StartAlgorithm()
    {
        try
        {
            preAlg = algorithmInput.GetComponent<TMP_InputField>().text;
            preContent = contentInput.GetComponent<TMP_InputField>().text;
            preIndex = startIndexInput.GetComponent<TMP_InputField>().text;

            string contentstr = contentInput.GetComponent<TMP_InputField>().text;
            int[] content = new int[contentstr.Length];
            try
            {
                for (int i = 0; i < contentstr.Length; i++)
                {
                    if (contentstr[i].Equals('b'))
                    {
                        content[i] = -1;
                    }
                    else
                    {
                        content[i] = int.Parse($"{contentstr[i]}");
                    }
                }
            }
            catch (FormatException)
            {
                Main.HandleError_NonAlg("문자열을 정수로 변환하지 못했습니다.", "격자에 잘못된 값이 입력되었습니다. 격자의 값은 0~9와 b(빈칸)만 가능합니다.");
                return;
            }

            //check
            try
            {
                if ((content[0] != -1) || (content[^1] != -1))
                {
                    Main.HandleError_NonAlg("격자의 값이 잘못 설정되었습니다.", "첫 격자와 마지막 격자의 값이 빈칸(b)이 아닙니다.", content);
                    return;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Main.HandleError_NonAlg("격자의 값을 입력하지 않았습니다.", "격자의 값을 입력하지 않았습니다.");
                return;
            }
            string algorithmFilePath = Application.persistentDataPath + $"/algorithms/{algorithmInput.GetComponent<TMP_InputField>().text}.txt";
            if (!File.Exists(algorithmFilePath))
            {
                Main.HandleError_NonAlg($"알고리즘({algorithmInput.GetComponent<TMP_InputField>().text})을 찾을 수 없습니다.",
                    "이름을 잘못 입력했거나, 알고리즘을 잘못된 방법으로 생성했을 수 있습니다. 도움말을 참고하세요.");
                return;
            }
            try
            {
                if ((int.Parse(startIndexInput.GetComponent<TMP_InputField>().text) >= content.Length) || (int.Parse(startIndexInput.GetComponent<TMP_InputField>().text) < 0))
                {
                    Main.HandleError_NonAlg("헤더의 초기 위치가 가능한 범위를 벗어났습니다.", "번호를 잘못 입력했거나, 첫 격자의 번호가 0임을 고려하지 않았을 가능성이 높습니다.");
                    return;
                }
            }
            catch (OverflowException)
            {
                Main.HandleError_NonAlg("헤더의 초기 위치가 가능한 범위를 벗어났습니다.", $"헤더의 위치 값이 {int.MaxValue}보다 크거나 \n{int.MinValue}보다 작습니다.", content);
                return;
            }

            //start
            Main.content = content;
            Main.startIndex = int.Parse(startIndexInput.GetComponent<TMP_InputField>().text);
            Main.algorithmName = algorithmInput.GetComponent<TMP_InputField>().text;
            SceneManager.LoadScene("ExecuteScene");
        } catch(Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
            return;
        }
    }

    public void Execute(string type)
    {
        try
        {
            if (script.GetComponent<Main>().executeOnce || script.GetComponent<Main>().executeContinuously || script.GetComponent<Main>().executeFast)
            {
                script.GetComponent<Main>().executeOnce = false;
                script.GetComponent<Main>().executeContinuously = false;
                script.GetComponent<Main>().executeFast = false;
                return;
            }
            switch (type)
            {
                case "once":
                    script.GetComponent<Main>().executeOnce = !script.GetComponent<Main>().executeOnce;
                    break;
                case "cont":
                    script.GetComponent<Main>().executeContinuously = !script.GetComponent<Main>().executeContinuously;
                    break;
                case "fast":
                    script.GetComponent<Main>().executeFast = !script.GetComponent<Main>().executeFast;
                    break;
                case "result":
                    script.GetComponent<Main>().executeFast = !script.GetComponent<Main>().executeFast;
                    script.GetComponent<Main>().showResult = !script.GetComponent<Main>().showResult;
                    CameraManager.moveCamera = !CameraManager.moveCamera;
                    break;

            }
        } catch (Exception e) {
            Main.HandleError_NonAlg(e.ToString(), "internal");
            return;
        }
    }
    public void Exit()
    {
        if(Main.logExecution) UnityEngine.Debug.Log("Quit");
        Application.Quit();
    }
    public void OpenAlgFolder()
    {
        try
        {
            if (isAndroidMode) { SceneManager.LoadScene("AlgScene"); }
            else
            { Process.Start(algFolderPath); }
        }
        catch (Win32Exception)
        {
            Main.HandleError_NonAlg("Windows 오류", "Android에서 Windows 모드를 사용했습니다. 'Android 모드로 전환' 버튼을 눌러 Android 모드를 사용하세요.");
        }
        catch (Exception e) {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }
    public void OpenHelp()
    {
        try
        {
            if (isAndroidMode) { SceneManager.LoadScene("HelpScene"); }
            else { Process.Start(helpFilePath); }
        }
        catch (Win32Exception)
        {
            Main.HandleError_NonAlg("Windows 오류", "Android에서 Windows 모드를 사용했습니다. 'Android 모드로 전환' 버튼을 눌러 Android 모드를 사용하세요.");
        }
        catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }
    public void ReturnToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
    public void OpenLogScene()
    {
        SceneManager.LoadScene("LogScene");
    }
    public void ChangePlatform()
    {
        isAndroidMode = !isAndroidMode;
        isPlatformManual = true;
        SceneManager.LoadScene("StartScene");
    }
    public void OpenScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    static readonly string plus1B64 = "c3RhcnRBdCBhDQovL+ygnOyLnOusuOyXkCDrgpjsmLQNCjphDQowLT4wLFIsYQ0KMS0+MSxSLGENCjItPjIsUixhDQozL" +
        "T4zLFIsYQ0KNC0+NCxSLGENCjUtPjUsUixhDQo2LT42LFIsYQ0KNy0+NyxSLGENCjgtPjgsUixhDQo5LT45LFIsYQ0KYi0+YixMLGINCmVuZA0KDQo6Yg0KOS0+M" +
        "CxMLGINCmItPjEsTCxjDQowLT4xLEwsYw0KMS0+MixMLGMNCjItPjMsTCxjDQozLT40LEwsYw0KNC0+NSxMLGMNCjUtPjYsTCxjDQo2LT43LEwsYw0KNy0+OCxM" +
        "LGMNCjgtPjksTCxjDQplbmQNCg0KOmMNCjAtPjAsTCxjDQoxLT4xLEwsYw0KMi0+MixMLGMNCjMtPjMsTCxjDQo0LT40LEwsYw0KNS0+NSxMLGMNCjYtPjYsTCxj" +
        "DQo3LT43LEwsYw0KOC0+OCxMLGMNCjktPjksTCxjDQpiLT5iLFIsZA0KZW5kDQoNCjpkDQplbmQ=";

    static readonly string minus1B64 = "c3RhcnRBdCBlDQovL+usuOygnCAx67KIIOuLtQ0KOm4NCnN0b3ANCmVuZA0KDQo6ZQ0KYi0+YixMLG4NCjAtPjAsUixlD" +
        "QoxLT4xLFIsZg0KMi0+MixSLGYNCjMtPjMsUixmDQo0LT40LFIsZg0KNS0+NSxSLGYNCjYtPjYsUixmDQo3LT43LFIsZg0KOC0+OCxSLGYNCjktPjksUixmDQplb" +
        "mQNCg0KOmYNCjAtPjAsUixmDQoxLT4xLFIsZg0KMi0+MixSLGYNCjMtPjMsUixmDQo0LT40LFIsZg0KNS0+NSxSLGYNCjYtPjYsUixmDQo3LT43LFIsZg0KOC0+O" +
        "CxSLGYNCjktPjksUixmDQpiLT5iLEwsZw0KZW5kDQoNCjpnDQowLT45LEwsZw0KMS0+MCxMLGgNCjItPjEsTCxoDQozLT4yLEwsaA0KNC0+MyxMLGgNCjUtPjQsT" +
        "CxoDQo2LT41LEwsaA0KNy0+NixMLGgNCjgtPjcsTCxoDQo5LT44LEwsaA0KZW5kDQoNCjpoDQoxLT4xLEwsaA0KMi0+MixMLGgNCjMtPjMsTCxoDQo0LT40LEwsaA" +
        "0KNS0+NSxMLGgNCjYtPjYsTCxoDQo3LT43LEwsaA0KOC0+OCxMLGgNCjktPjksTCxoDQowLT4wLEwsaA0KYi0+YixSLG0NCmVuZA0KDQo6bQ0KZW5k";

    static readonly string sumBase64 = "c3RhcnRBdCBzDQovL+usuOygnCAy67KIIOuLtQ0KOnMNCjAtPjAsUixzDQoxLT4xLFIscw0KMi0+MixSLHMNCjMtPjMsUixzD" +
        "Qo0LT40LFIscw0KNS0+NSxSLHMNCjYtPjYsUixzDQo3LT43LFIscw0KOC0+OCxSLHMNCjktPjksUixzDQpiLT5iLFIsYWxnX21pbg0KZW5kDQoNCjphbGdfbWluDQphbGc" +
        "gbWludXMxDQpnb3RvIHQNCmVuZA0KDQo6dA0KMC0+MCxMLHQNCjEtPjEsTCx0DQoyLT4yLEwsdA0KMy0+MyxMLHQNCjQtPjQsTCx0DQo1LT41LEwsdA0KNi0+NixMLHQNC" +
        "jctPjcsTCx0DQo4LT44LEwsdA0KOS0+OSxMLHQNCmItPmIsTCx1DQplbmQNCg0KOnUNCjAtPjAsTCx1DQoxLT4xLEwsdQ0KMi0+MixMLHUNCjMtPjMsTCx1DQo0LT40LEws" +
        "dQ0KNS0+NSxMLHUNCjYtPjYsTCx1DQo3LT43LEwsdQ0KOC0+OCxMLHUNCjktPjksTCx1DQpiLT5iLFIsYWxnX3BsdXMNCmVuZA0KDQo6YWxnX3BsdXMNCmFsZyBwbHVzMQ" +
        "0KZ290byBzDQplbmQ=";

    public static readonly string helpBase64 = "7Ja47Ja0IOuqhey5rSA6IEdUQSAoR3NocyBUdXJpbmcgbWFjaGluZSBBbGdvcml0aG0pCgrslrjslrQg7ISk66qFIDoK" +
        "CeydtCDslrjslrTripQg6rK96riw6rO86rOgIDI07ZWZ64WE64+EIOyYgeyerOyEse2PieqwgCAy6rWQ7Iuc7JeQIOuCmOyYqCDri6jsiJztmZTrkJwg7Yqc66eBIOuouOy" +
        "LoCjqtaztmIQg7IucIOuzgO2YleuQqCnsnYQg7Iuk7ZaJ7ZWp64uI64ukLgoJ7Yqc66eBIOuouOyLoOydgCDsl7Dsho3soIHsnLzroZwg67Cw7Je065CcIOqyqeyekOuTpO" +
        "qzvCDtl6TrjZQsIOq3uOumrOqzoCDslYzqs6DrpqzsppjsnLzroZwg6rWs7ISx65Cp64uI64ukLgoKCeqyqeyekOuTpOyXkOuKlCAwfjnsnZgg7Iir7J6Q65Ok6rO8IOu5i" +
        "Oy5uCjilqEp7J2EIOyggOyepe2VoCDsiJgg7J6I7Iq164uI64ukLgoJ7J2065WMLCDsspjsnYzqs7wg64Gd7J2YIOqyqeyekOydmCDqsJLsnYAg7ZWt7IOBIOu5iOy5uChi" +
        "66GcIOyeheugpSnsnbTslrTslbwg7ZWp64uI64ukLgoJ7JWM6rOg66as7KaY7J2AIOyVjOqzoOumrOymmOydmCDsg4Htg5zsmYAg6re4IOyDge2DnOyXkOyEnCDsiJjtlon" +
        "tlaAg66qF66C57Jy866GcIOq1rOyEseuQmOyWtCDsnojsirXri4jri6QuCgkn66y467KVIOyEpOuqhSfsl5DshJwg642UIOyekOyEuO2VnCDrgrTsmqnsnbQg7ISk66qF6" +
        "5CY7Ja0IOyeiOyKteuLiOuLpC4KCe2XpOuNlOuKlCDqsqnsnpDrpbwg6rCA66as7YKk66mwLCDrqoXroLnsnYQg7Ya17ZW0IOyZvOyqveydtOuCmCDsmKTrpbjsqr3snLz" +
        "roZwg7J2064+Z7ZWgIOyImCDsnojsirXri4jri6QuCgntlITroZzqt7jrnqjsnYAg7ZWt7IOBIO2XpOuNlOqwgCDqsIDrpqztgqTripQg6rKp7J6Q7J2YIOqwkijqsqnsnpD" +
        "qsJIp7J2EIOydveyKteuLiOuLpC4KCgnslYzqs6DrpqzsppjsnYAg64uk7J2M6rO8IOqwmeydgCDrsKnrspXsnLzroZwg7IOd7ISx7ZWY6rGw64KYIOyImOygle2VoCDsiJg" +
        "g7J6I7Iq164uI64ukLgoKCVdpbmRvd3Psl5DshJw6Cgkn7JWM6rOg66as7KaYIO2PtOuNlCDsl7TquLAnIOuyhO2KvOydhCDtgbTrpq3tlZjrqbQg7JWM6rOg66as7KaY7J2" +
        "EIOyggOyepe2VmOuKlCDtj7TrjZTqsIAg7Je066a964uI64ukLgoJ6re4IO2PtOuNlOyXkCDslYzqs6DrpqzsppjsnZgg7J2066aE6rO8IOuPmeydvO2VnCDsnbTrpoTsnYQg" +
        "6rCW64qUIO2FjeyKpO2KuCDtjIzsnbzsnYQg7IOd7ISx7ZWcIOuSpCwg64K07Jqp7J2EIOyeheugpe2VmOqzoCDsoIDsnqXtlZjrqbQgCgntlITroZzqt7jrnqjsl5DshJwg7" +
        "JWM6rOg66as7KaY7J2EIOyduOyLne2VoCDsiJgg7J6I7Iq164uI64ukLgoJ7JWM6rOg66as7KaY7J2AIOusuOuylSDshKTrqoXsl5Ag65Sw6528IOyeheugpe2VmOyLnOupt" +
        "CDrkKnri4jri6QuCgnslYzqs6Drpqzsppgg7IOd7ISxL+yImOyglSDtm4Qg7Iuk7ZaJ7ZWY6riwIOuyhO2KvOydhCDriITrpbTsi5zrqbQg7Iuk7ZaJ65Cp64uI64ukLgoJKy" +
        "BBbmRyb2lkIOuqqOuTnOuPhCDsnbTsmqkg6rCA64ql7ZWp64uI64ukLiDslYTrnpjrpbwg7LC46rOg7ZWY7IS47JqULgoKCUFuZHJvaWTsl5DshJw6Cgkn7JWM6rOg66as7Ka" +
        "YIO2OuOynke2VmOq4sCcg67KE7Yq87J2EIOuIhOultOuptCDslYzqs6Drpqzsppgg66as7Iqk7Yq466W8IOuzvCDsiJgg7J6I7Iq164uI64ukLiAKCeybkO2VmOuKlCDslYzq" +
        "s6DrpqzsppjsnYQg7ISg7YOd7ZWY6rGw64KYIOyDneyEse2VnCDrkqQsIOuCtOyaqeydhCDsnoXroKXtlZjqs6Ag7KCA7J6l7ZWY66m0CgntlITroZzqt7jrnqjsl5DshJwg" +
        "7JWM6rOg66as7KaY7J2EIOyduOyLne2VoCDsiJgg7J6I7Iq164uI64ukLgoJ7JWM6rOg66as7KaY7J2AIOusuOuylSDshKTrqoXsl5Ag65Sw6528IOyeheugpe2VmOyLnOup" +
        "tCDrkKnri4jri6QuCgnslYzqs6Drpqzsppgg7IOd7ISxL+yImOyglSDtm4Qg7Iuk7ZaJ7ZWY6riwIOuyhO2KvOydhCDriITrpbTsi5zrqbQg7Iuk7ZaJ65Cp64uI64ukLgoJK" +
        "yBXaW5kb3dzIOuqqOuTnOuKlCDsgqzsmqntlaAg7IiYIOyXhuycvOupsCwg7Jik66WY6rCAIOuwnOyDne2VqeuLiOuLpC4KCgntlITroZzqt7jrnqgg64uk7Jq066Gc65OcIO" +
        "yLnCBzdW3qs7wgcGx1czEsIG1pbnVzMSDslYzqs6DrpqzsppjsnbQg7J6Q64+ZIOyDneyEseuQmOupsCwg7J2064qUIOy2nOygnOuQnCDrrLjsoJzsnZgg7JWM6rOg66as7Ka" +
        "Y6rO8IOqwmeyKteuLiOuLpC4KCuusuOuylSDshKTrqoU6CgnslYzqs6DrpqzsppjsnbTrgpgg7IOB7YOc7J2YIOydtOumhOyXkOuKlCDslYztjIzrsrPqs7wg7Iir7J6QLCBf" +
        "KOyWuOuNlOuwlCnrp4wg7IKs7Jqp7ZW07JW8IO2VqeuLiOuLpC4KCe2VnOq4gOydgCDsgqzsmqntlaAg7IiYIOyXhuyKteuLiOuLpC4KCgnqsIEg66qF66C57J2AIOykhOuhn" +
        "CDqtazrtoTtlanri4jri6QuCgnruYjsubgo4pahKeydgCBi66GcIOyeheugpe2VqeuLiOuLpC4KCeqwgSDspITsnbQg7ZuE7Iig7ZWgIO2Kueygle2VnCDtgqTsm4zrk5zroZ" +
        "wg7Iuc7J6R7ZWY7KeAIOyViuuKlOuLpOuptCDsnbzrsJgg66qF66C57Jy866GcIOyduOyLne2VqeuLiOuLpC4KCgkwKSDsvZTrk5zsnZgg7LKrIOykhOyXkOuKlCBzdGFydEF" +
        "0IHvsi5zsnpHtlaAg65WM7J2YIOyDge2DnCDsnbTrpoR97J2EIOyeheugpe2VtOyVvCDtlanri4jri6QuCgnsmIjsi5w6CgkJc3RhcnRBdCBmb28KCQko7J207ZWYIOyDneue" +
        "tSkKCeydtCDsvZTrk5zripQg7Iuc7J6R7ZWgIOuVjCBmb28g7IOB7YOc7JeQ7IScIOyLnOyeke2VqeuLiOuLpC4KCgkxKSDsnbzrsJgg66qF66C5CgkJe+yhsOqxtH0tPnvrs" +
        "4Dqsr3qsJJ9LHvtl6TrjZTsnbTrj5nrsKntlqV9LHvsg4Htg5zsnbTrpoR9IOydmCDtmJXsi53snLzroZwg7J6F66Cl7ZWp64uI64ukLgoJCTAtPjEsUixh66GcIOyYiOyLnO" +
        "ulvCDrk6TslrQg67O06rKg7Iq164uI64ukLgoJCeydtCDrqoXroLnsnYAg7ZiE7J6sIOqyqeyekOydmCDqsJLsnbQgMOydvCDrlYwg7ZiE7J6sIOqyqeyekOydmCDqsJLsnYQ" +
        "gMeuhnCDrs4Dqsr3tlZjqs6AsIO2XpOuNlOulvCDsmKTrpbjsqr3snLzroZwg7ZWcIOy5uCDsnbTrj5ntlZjrqbAsCgkJ7IOB7YOc66W8IGHroZwg67OA6rK97ZWY64qUIOqy" +
        "g+ydhCDsnZjrr7jtlanri4jri6QuCgkJ7KGw6rG06rO8IOuzgOqyveqwkuyXkOuKlCAwfjnsmYAgYuunjCDsgqzsmqntlaAg7IiYIOyeiOyKteuLiOuLpC4KCQntl6TrjZTsn" +
        "bTrj5nrsKntlqXsl5DripQgTCxSKGwscinrp4wg7IKs7Jqp7ZWgIOyImCDsnojsirXri4jri6QuCgkKCTIpIDoKCQk664qUIOyDge2DnOulvCDtkZzsi5ztlZjripQg7YKk7J" +
        "uM65Oc7J6F64uI64ukLgoJCeyYiOulvCDrk6TrqbQgOmHripQgYSDsg4Htg5zrpbwsIDpiYXLripQgYmFyIOyDge2DnOulvCDtkZzsi5ztlanri4jri6QuCgkJ7J20IOuqheu" +
        "guSDslYTrnpjsl5DripQg6re4IOyDge2DnOyXkOyEnCDsi6TtlontlaAg66qF66C57J2EIOyeheugpe2VtOyVvCDtlanri4jri6QuIDMp7J2YIOyYiOyLnOulvCDssLjqs6Dt" +
        "lZjshLjsmpQuCgoJMykgZW5kCgkJ6re4IOyDge2DnOyXkOyEnCDsi6TtlontlaAg66qF66C57J2AIOuqqOuRkCDsnbQg7KSEIOychOyqveyXkCDsnojri6TripQg6rKD7J2EI" +
        "OuCmO2DgOuDheuLiOuLpC4KCQnsnbTripQg642UIOydtOyDgSDsi6TtlontlaAg66qF66C57J20IOyXhuuLpOuKlCDqsoPsnYQg65y77ZWY66+A66GcLCDsnbQg66qF66C57J" +
        "20IOyLpO2WieuQmOuptCDtmITsnqwg7JWM6rOg66as7KaY7J20IOyiheujjOuQqeuLiOuLpC4KCgkJ65Sw65287IScIOyWtOuWpCDsg4Htg5wgKGEpIOyXkOyEnCDqsqnsnpD" +
        "qsJLsnbQgMOydvCDrlYwg7JWM6rOg66as7KaY7J2EIOyiheujjO2VmOuKlCDrsKnrspXsnYAg64uk7J2M6rO8IOqwmeyKteuLiOuLpC4KCQkJOmEKCQkJMC0+MCxSLG4KCQkJM" +
        "S0+MSxSLGEKCQkJKOyDneuetSkKCQkJZW5kCgkJCTpuCgkJCWVuZAoKCTQpIGdvdG8KCQnsobDqsbQg7JeG7J20IOyDge2DnOulvCDrs4Dqsr3tlZjripQg66qF66C57J6F6" +
        "4uI64ukLgoJCWdvdG8ge+yDge2DnOydtOumhH0g6rO8IOqwmeydtCDsgqzsmqntlanri4jri6QuCgkJ7JiI66W8IOuTpOuptCwKCQkJc3RhcnRBdCBhCgkJCTphCgkJCWdvd" +
        "G8gYgoJCQllbmQKCQkJOmIKCQkJZW5kCgkJ66W8IOyLpO2Wie2VmOuptCBiIOyDge2DnOyXkOyEnCDsooXro4zrkKnri4jri6QuCgoJCeuLpOuluCDslYzqs6DrpqzsppjsnZ" +
        "gg7IOB7YOc64qUIOyduOyLne2VoCDsiJgg7JeG7Iq164uI64ukLgoJCeyYiOulvCDrk6TslrQsIOyDge2DnCBj6rCAIOyXhuuKlCDslYzqs6DrpqzsppggQeyXkOyEnCDslYz" +
        "qs6DrpqzsppggQuydmCDsg4Htg5woYynroZwg7J2064+Z7ZWY6riwIOychO2VtCBnb3RvIGPrpbwg7IKs7Jqp7ZWc64uk66m0LAoJCeyDge2DnOulvCDssL7snYQg7IiYIOyX" +
        "huuLpOuKlCDsmKTrpZjqsIAg67Cc7IOd7ZWp64uI64ukLgoJCeunjOyVvSBB7JeQ64+EIOyDge2DnCBj6rCAIOyeiOuLpOuptCDqt7gg7IOB7YOc66GcIOydtOuPme2VqeuL" +
        "iOuLpC4KCQnsnbzrsJgg66qF66C564+EIOuniOywrOqwgOyngOyeheuLiOuLpC4KCgk1KSBhbGcKCQntmITsnqwg7Iuk7ZaJ7ZWY6rOgIOyeiOuKlCDslYzqs6Drpqzsppjs" +
        "nbQg7JWE64uMIOuLpOuluCDslYzqs6DrpqzsppjsnYQg7Iuk7ZaJ7ZWY64qUIOuqheugueyeheuLiOuLpC4KCQnsmIjrpbwg65Ok7Ja0IOuztOqyoOyKteuLiOuLpC4KCQkJ" +
        "c3RhcnRBdCBhCgkJCTphCgkJCTAtPjAsTCxzCgkJCWVuZAoJCQk6cwoJCQlhbGcgc3VtCgkJCWdvdG8gYgoJCQllbmQKCQnqsqnsnpDqsJLsnbQgMOydtOudvOuptCBh7JeQ7" +
        "IScIHMg7IOB7YOc66GcIOydtOuPme2VtCBzdW0g7JWM6rOg66as7KaY7J2EIOyLpO2Wie2VqeuLiOuLpC4KCQlzdW0g7JWM6rOg66as7KaY7JeQ7IScIGVuZCDrqoXroLnslr" +
        "TqsIAg7Iuk7ZaJ65CY66m0LCDri6Tsi5wg7J20IOyVjOqzoOumrOymmOycvOuhnCDrj4zslYTsmYAgZ290byBi66W8IOyLpO2Wie2VqeuLiOuLpC4KCgk2KSBzdG9wCgkJ7J" +
        "WM6rOg66as7KaY7J2EIOyZhOyghO2eiCDsooXro4ztlZjripQg66qF66C57Ja07J6F64uI64ukLgoJCWFsZyDrqoXroLnslrTroZwg7Iuk7ZaJ65CcIOyVjOqzoOumrOymmO" +
        "yXkOyEnCDsgqzsmqnrkJjrqbQg6re4IOyghCDslYzqs6DrpqzsppjsnYAg7Iuk7ZaJ65CY7KeAIOyViuyKteuLiOuLpC4KCgkJKOyYiOyLnCkgNSnsnZgg7L2U65Oc7JeQ7I" +
        "ScLCBzdW0g7JWM6rOg66as7KaY7JeQ7IScIHN0b3DsnbQg7Iuk7ZaJ65CY66m0IGdvdG8gYuuKlCDsi6TtlonrkJjsp4Ag7JWK7Iq164uI64ukLgoKCTcpIC8vCgkJ7J2064" +
        "qUIOq3uCDspITsnbQg7KO87ISd7J6E7J2EIOuCmO2DgOuDheuLiOuLpC4KCQnso7zshJ3snYAg7Iuk7ZaJIOyLnCDsmYTsoITtnogg66y07Iuc65Cp64uI64ukLgoJCeyYiO" +
        "ulvCDrk6TrqbQsCgkJCTphCgkJCS8v65GQIOyImOulvCDrjZTtlagKCQkJYWxnIHN1bQoJCQllbmQKCQnsl5DshJwgJy8v65GQIOyImOulvCDrjZTtlagn7J2AIOyLpO2WiS" +
        "Dsi5wg66y07Iuc65CY6rOgIGFsZyBzdW3snbQg7Iuk7ZaJ65Cp64uI64ukLgoJCeyyqyDspITsl5DripQg7KO87ISd7J2EIOyeheugpe2VoCDsiJgg7JeG7Iq164uI64ukLg" +
        "oJCQoJCey2lOqwgOuhnCwg652E7Ja07JOw6riwIOyXhuydtCDsmYTsoITtnogg6rO167Cx7J24IOykhOydgCDso7zshJ3snLzroZwg7J247Iud7ZWp64uI64ukLgoJCeqzteu" +
        "wseyduCDspITsnYAg6rCA64+F7ISxIO2WpeyDgeydhCDsnITtlbQg7IKs7Jqp65CgIOyImCDsnojsirXri4jri6QuCgoKUC5TLiDsi6Ttlokg7IucIOyekOuPmeycvOuhnCD" +
        "sg53shLHrkJjripQg7JWM6rOg66as7KaY7J24IHN1bSwgbWludXMxLCBwbHVzMeydhCDssLjqs6DtlZjsi5zrqbQg642UIOyJveqyjCDsnbTtlbTtlZjsi6Qg7IiYIOyeiOy" +
        "dhCDqsoHri4jri6Qu";
}

