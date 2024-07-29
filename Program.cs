using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon;
using QCBur_dll;
using static User_Interface.Properties.Settings;
namespace User_Interface
{
    static class Program
    {
        public static AWSDynamo dbClient = null;
        public static AWSSNS snsClient = null;
        public static AWSS3 s3Client = null;
        public static AWSSimpleLambda lambdaClient = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Setup();
                SetupService.SetupProgram();
                SetupService.SetupRD();
                SetupService.SetupBinConfig();
                SetupService.SetupRDBinConfig();
                Application.Run(new Form1());

            }
            catch (Exception ex)
            {
                MessageBox.Show("Program error: " + ex.Message);
            }
        }

        static void Setup()
        {
            AWSCognito cognito = new AWSCognito();
            //cognito.AuthenticateUserAsync(Config.AliconaUser, Config.aliconaClientId, Config.appPoolId, Config.idPoolId).Wait();
            cognito.AuthenticateUserAsync(Config.AliconaUser, Config.aliconaUNSDevClientId, Config.unsDevPoolId, Config.unsDevIdPoolId).Wait();

            dbClient = new AWSDynamo(cognito.credentials);
            snsClient = new AWSSNS(cognito.credentials);
            s3Client = new AWSS3(cognito.credentials);
            lambdaClient = new AWSSimpleLambda(cognito.credentials, RegionEndpoint.USEast1);

        }
    }
}
