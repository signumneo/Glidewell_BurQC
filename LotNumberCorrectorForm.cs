using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace User_Interface
{
    public partial class LotNumberCorrectorForm : Form
    {
        public LotNumberCorrectorForm()
        {
            InitializeComponent();
        }

        public LotNumberCorrectorForm(string lotNumber)
        {
            InitializeComponent();
            TB_currentLotNumber.Text = lotNumber;
        }

        private void Btn_update_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TB_correctedLotNumber.Text))
            {
                MessageBox.Show("Please enter the correct lot number in the expected format!");
                return;
            }
            if (TB_correctedLotNumber.Text.Length != 10)
            {
                MessageBox.Show("The inputted lot number is not 10 digits long!");
                return;
            }
            Form1.form1Instance.correctLotNumber = TB_correctedLotNumber.Text;
            this.Close();
        }

        private void Btn_keep_Click(object sender, EventArgs e)
        {
            Form1.form1Instance.correctLotNumber = TB_currentLotNumber.Text;
            this.Close();
        }
    }
}
