using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemoveInputfieldNewline : MonoBehaviour
{
    public TMP_InputField field;

    // Update is called once per frame
    private void Update()
    {
        field.text = field.text.Replace("\n", "").Replace("\r","");
    }
}
