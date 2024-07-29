namespace User_Interface
{
    partial class InstructionsUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstructionsUserControl));
            this.lbl_SpindleTest = new System.Windows.Forms.Label();
            this.axPDFReader = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)(this.axPDFReader)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_SpindleTest
            // 
            this.lbl_SpindleTest.AutoSize = true;
            this.lbl_SpindleTest.Font = new System.Drawing.Font("Century Gothic", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_SpindleTest.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lbl_SpindleTest.Location = new System.Drawing.Point(426, 0);
            this.lbl_SpindleTest.Name = "lbl_SpindleTest";
            this.lbl_SpindleTest.Size = new System.Drawing.Size(216, 44);
            this.lbl_SpindleTest.TabIndex = 17;
            this.lbl_SpindleTest.Text = "Instructions";
            // 
            // axPDFReader
            // 
            this.axPDFReader.Enabled = true;
            this.axPDFReader.Location = new System.Drawing.Point(3, 47);
            this.axPDFReader.Name = "axPDFReader";
            this.axPDFReader.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axPDFReader.OcxState")));
            this.axPDFReader.Size = new System.Drawing.Size(1107, 604);
            this.axPDFReader.TabIndex = 18;
            // 
            // InstructionsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axPDFReader);
            this.Controls.Add(this.lbl_SpindleTest);
            this.Name = "InstructionsUserControl";
            this.Size = new System.Drawing.Size(1113, 654);
            this.Load += new System.EventHandler(this.InstructionsUserControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axPDFReader)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lbl_SpindleTest;
        public AxAcroPDFLib.AxAcroPDF axPDFReader;
    }
}
