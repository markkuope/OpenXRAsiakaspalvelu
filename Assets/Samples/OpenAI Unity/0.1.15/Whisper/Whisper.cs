using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace OpenAI
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Dropdown dropdown;
        //public HelmiChat helmiChat;
        [SerializeField] private Image progress;

        private readonly string fileName = "output.wav";
        private readonly int duration = 15;//äänityksen kesto asetettu 15s, voi muuttaa täältä

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();




        //lähetetään transkriptoitu arvo AI:lle HelmiChat scriptiin
        //käyttämällä muuttujaa whisperText

        public string whisperText;

       

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

                progress.fillAmount = 0;
                //message.text = res.Text;
                recordButton.enabled = true;


                //tässä kohdassa message.text arvo on whisper AI:n transkriptio
                //print(message.text);
                // tämä arvo viedään AI:lle HelmiChat scriptiin


                //whisperText = message.text;

               // helmiChat.SendReply();
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
