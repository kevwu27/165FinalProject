using TMPro;
using UnityEngine;

public class VRKeyboardManager : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;

    public void ShowKeyboardWithField(TMP_InputField input)
    {
        input.ActivateInputField();
        keyboard = TouchScreenKeyboard.Open(input.text, TouchScreenKeyboardType.Default);
        
    }
}
