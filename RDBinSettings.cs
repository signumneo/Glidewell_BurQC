using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using User_Interface.Main;
using static User_Interface.SetupService;

namespace User_Interface
{
    public partial class RDBinSettings : UserControl
    {
        public RDBinSettings()
        {
            InitializeComponent();
        }

        private void BinSettings_Loaded(object sender, EventArgs e)
        {
            pnlAccessSettings.BringToFront();

            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                lbl_version.Text = mRDBinConfig.version.ToString();
                int[] currLowerGCs = new int[10];
                int[] currUpperGCs = new int[10];
                int currMGP = 0;

                string textBoxName;
                string binName;
                for (int i = 0; i < 10; i++)
                {
                    currLowerGCs[i] = mRDBinConfig.GCBins[i].lowerGC;
                    currUpperGCs[i] = mRDBinConfig.GCBins[i].upperGC;

                    textBoxName = $"textBoxLowerGC{i + 1}";
                    ((TextBox)this.Controls[textBoxName]).Text = currLowerGCs[i].ToString();
                    textBoxName = $"textBoxUpperGC{i + 1}";
                    ((TextBox)this.Controls[textBoxName]).Text = currUpperGCs[i].ToString();

                    textBoxName = $"textBoxNewLowerGC{i + 1}";
                    ((TextBox)this.Controls[textBoxName]).Text = currLowerGCs[i].ToString();
                    textBoxName = $"textBoxNewUpperGC{i + 1}";
                    ((TextBox)this.Controls[textBoxName]).Text = currUpperGCs[i].ToString();


                    binName = $"bin{i + 1}";
                    ((ComboBox)this.Controls[binName]).Text = mRDBinConfig.GCBins[i].bin;

                    binName = $"newbin{i + 1}";
                    ((ComboBox)this.Controls[binName]).Text = mRDBinConfig.GCBins[i].bin;
                }
                currMGP = mRDBinConfig.MGPCutoffBin.MGP;
                textBoxName = "textBoxNewMGP";
                ((TextBox)this.Controls[textBoxName]).Text = currMGP.ToString();
                textBoxName = "textBoxMGP";
                ((TextBox)this.Controls[textBoxName]).Text = currMGP.ToString();

                binName = "newbinMGP";
                ((ComboBox)this.Controls[binName]).Text = mRDBinConfig.MGPCutoffBin.bin;

                binName = "binMGP";
                ((ComboBox)this.Controls[binName]).Text = mRDBinConfig.MGPCutoffBin.bin;

            }
            catch (Exception ex)
            {

            }
        }

        private void btnWriteSettings_Click(object sender, EventArgs e)
        {
            if (ValidateInputs() && ChangesMade)
            {
                if (MessageBox.Show("Update R&D bin settings?", "Confirm Operation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveRDBinConfig(mRDBinConfig);
                    MessageBox.Show("R&D bin settings have been updated!");
                    LoadSettings();

                }
            }
            ChangesMade = false;
        }
        int[] newLowerGCInputs = new int[10];
        int[] newUpperGCInputs = new int[10];
        int newMGPInput = 0;
        bool ChangesMade = false;
        private bool ValidateInputs()
        {
            string textBoxName;
            string binName;
            string binSelection;
            try
            {
                textBoxName = "textBoxNewMGP";
                newMGPInput = Convert.ToInt32(((TextBox)this.Controls[textBoxName]).Text);
                if (newMGPInput <= 0)
                {
                    MessageBox.Show("Please make sure that the MGP value is valid.");
                    return false;
                }
                binName = "newbinMGP";
                binSelection = ((ComboBox)this.Controls[binName]).Text;
                if (string.IsNullOrEmpty(binSelection))
                {
                    MessageBox.Show("Please make sure that a bin is selected for the MGP fail burs.");
                }
                mRDBinConfig.MGPCutoffBin.MGP = newMGPInput;
                mRDBinConfig.MGPCutoffBin.bin = binSelection;

                for (int i = 0; i < 10; i++)
                {
                    textBoxName = $"textBoxNewLowerGC{i + 1}";
                    newLowerGCInputs[i] = Convert.ToInt32(((TextBox)this.Controls[textBoxName]).Text);
                    textBoxName = $"textBoxNewUpperGC{i + 1}";
                    newUpperGCInputs[i] = Convert.ToInt32(((TextBox)this.Controls[textBoxName]).Text);
                  
                    if (newLowerGCInputs[i] == 0 && newUpperGCInputs[i] == 0)
                    {
                        continue;
                    }

                    binName = $"newbin{i+1}";
                    binSelection = ((ComboBox)this.Controls[binName]).Text;
                    if (newLowerGCInputs[i] >= newUpperGCInputs[i])
                    {
                        MessageBox.Show("Please make sure that upper GC > lower GC.");
                        return false;
                    }
                    if (newLowerGCInputs[i] < newUpperGCInputs[i] && string.IsNullOrEmpty(binSelection))
                    {
                        MessageBox.Show("Please make sure that a bin is selected for the criteria.");
                        return false;
                    }

                    mRDBinConfig.GCBins[i].bin = binSelection;
                    mRDBinConfig.GCBins[i].lowerGC = newLowerGCInputs[i];
                    mRDBinConfig.GCBins[i].upperGC = newUpperGCInputs[i];

                    //if (!string.IsNullOrEmpty(mRDBinConfig.GCBins[i].bin) && !string.IsNullOrEmpty(mRDBinConfig.MGPCutoffBin.bin) && mRDBinConfig.GCBins[i].bin == mRDBinConfig.MGPCutoffBin.bin)
                    //{
                    //    MessageBox.Show("The bin selected for the MGP cutoff has to be unique. Please update the bin selection.");
                    //    return false;
                    //}
                }

                bool doRangesOverlap = GF.IsOverlapping(newLowerGCInputs, newUpperGCInputs);

                if (doRangesOverlap)
                {
                    MessageBox.Show("At least two ranges overlap. Please check the lower and upper GC values.");
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Please make sure the new inputs are valid.");
                return false;
            }

            mRDBinConfig.updatedTime = DateTime.Now.ToString();
            mRDBinConfig.version += 1;
            ChangesMade = true;
            return true;
        }



       

        string USERNAME = "admin";
        string PASSWORD = "1";
        private void btnOpenSettings_Click(object sender, EventArgs e)
        {
            if (txtboxUserName.Text != USERNAME && txtboxPassword.Text != PASSWORD)
            {
                MessageBox.Show("Incorrect username or password! Please enter the correct username and password");
                return;
            }

            pnlAccessSettings.Hide();
        }

    }
}
