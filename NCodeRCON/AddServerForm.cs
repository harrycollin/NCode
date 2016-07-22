using MetroFramework.Controls;
using NCodeRCON.RConClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCodeRCON
{
    public partial class AddServerForm : MetroFramework.Forms.MetroForm
    {
        private MainForm mainForm = null;

        ServerInstance instance = new ServerInstance();
        bool valueNull = false;
        bool Validated = false;

        public AddServerForm(Form callingForm)
        {
            mainForm = callingForm as MainForm;
            InitializeComponent();
            addServerNameBox.TextChanged += new EventHandler(OnValuesChanged);
            addServerIPBox.TextChanged += new EventHandler(ValidateIP);
            addServerIPBox.TextChanged += new EventHandler(ImportantInfoChanged);
            addServerIPBox.TextChanged += new EventHandler(OnValuesChanged);
            addServerPortBox.TextChanged += new EventHandler(OnValuesChanged);
            addServerPortBox.TextChanged += new EventHandler(ImportantInfoChanged);
            addServerPasswordBox.TextChanged += new EventHandler(OnValuesChanged);
            addServerPasswordBox.TextChanged += new EventHandler(ImportantInfoChanged);

        }
      
        private void AddServerForm_Load(object sender, EventArgs e)
        {

        }

        public void ImportantInfoChanged(object o, EventArgs e)
        {
            Validated = false;
            addServerButton.Enabled = false;
        }

        /// <summary>
        /// Checks for null or invalid values in text fields
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void OnValuesChanged(object o, EventArgs e)
        {
            if (addServerNameBox.Text == "" || addServerIPBox.Text == "" || addServerPortBox.Text == "" || addServerPasswordBox.Text == "")
            {
                valueNull = true;
                testConnectionButton.Enabled = false;
            }
            else
            {
                instance.Name = addServerNameBox.Text;
                instance.Password = addServerPasswordBox.Text;
                instance.Port = int.Parse(addServerPortBox.Text);
                valueNull = false;
                testConnectionButton.Enabled = true;
            }

        }

        public void ValidateIP(object o, EventArgs e)
        {
            MetroTextBox t = (MetroTextBox)o;
            string value = t.Text;
            //Change the colour of the text
            if (IsValidIPv4(value)) { t.ForeColor = Color.DarkGreen; t.CustomForeColor = true; instance.IP = IPAddress.Parse(addServerIPBox.Text); }
            else { t.ForeColor = Color.DarkRed; t.CustomForeColor = true; }
        }

        public bool IsValidIPv4(string value)
        {         
            var quads = value.Split('.');
            // if we do not have 4 quads, return false
            if (!(quads.Length == 4)) return false;

            // for each quad
            foreach (var quad in quads)
            {
                int q;
                // if parse fails 
                // or length of parsed int != length of quad string (i.e.; '1' vs '001')
                // or parsed int < 0
                // or parsed int > 255
                // return false
                if (!Int32.TryParse(quad, out q)
                    || !q.ToString().Length.Equals(quad.Length)
                    || q < 0
                    || q > 255) { return false; }

            }
            return true;
        }

        private void testConnectionButton_Click(object sender, EventArgs e)
        {
            instance.Port = int.Parse(addServerPortBox.Text);
            instance.Password = addServerPasswordBox.Text;
            if(instance.client.Connect(instance.IP, instance.Port))
            {
                Thread.Sleep(2000);
                BinaryWriter writer = instance.client.TcpProtocol.BeginSend(NCode.Packet.RConRequestAuthenticate);
                writer.Write(instance.Password);
                instance.client.TcpProtocol.EndSend();
            }

            Thread.Sleep(5000);  
                     
            if (instance.client.Authenticated)
            {
                addServerButton.Enabled = true;
                MessageBox.Show("Authenticated");
                Validated = true;
            }
            else
            {
                MessageBox.Show("Authentication failed!");
                Validated = false;
            }
        }

        private void addServerButton_Click(object sender, EventArgs e)
        {
            mainForm.AddServer(instance);
            this.Close();
        }
    }
}
