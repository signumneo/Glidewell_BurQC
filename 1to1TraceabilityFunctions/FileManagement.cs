using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using User_Interface.Main;

namespace User_Interface._1to1TraceabilityFunctions
{
    class FileManagement
    {
        /*
            Description: 
                This class is intended to house all functions related to file management, including:
                1) Scanning folder for a file type
                2) Moving Files
                3) Deleting Files
                4) Merge PDFs

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
        public List<List<string>> ExtractDataFromCsvFileNames(string directoryPath)
        {
            /*
            Description: 
                This method retrieves information about csv file names in a given folder. 
                The expected file format for the CSV is: "Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38"
                Changes in the file name convention may affect the method functions.
                This function is generally used to scan a folder for distinct bur scans. 

            Parameters:
            ----------
            directoryPath : string
                This is the full path of the directory the csv files are located. 

            Returns:
            ----------
            fileNamesExtractedData : List<List<string>>
                This is a list of lists with the index of each list being as follows:
                [0]Lot Number
                [1]System Identifier (example: SYS1)
                [2]Scan Date
                [3]Scan Time
                [4]File Name (not including file extension)

                If nothing is found, null will be returned

            Example:
            ----------
                n/a

            Notes:
            ----------
            The expected name parsed length is different for the top and side scans. 
            */

            int expectedParsedLength = 6;
            try
            {
                List<List<string>> fileNamesExtractedData = new List<List<string>>(); //list of list where all data will be saved to
                string[] filesInDirectoryPath = Directory.GetFiles(directoryPath, "*.csv"); //get all files with exntesion csv in given directory
                foreach (string file in filesInDirectoryPath)
                {
                    string fileNameNoExtension = Path.GetFileNameWithoutExtension(file); //file name without extension
                    bool sideScanFile = IsSideScanFile(directoryPath, fileNameNoExtension);
                    if (sideScanFile) continue;
                    List<string> fileNameExtractedData = new List<string>(); //individual string (this will be inserted into the list of strings fileNamesExtractedData)
                    string[] fileNameNoExtensionParsed = fileNameNoExtension.Split('_'); //parsed file name without extension
                    if (fileNameNoExtensionParsed.Length == expectedParsedLength) //if the parsed length is as expected
                    {
                        string lotNumber = fileNameNoExtensionParsed[1];
                        string systemIdentifier = fileNameNoExtensionParsed[2];
                        string scanDate = fileNameNoExtensionParsed[4];
                        string scanTime = fileNameNoExtensionParsed[5];

                        Int32.TryParse(lotNumber, out int lot_number_int);

                        fileNameExtractedData.Add(lotNumber); //[0]
                        fileNameExtractedData.Add(systemIdentifier); //[1]
                        fileNameExtractedData.Add(scanDate); //[2]
                        fileNameExtractedData.Add(scanTime); //[3]
                        fileNameExtractedData.Add(fileNameNoExtension); //[4]
                        fileNamesExtractedData.Add(fileNameExtractedData); //add list fileNameExtractedData to list of lists fileNamesExtractedData
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

        private bool IsSideScanFile(string directoryPath, string csvFileName)
        {
            try
            {
                string[] pdfFilesInDir = Directory.GetFiles(directoryPath, "*.pdf");
                foreach (string file in pdfFilesInDir)
                {
                    if (file.ToUpper().Contains("SEGMENT") && file.Contains(csvFileName))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }


        public string[] ExtractDataFromCsvFileName(string csvFileName)
        {
            /*
            Description: 
                This method retrieves information about csv file names in a given folder. 
                The expected file format for the CSV is: "Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38"
                Changes in the file name convention may affect the method functions.
                This function is generally used to scan a folder for distinct bur scans. 

            Parameters:
            ----------
            directoryPath : string
                This is the full path of the directory the csv files are located. 

            Returns:
            ----------
            fileNamesExtractedData : List<List<string>>
                This is a list of lists with the index of each list being as follows:
                [0]Lot Number
                [1]System Identifier (example: SYS1)
                [2]Scan Date
                [3]Scan Time
                [4]File Name (not including file extension)

                If nothing is found, null will be returned

            Example:
            ----------
                n/a

            Notes:
            ----------
            The expected name parsed length is different for the top and side scans. 
            */

            int expectedParsedLength = 6;
            try
            {
                string[] fileNameExtractedData = new string[5]; //list of list where all data will be saved to
                
                string[] fileNameNoExtensionParsed = csvFileName.Split('_'); //parsed file name without extension
                string lotNumber = fileNameNoExtensionParsed[1];
                string systemIdentifier = fileNameNoExtensionParsed[2];
                string scanDate = fileNameNoExtensionParsed[4];
                string scanTime = fileNameNoExtensionParsed[5];

                Int32.TryParse(lotNumber, out int lot_number_int);

                fileNameExtractedData[0] = lotNumber; //[0]
                fileNameExtractedData[1] = systemIdentifier; //[1]
                fileNameExtractedData[2] = scanDate; //[2]
                fileNameExtractedData[3] = scanTime; //[3]
                fileNameExtractedData[4] = csvFileName; //[4]

                if (fileNameExtractedData.Length > 0)
                {
                    return fileNameExtractedData;
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

        public List<string> FindFilePaths(string directoryPath, string scanIdentifier, bool sideScans, string fileExtension = "all")
        {
            /*
            Description: 
                This method finds all file paths in a given folder, given a scan Identifier (example Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38).
                Only CSV and PDF file that have the expected parsed length will be returned. Example:
                A) PDF top scan expected parsed name length = 7
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1
                B) CSV top scan expected parsed name length = 6
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38

                The expected file format for the files is as shown above. Changes in the file name convention may cause errors. 

            Parameters:
            ----------
            directoryPath : string
                This is the full path of the directory the csv files are located. 

            scanIdentifier : string
                This is a string that identifies a specific scan. It should look like this "Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38"

            fileExtension : string
                Default = "all" --> will search for PDF and CSV filess
                This is the file extension in which you would like to search files. Options are:
                1) "all" --> will search for pdf and csv files
                2) "pdf" --> will search for pdf files only
                3) "csv" --> will serach for csv files only

            Returns:
            ----------
            scanFilePathsInDirectory : List<string>
                This is a list of lists of all file paths in the given folder that meet the scanIdentifier and fileExtension criteria
                If nothing is found, it will return an empty list
                If there is an error, it will return null

            Example:
            ----------
                n/a
            */
            int expectedParsedFileNameLengthPdf = 7;

            if (sideScans) expectedParsedFileNameLengthPdf = 8;
            int expectedParsedFileNameLengthCsv = 6;
            List<string> scanFilePathsInDirectory = new List<string>();
            List<string> sortedScanFilePathsInDirectory = new List<string>();

            try
            {
                //APPEND PDFS TO LIST
                if (fileExtension == "pdf" || fileExtension == "all")
                {
                    string[] filesInDirectoryPathPdfArray = Directory.GetFiles(directoryPath, "*.pdf");
                    List<string> filesInDirectoryPathPdfList = new List<string>();
                    Console.WriteLine("length of list of PDFs:" + filesInDirectoryPathPdfList.Count.ToString());
                    filesInDirectoryPathPdfList = SortFilePathsByBurIndex(filesInDirectoryPathPdfArray);
                    Console.WriteLine("length of list of sorted PDFs:" + filesInDirectoryPathPdfList.Count.ToString());
                    //string[] test;

                    if (filesInDirectoryPathPdfList != null && filesInDirectoryPathPdfList.Any())
                    {
                        foreach (string file in filesInDirectoryPathPdfArray)
                        //foreach (string file in filesInDirectoryPathPdfList)
                        {
                            //if the file has the expected number of parsed items AND if it contains the scan identifier (example (Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38))
                            string fileNameWOExt = Path.GetFileNameWithoutExtension(file);
                            string[] fileNameWOExtSep = fileNameWOExt.Split('_');
                            if (fileNameWOExtSep.Length == expectedParsedFileNameLengthPdf && fileNameWOExtSep[1] == scanIdentifier) //Path.GetFileNameWithoutExtension(file).Contains(scanIdentifier)
                            {
                                scanFilePathsInDirectory.Add(file);

                                Console.WriteLine("pdf file in if statement: " + file + "| Expected File Name WIthout Extension: " + Path.GetFileNameWithoutExtension(file).Split('_').Length.ToString() + "|" + expectedParsedFileNameLengthPdf.ToString() + "|" + Path.GetFileNameWithoutExtension(file).Contains(scanIdentifier).ToString() + "|" + scanIdentifier + "|" + Path.GetFileNameWithoutExtension(file));
                            }
                            else
                            {
                                Console.WriteLine("pdf NOT file in if statement: " + file + "| Expected File Name WIthout Extension: " + Path.GetFileNameWithoutExtension(file).Split('_').Length.ToString() + "|" + expectedParsedFileNameLengthPdf.ToString() + "|" + Path.GetFileNameWithoutExtension(file).Contains(scanIdentifier).ToString() + "|" + scanIdentifier + "|" + Path.GetFileNameWithoutExtension(file));
                            }
                        }
                        sortedScanFilePathsInDirectory = SortFilePathsByBurIndex2(scanFilePathsInDirectory);
                    }
                    else
                    {
                        Console.WriteLine("NOT IN FILES DIRECTORY ");
                    }
                }


                //APPEND CSV TO LIST
                if (fileExtension == "csv" || fileExtension == "all")
                {
                    string[] filesInDirectoryPathCsv = Directory.GetFiles(directoryPath, "*.csv");
                    foreach (string file in filesInDirectoryPathCsv)
                    {
                        string fileNameWOExt = Path.GetFileNameWithoutExtension(file);
                        string[] fileNameWOExtSep = fileNameWOExt.Split('_');
                        if (fileNameWOExtSep.Length == expectedParsedFileNameLengthCsv && fileNameWOExtSep[1] == scanIdentifier)
                        {
                            //scanFilePathsInDirectory.Add(file);
                            sortedScanFilePathsInDirectory.Add(file);
                        }
                    }
                }

                Console.WriteLine("------------------NOT SORTED FILES-------------------");
                foreach (string file in scanFilePathsInDirectory)
                {
                    Console.WriteLine(file);
                }
                Console.WriteLine("------------------END OF NOT SORTED FILES-------------------");


                Console.WriteLine("------------------SORTED FILES-------------------");
                foreach (string file in sortedScanFilePathsInDirectory)
                {
                    Console.WriteLine(file);
                }
                Console.WriteLine("------------------END OF SORTED FILES-------------------");

                return sortedScanFilePathsInDirectory;
            }
            catch
            {
                return null;
            }
        }

        private List<string> SortFilePathsByBurIndex2(List<string> filePaths)
        {
            /*
            Description: 
                This method sorts an array of file paths based on the bur index. Before this method was created, other 
                embedded functions were tried but it did not successfully sort the array. 
                It is important sorting the array so that when the pdf files are merged, it is matching the page number.
                This will avoid confusion. 
                It assumes that the file names are as follows
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1


                The expected file format for the files is as shown above. Changes in the file name convention may cause errors. 

            Parameters:
            ----------
            filePaths : List<string>
                This is the full path of the pdf files, including the file extension .pdf

            Returns:
            ----------
            sortedFilePathsList : List<string>
                This is a list of lists of all file paths order by the bur index number
                If nothing is found, it will return a list with empty values
                If there is an error, it will return null

            Example:
            ----------
                n/a
            */
            BurScan burScan = new BurScan();
            string[] sortedFilePathsArray = new string[40];
            List<string> sortedFilePathsList = new List<string>(); ;
            try
            {
                foreach (string filePath in filePaths)
                {
                    int burPosition = 0;
                    string burPositionString = burScan.StringBetween(Path.GetFileName(filePath), "_Position", ".pdf");
                    if (burPositionString.Contains("Segment")) 
                    {
                        burPositionString = burPositionString.Split('_')[0];
                    }
                    Int32.TryParse(burPositionString, out burPosition); //get bur position as an integer
                    sortedFilePathsList.Add(filePath);
                    //if (burPosition > 0 && burPosition <= 40)
                    //{
                    //    sortedFilePathsArray[(burPosition - 1)] = filePath;
                    //}
                }

                Console.WriteLine("---------------Sorted File Paths--------------");
                //for (int i = 0; i < 40; i++)
                //{
                //    string sortedFilePath = sortedFilePathsArray[i];
                //    if (!String.IsNullOrEmpty(sortedFilePath))
                //    {
                //        Console.WriteLine(sortedFilePath);
                //        sortedFilePathsList.Add(sortedFilePath);
                //    }
                //}
                foreach (string f in sortedFilePathsList)
                {
                    Console.WriteLine(f);

                }
                Console.WriteLine("---------------END--------------");
                return sortedFilePathsList;
            }

            catch
            {
                return null;
            }
        }

        public string MergePDFs(List<string> filesToMerge, string mergedFileName, string mergedFileRootFolder)
        {
            /*
            Description: 
                This will merge pdfs together and create a file of the merged pdfs

            Parameters:
            ----------
            filesToMerge : List<string>
                This is a list of all pdf file's full path, including the file extension .pdf. These files will be merged

            mergedFileName: string
                The name of the merged PDF file

            mergedFileRootFolder : string
                This is the root folder path where the merged file will be created

            Returns:
            ----------
            mergedFilePathOutput : string
                This is a the full path, including .pdf, of the merged file. 
                If there is an error, null will be returned. 

            Example:
            ----------
                n/a
            */
            bool isFileInUse; //to check if the file being merged, or output files are in use
            string mergedFilePathOutput; //file path for the merged file

            try
            {
                PdfDocument outputDocument = new PdfDocument();
                foreach (string file in filesToMerge)
                {
                    if (!string.IsNullOrEmpty(file)) //check that the string is not empty
                    {
                        //Open the document to import pages from it
                        PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                        //Iterate pages 
                        int count = inputDocument.PageCount;
                        for (int idx = 0; idx < count; idx++)
                        {
                            // Get the page from the external document
                            PdfPage page = inputDocument.Pages[idx];

                            //...and add to the output document
                            outputDocument.AddPage(page);
                        }
                    }
                }
                mergedFilePathOutput = mergedFileRootFolder + "\\" + mergedFileName + ".pdf";
                isFileInUse = IsFileLocked(mergedFilePathOutput);
                outputDocument.Save(mergedFilePathOutput);
                return mergedFilePathOutput;
            }
            catch
            {
                return null;
            }
        }

        public bool IsFileLocked(string filePath)
        {
            /*
            Description: 
                This method will check if a file is locked/being used by another system

            Parameters:
            ----------
            filePath : string
                The full file path including extensions of the file to check if it is locked/being used by another system

            Returns:
            ----------
            fileIsLocked : bool
                A boolean that indicates if the file is locked by another system

            Example:
            ----------
                n/a
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

        //------------OBSOLETE FROM HERE ON----------------
        private List<string> SortFilePathsByBurIndex(string[] filePaths)
        {
            /*
            Description: 
                This method sorts an array of file paths based on the bur index. Before this method was created, other 
                embedded functions to sort were tried but it did not successfully sort the array. 
                It is important sorting the array so that when the pdf files are merged, it is matching the page number.
                This will avoid confusion. 
                It assumes that the file names are as follows
                Example: Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38_Position 1


                The expected file format for the files is as shown above. Changes in the file name convention may cause errors. 

            Parameters:
            ----------
            filePaths : string[]
                This is the full path of the pdf files, including the file extension .pdf

            Returns:
            ----------
            sortedFilePathsList : List<string>
                This is a list of lists of all file paths order by the bur index number
                If nothing is found, it will return a list with empty values
                If there is an error, it will return null

            Example:
            ----------
                n/a
            */
            BurScan burScan = new BurScan();
            string[] sortedFilePathsArray = new string[40];
            List<string> sortedFilePathsList = new List<string>(); ;
            try
            {
                foreach (string filePath in filePaths)
                {
                    int burPosition = 0;
                    string burPositionString = burScan.StringBetween(Path.GetFileName(filePath), "_Position", ".pdf");
                    if (burPositionString.Contains("Segment"))
                    {
                        burPositionString = burPositionString.Split('_')[0];
                    }
                    Int32.TryParse(burPositionString, out burPosition); //get bur position as an integer
                    if (burPosition > 0 && burPosition <= 40)
                    {
                        sortedFilePathsArray[(burPosition - 1)] = filePath;
                    }
                }

                Console.WriteLine("---------------Sorted File Paths--------------");
                for (int i = 0; i < 40; i++)
                {
                    string sortedFilePath = sortedFilePathsArray[i];
                    if (!String.IsNullOrEmpty(sortedFilePath))
                    {
                        Console.WriteLine(sortedFilePath);
                        sortedFilePathsList.Add(sortedFilePath);
                    }
                }
                Console.WriteLine("---------------END--------------");
                return sortedFilePathsList;
            }

            catch
            {
                return null;
            }
        }

        public void RenameFiles(string directoryPath, string currentLotNumber, string correctLotNumber, bool sideScans)
        {
            try
            {
                List<string> selectedLotFiles = FindFilePaths(directoryPath, currentLotNumber, sideScans,"all");
                foreach (string file in selectedLotFiles)
                {
                    File.Move(file, file.Replace(currentLotNumber, correctLotNumber));
                }
            }
            catch (Exception ex)
            {
               
            }
        }
    }
}

