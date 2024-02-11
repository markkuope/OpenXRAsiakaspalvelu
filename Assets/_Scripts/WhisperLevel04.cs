using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace OpenAI
{
    public class WhisperLevel04 : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private ChatLevel04 chatLevel04;
        [SerializeField] private Image progress;

        private readonly string fileName = "output.wav";
        private readonly int duration = 15;

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            recordButton.onClick.AddListener(StartRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        private async void StartRecording()
        {
            if (isRecording)
            {
                isRecording = false;
                //Debug.Log("Stop recording...");

                time = 0;//aloitetaan äänityksen ajanlasku aina nollasta, tämä on lisäys Sargen koodiin

                Microphone.End(null);
                byte[] data = SaveWav.Save(fileName, clip);

                var req = new CreateAudioTranscriptionsRequest
                {
                    FileData = new FileData() { Data = data, Name = "audio.wav" },
                    // File = Application.persistentDataPath + "/" + fileName,
                    Model = "whisper-1",
                    Language = "fi"   // tässä määritellään kieli, fi = suomi, en = englanti
                };
                var res = await openai.CreateAudioTranscription(req);

                chatLevel04.SendReply(res.Text);
            }
            else
            {
                //Debug.Log("Start recording...");
                isRecording = true;

                var index = PlayerPrefs.GetInt("user-mic-device-index");
                clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
            }
        }

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                progress.fillAmount = time / duration;
            }

            if (time >= duration)
            {
                time = 0;
                progress.fillAmount = 0;
                StartRecording();
            }
        }
    }
}


