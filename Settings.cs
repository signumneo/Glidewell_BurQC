using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using static User_Interface.SetupService;
using User_Interface.Main;

namespace User_Interface
{

    public partial class Settings : UserControl
    {
        FilesFunctions filesFunctions = new FilesFunctions();
        CSVFunctions CSVFunctions = new CSVFunctions();
        int currentGrainCoverageCriteria1, currentGrainCoverageCriteria2, currentGrainCoverageCriteria3, currentGrainCoverageCriteria4, currentGrainCoverageCriteria5, currentMeanGrainProtusionCriteria1; //current criterias from csv file
        int newGrainCoverageCriteria1, newGrainCoverageCriteria2, newGrainCoverageCriteria3, newGrainCoverageCriteria4, newGrainCoverageCriteria5, newMeanGrainProtusionCriteria1; //new/desired criteria from PDF

        static string USERNAME = "admin";

        static string PASSWORD = "1";
        private void btnOpenSettings_Click(object sender, EventArgs e)
        {
            if (txtboxUserName.Text == USERNAME && txtboxPassword.Text == PASSWORD)
            {
                pnlAccessSettings.SendToBack();
                lblSettings.Hide();
            }
            else
            {
                DialogResult dialog = MessageBox.Show("Incorrect user name or password." +
                "Please contact system administrator",
                    "Incorrect User Name or Password");
            }
        }

        //private void btnFindSideScanRootFolderPath_Click(object sender, EventArgs e)
        //{
        //    using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select your root path. " })
        //    {
        //        if (fbd.ShowDialog() == DialogResult.OK)
        //        {
        //            txtboxNewFilePathSideScan.Text = fbd.SelectedPath;
        //        }
        //    }
        //}
        private void btnFindCsvBaggingFolderPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select your root path. " })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtboxNewCsvBaggingFolderPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnWriteSettings_Click(object sender, EventArgs e)
        {
            if (CheckUserInputs())
            {
                if (MessageBox.Show("Do you want to activate the new config?", "Confirm Operation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveProgramConfigFile(mPConfig);
                    MessageBox.Show("New configuration has been activated.");
                    LoadSettings();
                }
                //Application.Restart();
                //Environment.Exit(0);
            }

        }
        private void LoadSettings()
        {

            try
            {
                string currentGrainCoverageCriteria1String = mPConfig.GrainCoverage.GC1.ToString();
                string currentGrainCoverageCriteria2String = mPConfig.GrainCoverage.GC2.ToString();
                string currentGrainCoverageCriteria3String = mPConfig.GrainCoverage.GC3.ToString();
                string currentGrainCoverageCriteria4String = mPConfig.GrainCoverage.GC4.ToString();
                string currentGrainCoverageCriteria5String = mPConfig.GrainCoverage.GC5.ToString();

                textboxGC1Current.Text = currentGrainCoverageCriteria1String;
                textboxGC2Current.Text = currentGrainCoverageCriteria2String;
                textboxGC3Current.Text = currentGrainCoverageCriteria3String;
                textboxGC4Current.Text = currentGrainCoverageCriteria4String;
                textboxGC5Current.Text = currentGrainCoverageCriteria5String;

                textboxGC1New.Text = currentGrainCoverageCriteria1String;
                textboxGC2New.Text = currentGrainCoverageCriteria2String;
                textboxGC3New.Text = currentGrainCoverageCriteria3String;
                textboxGC4New.Text = currentGrainCoverageCriteria4String;
                textboxGC5New.Text = currentGrainCoverageCriteria5String;

                lblGC1.Text = currentGrainCoverageCriteria1String;
                lblGC2.Text = currentGrainCoverageCriteria2String;
                lblGC3.Text = currentGrainCoverageCriteria3String;
                lblGC4.Text = currentGrainCoverageCriteria4String;
                lblGC5.Text = currentGrainCoverageCriteria5String;



                txtboxCurrentCsvBaggingFolderPath.Text = mPConfig.CSVBaggingFolderPath;
                txtboxNewCsvBaggingFolderPath.Text = txtboxCurrentCsvBaggingFolderPath.Text;

                Int32.TryParse(currentGrainCoverageCriteria1String, out currentGrainCoverageCriteria1);
                Int32.TryParse(currentGrainCoverageCriteria2String, out currentGrainCoverageCriteria2);
                Int32.TryParse(currentGrainCoverageCriteria3String, out currentGrainCoverageCriteria3);
                Int32.TryParse(currentGrainCoverageCriteria4String, out currentGrainCoverageCriteria4);
                Int32.TryParse(currentGrainCoverageCriteria5String, out currentGrainCoverageCriteria5);

                txtboxCurrentTempFolderPath.Text = mPConfig.TempFolderName;
                txtbox_NewTempFolderPath.Text = txtboxCurrentTempFolderPath.Text;

                string currentMeanGrainProtusionCriteria1String = mPConfig.MeanGrainProtustion.MTV1.ToString();
                Int32.TryParse(currentMeanGrainProtusionCriteria1String, out currentMeanGrainProtusionCriteria1);
                textboxMTV1Current.Text = currentMeanGrainProtusionCriteria1String;
                textboxMTV1New.Text = currentMeanGrainProtusionCriteria1String;
                lblMGP1.Text = currentMeanGrainProtusionCriteria1String;

                GV.SetCriteria();
            }
            catch
            {
                /*
                DialogResult dialog = MessageBox.Show("Unabled to read pass fail criteria and/or root folder from txt file" +
                "If the PDFs are moved from the temp folder, you will no longer be able to see results on this screen.",
                    "Configurations Text File Error");
                    */
            }

        }
        private void SpindleTestUserControl_Load(object sender, EventArgs e)
        {
            pnlAccessSettings.BringToFront();
            textboxGC1Current.Enabled = false;
            textboxGC2Current.Enabled = false;
            textboxGC3Current.Enabled = false;
            textboxGC4Current.Enabled = false;
            textboxGC5Current.Enabled = false;
            textboxMTV1Current.Enabled = false;
            //txtboxCurrentFilePath.Enabled = false;
            txtboxCurrentTempFolderPath.Enabled = false;
            txtboxCurrentCsvBaggingFolderPath.Enabled = false;
            LoadSettings();
        }

        public Settings()
        {
            InitializeComponent();
        }

        private void btnFindTempFolderPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select your temp folder path. " })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtbox_NewTempFolderPath.Text = fbd.SelectedPath;
                }
            }
        }

        private bool CheckUserInputs()
        {
            bool userInputAcceptable = false;
            string rootLocation = "";
            string rootLocationSideScan = "";
            string rootFolderCsvBagging = "";
            int stateMachine = 0;

            if (stateMachine == 0)
            {
                stateMachine = 10;
            }

            //check if inputs are numbers an integers
            if (stateMachine == 10)
            {
                if (string.IsNullOrEmpty(textboxGC1New.Text) ||
                    string.IsNullOrEmpty(textboxGC2New.Text) ||
                    string.IsNullOrEmpty(textboxGC3New.Text) ||
                    string.IsNullOrEmpty(textboxGC4New.Text) ||
                    string.IsNullOrEmpty(textboxGC5New.Text) ||
                    string.IsNullOrEmpty(textboxMTV1New.Text))
                {
                    DialogResult dialog = MessageBox.Show("All MTV and GC inputs need to be not empty and a integer number" +
                       "Please make sure all input fields are filled",
                           "Inputs Error");
                }

                else
                {
                    try
                    {
                        Int32.TryParse(textboxGC1New.Text, out newGrainCoverageCriteria1);
                        Int32.TryParse(textboxGC2New.Text, out newGrainCoverageCriteria2);
                        Int32.TryParse(textboxGC3New.Text, out newGrainCoverageCriteria3);
                        Int32.TryParse(textboxGC4New.Text, out newGrainCoverageCriteria4);
                        Int32.TryParse(textboxGC5New.Text, out newGrainCoverageCriteria5);
                        Int32.TryParse(textboxMTV1New.Text, out newMeanGrainProtusionCriteria1);
                        Console.WriteLine("MTV1: " + newMeanGrainProtusionCriteria1 + ", GC1: " + newGrainCoverageCriteria1.ToString() + ", GC2: " + newGrainCoverageCriteria2.ToString() + ", GC3: " + newGrainCoverageCriteria3.ToString() + ", GC4: " + newGrainCoverageCriteria4.ToString() + ", GC5: " + newGrainCoverageCriteria5.ToString());
                        stateMachine = 20;
                    }
                    catch
                    {
                        DialogResult dialog = MessageBox.Show("All MTV and GC inputs need to be an integer number" +
                           "System will restart",
                               "Inputs Error");
                        stateMachine = 1000;
                    }
                }


            }

            //check if file path is not empty

            if (stateMachine == 20)
            {
                if (/*string.IsNullOrEmpty(txtbox_SelectedFilePath.Text) || */string.IsNullOrEmpty(txtbox_NewTempFolderPath.Text) || /*string.IsNullOrEmpty(txtboxNewFilePathSideScan.Text) ||*/ string.IsNullOrEmpty(txtboxNewCsvBaggingFolderPath.Text))
                {
                    DialogResult dialog = MessageBox.Show("New root folder(s) path and Temp Folder Name cannot be empty. " +
                       "Please make sure all input fields are completed",
                           "Inputs Error");
                    stateMachine = 1000;
                }
                else
                {
                    stateMachine = 30;
                }

            }

            //create root location string (CHECK BOX) 
            if (stateMachine == 30)
            {

                bool userNameFound;
                bool userNameFoundSideScan;
                bool userNameFoundCsvBagging;

                string[] rootFilePath;
                string[] rootFilePathSideScan;
                string[] rootFolderCsvBaggingArray;
                string userName;
                string userNameSideScan;

                try
                {

                    rootFolderCsvBagging = txtboxNewCsvBaggingFolderPath.Text;
                    rootFolderCsvBagging = rootFolderCsvBagging.Replace("/", "\\");
                    rootFolderCsvBaggingArray = (Regex.Split(rootFolderCsvBagging, @"\\"));

                    //CSV Bagging - CHECK IF FILE SELECTED IS UNDER A SPECIFIC USER
                    bool csvBaggingFolderError = false;
                    //Check if the file selected is under a specific user
                    if (rootFolderCsvBaggingArray.Length >= 2)
                    {
                        if (rootFolderCsvBaggingArray[1] == "Users")
                        {
                            userNameFoundCsvBagging = true;
                        }
                        else
                        {
                            userNameFoundCsvBagging = false;
                        }
                    }
                    else
                    {
                        userNameFoundCsvBagging = false;
                    }

                    //TOP SCAN SETTINGS
                    if (userNameFoundCsvBagging == true)
                    {
                        userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                        userName = Regex.Split(userName, @"\\")[1];
                        rootFolderCsvBagging = rootFolderCsvBagging.Replace("hello"/*rootFilePath[2]*/, userName);

                        Console.WriteLine("----------------ROOT FILE PATH--------------");
                        Console.WriteLine("file location: " + rootFolderCsvBagging);
                    }

                    if (csvBaggingFolderError == false)
                    {

                    }

                    stateMachine = 40;


                }

                catch (Exception error)
                {
                    MessageBox.Show("There has been an error. See below: \n \n " +
                         error, "System Error");
                    stateMachine = 1000;
                }

            }

            if (stateMachine == 40)
            {
                if (newMeanGrainProtusionCriteria1 > 0 &&
                    newGrainCoverageCriteria5 > newGrainCoverageCriteria4 && newGrainCoverageCriteria4 > newGrainCoverageCriteria3  && newGrainCoverageCriteria3 > newGrainCoverageCriteria2 &&
                    newGrainCoverageCriteria2 > newGrainCoverageCriteria1 &&
                    newGrainCoverageCriteria1 > 0
                    )
                {
                    stateMachine = 50;
                }
                else
                {
                    DialogResult dialog = MessageBox.Show("Please make sure that values for MTV1, GC1, GC2, GC3, GC4, GC5 follow the criteria below" +
                        Environment.NewLine +
                        Environment.NewLine + "MTV1 > 0" +
                        Environment.NewLine + "GC5 > GC4" +
                        Environment.NewLine + "GC4 > GC3" +
                        Environment.NewLine + "GC3 > GC2" +
                        Environment.NewLine + "GC2 > GC1" +
                        Environment.NewLine + "GC1 > 0",
                       "Verify Inputs");
                    Console.WriteLine("MTV1: " + newMeanGrainProtusionCriteria1 + ", GC1: " + newGrainCoverageCriteria1.ToString() + ", GC2: " + newGrainCoverageCriteria2.ToString() + ", GC3: " + newGrainCoverageCriteria3.ToString() + ", GC4: " + newGrainCoverageCriteria4.ToString() + ", GC5: " + newGrainCoverageCriteria5.ToString());

                    //if ()
                    stateMachine = 1000;
                }
            }

            if (stateMachine == 50)
            {

                mPConfig.TempFolderName = txtbox_NewTempFolderPath.Text;
                mPConfig.CSVBaggingFolderPath = rootFolderCsvBagging;
                mPConfig.MeanGrainProtustion.MTV1 = Convert.ToInt32(textboxMTV1New.Text);
                mPConfig.GrainCoverage.GC1 = Convert.ToInt32(textboxGC1New.Text);
                mPConfig.GrainCoverage.GC2 = Convert.ToInt32(textboxGC2New.Text);
                mPConfig.GrainCoverage.GC3 = Convert.ToInt32(textboxGC3New.Text);
                mPConfig.GrainCoverage.GC4 = Convert.ToInt32(textboxGC4New.Text);
                mPConfig.GrainCoverage.GC5 = Convert.ToInt32(textboxGC5New.Text);
                SaveProgramConfigFile(mPConfig);
                GV.SetCriteria();
                stateMachine = 60;
            }

            if (stateMachine == 60)
            {
                return true;
                //DialogResult dialog = MessageBox.Show("To activate new configuration, please restart the application." +
                //        "If you would like to restart the application now, click Yes" +
                //        Environment.NewLine + "If you would like to get back to the application, click No",
                //       "Verify Info", MessageBoxButtons.YesNo);

                //if (dialog == DialogResult.Yes)
                //{
                //    userInputAcceptable = true;
                //}
                //else if (dialog == DialogResult.No)
                //{

                //}
            }
            return false;
            //return userInputAcceptable;
        }

    }
}
