using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoStarter
{
    public partial class About : Form
    {
        static AutoStarter.Properties.Settings userSettings
        {
            get
            {
                return new AutoStarter.Properties.Settings();
            }
        }

        public About()
        {
            InitializeComponent();

            txt_Titel.Text = userSettings.Titel;
            lbl_Version.Text = "Version: "+userSettings.Version;
            txt_Build.Text = "Build: "+userSettings.Build;
            txt_BuildDate.Text = "Build-Date: "+userSettings.BuildDate;
        }
    }
}
