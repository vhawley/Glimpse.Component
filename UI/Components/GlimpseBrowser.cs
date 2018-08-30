using System;
using System.Configuration;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public partial class GlimpseBrowser : Form
    {

        public GlimpseBrowser()
        {
            InitializeComponent();

            webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
            webBrowser1.Navigated += webBrowser1_Navigated;
            webBrowser1.Navigating += webBrowser1_Navigating;
        }

        public void loadLoginUrl()
        {
            webBrowser1.Navigate("url/login/creator");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Console.Out.WriteLine("DocumentCompleted");
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            Console.Out.WriteLine("Navigated to... " + e.Url.ToString());
            textBox1.Text = e.Url.ToString();
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            Console.Out.WriteLine("Navigating to... " + e.Url.ToString());
            textBox1.Text = e.Url.ToString();
        }
    }
}
