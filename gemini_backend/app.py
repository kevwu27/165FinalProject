from flask import Flask, request, jsonify
from flask_cors import CORS
from gemini_utils import get_gemini_response

app = Flask(__name__)
CORS(app)  # Allow Unity to access this server

@app.route("/ask", methods=["POST"])
def ask():
    data = request.get_json()
    user_input = data.get("input", "")
    board_state = data.get("boardState", "")

    print(f"[Flask] Received input: {user_input}")
    print(f"[Flask] Received board: {board_state}")
    try:
        response, emotion = get_gemini_response(user_input, board_state)
        
        return jsonify({"response": response, "emotion": emotion})
    except Exception as e:
        print(f"[Flask] ERROR calling Gemini: {e}")
        return jsonify({"response": "Gemini API call failed", "emotion": emotion}), 403

if __name__ == "__main__":
    app.run(host="localhost", port=5001)
