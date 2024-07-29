using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User_Interface.Main;

namespace User_Interface._1to1TraceabilityFunctions
{
    class BurScan
    {
        /*Class Description
        Description: 
            This class is intended to house all functions related to retrieving data from bur scan files

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
        public double[,] RetrieveBurResultsFromCSV(string filePath)
        {
            /*
            Description: 
                This method is intended to retrieve mean grain protusion (MGP) and grain coverage (GC) from 
                the scan CSV file. Note that the method assumes there are no more than 40 burs per scan.
                Also, if the format of the CSV file changes the method may not work. 
                The CSV file this method is designed for contains 1 column. The rows in which there are relevant 
                data (MGP or GC) contain the following format:
                1) MGP: "Position 1;Cov;32.308;%;OK;0.000;0.000;0.000"
                2) GC: "Position 1;Mean GP;16.20;µm;OK;0.00;0.00;0.00"

            Parameters:
            ----------
            filePath : string
                This is the file path of the CSV in which the method will retrieve data from. 
                This parameter should be the full file path and include the ".csv" extension on it

            Returns:
            ----------
            burResults : double[,]
                This is a 40 x 2 matrix where the row represents the bur, 1st col represents MGP, 2nd col represents GP.
                If there is no bur in a given position, values will be 0.

            Example:
            ----------
                string fileName = "Lot_4222033001_SYS1_Scan_07-21-2022_00-35-38.csv";
                string fullFilePath = tempFolderPath + "\\" + fileName;
                double[,] burResults = new double[40, 2];
                burResults = filesFunctions.retrieveBurResultsFromCSV(fullFilePath);
                for(int i = 0; i< burResults.GetLength(0); i++)
                {
                    Console.WriteLine("Position: " + (i + 1).ToString() + " |MGP: " + burResults[i, 0] + " |GC: " + burResults[i, 1]);
                }
            */
            double[,] burResults = new double[40, 2];
            int meanGrainProtusionIndex = 0; //column index of mean grain protusion for burResults Matrix
            int grainCoverageIndex = 1; //column index of grain coverage for burResults Matrix
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filePath); //gets a list of each line of CSV file
                string burPositionString = ""; //place holder for bur position in string format

                int burPosition = 0; //place holder for bur position in integer format
                double resultValue = 0; //place holder for bur result value (MGP or GC) in double format
                int index = -1; //place holder for the column index where result value will be placed

                foreach (string line in lines)
                {
                    burPosition = 0;
                    resultValue = 0;
                    index = -1;

                    //Check if line(row) contains Mean GP on it
                    if (line.Contains("Mean GP;"))
                    {
                        index = meanGrainProtusionIndex;
                    }

                    //Check if line(row) contains grain coverage on it
                    else if (line.Contains("Cov;"))
                    {
                        index = grainCoverageIndex;
                    }

                    //Write to array if column index is not -1 (if it is GC columns index or mean grain protusion)
                    if (index == meanGrainProtusionIndex || index == grainCoverageIndex)
                    {
                        burPositionString = RetrieveBurPositionFromCSVString(line);
                        Int32.TryParse(burPositionString, out burPosition); //get bur position as an integer
                        resultValue = Convert.ToDouble(RetrieveBurResultsValueFromCSVString(line)); //gets mean grain protusion or grain coverage depending on index

                        //if the bur position is between 1 - 40 and the result is not zero, store value to bur results
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

        public double[,] GetBurResults(string filePath)
        {
            double[,] burResults = new double[40, 6];
            int meanGrainProtusionIndex = 1; //column index of mean grain protusion for burResults Matrix
            int grainCoverageIndex = 0; //column index of grain coverage for burResults Matrix
            int grainsSqMMIndex = 2;
            int avgDistTipsIndex = 3;
            int avgAreaIndex = 4;
            int avgDiameterIndex = 5;

            string[] lines = System.IO.File.ReadAllLines(filePath); //gets a list of each line of CSV file
            string burPositionString = ""; //place holder for bur position in string format

            int burPosition = 0; //place holder for bur position in integer format
            double resultValue = 0; //place holder for bur result value (MGP or GC) in double format
            int index = -1; //place holder for the column index where result value will be placed

            foreach (string line in lines)
            {
                burPosition = 0;
                resultValue = 0;
                index = -1;

                if (line.Contains("Cov;"))
                {
                    index = grainCoverageIndex;
                }
                //Check if line(row) contains Mean GP on it
                else if (line.Contains("Mean GP;"))
                {
                    index = meanGrainProtusionIndex;
                }

                else if (line.Contains("#/mm"))
                {
                    index = grainsSqMMIndex;
                }

                else if (line.Contains("Dist;"))
                {
                    index = avgDistTipsIndex;
                }

                else if (line.Contains("Area;"))
                {
                    index = avgAreaIndex;
                }

                else if (line.Contains("Dia;"))
                {
                    index = avgDiameterIndex;
                }
                //Write to array if column index is not -1 (if it is GC columns index or mean grain protusion)
                if (index != -1)
                {
                    burPositionString = RetrieveBurPositionFromCSVString(line);
                    Int32.TryParse(burPositionString, out burPosition); //get bur position as an integer
                    resultValue = Convert.ToDouble(RetrieveBurResultsValueFromCSVString(line)); //gets mean grain protusion or grain coverage depending on index

                    //if the bur position is between 1 - 40 and the result is not zero, store value to bur results
                    if ((burPosition - 1) >= 0 && (burPosition - 1) < 40 && resultValue > 0)
                    {
                        burResults[(burPosition - 1), index] = Math.Round(resultValue, 2);
                    }
                }
            }


            return burResults;
        }

        private string RetrieveBurPositionFromCSVString(string evaluateString)
        {
            /*
            Description: 
                This method retrieve the bur position from a string with the following format:
                "Position 1;<....>"
                <....> --> any string can come here and it won't affect the function

            Parameters:
            ----------
            evaluateString : string
                This is the string in which the position number will be retrieved from

            Returns:
            ----------
            burPosition : string
                this is the bur position in a string format

            Example:
            ----------
                n/a
            */
            string burPosition = StringBetween(evaluateString, "Position ", ";");
            return burPosition;
        }

        private string RetrieveBurResultsValueFromCSVString(string evaluateString)
        {
            /*
            Description: 
                This method retrieve the bur results from a string with the following format:
                "Position 1;<....>"
                <....> --> any string can come here and it won't affect the function
                The results will be either mean grain protusion or grain coverage depending on the input parameter.
                1) MGP: "Position 1;Cov;32.308;%;OK;0.000;0.000;0.000" --> this string format will return the grain coverage
                2) GC: "Position 1;Mean GP;16.20;µm;OK;0.00;0.00;0.00" --> this string format will return the grain coverage

            Parameters:
            ----------
            evaluateString : string
                This is the string in which the bur results will be retrieved from

            Returns:
            ----------
            burResult : string
                this is the value of the bur result (grain coverage or mean grain protusion)

            Example:
            ----------
                n/a
            */
            string[] columns = evaluateString.Split(';');
            string burResult = "";
            if (columns.Length >= 3)
            {
                burResult = columns[2];
            }
            return burResult;
        }

        public string StringBetween(string evaluateString, string firstString, string lastString)
        {
            /*
            Description: 
                This method retrieve a string between two other strings

            Parameters:
            ----------
            evaluateString : string
                This is the which you would like to evaluate 

            firstString : string
                This is the first string will be used to find the string in between

            lastString : string
                This is the last string will be used to find the string in between

            Returns:
            ----------
            finalString : string
                this will be the string between firstString and lastString. If nothing is found it will return an empty string

            Example:
            ----------
                //Find Glidewell in the string 2022-Glidewell-Laboratories
                string firstString = "2022-";
                string lastString = "-Laboratories";
                string evaluateString = "2022-Glidewell-Laboratories";

                string labName = "";
                labName = Between(evaluateString, firstString, lastString);
                Console.Writeline(labName); //This will print Glidewell
            */
            try
            {
                string finalString;
                int Pos1 = evaluateString.IndexOf(firstString) + firstString.Length;
                int Pos2 = evaluateString.IndexOf(lastString);
                finalString = evaluateString.Substring(Pos1, Pos2 - Pos1);
                return finalString;
            }
            catch
            {
                return "";
            }

        }
    }
}
