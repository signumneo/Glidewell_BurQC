namespace User_Interface
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btn_SpindleBreakIn = new System.Windows.Forms.Panel();
            this.btn_fixtureTransfer = new System.Windows.Forms.Button();
            this.btn_rdBinSettings = new System.Windows.Forms.Button();
            this.btn_binSettings = new System.Windows.Forms.Button();
            this.btn_RDSettings = new System.Windows.Forms.Button();
            this.btn_RD = new System.Windows.Forms.Button();
            this.btn_sorting = new System.Windows.Forms.Button();
            this.btn_TipScan1To1 = new System.Windows.Forms.Button();
            this.btn_SideScanPerLot = new System.Windows.Forms.Button();
            this.btn_SideScan = new System.Windows.Forms.Button();
            this.btn_SpindleTest = new System.Windows.Forms.Button();
            this.side_panel = new System.Windows.Forms.Panel();
            this.btn_SpindleBreak = new System.Windows.Forms.Button();
            this.btn_Home = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_Minimize = new System.Windows.Forms.Button();
            this.btn_CloseApplication = new System.Windows.Forms.Button();
            this.fixtureTransfer1 = new User_Interface.FixtureTransfer();
            this.rdBinSettings1 = new User_Interface.RDBinSettings();
            this.binSettings1 = new User_Interface.BinSettings();
            this.settings = new User_Interface.Settings();
            this.sideScanUserControl1 = new User_Interface.SideScanUserControl();
            this.sideScanUserControl = new User_Interface.SideScanUserControl();
            this.rdSettings1 = new User_Interface.RDSettings();
            this.inHouseBurInspection1 = new User_Interface.InHouseBurInspection();
            this.burSideScanPerLotUserControl = new User_Interface.BurSideScanPerLotUserControl();
            this.sorting1 = new User_Interface.Sorting();
            this.topScanUserControl = new User_Interface.HomeUserControl();
            this.tipScan1To11 = new User_Interface.TipScan1To1();
            this.tipScan1To1 = new User_Interface.TipScan1To1();
            this.btn_SpindleBreakIn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_SpindleBreakIn
            // 
            this.btn_SpindleBreakIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(39)))), ((int)(((byte)(40)))));
            this.btn_SpindleBreakIn.Controls.Add(this.btn_fixtureTransfer);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_rdBinSettings);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_binSettings);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_RDSettings);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_RD);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_sorting);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_TipScan1To1);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_SideScanPerLot);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_SideScan);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_SpindleTest);
            this.btn_SpindleBreakIn.Controls.Add(this.side_panel);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_SpindleBreak);
            this.btn_SpindleBreakIn.Controls.Add(this.btn_Home);
            this.btn_SpindleBreakIn.Controls.Add(this.pictureBox1);
            this.btn_SpindleBreakIn.Dock = System.Windows.Forms.DockStyle.Left;
            this.btn_SpindleBreakIn.Location = new System.Drawing.Point(0, 0);
            this.btn_SpindleBreakIn.Name = "btn_SpindleBreakIn";
            this.btn_SpindleBreakIn.Size = new System.Drawing.Size(203, 675);
            this.btn_SpindleBreakIn.TabIndex = 0;
            this.btn_SpindleBreakIn.Paint += new System.Windows.Forms.PaintEventHandler(this.btn_SpindleBreakIn_Paint);
            // 
            // btn_fixtureTransfer
            // 
            this.btn_fixtureTransfer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_fixtureTransfer.FlatAppearance.BorderSize = 0;
            this.btn_fixtureTransfer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_fixtureTransfer.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_fixtureTransfer.ForeColor = System.Drawing.Color.White;
            this.btn_fixtureTransfer.Location = new System.Drawing.Point(3, 587);
            this.btn_fixtureTransfer.Name = "btn_fixtureTransfer";
            this.btn_fixtureTransfer.Size = new System.Drawing.Size(197, 41);
            this.btn_fixtureTransfer.TabIndex = 25;
            this.btn_fixtureTransfer.Text = "Fixture Transfer";
            this.btn_fixtureTransfer.UseVisualStyleBackColor = true;
            this.btn_fixtureTransfer.Click += new System.EventHandler(this.btn_fixtureTransfer_Click);
            // 
            // btn_rdBinSettings
            // 
            this.btn_rdBinSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_rdBinSettings.FlatAppearance.BorderSize = 0;
            this.btn_rdBinSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_rdBinSettings.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_rdBinSettings.ForeColor = System.Drawing.Color.White;
            this.btn_rdBinSettings.Location = new System.Drawing.Point(12, 540);
            this.btn_rdBinSettings.Name = "btn_rdBinSettings";
            this.btn_rdBinSettings.Size = new System.Drawing.Size(197, 41);
            this.btn_rdBinSettings.TabIndex = 24;
            this.btn_rdBinSettings.Text = "R&&D Bin Settings";
            this.btn_rdBinSettings.UseVisualStyleBackColor = true;
            this.btn_rdBinSettings.Visible = false;
            this.btn_rdBinSettings.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_binSettings
            // 
            this.btn_binSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_binSettings.FlatAppearance.BorderSize = 0;
            this.btn_binSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_binSettings.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_binSettings.ForeColor = System.Drawing.Color.White;
            this.btn_binSettings.Location = new System.Drawing.Point(12, 399);
            this.btn_binSettings.Name = "btn_binSettings";
            this.btn_binSettings.Size = new System.Drawing.Size(197, 41);
            this.btn_binSettings.TabIndex = 23;
            this.btn_binSettings.Text = "Bin Settings";
            this.btn_binSettings.UseVisualStyleBackColor = true;
            this.btn_binSettings.Visible = false;
            this.btn_binSettings.Click += new System.EventHandler(this.btn_binSettings_Click);
            // 
            // btn_RDSettings
            // 
            this.btn_RDSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_RDSettings.FlatAppearance.BorderSize = 0;
            this.btn_RDSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_RDSettings.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_RDSettings.ForeColor = System.Drawing.Color.White;
            this.btn_RDSettings.Location = new System.Drawing.Point(12, 493);
            this.btn_RDSettings.Name = "btn_RDSettings";
            this.btn_RDSettings.Size = new System.Drawing.Size(197, 41);
            this.btn_RDSettings.TabIndex = 22;
            this.btn_RDSettings.Text = "R&&D Settings";
            this.btn_RDSettings.UseVisualStyleBackColor = true;
            this.btn_RDSettings.Click += new System.EventHandler(this.btn_RDSettings_Click);
            // 
            // btn_RD
            // 
            this.btn_RD.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_RD.FlatAppearance.BorderSize = 0;
            this.btn_RD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_RD.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_RD.ForeColor = System.Drawing.Color.White;
            this.btn_RD.Location = new System.Drawing.Point(12, 446);
            this.btn_RD.Name = "btn_RD";
            this.btn_RD.Size = new System.Drawing.Size(197, 41);
            this.btn_RD.TabIndex = 21;
            this.btn_RD.Text = "R&&D";
            this.btn_RD.UseVisualStyleBackColor = true;
            this.btn_RD.Click += new System.EventHandler(this.btn_RD_Click);
            // 
            // btn_sorting
            // 
            this.btn_sorting.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_sorting.FlatAppearance.BorderSize = 0;
            this.btn_sorting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_sorting.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_sorting.ForeColor = System.Drawing.Color.White;
            this.btn_sorting.Location = new System.Drawing.Point(12, 305);
            this.btn_sorting.Name = "btn_sorting";
            this.btn_sorting.Size = new System.Drawing.Size(197, 41);
            this.btn_sorting.TabIndex = 20;
            this.btn_sorting.Text = "Sorting";
            this.btn_sorting.UseVisualStyleBackColor = true;
            this.btn_sorting.Click += new System.EventHandler(this.btn_sorting_Click);
            // 
            // btn_TipScan1To1
            // 
            this.btn_TipScan1To1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_TipScan1To1.FlatAppearance.BorderSize = 0;
            this.btn_TipScan1To1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_TipScan1To1.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_TipScan1To1.ForeColor = System.Drawing.Color.White;
            this.btn_TipScan1To1.Location = new System.Drawing.Point(12, 211);
            this.btn_TipScan1To1.Name = "btn_TipScan1To1";
            this.btn_TipScan1To1.Size = new System.Drawing.Size(197, 41);
            this.btn_TipScan1To1.TabIndex = 19;
            this.btn_TipScan1To1.Text = "Tip Scan (1 to 1)";
            this.btn_TipScan1To1.UseVisualStyleBackColor = true;
            this.btn_TipScan1To1.Click += new System.EventHandler(this.btn_TipScan1To1_Click);
            // 
            // btn_SideScanPerLot
            // 
            this.btn_SideScanPerLot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_SideScanPerLot.FlatAppearance.BorderSize = 0;
            this.btn_SideScanPerLot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SideScanPerLot.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SideScanPerLot.ForeColor = System.Drawing.Color.White;
            this.btn_SideScanPerLot.Location = new System.Drawing.Point(12, 164);
            this.btn_SideScanPerLot.Name = "btn_SideScanPerLot";
            this.btn_SideScanPerLot.Size = new System.Drawing.Size(197, 41);
            this.btn_SideScanPerLot.TabIndex = 18;
            this.btn_SideScanPerLot.Text = "Side Scan - Per Lot";
            this.btn_SideScanPerLot.UseVisualStyleBackColor = true;
            this.btn_SideScanPerLot.Click += new System.EventHandler(this.btn_SideScanPerLot_Click);
            // 
            // btn_SideScan
            // 
            this.btn_SideScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_SideScan.FlatAppearance.BorderSize = 0;
            this.btn_SideScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SideScan.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SideScan.ForeColor = System.Drawing.Color.White;
            this.btn_SideScan.Location = new System.Drawing.Point(12, 117);
            this.btn_SideScan.Name = "btn_SideScan";
            this.btn_SideScan.Size = new System.Drawing.Size(197, 41);
            this.btn_SideScan.TabIndex = 17;
            this.btn_SideScan.Text = "Side Scan - Per Scan";
            this.btn_SideScan.UseVisualStyleBackColor = true;
            this.btn_SideScan.Click += new System.EventHandler(this.btn_SideScan_Click);
            // 
            // btn_SpindleTest
            // 
            this.btn_SpindleTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_SpindleTest.FlatAppearance.BorderSize = 0;
            this.btn_SpindleTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SpindleTest.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SpindleTest.ForeColor = System.Drawing.Color.White;
            this.btn_SpindleTest.Location = new System.Drawing.Point(12, 352);
            this.btn_SpindleTest.Name = "btn_SpindleTest";
            this.btn_SpindleTest.Size = new System.Drawing.Size(197, 41);
            this.btn_SpindleTest.TabIndex = 16;
            this.btn_SpindleTest.Text = "Scan Settings";
            this.btn_SpindleTest.UseVisualStyleBackColor = true;
            this.btn_SpindleTest.Click += new System.EventHandler(this.btn_SpindleTest_Click);
            // 
            // side_panel
            // 
            this.side_panel.BackColor = System.Drawing.Color.Brown;
            this.side_panel.Location = new System.Drawing.Point(0, 70);
            this.side_panel.Name = "side_panel";
            this.side_panel.Size = new System.Drawing.Size(10, 41);
            this.side_panel.TabIndex = 4;
            // 
            // btn_SpindleBreak
            // 
            this.btn_SpindleBreak.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_SpindleBreak.Enabled = false;
            this.btn_SpindleBreak.FlatAppearance.BorderSize = 0;
            this.btn_SpindleBreak.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_SpindleBreak.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SpindleBreak.ForeColor = System.Drawing.Color.White;
            this.btn_SpindleBreak.Location = new System.Drawing.Point(12, 258);
            this.btn_SpindleBreak.Name = "btn_SpindleBreak";
            this.btn_SpindleBreak.Size = new System.Drawing.Size(197, 41);
            this.btn_SpindleBreak.TabIndex = 15;
            this.btn_SpindleBreak.Text = "Instructions";
            this.btn_SpindleBreak.UseVisualStyleBackColor = true;
            this.btn_SpindleBreak.Click += new System.EventHandler(this.btn_SpindleBreak_Click);
            // 
            // btn_Home
            // 
            this.btn_Home.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Home.FlatAppearance.BorderSize = 0;
            this.btn_Home.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Home.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Home.ForeColor = System.Drawing.Color.White;
            this.btn_Home.Location = new System.Drawing.Point(12, 70);
            this.btn_Home.Name = "btn_Home";
            this.btn_Home.Size = new System.Drawing.Size(197, 41);
            this.btn_Home.TabIndex = 2;
            this.btn_Home.Text = "Tip Scan";
            this.btn_Home.UseVisualStyleBackColor = true;
            this.btn_Home.Click += new System.EventHandler(this.btn_Home_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(198, 45);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Brown;
            this.panel2.Controls.Add(this.btn_Minimize);
            this.panel2.Controls.Add(this.btn_CloseApplication);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(203, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1148, 27);
            this.panel2.TabIndex = 1;
            this.panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDown);
            this.panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseMove);
            this.panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseUp);
            // 
            // btn_Minimize
            // 
            this.btn_Minimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Minimize.AutoSize = true;
            this.btn_Minimize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Minimize.FlatAppearance.BorderSize = 0;
            this.btn_Minimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Minimize.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Minimize.ForeColor = System.Drawing.Color.White;
            this.btn_Minimize.Location = new System.Drawing.Point(1082, -3);
            this.btn_Minimize.Name = "btn_Minimize";
            this.btn_Minimize.Size = new System.Drawing.Size(31, 40);
            this.btn_Minimize.TabIndex = 18;
            this.btn_Minimize.Text = "-";
            this.btn_Minimize.UseVisualStyleBackColor = true;
            this.btn_Minimize.Click += new System.EventHandler(this.btn_Minimize_Click);
            // 
            // btn_CloseApplication
            // 
            this.btn_CloseApplication.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_CloseApplication.AutoSize = true;
            this.btn_CloseApplication.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_CloseApplication.FlatAppearance.BorderSize = 0;
            this.btn_CloseApplication.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_CloseApplication.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_CloseApplication.ForeColor = System.Drawing.Color.White;
            this.btn_CloseApplication.Location = new System.Drawing.Point(1112, 1);
            this.btn_CloseApplication.Name = "btn_CloseApplication";
            this.btn_CloseApplication.Size = new System.Drawing.Size(30, 31);
            this.btn_CloseApplication.TabIndex = 17;
            this.btn_CloseApplication.Text = "X";
            this.btn_CloseApplication.UseVisualStyleBackColor = true;
            this.btn_CloseApplication.Click += new System.EventHandler(this.btn_CloseApplication_Click);
            // 
            // fixtureTransfer1
            // 
            this.fixtureTransfer1.Location = new System.Drawing.Point(205, 25);
            this.fixtureTransfer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fixtureTransfer1.Name = "fixtureTransfer1";
            this.fixtureTransfer1.Size = new System.Drawing.Size(1113, 650);
            this.fixtureTransfer1.TabIndex = 16;
            // 
            // rdBinSettings1
            // 
            this.rdBinSettings1.Location = new System.Drawing.Point(222, 34);
            this.rdBinSettings1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdBinSettings1.Name = "rdBinSettings1";
            this.rdBinSettings1.Size = new System.Drawing.Size(1113, 654);
            this.rdBinSettings1.TabIndex = 15;
            // 
            // binSettings1
            // 
            this.binSettings1.Location = new System.Drawing.Point(222, 28);
            this.binSettings1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.binSettings1.Name = "binSettings1";
            this.binSettings1.Size = new System.Drawing.Size(1117, 650);
            this.binSettings1.TabIndex = 14;
            // 
            // settings
            // 
            this.settings.AutoSize = true;
            this.settings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.settings.BackColor = System.Drawing.SystemColors.Control;
            this.settings.Location = new System.Drawing.Point(221, 31);
            this.settings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.settings.Name = "settings";
            this.settings.Size = new System.Drawing.Size(1113, 654);
            this.settings.TabIndex = 3;
            // 
            // sideScanUserControl1
            // 
            this.sideScanUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sideScanUserControl1.AutoSize = true;
            this.sideScanUserControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sideScanUserControl1.Location = new System.Drawing.Point(210, 35);
            this.sideScanUserControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sideScanUserControl1.Name = "sideScanUserControl1";
            this.sideScanUserControl1.Size = new System.Drawing.Size(1108, 651);
            this.sideScanUserControl1.TabIndex = 10;
            // 
            // sideScanUserControl
            // 
            this.sideScanUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sideScanUserControl.AutoSize = true;
            this.sideScanUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sideScanUserControl.BackColor = System.Drawing.SystemColors.Control;
            this.sideScanUserControl.Location = new System.Drawing.Point(210, 35);
            this.sideScanUserControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sideScanUserControl.Name = "sideScanUserControl";
            this.sideScanUserControl.Size = new System.Drawing.Size(1108, 651);
            this.sideScanUserControl.TabIndex = 8;
            // 
            // rdSettings1
            // 
            this.rdSettings1.Location = new System.Drawing.Point(216, 39);
            this.rdSettings1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdSettings1.Name = "rdSettings1";
            this.rdSettings1.Size = new System.Drawing.Size(1113, 650);
            this.rdSettings1.TabIndex = 13;
            // 
            // inHouseBurInspection1
            // 
            this.inHouseBurInspection1.Location = new System.Drawing.Point(216, 28);
            this.inHouseBurInspection1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.inHouseBurInspection1.Name = "inHouseBurInspection1";
            this.inHouseBurInspection1.Size = new System.Drawing.Size(1113, 650);
            this.inHouseBurInspection1.TabIndex = 12;
            // 
            // burSideScanPerLotUserControl
            // 
            this.burSideScanPerLotUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.burSideScanPerLotUserControl.AutoSize = true;
            this.burSideScanPerLotUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.burSideScanPerLotUserControl.BackColor = System.Drawing.SystemColors.Control;
            this.burSideScanPerLotUserControl.Location = new System.Drawing.Point(216, 38);
            this.burSideScanPerLotUserControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.burSideScanPerLotUserControl.Name = "burSideScanPerLotUserControl";
            this.burSideScanPerLotUserControl.Size = new System.Drawing.Size(1105, 651);
            this.burSideScanPerLotUserControl.TabIndex = 5;
            // 
            // sorting1
            // 
            this.sorting1.Location = new System.Drawing.Point(210, 35);
            this.sorting1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sorting1.Name = "sorting1";
            this.sorting1.Size = new System.Drawing.Size(1113, 651);
            this.sorting1.TabIndex = 6;
            // 
            // topScanUserControl
            // 
            this.topScanUserControl.AutoSize = true;
            this.topScanUserControl.BackColor = System.Drawing.SystemColors.Control;
            this.topScanUserControl.Location = new System.Drawing.Point(216, 38);
            this.topScanUserControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.topScanUserControl.MaximumSize = new System.Drawing.Size(1900, 1000);
            this.topScanUserControl.MinimumSize = new System.Drawing.Size(1110, 650);
            this.topScanUserControl.Name = "topScanUserControl";
            this.topScanUserControl.Size = new System.Drawing.Size(1110, 650);
            this.topScanUserControl.TabIndex = 2;
            // 
            // tipScan1To11
            // 
            this.tipScan1To11.AutoSize = true;
            this.tipScan1To11.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tipScan1To11.Location = new System.Drawing.Point(222, 34);
            this.tipScan1To11.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tipScan1To11.Name = "tipScan1To11";
            this.tipScan1To11.Size = new System.Drawing.Size(1098, 644);
            this.tipScan1To11.TabIndex = 11;
            // 
            // tipScan1To1
            // 
            this.tipScan1To1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tipScan1To1.BackColor = System.Drawing.SystemColors.Control;
            this.tipScan1To1.Location = new System.Drawing.Point(11, -10);
            this.tipScan1To1.Margin = new System.Windows.Forms.Padding(4);
            this.tipScan1To1.Name = "tipScan1To1";
            this.tipScan1To1.Size = new System.Drawing.Size(1130, 651);
            this.tipScan1To1.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1351, 675);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btn_SpindleBreakIn);
            this.Controls.Add(this.fixtureTransfer1);
            this.Controls.Add(this.rdBinSettings1);
            this.Controls.Add(this.binSettings1);
            this.Controls.Add(this.settings);
            this.Controls.Add(this.sideScanUserControl1);
            this.Controls.Add(this.sideScanUserControl);
            this.Controls.Add(this.rdSettings1);
            this.Controls.Add(this.inHouseBurInspection1);
            this.Controls.Add(this.burSideScanPerLotUserControl);
            this.Controls.Add(this.sorting1);
            this.Controls.Add(this.topScanUserControl);
            this.Controls.Add(this.tipScan1To11);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.btn_SpindleBreakIn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel btn_SpindleBreakIn;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btn_Home;
        private System.Windows.Forms.PictureBox pictureBox1;
        private InstructionsUserControl spindleBreakInUserControl1;
        private System.Windows.Forms.Button btn_SpindleBreak;
        private System.Windows.Forms.Panel side_panel;
        private Settings spindleTestUserControl1;
        private System.Windows.Forms.Button btn_SpindleTest;
        private System.Windows.Forms.Button btn_Minimize;
        private System.Windows.Forms.Button btn_CloseApplication;
        private System.Windows.Forms.Button btn_SideScan;
        private System.Windows.Forms.Button btn_SideScanPerLot;
        private System.Windows.Forms.Button btn_TipScan1To1;
        private HomeUserControl topScanUserControl;
        private Settings settings;
        private BurSideScanPerLotUserControl burSideScanPerLotUserControl;
        private TipScan1To1 tipScan1To1;
        private System.Windows.Forms.Button btn_sorting;
        private Sorting sorting1;
        private SideScanUserControl sideScanUserControl;
        private SideScanUserControl sideScanUserControl1;
        private TipScan1To1 tipScan1To11;
        private System.Windows.Forms.Button btn_RD;
        private InHouseBurInspection inHouseBurInspection1;
        private System.Windows.Forms.Button btn_RDSettings;
        private RDSettings rdSettings1;
        private System.Windows.Forms.Button btn_binSettings;
        private BinSettings binSettings1;
        private System.Windows.Forms.Button btn_rdBinSettings;
        private RDBinSettings rdBinSettings1;
        private System.Windows.Forms.Button btn_fixtureTransfer;
        private FixtureTransfer fixtureTransfer1;
    }
}

