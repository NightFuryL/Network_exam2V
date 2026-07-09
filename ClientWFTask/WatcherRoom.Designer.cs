namespace ClientWFTask
{
    partial class WatcherRoom
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            btnExit = new Button();
            lblPlayer_1 = new Label();
            lblPlayer_2 = new Label();
            lblTurn = new Label();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // btnExit
            // 
            btnExit.Location = new Point(12, 12);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(80, 80);
            btnExit.TabIndex = 0;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // lblPlayer_1
            // 
            lblPlayer_1.AutoSize = true;
            lblPlayer_1.Location = new Point(504, 12);
            lblPlayer_1.Name = "lblPlayer_1";
            lblPlayer_1.Size = new Size(63, 20);
            lblPlayer_1.TabIndex = 1;
            lblPlayer_1.Text = "Player_1";
            // 
            // lblPlayer_2
            // 
            lblPlayer_2.AutoSize = true;
            lblPlayer_2.Location = new Point(504, 392);
            lblPlayer_2.Name = "lblPlayer_2";
            lblPlayer_2.Size = new Size(63, 20);
            lblPlayer_2.TabIndex = 2;
            lblPlayer_2.Text = "Player_2";
            // 
            // lblTurn
            // 
            lblTurn.AutoSize = true;
            lblTurn.Location = new Point(98, 415);
            lblTurn.Name = "lblTurn";
            lblTurn.Size = new Size(45, 20);
            lblTurn.TabIndex = 3;
            lblTurn.Text = "Turn: ";
            // 
            // panel1
            // 
            panel1.Location = new Point(98, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(400, 400);
            panel1.TabIndex = 4;
            // 
            // WatcherRoom
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(lblTurn);
            Controls.Add(lblPlayer_2);
            Controls.Add(lblPlayer_1);
            Controls.Add(btnExit);
            Name = "WatcherRoom";
            Text = "WatcherRoom";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnExit;
        private Label lblPlayer_1;
        private Label lblPlayer_2;
        private Label lblTurn;
        private Panel panel1;
    }
}
