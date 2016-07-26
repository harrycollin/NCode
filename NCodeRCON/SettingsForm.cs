using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCodeRCON
{
    public partial class SettingsForm : MetroFramework.Forms.MetroForm
    {
        MainForm form;
        public SettingsForm(Form MainForm)
        {
            form = (MainForm)MainForm;
            InitializeComponent();
            ServerNameBox.Text = form.SelectedServer.Name;
            ServerIPBox.Text = form.SelectedServer.IP.ToString();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {

        }
    }
}
