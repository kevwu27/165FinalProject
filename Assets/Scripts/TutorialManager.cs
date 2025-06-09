using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Utilities;
using Meta.WitAi.TTS.Data;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TMP_InputField userInputField;
    public Animator agentAnimator;
    public string jsonFileName = "checkers_tutorial.json";

    private List<TutorialStep> steps;
    private int currentStep = 0;

    [Header("Gameplay")]
    public GrabPiece grabPieceScript;
    public IntroMenu introMenu;

    public TTSSpeaker speaker;

    public GameObject chatButton;

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
                speaker.Speak(step.text);
                currentStep++;
                yield return new WaitForSeconds(4f);
                continue;
            }
            // Play animation
            agentAnimator.SetBool(step.animation, true);

            // Wait for animation to start
            yield return new WaitUntil(() => agentAnimator.GetCurrentAnimatorStateInfo(0).IsName(step.animation));

            // Now display text once animation starts
            dialogueText.text = step.text;
            speaker.Speak(step.text);
            yield return new WaitUntil(() => !speaker.IsSpeaking);

            // Wait for animation to finish
            yield return new WaitUntil(() =>
                agentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f
            );

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
        chatButton.SetActive(true);
    }
}

[System.Serializable]
public class TutorialStep
{
    public string text;
    public string animation;
}
