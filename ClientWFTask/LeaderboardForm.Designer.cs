namespace ClientWFTask
{
    partial class LeaderboardForm
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
            lbLeaderboard = new ListBox();
            btnRefresh = new Button();
            SuspendLayout();
            // 
            // lbLeaderboard
            // 
            lbLeaderboard.FormattingEnabled = true;
            lbLeaderboard.Location = new Point(206, 118);
            lbLeaderboard.Name = "lbLeaderboard";
            lbLeaderboard.Size = new Size(150, 104);
            lbLeaderboard.TabIndex = 0;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(463, 119);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // LeaderboardForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnRefresh);
            Controls.Add(lbLeaderboard);
            Name = "LeaderboardForm";
            Text = "LeaderboardForm";
            Load += LeaderboardForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox lbLeaderboard;
        private Button btnRefresh;
    }
}