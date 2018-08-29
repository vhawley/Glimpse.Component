using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    { 
        public Settings()
        {
            InitializeComponent();
            
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "AccessToken", "0");
        }

        public void SetSettings(XmlNode settings)
        {
            //PortString = SettingsHelper.ParseString(settings["AccessToken"]);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
