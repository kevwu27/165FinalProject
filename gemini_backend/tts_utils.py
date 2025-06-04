from google.cloud import texttospeech
import uuid
import os

client = texttospeech.TextToSpeechClient()

def synthesize_speech(text):
    input_text = texttospeech.SynthesisInput(text=text)
    voice = texttospeech.VoiceSelectionParams(language_code="en-US", ssml_gender=texttospeech.SsmlVoiceGender.NEUTRAL)
    audio_config = texttospeech.AudioConfig(audio_encoding=texttospeech.AudioEncoding.LINEAR16)

    response = client.synthesize_speech(input=input_text, voice=voice, audio_config=audio_config)

    filename = f"responses/{uuid.uuid4().hex}.wav"
    with open(filename, "wb") as out:
        out.write(response.audio_content)
    return filename
