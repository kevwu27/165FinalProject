using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GeminiRequester : MonoBehaviour
{
    public string serverUrl = "http://localhost:5001/ask";

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
        }
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
    }
}
