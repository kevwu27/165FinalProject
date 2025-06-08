using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpaceKey : MonoBehaviour
{
    public TMP_InputField targetField;

    public void OnSpace()
    {
        if (targetField != null)
        {
            targetField.text += " ";
            targetField.ActivateInputField();
        }
    }
}