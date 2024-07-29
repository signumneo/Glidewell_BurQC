using Amazon.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using User_Interface.Main;
using QCBur_dll.DataStructures;
using static User_Interface.Cloud.mCloud;

namespace User_Interface.Cloud
{
    public class mCloud
    {
        private static string http_url = "https://shzrrjpl7ojuizlnl4ue2tphle0zjlco.lambda-url.us-east-1.on.aws/";
        public enum Operation 
        { 
            updateFixtureTable,
            getFixtureData,
            updateSortedItems,
        }


        public static string GetSortingInformation(string fixtureId)
        {
            string url = http_url;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Method = "POST";
            httpRequest.Accept = "application/";
            httpRequest.ContentType = "application/json";

            string data = JsonConvert.SerializeObject(new
            {
                readType = Operation.getFixtureData.ToString(),
                fixtureId = fixtureId,
            }
            );

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(data);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                string result;
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void UpdateSortedInformation(string fixtureId, string lotFileName, bool sorted)
        {
            string url = http_url;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Method = "POST";
            httpRequest.Accept = "application/";
            httpRequest.ContentType = "application/json";

            string data = JsonConvert.SerializeObject(new
            {
                readType = Operation.updateSortedItems.ToString(),
                fixtureId = fixtureId,
                csvFileName = lotFileName,  
                sorted = sorted.ToString(),
            }
            );

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(data);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                string result;
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static string UpdateFixtureTable(string fixtureId, Inspection.Data msg, string operation, string csvFile, string burNum)
        {
            string url = http_url;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Method = "POST";
            httpRequest.Accept = "application/";
            httpRequest.ContentType = "application/json";

            string data = JsonConvert.SerializeObject(new
            {
                readType = operation,
                fixtureId = fixtureId,
                standardMsg = msg,
                csvFileName = csvFile,
                burName = burNum,
            }
            );

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(data);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                string result;
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
