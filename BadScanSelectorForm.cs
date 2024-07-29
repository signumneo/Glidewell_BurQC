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
    public partial class BadScanSelectorForm : Form
    {
        public BadScanSelectorForm()
        {
            InitializeComponent();
        }

        private void btn_ConfirmBadBurSelection_Click(object sender, EventArgs e)
        {
            string checkboxName;
            bool[] badScanBurResults = new bool[40];
            CheckBox chkBox;
            for (int i = 0; i < 40; i++)
            {
                checkboxName = $"checkboxBadScanBur{i+1}";
                chkBox = (CheckBox)panelBurBadScans.Controls[checkboxName];
                badScanBurResults[i] = chkBox.Checked;
            }
            Form1.form1Instance.badScanBurResults = badScanBurResults;
            this.Close();
        }
    }
}
