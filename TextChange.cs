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
                if (main.isStopped) text.text = "����Ǿ����ϴ�.";
                else if (main.executeFast || main.executeContinuously || main.executeOnce) text.text = "���� ���Դϴ�.";
                else text.text = "�Ͻ������Ǿ����ϴ�.";
            }
            if (gameObject.name.Equals("2"))
            {
                text.text = $"�˰���: {Main.algorithmName}";
            }
            if (gameObject.name.Equals("3"))
            {
                text.text = $"����: {main.currentStatus}";
            }
        } catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }
}
