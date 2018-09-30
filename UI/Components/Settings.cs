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
        private RequestFactory Factory;

        public Settings(RequestFactory factory)
        {
            InitializeComponent();
            Factory = factory;

            UpdateDisplayName();
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            Console.Out.WriteLine("GetSettings");
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
            return SettingsHelper.CreateSetting(document, parent, "AccessToken", Factory.GetAccessToken());
        }

        private int CreateSettingsRefreshNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "RefreshToken", Factory.GetRefreshToken());
        }

        private int CreateSettingsIdNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "IdToken", Factory.GetIdToken());
        }

        public void SetSettings(XmlNode settings)
        {
            Console.Out.WriteLine("SetSettings");
            string accessToken = SettingsHelper.ParseString(settings["AccessToken"]);
            string refreshToken = SettingsHelper.ParseString(settings["RefreshToken"]);
            string idToken = SettingsHelper.ParseString(settings["IdToken"]);

            // it's ok if they're null
            Factory.SetCredentials(accessToken, refreshToken, idToken);
        }

        public void ReceiveCredentials(object sender, CredentialsEventArgs e)
        {
            Console.Out.WriteLine("ReceiveCredentials");

            JObject creds = e.Message;
            string accessToken = creds.Value<string>("access_token");
            string refreshToken = creds.Value<string>("refresh_token");
            string idToken = creds.Value<string>("id_token");

            Factory.SetCredentials(accessToken, refreshToken, idToken);

            GlimpseBrowser.Close();

            UpdateDisplayName();
        }

        public async void UpdateDisplayName()
        {
            string name = await Factory.GetDisplayName();
            nameLabel.Text = name == null ? "Not Logged In" : name;
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
