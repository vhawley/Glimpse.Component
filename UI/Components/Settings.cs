using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;
using System.Xml;

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
            return SettingsHelper.CreateSetting(document, parent, "AccessToken", "0");
        }

        private int CreateSettingsRefreshNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "RefreshToken", "0");
        }

        private int CreateSettingsIdNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "IdToken", "0");
        }

        public void SetSettings(XmlNode settings)
        {
            AccessToken = SettingsHelper.ParseString(settings["AccessToken"]);
            RefreshToken = SettingsHelper.ParseString(settings["RefreshToken"]);
            IdToken = SettingsHelper.ParseString(settings["IdToken"]);
        }

        public void ReceiveCredentials(object sender, CredentialsEventArgs e)
        {
            JObject creds = e.Message;
            AccessToken = creds.Value<string>("access_token");
            RefreshToken = creds.Value<string>("refresh_token");
            IdToken = creds.Value<string>("id_token");
            GlimpseBrowser.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GlimpseBrowser = new GlimpseBrowser();
            GlimpseBrowser.RaiseCredentialsEvent += ReceiveCredentials;
            GlimpseBrowser.Show();
            GlimpseBrowser.loadLoginUrl();
        }

        private void nameLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
