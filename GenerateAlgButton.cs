using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class GenerateAlgButton : MonoBehaviour
{
    public GameObject algButtonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            DirectoryInfo di = new(Application.persistentDataPath + "/algorithms");
            foreach (FileInfo fi in di.GetFiles())
            {
                string fileName = fi.Name.Replace(".txt", "");
                GameObject algButton = Instantiate(algButtonPrefab);
                algButton.transform.SetParent(GameObject.Find("Content").transform);
                algButton.transform.localScale = Vector3.one;
                algButton.GetComponent<AlgButton>().algName = fileName;
                algButton.GetComponentInChildren<TMP_Text>().text = fileName;
            }
        } catch (Exception e)
        {
            Main.HandleError_NonAlg(e.ToString(), "internal");
            return;
        }
    }
}
