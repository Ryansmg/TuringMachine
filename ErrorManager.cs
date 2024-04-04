using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorManager : MonoBehaviour
{
    public static string errorDescription = "";
    public static string cause = "";
    public static string contentStr = "";
    public static string algName = "";
    public static string algLine = "";
    public static string algStatus = "";
    public static string algIndex = "";

    private void Start()
    {
        if (ButtonScript.returnToAchieveScene && !cause.Equals("internal"))
        {
            cause = contentStr = algLine = algStatus = algIndex = "(������)";
        }
        GameObject.Find("ErrorDescription").GetComponent<TMP_Text>().text = $"���� ����: {errorDescription}";
        GameObject.Find("Cause").GetComponent<TMP_Text>().text = $"����(����): {cause}";
        GameObject.Find("Content").GetComponent<TMP_Text>().text = $"���� ����: {contentStr}";
        GameObject.Find("AlgName").GetComponent<TMP_Text>().text = $"�˰��� �̸�: {algName}";
        GameObject.Find("AlgLine").GetComponent<TMP_Text>().text = $"������ �߻��� �� ��ȣ: {algLine}";
        GameObject.Find("AlgStatus").GetComponent<TMP_Text>().text = $"���� �� ����: {algStatus}";
        GameObject.Find("AlgIndex").GetComponent<TMP_Text>().text = $"���� �� ��� ��ġ: {algIndex}";

        if (!cause.Equals("internal")) return;
        GameObject.Find("Cause").SetActive(false);
        GameObject.Find("Content").SetActive(false);
        GameObject.Find("AlgName").SetActive(false);
        GameObject.Find("AlgLine").SetActive(false);
        GameObject.Find("AlgStatus").SetActive(false);
        GameObject.Find("AlgIndex").SetActive(false);
        GameObject.Find("Text").GetComponent<TMP_Text>().text = "�ý��� ���� ������ �߻��߽��ϴ�. �����ڿ��� �����ϼ���.";
        GameObject.Find("Text").GetComponent<TMP_Text>().fontSize = 55;
        GameObject.Find("ErrorDescription").GetComponent<TMP_Text>().fontSize = 30;
    }
}
