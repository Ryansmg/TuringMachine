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

    void Start()
    {
        GameObject.Find("ErrorDescription").GetComponent<TMP_Text>().text = $"오류 설명: {errorDescription}";
        GameObject.Find("Cause").GetComponent<TMP_Text>().text = $"원인(추정): {cause}";
        GameObject.Find("Content").GetComponent<TMP_Text>().text = $"격자 상태: {contentStr}";
        GameObject.Find("AlgName").GetComponent<TMP_Text>().text = $"알고리즘 이름: {algName}";
        GameObject.Find("AlgLine").GetComponent<TMP_Text>().text = $"오류가 발생한 줄 번호: {algLine}";
        GameObject.Find("AlgStatus").GetComponent<TMP_Text>().text = $"종료 시 상태: {algStatus}";
        GameObject.Find("AlgIndex").GetComponent<TMP_Text>().text = $"종료 시 헤더 위치: {algIndex}";

        if (cause.Equals("internal"))
        {
            GameObject.Find("Cause").SetActive(false);
            GameObject.Find("Content").SetActive(false);
            GameObject.Find("AlgName").SetActive(false);
            GameObject.Find("AlgLine").SetActive(false);
            GameObject.Find("AlgStatus").SetActive(false);
            GameObject.Find("AlgIndex").SetActive(false);
            GameObject.Find("Text").GetComponent<TMP_Text>().text = "시스템 내부 에러가 발생했습니다. 개발자에게 문의하세요.";
            GameObject.Find("Text").GetComponent<TMP_Text>().fontSize = 55;
            GameObject.Find("ErrorDescription").GetComponent<TMP_Text>().fontSize = 30;
        }
    }
}
