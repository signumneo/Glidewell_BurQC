using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User_Interface
{
    /*
    Description: 
        This class will incoorporate functions that will allow read and write to a csv file. 
    */
    class CSVFunctions
    {
        //read content from CSV file
        public string ReadFromCSVInCurrentDirectory(string fileName, int csvIndex)
        {
            /*
            Description: 
                This method is intended to read data from a CSV file.It allows the user to call specifically the index 
                of the csv it will read. For instance "0,1,2,3,4,5". With the file name and index=2, the function will return 2.
                Note that this function is reading a file located inside of the executable directory.

            Parameters:
            ----------
            fileName : string
                The name of the csv file including the extension. Note that this file has to be located int he same place as 
                the executable. 

            csvIndex : int
                This is the index in the csv you would like to retrieve the data. For instance, if the csv has content "A,B,C,D". 
                Choosing a csvIndex = 1 will return "B"

            Returns:
            ----------
            desiredContent : string
                This is the content retrieve from the CSV based on the file name and csv index. 
            */

            string fileDirectory = Directory.GetCurrentDirectory(); //get current directory. 
            string filePath = fileDirectory + "\\" + fileName; //append directory to file name to create full path
            string fileContent = System.IO.File.ReadAllText(filePath); //read file content
            string[] fileContentParsed = fileContent.Split(','); //split file content based on "," separator
            string desiredContent = fileContentParsed[csvIndex].Trim(); //retrieved desired indexed content 
            return desiredContent; //return result
        }

        //Write Data to a CSV file
        public void WriteToCSV(string filePath, string contentToWrite, bool enableNextLineWriting = true)
        {
            /*
            Description: 
                This method is intended to write data to a csv file.

            Parameters:
            ----------
            filePath : string
                The full file path of the csv, including the extension (if the file does not exist, it will create it) 

            contentToWrite : string
                This is the content that will be written to the csv

            enableNextLineWriting : bool
                If this is set to true, every time this function is called it will write to the next line of the csv. Otherwise,
                it will overwrite the first line. 

            Returns:
            ----------
            n/a
            */

            if (enableNextLineWriting == true)
            {
                //if file does not exist, create file
                if (!File.Exists(filePath))
                {
                    using (System.IO.StreamWriter file = File.CreateText(filePath))
                    {
                        file.WriteLine(contentToWrite);
                        
                    }
                }

                //if file exists, write on file
                else
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                    {
                        file.WriteLine(contentToWrite);
                    }
                }
            }

            //writes on the same line
            else
            {

                //if file does not exist, create file
                if (!File.Exists(filePath))
                {
                    using (System.IO.StreamWriter file = File.CreateText(filePath))
                    {
                        file.Write(contentToWrite);
                    }
                }


                //if file exists, write on file
                else
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                    {
                        file.Write(contentToWrite);
                    }
                }
            }
        }
    }
}
