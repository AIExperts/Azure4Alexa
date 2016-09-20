﻿using AlexaSkillsKit.Speechlet;
using Azure4Alexa.Alexa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Azure4Alexa.Tfl
{
    public class Status
    {

        // URL to GET Transport for London Status for all modes of transport
        // our example will be limited to just "tube"
        // "https://api.tfl.gov.uk/Line/Mode/tube,overground,dlr,tram,national-rail,cable-car,river-bus,river-tour/Status?detail=False"

        // See the StatusExample[1-3].json files in this folder for example results

        public static string tflStatusUrl =
            "https://api.tfl.gov.uk/Line/Mode/tube/Status?detail=False";


        // Call the remote web service.  Invoked from AlexaSpeechletAsync
        // Then, call another function with the raw JSON results to generate the spoken text and card text

        public static SpeechletResponse GetResults(Session session, HttpClient httpClient)
        {

            string httpResultString = "";

            // Connect to TFL API Endpoint

            httpClient.DefaultRequestHeaders.Clear();

            var httpResponseMessage =
                httpClient.GetAsync(tflStatusUrl).Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                httpResultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else
            {
                httpResponseMessage.Dispose();
                return AlexaUtils.BuildSpeechletResponse(new AlexaUtils.SimpleIntentResponse() { cardText = AlexaConstants.AppErrorMessage }, true);
            }


            var simpleIntentResponse = ParseResults(httpResultString);
            httpResponseMessage.Dispose();
            return AlexaUtils.BuildSpeechletResponse(simpleIntentResponse, true);

        }

        private static AlexaUtils.SimpleIntentResponse ParseResults(string resultString)
        {
            string stringToRead = String.Empty;
            string stringForCard = String.Empty;

            // you'll need to use JToken instead of JObject with TFL results

            dynamic resultObject = JToken.Parse(resultString);

            // if you're into structured data objects, use JArray
            // JArray resultObject2 = JArray.Parse(resultString);

            foreach (var i in resultObject)
            {

                if (i.lineStatuses != null)
                {
                    foreach (var j in i.lineStatuses)
                    {
              
                        if (j.disruption != null)
                        {
                            stringToRead += "<break time=\"2s\" /> ";
                            stringToRead += j.disruption.description + " ";
                            stringForCard += j.disruption.description + " \n";
                        }

                    }
                }
            }

            // Build the response

            if(stringForCard == String.Empty && stringToRead == String.Empty)
            {
                string noDisruptions = "There is a Good Service on the London Underground.";
                stringToRead += Alexa.AlexaUtils.AddSpeakTagsAndClean(noDisruptions);
                stringForCard = noDisruptions;
            }
            else
            {
                stringToRead = Alexa.AlexaUtils.AddSpeakTagsAndClean(stringToRead);
                stringForCard = "There are disruptions on the London Underground. The following lines are affected: \n" + stringForCard;
            }

            return new AlexaUtils.SimpleIntentResponse() { cardText = stringForCard, ssmlString = stringToRead };

            // if you want to add images, you can include them in the reply
            //return new AlexaUtils.SimpleIntentResponse() { cardText = stringForCard, ssmlString = stringToRead,
            //    largeImageUrl = "https://your-cors-friendly-url", smallImageUrl = "https://your-cors-friendly-url"};

        }
    }
} 