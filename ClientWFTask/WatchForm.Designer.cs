namespace ClientWFTask
{
    partial class WatchForm
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
            lbGames = new ListBox();
            btnRefresh = new Button();
            btnWatch = new Button();
            SuspendLayout();
            // 
            // lbGames
            // 
            lbGames.FormattingEnabled = true;
            lbGames.Location = new Point(162, 74);
            lbGames.Name = "lbGames";
            lbGames.Size = new Size(150, 104);
            lbGames.TabIndex = 0;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(262, 201);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnWatch
            // 
            btnWatch.Location = new Point(143, 227);
            btnWatch.Name = "btnWatch";
            btnWatch.Size = new Size(94, 29);
            btnWatch.TabIndex = 2;
            btnWatch.Text = "Watch";
            btnWatch.UseVisualStyleBackColor = true;
            btnWatch.Click += btnWatch_Click;
            // 
            // WatchForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnWatch);
            Controls.Add(btnRefresh);
            Controls.Add(lbGames);
            Name = "WatchForm";
            Text = "WatchForm";
            Load += WatchForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox lbGames;
        private Button btnRefresh;
        private Button btnWatch;
    }
}