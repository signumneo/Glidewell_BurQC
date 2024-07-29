using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.IO.Path;
using System.IO;
using Amazon.SimpleNotificationService.Util;
using Newtonsoft.Json;
using Amazon.Runtime.Internal.Util;
using User_Interface.Main;
using static User_Interface.Main.GV;
using System.Windows.Forms;
using PdfSharp.Pdf.Annotations;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
//using static User_Interface.SetupService.BinConfig;
using QCBur_dll;
using static QCBur_dll.BinSettings;
namespace User_Interface
{
    class SetupService
    {
        private static string PRG_CONFIG_FILE = Path.Combine(Directory.GetCurrentDirectory(), "SoftwareCustomables.json");
        private static string RD_CONFIG_FILE = Path.Combine(Directory.GetCurrentDirectory(), "RDConfig.json");
        private static string BIN_CONFIG_FILE = Path.Combine(Directory.GetCurrentDirectory(), "BinConfig.json");
        private static string RD_BIN_CONFIG_FILE = Path.Combine(Directory.GetCurrentDirectory(), "RDBinConfig.json");

        public static ProgramConfig mPConfig = new ProgramConfig();
        public static RDConfig mRDConfig = new RDConfig();
        public static BinConfig mBinConfig = new BinConfig();
        public static BinConfig mRDBinConfig = new BinConfig();

        public class ProgramConfig
        {
            public string TempFolderName { get; set; }
            public string CSVBaggingFolderPath { get; set; }

            public struct grainCoverage
            {
                public int GC1 { get; set; }
                public int GC2 { get; set; }
                public int GC3 { get; set; }
                public int GC4 { get; set; }
                public int GC5 { get; set; }
            }
            public struct meanGrainProtusion
            {
                public int MTV1 { get; set; }
            }

            public grainCoverage GrainCoverage;
            public meanGrainProtusion MeanGrainProtustion;
        }


        public class RDConfig
        {
            public string TempFolderName { get; set; }
            public string CSVBaggingFolderPath { get; set; }

            public struct grainCoverage
            {
                public int GC1 { get; set; }
                public int GC2 { get; set; }
                public int GC3 { get; set; }
                public int GC4 { get; set; }
                public int GC5 { get; set; }
            }
            public struct meanGrainProtusion
            {
                public int MTV1 { get; set; }
            }

            public grainCoverage GrainCoverage;
            public meanGrainProtusion MeanGrainProtustion;
        }

        //public class BinConfig
        //{
        //    public int version { get; set; }
        //    public string updatedTime { get; set; }

        //    public struct GCBin
        //    {
        //        public string bin { get; set; }
        //        public int lowerGC { get; set; }

        //        public int upperGC { get; set; }

        //    }
        //    public struct MGPBin
        //    {
        //        public string bin { get; set; }
        //        public int MGP { get; set; }
        //    }

        //    public GCBin[] GCBins = new GCBin[10];
        //    public MGPBin MGPCutoffBin = new MGPBin();
        //}

        //public class RDBinConfig
        //{
        //    public int version { get; set; }
        //    public string updatedTime { get; set; }

        //    public struct GCBin
        //    {
        //        public string bin { get; set; }
        //        public int lowerGC { get; set; }
        //        public int upperGC { get; set; }
        //    }
        //    public struct MGPBin
        //    {
        //        public string bin { get; set; }
        //        public int MGP { get; set; }
        //    }
           
        //    public GCBin[] GCBins = new GCBin[10];
        //    public MGPBin MGPCutoffBin = new MGPBin();
        //}

        public static bool SetupProgram()
        {
            if (File.Exists(PRG_CONFIG_FILE))
            { 
                ReadConfigFile(ref mPConfig);
            }
            else
            {
                File.Create(PRG_CONFIG_FILE).Dispose();
                SaveProgramConfigFile(mPConfig);
                return false;
            }
            CaseDirectorySetup();
            return true;
        }

        public static bool SetupRD()
        {
            if (File.Exists(RD_CONFIG_FILE))
            {
                ReadRDConfigFile(ref mRDConfig);
            }
            else
            {
                {
                    File.Create(RD_CONFIG_FILE).Dispose();
                    SaveRDConfigFile(mRDConfig);
                    return false;
                }
            }

            return true;
        }

        public static bool SetupBinConfig()
        {
            if (File.Exists(BIN_CONFIG_FILE))
            {
                ReadBinConfigFile(ref mBinConfig);
            }
            else
            {
                File.Create(BIN_CONFIG_FILE).Dispose();
                SaveBinConfig(mBinConfig);
                MessageBox.Show("Set up bin settings before processing files");
                return false;
            }
            return true;
        }

        public static bool SetupRDBinConfig()
        {
            if (File.Exists(RD_BIN_CONFIG_FILE))
            {
                ReadRDBinConfigFile(ref mRDBinConfig);
            }
            else
            {
                File.Create(RD_BIN_CONFIG_FILE).Dispose();
                SaveRDBinConfig(mRDBinConfig);
                MessageBox.Show("Set up R&D bin settings before processing files");

                return false;
            }
            return true;
        }
        public static void SaveProgramConfigFile(ProgramConfig programConfig)
        {
            string json = "";

            json = JsonConvert.SerializeObject(programConfig, Formatting.Indented);

            using (StreamWriter sw = new StreamWriter(PRG_CONFIG_FILE))
            {
                sw.Write(json);
            }
            ReadConfigFile(ref mPConfig);
        }

        public static void SaveRDConfigFile(RDConfig rdConfig)
        {
            string json = "";
            json = JsonConvert.SerializeObject(rdConfig, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(RD_CONFIG_FILE))
            {
                sw.Write(json);
            }
            ReadRDConfigFile(ref mRDConfig);
        }

        public static void SaveBinConfig(BinConfig binConfig)
        {
            string json = "";
            json = JsonConvert.SerializeObject(binConfig, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(BIN_CONFIG_FILE))
            {
                sw.Write(json);
            }
            ReadBinConfigFile(ref mBinConfig);
        }

        public static void SaveRDBinConfig(BinConfig binConfig)
        {
            string json = "";
            json = JsonConvert.SerializeObject(binConfig, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(RD_BIN_CONFIG_FILE))
            {
                sw.Write(json);
            }
            ReadRDBinConfigFile(ref mRDBinConfig);
        }

        public static bool ReadConfigFile(ref ProgramConfig programConfig)
        {
            using (StreamReader sr = File.OpenText(PRG_CONFIG_FILE))
            {
                string json = sr.ReadToEnd();
                try
                {
                    programConfig = JsonConvert.DeserializeObject<ProgramConfig>(json); 
                }
                catch (Exception ex)
                {

                    return false;
                }
            }
            return true;
        }

        public static bool ReadRDConfigFile(ref RDConfig rdConfig)
        {
            using (StreamReader sr = File.OpenText(RD_CONFIG_FILE))
            {
                string json = sr.ReadToEnd();
                try
                {
                    rdConfig = JsonConvert.DeserializeObject<RDConfig>(json);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ReadBinConfigFile(ref BinConfig binConfig)
        {
            using (StreamReader sr = File.OpenText(BIN_CONFIG_FILE))
            {
                string json = sr.ReadToEnd();

                try
                {
                    binConfig = JsonConvert.DeserializeObject<BinConfig>(json);
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ReadRDBinConfigFile(ref BinConfig binConfig)
        {
            using (StreamReader sr = File.OpenText(RD_BIN_CONFIG_FILE))
            {
                string json = sr.ReadToEnd();

                try
                {
                    binConfig = JsonConvert.DeserializeObject<BinConfig>(json);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }


        private static void CaseDirectorySetup()
        {
            LogDir = Path.Combine(Directory.GetCurrentDirectory(), "LogFiles");

            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);


            // Create blank shade bank file if they are not exist
          
        }
    }
}
