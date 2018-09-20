using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        public GlimpseBrowser GlimpseBrowser { get; set; }

        public Settings()
        {
            InitializeComponent();
            AccessToken = "0";
            RefreshToken = "0";
            IdToken = "0";
        }

        public string AccessToken;
        public string RefreshToken;
        public string IdToken;
        private static readonly HttpClient client = new HttpClient();

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsAccessNode(document, parent);
            CreateSettingsRefreshNode(document, parent);
            CreateSettingsIdNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsAccessNode(null, null);
        }

        private int CreateSettingsAccessNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "AccessToken", AccessToken);
        }

        private int CreateSettingsRefreshNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "RefreshToken", RefreshToken);
        }

        private int CreateSettingsIdNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "IdToken", IdToken);
        }

        public void SetSettings(XmlNode settings)
        {
            Console.Out.WriteLine("SetSettings");
            AccessToken = SettingsHelper.ParseString(settings["AccessToken"]);
            RefreshToken = SettingsHelper.ParseString(settings["RefreshToken"]);
            IdToken = SettingsHelper.ParseString(settings["IdToken"]);
        }

        public async void UpdateDisplayName()
        {
            nameLabel.Text = "Not Logged In";
            // Make user request
            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.dev.glimpsesr.com/user");
            userRequest.Headers.Add("Authorization", "OAuth " + AccessToken);
            HttpResponseMessage response = await client.SendAsync(userRequest);
            try
            {
                JObject userJson = JObject.Parse(await response.Content.ReadAsStringAsync());
                string name = userJson.Value<string>("displayName");
                if (name != null)
                {
                    nameLabel.Text = name;
                }
            }
            catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }
        }

        public void ReceiveCredentials(object sender, CredentialsEventArgs e)
        {
            JObject creds = e.Message;
            AccessToken = creds.Value<string>("access_token");
            RefreshToken = creds.Value<string>("refresh_token");
            IdToken = creds.Value<string>("id_token");
            GlimpseBrowser.Close();

            UpdateDisplayName();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GlimpseBrowser = new GlimpseBrowser();
            GlimpseBrowser.RaiseCredentialsEvent += ReceiveCredentials;
            GlimpseBrowser.Show();
            GlimpseBrowser.loadLoginUrl();
        }
    }
}
