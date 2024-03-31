using System;
using UnityEngine;
using TMPro;
using System.Text;

public class TextChange : MonoBehaviour
{
    public TMP_Text text;
    public GameObject script;
    public Main main;

    private void Start()
    {
        if (!gameObject.CompareTag("helpContent")) return;
        byte[] byte64 = Convert.FromBase64String(ButtonScript.helpBase64);
        string s1 = Encoding.UTF8.GetString(byte64);
        text.text = s1;
    }
    // Update is called once per frame
    private void Update()
    {
        try
        {
            if (gameObject.name.Equals("1"))
            {
                if (main.isStopped) text.text = "종료되었습니다.";
                else if (main.executeFast || main.executeContinuously || main.executeOnce) text.text = "실행 중입니다.";
                else text.text = "일시정지되었습니다.";
            }
            if (gameObject.name.Equals("2"))
            {
                text.text = $"알고리즘: {Main.algorithmName}";
            }
            if (gameObject.name.Equals("3"))
            {
                text.text = $"상태: {main.currentStatus}";
            }
        } catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }
}
