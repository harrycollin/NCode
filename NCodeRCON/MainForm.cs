using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Forms;
using NCodeRCON.RConClient;
using MetroFramework.Controls;

namespace NCodeRCON
{
    public partial class MainForm : MetroForm
    {
        public ServerInstance SelectedServer;
        public List<ServerInstance> servers = new List<ServerInstance>();
        public AddServerForm addServerForm;
        public SettingsForm settingsForm;

        public MainForm()
        {
            InitializeComponent();
        }

       
        public void SelectServer(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            for(int i = 0; i < servers.Count; i++)
            {
                if (servers[i].Name == control.Name)
                {
                    SelectedServer = servers[i];
                    
                    StartServerButton.Enabled = true;
                    StopServerButton.Enabled = true;
                    SettingsButton.Enabled = true;
                    ShutdownServerButton.Enabled = true;
                    RemoveServerButton.Enabled = true;
                    break;
                }
            }
        }
        
        public void AddServer(ServerInstance instance)
        {
            Color darkGrey = Color.FromArgb(44, 44, 44);

            servers.Add(instance);
           
            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.Height = 90;
            flp.Width = 390;
            flp.BackColor = darkGrey;
            flp.Click += new EventHandler(this.SelectServer);
            flp.Name = instance.Name;
            flowLayoutPanel1.Controls.Add(flp);
         
            Label namelabel = new Label();
            namelabel.Text = instance.Name;
            namelabel.TextAlign = ContentAlignment.MiddleLeft;
            namelabel.Font = new Font("Arial", 10, FontStyle.Bold);
            namelabel.ForeColor = Color.White;

            Label iplabel = new Label();
            iplabel.Text = "IP: " + instance.IP.ToString();
            iplabel.TextAlign = ContentAlignment.MiddleLeft;
            iplabel.Font = new Font("Arial", 10, FontStyle.Bold);
            iplabel.ForeColor = Color.White;

            Label portlabel = new Label();
            portlabel.Text = "Port: " + instance.Port.ToString();
            portlabel.TextAlign = ContentAlignment.MiddleLeft;
            portlabel.Font = new Font("Arial", 10, FontStyle.Bold);
            portlabel.ForeColor = Color.White;
                   
            flp.Controls.Add(namelabel);
            flp.Controls.Add(iplabel);
            flp.Controls.Add(portlabel); 
        }
           
        void OpenServerSettings(object o, EventArgs e)
        {
            SelectServer(o, e);
            settingsForm = new SettingsForm(this);
            settingsForm.Show();
        }

        private void NewServerButton_Click(object sender, EventArgs e)
        {
            addServerForm = new AddServerForm(this);
            addServerForm.Show();
        }

        private void StartServerButton_Click(object sender, EventArgs e)
        {
            SelectedServer.Start();
        }

        private void StopServerButton_Click(object sender, EventArgs e)
        {
            SelectedServer.Stop();
        }

        private void ShutdownServerButton_Click(object sender, EventArgs e)
        {
            SelectedServer.Shutdown();
        }

        private void RemoveServerButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Remove selected server?", "Remove Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                foreach(Control i in flowLayoutPanel1.Controls)
                {
                    if(i.Name == SelectedServer.Name)
                    {
                        i.Dispose();
                    }
                }
                servers.Remove(SelectedServer);

            }
        }
    }
}
