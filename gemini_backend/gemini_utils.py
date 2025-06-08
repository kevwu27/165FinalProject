import os
from dotenv import load_dotenv
import google.generativeai as genai
import re

load_dotenv()

api_key = os.getenv("GEMINI_API_KEY")
print(f"[GeminiUtils] Loaded API Key: {api_key}")  # ← this should show a real key, not None

genai.configure(api_key=api_key)

def get_gemini_response(user_input, board_state):
    prompt = f"""
You are an expressive VR game character inside a checkers game. You are here to guide the player in how to play checkers, but also in any other general requests. You are both the opponent of the user, and the teacher.
If the user's input is related to the checkers board or gameplay (e.g. asking about moves, rules, game progress), respond about the game and help the user win. Otherwise, respond normally.
The player is the black player, and you are the white player. The board state is given where (0, 0) is the side closest to you, where the white pieces are.

Respond briefly in 1–2 sentences.
Always include exactly one emotion tag at the end, chosen from: [Thankful, Headshake, Surprised, Offended].

Game Board:
{board_state}

Empty = 0,
Black = 1,
BlackKing = 2,
White = -1,
WhiteKing = -2

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
