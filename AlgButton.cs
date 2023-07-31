using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlgButton : MonoBehaviour
{
    public string algName;
    public void OpenAlg()
    {
        AlgSceneManager.isLog = false;
        AlgSceneManager.algName = algName;
        SceneManager.LoadScene("EditScene");
    }
    public void MakeAlg()
    {
        AlgSceneManager.isLog = false;
        algName = GameObject.Find("AlgInput").GetComponent<TMP_InputField>().text;
        if (File.Exists(Application.persistentDataPath + $"/algorithms/{algName}.txt")) return;
        ButtonScript.WriteFile(Application.persistentDataPath + $"/algorithms/{algName}.txt", "");
        AlgSceneManager.algName = algName;
        SceneManager.LoadScene("EditScene");
    }
    public void ViewLog()
    {
        AlgSceneManager.isLog = true;
        AlgSceneManager.algName = "";
        SceneManager.LoadScene("EditScene");
    }
}
