namespace ClientWFTask
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
            btnPlay = new Button();
            btnWatch = new Button();
            btnLeaderBoard = new Button();
            btnLogout = new Button();
            lblNickname = new Label();
            lblRating = new Label();
            lblGuid = new Label();
            SuspendLayout();
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(403, 78);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(94, 29);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "Play";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // btnWatch
            // 
            btnWatch.Location = new Point(296, 141);
            btnWatch.Name = "btnWatch";
            btnWatch.Size = new Size(94, 29);
            btnWatch.TabIndex = 2;
            btnWatch.Text = "Watch";
            btnWatch.UseVisualStyleBackColor = true;
            btnWatch.Click += btnWatch_Click;
            // 
            // btnLeaderBoard
            // 
            btnLeaderBoard.Location = new Point(426, 141);
            btnLeaderBoard.Name = "btnLeaderBoard";
            btnLeaderBoard.Size = new Size(94, 29);
            btnLeaderBoard.TabIndex = 3;
            btnLeaderBoard.Text = "LeaderBoard";
            btnLeaderBoard.UseVisualStyleBackColor = true;
            btnLeaderBoard.Click += btnLeaderBoard_Click;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(353, 211);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(94, 29);
            btnLogout.TabIndex = 1;
            btnLogout.Text = " Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // lblNickname
            // 
            lblNickname.AutoSize = true;
            lblNickname.Location = new Point(137, 103);
            lblNickname.Name = "lblNickname";
            lblNickname.Size = new Size(78, 20);
            lblNickname.TabIndex = 4;
            lblNickname.Text = "Nickname:";
            // 
            // lblRating
            // 
            lblRating.AutoSize = true;
            lblRating.Location = new Point(137, 132);
            lblRating.Name = "lblRating";
            lblRating.Size = new Size(59, 20);
            lblRating.TabIndex = 5;
            lblRating.Text = "Rating :";
            // 
            // lblGuid
            // 
            lblGuid.AutoSize = true;
            lblGuid.Location = new Point(137, 162);
            lblGuid.Name = "lblGuid";
            lblGuid.Size = new Size(47, 20);
            lblGuid.TabIndex = 6;
            lblGuid.Text = "Guid: ";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblGuid);
            Controls.Add(lblRating);
            Controls.Add(lblNickname);
            Controls.Add(btnLeaderBoard);
            Controls.Add(btnWatch);
            Controls.Add(btnLogout);
            Controls.Add(btnPlay);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnPlay;
        private Button btnWatch;
        private Button btnLeaderBoard;
        private Button btnLogout;
        private Label lblNickname;
        private Label lblRating;
        private Label lblGuid;
    }
}