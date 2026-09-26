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
        private System.Windows.Forms.Panel pnlLevelUp;
        private System.Windows.Forms.Label lblLevelUpTitle;
        private System.Windows.Forms.Button btnUpgrade1;
        private System.Windows.Forms.Button btnUpgrade2;
        private System.Windows.Forms.Button btnUpgrade3;
        private System.Windows.Forms.Label lblWave;
        private System.Windows.Forms.Panel pnlActiveAbility;
        private System.Windows.Forms.Label lblActiveAbility;
        private System.Windows.Forms.Panel pnlDebug;
        private System.Windows.Forms.Label lblDebugTitle;
        private System.Windows.Forms.CheckBox chkInfiniteHealth;
        private System.Windows.Forms.CheckBox chkInfiniteXp;
        private System.Windows.Forms.CheckBox chkNoAbilityCooldowns;
        private System.Windows.Forms.Button btnDebugAddXp;
        private System.Windows.Forms.Button btnDebugLevelUp;
        private System.Windows.Forms.Button btnDebugNextWave;
        private System.Windows.Forms.Button btnDebugKillEnemies;
        private System.Windows.Forms.Button btnDebugHeal;
        private System.Windows.Forms.Label lblDebugInfo;
        private System.Windows.Forms.Label lblUpgradesTitle;
        private System.Windows.Forms.ListBox lstUpgrades;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Panel pnlPause;
        private System.Windows.Forms.Label lblPaused;
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.Button btnPauseSettings;
        private System.Windows.Forms.Button btnPauseMainMenu;

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
            pnlLevelUp = new Panel();
            lblLevelUpTitle = new Label();
            btnUpgrade1 = new Button();
            btnUpgrade2 = new Button();
            btnUpgrade3 = new Button();
            lblWave = new Label();
            pnlActiveAbility = new Panel();
            lblActiveAbility = new Label();
            pnlDebug = new Panel();
            lblDebugTitle = new Label();
            chkInfiniteHealth = new CheckBox();
            chkInfiniteXp = new CheckBox();
            chkNoAbilityCooldowns = new CheckBox();
            btnDebugAddXp = new Button();
            btnDebugLevelUp = new Button();
            btnDebugNextWave = new Button();
            btnDebugKillEnemies = new Button();
            btnDebugHeal = new Button();
            lblDebugInfo = new Label();
            lblUpgradesTitle = new Label();
            lstUpgrades = new ListBox();
            btnPause = new Button();
            pnlPause = new Panel();
            lblPaused = new Label();
            btnResume = new Button();
            btnPauseSettings = new Button();
            btnPauseMainMenu = new Button();
            pnlHealthBackground.SuspendLayout();
            pnlGameOver.SuspendLayout();
            pnlXpBackground.SuspendLayout();
            pnlMainMenu.SuspendLayout();
            pnlMenuContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            pnlSettings.SuspendLayout();
            pnlSettingsContent.SuspendLayout();
            pnlLevelUp.SuspendLayout();
            pnlPause.SuspendLayout();
            pnlDebug.SuspendLayout();
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
            picLogo.Location = new Point(50, 30);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(400, 200);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
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
            btnPlay.ForeColor = Color.White;
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
            btnSettings.ForeColor = Color.White;
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
            btnQuit.ForeColor = Color.White;
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
            btnApplySettings.ForeColor = Color.White;
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
            btnSettingsBack.ForeColor = Color.White;
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
            menuAnimationTimer.Interval = 20;
            menuAnimationTimer.Tick += MenuAnimationTimer_Tick;
            // 
            // pnlLevelUp
            //
            pnlLevelUp.Anchor = AnchorStyles.None;
            pnlLevelUp.BackColor = Color.FromArgb(20, 20, 26);
            pnlLevelUp.BorderStyle = BorderStyle.FixedSingle;
            pnlLevelUp.Controls.Add(lblLevelUpTitle);
            pnlLevelUp.Controls.Add(btnUpgrade1);
            pnlLevelUp.Controls.Add(btnUpgrade2);
            pnlLevelUp.Controls.Add(btnUpgrade3);
            pnlLevelUp.Location = new Point(581, 180);
            pnlLevelUp.Name = "pnlLevelUp";
            pnlLevelUp.Size = new Size(540, 365);
            pnlLevelUp.TabIndex = 8;
            pnlLevelUp.Visible = false;
            //
            // lblLevelUpTitle
            //
            lblLevelUpTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblLevelUpTitle.ForeColor = Color.White;
            lblLevelUpTitle.Location = new Point(20, 15);
            lblLevelUpTitle.Name = "lblLevelUpTitle";
            lblLevelUpTitle.Size = new Size(500, 40);
            lblLevelUpTitle.TabIndex = 0;
            lblLevelUpTitle.Text = "LEVEL UP!";
            lblLevelUpTitle.TextAlign = ContentAlignment.MiddleCenter;
            //
            // btnUpgrade1
            //
            btnUpgrade1.BackColor = Color.FromArgb(28, 28, 34);
            btnUpgrade1.Cursor = Cursors.Hand;
            btnUpgrade1.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 80);
            btnUpgrade1.FlatAppearance.BorderSize = 1;
            btnUpgrade1.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnUpgrade1.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 56);
            btnUpgrade1.FlatStyle = FlatStyle.Flat;
            btnUpgrade1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnUpgrade1.ForeColor = Color.White;
            btnUpgrade1.Location = new Point(40, 70);
            btnUpgrade1.Name = "btnUpgrade1";
            btnUpgrade1.Size = new Size(460, 80);
            btnUpgrade1.TabIndex = 1;
            btnUpgrade1.Text = "UPGRADE 1";
            btnUpgrade1.UseVisualStyleBackColor = false;
            btnUpgrade1.Click += BtnUpgrade1_Click;
            //
            // btnUpgrade2
            //
            btnUpgrade2.BackColor = Color.FromArgb(28, 28, 34);
            btnUpgrade2.Cursor = Cursors.Hand;
            btnUpgrade2.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 80);
            btnUpgrade2.FlatAppearance.BorderSize = 1;
            btnUpgrade2.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnUpgrade2.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 56);
            btnUpgrade2.FlatStyle = FlatStyle.Flat;
            btnUpgrade2.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnUpgrade2.ForeColor = Color.White;
            btnUpgrade2.Location = new Point(40, 165);
            btnUpgrade2.Name = "btnUpgrade2";
            btnUpgrade2.Size = new Size(460, 80);
            btnUpgrade2.TabIndex = 2;
            btnUpgrade2.Text = "UPGRADE 2";
            btnUpgrade2.UseVisualStyleBackColor = false;
            btnUpgrade2.Click += BtnUpgrade2_Click;
            //
            // btnUpgrade3
            //
            btnUpgrade3.BackColor = Color.FromArgb(28, 28, 34);
            btnUpgrade3.Cursor = Cursors.Hand;
            btnUpgrade3.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 80);
            btnUpgrade3.FlatAppearance.BorderSize = 1;
            btnUpgrade3.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 24);
            btnUpgrade3.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 46, 56);
            btnUpgrade3.FlatStyle = FlatStyle.Flat;
            btnUpgrade3.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnUpgrade3.ForeColor = Color.White;
            btnUpgrade3.Location = new Point(40, 260);
            btnUpgrade3.Name = "btnUpgrade3";
            btnUpgrade3.Size = new Size(460, 80);
            btnUpgrade3.TabIndex = 3;
            btnUpgrade3.Text = "UPGRADE 3";
            btnUpgrade3.UseVisualStyleBackColor = false;
            btnUpgrade3.Click += BtnUpgrade3_Click;
            //
            // lblWave
            //
            lblWave.Anchor = AnchorStyles.Top;
            lblWave.BackColor = Color.FromArgb(170, 12, 12, 12);
            lblWave.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblWave.ForeColor = Color.White;
            lblWave.Location = new Point(741, 12);
            lblWave.Name = "lblWave";
            lblWave.Size = new Size(220, 48);
            lblWave.TabIndex = 9;
            lblWave.Text = "WAVE 1\r\nENEMIES: 3";
            lblWave.TextAlign = ContentAlignment.MiddleCenter;
            //
            // pnlActiveAbility
            //
            pnlActiveAbility.BackColor = Color.FromArgb(205, 12, 12, 12);
            pnlActiveAbility.BorderStyle = BorderStyle.FixedSingle;
            pnlActiveAbility.Controls.Add(lblActiveAbility);
            pnlActiveAbility.Location = new Point(631, 64);
            pnlActiveAbility.Name = "pnlActiveAbility";
            pnlActiveAbility.Size = new Size(440, 120);
            pnlActiveAbility.TabIndex = 14;
            pnlActiveAbility.Visible = false;
            //
            // lblActiveAbility
            //
            lblActiveAbility.BackColor = Color.Transparent;
            lblActiveAbility.Dock = DockStyle.Fill;
            lblActiveAbility.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblActiveAbility.ForeColor = Color.FromArgb(145, 255, 160);
            lblActiveAbility.Name = "lblActiveAbility";
            lblActiveAbility.TabIndex = 0;
            lblActiveAbility.Text = "";
            lblActiveAbility.TextAlign = ContentAlignment.MiddleCenter;
            //
            // pnlDebug
            //
            pnlDebug.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlDebug.BackColor = Color.FromArgb(220, 18, 18, 22);
            pnlDebug.BorderStyle = BorderStyle.FixedSingle;
            pnlDebug.Controls.Add(lblDebugInfo);
            pnlDebug.Controls.Add(btnDebugHeal);
            pnlDebug.Controls.Add(btnDebugKillEnemies);
            pnlDebug.Controls.Add(btnDebugNextWave);
            pnlDebug.Controls.Add(btnDebugLevelUp);
            pnlDebug.Controls.Add(btnDebugAddXp);
            pnlDebug.Controls.Add(chkNoAbilityCooldowns);
            pnlDebug.Controls.Add(chkInfiniteXp);
            pnlDebug.Controls.Add(chkInfiniteHealth);
            pnlDebug.Controls.Add(lblDebugTitle);
            pnlDebug.Location = new Point(1362, 212);
            pnlDebug.Name = "pnlDebug";
            pnlDebug.Size = new Size(320, 408);
            pnlDebug.TabIndex = 15;
            pnlDebug.Visible = false;
            //
            // lblDebugTitle
            //
            lblDebugTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblDebugTitle.ForeColor = Color.White;
            lblDebugTitle.Location = new Point(12, 8);
            lblDebugTitle.Name = "lblDebugTitle";
            lblDebugTitle.Size = new Size(294, 26);
            lblDebugTitle.TabIndex = 0;
            lblDebugTitle.Text = "DEBUG MENU (F3)";
            lblDebugTitle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // chkInfiniteHealth
            //
            chkInfiniteHealth.BackColor = Color.Transparent;
            chkInfiniteHealth.ForeColor = Color.White;
            chkInfiniteHealth.Location = new Point(12, 38);
            chkInfiniteHealth.Name = "chkInfiniteHealth";
            chkInfiniteHealth.Size = new Size(294, 22);
            chkInfiniteHealth.TabIndex = 1;
            chkInfiniteHealth.Text = "Infinite Health";
            chkInfiniteHealth.UseVisualStyleBackColor = false;
            chkInfiniteHealth.CheckedChanged += ChkInfiniteHealth_CheckedChanged;
            //
            // chkInfiniteXp
            //
            chkInfiniteXp.BackColor = Color.Transparent;
            chkInfiniteXp.ForeColor = Color.White;
            chkInfiniteXp.Location = new Point(12, 62);
            chkInfiniteXp.Name = "chkInfiniteXp";
            chkInfiniteXp.Size = new Size(294, 22);
            chkInfiniteXp.TabIndex = 2;
            chkInfiniteXp.Text = "Infinite XP after each level-up";
            chkInfiniteXp.UseVisualStyleBackColor = false;
            //
            // chkNoAbilityCooldowns
            //
            chkNoAbilityCooldowns.BackColor = Color.Transparent;
            chkNoAbilityCooldowns.ForeColor = Color.White;
            chkNoAbilityCooldowns.Location = new Point(12, 86);
            chkNoAbilityCooldowns.Name = "chkNoAbilityCooldowns";
            chkNoAbilityCooldowns.Size = new Size(294, 22);
            chkNoAbilityCooldowns.TabIndex = 3;
            chkNoAbilityCooldowns.Text = "No Ability Cooldowns";
            chkNoAbilityCooldowns.UseVisualStyleBackColor = false;
            //
            // btnDebugAddXp
            //
            btnDebugAddXp.BackColor = Color.FromArgb(40, 40, 46);
            btnDebugAddXp.FlatStyle = FlatStyle.Flat;
            btnDebugAddXp.ForeColor = Color.White;
            btnDebugAddXp.Location = new Point(12, 116);
            btnDebugAddXp.Name = "btnDebugAddXp";
            btnDebugAddXp.Size = new Size(294, 32);
            btnDebugAddXp.TabIndex = 4;
            btnDebugAddXp.Text = "+100 XP";
            btnDebugAddXp.UseVisualStyleBackColor = false;
            btnDebugAddXp.Click += BtnDebugAddXp_Click;
            //
            // btnDebugLevelUp
            //
            btnDebugLevelUp.BackColor = Color.FromArgb(40, 40, 46);
            btnDebugLevelUp.FlatStyle = FlatStyle.Flat;
            btnDebugLevelUp.ForeColor = Color.White;
            btnDebugLevelUp.Location = new Point(12, 152);
            btnDebugLevelUp.Name = "btnDebugLevelUp";
            btnDebugLevelUp.Size = new Size(294, 32);
            btnDebugLevelUp.TabIndex = 5;
            btnDebugLevelUp.Text = "Level Up";
            btnDebugLevelUp.UseVisualStyleBackColor = false;
            btnDebugLevelUp.Click += BtnDebugLevelUp_Click;
            //
            // btnDebugNextWave
            //
            btnDebugNextWave.BackColor = Color.FromArgb(40, 40, 46);
            btnDebugNextWave.FlatStyle = FlatStyle.Flat;
            btnDebugNextWave.ForeColor = Color.White;
            btnDebugNextWave.Location = new Point(12, 188);
            btnDebugNextWave.Name = "btnDebugNextWave";
            btnDebugNextWave.Size = new Size(294, 32);
            btnDebugNextWave.TabIndex = 6;
            btnDebugNextWave.Text = "Next Wave";
            btnDebugNextWave.UseVisualStyleBackColor = false;
            btnDebugNextWave.Click += BtnDebugNextWave_Click;
            //
            // btnDebugKillEnemies
            //
            btnDebugKillEnemies.BackColor = Color.FromArgb(40, 40, 46);
            btnDebugKillEnemies.FlatStyle = FlatStyle.Flat;
            btnDebugKillEnemies.ForeColor = Color.White;
            btnDebugKillEnemies.Location = new Point(12, 224);
            btnDebugKillEnemies.Name = "btnDebugKillEnemies";
            btnDebugKillEnemies.Size = new Size(294, 32);
            btnDebugKillEnemies.TabIndex = 7;
            btnDebugKillEnemies.Text = "Kill Enemies (No Rewards)";
            btnDebugKillEnemies.UseVisualStyleBackColor = false;
            btnDebugKillEnemies.Click += BtnDebugKillEnemies_Click;
            //
            // btnDebugHeal
            //
            btnDebugHeal.BackColor = Color.FromArgb(40, 40, 46);
            btnDebugHeal.FlatStyle = FlatStyle.Flat;
            btnDebugHeal.ForeColor = Color.White;
            btnDebugHeal.Location = new Point(12, 260);
            btnDebugHeal.Name = "btnDebugHeal";
            btnDebugHeal.Size = new Size(294, 32);
            btnDebugHeal.TabIndex = 8;
            btnDebugHeal.Text = "Heal";
            btnDebugHeal.UseVisualStyleBackColor = false;
            btnDebugHeal.Click += BtnDebugHeal_Click;
            //
            // lblDebugInfo
            //
            lblDebugInfo.ForeColor = Color.Gainsboro;
            lblDebugInfo.Location = new Point(12, 300);
            lblDebugInfo.Name = "lblDebugInfo";
            lblDebugInfo.Size = new Size(294, 94);
            lblDebugInfo.TabIndex = 9;
            lblDebugInfo.Text = "LEVEL: 1\r\nXP: 0 / 20\r\nWAVE: 1\r\nHP: 100 / 100\r\nENEMIES: 0";
            lblDebugInfo.TextAlign = ContentAlignment.TopLeft;
            //
            // lblUpgradesTitle
            //
            lblUpgradesTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUpgradesTitle.BackColor = Color.FromArgb(170, 12, 12, 12);
            lblUpgradesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUpgradesTitle.ForeColor = Color.White;
            lblUpgradesTitle.Location = new Point(1452, 12);
            lblUpgradesTitle.Name = "lblUpgradesTitle";
            lblUpgradesTitle.Size = new Size(230, 26);
            lblUpgradesTitle.TabIndex = 10;
            lblUpgradesTitle.Text = "UPGRADES";
            lblUpgradesTitle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lstUpgrades
            //
            lstUpgrades.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lstUpgrades.BackColor = Color.FromArgb(18, 18, 22);
            lstUpgrades.BorderStyle = BorderStyle.FixedSingle;
            lstUpgrades.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lstUpgrades.ForeColor = Color.White;
            lstUpgrades.FormattingEnabled = true;
            lstUpgrades.HorizontalScrollbar = true;
            lstUpgrades.ItemHeight = 20;
            lstUpgrades.Location = new Point(1452, 38);
            lstUpgrades.Name = "lstUpgrades";
            lstUpgrades.SelectionMode = SelectionMode.None;
            lstUpgrades.Size = new Size(230, 162);
            lstUpgrades.TabIndex = 11;
            //
            // btnPause
            //
            btnPause.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnPause.BackColor = Color.FromArgb(28, 28, 32);
            btnPause.Cursor = Cursors.Hand;
            btnPause.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnPause.FlatStyle = FlatStyle.Flat;
            btnPause.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnPause.ForeColor = Color.White;
            btnPause.Location = new Point(335, 16);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(96, 38);
            btnPause.TabIndex = 12;
            btnPause.Text = "PAUSE";
            btnPause.UseVisualStyleBackColor = false;
            btnPause.Click += BtnPause_Click;
            //
            // pnlPause
            //
            pnlPause.Anchor = AnchorStyles.None;
            pnlPause.BackColor = Color.FromArgb(20, 20, 26);
            pnlPause.BorderStyle = BorderStyle.FixedSingle;
            pnlPause.Controls.Add(lblPaused);
            pnlPause.Controls.Add(btnResume);
            pnlPause.Controls.Add(btnPauseSettings);
            pnlPause.Controls.Add(btnPauseMainMenu);
            pnlPause.Location = new Point(601, 175);
            pnlPause.Name = "pnlPause";
            pnlPause.Size = new Size(500, 380);
            pnlPause.TabIndex = 13;
            pnlPause.Visible = false;
            //
            // lblPaused
            //
            lblPaused.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblPaused.ForeColor = Color.White;
            lblPaused.Location = new Point(40, 20);
            lblPaused.Name = "lblPaused";
            lblPaused.Size = new Size(420, 60);
            lblPaused.TabIndex = 0;
            lblPaused.Text = "PAUSED";
            lblPaused.TextAlign = ContentAlignment.MiddleCenter;
            //
            // btnResume
            //
            btnResume.BackColor = Color.FromArgb(28, 28, 32);
            btnResume.Cursor = Cursors.Hand;
            btnResume.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnResume.FlatStyle = FlatStyle.Flat;
            btnResume.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnResume.ForeColor = Color.White;
            btnResume.Location = new Point(100, 105);
            btnResume.Name = "btnResume";
            btnResume.Size = new Size(300, 56);
            btnResume.TabIndex = 1;
            btnResume.Text = "RESUME";
            btnResume.UseVisualStyleBackColor = false;
            btnResume.Click += BtnResume_Click;
            //
            // btnPauseSettings
            //
            btnPauseSettings.BackColor = Color.FromArgb(28, 28, 32);
            btnPauseSettings.Cursor = Cursors.Hand;
            btnPauseSettings.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnPauseSettings.FlatStyle = FlatStyle.Flat;
            btnPauseSettings.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPauseSettings.ForeColor = Color.White;
            btnPauseSettings.Location = new Point(100, 175);
            btnPauseSettings.Name = "btnPauseSettings";
            btnPauseSettings.Size = new Size(300, 56);
            btnPauseSettings.TabIndex = 2;
            btnPauseSettings.Text = "SETTINGS";
            btnPauseSettings.UseVisualStyleBackColor = false;
            btnPauseSettings.Click += BtnPauseSettings_Click;
            //
            // btnPauseMainMenu
            //
            btnPauseMainMenu.BackColor = Color.FromArgb(28, 28, 32);
            btnPauseMainMenu.Cursor = Cursors.Hand;
            btnPauseMainMenu.FlatAppearance.BorderColor = Color.FromArgb(65, 65, 75);
            btnPauseMainMenu.FlatStyle = FlatStyle.Flat;
            btnPauseMainMenu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPauseMainMenu.ForeColor = Color.White;
            btnPauseMainMenu.Location = new Point(100, 245);
            btnPauseMainMenu.Name = "btnPauseMainMenu";
            btnPauseMainMenu.Size = new Size(300, 56);
            btnPauseMainMenu.TabIndex = 3;
            btnPauseMainMenu.Text = "MAIN MENU";
            btnPauseMainMenu.UseVisualStyleBackColor = false;
            btnPauseMainMenu.Click += BtnPauseMainMenu_Click;
            //
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(1702, 733);
            Controls.Add(btnPause);
            Controls.Add(lstUpgrades);
            Controls.Add(lblUpgradesTitle);
            Controls.Add(lblWave);
            Controls.Add(pnlActiveAbility);
            Controls.Add(pnlLevelUp);
            Controls.Add(pnlPause);
            Controls.Add(lblHealth);
            Controls.Add(lblXp);
            Controls.Add(pnlXpBackground);
            Controls.Add(lblLevel);
            Controls.Add(pnlGameOver);
            Controls.Add(pnlHealthBackground);
            Controls.Add(pnlSettings);
            Controls.Add(pnlMainMenu);
            Controls.Add(pnlDebug);
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
            pnlLevelUp.ResumeLayout(false);
            pnlPause.ResumeLayout(false);
            pnlDebug.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
