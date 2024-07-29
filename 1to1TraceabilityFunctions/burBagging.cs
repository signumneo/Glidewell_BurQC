using System;
using System.Collections.Generic;
using System.IO;

namespace User_Interface._1to1TraceabilityFunctions
{
    /*
    Description: 
        This class will incoorporate functions to be used in the bur bagging process. 
        It generates the barcode text, as well as the CSV file for the bur bagging software. 
    */
    class BurBagging
    {     
        CSVFunctions csvFunctions = new CSVFunctions();

        public string CreateBaggingCsvFiles(string scanIdentifier, double[,] burResults, bool[] badScanBurResults, string burHolderBarCode, string csvFileRootFolderPath)
        {
            /*
            Description: 
                This method is intended to create a CSV with a barcode text for each bur, in a comma separated manner. 
                if a bur was flagged as bad scan or if the reading of mean grain protusion or grain coverage is zero, 
                the method will not add the barcode to the csv file. The format of the bar code is as follows:

                <lot number>_<scanner number>_<yymmddhhMM>_<burindex>_<gc>_<mgp>

                The name of the CSV file is the bur holder fixture bar code and the datetime the scan took place: 

                <bur holder fixture bar code>_<yymmddhhMM>
            
                The CSV file is created on the location of the parameter csvFileRootFolderPath. This is usually the path in the settings. 

            Parameters:
            ----------
            scanIdentifier : string
                The scan identifier is a unique string generated for each scan. It is of the following format:
                Lot_<lot number>_SYS<scanner number>_Scan_<month>-<day>-<year>_<hour>-<month>-<second>
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00_35_40

            burResults : double[,]
                This is a 2x40 matrix where the main grain protusion is the first column, and grain coverage is second column
                if there are bad values, or not read values, zeros are populated in the rows

            badScanBurResults : bool[]
                this is a 1x40 with booleans that indicate if the bur had a bad scan or not. 

            burHolderBarCode : string
                This is the bar code of the bur holder matrix

            csvFileRootFolderPath : string
                This is the folder path where the CSV file will be saved at

            Returns:
            ----------
            barCodeBaseName : string
                This is the base name for the barcode scan, as follows
                <lot number>_<scanner number>_<yymmddhhMM>
                If it fails to extract this data, it will return null
            */
            string barCodeBaseName = null ;
            List<string> scanIdentifierExtractedData = new List<string>(); //individual string (this will be inserted into the list of strings fileNamesExtractedData)
            string[] scanIdentifierParsed = scanIdentifier.Split('_'); //parsed file name without extension   

            //Move Conflicting Files
            moveConflictingCsvFiles(csvFileRootFolderPath, burHolderBarCode);

            //Extract information from scan identifier
            if (scanIdentifierParsed.Length == 6) //if the parsed length is as expected
            {               
                string lotNumber = scanIdentifierParsed[1];
                string systemIdentifier = scanIdentifierParsed[2];
                string scanDate = scanIdentifierParsed[4];
                string scanTime = scanIdentifierParsed[5];

                string formatedDate = FormatDate(scanDate);
                string formatedTime = FormatTime(scanTime);
                string formatedSystemIdentifier = FormatSystemIdentifier(systemIdentifier);
                string csvFileContent = ""; //this is the string that will be written in the CSV file. It will contain the barcode of every bur that had a good scan and GCP && GC NOT ZERO
                string csvFileName = ""; //csv file name
                string csvFileFullPath = "";//csv full path 

                if (!string.IsNullOrEmpty(lotNumber) && !string.IsNullOrEmpty(formatedDate) && !string.IsNullOrEmpty(formatedTime) & !string.IsNullOrEmpty(formatedSystemIdentifier))
                {
                    barCodeBaseName = lotNumber + "_" + formatedSystemIdentifier + "_" + formatedDate + formatedTime;
                    csvFileName = burHolderBarCode + "_" + formatedDate + formatedTime;
                    csvFileFullPath = csvFileRootFolderPath + "\\" + csvFileName + ".csv";

                    //generate the csv content
                    bool firstBarcode = true;
                    for (int i = 0; i < 40; i++)
                    {
                        if (badScanBurResults[i] == false)
                        {
                            int mgp = Convert.ToInt32(Math.Floor(burResults[i, 0])); //mean grain protusion converted to integer and rounded down
                            int gc = Convert.ToInt32(Math.Floor(burResults[i, 1])); //grain coverage converted to integer and rounded down
                            int burScanIndex = i + 1;
                            string burBarCode = barCodeBaseName + "_" + burScanIndex.ToString() + "_" + gc.ToString() + "_" + mgp.ToString();
                            Console.WriteLine(burBarCode);
                            if (mgp != 0 && gc != 0)
                            {
                                if (firstBarcode == true)
                                {
                                    csvFileContent = burBarCode;
                                    firstBarcode = false;
                                }
                                else
                                {
                                    csvFileContent = csvFileContent + "," + burBarCode;
                                }
                            }
                        }                       
                    }
                    Console.WriteLine(csvFileFullPath);
                    Console.WriteLine(csvFileContent);
                    csvFunctions.WriteToCSV(csvFileFullPath, csvFileContent, false);
                }
            }
            return barCodeBaseName;
        }

        private string FormatDate(string date)
        {
            /*
            Description: 
                This function receives the date in <mm>-<dd>-<yyyy> and converts it to mmddyy, which is the requested format 

            Parameters:
            ----------
            date : string
                date in the following format:<mm>-<dd>-<yyyy>
                example: 07-21-2022

            Returns:
            ----------
            formatedDate : string
                date in the following format: mmddyy
                example: 072122
            */
            string[] dateParsed = date.Split('-');
            string formatedDate = "";

            
            if (dateParsed.Length == 3)
            {
                string day = dateParsed[0];
                string month = dateParsed[1];
                string year = dateParsed[2];

                if (year.Length == 4)
                {
                    string yearFormatted = year.Substring(year.Length - 2);
                    formatedDate = yearFormatted + month + day;
                }
                else
                {
                    formatedDate = null;
                }
                
            }
            else
            {
                formatedDate = null;
            }

            return formatedDate;
        }

        private string FormatTime(string time)
        {
            /*
            Description: 
                This function receives the time in <hh>-<mm>-<ss> and converts it to hhmm, which is the requested format 

            Parameters:
            ----------
            time : string
                date in the following format:<hh>-<mm>-<ss>
                example: 03-10-17

            Returns:
            ----------
            formatedtime : string
                timein the following format: mmddyy
                example: 0310
            */
            string[] timeParsed = time.Split('-');
            string formatedTime = "";


            if (timeParsed.Length == 3)
            {
                string hour = timeParsed[0];
                string minute = timeParsed[1];
                formatedTime = hour + minute;
            }
            else
            {
                formatedTime = null;
            }
            return formatedTime;
        }

        private string FormatSystemIdentifier(string systemId)
        {
            /*
            Description: 
                This function receives the system identifier SYS<system number> and converts it to <system number>, which is the requested format 

            Parameters:
            ----------
            systemId : string
                this is the system identifier that performed the scanning process SYS<system number>
                Example would be SYS1

            Returns:
            ----------
            formatedSystemId : string
                It will return the system number. For instance, with a system identifier SYS3, it will return 
                a string "3"
            */
            string formatedSystemId = string.Empty;
            for (int i = 0; i < systemId.Length; i++)
            {
                if (Char.IsDigit(systemId[i]))
                    formatedSystemId += systemId[i];
            }

            bool isformatedSystemIdANumber = int.TryParse(formatedSystemId, out int numericValue);

            if (formatedSystemId.Length > 0 && isformatedSystemIdANumber == true)
            {
                return formatedSystemId;
            }
            else
            {
                return null;
            }
        }

        private void checkArchiveFolder(string csvFileRootFolderPath)
        {
            //DIRECTORIES
            string[] rootDirs = Directory.GetDirectories(csvFileRootFolderPath); //get all directories in csv file root folder
            bool archiveDirectoryExist = false; //variable for storing if the archive folder exists or not
            string archiveDirectoryPath = csvFileRootFolderPath + "\\archive"; //expected archive folder path

            foreach (string directory in rootDirs)
            {

                if (directory == archiveDirectoryPath)
                {
                    archiveDirectoryExist = true;
                    break;
                }
            }

            if (archiveDirectoryExist == false)
            {
                try
                {
                    Directory.CreateDirectory(archiveDirectoryPath);
                }
                catch
                {

                }
            }
        }

        private void moveConflictingCsvFiles(string csvFileRootFolderPath, string fixtureIdentifier)
        {
            string[] csvFilePaths = Directory.GetFiles(csvFileRootFolderPath, "*.csv");
            List<string> conflictingFilePaths = new List<string>();
            string archiveDirectoryPath = csvFileRootFolderPath + "\\archive"; //expected archive folder path

            //Check if archive folder already exists
            checkArchiveFolder(csvFileRootFolderPath);

            //FIND CONFLICTING FILESFILES
            FileManagement fileManagement = new FileManagement();
            foreach (string csvFilePath in csvFilePaths)
            {
                Console.WriteLine(csvFilePath);
                string fixtureIdentifierOnPath = Path.GetFileNameWithoutExtension(csvFilePath).Split('_')[0];
                Console.WriteLine("fixtureIdentifierOnPath: " + fixtureIdentifierOnPath + " | " + "FixtureIdentifier: " + fixtureIdentifier);
                if (fixtureIdentifierOnPath == fixtureIdentifier)
                {
                    Console.WriteLine("conflicting file found");
                    conflictingFilePaths.Add(csvFilePath);
                    Console.WriteLine(csvFilePath);

                }
                Console.WriteLine("------------------");
            }

            //MOVE CONFLICTING FILES 
            foreach (string conflictingFilePath in conflictingFilePaths)
            {
                try
                {
                    string newFilePath = archiveDirectoryPath + "\\" + Path.GetFileName(conflictingFilePath);
                    // Ensure that the target does not exist.
                    if (File.Exists(newFilePath))
                    {
                        File.Delete(newFilePath);
                    }

                    // Move the file.
                    if (File.Exists(conflictingFilePath))
                    {
                        Console.WriteLine("Conflicting File Path: " + conflictingFilePath);
                        Console.WriteLine("New File Path: " + newFilePath);
                        //File.Move(sourceFileName = conflictingFilePath, newFilePath, true);
                        File.Move(conflictingFilePath,newFilePath);
                        Console.WriteLine("------------------");
                    }
                }
                catch
                {

                }
                

            }
        }
    }
}
