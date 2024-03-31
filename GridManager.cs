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
    private TMP_Text _contentText;
    private TMP_Text _indexText;

    // Update is called once per frame
    private void Start()
    {
        _contentText = contentUI.GetComponent<TMP_Text>();
        _indexText = indexUI.GetComponent<TMP_Text>();
    }

    void Update()
    {
        try
        {
            content = Main.content[index];
            _contentText.text = content == -1 ? "бр" : $"{content}";
            _indexText.text = $"{index}";
        } catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
        }
    }
}
