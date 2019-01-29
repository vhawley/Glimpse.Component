using System;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        private RequestFactory Factory;

        // public bool SendAfterFirstSplit { get; set; }

        public Settings(RequestFactory factory)
        {
            InitializeComponent();
            Factory = factory;
            // SendAfterFirstSplit = false;
            keyTextBox.DataBindings.Add("Text", Factory, "GlimpseKey", false, DataSourceUpdateMode.OnPropertyChanged);
            // sendAfterFirstSplitCheckbox.DataBindings.Add("Checked", this, "SendAfterFirstSplit", false, DataSourceUpdateMode.OnPropertyChanged);
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
            // SettingsHelper.CreateSetting(document, parent, "SendAfterFirstSplit", sendAfterFirstSplitCheckbox.Checked);
            return SettingsHelper.CreateSetting(document, parent, "GlimpseKey", keyTextBox.Text);
        }

        public void SetSettings(XmlNode settings)
        {
            Console.Out.WriteLine("SetSettings");
            string glimpseKey = SettingsHelper.ParseString(settings["GlimpseKey"]);
            // SendAfterFirstSplit = SettingsHelper.ParseBool(settings["SendAfterFirstSplit"], false);

            // it's ok if they're null
            Factory.GlimpseKey = glimpseKey;
        }
    }
}
