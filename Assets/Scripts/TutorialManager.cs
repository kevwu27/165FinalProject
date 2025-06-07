using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TMP_InputField userInputField;
    public Animator agentAnimator;
    public string jsonFileName = "checkers_tutorial.json";

    private List<TutorialStep> steps;
    private int currentStep = 0;

    public Button nextButton;
    private bool isAnimating = false;

    [Header("Gameplay")]
    public GrabPiece grabPieceScript;
    public IntroMenu introMenu;

    public void StartTutorial()
    {
        LoadTutorial();
        StartCoroutine(InitializeTutorial());
    }

    // Loads in the json file
    void LoadTutorial()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            steps = JsonUtilityWrapper.FromJsonList<TutorialStep>(json);
        }
        else
        {
            Debug.LogError("Tutorial JSON not found at " + path);
        }
    }

    IEnumerator InitializeTutorial()
    {
        // Wait until idle or "Ready" state
        yield return StartCoroutine(WaitUntilInState("TutorialMode"));

        // Now safe to start the tutorial
        StartCoroutine(PlayTutorial());

    }

    IEnumerator WaitUntilInState(string expectedStateName)
    {
        AnimatorStateInfo stateInfo = agentAnimator.GetCurrentAnimatorStateInfo(0);

        // Wait until the current state matches the one you're looking for
        while (!stateInfo.IsName(expectedStateName))
        {
            yield return null; // wait for next frame
            stateInfo = agentAnimator.GetCurrentAnimatorStateInfo(0);
        }
    }

    IEnumerator PlayTutorial()
    {
        agentAnimator.SetBool("TutorialMode", false);
        while (currentStep < steps.Count)
        {
            var step = steps[currentStep];

            if (step.animation == "Idle")
            {
                dialogueText.text = step.text;
                currentStep++;
                yield return new WaitForSeconds(2f);
                continue;
            }
            // Play animation
            agentAnimator.SetBool(step.animation, true);
            isAnimating = true;

            // Wait for animation to start
            yield return new WaitUntil(() => agentAnimator.GetCurrentAnimatorStateInfo(0).IsName(step.animation));

            // Now display text once animation starts
            dialogueText.text = step.text;

            // Wait for animation to finish
            yield return new WaitUntil(() =>
                agentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.85f
            );
            //  &&
            //     !agentAnimator.IsInTransition(0)

            // // Optional delay between steps
            // yield return new WaitForSeconds(step.delayBeforeNext);

            currentStep++;
        }

        // After tutorial finishes
        if (grabPieceScript != null)
            grabPieceScript.enabled = true;

        CheckersLogic.Instance.currentTurn = PlayerTurn.Black;

        if (introMenu != null)
            introMenu.gameObject.SetActive(false); // optionally hide menu if still around

        Debug.Log("Tutorial finished!");
        dialogueText.text = "";
        agentAnimator.SetBool("Pointing", false);
        agentAnimator.SetBool("Gesturing", false);
        agentAnimator.SetBool("Cards", false);
        agentAnimator.SetBool("TutorialMode", true);

        StartCoroutine(ChangeState());
    }
    
    IEnumerator ChangeState()
    {
        // Wait until idle or "Ready" state
        yield return StartCoroutine(WaitUntilInState("TutorialMode"));

        agentAnimator.SetBool("ConversationMode", true);
        agentAnimator.SetBool("TutorialMode", false);
        userInputField.gameObject.SetActive(true);
    }

    // public void ShowCurrentStep()
    // {
    //     if (currentStep >= steps.Count)
    //     {
    //         return;
    //     }

    //     var step = steps[currentStep];
    //     dialogueText.text = step.text;
    //     if (step.animation == "Idle") return;
    //     StartCoroutine(PlayBoolAnimation(step.animation, 2f));
    // }

    // IEnumerator PlayBoolAnimation(string paramName, float duration)
    // {
    //     agentAnimator.SetBool(paramName, true);
    //     yield return new WaitForSeconds(duration);
    //     agentAnimator.SetBool(paramName, false);
    // }

    // public void NextStep()
    // {
    //     currentStep++;
    //     if (currentStep < steps.Count)
    //     {
    //         ShowCurrentStep();
    //     }
    //     else
    //     {
    //         Debug.Log("Tutorial finished!");
    //         nextButton.gameObject.SetActive(false);
    //         dialogueText.text = "";
    //         agentAnimator.SetBool("ConversationMode", true);
    //         agentAnimator.SetBool("TutorialMode", false);
    //         userInputField.gameObject.SetActive(true);
    //         // TODO: Switch to conversation mode
    //     }
    // }
}

[System.Serializable]
public class TutorialStep
{
    public string text;
    public string animation;
    // public float delayBeforeNext;
}
