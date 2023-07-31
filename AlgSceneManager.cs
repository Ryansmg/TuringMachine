using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlgSceneManager : MonoBehaviour
{
    public static string algName;
    public static bool isLog;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            string algorithmFilePath = Application.persistentDataPath + $"/algorithms/{algName}.txt";
            if (isLog) algorithmFilePath = Application.persistentDataPath + $"/player-prev.log";
            if (isLog && ButtonScript.isAndroid) {
                SceneManager.LoadScene("AlgScene");
                return; 
            }
            string algorithm = File.ReadAllText(algorithmFilePath).Replace("\r", "");
            GameObject.Find("InputField").GetComponent<TMP_InputField>().text = algorithm;
            GameObject.Find("Title").GetComponent<TMP_Text>().text = $"알고리즘 편집 : {algName}";
            if (isLog) GameObject.Find("Title").GetComponent<TMP_Text>().text = $"Log";
        }
        catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }

    public void Save()
    {
        if (isLog) return;
        try
        {
            string content = GameObject.Find("InputField").GetComponent<TMP_InputField>().text;
            if (content.Equals(""))
            {
                File.Delete(Application.persistentDataPath + $"/algorithms/{algName}.txt");
            }
            else
            {
                ButtonScript.WriteFile(Application.persistentDataPath + $"/algorithms/{algName}.txt", GameObject.Find("InputField").GetComponent<TMP_InputField>().text);
            }
        }
        catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }
}
