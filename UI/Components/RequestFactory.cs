using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LiveSplit.UI.Components
{
    public class RequestFactory
    {

        private static readonly HttpClient client = new HttpClient();
        public string GlimpseKey { get; set; }
        private static string BaseApiUrl = "https://api.glimpsesr.com/";

        public RequestFactory() { }

        public RequestFactory(string glimpseKey)
        {
            GlimpseKey = glimpseKey;
        }

        public string GetGlimpseKey()
        {
            return GlimpseKey;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunStartEvent(string gameName, string categoryName, List<string> splitNames, JObject comparisons, DateTime startTime)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunStart";
            requestContent["gameName"] = gameName;
            requestContent["categoryName"] = categoryName;
            requestContent["splitNames"] = JArray.FromObject(splitNames);
            requestContent["comparisons"] = comparisons;
            requestContent["startTime"] = startTime.ToString("o");

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);
            
            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunSplitEvent(long runID, DateTime eventTime, int completedSplitNumber, TimeSpan contributableDuration, TimeSpan totalDuration)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunSplit";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["completedSplitNumber"] = completedSplitNumber;
            requestContent["contributableDuration"] = contributableDuration.TotalMilliseconds;
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunPauseEvent(long runID, DateTime eventTime, TimeSpan contributableDuration, TimeSpan totalDuration)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunPause";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["contributableDuration"] = contributableDuration.TotalMilliseconds;
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunResumeEvent(long runID, DateTime eventTime, TimeSpan totalDuration)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunResume";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunSplitSkipEvent(long runID, DateTime eventTime, TimeSpan totalDuration)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunSplitSkip";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunSplitUndoEvent(long runID, DateTime eventTime, TimeSpan totalDuration)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunSplitUndo";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunEndEvent(long runID, DateTime eventTime, int completedSplitNumber, TimeSpan contributableDuration, TimeSpan totalDuration)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunEnd";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["completedSplitNumber"] = completedSplitNumber;
            requestContent["contributableDuration"] = contributableDuration.TotalMilliseconds;
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunFinalizeEvent(long runID, DateTime eventTime, TimeSpan totalDuration)
        {
            JObject requestContent = new JObject();
            requestContent["type"] = "GlimpseRunFinalize";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        private async Task<HttpResponseMessage> PostGlimpseEvent(JObject content, int tries = 0)
        {
            // Make user request
            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Post, BaseApiUrl + "event");
            if (GlimpseKey != null && !GlimpseKey.Equals(""))
            {
                userRequest.Headers.Add("Authorization", GlimpseKey);
            }
           
            userRequest.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(userRequest);

            // only retry if 401 and we've tried less than 3 times and there's a refresh token
            if (response.StatusCode != HttpStatusCode.OK && tries < 3 && GlimpseKey != null)
            {
                return await PostGlimpseEvent(content, tries + 1);
            } else if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            }

            return response;
        }

    }
}
