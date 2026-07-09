namespace ClientWFTask
{
    partial class RegisterForm
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
            btnCreate = new Button();
            btnWatch = new Button();
            label1 = new Label();
            txtLogin = new TextBox();
            SuspendLayout();
            //
            // btnCreate
            //
            btnCreate.Location = new Point(346, 201);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(120, 34);
            btnCreate.TabIndex = 0;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            //
            // btnWatch
            //
            btnWatch.Location = new Point(346, 250);
            btnWatch.Name = "btnWatch";
            btnWatch.Size = new Size(120, 34);
            btnWatch.TabIndex = 5;
            btnWatch.Text = "Watch";
            btnWatch.UseVisualStyleBackColor = true;
            btnWatch.Click += btnWatch_Click;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(301, 110);
            label1.Name = "label1";
            label1.Size = new Size(49, 20);
            label1.TabIndex = 1;
            label1.Text = "Login:";
            // 
            // txtLogin
            // 
            txtLogin.Location = new Point(409, 103);
            txtLogin.Name = "txtLogin";
            txtLogin.Size = new Size(125, 27);
            txtLogin.TabIndex = 3;
            // 
            // RegisterForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnWatch);
            Controls.Add(txtLogin);
            Controls.Add(label1);
            Controls.Add(btnCreate);
            Name = "RegisterForm";
            Text = "RegisterForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCreate;
        private Button btnWatch;
        private Label label1;
        private TextBox txtLogin;
    }
}
