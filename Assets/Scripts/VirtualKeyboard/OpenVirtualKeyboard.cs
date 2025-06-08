using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenVirtualKeyboard : MonoBehaviour
{
    public GameObject keyboardPanel; // The parent GameObject of your virtual keyboard UI

    public void ShowKeyboard()
    {
        if (keyboardPanel != null)
        {
            keyboardPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Keyboard panel reference is missing.");
        }
    }
}