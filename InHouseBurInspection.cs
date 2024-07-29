using QCBur_dll.DataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using User_Interface._1to1TraceabilityFunctions;
using User_Interface.Main;
using static QCBur_dll.DataStructures.Inspection;
using static User_Interface.SetupService;
using System.IO;
using Newtonsoft.Json;
using QCBur_dll;
using static User_Interface.Cloud.mCloud;
using System.Threading;
using static User_Interface.Properties.Settings;
using PdfSharp.Pdf;
using System.Text.RegularExpressions;
using System.IO.Compression;
using static QCBur_dll.BinSettings;
using Amazon.DynamoDBv2.DocumentModel;

namespace User_Interface
{
    public partial class InHouseBurInspection : UserControl
    {
        FilesFunctions filesFunctions = new FilesFunctions(); //Reusable Functions
        PDFFunctions pdfFunctions = new PDFFunctions(); // Allows Merging of PDF
        CSVFunctions csvFunctions = new CSVFunctions();
        PDFViewer pdfViewer = new PDFViewer(); // Class to view PDFs
        FileManagement fileManagement = new FileManagement();
        BurScan burScan = new BurScan();
        GF gf = new GF();
        Inspection burInfo = new Inspection();
        //Inspection.BurInfo[] burInfo.burInfo = new Inspection.BurInfo[40];
        Inspection.Data[] burData = new Inspection.Data[40];
        Dictionary<string, Data> allBurData = new Dictionary<string, Data>();
        //INITIALIZES GLOBAL VARIABLES
        string tempFolderPath; // root temporary folder
        List<List<string>> extractedBurData = new List<List<string>>(); // data saved from all PDFs (grain coveraged, mean grain protusion, pass/fails)
        Dictionary<string, string> availableCSVFiles = new Dictionary<string, string>();
        int grainCoverageCriteria1;// grain coverage criteria 1 (GC1) set on settings tab
        int grainCoverageCriteria2;// grain coverage criteria 1 (GC2) set on settings tab
        int grainCoverageCriteria3;// grain coverage criteria 1 (GC3) set on settings tab
        int grainCoverageCriteria4;// grain coverage criteria 1 (GC4) set on settings tab
        int grainCoverageCriteria5;// grain coverage criteria 1 (GC5) set on settings tab

        int meanGrainProtusionCriteria1; // grain coverage criteria 1 (MGP1) set on settings tab
        double[,] burResultsCsv = new double[40, 6]; //40 x 2 matrix that containts the mean grain protusion and grain coverage of the burs. 


        public InHouseBurInspection()
        {
            InitializeComponent();

            Setup();
            btnClearScreen_Click(null, null);
            ClearBurInfo();
            ClearBurResultsCsv();
        }
        private void InHouseBurInspection_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                InHouseBurInspection_Loaded(null, null);
            }
        }

        private void InHouseBurInspection_Loaded(object sender, EventArgs e)
        {
            try
            {
                //string GC1 = mRDConfig.GrainCoverage.GC1.ToString();
                //string GC2 = mRDConfig.GrainCoverage.GC2.ToString();
                string GC3 = mRDConfig.GrainCoverage.GC3.ToString();
                string GC4 = mRDConfig.GrainCoverage.GC4.ToString();
                //string GC5 = mRDConfig.GrainCoverage.GC5.ToString();
                string MGP = mRDConfig.MeanGrainProtustion.MTV1.ToString();

                //lblGrainCoverageCriteria1.Text = GC1;
                //lblGrainCoverageCriteria2.Text = GC2;
                lblGrainCoverageCriteria3.Text = GC3;
                lblGrainCoverageCriteria4.Text = GC4;
                //lblGrainCoverageCriteria5.Text = GC5;
                lblMeanGrainProtusionCriteria1.Text = MGP;

                //Int32.TryParse(GC1, out grainCoverageCriteria1);
                //Int32.TryParse(GC2, out grainCoverageCriteria2);
                Int32.TryParse(GC3, out grainCoverageCriteria3);
                Int32.TryParse(GC4, out grainCoverageCriteria4);
                //Int32.TryParse(GC5, out grainCoverageCriteria5);
                Int32.TryParse(MGP, out meanGrainProtusionCriteria1);

                tempFolderPath = mRDConfig.TempFolderName;

                lbl_assemblyVersion.Text = GF.GetThreeDigitsVersionNumber();

                if (!string.IsNullOrEmpty(gf.tempFolderPath))
                {
                    btnScanTemporaryFolder_Click(null, null);

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Setup()
        {
            burData = new Inspection.Data[40];
            for (int i = 0; i < 40; i++)
            {
                burData[i] = new Inspection.Data();
            }
        }

        private void btnScanTemporaryFolder_Click(object sender, EventArgs e)
        {
            try
            {
                availableCSVFiles = gf.ScanFolder(tempFolderPath, sender == null);
                dropdownDistinctLotsDateTime.Items.Clear();
                foreach (string distinctFile in availableCSVFiles.Keys)
                {
                    if (distinctFile.Substring(0, 2) == "42") return;
                    dropdownDistinctLotsDateTime.Items.Add(distinctFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to scan temp folder: " + ex.Message);
                gf.LogThis("RD: Unable to scan temp folder: " + ex.Message);
            }
        }



        private void btnReadPDFResults_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(dropdownDistinctLotsDateTime.Text))
            {
                MessageBox.Show("Please select the lot and datetime results");
            }
            else
            {
                StateMachine();
            }
        }
         
        private async Task<bool> SaveResults(bool bagSort)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (string.IsNullOrEmpty(dropdownDistinctLotsDateTime.Text))
            {
                MessageBox.Show("Please select a csv file");
                return false;
            }
            if (string.IsNullOrEmpty(TB_fixtureBarcode.Text))
            {
                MessageBox.Show("Scan fixture barcode");
                return false;
            }

            bool[] badScanBurResults = new bool[40];
            bool[] discardBurResults = new bool[40];

            string burSelection = SelectBurs(out badScanBurResults, out discardBurResults);
            bool confirmBurSelection = ConfirmBurSelection(burSelection);
            bool uploadFinished = false;
            if (confirmBurSelection)
            {

                UploadScans();
                do
                {
                    uploadFinished = UploadTrayData(badScanBurResults, discardBurResults, bagSort);
                } while (!uploadFinished &&
        DialogResult.Retry == MessageBox.Show("FIXTURE UPDATE FAILED: \nTo try again, press \'Retry\'. Otherwise, click \'Cancel\'", "FIXTURE UPDATE FAILED!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error));
                if (uploadFinished)
                {
                    btnSaveResults.Enabled = false;
                    TB_fixtureBarcode.Enabled = false;
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }

        }


        private string SelectBurs(out bool[] badScanBurResults, out bool[] discardBurResults)
        {
            //bool[] badScanBurResults = new bool[40];
            //bool[] discardBurResults = new bool[40];
            string confirmResults = "";
            try
            {
                var badScanSelector = new BadScanSelector();
                badScanSelector.ShowDialog();
                badScanBurResults = Form1.form1Instance.badScanBurResults;
                discardBurResults = Form1.form1Instance.discardBurResults;
                var selectedBadResults = Enumerable.Range(0, badScanBurResults.Length).Where(x => Form1.form1Instance.badScanBurResults[x]);
                var selectedDiscardResults = Enumerable.Range(0, discardBurResults.Length).Where(x => Form1.form1Instance.discardBurResults[x]);
                string confirmBadResults = "Bad Scan Bur(s): \n";
                string confirmDiscardResults = "Discard Bur(s): \n";
                for (int i = 0; i < selectedBadResults.Count(); i++)
                {
                    confirmBadResults += $"- {selectedBadResults.ElementAt(i) + 1}\n";
                }

                for (int i = 0; i < selectedDiscardResults.Count(); i++)
                {
                    confirmDiscardResults += $"- {selectedDiscardResults.ElementAt(i) + 1}\n";
                }
                confirmResults = confirmBadResults + confirmDiscardResults;
                //var msgResult = MessageBox.Show(confirmResults, "Is this correct?", MessageBoxButtons.YesNo);

                //if (msgResult != DialogResult.Yes)
                //{
                //    MessageBox.Show("Click on \'Save Results\' again to reselect bad scans");
                //    return;
                //}
                return confirmResults;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SaveResults operation failed: " + ex.Message);
                gf.LogThis("SaveResults operation failed: " + ex.ToString());
                badScanBurResults = null;
                discardBurResults = null;
                return confirmResults;
            }
        }

        private bool ConfirmBurSelection(string confirmResults)
        {
            var msgResult = MessageBox.Show(confirmResults, "Is this correct?", MessageBoxButtons.YesNo);

            if (msgResult != DialogResult.Yes)
            {
                MessageBox.Show("Click on \'Save Results\' again to reselect bad scans");
                return false;
            }
            return true;
        }
        private bool UploadTrayData(bool[] badScanBurResults, bool[] discardBurResults, bool willSortBag)
        {
            string csvFileName = availableCSVFiles[dropdownDistinctLotsDateTime.Text];
            this.progressBar1.Visible = true;
            string startTime = "";
            string stopTime = "";
            try
            {
                startTime = filesFunctions.GetProgramStartTime(dropdownDistinctLotsDateTime.Text);
                stopTime = filesFunctions.GetProgramEndTime(tempFolderPath, csvFileName);
            }
            catch (Exception ex)
            {
                gf.LogThis("GetProgramTime error: " + ex.Message);
            }
            string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
            bool successfulUpload = true;
            gf.LogThis($"Updating fixture {fixtureId}");
            try
            {
                burInfo.areAcurataBurs = false;
                burInfo.willSortBag = willSortBag;
                burInfo.fixtureId = fixtureId;
                burInfo.csvFile = csvFileName;
                burInfo.startTime = startTime;
                burInfo.stopTime = stopTime;
                burInfo.lastUpdateTime = stopTime;
                string burName = "";
                for (int i = 0; i < burData.Length; i++)
                {
                    burName = $"bur{i + 1}";
                    if (burData[i] != null && badScanBurResults[i])
                    {
                        burData[i].P10 = true;
                        burData[i].P11 = false;

                    }
                    else if (burData[i] != null && discardBurResults[i])
                    {
                        burData[i].P10 = false;
                        burData[i].P11 = true;
                    }
                    if (allBurData.ContainsKey(burName))
                    {
                        allBurData.Remove(burName);
                    }
                    allBurData.Add(burName, burData[i]);
                    int numTries = 0;
                    bool result = false;
                }
                if (!Default.IsUsingHttp)
                {
                    burInfo.allBurs = allBurData;
                    burInfo.binSettings = JsonConvert.SerializeObject(mRDBinConfig);
                    Thread.Sleep(1000);

                    gf.LogThis($"Fixture {fixtureId} information is being sent to the cloud...");

                    Dictionary<bool, string> publishResult = Program.snsClient.PublishRDMessage(JsonConvert.SerializeObject(burInfo));
                    //Dictionary<bool, string> publishResult = Program.snsClient.PublishRDMessage(JsonConvert.SerializeObject(burInfo));
                    if (publishResult != null && publishResult.Count > 0)
                    {
                        if (publishResult.ContainsKey(false))
                        {
                            gf.LogThis($"Fixture {fixtureId} information was NOT sent to the cloud: " + publishResult[false]);
                            return false;
                        }
                        else
                        {
                            gf.LogThis($"Fixture {fixtureId} information was sent to the cloud...");
                        }
                    }
                    else
                    {
                        gf.LogThis("Publish result is empty");
                        return false;
                    }


                    Thread.Sleep(15000);

                    var response = Program.dbClient.GetAllBurInfo(fixtureId, Databases.FIXTURE_DATABASE);
                    if (response != null)
                    {
                        if (!string.IsNullOrEmpty(response[41]))
                        {
                            string returnedCSVFileName = response[41];
                            if (returnedCSVFileName != availableCSVFiles[dropdownDistinctLotsDateTime.Text])
                            {
                                gf.LogThis("The CSV file name in the database is not correct");
                                return false;
                            }
                        }
                        else
                        {
                            successfulUpload = false;
                        }
                        response.Remove(41);

                        if (response != null)
                        {
                            foreach (int key in response.Keys)
                            {
                                string currBurName = "bur" + Convert.ToString(key);
                                if (!Program.dbClient.Equals(burInfo.allBurs[currBurName], JsonConvert.DeserializeObject<Data>(response[key])))
                                {
                                    gf.LogThis($"Bur info does not match for {currBurName}\n" +
                                       "BurInfo from application: " + JsonConvert.SerializeObject(burInfo.allBurs[currBurName]) + "\n" +
                                       "BurInfo from database: " + response[key]);

                                    successfulUpload = false;
                                    break;
                                }
                                else
                                {
                                    successfulUpload = true;
                                }
                            }
                        }
                        else
                        {
                            gf.LogThis($"Database does not contain bur info for fixture {fixtureId}");
                            successfulUpload = false;
                        }


                    }
                    else
                    {
                        gf.LogThis($"Database does not contain information for fixture {fixtureId}");
                        successfulUpload = false;
                    }
                    Thread.Sleep(2000);
                }

                this.progressBar1.Visible = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Bur data not saved in the cloud: " + ex.Message);
                gf.LogThis($"Bur data not saved in the cloud, fixtureId {TB_fixtureBarcode.Text}: " + ex.ToString());
                this.progressBar1.Visible = false;
                return false;
            }
            return successfulUpload;
        }


        private bool UploadBurData(bool[] badScanBurResults, bool[] discardBurResults)
        {
            string csvFileName = availableCSVFiles[dropdownDistinctLotsDateTime.Text];
            this.progressBar1.Visible = true;
            string startTime = "";
            string stopTime = "";
            try
            {
                startTime = filesFunctions.GetProgramStartTime(dropdownDistinctLotsDateTime.Text);
                stopTime = filesFunctions.GetProgramEndTime(tempFolderPath, csvFileName);
            }
            catch (Exception ex)
            {
                gf.LogThis("GetProgramTime error: " + ex.Message);
            }
            string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
            bool successfulUpload = true;
            gf.LogThis($"Updating fixture {fixtureId}");
            try
            {
                burInfo.fixtureId = fixtureId;
                burInfo.csvFile = csvFileName;
                burInfo.startTime = startTime;
                burInfo.stopTime = stopTime;
                burInfo.lastUpdateTime = stopTime;
                string burName = "";
                for (int i = 0; i < burData.Length; i++)
                {
                    burName = $"bur{i + 1}";
                    if (burData[i] != null && badScanBurResults[i])
                    {
                        burData[i].P10 = true;
                        burData[i].P11 = false;

                    }
                    else if (burData[i] != null && discardBurResults[i])
                    {
                        burData[i].P10 = false;
                        burData[i].P11 = true;
                    }
                    if (allBurData.ContainsKey(burName))
                    {
                        allBurData.Remove(burName);
                    }
                    allBurData.Add(burName, burData[i]);
                    int numTries = 0;
                    bool result = false;
                }
                if (!Default.IsUsingHttp)
                {
                    burInfo.allBurs = allBurData;
                    burInfo.binSettings = JsonConvert.SerializeObject(mRDBinConfig);
                    Thread.Sleep(1000);

                    gf.LogThis($"Fixture {fixtureId} information is being sent to the cloud...");

                    //Dictionary<bool, string> publishResult = Program.snsClient.PublishRDMessage(JsonConvert.SerializeObject(burInfo));
                    Dictionary<bool, string> publishResult = Program.snsClient.PublishRDMessage(JsonConvert.SerializeObject(burInfo));
                    if (publishResult != null && publishResult.Count > 0)
                    {
                        if (publishResult.ContainsKey(false))
                        {
                            gf.LogThis($"Fixture {fixtureId} information was NOT sent to the cloud: " + publishResult[false]);
                            return false;
                        }
                        else
                        {
                            gf.LogThis($"Fixture {fixtureId} information was sent to the cloud...");
                        }
                    }
                    else
                    {
                        gf.LogThis("Publish result is empty");
                        return false;
                    }
                    Thread.Sleep(15000);

                    var response = Program.dbClient.GetAllBurInfo(fixtureId, Databases.FIXTURE_DATABASE);
                    if (response != null)
                    {
                        if (!string.IsNullOrEmpty(response[41]))
                        {
                            string returnedCSVFileName = response[41];
                            if (returnedCSVFileName != availableCSVFiles[dropdownDistinctLotsDateTime.Text])
                            {
                                gf.LogThis("The CSV file name in the database is not correct");
                                return false;
                            }
                        }
                        else
                        {
                            successfulUpload = false;
                        }
                        response.Remove(41);

                        if (response != null)
                        {
                            foreach (int key in response.Keys)
                            {
                                string currBurName = "bur" + Convert.ToString(key);
                                if (!Program.dbClient.Equals(burInfo.allBurs[currBurName], JsonConvert.DeserializeObject<Data>(response[key])))
                                {
                                    gf.LogThis($"Bur info does not match for {currBurName}\n" +
                                       "BurInfo from application: " + JsonConvert.SerializeObject(burInfo.allBurs[currBurName]) + "\n" +
                                       "BurInfo from database: " + response[key]);

                                    successfulUpload = false;
                                    break;
                                }
                                else
                                {
                                    successfulUpload = true;
                                }
                            }
                        }
                        else
                        {
                            gf.LogThis($"Database does not contain bur info for fixture {fixtureId}");
                            successfulUpload = false;
                        }


                    }
                    else
                    {
                        gf.LogThis($"Database does not contain information for fixture {fixtureId}");
                        successfulUpload = false;
                    }
                    Thread.Sleep(2000);
                }

                this.progressBar1.Visible = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Bur data not saved in the cloud: " + ex.Message);
                gf.LogThis($"Bur data not saved in the cloud, fixtureId {TB_fixtureBarcode.Text}: " + ex.ToString());
                this.progressBar1.Visible = false;
                return false;
            }
            return successfulUpload;
        }

        private async void btnSaveResults_Click(object sender, EventArgs e)
        {
            string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
            bool updated = false;
            int numTries = 0;
            if (checkBox_bag.Checked)
            {
                DialogResult result = MessageBox.Show("Will these burs be sorted or bagged?", "Confirm Operation", MessageBoxButtons.YesNo);

                switch (result)
                {
                    case DialogResult.Yes:
                        do
                        {
                            updated = await SaveResults(true);
                            // update tray database
                        }
                        while (!updated && numTries < GF.MAX_FAILURES);
                        if (updated)
                        {
                            MessageBox.Show($"Successful operation: Fixture {fixtureId} saved in the cloud");
                        }
                        else if (numTries >= GF.MAX_FAILURES)
                        {
                            MessageBox.Show($"ERROR: Fixture {fixtureId} was not saved in the cloud");
                        }
                        break;
                    case DialogResult.No:
                        MessageBox.Show("\'No\' Selected: No action will be taken");
                        break;
                }
            }
            else
            {
                DialogResult result = MessageBox.Show($"Select \'OK\' if the burs in fixture {fixtureId} won't be sorted or bagged.", "Confirm Operation", MessageBoxButtons.OKCancel);

                switch (result)
                {
                    case DialogResult.OK:
                        do
                        {
                            updated = await SaveResults(false);
                            // update tray database
                        }
                        while (!updated && numTries < GF.MAX_FAILURES);
                        if (updated)
                        {
                            MessageBox.Show($"Successful operation: Fixture {fixtureId} saved in the cloud");
                        }
                        else if (numTries >= GF.MAX_FAILURES)
                        {
                            MessageBox.Show($"ERROR: Fixture {fixtureId} was not saved in the cloud");
                        }
                        break;

                }
            }
        }

        private void btnClearScreen_Click(object sender, EventArgs e)
        {
            btnOpenPDFReader.Enabled = false;
            btnClearScreen.Enabled = false;
            btnScanTemporaryFolder.Enabled = true;
            dropdownDistinctLotsDateTime.Enabled = true;
            dropdownDistinctLotsDateTime.Text = string.Empty;
            btnScanTemporaryFolder_Click(null, null);
            btnReadPDFResults.Enabled = true;
            btnSaveResults.Enabled = false;
            TB_fixtureBarcode.Clear();
            TB_fixtureBarcode.Enabled = false;

            for (int i = 0; i < 40; i++)
            {
                string buttonName = "btn_Bur" + (i + 1).ToString();
                string mtvLabelName = "lbl_MTVBur" + (i + 1).ToString();
                string gcLabelName = "lbl_GCBur" + (i + 1).ToString();

                panel_Bur_Buttons.Controls[gcLabelName].ForeColor = Color.Gray;
                panel_Bur_Buttons.Controls[gcLabelName].Text = "n/a";

                panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";

                panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
            }
        }

        private void btnOpenPDFReader_Click(object sender, EventArgs e)
        {
            List<string> filesToMerge = new List<string>(); // will list all pdf file paths to be merged based on drop down menu selection
            string scanFileName = availableCSVFiles[dropdownDistinctLotsDateTime.Text];
            filesToMerge = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf");

            if (filesToMerge != null)
            {
                string mergedFilePath = pdfFunctions.MergePDFs(filesToMerge, "Merged", tempFolderPath);
                if (mergedFilePath == null)
                {
                    MessageBox.Show("Unable to merge PDFs or find them. Please make sure files are in root location, with standard name." +
                        "also ensure that all PDFs are closed", "PDF Merging Failed");
                }
                else
                {
                    try
                    {
                        //open merged PDF
                        if (File.Exists(mergedFilePath) == true)
                        {
                            System.Diagnostics.Process.Start(mergedFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to open merged file", "Merged PDF not found");
                        gf.LogThis("Unable to open merged file: " + ex.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Unable to merge PDFs or find them. Please make sure files are in the root location, in the standard format, and that all PDFs are closed.");
            }
        }

        private void StateMachine()
        {
            if (string.IsNullOrEmpty(dropdownDistinctLotsDateTime.Text))
            {
                return;
            }
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                int stateMachine = 0; // state machine 
                string fileName = availableCSVFiles[dropdownDistinctLotsDateTime.Text];
                string fullFilePath = tempFolderPath + "\\" + fileName + ".csv"; //full csv file path of select scan on drop down menu
                string[] burResults = new string[40];
                string[] meanGrainProtusionResults = new string[40];
                string[] grainCoverageResults = new string[40];
                //BUR RESULTS MATRIX
                //40 x 2 matrix where the row represents the bur, 1st col represents MGP, 2nd col represents GP.
                //If there is no bur in that position, values will be 0
                //double[,] burResultsCsv = new double[40, 2];
                ClearBurInfo();
                ClearBurResultsCsv();
                //burResultsCsv = burScan.RetrieveBurResultsFromCSV(fullFilePath);

                burResultsCsv = burScan.GetBurResults(fullFilePath);
                string[] burScanFile = fileManagement.ExtractDataFromCsvFileName(fileName);
                //for (int i = 0; i < 40; i++)
                //{
                //    burData[i] = new Inspection.BurInfo();
                //}
                //STATE 0: STARTING STATE MACHINE
                //CHECK IF LOT NUMBER HAS THE EXPECTED FORMAT
                if (stateMachine == 0)
                {
                    if (burScanFile[0].Length != 10)
                    {
                        LotNumberCorrectorForm correctorForm = new LotNumberCorrectorForm(burScanFile[0]);
                        correctorForm.ShowDialog();
                        fileManagement.RenameFiles(tempFolderPath, burScanFile[0], Form1.form1Instance.correctLotNumber, false);

                        string csvFileNameInDict = availableCSVFiles[dropdownDistinctLotsDateTime.Text];
                        string correctCSVFileName = csvFileNameInDict.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        //availableCSVFiles[dropdownDistinctLotsDateTime.Text] = availableCSVFiles[dropdownDistinctLotsDateTime.Text].Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        availableCSVFiles.Remove(dropdownDistinctLotsDateTime.Text);
                        dropdownDistinctLotsDateTime.Text = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        availableCSVFiles.Add(dropdownDistinctLotsDateTime.Text, correctCSVFileName);
                        //dropdownDistinctLotsDateTime.Text = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        burScanFile[0] = Form1.form1Instance.correctLotNumber;
                    }
                    stateMachine = 10;

                }

                //STATE 10: POPULATING SCREEN WITH RESULTS
                if (stateMachine == 10)
                {
                    string burType = burScanFile[0].Substring(0, 2);

                    int lotNumberId = 0;
                    try
                    {
                        lotNumberId = Int32.Parse(burType);
                    }
                    catch
                    {
                        lotNumberId = 0;
                        burType = "00";
                    }

                    BinConfig binConfig = Program.dbClient.GetConfig(burScanFile[0], burType);

                    SetupService.SaveRDBinConfig(binConfig); 
                    for (int i = 0; i < 40; i++)
                    {
                        string buttonName = "btn_Bur" + (i + 1).ToString();
                        string mtvLabelName = "lbl_MTVBur" + (i + 1).ToString();
                        string cgLabelName = "lbl_GCBur" + (i + 1).ToString();

                        //Mean Tip Value Pass/Fail Criteria
                        if (burResultsCsv[i, 1] > meanGrainProtusionCriteria1)
                        {
                            meanGrainProtusionResults[i] = "Fail";
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Brown;
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = burResultsCsv[i, 1].ToString();
                        }
                        else if (burResultsCsv[i, 1] <= meanGrainProtusionCriteria1 && burResultsCsv[i, 1] > 0)
                        {
                            meanGrainProtusionResults[i] = "Pass";
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.MediumSeaGreen;
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = burResultsCsv[i, 1].ToString();
                        }
                        else
                        {
                            meanGrainProtusionResults[i] = "Undetermined";
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                        }


                        //Grain Coverage Pass/Fail Criteria
                        #region prev
                        //if (burResultsCsv[i, 0] < grainCoverageCriteria1 && burResultsCsv[i, 0] > 0)
                        //{
                        //    grainCoverageResults[i] = "Fail";
                        //    panel_Bur_Buttons.Controls[cgLabelName].Text = burResultsCsv[i, 0].ToString();
                        //    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Red;
                        //}

                        //else if (burResultsCsv[i, 0] >= grainCoverageCriteria5 ||
                        //    (burResultsCsv[i, 0] >= grainCoverageCriteria1 && burResultsCsv[i, 0] < grainCoverageCriteria2))
                        //{
                        //    grainCoverageResults[i] = "Quarantine";
                        //    panel_Bur_Buttons.Controls[cgLabelName].Text = burResultsCsv[i, 0].ToString();
                        //    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Orange;
                        //}
                        //// both BENA and BNOW means pass, but showing different color
                        //else if ((burResultsCsv[i, 0] >= grainCoverageCriteria4 && burResultsCsv[i, 0] < grainCoverageCriteria5) ||
                        //    (burResultsCsv[i, 0] >= grainCoverageCriteria2 && burResultsCsv[i, 0] < grainCoverageCriteria3))
                        //{
                        //    grainCoverageResults[i] = "BENA";
                        //    panel_Bur_Buttons.Controls[cgLabelName].Text = burResultsCsv[i, 0].ToString();
                        //    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.DodgerBlue;
                        //}

                        //else if (burResultsCsv[i, 0] >= grainCoverageCriteria3 &&
                        //    burResultsCsv[i, 0] < grainCoverageCriteria4)
                        //{
                        //    grainCoverageResults[i] = "BNOW";
                        //    panel_Bur_Buttons.Controls[cgLabelName].Text = burResultsCsv[i, 0].ToString();
                        //    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.MediumSeaGreen;
                        //}
                        #endregion
                        if ((burResultsCsv[i, 0] < grainCoverageCriteria3 || burResultsCsv[i,0] > grainCoverageCriteria4) && burResultsCsv[i, 0] > 0)
                        {
                            grainCoverageResults[i] = "Fail";
                            panel_Bur_Buttons.Controls[cgLabelName].Text = burResultsCsv[i, 0].ToString();
                            panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Red;
                        }
                        else if (burResultsCsv[i, 0] >= grainCoverageCriteria3 &&
                            burResultsCsv[i, 0] < grainCoverageCriteria4)
                        {
                            grainCoverageResults[i] = "BNOW";
                            panel_Bur_Buttons.Controls[cgLabelName].Text = burResultsCsv[i, 0].ToString();
                            panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.MediumSeaGreen;
                        }
                        else
                        {
                            grainCoverageResults[i] = "Undetermined";
                            panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";
                            panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Gray;
                        }

                        //Filling Bur Results

                        if (grainCoverageResults[i] == "BNOW" && meanGrainProtusionResults[i] == "Pass")
                        {
                            burResults[i] = "BNOW";
                            panel_Bur_Buttons.Controls[buttonName].BackColor = Color.MediumSeaGreen;
                        }

                        //else if (grainCoverageResults[i] == "BENA" && meanGrainProtusionResults[i] == "Pass")
                        //{
                        //    burResults[i] = "BENA";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.DodgerBlue;
                        //}

                        else if (grainCoverageResults[i] == "Fail" || meanGrainProtusionResults[i] == "Fail")
                        {
                            burResults[i] = "Fail";
                            panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Brown;
                        }

                        //else if (grainCoverageResults[i] == "Quarantine" && meanGrainProtusionResults[i] == "Pass")
                        //{
                        //    burResults[i] = "Quarantine";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Orange;
                        //}

                        else
                        {
                            burResults[i] = "Undetermined";
                            //panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
                            panel_Bur_Buttons.Controls[cgLabelName].ForeColor = ColorTranslator.FromHtml(mRDBinConfig.GCBins[0].color);

                        }

                        //if (burResults[i] != "Undetermined")
                        //{
                        try
                        {

                            burData[i].P1 = burResultsCsv[i, 0].ToString();
                            burData[i].P2 = burResultsCsv[i, 1].ToString();
                            burData[i].P3 = burResultsCsv[i, 2].ToString();
                            burData[i].P4 = burResultsCsv[i, 3].ToString();
                            burData[i].P5 = burResultsCsv[i, 4].ToString();
                            burData[i].P6 = burResultsCsv[i, 5].ToString();
                            burData[i].P7 = (burResults[i] != "Undetermined") ? fileName + $"_Position{i + 1}.pdf" : "";
                            burData[i].P8 = burScanFile[0];
                            burData[i].P9 = burResults[i];
                            burData[i].P13 = gf.DetermineRDBin(burResultsCsv[i, 0], burResultsCsv[i, 1]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Update BurInfo error: " + ex.Message);
                            gf.LogThis("Update BurInfo error: " + ex.ToString());
                        }
                        //}


                    }
                    stateMachine = 1000;
                }

                //STATE 1000: Exiting State
                if (stateMachine == 1000)
                {
                    btnOpenPDFReader.Enabled = true;
                    btnReadPDFResults.Enabled = false;
                    btnClearScreen.Enabled = true;
                    btnScanTemporaryFolder.Enabled = false;
                    dropdownDistinctLotsDateTime.Enabled = false;
                    btnSaveResults.Enabled = true;
                    TB_fixtureBarcode.Enabled = true;
                }

                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
                gf.LogThis($"Bur data processing took {stopwatch.ElapsedMilliseconds}ms, csvFile: {fileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to process bur data: " + ex.Message);
                gf.LogThis("Unable to process bur data: " + ex.ToString());
            }
        }

        private void UploadScans()
        {
            string scannedFilesZipDir = Path.Combine(tempFolderPath, Path.GetFileNameWithoutExtension(availableCSVFiles[dropdownDistinctLotsDateTime.Text]));

            // Create main and sub directories
            if (!Directory.Exists(scannedFilesZipDir))
            {
                Directory.CreateDirectory(scannedFilesZipDir);
            }
            string goodScansZipDir = Path.Combine(scannedFilesZipDir, "Good Scans");
            string badScansZipDir = Path.Combine(scannedFilesZipDir, "Bad Scans");
            Directory.CreateDirectory(goodScansZipDir);
            Directory.CreateDirectory(badScansZipDir);

            List<string> files = new List<string>();

            List<string> pdfs = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf");

            if (pdfs != null)
            {
                foreach (string pdf in pdfs)
                {
                    files.Add(pdf);
                }
            }
            List<string> csvs = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "csv");
            if (csvs != null)
            {
                foreach (string csv in csvs)
                {
                    files.Add(csv);
                }
            }
          

            if (files == null) return;

            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".pdf")
                {
                    string fileNoExt = Path.GetFileNameWithoutExtension(file);
                    string burIndexStr = Regex.Split(fileNoExt, "Position")[1];
                    Int32.TryParse(burIndexStr, out int burIndex);
                    if (!Form1.form1Instance.badScanBurResults[burIndex - 1])
                    {
                        File.Copy(file, $"{goodScansZipDir}\\{Path.GetFileName(file)}", true);

                    }
                    else
                    {
                        File.Copy(file, $"{badScansZipDir}\\{Path.GetFileName(file)}", true);

                    }
                }
                else if (Path.GetExtension(file) == ".csv")
                {
                    File.Copy(file, $"{scannedFilesZipDir}\\{Path.GetFileName(file)}", true);
                }
                File.Delete(file);
            }
            if (!File.Exists(scannedFilesZipDir + ".zip")) ZipFile.CreateFromDirectory(scannedFilesZipDir, scannedFilesZipDir + ".zip", CompressionLevel.Fastest, false);

            UploadInHouseZips(scannedFilesZipDir +  ".zip");

        }

        private async void UploadInHouseZips(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string lotNumber = gf.GetLotNumber(fileName);

            if (lotNumber.Substring(0, 2) == "42") return;
            try
            {
                await Task.Run(() => Program.s3Client.UploadInHouseFileMultiPartAsync($"{lotNumber}/{fileName}", filePath));
            }
            catch (Exception ex)
            {

            }
        }

        private void ClearBurResultsCsv()
        {
            for (int i = 0; i < 40; i++)
            {
                burResultsCsv[i, 0] = 0;
                burResultsCsv[i, 1] = 0;

            }
        }

        private void ClearBurInfo()
        {
            burInfo.fixtureId = "";
            burInfo.csvFile = "";
            burInfo.stopTime = "";
            burInfo.startTime = "";
            burInfo.lastUpdateTime = "";
            if (burInfo.allBurs != null)
            {
                burInfo.allBurs = new Dictionary<string, Data>();
            }
            for (int i = 0; i < burData.Length; i++)
            {
                burData[i].P1 = "";
                burData[i].P2 = "";
                burData[i].P3 = "";
                burData[i].P4 = "";
                burData[i].P5 = "";
                burData[i].P6 = "";
                burData[i].P7 = "";
                burData[i].P8 = "";
                burData[i].P9 = "";
                burData[i].P10 = false;
                burData[i].P11 = false;
                burData[i].P12 = false;
                burData[i].P13 = "";
            }
            //burInfo.allBurs = allBurData;
        }

        private void dropdownDistinctLotsDateTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            StateMachine();
        }

       
    }
}
