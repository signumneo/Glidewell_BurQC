using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static User_Interface.CSVFunctions;
using User_Interface._1to1TraceabilityFunctions;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using static User_Interface.SetupService;
using Amazon.DynamoDBv2;

namespace User_Interface.Main
{
    public class GF
    {
        FileManagement fileManagement = new FileManagement();
        CSVFunctions csvFunctions = new CSVFunctions();
        string rootFolderPath; // source folder path, saved on text file 
        public string tempFolderPath; // root temporary folder
        List<List<string>> extractedBurData = new List<List<string>>(); // data saved from all PDFs (grain coveraged, mean grain protusion, pass/fails)

        int grainCoverageCriteria1;// grain coverage criteria 1 (GC1) set on settings tab
        int grainCoverageCriteria2;// grain coverage criteria 1 (GC2) set on settings tab
        int grainCoverageCriteria3;// grain coverage criteria 1 (GC3) set on settings tab
        int grainCoverageCriteria4;// grain coverage criteria 1 (GC4) set on settings tab
        int grainCoverageCriteria5;// grain coverage criteria 1 (GC5) set on settings tab

        int meanGrainProtusionCriteria1; // grain coverage criteria 1 (MGP1) set on settings tab
        string[] roofFolderDirectories; // directories in root folder path
        string LogDir = Path.Combine(Directory.GetCurrentDirectory(), "LogFolder");
        string LogFile = "";
        int NUM_BINS = 10;
        int MGP_BIN_IDX = 11;

        public static int MAX_FAILURES = 3;
        public GF()
        {
            RetrievePassFailCriteria();
            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }
        }
        public void RetrievePassFailCriteria()
        {
            try
            {
                //Retrive criteria values
                string grainCoverageCriteria1String = mPConfig.GrainCoverage.GC1.ToString();
                string grainCoverageCriteria2String = mPConfig.GrainCoverage.GC2.ToString();
                string grainCoverageCriteria3String = mPConfig.GrainCoverage.GC3.ToString();
                string grainCoverageCriteria4String = mPConfig.GrainCoverage.GC4.ToString();
                string grainCoverageCriteria5String = mPConfig.GrainCoverage.GC5.ToString();
                string meanGrainProtusionCriteria1String = mPConfig.MeanGrainProtustion.MTV1.ToString();

                //Parse criteria values to integers 
                Int32.TryParse(grainCoverageCriteria1String, out grainCoverageCriteria1);
                Int32.TryParse(grainCoverageCriteria2String, out grainCoverageCriteria2);
                Int32.TryParse(grainCoverageCriteria3String, out grainCoverageCriteria3);
                Int32.TryParse(grainCoverageCriteria4String, out grainCoverageCriteria4);
                Int32.TryParse(grainCoverageCriteria5String, out grainCoverageCriteria5);
                Int32.TryParse(meanGrainProtusionCriteria1String, out meanGrainProtusionCriteria1);

                //Create folde paths based on txt file constumables
                rootFolderPath = mPConfig.CSVBaggingFolderPath;
                tempFolderPath = mPConfig.TempFolderName;
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
        }


        public void SetProgramValues()
        {

        }

        public Dictionary<string, string> ScanFolder(string tempFolderPath, bool cleared)
        {
            List<string> distinctLotDateTimes = new List<string>(); // list with files that have distinct datetimes
            List<List<string>> csvFileNames = new List<List<string>>();
            Dictionary<string, string> fileNameMap = new Dictionary<string, string>();
            try
            {
                csvFileNames = fileManagement.ExtractDataFromCsvFileNames(tempFolderPath);

                if (csvFileNames != null && csvFileNames.Any())
                {
                    foreach (List<string> csvFile in csvFileNames)
                    {
                        string fileNameLotNumberDateTime = $"{csvFile[0]}_{csvFile[2]}_{csvFile[3]}";
                        //string csvFileName = csvFile[4];
                        distinctLotDateTimes.Add(fileNameLotNumberDateTime);
                        fileNameMap.Add(fileNameLotNumberDateTime, csvFile[4]);
                    }

                }
                else if (csvFileNames == null && !cleared)
                {
                    MessageBox.Show("No files found!", "Attention");
                }
                return fileNameMap;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to find files. Please make sure that the root file location has a folder named tempFolder");
                return null;
            }
        }

        public List<string> GetLotNumbers(bool sideScans)
        {
            int expectedParseLength = 7;
            if (sideScans) expectedParseLength = 8;

            string[] dirFiles = Directory.GetFiles(tempFolderPath, "*.pdf");
            List<string> lotNumbers = new List<string>();
            foreach (string file in dirFiles)
            {
                string fileNameNoExt = Path.GetFileNameWithoutExtension(file);
                string[] parsedFileName = fileNameNoExt.Split('_');
                if (parsedFileName.Length == expectedParseLength)
                {
                    lotNumbers.Add(parsedFileName[1]);
                }
            }

            return lotNumbers;

        }


        public Dictionary<string, List<string>> FindLotFilePaths(string distinctLot, bool isPDF, bool isSideScan)
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
            int expectedParseLength = 7;
            if (isSideScan) expectedParseLength = 8;

            string fileType = "*.pdf";
            if (!isPDF) fileType = "*.csv";
            string[] dirFiles = Directory.GetFiles(tempFolderPath, fileType);
            int expectedParsedFileNameLength = 8;
            if (!isPDF)
            {
                if (isSideScan) expectedParsedFileNameLength = expectedParseLength - 2;
                else expectedParsedFileNameLength = expectedParseLength - 1;
            }
            Dictionary<string, List<string>> extractedFileInfo = new Dictionary<string, List<string>>();
            try
            {
                if (isPDF)
                {
                    foreach (string file in dirFiles)
                    {
                        string fileNameNoExt = Path.GetFileNameWithoutExtension(file);
                        string[] fileNameNoExtParsed = fileNameNoExt.Split('_');
                        List<string> filePathsInDir = new List<string>();

                        if (fileNameNoExtParsed.Length == expectedParseLength)
                        {
                            filePathsInDir.Add(fileNameNoExtParsed[1]);   // lot number
                            filePathsInDir.Add(fileNameNoExtParsed[2]);  // system identifier
                            filePathsInDir.Add(fileNameNoExtParsed[4]);     // scan date
                            filePathsInDir.Add(fileNameNoExtParsed[5]);     // scan time     
                            string burPos = Between(fileNameNoExt, "Position", "_Segment");
                            string segmentNumber = fileNameNoExtParsed[expectedParseLength - 1].Substring(7);
                            filePathsInDir.Add(burPos);
                            filePathsInDir.Add(segmentNumber);

                            extractedFileInfo.Add(file, filePathsInDir);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return extractedFileInfo;
        }

        private string Between(string str, string FirstString, string LastString)
        {
            try
            {
                string FinalString;
                int Pos1 = str.IndexOf(FirstString) + FirstString.Length;
                int Pos2 = str.IndexOf(LastString);
                FinalString = str.Substring(Pos1, Pos2 - Pos1);
                return FinalString;
            }
            catch
            {
                return "";
            }

        }
        public void LogThis(string logContent)
        {
            // Log Files
            LogFile = LogDir + "\\log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            if (!File.Exists(LogFile))
            {
                File.Create(LogFile).Dispose();
            }
            using (StreamWriter sw = File.AppendText(LogFile))
            {
                sw.WriteLine(string.Format("[{0}] \t{1}", DateTime.Now.ToString("HH:mm:ss"), logContent));
            }
        }

        public static string GetThreeDigitsVersionNumber()
        {
            string fullVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            String[] versionArray = fullVersion.Split('.');
            var newVersion = string.Join(".", versionArray.Take(3));

            return newVersion;
        }


        public string DetermineBin(double grainCoverage, double meanGrainProtrusion)
        {
            if (meanGrainProtrusion > mBinConfig.MGPCutoffBin.MGP)
            {
                return mBinConfig.MGPCutoffBin.bin;
            }
            for (int i = 0; i < NUM_BINS; i++)
            {
                if ((mBinConfig.GCBins[i].lowerGC <= grainCoverage) && (grainCoverage < mBinConfig.GCBins[i].upperGC))
                {
                    return mBinConfig.GCBins[i].bin;

                }
            }
            return "";
        }

        public string DetermineRDBin(double grainCoverage, double meanGrainProtrusion)
        {
            if (meanGrainProtrusion > mRDBinConfig.MGPCutoffBin.MGP)
            {
                return mRDBinConfig.MGPCutoffBin.bin;
            }
            for (int i = 0; i < NUM_BINS; i++)
            {

                if ((mRDBinConfig.GCBins[i].lowerGC <= grainCoverage) && (grainCoverage < mRDBinConfig.GCBins[i].upperGC))
                {
                    return mRDBinConfig.GCBins[i].bin;

                }
            }
            return "";
        }

        public static bool IsOverlapping(int[] lowerGCs, int[] upperGCs)
        {
            List<int[]> ranges = new List<int[]>();
            for (int i = 0; i < lowerGCs.Length; i++)
            {
                if (lowerGCs[i] != 0 && upperGCs[i] != 0)
                {
                    ranges.Add(new int[2] { lowerGCs[i], upperGCs[i] });
                }
            }
            Comparer<int[]> comparer = Comparer<int[]>.Create((a, b) => a[0].CompareTo(b[0]));
            ranges.Sort(comparer);

            foreach (int[] array in ranges)
            {
                Console.WriteLine(string.Join(", ", array));
            }

            for (int i = 0; i < ranges.Count; i++)
            {
                for (int j = i+1; j < ranges.Count; j++)
                {
                    
                    int overlap = Math.Max(ranges[i][0], ranges[j][0]) - Math.Min(ranges[i][1], ranges[j][1]);
                    Console.WriteLine($"Max([{ranges[i][0]}, {ranges[i][1]}]) - Min([{ranges[j][0]}, {ranges[j][1]}]) = {overlap}");
                    if (overlap < 0)
                    {
                        return true;
                    }
                }
            }
         
            return false;
        }
        public string GetLotNumber(string fileName)
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
    }
}
