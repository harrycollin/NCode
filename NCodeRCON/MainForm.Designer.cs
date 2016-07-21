namespace NCodeRCON
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabControl1 = new MetroFramework.Controls.MetroTabControl();
            this.MainPanelTab = new MetroFramework.Controls.MetroTabPage();
            this.metroPanel2 = new MetroFramework.Controls.MetroPanel();
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.serverSettingsButton = new MetroFramework.Controls.MetroButton();
            this.addServerButton = new MetroFramework.Controls.MetroButton();
            this.removeServerButton = new MetroFramework.Controls.MetroButton();
            this.TabControl1.SuspendLayout();
            this.MainPanelTab.SuspendLayout();
            this.metroPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.MainPanelTab);
            this.TabControl1.Location = new System.Drawing.Point(23, 30);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(586, 384);
            this.TabControl1.TabIndex = 0;
            this.TabControl1.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // MainPanelTab
            // 
            this.MainPanelTab.Controls.Add(this.metroPanel1);
            this.MainPanelTab.Controls.Add(this.metroPanel2);
            this.MainPanelTab.ForeColor = System.Drawing.Color.Transparent;
            this.MainPanelTab.HorizontalScrollbarBarColor = true;
            this.MainPanelTab.Location = new System.Drawing.Point(4, 35);
            this.MainPanelTab.Name = "MainPanelTab";
            this.MainPanelTab.Size = new System.Drawing.Size(578, 345);
            this.MainPanelTab.TabIndex = 0;
            this.MainPanelTab.Text = "Overview";
            this.MainPanelTab.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.MainPanelTab.VerticalScrollbarBarColor = true;
            // 
            // metroPanel2
            // 
            this.metroPanel2.Controls.Add(this.removeServerButton);
            this.metroPanel2.Controls.Add(this.addServerButton);
            this.metroPanel2.Controls.Add(this.serverSettingsButton);
            this.metroPanel2.Controls.Add(this.flowLayoutPanel1);
            this.metroPanel2.HorizontalScrollbarBarColor = true;
            this.metroPanel2.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel2.HorizontalScrollbarSize = 10;
            this.metroPanel2.Location = new System.Drawing.Point(0, 3);
            this.metroPanel2.Name = "metroPanel2";
            this.metroPanel2.Size = new System.Drawing.Size(407, 339);
            this.metroPanel2.TabIndex = 5;
            this.metroPanel2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroPanel2.VerticalScrollbarBarColor = true;
            this.metroPanel2.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel2.VerticalScrollbarSize = 10;
            // 
            // metroPanel1
            // 
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 10;
            this.metroPanel1.Location = new System.Drawing.Point(409, 3);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(169, 339);
            this.metroPanel1.TabIndex = 6;
            this.metroPanel1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 10;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(400, 300);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // serverSettingsButton
            // 
            this.serverSettingsButton.Location = new System.Drawing.Point(300, 309);
            this.serverSettingsButton.Name = "serverSettingsButton";
            this.serverSettingsButton.Size = new System.Drawing.Size(103, 23);
            this.serverSettingsButton.TabIndex = 3;
            this.serverSettingsButton.Text = "Settings";
            this.serverSettingsButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // addServerButton
            // 
            this.addServerButton.Location = new System.Drawing.Point(3, 309);
            this.addServerButton.Name = "addServerButton";
            this.addServerButton.Size = new System.Drawing.Size(103, 23);
            this.addServerButton.TabIndex = 4;
            this.addServerButton.Text = "New";
            this.addServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.addServerButton.Click += new System.EventHandler(this.addServerButton_Click);
            // 
            // removeServerButton
            // 
            this.removeServerButton.Location = new System.Drawing.Point(112, 309);
            this.removeServerButton.Name = "removeServerButton";
            this.removeServerButton.Size = new System.Drawing.Size(103, 23);
            this.removeServerButton.TabIndex = 5;
            this.removeServerButton.Text = "Remove";
            this.removeServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 437);
            this.Controls.Add(this.TabControl1);
            this.Font = new System.Drawing.Font("Myriad Hebrew", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MainForm";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.TabControl1.ResumeLayout(false);
            this.MainPanelTab.ResumeLayout(false);
            this.metroPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTabControl TabControl1;
        private MetroFramework.Controls.MetroTabPage MainPanelTab;
        private MetroFramework.Controls.MetroPanel metroPanel2;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private MetroFramework.Controls.MetroButton removeServerButton;
        private MetroFramework.Controls.MetroButton addServerButton;
        private MetroFramework.Controls.MetroButton serverSettingsButton;
    }
}

