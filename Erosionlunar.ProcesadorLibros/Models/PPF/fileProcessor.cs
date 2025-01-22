using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Google.Protobuf;

namespace Erosionlunar.ProcesadorLibros.Models.PPF
{
    public class fileProcessor
    {
        /// <summary>
        /// Given a list of fileLibro it arrange it and return the same list but
        /// in the correct order of process.
        /// </summary>
        /// <param name="theFiles">List of fileLibros</param>
        public List<fileLibro> getFinalOrderFiles(List<fileLibro> theFiles)
        {
            numberOrderLibros(theFiles);  // sets the order to process of libro of each fileLibro
            numberOrderFechas(theFiles);// sets the order to process of date of each fileLibro
            numberOrderScale(theFiles);// sets the order to process each fileLibro
            return theFiles.OrderBy(f => f.orderScale).ToList();
        }
        /// <summary>
        /// Given a list of fielLibros it assign a number to each individual Id and
        /// and set the orderLibro value of each file libro to that number.
        /// </summary>
        /// <param name="theFiles">List of fileLibros</param>
        private void numberOrderLibros(List<fileLibro> theFiles)
        {
            var listIds = new List<string>();
            foreach(fileLibro oneFile in theFiles)
            {
                if (!listIds.Contains(oneFile.id))
                {
                    listIds.Add(oneFile.id);
                }
                oneFile.orderLibro = listIds.IndexOf(oneFile.id);
            }
        }
        /// <summary>
        /// Given a list of fielLibros it calculates the orderScale value by making a scale
        /// with the orderLibro, fraccion and orderDate values.
        /// </summary>
        /// <param name="theFiles">List of fileLibros</param>
        private void numberOrderScale(List<fileLibro> theFiles)
        {
            foreach(fileLibro oneFile in theFiles)
            {
                var scaleId = oneFile.orderLibro * 1000000;
                var scaleFraccion = Int32.Parse(oneFile.fraccion) * 10000;
                var scaleDate = oneFile.orderDate * 10;
                oneFile.orderScale = scaleId + scaleFraccion + scaleDate;
            }
        }
        /// <summary>
        /// Given a list of fielLibros it assign a number to each individual date and
        /// and set the orderDate value of each file libro to that number.
        /// </summary>
        /// <param name="theFiles">List of fileLibros</param>
        private void numberOrderFechas(List<fileLibro> theFiles)
        {
            var listDates = new List<DateTime>();
            foreach(fileLibro oneFile in theFiles)
            {
                if (!listDates.Contains(oneFile.date))
                {
                    listDates.Add(oneFile.date);
                }
            }
            listDates = listDates.OrderByDescending(date => date).ToList();
            foreach(fileLibro oneFile in theFiles)
            {
                oneFile.orderDate = listDates.IndexOf(oneFile.date);
            }
        }
        /// <summary>
        /// Opens the path given and adds each line to a list the amount of times also given 
        /// and returns the list.
        /// </summary>
        /// <param name="cantidad">Number of lines to add to the list.</param>
        /// <param name="direA">Path of the file to open.</param>
        private List<string> getLineasTXT(string direA, int cantidad)
        {
            var lasLineas = new List<string>();
            using (StreamReader reader = new StreamReader(direA))
            {
                for (int i = 0; i < cantidad; i++)
                {
                    // Read each line and add it to the list
                    string line = reader.ReadLine();

                    // Break if we reach the end of the file
                    if (line == null)
                        break;

                    lasLineas.Add(line);
                }
            }
            return lasLineas;
        }
        /// <summary>
        /// Opens the PDF path given and adds each line to a list the amount of times also given 
        /// and returns the list.
        /// </summary>
        /// <param name="cantidad">Number of lines to add to the list.</param>
        /// <param name="direA">Path of the file to open.</param>
        private List<string> getLineasPDF(string direA, int cantidad)
        {
            var lasLineas = new List<string>();
            using (PdfReader reader = new PdfReader(direA))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                PdfPage firstPage = pdfDoc.GetPage(1);
                List<string> firstPageText = PdfTextExtractor.GetTextFromPage(firstPage).Split('\n').ToList();
                lasLineas = firstPageText.GetRange(0, cantidad);
            }
            return lasLineas;
        }
        /// <summary>
        /// Given a path to a file and the amount of lines to take out of that file it
        /// opens it reads it and takes out the lines and returns them.
        /// </summary>
        /// <param name="amountLines">Number of lines to add to the list.</param>
        /// <param name="pathFile">Path of the file to open.</param>
        public List<string> getLineas(string pathFile, int amountLines)
        {
            var response = new List<string>();
            var extension = Path.GetExtension(pathFile).ToLower();
            if (extension == ".txt")
            {
                response = getLineasTXT(pathFile, 10);
            }
            else if (extension == ".pdf")
            {
                response = getLineasPDF(pathFile, 10);
            }
            return response;
        }

    }
}
