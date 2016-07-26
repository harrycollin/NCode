namespace NCodeRCON
{
    partial class SettingsForm
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
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.addNewServerNameLabel = new System.Windows.Forms.Label();
            this.ServerPasswordBox = new MetroFramework.Controls.MetroTextBox();
            this.ServerPortBox = new MetroFramework.Controls.MetroTextBox();
            this.ServerIPBox = new MetroFramework.Controls.MetroTextBox();
            this.ServerNameBox = new MetroFramework.Controls.MetroTextBox();
            this.addServerButton = new MetroFramework.Controls.MetroButton();
            this.SaveButton = new MetroFramework.Controls.MetroButton();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label3.Location = new System.Drawing.Point(61, 167);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Password:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label2.Location = new System.Drawing.Point(88, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label1.Location = new System.Drawing.Point(56, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "IP Address:";
            // 
            // addNewServerNameLabel
            // 
            this.addNewServerNameLabel.AutoSize = true;
            this.addNewServerNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.addNewServerNameLabel.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.addNewServerNameLabel.Location = new System.Drawing.Point(79, 80);
            this.addNewServerNameLabel.Name = "addNewServerNameLabel";
            this.addNewServerNameLabel.Size = new System.Drawing.Size(38, 13);
            this.addNewServerNameLabel.TabIndex = 12;
            this.addNewServerNameLabel.Text = "Name:";
            // 
            // ServerPasswordBox
            // 
            this.ServerPasswordBox.Location = new System.Drawing.Point(123, 167);
            this.ServerPasswordBox.Name = "ServerPasswordBox";
            this.ServerPasswordBox.Size = new System.Drawing.Size(280, 23);
            this.ServerPasswordBox.TabIndex = 11;
            this.ServerPasswordBox.Text = "changeme";
            this.ServerPasswordBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // ServerPortBox
            // 
            this.ServerPortBox.Location = new System.Drawing.Point(123, 138);
            this.ServerPortBox.Name = "ServerPortBox";
            this.ServerPortBox.Size = new System.Drawing.Size(280, 23);
            this.ServerPortBox.TabIndex = 10;
            this.ServerPortBox.Text = "5129";
            this.ServerPortBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // ServerIPBox
            // 
            this.ServerIPBox.BackColor = System.Drawing.Color.Red;
            this.ServerIPBox.ForeColor = System.Drawing.Color.Coral;
            this.ServerIPBox.Location = new System.Drawing.Point(123, 109);
            this.ServerIPBox.Name = "ServerIPBox";
            this.ServerIPBox.Size = new System.Drawing.Size(280, 23);
            this.ServerIPBox.TabIndex = 9;
            this.ServerIPBox.Text = "127.0.0.1";
            this.ServerIPBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // ServerNameBox
            // 
            this.ServerNameBox.Location = new System.Drawing.Point(123, 80);
            this.ServerNameBox.Name = "ServerNameBox";
            this.ServerNameBox.Size = new System.Drawing.Size(280, 23);
            this.ServerNameBox.TabIndex = 8;
            this.ServerNameBox.Text = "bruh";
            this.ServerNameBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // addServerButton
            // 
            this.addServerButton.Enabled = false;
            this.addServerButton.Location = new System.Drawing.Point(274, 206);
            this.addServerButton.Name = "addServerButton";
            this.addServerButton.Size = new System.Drawing.Size(128, 23);
            this.addServerButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.addServerButton.TabIndex = 17;
            this.addServerButton.Text = "Cancel";
            this.addServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // SaveButton
            // 
            this.SaveButton.Enabled = false;
            this.SaveButton.Location = new System.Drawing.Point(122, 206);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(128, 23);
            this.SaveButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.SaveButton.TabIndex = 16;
            this.SaveButton.Text = "Save";
            this.SaveButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // metroButton1
            // 
            this.metroButton1.Enabled = false;
            this.metroButton1.Location = new System.Drawing.Point(274, 235);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(128, 23);
            this.metroButton1.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroButton1.TabIndex = 19;
            this.metroButton1.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // metroButton2
            // 
            this.metroButton2.Enabled = false;
            this.metroButton2.Location = new System.Drawing.Point(122, 235);
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Size = new System.Drawing.Size(128, 23);
            this.metroButton2.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroButton2.TabIndex = 18;
            this.metroButton2.Text = "Test Connection";
            this.metroButton2.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 358);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.metroButton2);
            this.Controls.Add(this.addServerButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.addNewServerNameLabel);
            this.Controls.Add(this.ServerPasswordBox);
            this.Controls.Add(this.ServerPortBox);
            this.Controls.Add(this.ServerIPBox);
            this.Controls.Add(this.ServerNameBox);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label addNewServerNameLabel;
        private MetroFramework.Controls.MetroTextBox ServerPasswordBox;
        private MetroFramework.Controls.MetroTextBox ServerPortBox;
        private MetroFramework.Controls.MetroTextBox ServerIPBox;
        private MetroFramework.Controls.MetroTextBox ServerNameBox;
        private MetroFramework.Controls.MetroButton addServerButton;
        private MetroFramework.Controls.MetroButton SaveButton;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroButton metroButton2;
    }
}