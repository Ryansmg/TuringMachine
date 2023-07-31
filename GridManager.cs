using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int content = -1;
    public int index = 0;
    public GameObject contentUI;
    public GameObject indexUI;

    // Update is called once per frame
    void Update()
    {
        try
        {
            content = Main.content[index];
            if (content == -1) contentUI.GetComponent<TMP_Text>().text = "бр";
            else contentUI.GetComponent<TMP_Text>().text = $"{content}";

            indexUI.GetComponent<TMP_Text>().text = $"{index}";
        } catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }
}
