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
        public AddServerForm addServerForm;

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
        
        public void AddServer(ServerInstance instance)
        {
            Color darkGrey = Color.FromArgb(44, 44, 44);

            servers.Add(instance);
            Panel p = new Panel();
            p.Click += new EventHandler(this.SelectServer);
            p.Name = "server:" + instance.Name;
            p.Height = 90;
            p.Width = 375;
            p.BackColor = darkGrey;         
            flowLayoutPanel1.Controls.Add(p);

            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.Height = 90;
            flp.Width = 375;
            
            p.Controls.Add(flp);

            Label namelabel = new Label();
            namelabel.Text = instance.Name;
            namelabel.TextAlign = ContentAlignment.MiddleLeft;
            namelabel.Font = new Font("Arial", 10, FontStyle.Bold);
            namelabel.ForeColor = Color.White;

            Label iplabel = new Label();
            iplabel.Text = "IP:" + instance.IP.ToString();
            iplabel.TextAlign = ContentAlignment.MiddleLeft;
            iplabel.Font = new Font("Arial", 10, FontStyle.Bold);
            iplabel.ForeColor = Color.White;

            Label portlabel = new Label();
            portlabel.Text = "Port:" + instance.Port.ToString();
            portlabel.TextAlign = ContentAlignment.MiddleLeft;
            portlabel.Font = new Font("Arial", 10, FontStyle.Bold);
            portlabel.ForeColor = Color.White;

            Button start = new Button();
            start.Text = "Start";

            Button stop = new Button();
            stop.Text = "Stop";

            flp.Controls.Add(namelabel);
            flp.Controls.Add(iplabel);
            flp.Controls.Add(portlabel);
            flp.Controls.Add(start);
            flp.Controls.Add(stop);

        }

        private void button1_Click(object sender, EventArgs e)
        {
                                       
        }
      

        private void addServerButton_Click(object sender, EventArgs e)
        {
            addServerForm = new AddServerForm(this);
            addServerForm.Show();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ServerInstance s = new ServerInstance();
            s.Name = "Kleos Server";
            AddServer(s);
        }
    }
}
