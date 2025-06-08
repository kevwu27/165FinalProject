using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CloseKeyboardButton : MonoBehaviour
{
    public GameObject keyboardPanel;
    public float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = keyboardPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = keyboardPanel.AddComponent<CanvasGroup>();
        }
    }

    public void OnClose()
    {
        StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        keyboardPanel.SetActive(false);

        // Reset alpha for next time it shows
        canvasGroup.alpha = 1f;
    }
}
