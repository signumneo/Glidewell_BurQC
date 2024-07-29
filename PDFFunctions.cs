using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf.parser;

namespace User_Interface
{
    class PDFFunctions
    {
        //---------------------------
        //Merge PDF Functions

        public string MergePDFs(List<string> filesToMerge, string mergedFileName, string mergedFilePath, string scanType = "top")
        {
            FilesFunctions filesFunctions = new FilesFunctions();

            //Add code to check if files are open 
            bool isFileInUse;
            string[] filesToMergeInOrder = new string[40];
            string mergedFilePathOutput;

            filesToMergeInOrder = OrderResults(filesToMerge, scanType);

            Console.WriteLine("Number of files to merge: " + filesToMergeInOrder.Length.ToString());
            for (int i = 0; i < filesToMergeInOrder.Length; i++)
            {
                Console.WriteLine(filesToMergeInOrder[i]);
            }


            try
            {
                PdfDocument outputDocument = new PdfDocument();

                Console.WriteLine("-----------------------files to merge-------------------------");
                foreach (string file in filesToMergeInOrder)
                {
                    if (string.IsNullOrEmpty(file))
                    {
                    }
                    else
                    {
                        //Open the document to import pages from it
                        Console.WriteLine("pdf open");
                        PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                        //Iterate pages 
                        Console.WriteLine("page  count");
                        int count = inputDocument.PageCount;
                        Console.WriteLine("adding pages");
                        for (int idx = 0; idx < count; idx++)
                        {
                            // Get the page from the external document
                            PdfPage page = inputDocument.Pages[idx];

                            //...and add to the output document
                            outputDocument.AddPage(page);
                            Console.WriteLine("added page: " +idx.ToString());
                        }
                    }
                }
                Console.WriteLine("merged file name");
                mergedFilePathOutput = mergedFilePath + "\\" + mergedFileName + ".pdf";
    
                isFileInUse = filesFunctions.IsFileLocked(mergedFilePathOutput);
                Console.WriteLine("is file in use: " + isFileInUse.ToString());
                outputDocument.Save(mergedFilePathOutput);
                Console.WriteLine("save pdf: ");
                return mergedFilePathOutput;
            }
            catch
            {
                return null;
            }
        }

        private string[] OrderResults(List<string> filesToOrder, string scanType = "top")
        {
            string[] filesInOrder = new string[40];
            
            if (filesToOrder != null)
            {
                foreach (string file in filesToOrder)
                {           
                    try
                    {
                        string fileNameNoExtension = System.IO.Path.GetFileNameWithoutExtension(file);
                        string[] fileNameNoExtensionParsed = Regex.Split(fileNameNoExtension, "_");
                        string burIndex = "";
                        string segmentIndex = "";
                        if (scanType == "top")
                        {
                            burIndex = fileNameNoExtensionParsed[fileNameNoExtensionParsed.Length - 1];
                            
                        }
                        else if (scanType == "side")
                        {
                            burIndex = fileNameNoExtensionParsed[fileNameNoExtensionParsed.Length - 2];
                            segmentIndex = fileNameNoExtensionParsed[fileNameNoExtensionParsed.Length - 1];
                        }

                        

                        //this is going to spill Position1 for instance

                        string[] scan_position_parsed = burIndex.Split('n'); //user the letter n
                        string[] segment_number_parsed = segmentIndex.Split('t'); //user the letter t

                        if (scan_position_parsed.Length == 2)
                        {
                            burIndex = scan_position_parsed[1];
                        }
                        else
                        {
                            burIndex = "";
                        }

                        if (segment_number_parsed.Length == 2)
                        {
                            segmentIndex = segment_number_parsed[1];
                        }
                        else
                        {
                            segmentIndex = "";
                        }



                        Int32.TryParse(burIndex, out int burPosition);
                        Int32.TryParse(segmentIndex, out int segmentPosition);

                        if (scanType == "top")
                        {
                            if (burPosition > 0 && burPosition <= 40)
                            {
                                filesInOrder[burPosition - 1] = file;
                            }
                        }

                        else if (scanType == "side")
                        {
                            if (burPosition > 0 && burPosition <= 9 && segmentPosition > 0 && segmentPosition <= 9)
                            {
                                /*
                                This will put the side scans in the following order:
                                position 1, segment 1
                                position 1, segment 2, 
                                position 1, segment 3,
                                position 2, segment 1,
                                position 2, segment 2, 
                                position 2, segment 3,
                                .
                                .
                                .
                                position 9, segment 1,
                                position 9, segment 2,
                                position 9, segment 3

                                */
                               

                                int numberOfSegments = 3;
                                int listIndex = (burPosition - 1) * numberOfSegments + (segmentPosition - 1);
                                Console.WriteLine("file path: " + file.ToString());
                                Console.WriteLine("bur #: " +burPosition.ToString() + ", segment #: " + segmentPosition.ToString() + ", listIndex: " + listIndex.ToString());

                                filesInOrder[listIndex] = file;
                            }
                              
                        }                       
                    }
                    catch
                    {
                        filesInOrder = null;
                        break;
                    }
                }
            }

            return filesInOrder;
        }

        //---------------------------
        //Extract PDF Grain Coverage and Mean Grain Protusion Results

        private double grainCoverage;
        private double meanGrainProtusion;
        private double grainsPerSqMM;
        private double avgDistTips;
        private double avgArea;
        private double avgDiameter;
        private void ExtractGrainCoverage(String[] parsedPDFContent)
        {
            int parsedPDFContentIndex = 0;
            for (int i = 0; i < parsedPDFContent.Length; i++)
            {
                if (-1 != parsedPDFContent[i].IndexOf("coverage"))
                {
                    parsedPDFContentIndex = i + 1;
                    break;
                }
            }
            try
            {
                grainCoverage = Convert.ToDouble(parsedPDFContent[parsedPDFContentIndex]);
                //Console.WriteLine("The grain coverage is: " + SplitPDF_parts[my_index]);
            }
            catch (FormatException)
            {
                //Console.WriteLine("Unable to extract Grain Coverage.");
                grainCoverage = Global_constants.err_double;
            }

        }

        private void ExtractMeanGrainProtusion(String[] parsedPDFContent)
        {
            try
            {
                for (int i = 0; i < parsedPDFContent.Length; i++)
                {
                    if (parsedPDFContent[i] == "grain")
                    {
                        if (parsedPDFContent[i + 1] == "protrusion")
                        {
                            Console.WriteLine("protusion found");
                            Console.WriteLine("Mean Grain Protusion = " + parsedPDFContent[i + 2]);
                            Console.WriteLine("Length: " + parsedPDFContent[i + 2].Length.ToString());
                            string meanGrainProtusion = "";
                            double meanGrainProtusionDouble = 0;

                            int j = 0;
                            foreach (char c in parsedPDFContent[i + 2])
                            {
                                Console.WriteLine("/" + c.ToString() + "/");
                                if (c.ToString() == "s")
                                {
                                    meanGrainProtusion = meanGrainProtusion.Substring(0, (j - 1));
                                    break;
                                }
                                else
                                {
                                    meanGrainProtusion = meanGrainProtusion + c.ToString();
                                    j += 1;
                                }
                            }
                            Console.WriteLine("/" + meanGrainProtusion + "/");
                            meanGrainProtusionDouble = Convert.ToDouble(meanGrainProtusion);
                            this.meanGrainProtusion = meanGrainProtusionDouble;
                            break;
                        }
                    }
                }
            }
            catch
            {
                meanGrainProtusion = -2;
                Console.WriteLine("Unable to extract Grain Coverage.");
                meanGrainProtusion = Global_constants.err_double;
            }
        }

        public Tuple<double, double> ExtractMGPAndGCFromPDF(string pdfFileAddress)
        {
            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(pdfFileAddress);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            SimpleTextExtractionStrategy strategy;
            string extractionResult = "";
            String[] parsedPDFContent = null;
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                strategy = parser.ProcessContent(i, new SimpleTextExtractionStrategy());
                extractionResult = strategy.GetResultantText();
                parsedPDFContent = (extractionResult.ToLower().Split(new char[] { ' ' }));
            }
            ExtractGrainCoverage(parsedPDFContent);
            ExtractMeanGrainProtusion(parsedPDFContent);
            reader.Close();

            return Tuple.Create(grainCoverage, meanGrainProtusion);
        }

        private double[] ExtractBurInfo(String[] parsedPDFContent)
        {
            try
            {
                double[] burInfo = new double[6];
                for (int i = 0; i < parsedPDFContent.Length; i++)
                {
                    if (parsedPDFContent[i] == "coverage")
                    {
                        Console.WriteLine($"Grain Coverage = {parsedPDFContent[i + 1]}");
                        string grainCoverageString = parsedPDFContent[i + 1];
                        double grainCoverage = Convert.ToDouble(grainCoverageString);
                        this.grainCoverage = grainCoverage;
                        burInfo[0] = this.grainCoverage;
                    }
                    else if (parsedPDFContent[i] == "%\nmean")
                    {
                        if (parsedPDFContent[i + 2] == "protrusion")
                        {
                            Console.WriteLine($"Mean Grain Protrusion = {parsedPDFContent[i + 3]}");
                            string meanGrainProtusionString = parsedPDFContent[i + 3];
                            double meanGrainProtusion = Convert.ToDouble(meanGrainProtusionString);
                            this.meanGrainProtusion = meanGrainProtusion;
                            burInfo[1] = this.meanGrainProtusion;
                        }
                    }
                    else if (parsedPDFContent[i] == "millimeter")
                    {
                        Console.WriteLine($"Grains per SqMM = {parsedPDFContent[i + 1]}");
                        string grainsSqMMString = parsedPDFContent[i + 1];
                        double grainsSqMM = Convert.ToDouble(grainsSqMMString);
                        this.grainsPerSqMM = grainsSqMM;
                        burInfo[2] = this.grainsPerSqMM;
                    }
                    else if (parsedPDFContent[i] == "tips")
                    {
                        Console.WriteLine($"Average Distance Between Tips = {parsedPDFContent[i + 1]}");
                        string avgDistString = parsedPDFContent[i + 1];
                        double avgDist = Convert.ToDouble(avgDistString);
                        this.avgDistTips = avgDist;
                        burInfo[3] = this.avgDistTips;
                    }
                    else if (parsedPDFContent[i] == "area")
                    {
                        Console.WriteLine($"Average Area = {parsedPDFContent[i + 3]}");
                        string avgAreaString = parsedPDFContent[i + 3];
                        double avgArea = Convert.ToDouble(avgAreaString);
                        this.avgArea = avgArea;
                        burInfo[4] = this.avgArea;
                    }
                    else if (parsedPDFContent[i] == "diameter")
                    {
                        Console.WriteLine($"Average Diameter = {parsedPDFContent[i + 3]}");
                        string avgDiamString = parsedPDFContent[i + 3];
                        double avgDiam = Convert.ToDouble(avgDiamString);
                        this.avgDiameter = avgDiam;
                        burInfo[5] = this.avgDiameter;
                    }
                }
                return burInfo;

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public double[] ExtractBurInfoFromPDF(string pdfFileAddress)
        {
            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(pdfFileAddress);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            SimpleTextExtractionStrategy strategy;
            string extractionResult = "";
            String[] parsedPDFContent = null;
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                strategy = parser.ProcessContent(i, new SimpleTextExtractionStrategy());
                extractionResult = strategy.GetResultantText();
                parsedPDFContent = (extractionResult.ToLower().Split(new char[] { ' ' }));
            }
            ExtractGrainCoverage(parsedPDFContent);
            ExtractMeanGrainProtusion(parsedPDFContent);
            double[] burInfo = ExtractBurInfo(parsedPDFContent);
            reader.Close();

            return burInfo;
        }
    }
}
