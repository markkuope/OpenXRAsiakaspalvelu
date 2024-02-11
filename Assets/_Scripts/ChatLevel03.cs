using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

namespace OpenAI
{
    public class ChatLevel03 : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;
        [SerializeField] private NpcInfo npcInfo;
        [SerializeField] private WorldInfo gameInfo;
        [SerializeField] private NpcDialog npcDialog;
        public UnityEvent OnReplyReceived;

        // tallennetaan keskustelu tietokoneen muistiin ,tarvitaan nämä muuttujat
        private string MessageLog;
        private string LogPath;


        //private string response;
        public string response;



        private bool isDone = true;
        private RectTransform messageRect;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        public List<ChatMessage> messages = new List<ChatMessage>();

        // otetaan OPenAI Text To Speech käyttöön tässä
        [SerializeField] private TTSManager ttsManager;



        private void Start()
        {
            var message = new ChatMessage
            {
                Role = "user",
                Content =
                     "Speak finnish\n" +
                     "You, NPC, will act as a customer at a kitchen department of large department store.Your name is Helmi,  a friendly and outgoing retiree who is looking for a japanese kitchen knife.\n" +
                     "If you,NPC, are satisfied with the quality and price of the knife, end conversation and finish your sentence with the phrase END_CONVO \n" +
                     "You,ChatGPT,acting as Helmi, will start the conversation and wait for the answer. Do not start with Helmi: " +

                    // "The following info is the info about the game : \n" +
                    gameInfo.GetPrompt()
            };


            sendButton.onClick.AddListener(SendReply);
            // tallennetaan keskustelu tietokoneen muistiin
            messages.Add(new ChatMessage() { Content = message.Content, Role = "system" });
            LogPath = Application.dataPath + "/ChatLog/Log_" + System.DateTime.UtcNow.ToString() + ".json";
        }

        private RectTransform AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);

            if (message.Role != "user")
            {
                messageRect = item;
            }

            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);

            if (message.Role == "user")
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
                height += item.sizeDelta.y;
                scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                scroll.verticalNormalizedPosition = 0;

                //make sure to create a [ChatLog] directory in the Assets path
                //write Message Log into a file
                MessageLog = JsonConvert.SerializeObject(messages, Formatting.None);
                StreamWriter sw = new StreamWriter(LogPath, false);
                sw.Write(MessageLog);
                sw.Close();
            }
            return item;
        }

        private void SendReply()
        {
            SendReply(inputField.text);
        }

        public void SendReply(string input)
        {
            var message = new ChatMessage()
            {
                Role = "user",
                Content = input
            };
            messages.Add(message);

            openai.CreateChatCompletionAsync(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = messages
            }, OnResponse, OnComplete, new CancellationTokenSource());

            AppendMessage(message);

            inputField.text = "";
        }

        private void OnResponse(List<CreateChatCompletionResponse> responses)
        {
            var text = string.Join("", responses.Select(r => r.Choices[0].Delta.Content));

            if (text == "") return;

            if (text.Contains("END_CONVO"))
            {
                text = text.Replace("END_CONVO", "");

                Invoke(nameof(EndConvo), 15);
            }

            var message = new ChatMessage()
            {
                Role = "assistant",
                Content = text
            };

            if (isDone)
            {
                //aloitetaan animaatio jos sellaista tarvitaan
                //OnReplyReceived.Invoke();
                messageRect = AppendMessage(message);
                isDone = false;
            }

            messageRect.GetChild(0).GetChild(0).GetComponent<Text>().text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageRect);
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;

            response = text;
        }

        private void OnComplete()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageRect);
            height += messageRect.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;

            var message = new ChatMessage()
            {
                Role = "assistant",
                Content = response
            };
            messages.Add(message);

            // tässä kutsutaan OpenAI Text To Speech sovellusta
            if (ttsManager) ttsManager.SynthesizeAndPlay(response);

            isDone = true;
            response = "";
        }

        private void EndConvo()
        {
            //npcDialog.Recover();
            messages.Clear();
            SceneManager.LoadScene("Level04");
        }
    }
}


