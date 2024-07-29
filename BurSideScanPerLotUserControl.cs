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
using User_Interface.Main;
using static User_Interface.Main.GF;
using Amazon.CognitoIdentity.Model;
using System.IO.Compression;
using static User_Interface.SetupService;
using User_Interface._1to1TraceabilityFunctions;

namespace User_Interface
{
    public partial class BurSideScanPerLotUserControl : UserControl
    {
        //INTIALIZE CLASSES
        FilesFunctions filesFunctions = new FilesFunctions(); //Reusable Functions
        PDFFunctions pdfFunctions = new PDFFunctions(); // Allows Merging of PDF
        CSVFunctions csvFunctions = new CSVFunctions();
        PDFViewer pdfViewer = new PDFViewer(); // Class to view PDF
        MathFunctions mathFunctions = new MathFunctions();
        FileManagement fileManagement = new FileManagement();
        GF gf = new GF();
        //Global Variables
        string rootFolderPath;
        string tempFolderPath;
        string rootFolderSideScanPath;
        string csvStringToWrite = "";
        List<string> csvDataToWrite = new List<string>();

        int maxNumberOfBursPerScan = 8;
        int maxNumberOfBursPerLot = 2000;
        int maxNumberOfSegments = 3;
        public BurSideScanPerLotUserControl()
        {
            InitializeComponent();

            try
            {
                //Create folde paths based on txt file constumables
                //tempFolderPath = mPConfig.TempFolderName;
                //rootFolderSideScanPath = mPConfig.TempFolderName;
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


        }

        private void LoadSettings()
        {
            tempFolderPath = mPConfig.TempFolderName;
            rootFolderSideScanPath = mPConfig.TempFolderName;
        }

        private void btnScanTemporaryFolder_Click(object sender, EventArgs e)
        {
            try
            {
                LoadSettings();

                var test = gf.GetLotNumbers(true);
                if (test.Count == 0)
                {
                    MessageBox.Show("No files found", "Attention");
                }
                var distinctLotNumbers = test.Distinct().ToList();
                dropdownDistinctLotsDateTime.Items.Clear();
                foreach (string ln in distinctLotNumbers)
                {
                    dropdownDistinctLotsDateTime.Items.Add(ln);
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
              //  StateMachine();
                StateMachine2();
            }
        }

        //STATE MACHINE TO RETRIEVE ALL VALUES FROM ALL PDFS


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

        private void StateMachine()
        {
            int stateMachine = 0; // state machine 
            int nextBurSavedIndex = 0;

            List<string> filePathsToExtractDataFrom = new List<string>(); //has file paths for all PDFs results for data to be extracted from 
            string[,] grainCoverageValues = new string[maxNumberOfBursPerLot, maxNumberOfSegments + 2]; //list of grain coverage values (double)
            double[,] meanGrainProtusionValues = new double[maxNumberOfBursPerScan, maxNumberOfSegments + 1]; //list of mean grain protusion values (double)

            string[,] burResultsMatrix = new string[maxNumberOfBursPerLot, 10];

            double[,] grainCoverageStatistics = new double[maxNumberOfBursPerLot, 5]; //a 8 rows by 5 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min
            double[,] meanGrainProtusionStatistics = new double[maxNumberOfBursPerLot, 5];

            double[] grainCoverageAvgOfAvgStatistics = new double[5]; //a 8 rows by 5 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min
            double[] meanGrainProtusionAvgOfAvgStatistics = new double[5];

            string[] grainCoverageResults = new string[maxNumberOfBursPerScan]; //list of grain coverage results (strings)
            string[] meanGrainProtusionResults = new string[maxNumberOfBursPerScan]; //list of mean grain protusion results (strings)
            string[] burResults = new string[maxNumberOfBursPerScan];

            //List<string> fileNameExtractedData = new List<string>(); //
            //List<List<string>> fileNameExtractedData = new List<List<string>>(); //list of lists with each file scanner ID

            List<int>[] meanGrainProtusionResultsArray = new List<int>[maxNumberOfBursPerScan];

            //STATE 0: Starting State Machine
            if (stateMachine == 0)
            {
                stateMachine = 10;
            }

            //STATE 10: Check That File Path List is Not Null
            if (stateMachine == 10)
            {
                filePathsToExtractDataFrom = filesFunctions.FindFilePathsWithDistinctLots(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
                if (filePathsToExtractDataFrom == null)
                {
                    MessageBox.Show("Lot number not found", "Action Required");
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

                nextBurSavedIndex = 0;
                foreach (string file in filePathsToExtractDataFrom)
                {
                    Console.WriteLine(file);
                    List<string> fileData = new List<string>();
                    fileData = filesFunctions.ExtractInfoFromScanFile(file, "side");
                    string lotNumber = fileData[0];
                    string scanDateTime = fileData[1];
                    string burIndex = fileData[2];
                    string systemIdentifier = fileData[3];
                    //----
                    string scanDate = fileData[4];
                    string scanTime = fileData[5];
                    string burPositionString = fileData[6];
                    string segmentNumberString = fileData[7];
                    string burIdentifier = fileData[0] + "_" + fileData[1] + "_" + fileData[2] + "_" + fileData[3];

                    //the bur Identifier is : <lot number>_<scandatetime>_<bur index>_<system identifier>
                    Console.WriteLine(burIdentifier);

                    List<double> primeNumbers = new List<double>();

                    var results = pdfFunctions.ExtractMGPAndGCFromPDF(@file);
                    double grainCoverageResult = results.Item1; //grain coverage extracted from PDf
                    double meanGrainProtusionResult = results.Item2; // mean grain protusion extracted from PDF
                    //string segmentNumberString; //segment number retrieved from pdf file name
                    int segmentNumber = 100;
                    int burPosition = 100;

                    bool burIdentifierFound = false;
                    string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);

                    csvStringToWrite = (burPositionString + "," + segmentNumberString + "," + lotNumber + "," + systemIdentifier + "," + scanDate + "," + scanTime + "," + grainCoverageResult.ToString() + "," + meanGrainProtusionResult.ToString());


                    try
                    {
                        //string burPositionString = Between(fileNameNoExtension, "Position", "_Segment");
                        //string[] fileNameNoExtensionParsed = Regex.Split(fileNameNoExtension, "_Segment");

                        //string segmentNumberString = fileNameNoExtensionParsed[1];
                        Int32.TryParse(segmentNumberString, out segmentNumber);
                        Int32.TryParse(burPositionString, out burPosition);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to retrieve bur position from PDF name", "Action Required");
                        stateMachine = 1000;
                        break;
                    }

                    if (burPosition > 0 && burPosition <= maxNumberOfBursPerScan && segmentNumber > 0 && segmentNumber <= 3)
                    {
                        csvDataToWrite.Add(csvStringToWrite);
                        for (int i = 0; i < maxNumberOfBursPerLot; i++)
                        {
                            string matrixBurIdentifier = burResultsMatrix[i, 0];

                            if (burIdentifier == matrixBurIdentifier)
                            {
                                burIdentifierFound = true;
                                burResultsMatrix[i, (segmentNumber)] = grainCoverageResult.ToString();
                                burResultsMatrix[i, (segmentNumber + 3)] = meanGrainProtusionResult.ToString();
                                burResultsMatrix[i, 7] = burPosition.ToString();
                                break;
                            }
                        }

                        if (burIdentifierFound == false)
                        {
                            burResultsMatrix[nextBurSavedIndex, 0] = burIdentifier;
                            burResultsMatrix[nextBurSavedIndex, 7] = burPosition.ToString();
                            burResultsMatrix[nextBurSavedIndex, segmentNumber] = grainCoverageResult.ToString();
                            burResultsMatrix[nextBurSavedIndex, (segmentNumber + 3)] = meanGrainProtusionResult.ToString();
                            nextBurSavedIndex += 1;
                        }
                    }
                }

                for (int i = 0; i < (nextBurSavedIndex); i++)
                {

                    string test = burResultsMatrix[i, 0];
                    Console.WriteLine("bur ID: " + burResultsMatrix[i, 0] +
                        ", S1 -GC: " + burResultsMatrix[i, 1] + ", S2-GC: " + burResultsMatrix[i, 2] + ", S3-GC: " + burResultsMatrix[i, 3] +
                         ", S1-MGP: " + burResultsMatrix[i, 4] + ", S2-MGP: " + burResultsMatrix[i, 5] + ", S3-MGP: " + burResultsMatrix[i, 6] +
                         ", bur Number: " + burResultsMatrix[i, 7]
                         );

                    string[] grainCoverageResultsStringArray = new string[3]; //a 8 rows by 5 col matrix
                    grainCoverageResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 1, 3);
                    double[] grainCoverageResultsDoubleArray = mathFunctions.StringToDoubleArray(grainCoverageResultsStringArray);
                }
                /*
                for (int i = 0; i < 40; i++)
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
                */
                stateMachine = 30;
            }

            //STATE 30: Calculate Averages of Average Results
            if (stateMachine == 30)
            {
                for (int i = 0; i < nextBurSavedIndex; i++)
                {

                    Console.WriteLine("bur ID: " + burResultsMatrix[i, 0] +
                        ", S1 -GC: " + burResultsMatrix[i, 1] + ", S2-GC: " + burResultsMatrix[i, 2] + ", S3-GC: " + burResultsMatrix[i, 3] +
                         ", S1-MGP: " + burResultsMatrix[i, 4] + ", S2-MGP: " + burResultsMatrix[i, 5] + ", S3-MGP: " + burResultsMatrix[i, 6] +
                         ", bur Number: " + burResultsMatrix[i, 7]
                         );

                    string[] grainCoverageResultsStringArray = new string[3]; //a 8 rows by 3 col matrix
                    grainCoverageResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 1, 3);
                    double[] grainCoverageResultsDoubleArray = mathFunctions.StringToDoubleArray(grainCoverageResultsStringArray);

                    string[] MeanGrainProtusionResultsStringArray = new string[3]; //a 8 rows by 5 col matrix
                    MeanGrainProtusionResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 4, 6);
                    double[] MeanGrainProtusionResultDoubleArray = mathFunctions.StringToDoubleArray(MeanGrainProtusionResultsStringArray);

                    for (int j = 0; j < 3; j++)
                    {
                        Console.WriteLine(j.ToString() + ",GC: " + grainCoverageResultsDoubleArray[j].ToString() +
                            ", MGP: " + MeanGrainProtusionResultDoubleArray[j].ToString());
                    }

                    grainCoverageStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 1] = mathFunctions.CalculateAverage(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 3] = mathFunctions.CalculateMaxValue(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 4] = mathFunctions.CalculateMinValue(grainCoverageResultsDoubleArray);

                    meanGrainProtusionStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 1] = mathFunctions.CalculateAverage(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 3] = mathFunctions.CalculateMaxValue(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 4] = mathFunctions.CalculateMinValue(MeanGrainProtusionResultDoubleArray);
                }



                /*
                for (int i = 0; i < nextBurSavedIndex; i++)
                    {
                        Console.WriteLine("-------Table Index" + (i).ToString() + "-------");
                        Console.WriteLine("GRAIN COVERAGE:");
                        Console.WriteLine(" number of non zeros: " + (grainCoverageStatistics[i, 0]).ToString());
                        Console.WriteLine(" average:" + grainCoverageStatistics[i, 1].ToString());
                        Console.WriteLine(" standard deviation: " + grainCoverageStatistics[i, 2].ToString());
                        Console.WriteLine(" max: " + grainCoverageStatistics[i, 3].ToString());
                        Console.WriteLine(" min: " + grainCoverageStatistics[i, 4].ToString());
                        Console.WriteLine("MEAN GRAIN PROTUSION:");
                        Console.WriteLine(" number of non zeros: " + (meanGrainProtusionStatistics[i, 0]).ToString());
                        Console.WriteLine(" average: " + meanGrainProtusionStatistics[i, 1].ToString());
                        Console.WriteLine(" standard deviation: " + meanGrainProtusionStatistics[i, 2].ToString());
                        Console.WriteLine(" max: " + meanGrainProtusionStatistics[i, 3].ToString());
                        Console.WriteLine(" min: " + meanGrainProtusionStatistics[i, 4].ToString());
                    }
                    */
                stateMachine = 40;

            }

            //STATE 40: find average of averages
            if (stateMachine == 40)
            {
                double[] grainCoverageAvgOfAvgsArray = new double[maxNumberOfBursPerScan];
                grainCoverageAvgOfAvgsArray = mathFunctions.GetColumn(grainCoverageStatistics, 1);

                double[] meanGrainProtusionAvgOfAvgsArray = new double[maxNumberOfBursPerScan];
                meanGrainProtusionAvgOfAvgsArray = mathFunctions.GetColumn(meanGrainProtusionStatistics, 1);


                for (int i = 0; i < nextBurSavedIndex; i++)
                {
                    //grainCoverageAvgOfAvgStatistics
                    grainCoverageAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(grainCoverageAvgOfAvgsArray);

                    //MeanGrainProtusionAvgOfAvgStatistics
                    meanGrainProtusionAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(meanGrainProtusionAvgOfAvgsArray);
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

                //Avergae of Averages Mean Grain Protusion Statistics
                lblAvgOfAvgsNumberOfDataPointsMGP.Text = meanGrainProtusionAvgOfAvgStatistics[0].ToString();
                lblAvgOfAvgsAverageMGP.Text = meanGrainProtusionAvgOfAvgStatistics[1].ToString();
                lblAvgOfAvgsStdevMGP.Text = meanGrainProtusionAvgOfAvgStatistics[2].ToString();
                lblAvgOfAvgsMaxMGP.Text = meanGrainProtusionAvgOfAvgStatistics[3].ToString();
                lblAvgOfAvgsMinMGP.Text = meanGrainProtusionAvgOfAvgStatistics[4].ToString();

                stateMachine = 1000;
            }

            if (stateMachine == 1000)
            {
                btnMoveDeleteFiles.Enabled = true;
                btnOpenPDFReader.Enabled = false;
                btnReadPDFResults.Enabled = false;
                btnClearScreen.Enabled = true;
                btnScanTemporaryFolder.Enabled = false;
                dropdownDistinctLotsDateTime.Enabled = false;
            }
        }


        private void StateMachine2()
        {
            int stateMachine = 0; // state machine 
            int nextBurSavedIndex = 0;

            List<string> filePathsToExtractDataFrom = new List<string>(); //has file paths for all PDFs results for data to be extracted from 
            string[,] grainCoverageValues = new string[maxNumberOfBursPerLot, maxNumberOfSegments + 2]; //list of grain coverage values (double)
            double[,] meanGrainProtusionValues = new double[maxNumberOfBursPerScan, maxNumberOfSegments + 1]; //list of mean grain protusion values (double)

            string[,] burResultsMatrix = new string[maxNumberOfBursPerLot, 10];

            double[,] grainCoverageStatistics = new double[maxNumberOfBursPerLot, 5]; //a 8 rows by 5 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min
            double[,] meanGrainProtusionStatistics = new double[maxNumberOfBursPerLot, 5];

            double[] grainCoverageAvgOfAvgStatistics = new double[5]; //a 8 rows by 5 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min
            double[] meanGrainProtusionAvgOfAvgStatistics = new double[5];

            string[] grainCoverageResults = new string[maxNumberOfBursPerScan]; //list of grain coverage results (strings)
            string[] meanGrainProtusionResults = new string[maxNumberOfBursPerScan]; //list of mean grain protusion results (strings)
            string[] burResults = new string[maxNumberOfBursPerScan];

            //List<string> fileNameExtractedData = new List<string>(); //
            //List<List<string>> fileNameExtractedData = new List<List<string>>(); //list of lists with each file scanner ID

            List<int>[] meanGrainProtusionResultsArray = new List<int>[maxNumberOfBursPerScan];

            //STATE 0: Starting State Machine
            if (stateMachine == 0)
            {
                stateMachine = 10;
            }

            //STATE 10: Check That File Path List is Not Null
            if (stateMachine == 10)
            {
                filePathsToExtractDataFrom = filesFunctions.FindFilePathsWithDistinctLots(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
                if (filePathsToExtractDataFrom.Count > 0)
                {
                    string file = filePathsToExtractDataFrom[0];
                    List<string> fileData = filesFunctions.ExtractInfoFromScanFile(file, "side");

                    string lotNumber = fileData[0];
                    if (lotNumber.Length != 10)
                    {
                        LotNumberCorrectorForm correctorForm = new LotNumberCorrectorForm(lotNumber);
                        correctorForm.ShowDialog();
                        fileManagement.RenameFiles(tempFolderPath, lotNumber, Form1.form1Instance.correctLotNumber, true);

                        string csvFileNameInDict = file;
                        string correctCSVFileName = csvFileNameInDict.Replace(lotNumber, Form1.form1Instance.correctLotNumber);
                        //availableCSVFiles[dropdownDistinctLotsDateTime.Text] = availableCSVFiles[dropdownDistinctLotsDateTime.Text].Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        //dropdownDistinctLotsDateTime.Text = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        lotNumber = Form1.form1Instance.correctLotNumber;
                        dropdownDistinctLotsDateTime.Text = lotNumber;
                        filePathsToExtractDataFrom = filesFunctions.FindFilePathsWithDistinctLots(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
                    }

                    stateMachine = 20;
                }
                else
                {
                    MessageBox.Show("Lot number not found", "Action Required");
                    stateMachine = 1000;
                }
            }

            //STATE 20: Extract Mean Grain Protusion and Grain Coverage From PDFs
            if (stateMachine == 20)
            {

                nextBurSavedIndex = 0;
                foreach (string file in filePathsToExtractDataFrom)
                {
                    Console.WriteLine(file);
                    List<string> fileData = new List<string>();
                    fileData = filesFunctions.ExtractInfoFromScanFile(file, "side");
                    string lotNumber = fileData[0];
                    string scanDateTime = fileData[1];
                    string burIndex = fileData[2];
                    string systemIdentifier = fileData[3];
                    //----
                    string scanDate = fileData[4];
                    string scanTime = fileData[5];
                    string burPositionString = fileData[6];
                    string segmentNumberString = fileData[7];
                    string burIdentifier = lotNumber + "_" + scanDateTime + "_" + burIndex + "_" + systemIdentifier;

                    //the bur Identifier is : <lot number>_<scandatetime>_<bur index>_<system identifier>
                    Console.WriteLine(burIdentifier);

                    List<double> primeNumbers = new List<double>();

                    var results = pdfFunctions.ExtractMGPAndGCFromPDF(@file);
                    double grainCoverageResult = results.Item1; //grain coverage extracted from PDf
                    double meanGrainProtusionResult = results.Item2; // mean grain protusion extracted from PDF
                    //string segmentNumberString; //segment number retrieved from pdf file name
                    int segmentNumber = 100;
                    int burPosition = 100;

                    bool burIdentifierFound = false;
                    string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);

                    csvStringToWrite = (burPositionString + "," + segmentNumberString + "," + lotNumber + "," + systemIdentifier + "," + scanDate + "," + scanTime + "," + grainCoverageResult.ToString() + "," + meanGrainProtusionResult.ToString());


                    try
                    {
                        //string burPositionString = Between(fileNameNoExtension, "Position", "_Segment");
                        //string[] fileNameNoExtensionParsed = Regex.Split(fileNameNoExtension, "_Segment");

                        //string segmentNumberString = fileNameNoExtensionParsed[1];
                        Int32.TryParse(segmentNumberString, out segmentNumber);
                        Int32.TryParse(burPositionString, out burPosition);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to retrieve bur position from PDF name", "Action Required");
                        stateMachine = 1000;
                        break;
                    }

                    if (burPosition > 0 && burPosition <= maxNumberOfBursPerScan && segmentNumber > 0 && segmentNumber <= 3)
                    {
                        csvDataToWrite.Add(csvStringToWrite);
                        for (int i = 0; i < maxNumberOfBursPerLot; i++)
                        {
                            string matrixBurIdentifier = burResultsMatrix[i, 0];

                            if (burIdentifier == matrixBurIdentifier)
                            {
                                burIdentifierFound = true;
                                burResultsMatrix[i, (segmentNumber)] = grainCoverageResult.ToString();
                                burResultsMatrix[i, (segmentNumber + 3)] = meanGrainProtusionResult.ToString();
                                burResultsMatrix[i, 7] = burPosition.ToString();
                                break;
                            }
                        }

                        if (burIdentifierFound == false)
                        {
                            burResultsMatrix[nextBurSavedIndex, 0] = burIdentifier;
                            burResultsMatrix[nextBurSavedIndex, 7] = burPosition.ToString();
                            burResultsMatrix[nextBurSavedIndex, segmentNumber] = grainCoverageResult.ToString();
                            burResultsMatrix[nextBurSavedIndex, (segmentNumber + 3)] = meanGrainProtusionResult.ToString();
                            nextBurSavedIndex += 1;
                        }
                    }
                }

                for (int i = 0; i < (nextBurSavedIndex); i++)
                {

                    string test = burResultsMatrix[i, 0];
                    Console.WriteLine("bur ID: " + burResultsMatrix[i, 0] +
                        ", S1 -GC: " + burResultsMatrix[i, 1] + ", S2-GC: " + burResultsMatrix[i, 2] + ", S3-GC: " + burResultsMatrix[i, 3] +
                         ", S1-MGP: " + burResultsMatrix[i, 4] + ", S2-MGP: " + burResultsMatrix[i, 5] + ", S3-MGP: " + burResultsMatrix[i, 6] +
                         ", bur Number: " + burResultsMatrix[i, 7]
                         );

                    string[] grainCoverageResultsStringArray = new string[3]; //a 8 rows by 5 col matrix
                    grainCoverageResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 1, 3);
                    double[] grainCoverageResultsDoubleArray = mathFunctions.StringToDoubleArray(grainCoverageResultsStringArray);
                }
                /*
                for (int i = 0; i < 40; i++)
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
                */
                stateMachine = 30;
            }

            //STATE 30: Calculate Averages of Average Results
            if (stateMachine == 30)
            {
                for (int i = 0; i < nextBurSavedIndex; i++)
                {

                    Console.WriteLine("bur ID: " + burResultsMatrix[i, 0] +
                        ", S1 -GC: " + burResultsMatrix[i, 1] + ", S2-GC: " + burResultsMatrix[i, 2] + ", S3-GC: " + burResultsMatrix[i, 3] +
                         ", S1-MGP: " + burResultsMatrix[i, 4] + ", S2-MGP: " + burResultsMatrix[i, 5] + ", S3-MGP: " + burResultsMatrix[i, 6] +
                         ", bur Number: " + burResultsMatrix[i, 7]
                         );

                    string[] grainCoverageResultsStringArray = new string[3]; //a 8 rows by 3 col matrix
                    grainCoverageResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 1, 3);
                    double[] grainCoverageResultsDoubleArray = mathFunctions.StringToDoubleArray(grainCoverageResultsStringArray);

                    string[] MeanGrainProtusionResultsStringArray = new string[3]; //a 8 rows by 5 col matrix
                    MeanGrainProtusionResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 4, 6);
                    double[] MeanGrainProtusionResultDoubleArray = mathFunctions.StringToDoubleArray(MeanGrainProtusionResultsStringArray);

                    for (int j = 0; j < 3; j++)
                    {
                        Console.WriteLine(j.ToString() + ",GC: " + grainCoverageResultsDoubleArray[j].ToString() +
                            ", MGP: " + MeanGrainProtusionResultDoubleArray[j].ToString());
                    }

                    grainCoverageStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 1] = mathFunctions.CalculateAverage(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 3] = mathFunctions.CalculateMaxValue(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 4] = mathFunctions.CalculateMinValue(grainCoverageResultsDoubleArray);

                    meanGrainProtusionStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 1] = mathFunctions.CalculateAverage(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 3] = mathFunctions.CalculateMaxValue(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 4] = mathFunctions.CalculateMinValue(MeanGrainProtusionResultDoubleArray);
                }



                /*
                for (int i = 0; i < nextBurSavedIndex; i++)
                    {
                        Console.WriteLine("-------Table Index" + (i).ToString() + "-------");
                        Console.WriteLine("GRAIN COVERAGE:");
                        Console.WriteLine(" number of non zeros: " + (grainCoverageStatistics[i, 0]).ToString());
                        Console.WriteLine(" average:" + grainCoverageStatistics[i, 1].ToString());
                        Console.WriteLine(" standard deviation: " + grainCoverageStatistics[i, 2].ToString());
                        Console.WriteLine(" max: " + grainCoverageStatistics[i, 3].ToString());
                        Console.WriteLine(" min: " + grainCoverageStatistics[i, 4].ToString());
                        Console.WriteLine("MEAN GRAIN PROTUSION:");
                        Console.WriteLine(" number of non zeros: " + (meanGrainProtusionStatistics[i, 0]).ToString());
                        Console.WriteLine(" average: " + meanGrainProtusionStatistics[i, 1].ToString());
                        Console.WriteLine(" standard deviation: " + meanGrainProtusionStatistics[i, 2].ToString());
                        Console.WriteLine(" max: " + meanGrainProtusionStatistics[i, 3].ToString());
                        Console.WriteLine(" min: " + meanGrainProtusionStatistics[i, 4].ToString());
                    }
                    */
                stateMachine = 40;

            }

            //STATE 40: find average of averages
            if (stateMachine == 40)
            {
                double[] grainCoverageAvgOfAvgsArray = new double[maxNumberOfBursPerScan];
                grainCoverageAvgOfAvgsArray = mathFunctions.GetColumn(grainCoverageStatistics, 1);

                double[] meanGrainProtusionAvgOfAvgsArray = new double[maxNumberOfBursPerScan];
                meanGrainProtusionAvgOfAvgsArray = mathFunctions.GetColumn(meanGrainProtusionStatistics, 1);


                for (int i = 0; i < nextBurSavedIndex; i++)
                {
                    //grainCoverageAvgOfAvgStatistics
                    grainCoverageAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(grainCoverageAvgOfAvgsArray);

                    //MeanGrainProtusionAvgOfAvgStatistics
                    meanGrainProtusionAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(meanGrainProtusionAvgOfAvgsArray);
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

                //Avergae of Averages Mean Grain Protusion Statistics
                lblAvgOfAvgsNumberOfDataPointsMGP.Text = meanGrainProtusionAvgOfAvgStatistics[0].ToString();
                lblAvgOfAvgsAverageMGP.Text = meanGrainProtusionAvgOfAvgStatistics[1].ToString();
                lblAvgOfAvgsStdevMGP.Text = meanGrainProtusionAvgOfAvgStatistics[2].ToString();
                lblAvgOfAvgsMaxMGP.Text = meanGrainProtusionAvgOfAvgStatistics[3].ToString();
                lblAvgOfAvgsMinMGP.Text = meanGrainProtusionAvgOfAvgStatistics[4].ToString();

                stateMachine = 1000;
            }

            if (stateMachine == 1000)
            {
                btnMoveDeleteFiles.Enabled = true;
                btnOpenPDFReader.Enabled = false;
                btnReadPDFResults.Enabled = false;
                btnClearScreen.Enabled = true;
                btnScanTemporaryFolder.Enabled = false;
                dropdownDistinctLotsDateTime.Enabled = false;
            }
        }


        private void StateMachine1()
        {
            int stateMachine = 0; // state machine 
            int nextBurSavedIndex = 0;

            List<string> filePathsToExtractDataFrom = new List<string>(); //has file paths for all PDFs results for data to be extracted from 
            string[,] grainCoverageValues = new string[maxNumberOfBursPerLot, maxNumberOfSegments + 2]; //list of grain coverage values (double)
            double[,] meanGrainProtusionValues = new double[maxNumberOfBursPerScan, maxNumberOfSegments + 1]; //list of mean grain protusion values (double)

            string[,] burResultsMatrix = new string[maxNumberOfBursPerLot, 10];

            double[,] grainCoverageStatistics = new double[maxNumberOfBursPerLot, 5]; //a 8 rows by 5 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min
            double[,] meanGrainProtusionStatistics = new double[maxNumberOfBursPerLot, 5];

            double[] grainCoverageAvgOfAvgStatistics = new double[5]; //a 8 rows by 5 col matrix
            //columns will be number of data points(non zero), avg, stdev, max, min
            double[] meanGrainProtusionAvgOfAvgStatistics = new double[5];

            string[] grainCoverageResults = new string[maxNumberOfBursPerScan]; //list of grain coverage results (strings)
            string[] meanGrainProtusionResults = new string[maxNumberOfBursPerScan]; //list of mean grain protusion results (strings)
            string[] burResults = new string[maxNumberOfBursPerScan];

            //List<string> fileNameExtractedData = new List<string>(); //
            //List<List<string>> fileNameExtractedData = new List<List<string>>(); //list of lists with each file scanner ID

            List<int>[] meanGrainProtusionResultsArray = new List<int>[maxNumberOfBursPerScan];

            //STATE 0: Starting State Machine
            if (stateMachine == 0)
            {
                stateMachine = 10;
            }
            Dictionary<string, List<string>> info = new Dictionary<string, List<string>>();

            //STATE 10: Check That File Path List is Not Null
            if (stateMachine == 10)
            {
                //info = gf.FindLotFilePaths(dropdownDistinctLotsDateTime.Text, true, true);
                filePathsToExtractDataFrom = filesFunctions.FindFilePathsWithDistinctLots(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
                if (filePathsToExtractDataFrom.Count > 0)
                {
                    string file = filePathsToExtractDataFrom[0];
                    List<string> fileData = filesFunctions.ExtractInfoFromScanFile(file, "side");

                    string lotNumber = fileData[0];
                    if (lotNumber.Length != 9)
                    {
                        LotNumberCorrectorForm correctorForm = new LotNumberCorrectorForm(lotNumber);
                        correctorForm.ShowDialog();
                        fileManagement.RenameFiles(tempFolderPath, lotNumber, Form1.form1Instance.correctLotNumber, true);

                        string csvFileNameInDict = file;
                        string correctCSVFileName = csvFileNameInDict.Replace(lotNumber, Form1.form1Instance.correctLotNumber);
                        //availableCSVFiles[dropdownDistinctLotsDateTime.Text] = availableCSVFiles[dropdownDistinctLotsDateTime.Text].Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        //dropdownDistinctLotsDateTime.Text = dropdownDistinctLotsDateTime.Text.Replace(burScanFile[0], Form1.form1Instance.correctLotNumber);
                        lotNumber = Form1.form1Instance.correctLotNumber;
                    }

                    stateMachine = 20;
                }
                else
                {
                    MessageBox.Show("Lot number not found", "Action Required");
                    stateMachine = 1000;
                }

            }
            //STATE 20: Extract Mean Grain Protusion and Grain Coverage From PDFs
            if (stateMachine == 20)
            {

                nextBurSavedIndex = 0;
                foreach (string file in filePathsToExtractDataFrom)
                {
                    Console.WriteLine(file);
                    List<string> fileData = filesFunctions.ExtractInfoFromScanFile(file, "side");

                    string lotNumber = fileData[0];
                    string scanDateTime = fileData[2] + "_" + fileData[3];
                    string burIndex = fileData[4];
                    string systemIdentifier = fileData[1];
                    string scanDate = fileData[2];
                    string scanTime = fileData[3];
                    string burPositionString = fileData[4];
                    string segmentNumberString = fileData[5];
                    string burIdentifier = lotNumber + "_" + scanDateTime + "_" + burIndex + "_" + systemIdentifier;

                    //the bur Identifier is : <lot number>_<scandatetime>_<bur index>_<system identifier>
                    Console.WriteLine(burIdentifier);

                    List<double> primeNumbers = new List<double>();

                    var results = pdfFunctions.ExtractMGPAndGCFromPDF(@file);
                    double grainCoverageResult = results.Item1; //grain coverage extracted from PDf
                    double meanGrainProtusionResult = results.Item2; // mean grain protusion extracted from PDF
                    //string segmentNumberString; //segment number retrieved from pdf file name
                    int segmentNumber = 100;
                    int burPosition = 100;

                    bool burIdentifierFound = false;
                    string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);

                    csvStringToWrite = (burPositionString + "," + segmentNumberString + "," + lotNumber + "," + systemIdentifier + "," + scanDate + "," + scanTime + "," + grainCoverageResult.ToString() + "," + meanGrainProtusionResult.ToString());


                    try
                    {
                        //string burPositionString = Between(fileNameNoExtension, "Position", "_Segment");
                        //string[] fileNameNoExtensionParsed = Regex.Split(fileNameNoExtension, "_Segment");

                        //string segmentNumberString = fileNameNoExtensionParsed[1];
                        Int32.TryParse(segmentNumberString, out segmentNumber);
                        Int32.TryParse(burPositionString, out burPosition);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to retrieve bur position from PDF name", "Action Required");
                        stateMachine = 1000;
                        break;
                    }

                    if (burPosition > 0 && burPosition <= maxNumberOfBursPerScan && segmentNumber > 0 && segmentNumber <= 3)
                    {
                        csvDataToWrite.Add(csvStringToWrite);
                        for (int i = 0; i < maxNumberOfBursPerLot; i++)
                        {
                            string matrixBurIdentifier = burResultsMatrix[i, 0];

                            if (burIdentifier == matrixBurIdentifier)
                            {
                                burIdentifierFound = true;
                                burResultsMatrix[i, (segmentNumber)] = grainCoverageResult.ToString();
                                burResultsMatrix[i, (segmentNumber + 3)] = meanGrainProtusionResult.ToString();
                                burResultsMatrix[i, 7] = burPosition.ToString();
                                break;
                            }
                        }

                        if (burIdentifierFound == false)
                        {
                            burResultsMatrix[nextBurSavedIndex, 0] = burIdentifier;
                            burResultsMatrix[nextBurSavedIndex, 7] = burPosition.ToString();
                            burResultsMatrix[nextBurSavedIndex, segmentNumber] = grainCoverageResult.ToString();
                            burResultsMatrix[nextBurSavedIndex, (segmentNumber + 3)] = meanGrainProtusionResult.ToString();
                            nextBurSavedIndex += 1;
                        }
                    }
                }

                for (int i = 0; i < (nextBurSavedIndex); i++)
                {

                    string test = burResultsMatrix[i, 0];
                    Console.WriteLine("bur ID: " + burResultsMatrix[i, 0] +
                        ", S1 -GC: " + burResultsMatrix[i, 1] + ", S2-GC: " + burResultsMatrix[i, 2] + ", S3-GC: " + burResultsMatrix[i, 3] +
                         ", S1-MGP: " + burResultsMatrix[i, 4] + ", S2-MGP: " + burResultsMatrix[i, 5] + ", S3-MGP: " + burResultsMatrix[i, 6] +
                         ", bur Number: " + burResultsMatrix[i, 7]
                         );

                    //string[] grainCoverageResultsStringArray = new string[3]; //a 8 rows by 5 col matrix
                    //grainCoverageResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 1, 3);
                    //double[] grainCoverageResultsDoubleArray = mathFunctions.StringToDoubleArray(grainCoverageResultsStringArray);
                }

                stateMachine = 30;
            }
            //STATE 30: Calculate Averages of Average Results
            if (stateMachine == 30)
            {
                for (int i = 0; i < nextBurSavedIndex; i++)
                {

                    Console.WriteLine("\nbur ID: " + burResultsMatrix[i, 0] +
                        ", S1 -GC: " + burResultsMatrix[i, 1] + ", S2-GC: " + burResultsMatrix[i, 2] + ", S3-GC: " + burResultsMatrix[i, 3] +
                         ", S1-MGP: " + burResultsMatrix[i, 4] + ", S2-MGP: " + burResultsMatrix[i, 5] + ", S3-MGP: " + burResultsMatrix[i, 6] +
                         ", bur Number: " + burResultsMatrix[i, 7]
                          );

                    string[] grainCoverageResultsStringArray = new string[3]; //a 8 rows by 3 col matrix
                    grainCoverageResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 1, 3);
                    double[] grainCoverageResultsDoubleArray = mathFunctions.StringToDoubleArray(grainCoverageResultsStringArray);

                    string[] MeanGrainProtusionResultsStringArray = new string[3]; //a 8 rows by 5 col matrix
                    MeanGrainProtusionResultsStringArray = mathFunctions.GetRowPerColIndex(burResultsMatrix, i, 4, 6);
                    double[] MeanGrainProtusionResultDoubleArray = mathFunctions.StringToDoubleArray(MeanGrainProtusionResultsStringArray);
                    for (int j = 0; j < 3; j++)
                    {
                        Console.WriteLine(j.ToString() + ",GC: " + grainCoverageResultsDoubleArray[j].ToString() +
                            ", MGP: " + MeanGrainProtusionResultDoubleArray[j].ToString());
                    }

                    grainCoverageStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 1] = mathFunctions.CalculateAverage(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 3] = mathFunctions.CalculateMaxValue(grainCoverageResultsDoubleArray);
                    grainCoverageStatistics[i, 4] = mathFunctions.CalculateMinValue(grainCoverageResultsDoubleArray);

                    meanGrainProtusionStatistics[i, 0] = mathFunctions.FindNumberOfNonZerosNumbers(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 1] = mathFunctions.CalculateAverage(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 2] = mathFunctions.CalculateStandardDeviation(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 3] = mathFunctions.CalculateMaxValue(MeanGrainProtusionResultDoubleArray);
                    meanGrainProtusionStatistics[i, 4] = mathFunctions.CalculateMinValue(MeanGrainProtusionResultDoubleArray);
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


                for (int i = 0; i < nextBurSavedIndex; i++)
                {
                    //grainCoverageAvgOfAvgStatistics
                    grainCoverageAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(grainCoverageAvgOfAvgsArray);
                    grainCoverageAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(grainCoverageAvgOfAvgsArray);

                    //MeanGrainProtusionAvgOfAvgStatistics
                    meanGrainProtusionAvgOfAvgStatistics[0] = mathFunctions.FindNumberOfNonZerosNumbers(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[1] = mathFunctions.CalculateAverage(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[2] = mathFunctions.CalculateStandardDeviation(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[3] = mathFunctions.CalculateMaxValue(meanGrainProtusionAvgOfAvgsArray);
                    meanGrainProtusionAvgOfAvgStatistics[4] = mathFunctions.CalculateMinValue(meanGrainProtusionAvgOfAvgsArray);
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

                //Avergae of Averages Mean Grain Protusion Statistics
                lblAvgOfAvgsNumberOfDataPointsMGP.Text = meanGrainProtusionAvgOfAvgStatistics[0].ToString();
                lblAvgOfAvgsAverageMGP.Text = meanGrainProtusionAvgOfAvgStatistics[1].ToString();
                lblAvgOfAvgsStdevMGP.Text = meanGrainProtusionAvgOfAvgStatistics[2].ToString();
                lblAvgOfAvgsMaxMGP.Text = meanGrainProtusionAvgOfAvgStatistics[3].ToString();
                lblAvgOfAvgsMinMGP.Text = meanGrainProtusionAvgOfAvgStatistics[4].ToString();

                stateMachine = 1000;
            }

            if (stateMachine == 1000)
            {
                btnMoveDeleteFiles.Enabled = true;
                btnOpenPDFReader.Enabled = false;
                btnReadPDFResults.Enabled = false;
                btnClearScreen.Enabled = true;
                btnScanTemporaryFolder.Enabled = false;
                dropdownDistinctLotsDateTime.Enabled = false;
            }
        }

        private void btnClearScreen_Click(object sender, EventArgs e)
        {
            string neutralNumber = "00";
            lblAvgOfAvgsNumberOfDataPointsMGP.Text = neutralNumber;
            lblAvgOfAvgsAverageMGP.Text = neutralNumber;
            lblAvgOfAvgsStdevMGP.Text = neutralNumber;
            lblAvgOfAvgsMaxMGP.Text = neutralNumber;
            lblAvgOfAvgsMinMGP.Text = neutralNumber;

            lblAvgOfAvgsNumberOfDataPointsGC.Text = neutralNumber;
            lblAvgOfAvgsAverageGC.Text = neutralNumber;
            lblAvgOfAvgsStdevGC.Text = neutralNumber;
            lblAvgOfAvgsMaxGC.Text = neutralNumber;
            lblAvgOfAvgsMinGC.Text = neutralNumber;

            btnClearScreen.Enabled = false;
            btnScanTemporaryFolder.Enabled = true;
            btnReadPDFResults.Enabled = true;
            dropdownDistinctLotsDateTime.Enabled = true;
            dropdownDistinctLotsDateTime.Items.Clear();
            dropdownDistinctLotsDateTime.Text = "";
            csvDataToWrite.Clear();
        }

        private void btnMoveDeleteFiles_Click(object sender, EventArgs e)
        {
            int stateMachine = 0; //state machine variable
            //string newDirectoryName = "";
            string machineIdentifier = "";
            string lotNumber = "";
            //string date = "";
            //string time = "";

            List<string> pdfFilesToMove = filesFunctions.FindFilePathsWithDistinctLots(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf", "side");
            List<string> csvFilesToMove = filesFunctions.FindFilePathsWithDistinctLots(tempFolderPath, dropdownDistinctLotsDateTime.Text, "csv", "side");
            List<string> allFilesToMove = pdfFilesToMove;
            string scannedFilesZipDir = tempFolderPath;
            Console.WriteLine(scannedFilesZipDir);
            if (!Directory.Exists(scannedFilesZipDir))
            {
                Directory.CreateDirectory(scannedFilesZipDir);
            }
            if (pdfFilesToMove == null)
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
                if (pdfFilesToMove != null)
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

            //STATE 50: Notify operator of lot, datetime, bad scans, directories and get confirmation
            if (stateMachine == 30)
            {
                //Preparing Strings for operator message. 
                machineIdentifier = Regex.Split(pdfFilesToMove[0], "_")[2]; // Machine ID (SYS1, SYS2, SYS3,...,SYS<system number>)               
                lotNumber = Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[0];

                DialogResult dialogConfirmFilesMovement = MessageBox.Show("Confirm the information below is correct" +
                        Environment.NewLine +
                        Environment.NewLine + "Machine Identifier: " + machineIdentifier +
                        Environment.NewLine + "Lot Number: " + lotNumber +
                        Environment.NewLine + "-------------------- ",
                       //Environment.NewLine + "Files will be moved to: <Root Folder Side Scan>",
                       "Move Files - Confirmation", MessageBoxButtons.YesNo);

                if (dialogConfirmFilesMovement == DialogResult.No)
                {
                    btnOpenPDFReader.Enabled = true;
                    btnMoveDeleteFiles.Enabled = true;
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
            }

            //STATE 60: Copy files to new folder location
            if (stateMachine == 60)
            {
                try
                {
                    if (csvFilesToMove != null)
                    {
                        foreach (string csvFile in csvFilesToMove)
                        {
                            allFilesToMove.Add(csvFile);
                        }
                    }

                    foreach (string file in allFilesToMove)
                    {
                        string fileName = Path.GetFileName(file);
                        string fileNameWOExt = Path.GetFileNameWithoutExtension(fileName);
                        string[] fileInfo = fileNameWOExt.Split('_');

                        string scanZipFileName = $"{fileInfo[0]}_{fileInfo[1]}_{fileInfo[2]}_Side{fileInfo[3]}_{fileInfo[4]}_{fileInfo[5]}";
                        string scanZipFileDir = Path.Combine(scannedFilesZipDir, scanZipFileName);
                        if (!Directory.Exists(scanZipFileDir)) Directory.CreateDirectory(scanZipFileDir);
                        File.Copy(file, $"{scanZipFileDir}\\{fileName}", true);
                    }
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

                foreach (string file in allFilesToMove)
                {
                    Console.WriteLine("files to transfer length = " + allFilesToMove.Count().ToString());
                    isfileinuse = filesFunctions.IsFileLocked(file);
                    if (isfileinuse == true)
                    {
                        Console.WriteLine("----------------Files In Use-----------------");
                        Console.WriteLine("file in used. Copying process not finished");
                        Console.WriteLine(file);
                        //break;
                    }
                    else
                    {
                        int idx = allFilesToMove.IndexOf(file);
                        File.Delete(file);
                        Console.WriteLine("File deleted: " + file);
                        //allFilesToMove.RemoveAt(idx);
                    }
                    //}
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
            string csvFileName = "Lot_" + dropdownDistinctLotsDateTime.Text + "_SideScan.csv";
            string csvFilePath = tempFolderPath + "\\" + csvFileName;
            string csvFileInZip = Path.Combine(scannedFilesZipDir, csvFileName);

            //STATE 80: Operation Completed
            if (stateMachine == 80)
            {
                Console.WriteLine("------------------CSV FUNCTIONS------------------");
                if (File.Exists(csvFilePath) == false)
                {
                    string header = "Position#, Segment#, Lot#, System ID, Date, Time, Grain Coverage, Mean Grain Protrusion";
                    csvFunctions.WriteToCSV(csvFilePath, header);
                }
                Console.WriteLine("Number of rows: " + csvDataToWrite.Count.ToString());
                foreach (string dataLine in csvDataToWrite)
                {
                    Console.WriteLine(dataLine);

                    if (string.IsNullOrEmpty(dataLine) == false)
                    {
                        csvFunctions.WriteToCSV(csvFilePath, dataLine);
                    }
                }
                //if (File.Exists(csvFileInZip))
                //{
                //    File.Delete(csvFileInZip);
                //}
                if (!File.Exists(csvFileInZip))
                {
                    File.Copy(csvFilePath, csvFileInZip);
                }
                stateMachine = 85;
                Console.WriteLine("------------------CSV FUNCTIONS ENDED------------------");
            }

            if (stateMachine == 85)
            {
                string[] allScans = Directory.GetDirectories(scannedFilesZipDir);
                foreach (string scan in allScans)
                {
                    if (!File.Exists(scan + ".zip"))
                    {
                        ZipFile.CreateFromDirectory(scan, scan + ".zip", CompressionLevel.Fastest, false);
                        if (Directory.Exists(scan))
                        {
                            Directory.Delete(scan, true);
                        }
                    }
                }

                UploadZipFile(scannedFilesZipDir);
                stateMachine = 90;
            }

            //STATE 90: Operation Completed
            if (stateMachine == 90)
            {
                btnMoveDeleteFiles.Enabled = false;
                DialogResult dialog = MessageBox.Show("File management and csv writing operations completed", "Operation Complete");
                stateMachine = 1000;
            }

            //STATE 1000: final state
            if (stateMachine == 1000)
            {

            }
        }

        private async void UploadZipFile(string lotFolderDir)
        {
            try
            {
                string[] ext = { "csv", "zip" };
                string lotNumber = dropdownDistinctLotsDateTime.Text;
                string[] zippedFiles = Directory.GetFiles(lotFolderDir, "*.*").Where(f => ext.Contains(f.Split('.').Last().ToLower())).ToArray();
                foreach (string file in zippedFiles)
                {
                    string fileName = Path.GetFileName(file);

                    if (fileName.ToUpper().Contains("SIDE"))
                    {
                        await Task.Run(() => Program.s3Client.UploadFileMultiPartAsync(true, $"{lotNumber}/{fileName}", file, "STANDARD"));
                    }

                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}
