using System;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        private RequestFactory Factory;

        public Settings(RequestFactory factory)
        {
            InitializeComponent();
            Factory = factory;
            keyTextBox.DataBindings.Add("Text", Factory, "GlimpseKey", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            Console.Out.WriteLine("GetSettings");
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
            return SettingsHelper.CreateSetting(document, parent, "GlimpseKey", keyTextBox.Text);
        }

        public void SetSettings(XmlNode settings)
        {
            Console.Out.WriteLine("SetSettings");
            string glimpseKey = SettingsHelper.ParseString(settings["GlimpseKey"]);

            // it's ok if they're null
            Factory.GlimpseKey = glimpseKey;
            
        }
    }
}
