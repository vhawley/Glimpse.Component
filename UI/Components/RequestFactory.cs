﻿using Newtonsoft.Json.Linq;
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
        private string AccessToken;
        private string RefreshToken;
        private string IdToken;
        private static string BaseApiUrl = "https://api.dev.glimpsesr.com/";

        public RequestFactory() { }

        public RequestFactory(string accessToken, string refreshToken, string idToken)
        {
            SetCredentials(accessToken, refreshToken, idToken);
        }

        public void SetCredentials(string accessToken, string refreshToken, string idToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            IdToken = idToken;
        }

        public string GetAccessToken()
        {
            return AccessToken;
        }

        public string GetRefreshToken()
        {
            return RefreshToken;
        }

        public string GetIdToken()
        {
            return IdToken;
        }

        private async Task<HttpStatusCode> RefreshCredentials()
        {
            // Make refresh token request
            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Post, BaseApiUrl + "refresh");
            if (RefreshToken != null)
            {
                userRequest.Content = new StringContent(RefreshToken);
            }
            HttpResponseMessage response = await client.SendAsync(userRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    JObject creds = JObject.Parse(await response.Content.ReadAsStringAsync());
                    string accessToken = creds.Value<string>("access_token");
                    string refreshToken = creds.Value<string>("refresh_token");
                    AccessToken = accessToken;
                    RefreshToken = refreshToken;
                } catch (Exception exc)
                {
                    Console.Out.WriteLine(exc.Message);
                }
            } else
            {
                Console.Out.WriteLine(await response.Content.ReadAsStringAsync());
            }
            return response.StatusCode;
        }

        public async Task<string> GetDisplayName(int tries = 0)
        {
            // Make user request
            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Get, BaseApiUrl + "user");
            if (AccessToken != null)
            {
                userRequest.Headers.Add("Authorization", "OAuth " + AccessToken);
            }
            HttpResponseMessage response = await client.SendAsync(userRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    JObject userJson = JObject.Parse(await response.Content.ReadAsStringAsync());
                    return userJson.Value<string>("displayName");
                }
                catch (Exception exc)
                {
                    Console.Out.WriteLine(exc.Message);
                    return null;
                }
            }
            // only retry if 401 and we've tried less than 3 times and there's a refresh token
            else if (response.StatusCode == HttpStatusCode.Unauthorized && tries < 3 && RefreshToken != null)
            {
                if (await RefreshCredentials() == HttpStatusCode.OK)
                {
                    return await GetDisplayName(tries + 1);
                }
            }
            return null;
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

        public async Task<HttpResponseMessage> PostGlimpseRunSplitEvent(int runID, DateTime eventTime, int completedSplitNumber, TimeSpan contributableDuration, TimeSpan totalDuration)
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

        public async Task<HttpResponseMessage> PostGlimpseRunEndEvent(int runID, DateTime eventTime, int completedSplitNumber, TimeSpan contributableDuration, TimeSpan totalDuration)
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

        private async Task<HttpResponseMessage> PostGlimpseEvent(JObject content)
        {
            // Make user request
            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Post, BaseApiUrl + "event");
            if (AccessToken != null)
            {
                userRequest.Headers.Add("Authorization", "OAuth " + AccessToken);
            }
           
            userRequest.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(userRequest);

            return response;
        }

    }
}