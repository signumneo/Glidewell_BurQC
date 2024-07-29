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
using USB_Barcode_Scanner;
using System.Diagnostics;
using User_Interface._1to1TraceabilityFunctions;
using static User_Interface.SetupService;

namespace User_Interface
{
    public partial class TipScan1To1 : UserControl
    {
        /* Class Description
        Description: 
            This class is the controls the user interface for 1 to 1 tracability. 

        Parameters:
        ----------
            n/a

        Returns:
        ----------
            n/a

        Example:
        ----------
            n/a

         */
        //INTIALIZE CLASSES
        FilesFunctions filesFunctions = new FilesFunctions(); //Reusable Functions
        //PDFFunctions pdfFunctions = new PDFFunctions(); // Allows Merging of PDF
        //PDFViewer pdfViewer = new PDFViewer(); // Class to view PDFs

        //NEWER CLASSES
        BurScan burScan = new BurScan();
        FileManagement fileManagement = new FileManagement();
        BurBagging burBagging = new BurBagging();

        //INITIALIZES GLOBAL VARIABLES
        string rootFolderPath; // source folder path, saved on text file 
        string tempFolderPath; // root temporary folder
        string csvBaggingFolderPath; // root temporary folder
        List<List<string>> extractedBurData = new List<List<string>>(); // data saved from all PDFs (grain coveraged, mean grain protusion, pass/fails)
        double[,] burResultsCsv = new double[40, 2]; //40 x 2 matrix that containts the mean grain protusion and grain coverage of the burs. 

        string[] roofFolderDirectories; // directories in root folder path

        //INITIALIZE COMPONENT
        public TipScan1To1()
        {
            InitializeComponent();
        }

        //LOAD HOME SCREN
        private void TipScan1To1_Load(object sender, EventArgs e)
        {

            BarcodeScanner barcodeScanner = new BarcodeScanner(txtboxScannedText);
            barcodeScanner.BarcodeScanned += BarcodeScanner_BarcodeScanned;
            //Retrive pass and fail criteria from txt file. 
            try
            {
                //Read folder paths based from txt file constumables (where the software settings are saved)
                rootFolderPath = mPConfig.CSVBaggingFolderPath;
                tempFolderPath = mPConfig.TempFolderName;
                csvBaggingFolderPath = mPConfig.CSVBaggingFolderPath;
                //Console.WriteLine("CSV Bagging folder path: " + csvBaggingFolderPath);
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

            //preare visualization (buttons, pdf viewer, etc)
            btnMoveDeleteFiles.Enabled = false;
            btnOpenPDFReader.Enabled = false;
            btnClearScreen.Enabled = false;
            btnReadPDFResults.Enabled = true;
            btnScanTemporaryFolder.Enabled = true;
            dropdownDistinctLotsDateTime.Enabled = true;
        }

        //BAR CODE SCANNING
        private void BarcodeScanner_BarcodeScanned(object sender, BarcodeScannerEventArgs e)
        {
            string barcodeText = e.Barcode.ToString();
            //txtboxScannedText.Text = barcodeText;
        }

        //RESET BUTTON
        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Are you sure you would like to clear the results from the screen?" +
                "Once the screen is cleared, you will no longer be able to see results for this lot and date/time on this screen.",
                    "Clear Screen Confirmation", MessageBoxButtons.YesNo);

            if (dialog == DialogResult.Yes)
            {
                Clear_All();
            }
        }

        //BUTTON TO SCAN TEMP FOLDER TO FIND DISTINCT DATE_TIME and LOT NUMBRES
        private void btnScanTemporaryFolder_Click(object sender, EventArgs e)
        {
            List<string> tempFolderDistinctLotDateTimes = new List<string>(); // list with distinct dates and times from temp folder
            List<List<string>> csvFileNamesExtractedData = new List<List<string>>();
            try
            {
                csvFileNamesExtractedData = fileManagement.ExtractDataFromCsvFileNames(tempFolderPath);
                //csvFileNamesExtractedData  = filesFunctions.ExtractInfoFromTempFolder(tempFolderPath);
                // READ THE CSV FILES' NAME > IF THERE IS A FILES > REFORMATTED THEN INTO DROPDOWN OPTIONS
                if (csvFileNamesExtractedData != null && csvFileNamesExtractedData.Any())
                {
                    foreach (List<string> csvFile in csvFileNamesExtractedData)
                    {
                        string lotNumber = csvFile[0];
                        string systemIdentifier = csvFile[1];
                        string scanDate = csvFile[2];
                        string scanTime = csvFile[3];
                        string csvFileName = csvFile[4];
                        string toPrint = lotNumber + "|" + systemIdentifier + "|" + scanDate + "|" + scanTime + "|" + csvFileName;
                        Console.WriteLine(toPrint);
                        tempFolderDistinctLotDateTimes.Add(csvFileName);
                    }
                }
                else
                {
                    MessageBox.Show("No files found, Attention");
                }
                dropdownDistinctLotsDateTime.Items.Clear(); // clear any items on drop down menu currently
                foreach (string distinctCsvFile in tempFolderDistinctLotDateTimes)
                {
                    dropdownDistinctLotsDateTime.Items.Add(distinctCsvFile); // populate drop down list 
                }
            }
            catch
            {
                MessageBox.Show("Unable to find files. Make sure that the root file location on settings have a Temp Folder in it. ", "Temp Folder of File structure not found");
            }
        }

        //BUTTON TO MERGE ALL PDFS OF SELECTED DATE_TIME AND LOT, AND OPEN MERGED PDF
        private void btnOpenPDFReader_Click_1(object sender, EventArgs e)
        {

            List<string> filesToMerge = new List<string>(); // will list all pdf file paths to be merged based on drop down menu selection

            Console.WriteLine("----------MERGE TEST----------");
            Console.WriteLine(dropdownDistinctLotsDateTime.Text);
            filesToMerge = fileManagement.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, true, "pdf");
            // ????????????? FOR WHAT ?????????????????????????????
            foreach (string fileToMerge in filesToMerge)
            {
                Console.WriteLine(fileToMerge);
            }

            Console.WriteLine("----------END----------");

            if (filesToMerge != null)
            {
                string mergedFilePath = fileManagement.MergePDFs(filesToMerge, "Merged", tempFolderPath);
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

        //CLEAR BUR RESULTS 
        private void ClearburResultsCsv()
        {
            for (int i = 0; i < 40; i++)
            {
                burResultsCsv[i, 0] = 0;
                burResultsCsv[i, 1] = 0;
            }
        }

        //BUTTON TO READ PDF RESULTS
        private void btnReadPDFResults_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(dropdownDistinctLotsDateTime.Text))
            {
                MessageBox.Show("Please select the lot and datetime results you want to results results from", "Required fields are empty");
            }
            else
            {
                int stateMachine = 0; // state machine for retrieving bur results and populating HMI
                string fullFilePath = tempFolderPath + "\\" + dropdownDistinctLotsDateTime.Text + ".csv"; //full csv file path of select scan on drop down menu

                //BUR RESULTS MATRIX
                //40 x 2 matrix where the row represents the bur, 1st col represents MGP, 2nd col represents GP.
                //If there is no bur in that position, values will be 0
                //double[,] burResultsCsv = new double[40, 2];   
                ClearburResultsCsv();
                burResultsCsv = burScan.RetrieveBurResultsFromCSV(fullFilePath);

                //STATE 0: STARTING STATE MACHINE
                if (stateMachine == 0)
                {
                    stateMachine = 10;
                }

                //STATE 10: POPULATING SCREEN WITH RESULTS
                if (stateMachine == 10)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        string buttonName = "btn_Bur" + (i + 1).ToString();
                        string mtvLabelName = "lbl_MTVBur" + (i + 1).ToString();
                        string cgLabelName = "lbl_GCBur" + (i + 1).ToString();
                        //Update Buttons
                        if (burResultsCsv[i, 0] > 0 && burResultsCsv[i, 1] > 0)
                        {
                            //UPDATE BUR COLLOR
                            panel_Bur_Buttons.Controls[buttonName].BackColor = Color.DeepSkyBlue;
                            //UPDATE MGP VALUE
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = burResultsCsv[i, 0].ToString();
                            //UPDATE GC VALUE
                            panel_Bur_Buttons.Controls[cgLabelName].Text = burResultsCsv[i, 1].ToString();
                        }
                        else
                        {
                            //UPDATE BUR COLLOR
                            panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
                            //UPDATE MGP VALUE
                            panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";
                            //UPDATE GC VALUE
                            panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";
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
            ClearburResultsCsv();
            btnReadPDFResults.Enabled = true;
            txtboxScannedText.Text = "";
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
        }

        //BUTTON TO DO FILE MANAGEMENT       
        private void btnClearScreen_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Are you sure you would like to clear the results from the screen?" +
        "Once the screen is cleared, you will no longer be able to see results for this lot and date/time on this screen.",
        "Clear Screen Confirmation", MessageBoxButtons.YesNo);

            if (dialog == DialogResult.Yes)
            {
                Clear_All();
            }
        }

        //MOVE/DELETE FILES AND CREATE CSV FOR BAGGING  
        private void btnMoveDeleteFiles_Click_1(object sender, EventArgs e)
        {
            string[] rootDirs; // directories in root folder path
            string[] goodScanDirs; // directories in root folder path
            string badscansDirectoryFullPath; //bad scan directory full path
            string goodscansDirectoryFullPath; // good scan directory full path
            string scansDirectoryFullPath; // scans directory full path
            string rescansDirectoryFullPath; // rescans directory full path
            string subFolderName; // subfolder name (Scan or Rescan) --> based on user input
            int stateMachine = 0; //state machine variable
            string newDirectoryName = "";
            List<string> listOfPDFFilesToTransfer = new List<string>(); // list of PDF files to copy/delete
            List<string> listOfCSVFilesToTransfer = new List<string>(); // list of PDF files to copy/delete
            List<string> listOfFilesToTransfer = new List<string>(); // list of PDF and CSV files to copy/delete
            bool[] badScanBurResults = new bool[40];

            bool goodscansDirectoryExists = false; // variable to check if folder good scans exists
            bool badscansDirectoryExists = false; // variable to check if folder bad scans exists
            bool scansDirectoryExists = false; // variable to check if folder scans exists
            bool rescansDirectoryExists = false; // variable to check if folder rescans exists

            goodscansDirectoryFullPath = rootFolderPath + "\\" + "Good Scans"; //directory path for good scans
            badscansDirectoryFullPath = rootFolderPath + "\\" + "Bad Scans"; //directory path for bad scans
            scansDirectoryFullPath = rootFolderPath + "\\" + "Good Scans" + "\\" + "Scans"; //directory for scans inside good scans
            rescansDirectoryFullPath = rootFolderPath + "\\" + "Good Scans" + "\\" + "Rescans"; //directory for rescans inside good scans


            //Check with operator if this is a scan or a rescan 
            DialogResult dialogScan = MessageBox.Show("Is this a re-scan?" + "" +
                        "Select Accordingly" +
                        Environment.NewLine +
                        Environment.NewLine + "Yes --> Files will be moved to Root/Rescan folder" +
                        Environment.NewLine + "No --> Files will be moved to Root/Scan folder",
                       "Scan or Re-Scan", MessageBoxButtons.YesNo);

            //Set subfolder name based on user selection
            if (dialogScan == DialogResult.No)
            {
                subFolderName = "Scans";
            }
            else if (dialogScan == DialogResult.Yes)
            {
                subFolderName = "Rescans";
            }
            else
            {
                subFolderName = "Unkown";
            }

            //disable the pdf reaser and move delete files. 
            btnOpenPDFReader.Enabled = false; //DELETE? 
            btnMoveDeleteFiles.Enabled = false; // DELETE? 

            listOfPDFFilesToTransfer = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "pdf"); //DELETE?
            listOfCSVFilesToTransfer = filesFunctions.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, "csv"); //DELETE?
            listOfFilesToTransfer = fileManagement.FindFilePaths(tempFolderPath, dropdownDistinctLotsDateTime.Text, true, "all");


            //check if the root folder path for the subdirectories exists 
            try
            {
                roofFolderDirectories = Directory.GetDirectories(rootFolderPath);
            }
            catch
            {
                DialogResult dialog = MessageBox.Show("Error find directories in root folder",
                    "Error- Root Folder", MessageBoxButtons.OK);
                stateMachine = 900;
            }


            //Check if the list of files to transfer is not null
            if (listOfFilesToTransfer == null)
            {
                DialogResult dialog = MessageBox.Show("No files found with select lot # and datetime. Please make sure 1) PDFs are in temp folder 2) drop down menu" +
                        "is selected with a lot # and datetime 3) settings is set with proper folder location ",
                    "No files with this Lot # Found", MessageBoxButtons.OK);
                stateMachine = 900;
            }

            //STATE 0: Neutral State
            if (stateMachine == 0)
            {
                stateMachine = 10;
            }

            //STATE 10: check if Bur Scan Text Box is not empty
            if (stateMachine == 10)
            {
                if (!string.IsNullOrEmpty(txtboxScannedText.Text))
                {
                    stateMachine = 20;
                }
                else
                {
                    DialogResult dialog = MessageBox.Show("Please make sure to scan the bar code on the bur fixture, under the 'Scan Bar Code' Section",
                    "Bar Code Field Is Empty!", MessageBoxButtons.OK);
                    stateMachine = 900;
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

            //STATE 30: Check If Main Directories Exist (Good Scans, Bad Scans, Scans, Rescans)
            if (stateMachine == 30)
            {
                rootDirs = Directory.GetDirectories(rootFolderPath);

                foreach (string dir in rootDirs)
                {
                    if (dir == goodscansDirectoryFullPath)
                    {
                        goodscansDirectoryExists = true;
                    }

                    if (dir == badscansDirectoryFullPath)
                    {
                        badscansDirectoryExists = true;
                    }

                    if (goodscansDirectoryExists == true && badscansDirectoryExists == true)
                    {
                        break;
                    }
                }

                if (goodscansDirectoryExists == false)
                {
                    Directory.CreateDirectory(goodscansDirectoryFullPath);
                }
                if (badscansDirectoryExists == false)
                {
                    Directory.CreateDirectory(badscansDirectoryFullPath);
                }

                goodScanDirs = Directory.GetDirectories(goodscansDirectoryFullPath);

                foreach (string dir in goodScanDirs)
                {
                    if (dir == scansDirectoryFullPath)
                    {
                        scansDirectoryExists = true;
                    }

                    if (dir == badscansDirectoryFullPath)
                    {
                        rescansDirectoryExists = true;
                    }

                    if (scansDirectoryExists == true && rescansDirectoryExists == true)
                    {
                        break;
                    }
                }

                if (scansDirectoryExists == false)
                {
                    Directory.CreateDirectory(scansDirectoryFullPath);
                }

                if (rescansDirectoryExists == false)
                {
                    Directory.CreateDirectory(rescansDirectoryFullPath);
                }
                stateMachine = 40;

                //-------------------------
            }

            //STATE 40: Identify the bad scans (through the user)
            if (stateMachine == 40)
            {
                var BadScanSelectorForm = new BadScanSelector();
                BadScanSelectorForm.ShowDialog();
                badScanBurResults = Form1.form1Instance.badScanBurResults;
                stateMachine = 50;
            }

            //STATE 50: Notify operator of lot, datetime, bad scans, directories and get confirmation
            if (stateMachine == 50)
            {
                //Preparing Strings for operator message. 
                string machineIdentifier = Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[2]; // Machine ID (SYS1, SYS2, SYS3,...,SYS<system number>)
                string scanDateTime = Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[4] + " " + Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[5];
                string lotNumber = Regex.Split(dropdownDistinctLotsDateTime.Text, "_")[1];

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

                DialogResult dialogConfirmFilesMovement = MessageBox.Show("Confirm the information below is correct" +
                        Environment.NewLine +
                        Environment.NewLine + "Machine Identifier: " + machineIdentifier +
                        Environment.NewLine + "Date and Time: " + scanDateTime +
                        Environment.NewLine + "Lot Number: " + lotNumber +
                        Environment.NewLine + "Scans or Rescans: " + subFolderName +
                        Environment.NewLine + "-------------------- " +

                        Environment.NewLine + "Good Scans Will be Moved to: <Root Folder>/Good Scans/" + subFolderName +
                        Environment.NewLine + "Bad Scans Will be Moved to: <Root Folder>/Bad Scans/" +
                        Environment.NewLine + "Bad Scans List:" + badScansList,
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
                    string directory_full_path;
                    directory_full_path = rootFolderPath + "\\" + newDirectoryName + "\\";

                    /*
                     DELETE THIS 
                    if (listOfCSVFilesToTransfer != null)
                    {
                        listOfPDFFilesToTransfer.Add(listOfCSVFilesToTransfer[0]);
                    }
                    */

                    foreach (string file in listOfFilesToTransfer)
                    {
                        scansDirectoryFullPath = scansDirectoryFullPath + "\\";
                        rescansDirectoryFullPath = rescansDirectoryFullPath + "\\";
                        badscansDirectoryFullPath = badscansDirectoryFullPath + "\\";

                        if (Path.GetExtension(file) == ".pdf")
                        {
                            string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
                            string burIndexString = Regex.Split(fileNameNoExtension, "Position")[1];
                            Int32.TryParse(burIndexString, out int burIndex);
                            Console.WriteLine("Bur Index:" + burIndex.ToString() + "," + "Good Scan? " + Form1.form1Instance.badScanBurResults[burIndex - 1].ToString());

                            if (Form1.form1Instance.badScanBurResults[burIndex - 1] == false)
                            {
                                Console.WriteLine(subFolderName);
                                if (subFolderName == "Scans")
                                {
                                    File.Copy(file, $"{scansDirectoryFullPath}{Path.GetFileName(file)}", true); // copy file to lot folder location
                                }
                                else if (subFolderName == "Rescans")
                                {
                                    File.Copy(file, $"{rescansDirectoryFullPath}{Path.GetFileName(file)}", true); // copy file to lot folder location
                                }
                            }

                            else
                            {
                                File.Copy(file, $"{badscansDirectoryFullPath}{Path.GetFileName(file)}", true);
                            }
                            Console.WriteLine("------------------END-----------------------");
                        }

                        else if (Path.GetExtension(file) == ".csv")
                        {
                            if (subFolderName == "Scans")
                            {
                                File.Copy(file, $"{scansDirectoryFullPath}{Path.GetFileName(file)}", true); // copy file to lot folder location
                            }
                            else if (subFolderName == "Rescans")
                            {
                                File.Copy(file, $"{rescansDirectoryFullPath}{Path.GetFileName(file)}", true); // copy file to lot folder location
                            }
                        }
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

                string directory_full_path;
                directory_full_path = rootFolderPath + "\\" + newDirectoryName + "\\";

                for (int i = 0; i < 10000; i++)
                {
                    foreach (string file in listOfFilesToTransfer)
                    {
                        Console.WriteLine("files to transfer length = " + listOfFilesToTransfer.Count().ToString());
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
                            int idx = listOfFilesToTransfer.IndexOf(file);
                            File.Delete(file);
                            listOfFilesToTransfer.RemoveAt(idx);
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
                    stateMachine = 80;
                }
            }

            //STATE 80: Bur Bagging
            if (stateMachine == 80)
            {
                Console.WriteLine("-------------BAG FUNCTIONS-------------");

                string burBaggingResults = burBagging.CreateBaggingCsvFiles(dropdownDistinctLotsDateTime.Text, burResultsCsv, badScanBurResults, txtboxScannedText.Text, csvBaggingFolderPath);
                if (!string.IsNullOrEmpty(burBaggingResults))
                {
                    Console.WriteLine("Formated Name: " + burBaggingResults.ToString());
                    Console.WriteLine("-------------END-------------");
                }

                stateMachine = 90;
            }



            //STATE 90: Operation Completed
            if (stateMachine == 90)
            {
                DialogResult dialog = MessageBox.Show("File management operation completed", "Operation Complete");
                stateMachine = 1000;
            }

            //STATE 900: Reset Buttons State
            if (stateMachine == 900)
            {
                btnMoveDeleteFiles.Enabled = true;
                btnOpenPDFReader.Enabled = true;
                btnClearScreen.Enabled = true;
                btnScanTemporaryFolder.Enabled = false;
                dropdownDistinctLotsDateTime.Enabled = true;
                btnReadPDFResults.Enabled = false;
                stateMachine = 1000;
            }

            //STATE 1000: final state
            if (stateMachine == 1000)
            {
            }
            //--------------END--------------
        }
    }
}
