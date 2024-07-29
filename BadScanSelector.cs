using System;
using System.Collections;
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
    public partial class BadScanSelector : Form
    {
        public BadScanSelector()
        {
            InitializeComponent();
            
        }

        private bool[] badScanBurResults = new bool[40];
        public int test; 

        private void btn_ConfirmBadBurSelection_Click(object sender, EventArgs e)
        {
            string checkboxName;
            bool[] badScanBurResults = new bool[40];
            bool[] discardBurResults = new bool[40];

            CheckedListBox checkedListBox;
            //Update Buttons
            for (int i = 0; i < 40; i++)
            {
                checkboxName = "checkedListBox" + (i + 1).ToString();
                checkedListBox = (CheckedListBox)panelBurBadScans.Controls[checkboxName];
                if (checkedListBox.CheckedItems.Count == 0)
                {
                    continue;
                }

                if (checkedListBox.CheckedItems.Contains("Bad Scan?"))
                {
                    badScanBurResults[i] = true;
                }
                else if (checkedListBox.CheckedItems.Contains("Discard?"))
                {
                    discardBurResults[i] = true;
                }
                //checkboxName = "RB_badScanBur" + (i + 1).ToString();
                //chkBox = (CheckBox)panelBurBadScans.Controls[checkboxName];
                //badScanBurResults[i] = chkBox.Checked;

                //checkboxName = "RB_discardBur" + (i + 1).ToString();
                //chkBox = (CheckBox)panelBurBadScans.Controls[checkboxName];
                //discardBurResults[i] = chkBox.Checked;
            }



            //Console.WriteLine("---------------Results from Bur Selector Form------------------------");
            //for (int i = 0; i < badScanBurResults.Length; i++)
            //{
            //    //Console.WriteLine("Bur " + (i+1).ToString()+ " : " + badScanBurResults[i].ToString());
            //}
            //Console.WriteLine("---------------END------------------------");

            Form1.form1Instance.badScanBurResults = badScanBurResults;
            Form1.form1Instance.discardBurResults = discardBurResults;
            this.Close();
        }

        private int lastCheck = -1;

        private void CBL_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CheckedListBox checkedListBox = (CheckedListBox)sender;
            if (e.NewValue == CheckState.Checked)
            {
                IEnumerator myEnumerator;
                myEnumerator = checkedListBox.CheckedIndices.GetEnumerator();
                int y;
                while (myEnumerator.MoveNext() != false)
                {
                    y = (int)myEnumerator.Current;
                    checkedListBox.SetItemChecked(y, false);
                }
            }
        }
    }
}
