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
    public partial class BadScanSelector_UC : UserControl
    {
        public bool IsClosed = false;
        public BadScanSelector_UC()
        {
            InitializeComponent();
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private bool[] badScanBurResults = new bool[40];

        private void btn_ConfirmBadBurSelection_Click(object sender, EventArgs e)
        {
            string checkboxName;
            bool[] badScanBurResults = new bool[40];
            CheckBox chkBox;
            //Update Buttons
            string confirmCheckedScans = "Selected burs: \n";
            for (int i = 0; i < 40; i++)
            {
                checkboxName = "checkboxBadScanBur" + (i + 1).ToString();
                chkBox = (CheckBox)panelBurBadScans.Controls[checkboxName];
                badScanBurResults[i] = chkBox.Checked;
                if (badScanBurResults[i]) confirmCheckedScans += $"- {i + 1}\n";
            }

            var result = MessageBox.Show(confirmCheckedScans, "Confirm Selection", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                Form1.form1Instance.badScanBurResults = badScanBurResults;
                this.Hide();
            }
            else
            {

            }
        }
    }
}
