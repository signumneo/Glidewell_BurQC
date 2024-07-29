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
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text.RegularExpressions;
using Amazon.Runtime;
using User_Interface.Main;

namespace User_Interface

{
    class FilesFunctions
    {
        //-----------------------------METHODS USED----------------------------------------
        //NOTE: The methods "IsFileLocked" and "FindFilePaths" are used in other classes. The other methods in this class are all obsolete

        public bool IsFileLocked(string filePath)
        {
            /*
            Description: 
                This method checks if a file is locked (being used by another system)

            Parameters:
            ----------
            filePath : string
                This is the full file path, including the file extension (example s.csv')

            Returns:
            ----------
            fileIsLocked : string
                This indicates if the file is locked (being used by another application) or not
            */
            bool fileIsLocked = false;
            try
            {
                FileStream fs =
                    File.Open(filePath, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.None);
                fs.Close();
            }
            catch (IOException ex)
            {
                fileIsLocked = true;
                Console.WriteLine(ex);
            }
            return fileIsLocked;
        }

        public List<string> FindFilePaths(string directoryPath, string distinctLotDateTime, string fileExtension, string scanType = "top")
        {
            /*
            Description: 
                This method checks finds all file paths in a given folder. It will find files with a fiven lot # date and time. Also, it allows 
                the user to sort for what type of files it is looking for (.csv, .pdf or all). Finally, it checks if the file format the method is 
                searching for is based on the top scan file name format or the side scan file name format. 

            Parameters:
            ----------
            directoryPath: string
                This is the full path of the directory the method will search for the files

            distinctLotDateTime : string
                This is the distinct lot date and time the method will search the files. This has to be in the following format:
                "Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38". This can uniquely identify a scan.

            fileExtension : string
                This is the file extension the method will search for. Options are:
                    - '.csv'
                    - '.pdf'
                    - 'all'
            scanType : string
                This is the scan type the method will search for.Options are 'top' or 'side'. 'top' is the default option if scanType is not explicitly called. 
                The expected name parsed length is different for the top and side scans. 
               1) Top Scan
                   A) PDF side scan expected parsed name length = 7
                   Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1
                   B) CSV side scan expected parsed name length = 6
                   Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38
               2) Side Scan
                   A) PDF side scan expected parsed name length = 8
                   Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position1_Segment1
                   B) CSV side scan expected parsed name length = 6
                   Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38
            Returns:
            ----------
            filePathsInDirectory : List<string>
                a list of all all file paths in the directory given the desired lot date time, file extension and scan type. 
            */


            int expectedParsedLength = 7;
            if (scanType == "top")
            {
                expectedParsedLength = 7;
            }
            else if (scanType == "side")
            {
                expectedParsedLength = 8;
            }


            string[] filesInDirectoryPath = Directory.GetFiles(directoryPath, ("*." + fileExtension));
            int expectedParsedFileNameLength;
            if (fileExtension == "pdf")
            {
                expectedParsedFileNameLength = expectedParsedLength;
            }
            else if (fileExtension == "csv")
            {
                if (scanType == "top")
                {
                    expectedParsedFileNameLength = expectedParsedLength - 1;
                }
                else if (scanType == "side")
                {
                    expectedParsedFileNameLength = expectedParsedLength - 2;
                }
                else
                {
                    expectedParsedFileNameLength = 1;
                }
            }
            else
            {
                expectedParsedFileNameLength = 0;
            }

            //List<List<string>> list = new List<List<string>>();
            List<string> filePathsInDirectory = new List<string>();
            if (expectedParsedFileNameLength != 0)
            {
                try
                {
                    foreach (string file in filesInDirectoryPath)
                    {
                        //List<string> sublist = new List<string>();
                        string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
                        string[] fileNameNoExtensionParsed = fileNameNoExtension.Split('_');

                        if (fileNameNoExtensionParsed.Length == expectedParsedFileNameLength)
                        {
                            string lotNumber = fileNameNoExtensionParsed[1];
                            string systemIdentifier = fileNameNoExtensionParsed[2];
                            string scanDate = fileNameNoExtensionParsed[4];
                            string scanTime = fileNameNoExtensionParsed[5];
                            string scanDateTime = scanDate + "_" + scanTime;

                            string fileNameLotNumberDateTime = lotNumber + "_" + scanDateTime;

                            if (fileNameLotNumberDateTime == distinctLotDateTime)
                            {
                                string file_path = file;
                                filePathsInDirectory.Add(file_path);
                            }
                        }

                    }
                    if (filePathsInDirectory.Count > 0)
                    {
                        Console.WriteLine("list not 0");
                        filePathsInDirectory = filePathsInDirectory.OrderBy(x => filePathsInDirectory).ToList();
                        return filePathsInDirectory;

                    }
                    else
                    {
                        Console.WriteLine("list null");
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }



        //-----------------------------OBSOLETE FROM HERE ON----------------------------------------
        public List<List<string>> ExtractInfoFromTempFolderCsvs(string directoryPath, string scanType = "top")
        {
            /*
            The expected name parsed length is different for the top and side scans. 
            1) Top Scan
                A) PDF side scan expected parsed name length = 7
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1
                B) CSV side scan expected parsed name length = 6
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38
            2) Side Scan
                A) PDF side scan expected parsed name length = 8
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position1_Segment1_1-8
                B) CSV side scan expected parsed name length = 7
                Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Segment1
            */

            int expectedParsedLength = 6;
            if (scanType == "top")
            {
                expectedParsedLength = 6;
            }
            else if (scanType == "side")
            {
                expectedParsedLength = 7;
            }
            try
            {
                string[] filesInDirectoryPath = Directory.GetFiles(directoryPath, "*.csv");

                List<List<string>> fileNamesExtractedData = new List<List<string>>();

                foreach (string file in filesInDirectoryPath)
                {
                    List<string> fileNameExtractedData = new List<string>();
                    string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
                    string[] fileNameNoExtensionParsed = fileNameNoExtension.Split('_');
                    if (fileNameNoExtensionParsed.Length == expectedParsedLength)
                    {
                        string lotNumber = fileNameNoExtensionParsed[1];
                        string systemIdentifier = fileNameNoExtensionParsed[2];
                        string scanDate = fileNameNoExtensionParsed[4];
                        string scanTime = fileNameNoExtensionParsed[5];

                        Int32.TryParse(lotNumber, out int lot_number_int);

                        fileNameExtractedData.Add(lotNumber);
                        fileNameExtractedData.Add(systemIdentifier);
                        fileNameExtractedData.Add(scanDate);
                        fileNameExtractedData.Add(scanTime);
                        fileNameExtractedData.Add(fileNameNoExtension);
                        fileNamesExtractedData.Add(fileNameExtractedData);
                    }
                }
                if (fileNamesExtractedData.Count > 0)
                {
                    return fileNamesExtractedData;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public List<List<string>> ExtractInfoFromTempFolder(string directoryPath, string scanType = "top")
        {
            /*
            The expected name parsed length is different for the top and side scans. 
            1) Top Scan
                A) PDF side scan expected parsed name length = 7
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1
                B) CSV side scan expected parsed name length = 6
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38
            2) Side Scan
                A) PDF side scan expected parsed name length = 8
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position1_Segment1_1-8
                B) CSV side scan expected parsed name length = 7
                Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Segment1
            */

            int expectedParsedLength = 7;
            if (scanType == "top")
            {
                expectedParsedLength = 7;
            }
            else if (scanType == "side")
            {
                expectedParsedLength = 8;
            }


            string[] filesInDirectoryPath = Directory.GetFiles(directoryPath, "*.pdf");

            List<List<string>> fileNamesExtractedData = new List<List<string>>();

            foreach (string file in filesInDirectoryPath)
            {
                List<string> fileNameExtractedData = new List<string>();
                string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
                string[] fileNameNoExtensionParsed = fileNameNoExtension.Split('_');
                Console.WriteLine(fileNameNoExtensionParsed.Length);
                if (fileNameNoExtensionParsed.Length == expectedParsedLength)
                {
                    string lotNumber = fileNameNoExtensionParsed[1];
                    string systemIdentifier = fileNameNoExtensionParsed[2];
                    string scanDate = fileNameNoExtensionParsed[4];
                    string scanTime = fileNameNoExtensionParsed[5];
                    string scanDateTime = scanDate + "_" + scanTime;


                    string burIndex = "";
                    if (scanType == "top")
                    {
                        burIndex = fileNameNoExtensionParsed[fileNameNoExtensionParsed.Length - 1];
                    }
                    else if (scanType == "side")
                    {
                        burIndex = fileNameNoExtensionParsed[fileNameNoExtensionParsed.Length - 2];
                    }

                    //this is going to spill Position1 for instance

                    string[] scan_position_parsed = burIndex.Split('n'); //user the letter n

                    if (scan_position_parsed.Length == 2)
                    {
                        burIndex = scan_position_parsed[1];
                    }
                    else
                    {
                        burIndex = "";
                    }

                    Int32.TryParse(lotNumber, out int lot_number_int);
                    Int32.TryParse(burIndex, out int scan_position_int);

                    fileNameExtractedData.Add(lotNumber);
                    fileNameExtractedData.Add(scanDateTime);
                    fileNameExtractedData.Add(burIndex);
                    fileNameExtractedData.Add(systemIdentifier);
                    fileNamesExtractedData.Add(fileNameExtractedData);
                }
            }
            if (fileNamesExtractedData.Count > 0)
            {
                return fileNamesExtractedData;
            }
            else
            {
                return null;
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
        public List<string> ExtractInfoFromScanFile(string filePath, string scanType = "top")
        {
            /*
           The expected name parsed length is different for the top and side scans. 
           1) Top Scan
               A) PDF side scan expected parsed name length = 7
               Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1
               B) CSV side scan expected parsed name length = 6
               Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38
           2) Side Scan
               A) PDF side scan expected parsed name length = 8
               Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position1_Segment1
               B) CSV side scan expected parsed name length = 7
               Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Segment1
           */

            int expectedParsedLength = 7;
            if (scanType == "top")
            {
                expectedParsedLength = 7;
            }
            else if (scanType == "side")
            {
                expectedParsedLength = 8;
            }

            List<string> filePathExtractedData = new List<string>();
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(filePath);
            string[] fileNameNoExtensionParsed = fileNameNoExtension.Split('_');
            if (fileNameNoExtensionParsed.Length == expectedParsedLength)
            {
                string lotNumber = fileNameNoExtensionParsed[1];
                string systemIdentifier = fileNameNoExtensionParsed[2];
                string scanDate = fileNameNoExtensionParsed[4];
                string scanTime = fileNameNoExtensionParsed[5];
                string scanDateTime = scanDate + "_" + scanTime;


                //----------------------

                string burPositionString = Between(fileNameNoExtension, "Position", "_Segment");
                string[] segmentNumberStringArray = Regex.Split(fileNameNoExtension, "_Segment");
                string segmentNumberString = segmentNumberStringArray[1];
                //Int32.TryParse(segmentNumberString, out int segmentNumber);
                //Int32.TryParse(burPositionString, out int burPosition);
                //----------------------



                string burIndex = "";
                if (scanType == "top")
                {
                    burIndex = fileNameNoExtensionParsed[fileNameNoExtensionParsed.Length - 1];
                }
                else if (scanType == "side")
                {
                    burIndex = fileNameNoExtensionParsed[fileNameNoExtensionParsed.Length - 2];
                }

                //this is going to spill Position1 for instance

                string[] scan_position_parsed = burIndex.Split('n'); //user the letter n

                if (scan_position_parsed.Length == 2)
                {
                    burIndex = scan_position_parsed[1];
                }
                else
                {
                    burIndex = "";
                }

                Int32.TryParse(lotNumber, out int lot_number_int);
                Int32.TryParse(burIndex, out int scan_position_int);

                filePathExtractedData.Add(lotNumber); //[0]
                filePathExtractedData.Add(scanDateTime);//[1]
                filePathExtractedData.Add(burIndex);//[2]
                filePathExtractedData.Add(systemIdentifier);//[3]
                                                            //---------------------
                filePathExtractedData.Add(scanDate);//[4]
                filePathExtractedData.Add(scanTime);//[5]
                filePathExtractedData.Add(burPositionString);//[6]
                filePathExtractedData.Add(segmentNumberString);//[7]


            }
            if (filePathExtractedData.Count > 0)
            {
                return filePathExtractedData;
            }
            else
            {
                return null;
            }
        }


        public List<string> FindFilePathsWithDistinctLots(string directoryPath, string distinctLot, string fileExtension, string scanType = "top")
        {
            //string[] files = Directory.GetFiles(@folder_path, "*.pdf");
            /*
           The expected name parsed length is different for the top and side scans. 
           1) Top Scan
               A) PDF side scan expected parsed name length = 7
               Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1
               B) CSV side scan expected parsed name length = 6
               Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38
           2) Side Scan
               A) PDF side scan expected parsed name length = 8
               Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position1_Segment1
               B) CSV side scan expected parsed name length = 6
               Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38
           */

            int expectedParsedLength = 7;
            if (scanType == "top")
            {
                expectedParsedLength = 7;
            }
            else if (scanType == "side")
            {
                expectedParsedLength = 8;
            }


            string[] filesInDirectoryPath = Directory.GetFiles(directoryPath, ("*." + fileExtension));
            int expectedParsedFileNameLength;
            if (fileExtension == "pdf")
            {
                expectedParsedFileNameLength = expectedParsedLength;
            }
            else if (fileExtension == "csv")
            {
                if (scanType == "top")
                {
                    expectedParsedFileNameLength = expectedParsedLength - 1;
                }
                else if (scanType == "side")
                {
                    expectedParsedFileNameLength = expectedParsedLength - 2;
                }
                else
                {
                    expectedParsedFileNameLength = 1;
                }

            }
            else
            {
                expectedParsedFileNameLength = 0;
            }

            //List<List<string>> list = new List<List<string>>();
            List<string> filePathsInDirectory = new List<string>();
            if (expectedParsedFileNameLength != 0)
            {
                try
                {
                    foreach (string file in filesInDirectoryPath)
                    {
                        //List<string> sublist = new List<string>();
                        string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
                        string[] fileNameNoExtensionParsed = fileNameNoExtension.Split('_');

                        if (fileNameNoExtensionParsed.Length == expectedParsedFileNameLength)
                        {
                            string lotNumber = fileNameNoExtensionParsed[1];
                            string scanDate = fileNameNoExtensionParsed[4];
                            string scanTime = fileNameNoExtensionParsed[5];
                            string scanDateTime = scanDate + "_" + scanTime;

                            if (lotNumber == distinctLot)
                            {
                                string file_path = file;
                                filePathsInDirectory.Add(file_path);
                            }
                        }
                    }
                    if (filePathsInDirectory.Count > 0)
                    {
                        Console.WriteLine("list not 0");
                        filePathsInDirectory = filePathsInDirectory.OrderBy(x => filePathsInDirectory).ToList();
                        return filePathsInDirectory;

                    }
                    else
                    {
                        Console.WriteLine("list null");
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public double[,] retrieveBurResultsFromCSV(string filePath)
        {
            double[,] burResults = new double[40, 2];

            int meanGrainProtusionIndex = 0;
            int grainCoverageIndex = 1;
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                string burPositionString = "";

                int burPosition = 0;
                double resultValue = 0;
                int index = -1;

                foreach (string line in lines)
                {
                    burPosition = 0;
                    resultValue = 0;
                    index = -1;
                    //Console.WriteLine(line);
                    if (line.Contains("Mean GP;"))
                    {
                        index = meanGrainProtusionIndex;
                    }
                    else if (line.Contains("Cov;"))
                    {
                        index = grainCoverageIndex;
                    }

                    if (index == meanGrainProtusionIndex || index == grainCoverageIndex)
                    {
                        burPositionString = retrieveBurPositionFromCSVString(line);
                        Int32.TryParse(burPositionString, out burPosition);
                        resultValue = Convert.ToDouble(retrieveBurResultsValueFromCSVString(line)); //gets mean grain protusion or grain coverage depending on index

                        if ((burPosition - 1) >= 0 && (burPosition - 1) < 40 && resultValue > 0)
                        {
                            burResults[(burPosition - 1), index] = Math.Round(resultValue, 2);
                        }
                    }
                }
            }
            catch
            {

            }

            return burResults;
        }
        private string retrieveBurPositionFromCSVString(string evaluateString)
        {
            string burPosition = Between(evaluateString, "Position ", ";");
            return burPosition;
        }
        private string retrieveBurResultsValueFromCSVString(string evaluateString)
        {
            string[] columns = evaluateString.Split(';');
            string burMeanGrainProtusion = "";
            if (columns.Length >= 3)
            {
                burMeanGrainProtusion = columns[2];
            }
            return burMeanGrainProtusion;
        }

        public string GetProgramStartTime(string scanIdentifier)
        {
            string[] dtInfo = scanIdentifier.Split('_'); //lotNumber_date_time

            DateTime dtStartTime = DateTime.ParseExact(dtInfo[1] + dtInfo[2], "MM-dd-yyyyHH-mm-ss",
                          System.Globalization.CultureInfo.InvariantCulture);
            string startTime = dtStartTime.ToString("s") + dtStartTime.ToString("zzz");
            return startTime;
        }

        public string GetProgramEndTime(string directoryPath, string scanIdentifier)
        {

            var sortedFiles = Directory.GetFiles(directoryPath, $"{scanIdentifier}_*").OrderBy(f => f);
            //   .Select(f => new FileInfo(f));
            //.OrderBy(f=>(f.Name.Split(' ')[1]).Remove((f.Name.Split(' ')[1]).IndexOf('.')));


            string lastReportName = "";
            int lastReportNum = -1;
            foreach (var file in sortedFiles)
            {
                string positionFileName = file.Split('_')[6]; // Position N.pdf
                string position = positionFileName.Replace("Position", String.Empty);
                lastReportNum = Math.Max(Int32.Parse(position.Remove(position.IndexOf('.'))), lastReportNum);
                
            }

            string endTime = "";
            int filesCount = sortedFiles.Count();
            if (filesCount > 0)
            {
                lastReportName = sortedFiles.Where(n => n.Contains(lastReportNum + ".pdf")).First();
               // string lastReportName = sortedFiles.ElementAt(lastReportNum);
                DateTime stopTime = Directory.GetLastWriteTime(lastReportName).AddMinutes(1.0);
                endTime = stopTime.ToString("s") + stopTime.ToString("zzz");
                return endTime;
            }
            else
            {
                return endTime;
            }
        }

       
    }
}

