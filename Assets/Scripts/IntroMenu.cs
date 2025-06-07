using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroMenu : MonoBehaviour
{
    public Animator menuAnimator;               // Animator for swipe/fade out
    public GameObject menuCanvas;              // Entire intro menu
    public TutorialManager tutorialManager;    // Reference to TutorialManager
    public GrabPiece grabPieceScript;          // Disable interaction until ready

    void Start()
    {
        grabPieceScript.enabled = false;
    }

    public void OnPlayPressed()
    {
        // 1. Disable interaction with pieces
        grabPieceScript.enabled = false;

        // 2. Start swipe/fade-out animation
        menuAnimator.SetTrigger("Play");

        // 3. Begin tutorial after delay to let animation finish
        StartCoroutine(StartTutorialAfterDelay(1f));
    }

    private System.Collections.IEnumerator StartTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 4. Hide the menu canvas
        menuCanvas.SetActive(false);

        // 5. Start the tutorial logic
        tutorialManager.StartTutorial();
    }

    public void EnablePieceInteraction()
    {
        grabPieceScript.enabled = true;
    }
}

