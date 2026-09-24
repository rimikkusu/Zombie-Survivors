namespace Vampire_Survivors
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlHealthBackground;
        private System.Windows.Forms.Panel pnlHealthFill;
        private System.Windows.Forms.Label lblHealth;
        private System.Windows.Forms.Panel pnlGameOver;
        private System.Windows.Forms.Label lblGameOver;
        private System.Windows.Forms.Button btnRespawn;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Panel pnlXpBackground;
        private System.Windows.Forms.Panel pnlXpFill;
        private System.Windows.Forms.Label lblXp;

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
            pnlHealthBackground = new Panel();
            pnlHealthFill = new Panel();
            lblHealth = new Label();
            pnlGameOver = new Panel();
            lblGameOver = new Label();
            btnRespawn = new Button();
            lblLevel = new Label();
            pnlXpBackground = new Panel();
            pnlXpFill = new Panel();
            lblXp = new Label();
            pnlHealthBackground.SuspendLayout();
            pnlGameOver.SuspendLayout();
            pnlXpBackground.SuspendLayout();
            SuspendLayout();
            //
            // pnlHealthBackground
            //
            pnlHealthBackground.Anchor = AnchorStyles.Top;
            pnlHealthBackground.BackColor = Color.FromArgb(20, 20, 20);
            pnlHealthBackground.Controls.Add(lblHealth);
            pnlHealthBackground.Controls.Add(pnlHealthFill);
            pnlHealthBackground.Location = new Point(691, 12);
            pnlHealthBackground.Name = "pnlHealthBackground";
            pnlHealthBackground.Size = new Size(320, 28);
            pnlHealthBackground.TabIndex = 0;
            //
            // pnlHealthFill
            //
            pnlHealthFill.BackColor = Color.Red;
            pnlHealthFill.Location = new Point(2, 2);
            pnlHealthFill.Name = "pnlHealthFill";
            pnlHealthFill.Size = new Size(316, 24);
            pnlHealthFill.TabIndex = 0;
            //
            // lblHealth
            //
            lblHealth.BackColor = Color.Transparent;
            lblHealth.Dock = DockStyle.Fill;
            lblHealth.Font = new Font("Arial", 10F, FontStyle.Bold);
            lblHealth.ForeColor = Color.White;
            lblHealth.Location = new Point(0, 0);
            lblHealth.Name = "lblHealth";
            lblHealth.Size = new Size(320, 28);
            lblHealth.TabIndex = 1;
            lblHealth.Text = "HP 100 / 100";
            lblHealth.TextAlign = ContentAlignment.MiddleCenter;
            //
            // pnlGameOver
            //
            pnlGameOver.Anchor = AnchorStyles.None;
            pnlGameOver.BackColor = Color.FromArgb(20, 20, 20);
            pnlGameOver.Controls.Add(btnRespawn);
            pnlGameOver.Controls.Add(lblGameOver);
            pnlGameOver.Location = new Point(591, 276);
            pnlGameOver.Name = "pnlGameOver";
            pnlGameOver.Size = new Size(520, 180);
            pnlGameOver.TabIndex = 1;
            pnlGameOver.Visible = false;
            //
            // lblGameOver
            //
            lblGameOver.Dock = DockStyle.Top;
            lblGameOver.Font = new Font("Arial", 36F, FontStyle.Bold);
            lblGameOver.ForeColor = Color.White;
            lblGameOver.Location = new Point(0, 0);
            lblGameOver.Name = "lblGameOver";
            lblGameOver.Size = new Size(520, 105);
            lblGameOver.TabIndex = 0;
            lblGameOver.Text = "GAME OVER";
            lblGameOver.TextAlign = ContentAlignment.MiddleCenter;
            //
            // btnRespawn
            //
            btnRespawn.BackColor = Color.FromArgb(64, 64, 64);
            btnRespawn.Cursor = Cursors.Hand;
            btnRespawn.FlatStyle = FlatStyle.Flat;
            btnRespawn.Font = new Font("Arial", 12F, FontStyle.Bold);
            btnRespawn.ForeColor = Color.White;
            btnRespawn.Location = new Point(180, 120);
            btnRespawn.Name = "btnRespawn";
            btnRespawn.Size = new Size(160, 45);
            btnRespawn.TabIndex = 1;
            btnRespawn.TabStop = false;
            btnRespawn.Text = "RESPAWN";
            btnRespawn.UseVisualStyleBackColor = false;
            btnRespawn.Click += BtnRespawn_Click;
            //
            // lblLevel
            //
            lblLevel.Anchor = AnchorStyles.Bottom;
            lblLevel.Font = new Font("Arial", 11F, FontStyle.Bold);
            lblLevel.ForeColor = Color.White;
            lblLevel.Location = new Point(651, 662);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(400, 20);
            lblLevel.TabIndex = 2;
            lblLevel.Text = "LEVEL 1";
            lblLevel.TextAlign = ContentAlignment.MiddleCenter;
            //
            // pnlXpBackground
            //
            pnlXpBackground.Anchor = AnchorStyles.Bottom;
            pnlXpBackground.BackColor = Color.FromArgb(20, 20, 20);
            pnlXpBackground.Controls.Add(pnlXpFill);
            pnlXpBackground.Location = new Point(651, 684);
            pnlXpBackground.Name = "pnlXpBackground";
            pnlXpBackground.Size = new Size(400, 18);
            pnlXpBackground.TabIndex = 3;
            //
            // pnlXpFill
            //
            pnlXpFill.BackColor = Color.Cyan;
            pnlXpFill.Location = new Point(2, 2);
            pnlXpFill.Name = "pnlXpFill";
            pnlXpFill.Size = new Size(396, 14);
            pnlXpFill.TabIndex = 0;
            //
            // lblXp
            //
            lblXp.Anchor = AnchorStyles.Bottom;
            lblXp.Font = new Font("Arial", 9F, FontStyle.Regular);
            lblXp.ForeColor = Color.White;
            lblXp.Location = new Point(651, 704);
            lblXp.Name = "lblXp";
            lblXp.Size = new Size(400, 20);
            lblXp.TabIndex = 4;
            lblXp.Text = "0 / 50 XP";
            lblXp.TextAlign = ContentAlignment.MiddleCenter;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(1702, 733);
            Controls.Add(lblXp);
            Controls.Add(pnlXpBackground);
            Controls.Add(lblLevel);
            Controls.Add(pnlGameOver);
            Controls.Add(pnlHealthBackground);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            KeyPreview = true;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ArenaSurvivors";
            pnlHealthBackground.ResumeLayout(false);
            pnlGameOver.ResumeLayout(false);
            pnlXpBackground.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
