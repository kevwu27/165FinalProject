using UnityEngine;
using TMPro;

public class InputManager : MonoBehaviour
{
    public TMP_InputField userInputField;
    public GeminiRequester geminiRequester;

    void Start()
    {
        // Register the callback once
        userInputField.onSubmit.AddListener(HandleSubmit);
    }

    void HandleSubmit(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            geminiRequester.SendToGemini(text);
            userInputField.text = "";
            userInputField.ActivateInputField(); // optional: focus again
        }
    }
}


