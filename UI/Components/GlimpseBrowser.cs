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
        }

        public void loadLoginUrl()
        {
            webBrowser1.Navigate("https://google.com");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser wb = (WebBrowser) sender;
        }
    }
}
