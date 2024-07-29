using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static User_Interface.SetupService;
using User_Interface.Main;

namespace User_Interface
{
    public partial class RDSettings : UserControl
    {
        private static string USERNAME = "admin";
        private static string PASSWORD = "1";
        public RDSettings()
        {
            InitializeComponent();
        }
        private void RDSettings_Loaded(object sender, EventArgs e)
        {
            try
            {
                pnlAccessSettings.BringToFront();

                LoadSettings();
            }
            catch (Exception ex)
            {

            }
        }

        private void LoadSettings()
        {
            try
            {
                //string currGC1 = mRDConfig.GrainCoverage.GC1.ToString();
                //string currGC2 = mRDConfig.GrainCoverage.GC2.ToString();
                string currGC3 = mRDConfig.GrainCoverage.GC3.ToString();
                string currGC4 = mRDConfig.GrainCoverage.GC4.ToString();
                //string currGC5 = mRDConfig.GrainCoverage.GC5.ToString();


                //TB_GC1Current.Text = currGC1;
                //TB_GC2Current.Text = currGC2;
                TB_GC3Current.Text = currGC3;
                TB_GC4Current.Text = currGC4;
                //TB_GC5Current.Text = currGC5;

                //TB_GC1New.Text = currGC1;
                //TB_GC2New.Text = currGC2;
                TB_GC3New.Text = currGC3;
                TB_GC4New.Text = currGC4;
                //TB_GC5New.Text = currGC5;

                //lblGC1.Text = currGC1;
                //lblGC2.Text = currGC2;
                lblGC3.Text = currGC3;
                lblGC4.Text = currGC4;
                //lblGC5.Text = currGC5;

                TB_CurrentCsvBaggingFolderPath.Text = mRDConfig.CSVBaggingFolderPath;
                TB_NewCsvBaggingFolderPath.Text = mRDConfig.CSVBaggingFolderPath;

                TB_CurrentTempFolderPath.Text = mRDConfig.TempFolderName;
                TB_NewTempFolderPath.Text = mRDConfig.TempFolderName;

                string currMGP = mRDConfig.MeanGrainProtustion.MTV1.ToString();
                TB_MTV1Current.Text = currMGP;
                TB_MTV1New.Text = currMGP;

            }
            catch (Exception) { }

        }

        private void btnOpenSettings_Click(object sender, EventArgs e)
        {
            if (TB_UserName.Text == USERNAME && TB_Password.Text == PASSWORD)
            {
                pnlAccessSettings.SendToBack();
                lblSettings.Hide();
            }
            else
            {
                DialogResult dialog = MessageBox.Show("Incorrect username or password");
            }
        }

        private void btnFindTempFolderPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select your temp folder path. " })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    TB_NewTempFolderPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnFindCsvBaggingFolderPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select your root path. " })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    TB_NewCsvBaggingFolderPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnWriteSettings_Click(object sender, EventArgs e)
        {
            if (CheckUserInputs())
            {
                if (MessageBox.Show("Do you want to activate the new config?", "Confirm Operation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveRDConfigFile(mRDConfig);
                    MessageBox.Show("New configuration has been activated.");
                    LoadSettings();
                }
            }
        }

        private bool CheckUserInputs()
        {
            int newMGP, newGC1, newGC2, newGC3, newGC4, newGC5 = 0;
            try
            {
                //if (string.IsNullOrEmpty(TB_GC1New.Text) ||
                //    string.IsNullOrEmpty(TB_GC2New.Text) ||
                //    string.IsNullOrEmpty(TB_GC3New.Text) ||
                //    string.IsNullOrEmpty(TB_GC4New.Text) ||
                //    string.IsNullOrEmpty(TB_GC5New.Text) ||
                //    string.IsNullOrEmpty(TB_MTV1New.Text))
                if (string.IsNullOrEmpty(TB_GC3New.Text) ||
                   string.IsNullOrEmpty(TB_GC4New.Text) ||
                   string.IsNullOrEmpty(TB_MTV1New.Text))
                {
                    DialogResult dialog = MessageBox.Show("Please enter values for all GC and MGP criteria.");
                    return false;
                }
                else
                {
                    try
                    {
                        //Int32.TryParse(TB_GC1New.Text, out newGC1);
                        //Int32.TryParse(TB_GC2New.Text, out newGC2);
                        //Int32.TryParse(TB_GC3New.Text, out newGC3);
                        //Int32.TryParse(TB_GC4New.Text, out newGC4);
                        //Int32.TryParse(TB_GC5New.Text, out newGC5);
                        //Int32.TryParse(TB_MTV1New.Text, out newMGP);
                        //Int32.TryParse(TB_GC1New.Text, out newGC1);
                        //Int32.TryParse(TB_GC2New.Text, out newGC2);
                        Int32.TryParse(TB_GC3New.Text, out newGC3);
                        Int32.TryParse(TB_GC4New.Text, out newGC4);
                        //Int32.TryParse(TB_GC5New.Text, out newGC5);
                        Int32.TryParse(TB_MTV1New.Text, out newMGP);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Invalid values for MGP or GCs");
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(TB_NewCsvBaggingFolderPath.Text) ||
                    string.IsNullOrEmpty(TB_NewTempFolderPath.Text))
                {
                    DialogResult dialog = MessageBox.Show("Please specify the new csv folder path and the temp folder temp");
                    return false;
                }


                /// CSV BAGGING PATH??

                //if (!(newMGP > 0 &&
                //    newGC5 > newGC4 &&
                //    newGC4 > newGC3 &&
                //    newGC3 > newGC2 &&
                //    newGC2 > newGC1 &&
                //    newGC1 > 0))
                if (!(newMGP > 0 &&
                  newGC4 > newGC3 &&
                  newGC3 > 0))
                {
                    DialogResult dialog = MessageBox.Show("Please make sure that values for MGP and GCs follow the criteria below: \n MGP > 0 \n GC5 > GC4 > GC3 > GC2 > GC1 > 0");
                    return false;
                }

                mRDConfig.TempFolderName = TB_NewTempFolderPath.Text;
                mRDConfig.CSVBaggingFolderPath = TB_NewCsvBaggingFolderPath.Text;
                mRDConfig.MeanGrainProtustion.MTV1 = newMGP;
                //mRDConfig.GrainCoverage.GC1 = newGC1;
                //mRDConfig.GrainCoverage.GC2 = newGC2;
                mRDConfig.GrainCoverage.GC3 = newGC3;
                mRDConfig.GrainCoverage.GC4 = newGC4;
                //mRDConfig.GrainCoverage.GC5 = newGC5;


                return true;
            }
            catch (Exception)
            {
                
                return false;
            }
        }
    }
}
