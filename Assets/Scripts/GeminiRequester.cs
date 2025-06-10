using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Utilities;
using Meta.WitAi.TTS.Data;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

public class GeminiRequester : MonoBehaviour
{
    public string serverUrl = "http://localhost:5001/ask";
    public TextMeshProUGUI dialogueText;
    public Animator agentAnimator;

    private string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=AIzaSyDRoj6IIG2wnmN9gl8Afg5rEfa8M-_KrHM";

    public TTSSpeaker speaker;

    public void SendToGemini(string userInput)
    {
        Debug.Log("sending to gemini: " + userInput);
        // StartCoroutine(PostRequest(userInput));
        StartCoroutine(SendRequest(userInput));
    }

    IEnumerator SendRequest(string userInput)
    {
        string boardState = CheckersLogic.Instance.PrintBoard();
        string prompt = $@"
            You are an expressive VR game character inside a checkers game. You are here to guide the player in how to play checkers, but also in any other general requests. You are both the opponent of the user, and the teacher.
            If the user's input is related to the checkers board or gameplay (e.g. asking about moves, rules, game progress), respond about the game and help the user win. Otherwise, respond normally.
            The player is the black player, and you are the white player. Black pieces move diagonally toward row 7. Only Black kings can move diagonally row 7 or row 0.
            White pieces move diagonally toward row 0. Only White kings can move diagonally row 7 or row 0. When helping the player, give advice that would help them (the black pieces) win!

            Respond briefly in 1–2 sentences.
            Always include exactly one emotion tag at the end, chosen from: [Thankful, Headshake, Surprised, Offended].

            Game Board:
            {boardState}

            User: {userInput}
            Response:
        ";

        var requestData = new GeminiRequest
        {
            contents = new[] {
                new Content
                {
                    parts = new[] {
                        new Part 
                        {
                            text = prompt
                        }
                    }
                }
            }
        };

        string json = JsonConvert.SerializeObject(requestData); // OR use Newtonsoft.Json for full support

        var request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;
            Debug.Log("Raw Gemini response:\n" + result);

            GeminiResponse geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(result);

            string response = geminiResponse.candidates[0].content.parts[0].text;
            Debug.Log("Gemini says: " + response);

            // Optional: Extract emotion tag using regex
            string emotion = "Neutral";
            var match = System.Text.RegularExpressions.Regex.Match(response, @"\[(\w+)\]$");
            if (match.Success)
            {
                emotion = match.Groups[1].Value;
                response = response.Replace(match.Value, "").Trim(); // remove tag from speech
            }

            Debug.Log("Gemini says: " + response);
            Debug.Log("Emotion: " + emotion);

            dialogueText.text = response;
            speaker.Speak(response);

            agentAnimator.SetBool("ConversationMode", false);
            switch (emotion)
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

            StartCoroutine(ResetEmotion(emotion, 5f));
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    IEnumerator ResetEmotion(string paramName, float delay)
    {
        yield return new WaitForSeconds(delay);
        agentAnimator.SetBool(paramName, false);
        agentAnimator.SetBool("ConversationMode", true);

        yield return new WaitUntil(() => !speaker.IsSpeaking);
        dialogueText.text = "";

    }

    [System.Serializable]
    public class RequestData
    {
        public string input;
        public string boardState;
    }

    [System.Serializable]
    public class GeminiResponsePart
    {
        public string text;
    }

    [System.Serializable]
    public class GeminiResponseContent
    {
        public GeminiResponsePart[] parts;
    }

    [System.Serializable]
    public class GeminiResponseCandidate
    {
        public GeminiResponseContent content;
        public string finishReason;
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public GeminiResponseCandidate[] candidates;
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class GeminiRequest
    {
        public Content[] contents;
    }
}
