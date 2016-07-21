namespace NCodeRCON
{
    partial class AddServerForm
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
            this.addServerNameBox = new MetroFramework.Controls.MetroTextBox();
            this.addServerIPBox = new MetroFramework.Controls.MetroTextBox();
            this.addServerPortBox = new MetroFramework.Controls.MetroTextBox();
            this.addServerPasswordBox = new MetroFramework.Controls.MetroTextBox();
            this.addNewServerNameLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.testConnectionButton = new MetroFramework.Controls.MetroButton();
            this.addServerButton = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // addServerNameBox
            // 
            this.addServerNameBox.Location = new System.Drawing.Point(101, 75);
            this.addServerNameBox.Name = "addServerNameBox";
            this.addServerNameBox.Size = new System.Drawing.Size(280, 23);
            this.addServerNameBox.TabIndex = 0;
            this.addServerNameBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // addServerIPBox
            // 
            this.addServerIPBox.BackColor = System.Drawing.Color.Red;
            this.addServerIPBox.ForeColor = System.Drawing.Color.Coral;
            this.addServerIPBox.Location = new System.Drawing.Point(101, 104);
            this.addServerIPBox.Name = "addServerIPBox";
            this.addServerIPBox.Size = new System.Drawing.Size(280, 23);
            this.addServerIPBox.TabIndex = 1;
            this.addServerIPBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // addServerPortBox
            // 
            this.addServerPortBox.Location = new System.Drawing.Point(101, 133);
            this.addServerPortBox.Name = "addServerPortBox";
            this.addServerPortBox.Size = new System.Drawing.Size(280, 23);
            this.addServerPortBox.TabIndex = 2;
            this.addServerPortBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // addServerPasswordBox
            // 
            this.addServerPasswordBox.Location = new System.Drawing.Point(101, 162);
            this.addServerPasswordBox.Name = "addServerPasswordBox";
            this.addServerPasswordBox.Size = new System.Drawing.Size(280, 23);
            this.addServerPasswordBox.TabIndex = 3;
            this.addServerPasswordBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // addNewServerNameLabel
            // 
            this.addNewServerNameLabel.AutoSize = true;
            this.addNewServerNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.addNewServerNameLabel.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.addNewServerNameLabel.Location = new System.Drawing.Point(57, 75);
            this.addNewServerNameLabel.Name = "addNewServerNameLabel";
            this.addNewServerNameLabel.Size = new System.Drawing.Size(38, 13);
            this.addNewServerNameLabel.TabIndex = 4;
            this.addNewServerNameLabel.Text = "Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label1.Location = new System.Drawing.Point(34, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "IP Address:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label2.Location = new System.Drawing.Point(66, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label3.Location = new System.Drawing.Point(39, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Password:";
            // 
            // testConnectionButton
            // 
            this.testConnectionButton.Enabled = false;
            this.testConnectionButton.Location = new System.Drawing.Point(101, 199);
            this.testConnectionButton.Name = "testConnectionButton";
            this.testConnectionButton.Size = new System.Drawing.Size(128, 23);
            this.testConnectionButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.testConnectionButton.TabIndex = 8;
            this.testConnectionButton.Text = "Test Connection";
            this.testConnectionButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.testConnectionButton.Click += new System.EventHandler(this.testConnectionButton_Click);
            // 
            // addServerButton
            // 
            this.addServerButton.Enabled = false;
            this.addServerButton.Location = new System.Drawing.Point(253, 199);
            this.addServerButton.Name = "addServerButton";
            this.addServerButton.Size = new System.Drawing.Size(128, 23);
            this.addServerButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.addServerButton.TabIndex = 9;
            this.addServerButton.Text = "Add Server";
            this.addServerButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // AddServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 245);
            this.Controls.Add(this.addServerButton);
            this.Controls.Add(this.testConnectionButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.addNewServerNameLabel);
            this.Controls.Add(this.addServerPasswordBox);
            this.Controls.Add(this.addServerPortBox);
            this.Controls.Add(this.addServerIPBox);
            this.Controls.Add(this.addServerNameBox);
            this.Name = "AddServerForm";
            this.Text = "Add Server";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Load += new System.EventHandler(this.AddServerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroTextBox addServerNameBox;
        private MetroFramework.Controls.MetroTextBox addServerIPBox;
        private MetroFramework.Controls.MetroTextBox addServerPortBox;
        private MetroFramework.Controls.MetroTextBox addServerPasswordBox;
        private System.Windows.Forms.Label addNewServerNameLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private MetroFramework.Controls.MetroButton testConnectionButton;
        private MetroFramework.Controls.MetroButton addServerButton;
    }
}