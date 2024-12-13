using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;

namespace Erosionlunar.ProcesadorLibros.Abstract
{
    public abstract class ABSManipulador
    {
        protected List<Regex> processRegex;
        protected string pageNumberName;
        protected int qSpacesToTakeOut;
        protected Encoding openerEncod;
        /// <summary>
        /// Default method to get a Date of a Libro file.
        /// </summary>
        /// <param name="pathF">File path of a Libro file.</param>
        /// <remarks>
        /// Only returns the current Date.
        /// </remarks>
        public virtual DateTime getFecha(string pathF) { return DateTime.Now; }
        /// <summary>
        /// Generic method to get a Date of a Libro file. 
        /// The file must be a .txt.
        /// </summary>
        /// <param name="qLines">Quantity of lines that are taken from the start of the file</param>
        /// <param name="pathF">File path of a Libro file.</param>
        /// <remarks>
        /// Search in the file a date with this pattern:  \d{1,2}\s?/\s?\d{4}
        /// </remarks>
        protected DateTime getDateTimeGeneric(int qLines, string pathF)
        {
            var theLines = getLines(pathF, qLines);
            var regexDate = new Regex(@"\d{1,2}\s?/\s?\d{4}");
            var match = Regex.Match("", "nonmatchingpattern");
            var theDate = DateTime.Now;                                      // Default value of laFecha
            var counter = 0;
            //Search for pattern match in lines
            while (!match.Success && counter < qLines)                 
            {
                match = regexDate.Match(theLines[counter]);
                counter++;
            }
            //If it has a match it formats the match string and converts it to DateTime
            if (match.Success)                                         
            {
                var rawDate = match.ValueSpan.ToString();
                var splitedDate = rawDate.Split('/').ToList(); 
                var formattedDate = String.Join('/', "01", addCeroMonth(splitedDate[0].Trim()), splitedDate[1].Trim());
                theDate = DateTime.ParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            return theDate;
        }
        /// <summary>
        /// Generic method to get a Date of a Libro file. 
        /// The file must be a .pdf.
        /// </summary>
        /// <param name="qCharacters">Quantity of characters that are taken from the start of the file</param>
        /// <param name="pathF">File path of a Libro file.</param>
        /// <remarks>
        /// Search in the file a date with this pattern:  \d{1,2}\s?/\s?\d{4}
        /// </remarks>
        protected DateTime getDateTimeGenericPDF(int qCharacters, string pathF)
        {
            var firstPageText = "";
            var regex = new Regex(@"\d{1,2}\s?/\s?\d{4}");
            var match = Regex.Match("", "nonmatchingpattern");
            var laFecha = DateTime.Now;                                           // Default value of laFecha
            //Open PDF and gets characters first page
            using (PdfReader reader = new PdfReader(pathF))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                PdfPage firstPage = pdfDoc.GetPage(1);
                firstPageText = PdfTextExtractor.GetTextFromPage(firstPage);
            }
            firstPageText = firstPageText.Substring(0, qCharacters);
            match = regex.Match(firstPageText);
            //If it gets a match it parse the string to get the date
            if (match.Success)
            {
                var rawDate = match.ValueSpan.ToString();
                var splitedDate = rawDate.Split('/').ToList();
                var formattedDate = String.Join('/', "01", addCeroMonth(splitedDate[0].Trim()), splitedDate[1].Trim());
                laFecha = DateTime.ParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            return laFecha;
        }
        /// <summary>
        /// It get an amount of lines of a txt file.
        /// </summary>
        /// <param name="quantity">Quantity of lines that are taken from the start of the file</param>
        /// <param name="pathF">File path of a Libro file.</param>
        protected List<string> getLines(string pathF, int quantity)
        {
            var theLines = new List<string>();
            using (StreamReader reader = new StreamReader(pathF))
            {
                for (int i = 0; i < quantity; i++)
                {
                    // Read each line and add it to the list
                    string line = reader.ReadLine();
                    // Break if we reach the end of the file
                    if (line == null) { break; }
                    theLines.Add(line);
                }
            }
            return theLines;
        }
        /// <summary>
        /// Returns the MD5 hash of a file
        /// </summary>
        /// <param name="pathF">File path of a Libro file.</param>
        private string CalculateMD5(string pathF)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(pathF))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }
        /// <summary>
        /// Returns the line and page number, if it gets a match it changes the page number
        /// </summary>
        /// <param name="lineRaw">Line of the libro file.</param>
        /// <param name="lastPageNumber">Last page number used.</param>
        /// <param name="theRegex">It matches the line that has the page number.</param>
        /// <param name="qSpaces">Quantity of spaces to take out of the end of the line.</param>
        /// <param name="numberName">The word that announce the page number.</param>
        /// <remarks>
        /// If it doesn't make a match it returns the same line and page number.
        /// </remarks>
        protected List<string> modLinePageNumber(string lineRaw, string lastPageNumber)
        {
            var lineAndFol = new List<string>(2);
            var line = lineRaw.Replace("\u000C", "").TrimEnd();//\u000C ♀
            var newPageNumber = lastPageNumber;
            //if the lines match it gets a new line and the next page number
            if (processRegex[0].IsMatch(line))
            {
                line = changePageNumberLine(line, newPageNumber);
                newPageNumber = (Int32.Parse(newPageNumber) + 1).ToString();
            }
            lineAndFol.Add(line);
            lineAndFol.Add(newPageNumber);
            return lineAndFol;
        }

        /// <summary>
        /// Takes a line and takes out characters from the end and adds a word and number insted.
        /// </summary>
        /// <param name="lineRaw">Line of the libro file.</param>
        /// <param name="pageNumber">Page number used.</param>
        /// <param name="qTakenOut">Quantity of spaces to take out of the end of the line.</param>
        /// <param name="numberName">The word that announce the page number.</param>
        /// <remarks>
        /// If the amount to take out is longer that the line it only returns the word before the page number and number.
        /// </remarks>
        private string changePageNumberLine(string lineRaw, string pageNumber)
        {
            StringBuilder newLine;
            var beforeLine = lineRaw;
            var qToTakeOut = qSpacesToTakeOut;
            // If the line is longer that the spaces to take out of the end.
            if (beforeLine.Length < qToTakeOut)
            {
                beforeLine = "";
                qToTakeOut = 0;
            }
            newLine = new StringBuilder(beforeLine.Length + pageNumberName.Length + pageNumber.Length);
            newLine.Append(beforeLine, 0, beforeLine.Length - qToTakeOut); //The line without some characters at the end.
            newLine.Append(pageNumberName);
            newLine.Append(pageNumber);
            return newLine.ToString();
        }

        /// <summary>
        /// Default method to modify a libro file. At the end returns the last page number and last accounting entry.
        /// </summary>
        /// <param name="pageNumberF">First Page number.</param>
        /// <param name="entryF">First accounting entry.</param>
        /// <param name="pathF">File path of a Libro file.</param>
        /// <param name="finalPathF">File path of the modify Libro file.</param>
        /// <param name="theEncod">The encoding used to open the raw libro file.</param>
        /// <param name="theRegex">The regex used in the process.</param>
        /// <remarks>
        /// Only returns an empty List<int>.
        /// </remarks>
        public virtual List<int> modFile(int pageNumberF, int entryF, string pathF, string finalPathF)
        {
            return new List<int>();
        }

        /// <summary>
        /// Returns the number of a month given. If the number is only one digit it adds a cero before.
        /// </summary>
        /// <param name="theMonth">The number of the month.</param>
        protected string addCeroMonth(string theMonth)
        {
            string response = theMonth;
            if(response.Length == 1)
            {
                response = "0" + response;
            }
            return response;
        }
        /// <summary>
        /// Generic method used to modify a libro file. Returns the last page number entry used.
        /// </summary>
        /// <param name="qSpaces">Amount of spaces to take out of the end of a page number line.</param>
        /// <param name="numberName">The word that announce the page number.</param>
        /// <param name="pathF">File path of a Libro file.</param>
        /// <param name="pathModF">File path of the modify Libro file.</param>
        /// <param name="pageNumberF">First Page number.</param>
        /// <param name="theEncod">The encoding used to open the raw libro file.</param>
        /// <param name="theRegex">The regex used in the process.</param>
        protected int modFastGeneric(int pageNumberF, string pathF, string pathModF)
        {
            var changingPageNumber = pageNumberF.ToString();
            var pathNewF = Path.Combine(Path.GetDirectoryName(pathF), pathModF);
            var finalEncod = Encoding.GetEncoding(1252);

            using (StreamWriter writer = new StreamWriter(pathNewF, true, finalEncod))
            {
                using (StreamReader reader = new StreamReader(pathF, openerEncod))
                {
                    char[] buffer = new char[4096]; // Smaller buffer size for reduced memory usage
                    string remainder = "";
                    while (true)
                    {
                        int bytesRead = reader.Read(buffer, 0, buffer.Length);          //gets an amount of bytes of the file
                        if (bytesRead == 0) { break; } //If its the EOF  
                        string chunk = remainder + new string(buffer, 0, bytesRead);   // it turns the bytes to string
                        string[] lines = chunk.Split('\n');                            // It breaks the string and creates the array of lines
                        //loops through the list of lines and modify if its a page number line except the last index
                        for (int i = 0; i < lines.Length - 1; i++)
                        {
                            var aLine = lines[i];
                            var lineAndPageNumber = modLinePageNumber(aLine, changingPageNumber);
                            var newLine = lineAndPageNumber[0];
                            changingPageNumber = lineAndPageNumber[1];
                            if (!string.IsNullOrEmpty(newLine))
                            {
                                writer.WriteLine(newLine);
                            }
                        }
                        remainder = lines[lines.Length - 1];           // Adds the last line to the reminder that it could be not a whole line.
                    }
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        writer.WriteLine(remainder);
                    }
                }
            }
            int pageNumberL = (Int32.Parse(changingPageNumber) - 1);
            File.Delete(pathF);
            return pageNumberL;
        }
        public virtual string getNameFile(DateTime theDate, string fraccion, string nameFile, string pathFile)
        {
            var month = addCeroMonth(theDate.Month.ToString());
            var year = theDate.Year.ToString().Substring(2,2);
            var folderPath = Path.GetDirectoryName(pathFile);
            var extension = Path.GetExtension(pathFile).ToLower();
            var response = String.Join("\\", folderPath, nameFile + month + year);
            if(fraccion != "0")
            {
                response += $"P{fraccion}";
            }
            response += extension;
            return response;
        }
        public virtual void setProcessRegexFromString(List<string> rawRegex)
        {
            processRegex = new List<Regex>();
            var newRegex = new Regex(rawRegex[0]);
            processRegex.Add(newRegex);
        }
        public virtual void setProcessInfo(List<string> theList)
        {
            pageNumberName = theList[0];
            qSpacesToTakeOut = Int32.Parse(theList[1]);
            
        }
        public void setEncoding(string encod)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            openerEncod = Encoding.GetEncoding(Int32.Parse(encod));
        }
    }
}