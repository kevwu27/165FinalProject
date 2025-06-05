using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class GeminiRequester : MonoBehaviour
{
    public string serverUrl = "http://localhost:5001/ask";
    public TextMeshProUGUI dialogueText;
    public Animator agentAnimator;

    public void SendToGemini(string userInput)
    {
        Debug.Log("sending to gemini: " + userInput);
        StartCoroutine(PostRequest(userInput));
    }

    IEnumerator PostRequest(string text)
    {
        var input = new RequestData
        {
            input = text
        };

        string json = JsonUtility.ToJson(input);

        var req = new UnityWebRequest("localhost:5001/ask", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(jsonToSend);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + req.error);
        }
        else
        {
            string result = req.downloadHandler.text;
            GeminiResponse res = JsonUtility.FromJson<GeminiResponse>(result);

            Debug.Log("Gemini says: " + res.response);
            Debug.Log("Emotion: " + res.emotion);

            dialogueText.text = res.response;

            agentAnimator.SetBool("ConversationMode", false);
            switch (res.emotion)
            {
                case "Thankful":
                    agentAnimator.SetBool("Thankful", true);
                    break;
                case "Headshake":
                    agentAnimator.SetBool("Headshake", true);
                    break;
                case "Surprised":
                    agentAnimator.SetBool("Surprised", true);
                    break;
                case "Offended":
                    agentAnimator.SetBool("Offended", true);
                    break;
                default:
                    break;
            }

            // Optional: auto-reset after animation
            StartCoroutine(ResetEmotion(res.emotion, 5f));
        }
    }

    IEnumerator ResetEmotion(string paramName, float delay)
    {
        yield return new WaitForSeconds(delay);
        agentAnimator.SetBool(paramName, false);
        agentAnimator.SetBool("ConversationMode", true);
    }

    [System.Serializable]
    public class RequestData
    {
        public string input;
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public string response;
        public string emotion;
    }
}
