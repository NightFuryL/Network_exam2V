namespace ClientWFTask
{
    partial class RoomForm
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
            // RoomForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(lblTurn);
            Controls.Add(lblPlayer_2);
            Controls.Add(lblPlayer_1);
            Controls.Add(btnExit);
            Name = "RoomForm";
            Text = "RoomForm";
            Load += RoomForm_Load;
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