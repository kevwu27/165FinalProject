using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundEndMenu : MonoBehaviour
{
    public Animator menuAnimator;             // Controls the swipe-in animation
    public GameObject menuCanvas;             // Whole canvas to show/hide
    public Image resultImage;                 // UI image for "You Won"/"You Lost"
    public Sprite youWonSprite;               // Assign in Inspector
    public Sprite youLostSprite;              // Assign in Inspector

    public void ShowEndScreen(bool playerWon)
    {
        Debug.Log("ShowEndScreen called");
        // Ensure animator is reset before playing again
        menuCanvas.SetActive(true);
        menuAnimator.Rebind();
        resultImage.sprite = playerWon ? youWonSprite : youLostSprite;
        menuAnimator.SetTrigger("Show");      // Make sure "Show" is defined in Animator
        Debug.Log($"menuCanvas active? {menuCanvas.activeSelf}, menuAnimator exists? {menuAnimator != null}");

    }

    public void OnPlayAgainPressed()
    {
        menuAnimator.SetTrigger("Play");      // Optional: swipe away
        StartCoroutine(DelayedRestart(0.5f));
    }

    private IEnumerator DelayedRestart(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Reset animator state after hiding
        menuAnimator.Rebind();
        menuAnimator.ResetTrigger("Play");
        menuAnimator.ResetTrigger("Show");
        menuCanvas.SetActive(false);
        CheckersLogic.Instance.RestartGame(); // Calls the reset logic
    }
}