//Engineer: Luis Henrique Accioly Gastao
//Date: March 5th, 2022
//Version: 1.2
//Company: Glidewell Laboratories

//NOTE: "Mean Grain Protrusion" (MGP) used to be called "Mean Tip Value" (MTV). Therefore, all user interfacing values have been changed to show MGP. 
//However, all values in the code will show "Mean Tip Value" (MTV). MTV and MGP are the same thing. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using QCBur_dll;
using QCBur_dll.DataStructures;
using User_Interface._1to1TraceabilityFunctions;
using static User_Interface.Main.GF;
using User_Interface.Main;
using Newtonsoft.Json;
using PdfSharp.Fonts;
using static User_Interface.Properties.Settings;
using static User_Interface.Cloud.mCloud;
using static QCBur_dll.DataStructures.Inspection;
using Amazon.CognitoIdentityProvider.Model.Internal.MarshallTransformations;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using static System.IO.Packaging.Package;
using Amazon.S3;
using System.Security.AccessControl;
using System.Security.Permissions;
using PdfSharp.Internal;
using Amazon.SecurityToken.Model;
using static User_Interface.SetupService;
using static QCBur_dll.BinSettings;
using Amazon.DynamoDBv2.DocumentModel;

namespace User_Interface
{
    public partial class HomeUserControl : UserControl
    {
        //INTIALIZE CLASSES
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

        string selectedFileName = "";
        public HomeUserControl()
        {
            InitializeComponent();

            Setup();

            ClearBurInfo();
            ClearBurResultsCsv();
            TB_fixtureBarcode.TextChanged += TB_fixtureBarcode_TextChanged;
        }

        private void Setup()
        {
            burData = new Inspection.Data[40];
            for (int i = 0; i < 40; i++)
            {
                burData[i] = new Inspection.Data();
            }
        }

        private async void HandleZippedFiles()
        {
            try
            {
                string[] zippedFiles = Directory.GetFiles(tempFolderPath, "*.zip");
                foreach (string file in zippedFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string lotNumber = gf.GetLotNumber(fileName);

                    if (lotNumber.Substring(0, 2) != "42")
                    {
                        await Task.Run(() => Program.s3Client.UploadInHouseFileMultiPartAsync($"{lotNumber}/{fileName}", file));
                    }
                    else
                    {
                        if (!file.ToUpper().Contains("SIDE"))
                        {
                            await Task.Run(() => Program.s3Client.UploadFileMultiPartAsync(false, $"{lotNumber}/{fileName}", file, "STANDARD"));
                        }
                        else
                        {
                            await Task.Run(() => Program.s3Client.UploadFileMultiPartAsync(true, $"{lotNumber}/{fileName}", file, "STANDARD"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void HandleCSVFiles()
        {
            try
            {
                string[] csvFiles = Directory.GetFiles(tempFolderPath, "*.csv");
                foreach (string file in csvFiles)
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.ToUpper().Contains("SIDE"))
                    {
                        string lotNumber = gf.GetLotNumber(fileName);
                        await Task.Run(() => Program.s3Client.UploadFileMultiPartAsync(true, $"{lotNumber}/{fileName}", file, "STANDARD"));
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        private string GetLotNumber(string fileName)
        {
            try
            {
                string[] scanInfo = fileName.Split('_');

                return scanInfo[1];
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private async void UploadZipFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string lotNumber = gf.GetLotNumber(fileName);
            try
            {
                await Task.Run(() => Program.s3Client.UploadFileMultiPartAsync(false, $"{lotNumber}/{fileName}", filePath, "STANDARD"));
            }
            catch (Exception ex)
            {

            }
        }
        private void HomeUserControl_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                HomeUserControl_Load(null, null);
            }
        }
        //LOAD HOME SCREN
        private void HomeUserControl_Load(object sender, EventArgs e)
        {
            //Retrive pass and fail criteria from txt file. 
            try
            {
                //Retrive criteria values
                string grainCoverageCriteria1String = mPConfig.GrainCoverage.GC1.ToString();
                string grainCoverageCriteria2String = mPConfig.GrainCoverage.GC2.ToString();
                string grainCoverageCriteria3String = mPConfig.GrainCoverage.GC3.ToString();
                string grainCoverageCriteria4String = mPConfig.GrainCoverage.GC4.ToString();
                string grainCoverageCriteria5String = mPConfig.GrainCoverage.GC5.ToString();
                string meanGrainProtusionCriteria1String = mPConfig.MeanGrainProtustion.MTV1.ToString();

                //Display criteria values
                //lblGrainCoverageCriteria1.Text = grainCoverageCriteria1String;
                //lblGrainCoverageCriteria2.Text = grainCoverageCriteria2String;
                //lblGrainCoverageCriteria3.Text = grainCoverageCriteria3String;
                //lblGrainCoverageCriteria4.Text = grainCoverageCriteria4String;
                //lblGrainCoverageCriteria5.Text = grainCoverageCriteria5String;
                //lblMeanGrainProtusionCriteria1.Text = meanGrainProtusionCriteria1String;

                //Parse criteria values to integers 
                Int32.TryParse(grainCoverageCriteria1String, out grainCoverageCriteria1);
                Int32.TryParse(grainCoverageCriteria2String, out grainCoverageCriteria2);
                Int32.TryParse(grainCoverageCriteria3String, out grainCoverageCriteria3);
                Int32.TryParse(grainCoverageCriteria4String, out grainCoverageCriteria4);
                Int32.TryParse(grainCoverageCriteria5String, out grainCoverageCriteria5);
                Int32.TryParse(meanGrainProtusionCriteria1String, out meanGrainProtusionCriteria1);

                //Create folde paths based on txt file constumables
                tempFolderPath = mPConfig.TempFolderName;


                lbl_assemblyVersion.Text = GF.GetThreeDigitsVersionNumber();
                HandleZippedFiles();
                HandleCSVFiles();
            }
            catch (Exception error)
            {

                Console.WriteLine(error);
                gf.LogThis("Error when loading HomeUserControl form: " + error.ToString());
                /*
                DialogResult dialog = MessageBox.Show("Unabled to read pass fail criteria and/or root folder from txt file." +
                "If the PDFs are moved from the temp folder, you will no longer be able to see results on this screen. \n \n" + error,
                    "Configurations Text File Error");
                    */
            }

            //preare visualization (buttons, pdf viewer, etc)
            pdfViewer.Hide();
            btnMoveDeleteFiles.Enabled = false;
            btnOpenPDFReader.Enabled = false;
            btnClearScreen.Enabled = false;
            btnReadPDFResults.Enabled = true;
            btnScanTemporaryFolder.Enabled = true;
            dropdownDistinctLotsDateTime.Enabled = true;
            btnSaveResults.Enabled = false;
            TB_fixtureBarcode.Enabled = false;
        }

        //RESET BUTTON
        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear_All();
        }

        //BUTTON TO SCAN TEMP FOLDER TO FIND DISTINCT DATE_TIME and LOT NUMBRES
        private void btnScanTempFolder_Click(object sender, EventArgs e)
        {
            try
            {
                availableCSVFiles = gf.ScanFolder(tempFolderPath, sender == null);
                dropdownDistinctLotsDateTime.Items.Clear(); // clear any items on drop down menu currently
                foreach (string distinctCsvFile in availableCSVFiles.Keys)
                {
                    //Console.WriteLine(distinctCsvFile.Substring(0, 2));
                    //if (distinctCsvFile.Substring(0,2) == "42")
                    //{
                    dropdownDistinctLotsDateTime.Items.Add(distinctCsvFile); // populate drop down list 

                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to scan temp folder: " + ex.Message);
                gf.LogThis("Unable to scan temp folder: " + ex.ToString());
            }
        }

        //BUTTON TO MERGE ALL PDFS OF SELECTED DATE_TIME AND LOT, AND OPEN MERGED PDF
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

        //BUTTON TO READ PDF RESULTS
        private void btnReadResults_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(dropdownDistinctLotsDateTime.Text))
            {
                MessageBox.Show("Please select the lot and datetime results you want to results results from", "Required fields are empty");
            }
            else
            {
                StateMachine();
            }
        }

        private void PopulateBurCriteria()
        {
            try
            {
                DG_binSettings.Rows.Clear();
                int row = 0;
                for (int i = 0; i < mBinConfig.GCBins.Length; i++)
                {
                    if (!string.IsNullOrEmpty(mBinConfig.GCBins[i].bin))
                    {
                        string[] criteria = { mBinConfig.GCBins[i].bin, mBinConfig.GCBins[i].lowerGC.ToString(), mBinConfig.GCBins[i].upperGC.ToString(), mBinConfig.GCBins[i].burCategory };
                        DG_binSettings.Rows.Add(criteria);
                        DG_binSettings.Rows[row].Cells[3].Style.BackColor = ColorTranslator.FromHtml(mBinConfig.GCBins[i].color);
                        row++;
                    }
                }
                DG_MGP.Rows.Clear();
                string[] mgp = { mBinConfig.MGPCutoffBin.bin, mBinConfig.MGPCutoffBin.MGP.ToString(), mBinConfig.MGPCutoffBin.burCategory };
                DG_MGP.Rows.Add(mgp);
                DG_MGP.Rows[0].Cells[2].Style.BackColor = ColorTranslator.FromHtml(mBinConfig.MGPCutoffBin.color);

                //lblMeanGrainProtusionCriteria1.Text = mBinConfig.MGPCutoffBin.MGP.ToString();

                if (DG_binSettings.Rows.Count == 0)
                {
                    MessageBox.Show("Please make sure bin settings for the lot or bur type have been configured on the dashboard!");
                }

                DG_binSettings.ClearSelection();
                DG_MGP.ClearSelection();

            }
            catch (Exception )
            {

            }
       
        }

        //Inspection burInfo = new Inspection();
        ////Inspection.BurInfo[] burInfo.burInfo = new Inspection.BurInfo[40];
        //Inspection.BurInfo[] burData = new Inspection.BurInfo[40];
        //STATE MACHINE TO RETRIEVE ALL VALUES FROM ALL PDFS
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
                string selectedDropDownText = dropdownDistinctLotsDateTime.Text;
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
                selectedFileName = dropdownDistinctLotsDateTime.Text;
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

                        btnScanTempFolder_Click(null, null);

                        string updatedName = selectedDropDownText.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        dropdownDistinctLotsDateTime.Text = updatedName;
                        burScanFile[0] = Form1.form1Instance.correctLotNumber;


                        //string csvFileNameInDict = availableCSVFiles[dropdownDistinctLotsDateTime.Text];
                        //string correctCSVFileName = csvFileNameInDict.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        ////availableCSVFiles[dropdownDistinctLotsDateTime.Text] = availableCSVFiles[dropdownDistinctLotsDateTime.Text].Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        //availableCSVFiles.Remove(dropdownDistinctLotsDateTime.Text);
                        //dropdownDistinctLotsDateTime.Invalidate();
                        ////dropdownDistinctLotsDateTime.Text = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        //selectedFileName = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        //burScanFile[0] = Form1.form1Instance.correctLotNumber;
                        ////dropdownDistinctLotsDateTime.Text = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        //dropdownDistinctLotsDateTime.Text = selectedFileName;
                        //availableCSVFiles.Add(dropdownDistinctLotsDateTime.Text, correctCSVFileName);
                        ////dropdownDistinctLotsDateTime.Text = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        ////burScanFile[0] = Form1.form1Instance.correctLotNumber;
                        ////selectedFileName = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);


                    }
                    stateMachine = 10;

                }

                //STATE 10: POPULATING SCREEN WITH RESULTS
                if (stateMachine == 10)
                {
                    //lotNumber.Substring(0, 2) != "42"
                    string burType = burScanFile[0].Substring(0, 2);
                    try
                    {
                        Int32.Parse(burType);
                    }
                    catch
                    {
                        burType = "00";
                    }
                    BinConfig binConfig = Program.dbClient.GetConfig(burScanFile[0], burType);

                    SetupService.SaveBinConfig(binConfig);

                    PopulateBurCriteria();

                    for (int i = 0; i < 40; i++)
                    {
                        string buttonName = "btn_Bur" + (i + 1).ToString();
                        string mtvLabelName = "lbl_MTVBur" + (i + 1).ToString();
                        string cgLabelName = "lbl_GCBur" + (i + 1).ToString();

                        double burMeanGrain = burResultsCsv[i, 1];
                        double burGrainCov = burResultsCsv[i, 0];
                        string burBin = "";
                        if (burMeanGrain > mBinConfig.MGPCutoffBin.MGP)
                        {
                            meanGrainProtusionResults[i] = "FAIL";
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = ColorTranslator.FromHtml(mBinConfig.MGPCutoffBin.color);
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = burMeanGrain.ToString();
                            panel_Bur_Buttons.Controls[buttonName].BackColor = ColorTranslator.FromHtml(mBinConfig.MGPCutoffBin.color);
                            burResults[i] = mBinConfig.MGPCutoffBin.burCategory;
                        }
                        else if (burMeanGrain <= mBinConfig.MGPCutoffBin.MGP && burMeanGrain > 0)
                        {
                            meanGrainProtusionResults[i] = mBinConfig.MGPCutoffBin.burCategory;
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.MediumSeaGreen;
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = burMeanGrain.ToString();
                        }
                        else
                        {
                            meanGrainProtusionResults[i] = "Undetermined";
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                        }

                        for (int bin = 0; bin < mBinConfig.GCBins.Length; bin++)
                        {
                            if (mBinConfig.GCBins[bin].lowerGC <= burGrainCov && burGrainCov < mBinConfig.GCBins[bin].upperGC &&
                                 burMeanGrain <= mBinConfig.MGPCutoffBin.MGP && burMeanGrain > 0)
                            {
                                grainCoverageResults[i] = mBinConfig.GCBins[bin].burCategory;
                                burResults[i] = mBinConfig.GCBins[bin].burCategory;
                                panel_Bur_Buttons.Controls[cgLabelName].Text = burGrainCov.ToString();
                                panel_Bur_Buttons.Controls[cgLabelName].ForeColor = ColorTranslator.FromHtml(mBinConfig.GCBins[bin].color);
                                panel_Bur_Buttons.Controls[buttonName].BackColor = ColorTranslator.FromHtml(mBinConfig.GCBins[bin].color);
                                break;
                            }
                            else if (mBinConfig.GCBins[bin].lowerGC <= burGrainCov && burGrainCov < mBinConfig.GCBins[bin].upperGC && burMeanGrain > mBinConfig.MGPCutoffBin.MGP)
                            {
                                grainCoverageResults[i] = mBinConfig.MGPCutoffBin.burCategory;
                                burResults[i] = mBinConfig.MGPCutoffBin.burCategory;
                                panel_Bur_Buttons.Controls[cgLabelName].Text = burGrainCov.ToString();
                                panel_Bur_Buttons.Controls[cgLabelName].ForeColor = ColorTranslator.FromHtml(mBinConfig.GCBins[bin].color);
                                panel_Bur_Buttons.Controls[buttonName].BackColor = ColorTranslator.FromHtml(mBinConfig.MGPCutoffBin.color);
                                //    burResults[i] = "Fail";
                                //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Brown;
                                break;
                            }
                            else
                            {
                                grainCoverageResults[i] = "Undetermined";
                                burResults[i] = "Undetermined";
                                panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";
                                panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Gray;
                                panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
                            }
                        }

                        //Filling Bur Results




                        //if (grainCoverageResults[i] == "BNOW" && meanGrainProtusionResults[i] == mBinConfig.MGPCutoffBin.burCategory)
                        //{
                        //    burResults[i] = "BNOW";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.MediumSeaGreen;
                        //}
                        //else if (grainCoverageResults[i] == "BENA" && meanGrainProtusionResults[i] == mBinConfig.MGPCutoffBin.burCategory)
                        //{
                        //    burResults[i] = "BENA";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.DodgerBlue;
                        //}
                        //else if (grainCoverageResults[i] == "Fail" || meanGrainProtusionResults[i] == "FAIL")
                        //{
                        //    burResults[i] = "Fail";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Brown;
                        //}
                        //else if (grainCoverageResults[i] == "Quarantine" && meanGrainProtusionResults[i] == mBinConfig.MGPCutoffBin.burCategory)
                        //{
                        //    burResults[i] = "Quarantine";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Orange;
                        //}
                        //else
                        //{
                        //    burResults[i] = "Undetermined";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
                        //}

                        #region prev
                        //Mean Tip Value Pass/Fail Criteria
                        //if (burResultsCsv[i, 1] > meanGrainProtusionCriteria1)
                        //{
                        //    meanGrainProtusionResults[i] = "Fail";
                        //    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Brown;
                        //    panel_Bur_Buttons.Controls[mtvLabelName].Text = burResultsCsv[i, 1].ToString();
                        //}
                        //else if (burResultsCsv[i, 1] <= meanGrainProtusionCriteria1 && burResultsCsv[i, 1] > 0)
                        //{
                        //    meanGrainProtusionResults[i] = "Pass";
                        //    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.MediumSeaGreen;
                        //    panel_Bur_Buttons.Controls[mtvLabelName].Text = burResultsCsv[i, 1].ToString();
                        //}
                        //else
                        //{
                        //    meanGrainProtusionResults[i] = "Undetermined";
                        //    panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";
                        //    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                        //}

                        //Grain Coverage Pass/Fail Criteria
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
                        //else
                        //{
                        //    grainCoverageResults[i] = "Undetermined";
                        //    panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";
                        //    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Gray;
                        //}
                        #endregion

                        //if (grainCoverageResults[i] == "BNOW" && meanGrainProtusionResults[i] == "Pass")
                        //{
                        //    burResults[i] = "BNOW";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.MediumSeaGreen;
                        //}

                        //else if (grainCoverageResults[i] == "BENA" && meanGrainProtusionResults[i] == "Pass")
                        //{
                        //    burResults[i] = "BENA";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.DodgerBlue;
                        //}

                        //else if (grainCoverageResults[i] == "Fail" || meanGrainProtusionResults[i] == "Fail")
                        //{
                        //    burResults[i] = "Fail";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Brown;
                        //}

                        //else if (grainCoverageResults[i] == "Quarantine" && meanGrainProtusionResults[i] == "Pass")
                        //{
                        //    burResults[i] = "Quarantine";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Orange;
                        //}

                        //else
                        //{
                        //    burResults[i] = "Undetermined";
                        //    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
                        //}

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
                            burData[i].P13 = gf.DetermineBin(burResultsCsv[i, 0], burResultsCsv[i, 1]);
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
                    btnMoveDeleteFiles.Enabled = true;
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

        //CLEAR ALL VALUES
        private void Clear_All()
        {
            btnMoveDeleteFiles.Enabled = false;
            btnOpenPDFReader.Enabled = false;
            btnClearScreen.Enabled = false;
            btnScanTemporaryFolder.Enabled = true;
            dropdownDistinctLotsDateTime.Enabled = true;
            dropdownDistinctLotsDateTime.Text = null;
            // dropdownDistinctLotsDateTime.Items.Clear();
            btnScanTempFolder_Click(null, null);
            btnReadPDFResults.Enabled = true;
            btnSaveResults.Enabled = false;
            TB_fixtureBarcode.Clear();
            TB_fixtureBarcode.Enabled = false;
            for (int i = 0; i < 40; i++)
            {
                string buttonName = "btn_Bur" + (i + 1).ToString();
                string mtvLabelName = "lbl_MTVBur" + (i + 1).ToString();
                string cgLabelName = "lbl_GCBur" + (i + 1).ToString();

                panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Gray;
                panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";

                panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";

                panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;

                Form1.form1Instance.badScanBurResults[i] = false;
                Form1.form1Instance.discardBurResults[i] = false;
            }
        }

        //BUTTON TO DO FILE MANAGEMENT
        private void btnMoveDeleteFiles_Click(object sender, EventArgs e)
        {
            try
            {

                int stateMachine = 0; //state machine variable
                List<string> listOfPDFFilesToTransfer = new List<string>(); // list of PDF files to copy/delete
                List<string> listOfCSVFilesToTransfer = new List<string>(); // list of PDF files to copy/delete

                string scannedFilesZipDir = Path.Combine(tempFolderPath, Path.GetFileNameWithoutExtension(availableCSVFiles[dropdownDistinctLotsDateTime.Text]));
                if (!Directory.Exists(scannedFilesZipDir))
                {

                    Directory.CreateDirectory(scannedFilesZipDir);

                }
                string goodScansZipDir = Path.Combine(scannedFilesZipDir, "Good Scans");
                string badScansZipDir = Path.Combine(scannedFilesZipDir, "Bad Scans");
                Directory.CreateDirectory(goodScansZipDir);
                Directory.CreateDirectory(badScansZipDir);

                btnOpenPDFReader.Enabled = false;
                btnMoveDeleteFiles.Enabled = false;
                listOfPDFFilesToTransfer = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf");
                listOfCSVFilesToTransfer = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "csv");

                if (listOfPDFFilesToTransfer == null)
                {
                    DialogResult dialog = MessageBox.Show("No files found on temp folder");
                    stateMachine = 1000;
                }

                //STATE 0: Neutral State
                if (stateMachine == 0)
                {
                    stateMachine = 10;
                }

                //STATE 10: check if files to transfer is null
                if (stateMachine == 10)
                {
                    if (listOfPDFFilesToTransfer != null)
                    {
                        stateMachine = 20;
                    }
                    else
                    {
                        DialogResult dialog = MessageBox.Show("No files found with select lot # and datetime. Please make sure 1) PDFs are in temp folder 2) drop down menu" +
                            "is selected with a lot # and datetime 3) settings is set with proper folder location ",
                        "No files with this Lot # Found", MessageBoxButtons.OK);
                    }
                }

                //STATE 20: check that the lot number date and time selector is not empty
                if (stateMachine == 20)
                {
                    if (dropdownDistinctLotsDateTime.Text != null)
                    {
                        if (sender != null) stateMachine = 40;
                        else stateMachine = 50;
                    }
                    else
                    {
                        DialogResult dialog = MessageBox.Show("Drop down selection is empty. Please make sure to select lot datetime you want to move",
                        "Empty Drop Down Selection", MessageBoxButtons.OK);
                        stateMachine = 1000;
                    }
                }

                //STATE 30: Check If Main Directories Exist (Good Scans, Bad Scans, Scans, Rescans)
                //if (stateMachine == 30)
                //{
                //    if (sender != null) stateMachine = 40;
                //    else stateMachine = 50;

                //    //-------------------------
                //}

                //STATE 40: Identify the bad scans (through the user)
                if (stateMachine == 40)
                {
                    var BadScanSelectorForm = new BadScanSelectorForm();
                    BadScanSelectorForm.ShowDialog();
                    stateMachine = 50;
                }

                //STATE 50: Notify operator of lot, datetime, bad scans, directories and get confirmation
                if (stateMachine == 50)
                {
                    //Preparing Strings for operator message. 
                    string machineIdentifier = Regex.Split(listOfPDFFilesToTransfer[0], "_")[2]; // Machine ID (SYS1, SYS2, SYS3,...,SYS<system number>)
                    string scanDateTime = Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[1] + " " + Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[2];
                    string lotNumber = Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[0];
                    string badScansList = "";
                    int numberOfBadScans = 0;
                    string goodScansList = "";
                    int numberOfGoodScans = 0;
                    for (int i = 0; i < Form1.form1Instance.badScanBurResults.Length; i++)
                    {
                        if (Form1.form1Instance.badScanBurResults[i] == true)
                        {
                            if (numberOfBadScans == 0)
                            {
                                badScansList = (i + 1).ToString();
                            }
                            else
                            {
                                badScansList = badScansList + "," + (i + 1).ToString();
                            }
                            numberOfBadScans += 1;
                        }
                        else
                        {
                            if (numberOfBadScans == 0)
                            {
                                goodScansList = (i + 1).ToString();
                            }
                            else
                            {
                                goodScansList = goodScansList + "," + (i + 1).ToString();
                            }
                            numberOfGoodScans += 1;
                        }
                    }

                    string confirmationMessage = "Confirm the information below is correct" +
                            Environment.NewLine +
                            Environment.NewLine + "Machine Identifier: " + machineIdentifier +
                            Environment.NewLine + "Date and Time: " + scanDateTime +
                            Environment.NewLine + "Lot Number: " + lotNumber +
                            // Environment.NewLine + "Scans or Rescans: " + subFolderName +
                            Environment.NewLine + "-------------------- " +

                            //Environment.NewLine + "Good Scans Will be Moved to: <Root Folder>/Good Scans/" + subFolderName +
                            // Environment.NewLine + "Good Scans Will be Moved to: <Root Folder>/Good Scans/" +
                            //Environment.NewLine + "Bad Scans Will be Moved to: <Root Folder>/Bad Scans/" +
                            Environment.NewLine + "Bad Scans List:" + badScansList;
                    DialogResult dialogConfirmFilesMovement = MessageBox.Show(confirmationMessage,
                           "Move Files - Confirmation", MessageBoxButtons.YesNo);

                    if (dialogConfirmFilesMovement == DialogResult.No)
                    {
                        btnOpenPDFReader.Enabled = true;
                        btnMoveDeleteFiles.Enabled = true;
                        btnSaveResults.Enabled = true;
                        stateMachine = 1000;
                        Console.WriteLine("No Selected");
                    }
                    else if (dialogConfirmFilesMovement == DialogResult.Yes)
                    {
                        stateMachine = 60;
                        Console.WriteLine("Yes Selected");
                    }
                    else
                    {
                        stateMachine = 1000;
                    }
                    gf.LogThis($"{dialogConfirmFilesMovement.ToString()} Selected: {confirmationMessage}");
                }

                //STATE 60: Copy files to new folder location
                if (stateMachine == 60)
                {
                    try
                    {
                        if (listOfCSVFilesToTransfer != null)
                        {
                            listOfPDFFilesToTransfer.Add(listOfCSVFilesToTransfer[0]);
                        }

                        foreach (string file in listOfPDFFilesToTransfer)
                        {
                            if (Path.GetExtension(file) == ".pdf")
                            {
                                string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
                                string burIndexString = Regex.Split(fileNameNoExtension, "Position")[1];
                                Int32.TryParse(burIndexString, out int burIndex);
                                Console.WriteLine("Bur Index:" + burIndex.ToString() + "," + "Good Scan? " + Form1.form1Instance.badScanBurResults[burIndex - 1].ToString());

                                if (!Form1.form1Instance.badScanBurResults[burIndex - 1])
                                {
                                    File.Copy(file, $"{goodScansZipDir}\\{Path.GetFileName(file)}", true);
                                }
                                else
                                {
                                    File.Copy(file, $"{badScansZipDir}\\{Path.GetFileName(file)}", true);
                                }
                                Console.WriteLine("------------------END-----------------------");
                            }

                            else if (Path.GetExtension(file) == ".csv")
                            {
                                File.Copy(file, $"{scannedFilesZipDir}\\{Path.GetFileName(file)}", true);
                            }
                        }
                        if (!File.Exists(scannedFilesZipDir + ".zip")) ZipFile.CreateFromDirectory(scannedFilesZipDir, scannedFilesZipDir + ".zip", CompressionLevel.Fastest, false);
                        stateMachine = 70;
                    }
                    catch (Exception error)
                    {
                        DialogResult dialog = MessageBox.Show("Error: " + error, "Error");
                        stateMachine = 1000;
                    }
                }
                //STATE 70: check that files copying has finished and delete files from temp folder 
                if (stateMachine == 70)
                {
                    Thread.Sleep(500);
                    bool isfileinuse = true;

                    foreach (string file in listOfPDFFilesToTransfer)
                    {
                        Console.WriteLine("files to transfer length = " + listOfPDFFilesToTransfer.Count().ToString());
                        isfileinuse = filesFunctions.IsFileLocked(file);
                        if (isfileinuse == true)
                        {
                            Console.WriteLine("----------------Files In Use-----------------");
                            Console.WriteLine("file in used. Copying process not finished");
                            Console.WriteLine(file);
                        }
                        else
                        {
                            File.Delete(file);
                            Console.WriteLine($"{file} deleted");

                        }
                    }
                    if (isfileinuse == false)
                    {
                        stateMachine = 75;
                    }
                    else
                    {
                        DialogResult dialog = MessageBox.Show("Files in use timeout. " +
                           "Unable to delete files. Files are in use. At times, this can be due to uploading files to a cloud location");
                        stateMachine = 1000;
                    }
                }

                // STATE 75: Upload files
                if (stateMachine == 75)
                {
                    try
                    {
                        UploadZipFile(scannedFilesZipDir + ".zip");
                        stateMachine = 80;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Files associated with {availableCSVFiles[dropdownDistinctLotsDateTime.Text]} were not uploaded");
                        stateMachine = 1000;
                    }
                }

                //STATE 80: Operation Completed
                if (stateMachine == 80)
                {
                    DialogResult dialog = MessageBox.Show("File management operation completed", "Operation Complete");
                    gf.LogThis($"File management operation completed for files associated with {availableCSVFiles[dropdownDistinctLotsDateTime.Text]}");
                    stateMachine = 1000;
                }

                //STATE 1000: final state
                if (stateMachine == 1000)
                {
                    if (Directory.Exists(scannedFilesZipDir))
                    {
                        Directory.Delete(scannedFilesZipDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to move/delete files: " + ex.Message);
                gf.LogThis("Unable to move/delete files: " + ex.ToString());
            }
        }

        private async void TB_fixtureBarcode_TextChanged(object sender, EventArgs e)
        {
            string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
            if (!string.IsNullOrEmpty(fixtureId))
            {
                bool barcodeExists = await CheckFixtureBarcodeExistsAsync(fixtureId);
                if (barcodeExists)
                {
                    MessageBox.Show($"Fixture barcode {fixtureId} already exists in the database with burs. Please check fixture status.");
                    TB_fixtureBarcode.Clear(); // Optionally clear the textbox to prevent further action
                }
            }
        }

        private async Task<bool> CheckFixtureBarcodeExistsAsync(string fixtureId)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurQCTrayTable");
            var filter = new QueryFilter("trayID", QueryOperator.Equal, fixtureId);
            var search = table.Query(filter);

            try
            {
                var documentSet = await search.GetNextSetAsync();
                if (documentSet.Count > 0)
                {
                    var document = documentSet[0];
                    for (int i = 1; i <= 100; i++)
                    {
                        string burKey = $"bur{i}";
                        if (document.TryGetValue(burKey, out DynamoDBEntry burEntry))
                        {
                            var burData = burEntry.AsDocument();
                            if (burData != null && burData.Count > 0)
                            {
                                return true; // Fixture has burs
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking fixture barcode existence: {ex.Message}");
                Logger.LogError("Error checking fixture barcode existence", ex);
            }
            return false; // No burs found
        }

        private async void Btn_saveResults_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (string.IsNullOrEmpty(dropdownDistinctLotsDateTime.Text))
            {
                MessageBox.Show("Please select a csv file");
                return;
            }
            if (string.IsNullOrEmpty(TB_fixtureBarcode.Text))
            {
                MessageBox.Show("Scan fixture barcode");
                return;
            }

            string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
            bool barcodeExists = await CheckFixtureBarcodeExistsAsync(fixtureId);
            if (barcodeExists)
            {
                MessageBox.Show($"Fixture {fixtureId} already exists in the database. Upload aborted.");
                return;
            }

            bool[] badScanBurResults = new bool[40];
            bool[] discardBurResults = new bool[40];
            #region testing
            //try
            //{
            //    var badScanSelector = new BadScanSelector();
            //    badScanSelector.ShowDialog();
            //    badScanBurResults = Form1.form1Instance.badScanBurResults;
            //    discardBurResults = Form1.form1Instance.discardBurResults;
            //    var selectedBadResults = Enumerable.Range(0, badScanBurResults.Length).Where(x => badScanBurResults[x]);
            //    var selectedDiscardResults = Enumerable.Range(0, discardBurResults.Length).Where(x => discardBurResults[x]);
            //    string confirmResults = "";
            //    string confirmBadResults = "Bad Scan Bur(s): \n";
            //    string confirmDiscardResults = "Discard Bur(s): \n";
            //    for (int i = 0; i < selectedBadResults.Count(); i++)
            //    {
            //        confirmBadResults += $"- {selectedBadResults.ElementAt(i) + 1}\n";
            //    }

            //    for (int i = 0; i < selectedDiscardResults.Count(); i++)
            //    {
            //        confirmDiscardResults += $"- {selectedDiscardResults.ElementAt(i) + 1}\n";
            //    }
            //    confirmResults = confirmBadResults + confirmDiscardResults;
            //    var msgResult = MessageBox.Show(confirmResults, "Is this correct?", MessageBoxButtons.YesNo);

            //    if (msgResult != DialogResult.Yes)
            //    {
            //        MessageBox.Show("Click on \'Save Results\' again to reselect bad scans");
            //        return;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("SaveResults operation failed: " + ex.Message);
            //    gf.LogThis("SaveResults operation failed: " + ex.ToString());
            //}


            ////bool inserted = Program.dbClient.PutItem(TB_fixtureBarcode.Text, Databases.BUR_DATABASE);
            //string burName = "";
            ////if (inserted)
            ////{
            //try
            //{
            //    bool successfulUpload = true;
            //    for (int i = 0; i < burData.Length; i++)
            //    {
            //        burName = $"bur{i + 1}";
            //        if (burData[i] != null && badScanBurResults[i])
            //        {
            //            burData[i].P10 = true;
            //            burData[i].P11 = false;
            //        }
            //        else if (burData[i] != null && discardBurResults[i])
            //        {
            //            burData[i].P10 = false;
            //            burData[i].P11 = true;
            //        }
            //        int numTries = 0;
            //        bool result = false;
            //        if (!Default.IsUsingHttp)
            //        {
            //            //bool result = Program.dbClient.UpdateFixtureTable(TB_fixtureBarcode.Text, burName, burData[i], Databases.FIXTURE_DATABASE, availableCSVFiles[dropdownDistinctLotsDateTime.Text] + ".csv");
            //            //if (!result)
            //            //{
            //            //    gf.LogThis($"Error logging bur data at position {i + 1}, trying again");
            //            //    Program.dbClient.UpdateFixtureTable(TB_fixtureBarcode.Text, burName, burData[i], Databases.FIXTURE_DATABASE, availableCSVFiles[dropdownDistinctLotsDateTime.Text] + ".csv");
            //            //}
            //            do
            //            {
            //                result = Program.dbClient.UpdateFixtureTable(TB_fixtureBarcode.Text, burName, burData[i], Databases.FIXTURE_DATABASE, availableCSVFiles[dropdownDistinctLotsDateTime.Text] + ".csv");
            //                if (!result) numTries++;
            //                Thread.Sleep(200);
            //            }
            //            while (!result && numTries < GF.MAX_FAILURES);

            //            if (result)
            //            {
            //                gf.LogThis($"Bur Data at position {i+1} successfully updated");
            //            }
            //            else if (numTries >= GF.MAX_FAILURES)
            //            {
            //                successfulUpload = false;
            //                gf.LogThis($"Error logging bur data at position {i + 1}, failed 3 times");
            //            }
            //        }
            //        else
            //        {
            //            UpdateFixtureTable(TB_fixtureBarcode.Text, burData[i], Operation.updateFixtureTable.ToString(), availableCSVFiles[dropdownDistinctLotsDateTime.Text] + ".csv", burName);
            //        }

            //        //Thread.Sleep(200);

            //    }
            //    stopwatch.Stop();
            //    if (!successfulUpload)
            //    {
            //        gf.LogThis($"At least one bur update failed for fixtureId {TB_fixtureBarcode.Text}");
            //        DialogResult response = MessageBox.Show("FIXTURE UPDATE FAILED: \nTo try again, press \'OK\'. Otherwise, click \'Cancel\'", "FIXTURE UPDATE FAILED!",MessageBoxButtons.OKCancel,MessageBoxIcon.Error);

            //        if (response == DialogResult.OK)
            //        {

            //        }
            //    }
            //    else
            //    {
            //        gf.LogThis($"FixtureId {TB_fixtureBarcode.Text} data successfully updated, took {stopwatch.ElapsedMilliseconds}ms");
            //    }


            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Bur data not saved in the cloud: " + ex.Message);
            //    gf.LogThis($"Bur data not saved in the cloud, fixtureId {TB_fixtureBarcode.Text}: " + ex.ToString());
            //    return;
            //}
            ////}
            #endregion testing

            string burSelection = SelectBurs(out badScanBurResults, out discardBurResults);
            bool confirmBurSelection = ConfirmBurSelection(burSelection);
            bool uploadFinished = false;
            if (confirmBurSelection)
            {
                do
                {
                    uploadFinished = UploadBurData(badScanBurResults, discardBurResults);
                } while (!uploadFinished &&
                DialogResult.Retry == MessageBox.Show("FIXTURE UPDATE FAILED: \nTo try again, press \'Retry\'. Otherwise, click \'Cancel\'", "FIXTURE UPDATE FAILED!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error));
                //uploadFinished = UploadBurData(badScanBurResults, discardBurResults);
                if (uploadFinished)
                {
                    btnSaveResults.Enabled = false;
                    TB_fixtureBarcode.Enabled = false;
                    DialogResult moveDelete = MessageBox.Show("Fixture data saved in the cloud!\n Do you want to Move/Delete the files?", "", MessageBoxButtons.YesNo);
                    if (moveDelete == DialogResult.Yes)
                    {
                        btnMoveDeleteFiles_Click(null, null);

                        Clear_All();
                    }
                }



                //else
                //{
                //    gf.LogThis($"At least one bur update failed for fixtureId {TB_fixtureBarcode.Text}");
                //    DialogResult response = MessageBox.Show("FIXTURE UPDATE FAILED: \nTo try again, press \'OK\'. Otherwise, click \'Cancel\'", "FIXTURE UPDATE FAILED!", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                //    if (response == DialogResult.OK)
                //    {

                //    }
                //}
            }
            else
            {
                return;
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

        private bool UploadBurData(bool[] badScanBurResults, bool[] discardBurResults)
        {
            string csvFileName = availableCSVFiles[dropdownDistinctLotsDateTime.Text];
            // Form1.form1Instance.ShowBusyWindow(true);
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
                        //burInfo.allBurs[burName].P10 = false;
                        //burInfo.allBurs[burName].P11 = true;
                        //allBurData[burName].P10 = false;
                        //allBurData[burName].P11 = true;
                    }
                    if (allBurData.ContainsKey(burName))
                    {
                        allBurData.Remove(burName);
                    }
                    allBurData.Add(burName, burData[i]);
                    int numTries = 0;
                    bool result = false;

                    if (Default.IsUsingHttp)
                    {
                        UpdateFixtureTable(TB_fixtureBarcode.Text, burData[i], Operation.updateFixtureTable.ToString(), csvFileName + ".csv", burName);
                        UpdateProgressBar((i * 100) / burData.Length);
                    }
                    //Thread.Sleep(200);
                    //Form1.form1Instance.UpdateBusyWindowProgress((i * 100) / burData.Length);

                }
                if (!Default.IsUsingHttp)
                {
                    if (dropdownDistinctLotsDateTime.Text.Substring(0, 2) == "42") burInfo.areAcurataBurs = true;
                    else burInfo.areAcurataBurs = false;
                    burInfo.willSortBag = true;
                    burInfo.allBurs = allBurData;
                    burInfo.binSettings = JsonConvert.SerializeObject(mBinConfig);
                    Thread.Sleep(1000);

                    gf.LogThis($"Fixture {fixtureId} information is being sent to the cloud...");

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
                    //  Dictionary<int, string> response = new Dictionary<int, string>();
                    //for (int i = 0; i < 10; i++)
                    //{
                    // response = Program.dbClient.GetAllBurInfo($"hhtest{i}", Databases.FIXTURE_DATABASE);
                    // response = Program.dbClient.GetAllBurInfo($"hhtest{i}", Databases.TEST_BUR_DATABASE);
                    var response = Program.dbClient.GetAllBurInfo(fixtureId, Databases.FIXTURE_DATABASE);
                    UpdateProgressBar(10 * 100 / 40);
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

                //}
                //Form1.form1Instance.ShowBusyWindow(false);
                this.progressBar1.Visible = false;
                UpdateProgressBar(0);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Bur data not saved in the cloud: " + ex.Message);
                gf.LogThis($"Bur data not saved in the cloud, fixtureId {TB_fixtureBarcode.Text}: " + ex.ToString());
                // Form1.form1Instance.ShowBusyWindow(false);
                this.progressBar1.Visible = false;
                return false;
            }
            return successfulUpload;
        }

        private void UpdateProgressBar(int i)
        {
            progressBar1.Value = i;
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
            DG_binSettings.Rows.Clear();
            DG_MGP.Rows.Clear();
            selectedFileName = "";
            burInfo.fixtureId = "";
            burInfo.csvFile = "";
            burInfo.stopTime = "";
            burInfo.startTime = "";
            burInfo.lastUpdateTime = "";
            burInfo.areAcurataBurs = false;
            burInfo.willSortBag = false;
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

        private int selectedIndex = -1;

        private void dropdownDistinctLotsDateTime_SelectedIndexChanged(object sender, EventArgs e)
        {

            //if (selectedIndex == dropdownDistinctLotsDateTime.SelectedIndex) return;
            //if (selectedIndex > 0) dropdownDistinctLotsDateTime.Items[selectedIndex] = dropdownDistinctLotsDateTime.Text;

            try
            {

                StateMachine();

                //if (selectedIndex == dropdownDistinctLotsDateTime.SelectedIndex) return;
                //if (selectedIndex > 0) dropdownDistinctLotsDateTime.Items[selectedIndex] = dropdownDistinctLotsDateTime.Text;
                //if (dropdownDistinctLotsDateTime.SelectedIndex >= 0)
                //{
                //    selectedIndex = dropdownDistinctLotsDateTime.SelectedIndex;
                //    dropdownDistinctLotsDateTime.Items[selectedIndex] = selectedFileName;
                //}

            }
            catch (Exception)
            {

            }
            //dropdownDistinctLotsDateTime.Text = selectedFileName;

        }
    }
}



