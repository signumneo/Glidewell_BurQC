using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static User_Interface.SetupService;

namespace User_Interface.Main
{
    public static class GV
    {
        // Program Config
        public const string PRG_CONFIG_FILE = "ProgramConfig.json";

        public static int GRAIN_COV_CRITERIA1;
        public static int GRAIN_COV_CRITERIA2;
        public static int GRAIN_COV_CRITERIA3;
        public static int GRAIN_COV_CRITERIA4;
        public static int GRAIN_COV_CRITERIA5;
        public static int MEAN_GRAIN_PROTUSION_CRITERIA1;

        // Log file
        public static string LogDir = "";
        public static string LogFile = "";


        public static string TEMP_FOLDER_PATH = "";
        public static string CSV_BAGGING_FOLDER_PATH = "";


        public static void SetCriteria()
        {
            try
            {
                GRAIN_COV_CRITERIA1 = mPConfig.GrainCoverage.GC1;
                GRAIN_COV_CRITERIA2 = mPConfig.GrainCoverage.GC2;
                GRAIN_COV_CRITERIA3 = mPConfig.GrainCoverage.GC3;
                GRAIN_COV_CRITERIA4 = mPConfig.GrainCoverage.GC4;
                GRAIN_COV_CRITERIA5 = mPConfig.GrainCoverage.GC5;
                MEAN_GRAIN_PROTUSION_CRITERIA1 = mPConfig.MeanGrainProtustion.MTV1;
                TEMP_FOLDER_PATH = mPConfig.TempFolderName;
                CSV_BAGGING_FOLDER_PATH = mPConfig.CSVBaggingFolderPath;
            }
            catch (Exception e)
            {

            }
        }

    }
}
