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
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.metroPanel2 = new MetroFramework.Controls.MetroPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.PlayersTab = new MetroFramework.Controls.MetroTabPage();
            this.RemoveServerButton = new MetroFramework.Controls.MetroButton();
            this.NewServerButton = new MetroFramework.Controls.MetroButton();
            this.ShutdownServerButton = new MetroFramework.Controls.MetroButton();
            this.SettingsButton = new MetroFramework.Controls.MetroButton();
            this.StatsPanel = new MetroFramework.Controls.MetroPanel();
            this.StopServerButton = new MetroFramework.Controls.MetroButton();
            this.StartServerButton = new MetroFramework.Controls.MetroButton();
            this.PlayersLabel = new MetroFramework.Controls.MetroLabel();
            this.GameStatusLabel = new MetroFramework.Controls.MetroLabel();
            this.ServerStatus = new MetroFramework.Controls.MetroLabel();
            this.TabControl1.SuspendLayout();
            this.MainPanelTab.SuspendLayout();
            this.metroPanel1.SuspendLayout();
            this.metroPanel2.SuspendLayout();
            this.StatsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.MainPanelTab);
            this.TabControl1.Controls.Add(this.PlayersTab);
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
            this.MainPanelTab.Text = "Servers";
            this.MainPanelTab.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.MainPanelTab.VerticalScrollbarBarColor = true;
            // 
            // metroPanel1
            // 
            this.metroPanel1.Controls.Add(this.StartServerButton);
            this.metroPanel1.Controls.Add(this.StopServerButton);
            this.metroPanel1.Controls.Add(this.StatsPanel);
            this.metroPanel1.Controls.Add(this.SettingsButton);
            this.metroPanel1.Controls.Add(this.ShutdownServerButton);
            this.metroPanel1.Controls.Add(this.NewServerButton);
            this.metroPanel1.Controls.Add(this.RemoveServerButton);
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
            // metroPanel2
            // 
            this.metroPanel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
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
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(400, 333);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // PlayersTab
            // 
            this.PlayersTab.HorizontalScrollbarBarColor = true;
            this.PlayersTab.Location = new System.Drawing.Point(4, 35);
            this.PlayersTab.Name = "PlayersTab";
            this.PlayersTab.Size = new System.Drawing.Size(578, 345);
            this.PlayersTab.TabIndex = 1;
            this.PlayersTab.Text = "Players";
            this.PlayersTab.VerticalScrollbarBarColor = true;
            // 
            // RemoveServerButton
            // 
            this.RemoveServerButton.Enabled = false;
            this.RemoveServerButton.Location = new System.Drawing.Point(4, 307);
            this.RemoveServerButton.Name = "RemoveServerButton";
            this.RemoveServerButton.Size = new System.Drawing.Size(161, 27);
            this.RemoveServerButton.TabIndex = 2;
            this.RemoveServerButton.Text = "Remove";
            this.RemoveServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.RemoveServerButton.Click += new System.EventHandler(this.RemoveServerButton_Click);
            // 
            // NewServerButton
            // 
            this.NewServerButton.Location = new System.Drawing.Point(4, 274);
            this.NewServerButton.Name = "NewServerButton";
            this.NewServerButton.Size = new System.Drawing.Size(161, 27);
            this.NewServerButton.TabIndex = 3;
            this.NewServerButton.Text = "New";
            this.NewServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.NewServerButton.Click += new System.EventHandler(this.NewServerButton_Click);
            // 
            // ShutdownServerButton
            // 
            this.ShutdownServerButton.Enabled = false;
            this.ShutdownServerButton.Location = new System.Drawing.Point(4, 241);
            this.ShutdownServerButton.Name = "ShutdownServerButton";
            this.ShutdownServerButton.Size = new System.Drawing.Size(161, 27);
            this.ShutdownServerButton.TabIndex = 4;
            this.ShutdownServerButton.Text = "Shutdown";
            this.ShutdownServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.ShutdownServerButton.Click += new System.EventHandler(this.ShutdownServerButton_Click);
            // 
            // SettingsButton
            // 
            this.SettingsButton.Enabled = false;
            this.SettingsButton.Location = new System.Drawing.Point(4, 208);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(161, 27);
            this.SettingsButton.TabIndex = 5;
            this.SettingsButton.Text = "Connection Settings";
            this.SettingsButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // StatsPanel
            // 
            this.StatsPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
            this.StatsPanel.Controls.Add(this.PlayersLabel);
            this.StatsPanel.Controls.Add(this.ServerStatus);
            this.StatsPanel.Controls.Add(this.GameStatusLabel);
            this.StatsPanel.Enabled = false;
            this.StatsPanel.HorizontalScrollbarBarColor = true;
            this.StatsPanel.HorizontalScrollbarHighlightOnWheel = false;
            this.StatsPanel.HorizontalScrollbarSize = 10;
            this.StatsPanel.Location = new System.Drawing.Point(4, 3);
            this.StatsPanel.Name = "StatsPanel";
            this.StatsPanel.Size = new System.Drawing.Size(161, 136);
            this.StatsPanel.TabIndex = 6;
            this.StatsPanel.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.StatsPanel.VerticalScrollbarBarColor = true;
            this.StatsPanel.VerticalScrollbarHighlightOnWheel = false;
            this.StatsPanel.VerticalScrollbarSize = 10;
            // 
            // StopServerButton
            // 
            this.StopServerButton.Enabled = false;
            this.StopServerButton.Location = new System.Drawing.Point(4, 175);
            this.StopServerButton.Name = "StopServerButton";
            this.StopServerButton.Size = new System.Drawing.Size(161, 27);
            this.StopServerButton.TabIndex = 7;
            this.StopServerButton.Text = "Stop";
            this.StopServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.StopServerButton.Click += new System.EventHandler(this.StopServerButton_Click);
            // 
            // StartServerButton
            // 
            this.StartServerButton.Enabled = false;
            this.StartServerButton.Location = new System.Drawing.Point(4, 142);
            this.StartServerButton.Name = "StartServerButton";
            this.StartServerButton.Size = new System.Drawing.Size(161, 27);
            this.StartServerButton.TabIndex = 8;
            this.StartServerButton.Text = "Start";
            this.StartServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.StartServerButton.Click += new System.EventHandler(this.StartServerButton_Click);
            // 
            // PlayersLabel
            // 
            this.PlayersLabel.AutoSize = true;
            this.PlayersLabel.Location = new System.Drawing.Point(3, 38);
            this.PlayersLabel.Name = "PlayersLabel";
            this.PlayersLabel.Size = new System.Drawing.Size(120, 19);
            this.PlayersLabel.TabIndex = 0;
            this.PlayersLabel.Text = "Connected Players:";
            this.PlayersLabel.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // GameStatusLabel
            // 
            this.GameStatusLabel.AutoSize = true;
            this.GameStatusLabel.Location = new System.Drawing.Point(3, 19);
            this.GameStatusLabel.Name = "GameStatusLabel";
            this.GameStatusLabel.Size = new System.Drawing.Size(90, 19);
            this.GameStatusLabel.TabIndex = 1;
            this.GameStatusLabel.Text = "Game Server:";
            this.GameStatusLabel.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // ServerStatus
            // 
            this.ServerStatus.AutoSize = true;
            this.ServerStatus.Location = new System.Drawing.Point(3, 0);
            this.ServerStatus.Name = "ServerStatus";
            this.ServerStatus.Size = new System.Drawing.Size(88, 19);
            this.ServerStatus.TabIndex = 2;
            this.ServerStatus.Text = "Server Status:";
            this.ServerStatus.Theme = MetroFramework.MetroThemeStyle.Dark;
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
            this.metroPanel1.ResumeLayout(false);
            this.metroPanel2.ResumeLayout(false);
            this.StatsPanel.ResumeLayout(false);
            this.StatsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTabControl TabControl1;
        private MetroFramework.Controls.MetroTabPage MainPanelTab;
        private MetroFramework.Controls.MetroPanel metroPanel2;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private MetroFramework.Controls.MetroTabPage PlayersTab;
        private MetroFramework.Controls.MetroButton StartServerButton;
        private MetroFramework.Controls.MetroButton StopServerButton;
        private MetroFramework.Controls.MetroPanel StatsPanel;
        private MetroFramework.Controls.MetroButton SettingsButton;
        private MetroFramework.Controls.MetroButton ShutdownServerButton;
        private MetroFramework.Controls.MetroButton NewServerButton;
        private MetroFramework.Controls.MetroButton RemoveServerButton;
        private MetroFramework.Controls.MetroLabel PlayersLabel;
        private MetroFramework.Controls.MetroLabel ServerStatus;
        private MetroFramework.Controls.MetroLabel GameStatusLabel;
    }
}

