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
using System.Threading;
using Microsoft.Office.Core;
using static User_Interface.SetupService;

namespace User_Interface
{
    public partial class SideScanUserControl : UserControl
    {
        //INTIALIZE CLASSES
        FilesFunctions filesFunctions = new FilesFunctions(); //Reusable Functions
        PDFFunctions pdfFunctions = new PDFFunctions(); // Allows Merging of PDF
        CSVFunctions csvFunctions = new CSVFunctions();
        PDFViewer pdfViewer = new PDFViewer(); // Class to view PDF
        MathFunctions mathFunctions = new MathFunctions();


        //Global Variables
        string rootFolderPath;
        string tempFolderPath;
        int maxNumberOfBursPerScan = 8;
        int maxNumberOfSegments = 3;

        public SideScanUserControl()
        {
            InitializeComponent();

            try
            {
                //Create folde paths based on txt file constumables
                //rootFolderPath = mPConfig.CSVBaggingFolderPath;
                //tempFolderPath = mPConfig.TempFolderName;
                LoadSettings();
            }
            catch (Exception error)
            {

                Console.WriteLine(error);
                /*
                DialogResult dialog = MessageBox.Show("Unabled to read pass fail criteria and/or root folder from txt file." +
                "If the PDFs are moved from the temp folder, you will no longer be able to see results on this screen. \n \n" + error,
                    "Configurations Text File Error");
                    */
            }

            btnClearScreen.Enabled = false;
            btnScanTemporaryFolder.Enabled = true;
            btnReadPDFResults.Enabled = true;

            #region testing 
            //Console.WriteLine("-------------Statistics-------------");
            //double[] array1 = new double[] { 1, 3, 5, 7, 9, 0 };
            //Console.WriteLine("number of non zeros: " + mathFunctions.FindNumberOfNonZerosNumbers(array1).ToString());
            //Console.WriteLine("average: " + mathFunctions.CalculateAverage(array1).ToString());
            //Console.WriteLine("standard deviation: " + mathFunctions.CalculateStandardDeviation(array1).ToString());
            //Console.WriteLine("max: " + mathFunctions.CalculateMaxValue(array1).ToString());
            //Console.WriteLine("min: " + mathFunctions.CalculateMinValue(array1).ToString());

            //double[,] array2Da = new double[4, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };
            //double[] array1D = mathFunctions.GetColumn(array2Da, 1);
            //Console.WriteLine("length: " + array1D.Length.ToString());

            //for (int i = 0; i < array1D.Length; i++)
            //{
            //    Console.WriteLine("value: " + array1D[i].ToString());
            //}
            //Console.WriteLine("-------------End-------------");
            #endregion
        }

        private void LoadSettings()
        {
            rootFolderPath = mPConfig.CSVBaggingFolderPath;
            tempFolderPath = mPConfig.TempFolderName;
        }

        private void btnScanTemporaryFolder_Click(object sender, EventArgs e)
        {
            LoadSettings();

            List<List<string>> tempFolderFileNamesExtractedData = new List<List<string>>(); //list of lists with each file scanner ID
            //Lot number, date, time, etc
            List<string> tempFolderLotDateTimes = new List<string>(); // a list with dates and times from the file names in temp folder
            List<string> tempFolderDistinctLotDateTimes = new List<string>(); // list with distinct dates and times from temp folder

            try
            {
                tempFolderFileNamesExtractedData = filesFunctions.ExtractInfoFromTempFolder(tempFolderPath, "side");
                Console.WriteLine(tempFolderPath);
                if (tempFolderFileNamesExtractedData != null)
                {
                    foreach (List<string> fileData in tempFolderFileNamesExtractedData)
                    {
                        tempFolderLotDateTimes.Add(fileData[0] + "_" + fileData[1]);//Create list with dates/times
                    }
                }
                else
                {
                    Console.WriteLine(tempFolderFileNamesExtractedData.Count);
                    MessageBox.Show("No files found", "Attention");
                }

                tempFolderDistinctLotDateTimes = tempFolderLotDateTimes.Distinct().ToList(); //get only lot distinct dates and times

                dropdownDistinctLotsDateTime.Items.Clear(); // clear any items on drop down menu currently
                foreach (string file_data in tempFolderDistinctLotDateTimes)
                {
                    dropdownDistinctLotsDateTime.Items.Add(file_data); // populate drop down list 
                }
            }
            catch
            {
                MessageBox.Show("Unable to find files. Make sure that the root file location on settings have a Temp Folder in it. ", "Temp Folder of File structure not found");
            }
        }

        private void btnReadPDFResults_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(dropdownDistinctLotsDateTime.Text))
            {
                MessageBox.Show("Please select the lot and datetime results you want to results results from", "Required fields are empty");
            }
            else
            {
                btnClearScreen.Enabled = true;
                btnScanTemporaryFolder.Enabled = false;
                btnReadPDFResults.Enabled = false;
                StateMachine();
            }
        }

        public string Between(string STR, string FirstString, string LastString)
        {
            try
            {
                string FinalString;
                int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
                int Pos2 = STR.IndexOf(LastString);
                FinalString = STR.Substring(Pos1, Pos2 - Pos1);
                return FinalString;
            }
            catch
            {
                return "";
            }

        }

        //STATE MACHINE TO RETRIEVE ALL VALUES FROM ALL PDFS
        private void StateMachine()
        {
            int stateMachine = 0; // state machine 

            List<string> filePathsToExtractDataFrom = new List<string>(); //has file paths for all PDFs results for data to be extracted from 
            double[,] grainCoverageValues = new double[maxNumberOfBursPerScan, maxNumberOfSegments]; //list of grain coverage values (double)
            double[,] meanGrainProtusionValues = new double[maxNumberOfBursPerScan, maxNumberOfSegments]; //list of mean grain protusion values (double)

            double[,] grainCoverageStatistics = new double[maxNumberOfBursPerScan, 6]; //a 8 rows by 6 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min, median
            double[,] meanGrainProtusionStatistics = new double[maxNumberOfBursPerScan, 6];

            double[] grainCoverageAvgOfAvgStatistics = new double[6]; //a 8 rows by 6 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min, median
            double[] meanGrainProtusionAvgOfAvgStatistics = new double[6];




            string[] grainCoverageResults = new string[maxNumberOfBursPerScan]; //list of grain coverage results (strings)
            string[] meanGrainProtusionResults = new string[maxNumberOfBursPerScan]; //list of mean grain protusion results (strings)
            string[] burResults = new string[maxNumberOfBursPerScan];

            List<int>[] meanGrainProtusionResultsArray = new List<int>[maxNumberOfBursPerScan];

            //STATE 0: Starting State Machine
            if (stateMachine == 0)
            {
                stateMachine = 10;
            }

            //STATE 10: Check That File Path List is Not Null
            if (stateMachine == 10)
            {
                filePathsToExtractDataFrom = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
                if (filePathsToExtractDataFrom == null)
                {
                    MessageBox.Show("Lot number_date_time not found", "Action Required");
                    stateMachine = 1000;
                }
                else
                {
                    stateMachine = 20;
                }
            }

            //STATE 20: Extract Mean Grain Protusion and Grain Coverage From PDFs
            if (stateMachine == 20)
            {
                foreach (string file in filePathsToExtractDataFrom)
                {
                    List<double> primeNumbers = new List<double>();

                    var results = pdfFunctions.ExtractMGPAndGCFromPDF(@file);
                    double grainCoverageResult = results.Item1; //grain coverage extracted from PDf
                    double meanGrainProtusionResult = results.Item2; // mean grain protusion extracted from PDF
                    string segmentNumberString; //segment number retrieved from pdf file name
                    int segmentNumber = 100;
                    int burPosition = 100;

                    string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);

                    try
                    {
                        string burPositionString = Between(fileNameNoExtension, "Position", "_Segment");
                        string[] fileNameNoExtensionParsed = Regex.Split(fileNameNoExtension, "_Segment");

                        segmentNumberString = fileNameNoExtensionParsed[1];
                        Int32.TryParse(segmentNumberString, out segmentNumber);
                        Int32.TryParse(burPositionString, out burPosition);
                        if (burPosition > 0 && burPosition <= maxNumberOfBursPerScan && segmentNumber > 0 && segmentNumber <= 3)
                        {
                            grainCoverageValues[(burPosition - 1), (segmentNumber - 1)] = grainCoverageResult;
                            meanGrainProtusionValues[(burPosition - 1), (segmentNumber - 1)] = meanGrainProtusionResult;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Failed to retrieve bur position from PDF name", "Action Required");
                        stateMachine = 1000;
                        break;
                    }
                }

                for (int i = 0; i < maxNumberOfBursPerScan; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Console.WriteLine("----------------------");
                        Console.WriteLine("GC: " + grainCoverageValues[i, j].ToString());
                        Console.WriteLine("MGP: " + meanGrainProtusionValues[i, j].ToString());
                        Console.WriteLine("burPosition: " + (i + 1).ToString());
                        Console.WriteLine("Segment index: " + (j + 1).ToString());
                        Console.WriteLine("----------------------");
                    }
                }
                stateMachine = 30;
            }

            //STATE 30: Calculate Averages of Average Results
            if (stateMachine == 30)
            {
                for (int i = 0; i < maxNumberOfBursPerScan; i++)
                {

                    grainCoverageStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(mathFunctions.GetRow(grainCoverageValues, i));
                    grainCoverageStatistics[i, 1] = mathFunctions.CalculateAverage(mathFunctions.GetRow(grainCoverageValues, i));
                    grainCoverageStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(mathFunctions.GetRow(grainCoverageValues, i));
                    grainCoverageStatistics[i, 3] = mathFunctions.CalculateMaxValue(mathFunctions.GetRow(grainCoverageValues, i));
                    grainCoverageStatistics[i, 4] = mathFunctions.CalculateMinValue(mathFunctions.GetRow(grainCoverageValues, i));
                    grainCoverageStatistics[i, 5] = mathFunctions.CalculateMedian(mathFunctions.GetRow(grainCoverageValues, i));

                    meanGrainProtusionStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(mathFunctions.GetRow(meanGrainProtusionValues, i));
                    meanGrainProtusionStatistics[i, 1] = mathFunctions.CalculateAverage(mathFunctions.GetRow(meanGrainProtusionValues, i));
                    meanGrainProtusionStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(mathFunctions.GetRow(meanGrainProtusionValues, i));
                    meanGrainProtusionStatistics[i, 3] = mathFunctions.CalculateMaxValue(mathFunctions.GetRow(meanGrainProtusionValues, i));
                    meanGrainProtusionStatistics[i, 4] = mathFunctions.CalculateMinValue(mathFunctions.GetRow(meanGrainProtusionValues, i));
                    meanGrainProtusionStatistics[i, 5] = mathFunctions.CalculateMedian(mathFunctions.GetRow(meanGrainProtusionValues, i));
                }



                for (int i = 0; i < maxNumberOfBursPerScan; i++)
                {
                    Console.WriteLine("-------Bur" + (i + 1).ToString() + "-------");
                    Console.WriteLine("GRAIN COVERAGE:");
                    Console.WriteLine(" number of non zeros: " + (grainCoverageStatistics[i, 0]).ToString());
                    Console.WriteLine(" average:" + grainCoverageStatistics[i, 1].ToString());
                    Console.WriteLine(" standard deviation: " + grainCoverageStatistics[i, 2].ToString());
                    Console.WriteLine(" max: " + grainCoverageStatistics[i, 3].ToString());
                    Console.WriteLine(" min: " + grainCoverageStatistics[i, 4].ToString());
                    Console.WriteLine(" median: " + grainCoverageStatistics[i, 5].ToString());
                    Console.WriteLine("MEAN GRAIN PROTUSION:");
                    Console.WriteLine(" number of non zeros: " + (meanGrainProtusionStatistics[i, 0]).ToString());
                    Console.WriteLine(" average: " + meanGrainProtusionStatistics[i, 1].ToString());
                    Console.WriteLine(" standard deviation: " + meanGrainProtusionStatistics[i, 2].ToString());
                    Console.WriteLine(" max: " + meanGrainProtusionStatistics[i, 3].ToString());
                    Console.WriteLine(" min: " + meanGrainProtusionStatistics[i, 4].ToString());
                    Console.WriteLine(" median: " + meanGrainProtusionStatistics[i, 5].ToString());
                }

                stateMachine = 40;
            }

            //STATE 40: find average of averages
            if (stateMachine == 40)
            {
                double[] grainCoverageAvgOfAvgsArray = new double[maxNumberOfBursPerScan];
                grainCoverageAvgOfAvgsArray = mathFunctions.GetColumn(grainCoverageStatistics, 1);

                double[] meanGrainProtusionAvgOfAvgsArray = new double[maxNumberOfBursPerScan];
                meanGrainProtusionAvgOfAvgsArray = mathFunctions.GetColumn(meanGrainProtusionStatistics, 1);


                for (int i = 0; i < maxNumberOfBursPerScan; i++)
                {
                    //grainCoverageAvgOfAvgStatistics
                    grainCoverageAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[5] = mathFunctions.CalculateMedian(grainCoverageAvgOfAvgsArray);

                    //grainCoverageAvgOfAvgStatistics
                    meanGrainProtusionAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[5] = mathFunctions.CalculateMedian(meanGrainProtusionAvgOfAvgsArray);
                }




                Console.WriteLine("-------Burs-------");
                Console.WriteLine("GRAIN COVERAGE AVG OF AVGs:");
                Console.WriteLine(" number of non zeros: " + (grainCoverageAvgOfAvgStatistics[0]).ToString());
                Console.WriteLine(" average: " + grainCoverageAvgOfAvgStatistics[1].ToString());
                Console.WriteLine(" standard deviation: " + grainCoverageAvgOfAvgStatistics[2].ToString());
                Console.WriteLine(" max: " + grainCoverageAvgOfAvgStatistics[3].ToString());
                Console.WriteLine(" min: " + grainCoverageAvgOfAvgStatistics[4].ToString());
                Console.WriteLine("MEAN GRAIN PROTUSION AVG OF AVGs:");
                Console.WriteLine(" number of non zeros: " + (meanGrainProtusionAvgOfAvgStatistics[0]).ToString());
                Console.WriteLine(" average: " + meanGrainProtusionAvgOfAvgStatistics[1].ToString());
                Console.WriteLine(" standard deviation: " + meanGrainProtusionAvgOfAvgStatistics[2].ToString());
                Console.WriteLine(" max: " + meanGrainProtusionAvgOfAvgStatistics[3].ToString());
                Console.WriteLine(" min: " + meanGrainProtusionAvgOfAvgStatistics[4].ToString());

                stateMachine = 50;


            }

            //STATE 50: Populate Screen
            if (stateMachine == 50)
            {

                //Average of Averages Grain Coverage Statistics
                lblAvgOfAvgsNumberOfDataPointsGC.Text = grainCoverageAvgOfAvgStatistics[0].ToString();
                lblAvgOfAvgsAverageGC.Text = grainCoverageAvgOfAvgStatistics[1].ToString();
                lblAvgOfAvgsStdevGC.Text = grainCoverageAvgOfAvgStatistics[2].ToString();
                lblAvgOfAvgsMaxGC.Text = grainCoverageAvgOfAvgStatistics[3].ToString();
                lblAvgOfAvgsMinGC.Text = grainCoverageAvgOfAvgStatistics[4].ToString();
                lblAvgOfAvgsMedianGC.Text = grainCoverageAvgOfAvgStatistics[5].ToString();

                int gcStatisticsHeight = grainCoverageStatistics.GetLength(0);
                int gcStatisticsWidth = grainCoverageStatistics.GetLength(1);


                dataGridViewGC.ColumnCount = gcStatisticsWidth;
                dataGridViewGC.Columns[0].HeaderText = "# Data Points";
                dataGridViewGC.Columns[1].HeaderText = "Avg";
                dataGridViewGC.Columns[2].HeaderText = "Stdev";
                dataGridViewGC.Columns[3].HeaderText = "Max";
                dataGridViewGC.Columns[4].HeaderText = "Min";
                dataGridViewGC.Columns[5].HeaderText = "Median";


                for (int r = 0; r < gcStatisticsHeight; r++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(this.dataGridViewGC);

                    for (int c = 0; c < gcStatisticsWidth; c++)
                    {
                        row.Cells[c].Value = grainCoverageStatistics[r, c];
                    }

                    this.dataGridViewGC.Rows.Add(row);
                }

                //Avergae of Averages Mean Grain Protusion Statistics
                lblAvgOfAvgsNumberOfDataPointsMGP.Text = meanGrainProtusionAvgOfAvgStatistics[0].ToString();
                lblAvgOfAvgsAverageMGP.Text = meanGrainProtusionAvgOfAvgStatistics[1].ToString();
                lblAvgOfAvgsStdevMGP.Text = meanGrainProtusionAvgOfAvgStatistics[2].ToString();
                lblAvgOfAvgsMaxMGP.Text = meanGrainProtusionAvgOfAvgStatistics[3].ToString();
                lblAvgOfAvgsMinMGP.Text = meanGrainProtusionAvgOfAvgStatistics[4].ToString();
                lblAvgOfAvgsMedianMGP.Text = meanGrainProtusionAvgOfAvgStatistics[5].ToString();

                int mgpStatisticsHeight = meanGrainProtusionStatistics.GetLength(0);
                int mgpStatisticsWidth = meanGrainProtusionStatistics.GetLength(1);


                dataGridViewMGP.ColumnCount = mgpStatisticsWidth;
                dataGridViewMGP.Columns[0].HeaderText = "# Data Points";
                dataGridViewMGP.Columns[1].HeaderText = "Avg";
                dataGridViewMGP.Columns[2].HeaderText = "Stdev";
                dataGridViewMGP.Columns[3].HeaderText = "Max";
                dataGridViewMGP.Columns[4].HeaderText = "Min";
                dataGridViewMGP.Columns[5].HeaderText = "Median";


                for (int r = 0; r < mgpStatisticsHeight; r++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(this.dataGridViewMGP);

                    for (int c = 0; c < mgpStatisticsWidth; c++)
                    {
                        row.Cells[c].Value = meanGrainProtusionStatistics[r, c];
                    }

                    this.dataGridViewMGP.Rows.Add(row);
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
            }

            /*
            //STATE 30: Interpret Results
            if (stateMachine == 30)
            {
                for (int i = 0; i < 40; i++)
                {
                    //Mean Tip Value Pass/Fail Criteria
                    if (meanGrainProtusionValues[i] > meanGrainProtusionCriteria1)
                    {
                        meanGrainProtusionResults[i] = "Fail";
                    }
                    else if (meanGrainProtusionValues[i] <= meanGrainProtusionCriteria1 && meanGrainProtusionValues[i] > 0)
                    {
                        meanGrainProtusionResults[i] = "Pass";
                    }
                    else
                    {
                        meanGrainProtusionResults[i] = "Undetermined";
                    }

                    //Grain Coverage Pass/Fail Criteria
                    if (grainCoverageValues[i] < grainCoverageCriteria1 && grainCoverageValues[i] > 0)
                    {
                        grainCoverageResults[i] = "Fail";
                    }

                    else if (grainCoverageValues[i] > grainCoverageCriteria3 ||
                        (grainCoverageValues[i] >= grainCoverageCriteria1 && grainCoverageValues[i] < grainCoverageCriteria2))
                    {
                        grainCoverageResults[i] = "Quarantine";
                    }

                    else if (grainCoverageValues[i] >= grainCoverageCriteria2 &&
                        grainCoverageValues[i] <= grainCoverageCriteria3)
                    {
                        grainCoverageResults[i] = "Pass";
                    }
                    else
                    {
                        grainCoverageResults[i] = "Undetermined";
                    }

                    //Filling Bur Results

                    if (grainCoverageResults[i] == "Pass" && meanGrainProtusionResults[i] == "Pass")
                    {
                        burResults[i] = "Pass";
                    }

                    else if (grainCoverageResults[i] == "Fail" || meanGrainProtusionResults[i] == "Fail")
                    {
                        burResults[i] = "Fail";
                    }

                    else if (grainCoverageResults[i] == "Quarantine" && meanGrainProtusionResults[i] == "Pass")
                    {
                        burResults[i] = "Quarantine";
                    }

                    else
                    {
                        burResults[i] = "Undetermined";
                    }
                }
                stateMachine = 40;
            }

            //STATE 40: Populate screen according to results
            if (stateMachine == 40)
            {
                for (int i = 0; i < 40; i++)
                {
                    string buttonName = "btn_Bur" + (i + 1).ToString();
                    string mtvLabelName = "lbl_MTVBur" + (i + 1).ToString();
                    string cgLabelName = "lbl_GCBur" + (i + 1).ToString();
                    //Update Buttons
                    if (burResults[i] == "Pass")
                    {
                        panel_Bur_Buttons.Controls[buttonName].BackColor = Color.MediumSeaGreen;
                    }
                    else if (burResults[i] == "Quarantine")
                    {
                        panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Orange;
                    }
                    else if (burResults[i] == "Fail")
                    {
                        panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Brown;
                    }
                    else if (burResults[i] == "Undetermined")
                    {
                        panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
                    }

                    //Update MTV Values
                    if (meanGrainProtusionResults[i] == "Undetermined")
                    {
                        panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";
                        panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                    }

                    else
                    {
                        panel_Bur_Buttons.Controls[mtvLabelName].Text = meanGrainProtusionValues[i].ToString();
                        if (meanGrainProtusionResults[i] == "Pass")
                        {
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.MediumSeaGreen;
                        }
                        else if (meanGrainProtusionResults[i] == "Fail")
                        {
                            panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Brown;
                        }

                    }

                    //Update Grain Coverage Values
                    if (grainCoverageResults[i] == "Undetermined")
                    {
                        panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";
                        panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Gray;
                    }
                    else
                    {
                        panel_Bur_Buttons.Controls[cgLabelName].Text = grainCoverageValues[i].ToString();
                        if (grainCoverageResults[i] == "Pass")
                        {
                            panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.MediumSeaGreen;
                        }
                        else if (grainCoverageResults[i] == "Quarantine")
                        {
                            panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Orange;
                        }
                        else if (grainCoverageResults[i] == "Fail")
                        {
                            panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Red;
                        }

                    }


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
            dropdownDistinctLotsDateTime.Items.Clear();
            btnReadPDFResults.Enabled = true;
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
            }
            */
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void btnClearScreen_Click(object sender, EventArgs e)
        {
            Clear_All();

        }

        private void btnOpenPDFReader_Click(object sender, EventArgs e)
        {
            List<string> filesToMerge = new List<string>(); // will list all pdf file paths to be merged based on drop down menu selection

            filesToMerge = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");

            if (filesToMerge != null)
            {
                Console.WriteLine("FILES TO MERGE NOT NULL!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                string mergedFilePath = pdfFunctions.MergePDFs(filesToMerge, "Merged", tempFolderPath, "side");
                if (mergedFilePath == null)
                {
                    Console.WriteLine("MERGED FILE PATH IS NULL!!!");
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
                    catch
                    {
                        MessageBox.Show("Unable to open merged file", "Merged PDF not found");
                    }
                }
            }
            else
            {
                MessageBox.Show("Unable to open merged file", "Merged PDF not found");
            }
        }

        private void Clear_All()
        {
            btnMoveDeleteFiles.Enabled = false;
            btnOpenPDFReader.Enabled = false;
            btnClearScreen.Enabled = false;
            btnScanTemporaryFolder.Enabled = true;
            dropdownDistinctLotsDateTime.Enabled = true;
            dropdownDistinctLotsDateTime.Text = null;
            dropdownDistinctLotsDateTime.Items.Clear();
            btnReadPDFResults.Enabled = true;

            dataGridViewMGP.Rows.Clear();
            dataGridViewMGP.Refresh();

            dataGridViewGC.Rows.Clear();
            dataGridViewGC.Refresh();

            string neutralNumber = "00";
            lblAvgOfAvgsNumberOfDataPointsMGP.Text = neutralNumber;
            lblAvgOfAvgsAverageMGP.Text = neutralNumber;
            lblAvgOfAvgsStdevMGP.Text = neutralNumber;
            lblAvgOfAvgsMaxMGP.Text = neutralNumber;
            lblAvgOfAvgsMinMGP.Text = neutralNumber;
            lblAvgOfAvgsMedianMGP.Text = neutralNumber;

            lblAvgOfAvgsNumberOfDataPointsGC.Text = neutralNumber;
            lblAvgOfAvgsAverageGC.Text = neutralNumber;
            lblAvgOfAvgsStdevGC.Text = neutralNumber;
            lblAvgOfAvgsMaxGC.Text = neutralNumber;
            lblAvgOfAvgsMinGC.Text = neutralNumber;
            lblAvgOfAvgsMedianGC.Text = neutralNumber;
        }

        private void btnMoveDeleteFiles_Click(object sender, EventArgs e)
        {
            List<string> filesToDelete = new List<string>();
            List<string> filesToDeleteCsv = new List<string>();
            List<string> filesToDeletePdf = new List<string>();
            int stateMachine = 0;
            //filesToDelete = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
            filesToDeleteCsv = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "csv", "side");
            filesToDeletePdf = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
            filesToDelete.AddRange(filesToDeletePdf);
            filesToDelete.AddRange(filesToDeleteCsv);

            DialogResult dialogScan = MessageBox.Show("Would you like to delete all files related to this scan?" + "" +
                        "Select Accordingly" +
                        Environment.NewLine +
                        Environment.NewLine + "Yes --> Recommended if any scan images were found to be bad" +
                        Environment.NewLine + "No",
                       "Delete all scan files", MessageBoxButtons.YesNo);

            //Set subfolder name based on user selection
            if (dialogScan == DialogResult.Yes)
            {
                //STATE 0: Neutral State
                if (stateMachine == 0)
                {
                    stateMachine = 10;
                }

                //STATE 10: check if files to transfer is null
                if (stateMachine == 10)
                {
                    if (filesToDelete != null)
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
                        stateMachine = 30;
                    }
                    else
                    {
                        DialogResult dialog = MessageBox.Show("Drop down selection is empty. Please make sure to select lot datetime you want to move",
                        "Empty Drop Down Selection", MessageBoxButtons.OK);
                        stateMachine = 1000;
                    }
                }

                //STATE 70: check that files copying has finished and delete files from temp folder 
                if (stateMachine == 30)
                {
                    Thread.Sleep(100);
                    bool isfileinuse = true;

                    for (int i = 0; i < 10000; i++)
                    {
                        foreach (string file in filesToDelete)
                        {
                            isfileinuse = filesFunctions.IsFileLocked(file);
                            if (isfileinuse == true)
                            {
                                Console.WriteLine("----------------Files In Use-----------------");
                                Console.WriteLine("file in used.");
                                Console.WriteLine(file);
                                //break;
                            }
                            else
                            {
                                int idx = filesToDelete.IndexOf(file);
                                File.Delete(file);
                                filesToDelete.RemoveAt(idx);
                                break;
                            }
                        }
                    }
                    if (isfileinuse == false)
                    {
                        stateMachine = 80;
                    }
                    else
                    {
                        DialogResult dialog = MessageBox.Show("Files in use timeout. " +
                           "Unable to delete files. Files are in use. At times, this can be due to uploading files to a cloud location");
                        stateMachine = 1000;
                    }
                }

                //STATE 80: Operation Completed
                if (stateMachine == 80)
                {
                    DialogResult dialog = MessageBox.Show("File deletion completed", "Operation Complete");
                    stateMachine = 1000;
                }

                //STATE 1000: final state
                if (stateMachine == 1000)
                {
                }
            }
            else if (dialogScan == DialogResult.No)
            {
                
            }         
        }
    }
}
