import os
from dotenv import load_dotenv
import google.generativeai as genai
import re

load_dotenv()

api_key = os.getenv("GEMINI_API_KEY")
print(f"[GeminiUtils] Loaded API Key: {api_key}")  # ← this should show a real key, not None

genai.configure(api_key=api_key)

def get_gemini_response(user_input):
    prompt = f"""
You are an expressive VR game character. 
Respond to the user briefly in 1–2 sentences. 
Always include exactly one emotion tag from this exact list at the end in square brackets: [Thankful, Headshake, Surprised, Offended].

User: {user_input}
Response:
"""
    
    model = genai.GenerativeModel("models/gemini-2.0-flash")
    response = model.generate_content(prompt)
    print(f"[Flask] Gemini response: {response}")
    full_text = response.text.strip()

    match = re.search(r"\[(.*?)\]$", full_text)
    emotion = match.group(1) if match else "neutral"
    message = re.sub(r"\[.*?\]$", "", full_text).strip()

    return message, emotion
