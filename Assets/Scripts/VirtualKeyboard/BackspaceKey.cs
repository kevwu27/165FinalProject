using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BackspaceKey : MonoBehaviour
{
    public TMP_InputField targetField;

    public void OnBackspace()
    {
        if (targetField != null && targetField.text.Length > 0)
        {
            targetField.text = targetField.text.Substring(0, targetField.text.Length - 1);
            targetField.ActivateInputField();
        }
    }
}