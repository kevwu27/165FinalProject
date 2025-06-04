import os
from dotenv import load_dotenv
import google.generativeai as genai

load_dotenv()

api_key = os.getenv("GEMINI_API_KEY")
print(f"[GeminiUtils] Loaded API Key: {api_key}")  # ← this should show a real key, not None

genai.configure(api_key=api_key)

def get_gemini_response(user_input):
    model = genai.GenerativeModel("models/gemini-2.0-flash")
    response = model.generate_content(user_input)
    return response.text
