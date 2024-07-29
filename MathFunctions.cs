using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace User_Interface
{
    class MathFunctions
    {
        public double[] RemoveZeros(double[] array)
        {
            List<double> listOfNumbers = new List<double>();
            double[] arrayNoZeros;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != 0)
                {
                    listOfNumbers.Add(array[i]);
                }
            }
            arrayNoZeros = listOfNumbers.ToArray();
            return arrayNoZeros;
        }

        public double CalculateMedian(double[] numbersArray, bool ignoreZeroValues = true)
        {
            int arrayLength = numbersArray.Length;
            double median;
            

            double[] numbersArrayNoZeroSorted = RemoveZeros(numbersArray);
            Array.Sort(numbersArrayNoZeroSorted);

            Array.Sort(numbersArray);

            double[] numbersForMedianCalculation;



            if (ignoreZeroValues == true)
            {
                numbersForMedianCalculation = numbersArrayNoZeroSorted;
            }
            else
            {
                numbersForMedianCalculation = numbersArray;
            }

            int numberCount = numbersForMedianCalculation.Count();
            
            if (numberCount == 0)
            {
                median = 0;
            }
            else
            {
                if ((numberCount % 2) == 0)
                {
                    double medianHigh = numbersForMedianCalculation[numberCount/2];
                    double medianLow = numbersForMedianCalculation[(numberCount/2) - 1];
                    median = (medianHigh + medianLow) / 2;
                }
                else
                {
                    double halfIndex = numberCount / 2;
                    int medianIndex = Convert.ToInt32(Math.Floor(halfIndex));
                    median = numbersForMedianCalculation[medianIndex];
                }
            }

            return median;
        }
        public double CalculateStandardDeviation(double[] numbersArray, bool ignoreZeroValues = true)
        {
            double standardDeviation = 0;
            double average = 0;
            double meanSquareSum = 0;
            int numberOfDataPoints = 0;
            int arrayLength = numbersArray.Length;

            if (ignoreZeroValues == true)
            {
                average = CalculateAverage(numbersArray, true);
                numberOfDataPoints = FindNumberOfNonZerosNumbers(numbersArray);
            }
            else
            {
                average = CalculateAverage(numbersArray, false);
                numberOfDataPoints = arrayLength;
            }

            for (int i = 0; i < arrayLength; i++)
            {
                if (ignoreZeroValues == true)
                {
                    if (numbersArray[i] != 0)
                    {
                        meanSquareSum += Math.Pow((numbersArray[i] - average), 2);
                    }
                }
                else
                {
                    meanSquareSum += Math.Pow((numbersArray[i] - average), 2);
                }
            }

            if (numberOfDataPoints != 0)
            {
                standardDeviation = Math.Sqrt(meanSquareSum / numberOfDataPoints);
            }
            else
            {
                standardDeviation = 0;
            }
            standardDeviation = Math.Round(standardDeviation, 2);
            return standardDeviation;
        }

        public double CalculateMaxValue(double[] numbersArray, bool ignoreZeroValues = true)
        {
            double maxValue = 0;

            int arrayLength = numbersArray.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (numbersArray[i] > maxValue)
                {
                    maxValue = numbersArray[i];
                }
            }

            maxValue = Math.Round(maxValue, 2);
            return maxValue;
        }

        public double CalculateMinValue(double[] numbersArray, bool ignoreZeroValues = true)
        {
            double minValue = 10000000000000;
            int arrayLength = numbersArray.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (numbersArray[i] < minValue && numbersArray[i] != 0)
                {
                    minValue = numbersArray[i];
                }
            }

            minValue = Math.Round(minValue, 2);
            return minValue;
        }

        public double CalculateAverage(double[] numbersArray, bool ignoreZeroValues = true)
        {
            double average = 0;
            double sum = 0;
            int numberOfNonZeroValues = FindNumberOfNonZerosNumbers(numbersArray);
            int arrayLength = numbersArray.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (numbersArray[i] != 0)
                {
                    sum += numbersArray[i];
                }
            }

            if (ignoreZeroValues == true)
            {
                if (numberOfNonZeroValues != 0)
                {
                    average = sum / numberOfNonZeroValues;
                }
                else
                {
                    average = 0;
                }
            }
            else
            {
                if (numberOfNonZeroValues != 0)
                {
                    average = sum / arrayLength;
                }
                else
                {
                    average = 0;
                }
            }
            average = Math.Round(average, 2);
            return average;
        }

        public int FindNumberOfNonZerosNumbers(double[] numbersArray)
        {
            int numberOfNonZeroValues = 0;
            int arrayLength = numbersArray.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                if (numbersArray[i] != 0)
                {
                    numberOfNonZeroValues += 1;
                }
            }

            return numberOfNonZeroValues;
        }

        public double[]  GetRow(double[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        public double[] GetColumn(double[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        public string[] GetRowPerColIndex(string[,] matrix, int rowNumber, int startingCol, int endingCol)
        {
            string[] arrayStringReformated = new string[(endingCol - startingCol + 1)];

            for (int col = startingCol; col <= endingCol; col++)
            {
                arrayStringReformated[(col- startingCol)] = matrix[rowNumber, col];
            }
          
            return arrayStringReformated;
        }

        public double[] StringToDoubleArray(string[] stringArray)
        {
            double[] doubleArray = new double[stringArray.Length];

            for (int i = 0; i < stringArray.Length; i++)
            {
                try
                {
                    if (stringArray[i] != null)
                    {
                        doubleArray[i] = Double.Parse(stringArray[i]);
                    }
                    else
                    {
                        doubleArray[i] = 0;
                    }
                }
                catch
                {
                    doubleArray[i] = 0;
                }
            }
            return doubleArray;
        }
    }
}
