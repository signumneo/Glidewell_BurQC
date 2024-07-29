namespace User_Interface
{
    partial class PDFViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PDFViewer));
            this.scPDFReader = new System.Windows.Forms.SplitContainer();
            this.btn_ScanTempFolder = new System.Windows.Forms.Button();
            this.axPDFReader = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)(this.scPDFReader)).BeginInit();
            this.scPDFReader.Panel1.SuspendLayout();
            this.scPDFReader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axPDFReader)).BeginInit();
            this.SuspendLayout();
            // 
            // scPDFReader
            // 
            this.scPDFReader.Location = new System.Drawing.Point(0, 12);
            this.scPDFReader.Name = "scPDFReader";
            // 
            // scPDFReader.Panel1
            // 
            this.scPDFReader.Panel1.Controls.Add(this.btn_ScanTempFolder);
            this.scPDFReader.Panel1.Controls.Add(this.axPDFReader);
            this.scPDFReader.Panel2Collapsed = true;
            this.scPDFReader.Size = new System.Drawing.Size(525, 642);
            this.scPDFReader.TabIndex = 1;
            // 
            // btn_ScanTempFolder
            // 
            this.btn_ScanTempFolder.BackColor = System.Drawing.Color.SteelBlue;
            this.btn_ScanTempFolder.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_ScanTempFolder.ForeColor = System.Drawing.Color.White;
            this.btn_ScanTempFolder.Location = new System.Drawing.Point(172, 27);
            this.btn_ScanTempFolder.Name = "btn_ScanTempFolder";
            this.btn_ScanTempFolder.Size = new System.Drawing.Size(125, 35);
            this.btn_ScanTempFolder.TabIndex = 76;
            this.btn_ScanTempFolder.Text = "Scan";
            this.btn_ScanTempFolder.UseVisualStyleBackColor = false;
            this.btn_ScanTempFolder.Click += new System.EventHandler(this.btn_ScanTempFolder_Click);
            // 
            // axPDFReader
            // 
            this.axPDFReader.Enabled = true;
            this.axPDFReader.Location = new System.Drawing.Point(12, 146);
            this.axPDFReader.Name = "axPDFReader";
            this.axPDFReader.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axPDFReader.OcxState")));
            this.axPDFReader.Size = new System.Drawing.Size(513, 496);
            this.axPDFReader.TabIndex = 0;
            this.axPDFReader.Enter += new System.EventHandler(this.axPDFReader_Enter);
            // 
            // PDFViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 660);
            this.Controls.Add(this.scPDFReader);
            this.Name = "PDFViewer";
            this.Text = "PDFViewer";
            this.scPDFReader.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scPDFReader)).EndInit();
            this.scPDFReader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.axPDFReader)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scPDFReader;
        public AxAcroPDFLib.AxAcroPDF axPDFReader;
        private System.Windows.Forms.Button btn_ScanTempFolder;
    }
}