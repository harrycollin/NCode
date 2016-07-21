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

namespace NCodeRCON
{
    public partial class MainForm : MetroForm
    {
        public ServerInstance SelectedServer;
        public List<ServerInstance> servers = new List<ServerInstance>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {

        }

        public void SelectServer(object sender, EventArgs e)
        {
            Panel panel = (Panel)sender;
            for(int i = 0; i < servers.Count; i++)
            {
                if (servers[i].Name == panel.Name)
                {
                    SelectedServer = servers[i];
                    MessageBox.Show(SelectedServer.Name);
                    break;
                }
            }
        }
             
        private void button1_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < 2; i++)
            {
                ServerInstance s = new ServerInstance();
                s.Name = "server" + i;
                servers.Add(s);
                Panel p = new Panel();
                p.Click += new EventHandler(this.SelectServer);
                p.Name = "server" + i;
                p.Height = 90;
                p.Width = 375;
                p.BackColor = Color.DarkGray;
                flowLayoutPanel1.Controls.Add(p);

                Button settings = new Button();
                settings.Text = "Setting";
                
                p.Controls.Add(settings);
            }
        }

        private void addServerButton_Click(object sender, EventArgs e)
        {
            AddServerForm a = new AddServerForm();
            a.Show();
        }
    }
}
