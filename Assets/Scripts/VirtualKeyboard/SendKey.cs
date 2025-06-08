using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SendKey : MonoBehaviour
{
    public TMP_InputField targetField;
    public GeminiRequester geminiRequester;

    public void OnSend()
    {
        if (targetField != null && geminiRequester != null && !string.IsNullOrWhiteSpace(targetField.text))
        {
            geminiRequester.SendToGemini(targetField.text);
            targetField.text = "";
            targetField.ActivateInputField();
        }
    }
}
