namespace ClientControlled
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblIp = new Label();
            lblPassword = new Label();
            SuspendLayout();
            // 
            // lblIp
            // 
            lblIp.AutoSize = true;
            lblIp.Font = new Font("Segoe UI", 10F);
            lblIp.Location = new Point(309, 185);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(130, 19);
            lblIp.TabIndex = 2;
            lblIp.Text = "IP Address: Pending";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 10F);
            lblPassword.Location = new Point(309, 214);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(142, 19);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "Password: Generating";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblPassword);
            Controls.Add(lblIp);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblIp;
        private Label lblPassword;
    }
}
