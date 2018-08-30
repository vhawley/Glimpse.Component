using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            webBrowser1.Navigate("https://google.com");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Console.Out.WriteLine("DocumentCompleted");
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            textBox1.Text = e.Url.ToString();
        }
    }
}
