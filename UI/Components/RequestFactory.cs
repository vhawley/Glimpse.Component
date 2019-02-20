using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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

        public async Task<HttpResponseMessage> PostGlimpseRunStartEvent(string gameName, string categoryName, List<string> splitNames, Dictionary<string, List<double?>> comparisons, DateTime startTime)
        {
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
            requestContent["type"] = "GlimpseRunStart";
            requestContent["gameName"] = gameName;
            requestContent["categoryName"] = categoryName;
            requestContent["splitNames"] = splitNames;
            requestContent["comparisons"] = comparisons;
            requestContent["startTime"] = startTime.ToString("o");

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);
            
            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunSplitEvent(long runID, DateTime eventTime, int completedSplitNumber, TimeSpan contributableDuration, TimeSpan totalDuration)
        {
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
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
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
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
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
            requestContent["type"] = "GlimpseRunResume";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunSplitSkipEvent(long runID, DateTime eventTime, TimeSpan totalDuration)
        {
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
            requestContent["type"] = "GlimpseRunSplitSkip";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunSplitUndoEvent(long runID, DateTime eventTime, TimeSpan totalDuration)
        {
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
            requestContent["type"] = "GlimpseRunSplitUndo";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        public async Task<HttpResponseMessage> PostGlimpseRunEndEvent(long runID, DateTime eventTime, int completedSplitNumber, TimeSpan contributableDuration, TimeSpan totalDuration)
        {
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
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
            Dictionary<string, object> requestContent = new Dictionary<string, object>();
            requestContent["type"] = "GlimpseRunFinalize";
            requestContent["runID"] = runID;
            requestContent["eventTime"] = eventTime.ToString("o");
            requestContent["totalDuration"] = totalDuration.TotalMilliseconds;

            HttpResponseMessage response = await PostGlimpseEvent(requestContent);

            return response;
        }

        private async Task<HttpResponseMessage> PostGlimpseEvent(Dictionary<string, object> content, int tries = 0)
        {
            // Make user request
            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Post, BaseApiUrl + "event");
            if (GlimpseKey != null && !GlimpseKey.Equals(""))
            {
                userRequest.Headers.Add("Authorization", GlimpseKey);
            }

            // serialize post object
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string contentString = serializer.Serialize(content);
            userRequest.Content = new StringContent(contentString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(userRequest);

            // only retry if 401 and we've tried less than 3 times and there's a refresh token
            if (response.StatusCode != HttpStatusCode.OK && tries < 3 && GlimpseKey != null)
            {
                return await PostGlimpseEvent(content, tries + 1);
            } else if (response.StatusCode != HttpStatusCode.OK)
            {
                await LogToGlimpse(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            }

            return response;
        }

        public async Task LogToGlimpse(string content)
        {
            HttpRequestMessage logRequest = new HttpRequestMessage(HttpMethod.Post, BaseApiUrl + "log");

            StringContent logContent = new StringContent(content, Encoding.UTF8);
            logRequest.Content = logContent;

            HttpResponseMessage response = await client.SendAsync(logRequest);
        } 
    }
}
