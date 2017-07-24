using AlexaSkillsKit.Slu;
using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.UI;
using NLog;
using RokuDeviceLib;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace RokuControlSkill.Controllers
{
    public class RokuControlSpeechlet : SpeechletAsync
    {
        private static string rokuendpoint = Properties.Settings.Default.rokuEndpoint;
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public override async Task<SpeechletResponse> OnIntentAsync(IntentRequest intentRequest, Session session)
        {
            Log.Info("OnIntent requestId={0}, sessionId={1}", intentRequest.RequestId, session.SessionId);

            // Get intent from the request object.
            var intent = intentRequest.Intent;
            var intentName = (intent != null) ? intent.Name : null;

            // Note: If the session is started with an intent, no welcome message will be rendered;
            // rather, the intent specific response will be returned.
            if (String.Equals("ControlRoku", intentName, StringComparison.OrdinalIgnoreCase))
            {
                return await ControlRoku(intent, session);
            }
            else if (String.Equals("TypeWords", intentName, StringComparison.OrdinalIgnoreCase))
            {
                return await TypeWords(intent, session);
            }
            else if (String.Equals("LaunchApp", intentName, StringComparison.OrdinalIgnoreCase))
            {
                return await LaunchApp(intent, session);
            }
            else
            {
                throw new SpeechletException("Invalid Intent");
            }
        }
        public override async Task<SpeechletResponse> OnLaunchAsync(LaunchRequest launchRequest, Session session)
        {
            Log.Info("OnLaunch requestId={0}, sessionId={1}", launchRequest.RequestId, session.SessionId);
            return await GetWelcomeResponse();
        }
        public override async Task OnSessionStartedAsync(SessionStartedRequest sessionStartedRequest, Session session)
        {
            Log.Info("OnSessionStarted requestId={0}, sessionId={1}", sessionStartedRequest.RequestId, session.SessionId);
        }
        public override async Task OnSessionEndedAsync(SessionEndedRequest sessionEndedRequest, Session session)
        {
            Log.Info("OnSessionEnded requestId={0}, sessionId={1}", sessionEndedRequest.RequestId, session.SessionId);
        }

        private async Task<SpeechletResponse> ControlRoku(Intent intent, Session session)
        {
            var keyvalues = intent.Slots["key"].Value;

            foreach (var v in keyvalues.Split(new String[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                RokuClient.PressKey(rokuendpoint, (RokuClient.Keymap.Keys.Contains(v) ? RokuClient.Keymap[v] : v));
            }

            var speechOutput = String.Format("Pressing {0}", keyvalues);

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return await BuildSpeechletResponse("Key Press", speechOutput, false);
        }
        private async Task<SpeechletResponse> TypeWords(Intent intent, Session session)
        {
            var speechOutput = "";
            var keyvalues = intent.Slots["search"].Value;
            if (!String.IsNullOrWhiteSpace(keyvalues))
            {
                foreach (var v in keyvalues.ToCharArray())
                {
                    RokuClient.PressKey(rokuendpoint, "Lit_" + v.ToString().ToLower());
                    Thread.Sleep(10);
                }
                speechOutput = String.Format("Typing {0}", keyvalues);
            }

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return await BuildSpeechletResponse("Typing", speechOutput, false);
        }
        private async Task<SpeechletResponse> LaunchApp(Intent intent, Session session)
        {
            var appname = intent.Slots["app"].Value;
            var speechOutput = "";

            if (!String.IsNullOrWhiteSpace(appname))
            {
                RokuClient.LaunchAppByName(rokuendpoint, appname);
                speechOutput = String.Format("Launching {0}", appname);
            }

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input            
            return await BuildSpeechletResponse("Open App", speechOutput, false);
        }

        private async Task<SpeechletResponse> GetWelcomeResponse()
        {
            // Create the welcome message.
            var speechOutput = "Welcome to Roku Control";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return await BuildSpeechletResponse("Welcome", speechOutput, false);
        }
    
        private async Task<SpeechletResponse> BuildSpeechletResponse(string title, string output, bool shouldEndSession)
        {
            //Create a standard card
            var image = new Image()
            {
                SmallImageUrl = "https://samman.hopto.org/rokusmall.png",
                LargeImageUrl = "https://samman.hopto.org/rokubig.png"
            };

            var card = new StandardCard()
            {
                Title = String.Format("RokuControl - {0}", title),
                Text = String.Format("{0}", output),
                Image = image
            };
           
            // Create the plain text output.
            var speech = new PlainTextOutputSpeech()
            {
                Text = output
            };

            // Create the speechlet response.
            var response = new SpeechletResponse()
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = speech,
                Card = card
            };

            return response;
        }
    }
}