namespace User_Interface
{
    partial class LotNumberCorrectorForm
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
            this.TB_correctedLotNumber = new System.Windows.Forms.TextBox();
            this.Btn_update = new System.Windows.Forms.Button();
            this.Btn_keep = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.TB_currentLotNumber = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TB_correctedLotNumber
            // 
            this.TB_correctedLotNumber.Location = new System.Drawing.Point(170, 123);
            this.TB_correctedLotNumber.Name = "TB_correctedLotNumber";
            this.TB_correctedLotNumber.Size = new System.Drawing.Size(100, 20);
            this.TB_correctedLotNumber.TabIndex = 0;
            // 
            // Btn_update
            // 
            this.Btn_update.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.Btn_update.Location = new System.Drawing.Point(205, 157);
            this.Btn_update.Name = "Btn_update";
            this.Btn_update.Size = new System.Drawing.Size(75, 23);
            this.Btn_update.TabIndex = 2;
            this.Btn_update.Text = "Update";
            this.Btn_update.UseVisualStyleBackColor = false;
            this.Btn_update.Click += new System.EventHandler(this.Btn_update_Click);
            // 
            // Btn_keep
            // 
            this.Btn_keep.Location = new System.Drawing.Point(301, 157);
            this.Btn_keep.Name = "Btn_keep";
            this.Btn_keep.Size = new System.Drawing.Size(84, 23);
            this.Btn_keep.TabIndex = 3;
            this.Btn_keep.Text = "Keep Current";
            this.Btn_keep.UseVisualStyleBackColor = true;
            this.Btn_keep.Click += new System.EventHandler(this.Btn_keep_Click);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.Color.RosyBrown;
            this.textBox2.Font = new System.Drawing.Font("Century Gothic", 9F);
            this.textBox2.Location = new System.Drawing.Point(13, 25);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(372, 59);
            this.textBox2.TabIndex = 5;
            this.textBox2.Text = "The lot number is not in the expected format. If the lot number is not correct, p" +
    "lease enter the correct number and press \"Update.\" Otherwise, press \"Keep.\"";
            // 
            // TB_currentLotNumber
            // 
            this.TB_currentLotNumber.Enabled = false;
            this.TB_currentLotNumber.Location = new System.Drawing.Point(170, 97);
            this.TB_currentLotNumber.Name = "TB_currentLotNumber";
            this.TB_currentLotNumber.Size = new System.Drawing.Size(100, 20);
            this.TB_currentLotNumber.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(94, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Current";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(94, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Corrected";
            // 
            // LotNumberCorrectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 192);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TB_currentLotNumber);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.Btn_keep);
            this.Controls.Add(this.Btn_update);
            this.Controls.Add(this.TB_correctedLotNumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LotNumberCorrectorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LotNumberCorrectorForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TB_correctedLotNumber;
        private System.Windows.Forms.Button Btn_update;
        private System.Windows.Forms.Button Btn_keep;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox TB_currentLotNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}