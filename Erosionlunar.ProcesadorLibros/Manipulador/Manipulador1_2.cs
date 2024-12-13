using Erosionlunar.ProcesadorLibros.Abstract;
using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
namespace Erosionlunar.ProcesadorLibros.Manipulador
{
    public class Manipulador1_2 : ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            return getDateTimeGeneric(3, dirA);
        }
        public override List<int> modFile(int startingPageNumber, int startingEntry, string startingPath, string finalPath)
        {
            var lastFolio = modSpecific(startingPageNumber, startingPath, finalPath);

            var lastEntry = 0;
            return new List<int> { lastFolio, lastEntry };
        }
        public override void setProcessRegexFromString(List<string> rawRegex)
        {
            processRegex = new List<Regex>();
            
            var regexPageNumber = new Regex(rawRegex[0]);
            var regexExclussions = new Regex(rawRegex[1]);
            processRegex.Add(regexPageNumber);
            processRegex.Add(regexExclussions);
        }
        private int modSpecific(int pageNumberF, string pathF, string pathModF)
        {
            var changingPageNumber = pageNumberF.ToString();
            var pathNewF = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pathF), pathModF);
            var finalEncod = Encoding.GetEncoding(1252);
            var sizeMaxLine = 0;

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
                            if(sizeMaxLine < newLine.Length)
                            {
                                sizeMaxLine = newLine.Length;
                            }
                            if(sizeMaxLine > newLine.Length && processRegex[1].IsMatch(newLine))
                            {
                                newLine = new string(' ',  sizeMaxLine - newLine.Length) + newLine;
                            }
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
    }
}
