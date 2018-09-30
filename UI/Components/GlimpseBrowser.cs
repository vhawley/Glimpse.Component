using System;
using System.Configuration;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class GlimpseBrowser : Form
    {

        public event EventHandler<CredentialsEventArgs> RaiseCredentialsEvent;

        public GlimpseBrowser()
        {
            InitializeComponent();

            webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
            webBrowser1.Navigated += webBrowser1_Navigated;
            webBrowser1.Navigating += webBrowser1_Navigating;
            
        }

        public void loadLoginUrl()
        {
            webBrowser1.Navigate("https://api.dev.glimpsesr.com/login/creator");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Console.Out.WriteLine("DocumentCompleted");
            if (e.Url.ToString().StartsWith("https://api.dev.glimpsesr.com/redirect"))
            {
                OnRaiseCustomEvent(new CredentialsEventArgs(webBrowser1.Document.Body.InnerText));
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            Console.Out.WriteLine("Navigated to... " + e.Url.ToString());
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            Console.Out.WriteLine("Navigating to... " + e.Url.ToString());
            textBox1.Text = e.Url.ToString();
        }

        protected virtual void OnRaiseCustomEvent(CredentialsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            RaiseCredentialsEvent?.Invoke(this, e);
        }
    }

    public class CredentialsEventArgs : EventArgs
    {
        public CredentialsEventArgs(string s)
        {
            try
            {
                JObject json = JObject.Parse(s);
                message = json;
            }
            catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
                message = null;
            }
        }

        private JObject message;

        public JObject Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
