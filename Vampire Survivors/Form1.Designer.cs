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
        private System.Windows.Forms.Panel pnlMainMenu;
        private System.Windows.Forms.Panel pnlMenuContent;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Panel pnlSettingsContent;
        private System.Windows.Forms.Label lblSettingsTitle;
        private System.Windows.Forms.Label lblDisplayMode;
        private System.Windows.Forms.ComboBox cmbDisplayMode;
        private System.Windows.Forms.Button btnApplySettings;
        private System.Windows.Forms.Button btnSettingsBack;
        private System.Windows.Forms.Timer menuAnimationTimer;

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
            this.components = new System.ComponentModel.Container();
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
            pnlMainMenu = new Panel();
            pnlMenuContent = new Panel();
            picLogo = new PictureBox();
            btnPlay = new Button();
            btnSettings = new Button();
            btnQuit = new Button();
            pnlSettings = new Panel();
            pnlSettingsContent = new Panel();
            lblSettingsTitle = new Label();
            lblDisplayMode = new Label();
            cmbDisplayMode = new ComboBox();
            btnApplySettings = new Button();
            btnSettingsBack = new Button();
            menuAnimationTimer = new System.Windows.Forms.Timer(this.components);
            pnlHealthBackground.SuspendLayout();
            pnlGameOver.SuspendLayout();
            pnlXpBackground.SuspendLayout();
            pnlMainMenu.SuspendLayout();
            pnlMenuContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            pnlSettings.SuspendLayout();
            pnlSettingsContent.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHealthBackground
            // 
            pnlHealthBackground.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            pnlHealthBackground.BackColor = Color.FromArgb(20, 20, 20);
            pnlHealthBackground.BorderStyle = BorderStyle.FixedSingle;
            pnlHealthBackground.Controls.Add(pnlHealthFill);
            pnlHealthBackground.Location = new Point(20, 39);
            pnlHealthBackground.Name = "pnlHealthBackground";
            pnlHealthBackground.Size = new Size(300, 24);
            pnlHealthBackground.TabIndex = 1;
            // 
            // pnlHealthFill
            // 
            pnlHealthFill.BackColor = Color.Red;
            pnlHealthFill.Location = new Point(0, 0);
            pnlHealthFill.Name = "pnlHealthFill";
            pnlHealthFill.Size = new Size(298, 22);
            pnlHealthFill.TabIndex = 0;
            // 
            // lblHealth
            // 
            lblHealth.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblHealth.BackColor = Color.Transparent;
            lblHealth.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHealth.ForeColor = Color.White;
            lblHealth.Location = new Point(20, 15);
            lblHealth.Name = "lblHealth";
            lblHealth.Size = new Size(300, 20);
            lblHealth.TabIndex = 0;
            lblHealth.Text = "HP 100 / 100";
            lblHealth.TextAlign = ContentAlignment.MiddleLeft;
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
            pnlGameOver.TabIndex = 2;
            pnlGameOver.Visible = false;
            // 
            // lblGameOver
            // 
            lblGameOver.Dock = DockStyle.Top;
            lblGameOver.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
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
            btnRespawn.BackColor = Color.FromArgb(28, 28, 32);
            btnRespawn.Cursor = Cursors.Hand;
            btnRespawn.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnRespawn.FlatAppearance.BorderSize = 1;
            btnRespawn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnRespawn.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 54);
            btnRespawn.FlatStyle = FlatStyle.Flat;
            btnRespawn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
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
            lblLevel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblLevel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLevel.ForeColor = Color.White;
            lblLevel.Location = new Point(20, 643);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(300, 20);
            lblLevel.TabIndex = 3;
            lblLevel.Text = "LEVEL 1";
            lblLevel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlXpBackground
            // 
            pnlXpBackground.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            pnlXpBackground.BackColor = Color.FromArgb(20, 20, 20);
            pnlXpBackground.BorderStyle = BorderStyle.FixedSingle;
            pnlXpBackground.Controls.Add(pnlXpFill);
            pnlXpBackground.Location = new Point(20, 691);
            pnlXpBackground.Name = "pnlXpBackground";
            pnlXpBackground.Size = new Size(300, 22);
            pnlXpBackground.TabIndex = 4;
            // 
            // pnlXpFill
            // 
            pnlXpFill.BackColor = Color.Cyan;
            pnlXpFill.Location = new Point(0, 0);
            pnlXpFill.Name = "pnlXpFill";
            pnlXpFill.Size = new Size(298, 20);
            pnlXpFill.TabIndex = 0;
            // 
            // lblXp
            // 
            lblXp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblXp.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblXp.ForeColor = Color.White;
            lblXp.Location = new Point(20, 667);
            lblXp.Name = "lblXp";
            lblXp.Size = new Size(300, 20);
            lblXp.TabIndex = 5;
            lblXp.Text = "0 / 50 XP";
            lblXp.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlMainMenu
            // 
            pnlMainMenu.BackColor = Color.FromArgb(12, 12, 12);
            pnlMainMenu.Controls.Add(pnlMenuContent);
            pnlMainMenu.Dock = DockStyle.Fill;
            pnlMainMenu.Location = new Point(0, 0);
            pnlMainMenu.Name = "pnlMainMenu";
            pnlMainMenu.Size = new Size(1702, 733);
            pnlMainMenu.TabIndex = 6;
            // 
            // pnlMenuContent
            // 
            pnlMenuContent.BackColor = Color.Transparent;
            pnlMenuContent.Controls.Add(picLogo);
            pnlMenuContent.Controls.Add(btnPlay);
            pnlMenuContent.Controls.Add(btnSettings);
            pnlMenuContent.Controls.Add(btnQuit);
            pnlMenuContent.Location = new Point(601, 100);
            pnlMenuContent.Name = "pnlMenuContent";
            pnlMenuContent.Size = new Size(500, 500);
            pnlMenuContent.TabIndex = 0;
            // 
            // picLogo
            // 
            picLogo.BackColor = Color.Transparent;
            picLogo.Image = global::Vampire_Survivors.Properties.Resources.Logo;
            picLogo.Location = new Point(10, 20);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(480, 240);
            picLogo.SizeMode = PictureBoxSizeMode.CenterImage;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            // 
            // btnPlay
            // 
            btnPlay.BackColor = Color.FromArgb(28, 28, 32);
            btnPlay.Cursor = Cursors.Hand;
            btnPlay.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnPlay.FlatAppearance.BorderSize = 1;
            btnPlay.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnPlay.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 54);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPlay.ForeColor = Color.FromArgb(235, 235, 240);
            btnPlay.Location = new Point(120, 280);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(260, 52);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "PLAY";
            btnPlay.UseVisualStyleBackColor = false;
            btnPlay.Click += BtnPlay_Click;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = Color.FromArgb(28, 28, 32);
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnSettings.FlatAppearance.BorderSize = 1;
            btnSettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 54);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSettings.ForeColor = Color.FromArgb(235, 235, 240);
            btnSettings.Location = new Point(120, 346);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(260, 52);
            btnSettings.TabIndex = 1;
            btnSettings.Text = "SETTINGS";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += BtnSettings_Click;
            // 
            // btnQuit
            // 
            btnQuit.BackColor = Color.FromArgb(28, 28, 32);
            btnQuit.Cursor = Cursors.Hand;
            btnQuit.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnQuit.FlatAppearance.BorderSize = 1;
            btnQuit.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnQuit.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 54);
            btnQuit.FlatStyle = FlatStyle.Flat;
            btnQuit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnQuit.ForeColor = Color.FromArgb(235, 235, 240);
            btnQuit.Location = new Point(120, 412);
            btnQuit.Name = "btnQuit";
            btnQuit.Size = new Size(260, 52);
            btnQuit.TabIndex = 2;
            btnQuit.Text = "QUIT";
            btnQuit.UseVisualStyleBackColor = false;
            btnQuit.Click += BtnQuit_Click;
            // 
            // pnlSettings
            // 
            pnlSettings.BackColor = Color.FromArgb(12, 12, 12);
            pnlSettings.Controls.Add(pnlSettingsContent);
            pnlSettings.Dock = DockStyle.Fill;
            pnlSettings.Location = new Point(0, 0);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.Size = new Size(1702, 733);
            pnlSettings.TabIndex = 7;
            pnlSettings.Visible = false;
            // 
            // pnlSettingsContent
            // 
            pnlSettingsContent.BackColor = Color.Transparent;
            pnlSettingsContent.Controls.Add(lblSettingsTitle);
            pnlSettingsContent.Controls.Add(lblDisplayMode);
            pnlSettingsContent.Controls.Add(cmbDisplayMode);
            pnlSettingsContent.Controls.Add(btnApplySettings);
            pnlSettingsContent.Controls.Add(btnSettingsBack);
            pnlSettingsContent.Location = new Point(601, 150);
            pnlSettingsContent.Name = "pnlSettingsContent";
            pnlSettingsContent.Size = new Size(500, 360);
            pnlSettingsContent.TabIndex = 0;
            // 
            // lblSettingsTitle
            // 
            lblSettingsTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblSettingsTitle.ForeColor = Color.FromArgb(245, 245, 250);
            lblSettingsTitle.Location = new Point(50, 15);
            lblSettingsTitle.Name = "lblSettingsTitle";
            lblSettingsTitle.Size = new Size(400, 55);
            lblSettingsTitle.TabIndex = 10;
            lblSettingsTitle.Text = "SETTINGS";
            lblSettingsTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDisplayMode
            // 
            lblDisplayMode.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDisplayMode.ForeColor = Color.FromArgb(170, 175, 185);
            lblDisplayMode.Location = new Point(120, 85);
            lblDisplayMode.Name = "lblDisplayMode";
            lblDisplayMode.Size = new Size(260, 24);
            lblDisplayMode.TabIndex = 11;
            lblDisplayMode.Text = "DISPLAY MODE";
            lblDisplayMode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cmbDisplayMode
            // 
            cmbDisplayMode.BackColor = Color.FromArgb(28, 28, 32);
            cmbDisplayMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDisplayMode.FlatStyle = FlatStyle.Flat;
            cmbDisplayMode.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            cmbDisplayMode.ForeColor = Color.FromArgb(235, 235, 240);
            cmbDisplayMode.FormattingEnabled = true;
            cmbDisplayMode.Items.AddRange(new object[] { "Windowed", "Windowed Maximized", "Fullscreen" });
            cmbDisplayMode.Location = new Point(120, 115);
            cmbDisplayMode.Name = "cmbDisplayMode";
            cmbDisplayMode.Size = new Size(260, 33);
            cmbDisplayMode.TabIndex = 0;
            // 
            // btnApplySettings
            // 
            btnApplySettings.BackColor = Color.FromArgb(28, 28, 32);
            btnApplySettings.Cursor = Cursors.Hand;
            btnApplySettings.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnApplySettings.FlatAppearance.BorderSize = 1;
            btnApplySettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnApplySettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 54);
            btnApplySettings.FlatStyle = FlatStyle.Flat;
            btnApplySettings.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnApplySettings.ForeColor = Color.FromArgb(235, 235, 240);
            btnApplySettings.Location = new Point(120, 180);
            btnApplySettings.Name = "btnApplySettings";
            btnApplySettings.Size = new Size(260, 52);
            btnApplySettings.TabIndex = 1;
            btnApplySettings.Text = "APPLY";
            btnApplySettings.UseVisualStyleBackColor = false;
            btnApplySettings.Click += BtnApplySettings_Click;
            // 
            // btnSettingsBack
            // 
            btnSettingsBack.BackColor = Color.FromArgb(28, 28, 32);
            btnSettingsBack.Cursor = Cursors.Hand;
            btnSettingsBack.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnSettingsBack.FlatAppearance.BorderSize = 1;
            btnSettingsBack.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnSettingsBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 54);
            btnSettingsBack.FlatStyle = FlatStyle.Flat;
            btnSettingsBack.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSettingsBack.ForeColor = Color.FromArgb(235, 235, 240);
            btnSettingsBack.Location = new Point(120, 246);
            btnSettingsBack.Name = "btnSettingsBack";
            btnSettingsBack.Size = new Size(260, 52);
            btnSettingsBack.TabIndex = 2;
            btnSettingsBack.Text = "BACK";
            btnSettingsBack.UseVisualStyleBackColor = false;
            btnSettingsBack.Click += BtnSettingsBack_Click;
            // 
            // menuAnimationTimer
            // 
            menuAnimationTimer.Interval = 16;
            menuAnimationTimer.Tick += MenuAnimationTimer_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(1702, 733);
            Controls.Add(lblHealth);
            Controls.Add(lblXp);
            Controls.Add(pnlXpBackground);
            Controls.Add(lblLevel);
            Controls.Add(pnlGameOver);
            Controls.Add(pnlHealthBackground);
            Controls.Add(pnlSettings);
            Controls.Add(pnlMainMenu);
            FormBorderStyle = FormBorderStyle.Sizable;
            KeyPreview = true;
            MaximizeBox = true;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ArenaSurvivors";
            WindowState = FormWindowState.Maximized;
            pnlHealthBackground.ResumeLayout(false);
            pnlGameOver.ResumeLayout(false);
            pnlXpBackground.ResumeLayout(false);
            pnlMainMenu.ResumeLayout(false);
            pnlMenuContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            pnlSettings.ResumeLayout(false);
            pnlSettingsContent.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
