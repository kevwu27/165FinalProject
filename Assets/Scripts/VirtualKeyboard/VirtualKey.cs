using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VirtualKey : MonoBehaviour
{
    public string character;
    public TMP_InputField targetField;

    public void OnKeyPress()
    {
        if (targetField != null && !string.IsNullOrEmpty(character))
        {
            targetField.text += character;
            targetField.ActivateInputField();
        }
    }
}