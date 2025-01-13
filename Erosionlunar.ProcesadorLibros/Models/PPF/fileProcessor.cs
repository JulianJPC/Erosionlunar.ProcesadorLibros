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
        /// Given the list of dates, Ids and fracciones calculates the order to process the libros. And returns this order a list<int>
        /// </summary>
        /// <param name="dates">List of Dates of a list of libros</param>
        /// <param name="ids">List of ids of a list of libros</param>
        /// <param name="fracciones">List of fracciones of a list of libros</param>
        public List<int> getFinalOrderFiles(List<DateTime> dates, List<string> ids, List<string> fracciones)
        {
            var amountDates = dates.Distinct().ToList().Count; //unique dates
            var amountIds = ids.Distinct().ToList().Count;    // unique IdL
            var orderLibros = numberOrderLibros(ids);  // gets List with the index of order of each Id libro, first is 0
            var orderFechas = numberOrderFechas(dates);// gets List with the index of order of each Id libro, first is 0
            var minFraccionArch = calculateMinFracciones(ids, dates, fracciones); // List with the minimum fraccion of each libro.
            return calculateOrder(orderLibros, orderFechas, amountIds, amountDates, fracciones, minFraccionArch);
        }
        /// <summary>
        /// Given a list of strings it assign a number to each individual Id and then 
        /// makes a list of the of the same size as the original with the assign number
        /// of each Id
        /// </summary>
        /// <param name="listIdL">List of Ids of a list of libros</param>
        private List<int> numberOrderLibros(List<string> listIdL)
        {
            var response = new List<int>();
            var uniquesIds = listIdL.Distinct().ToList();
            for (int i = 0; i < listIdL.Count; i++)
            {
                response.Add(uniquesIds.IndexOf(listIdL[i]));
            }
            return response;
        }
        /// <summary>
        /// Given a list of DateTime it assign a number to each individual Date in chronological order and then 
        /// makes a list of the of the same size as the original with the assign number
        /// of each Id
        /// </summary>
        /// <param name="dates">List of dates of a list of libros</param>
        private List<int> numberOrderFechas(List<DateTime> dates)
        {
            var response = new List<int>();
            var orderedDate = dates.Distinct().ToList().OrderByDescending(date => date).ToList();
            for (int i = 0; i < dates.Count; i++)
            {
                response.Add(orderedDate.IndexOf(dates[i]));
            }
            return response;
        }
        /// <summary>
        /// Given a list of Ids, Dates and fracciones calculates a list of the minimum number of fraccion
        /// corresponding with each fraccion and the returns that list.
        /// </summary>
        /// <param name="dates">List of Dates of a list of libros</param>
        /// <param name="ids">List of ids of a list of libros</param>
        /// <param name="fracciones">List of fracciones of a list of libros</param>
        private List<string> calculateMinFracciones(List<string> Ids, List<DateTime> dates, List<string> fraciones)
        {
            var indexesLibrosSame = new List<List<int>>();
            var response = new List<string>();
            for (int i = 0; i < Ids.Count; i++)
            {
                var listIndexSameIandD = searchIndexesDateAndIdL(Ids[i], dates[i], Ids, dates);
                indexesLibrosSame.Add(listIndexSameIandD);
            }

            foreach (List<int> oneListIndex in indexesLibrosSame)
            {
                response.Add(searchLowerFraccionOfIndexes(oneListIndex, fraciones));
            }
            return response;
        }
        /// <summary>
        /// Given a list of Ids, list of Dates, a Date amd an Id, it makes a list with the indexes that have that same
        /// Id and Date in the list and returns it
        /// </summary>
        /// <param name="listDates">List of Dates of a list of libros</param>
        /// <param name="listIdL">List of ids of a list of libros</param>
        /// <param name="idL">A Id of libro</param>
        /// <param name="date">A date of libro</param>
        private List<int> searchIndexesDateAndIdL(string idL, DateTime date, List<string> listIdL, List<DateTime> listDates)
        {
            var response = new List<int>();
            for(int i = 0; i < listIdL.Count; i++)
            {
                if(listIdL[i] == idL && listDates[i] == date)
                {
                    response.Add(i);
                }
            }
            return response;
        }
        /// <summary>
        /// Given a list of the indexes that have same Date and IdL, searches in the list of Fracciones
        /// with those indexes and chooses the index of minimum value.
        /// </summary>
        /// <param name="indexes">List of indexes that have same Date and Id libro.</param>
        /// <param name="listFracciones">List of fracciones of libros</param>
        private string searchLowerFraccionOfIndexes(List<int> indexes, List<string> listFracciones)
        {
            var response = 999;
            foreach(int i in indexes)
            {
                if (Int32.Parse(listFracciones[i]) < response)
                {
                    response = Int32.Parse(listFracciones[i]);
                }
            }
            return response.ToString();
        }
        /// <summary>
        /// Given a list of the order of the ids Libros, other list of the order of the dates, the amount of Ids Libros,
        /// the amount of dates, a list of fracciones and a list of the minimum fracciones, calculte a default order of
        /// wich the libros will be processed. Puts that order in a list and returns it
        /// </summary>
        /// <param name="ordersLibros">List of the order to process each book.</param>
        /// <param name="ordersDates">List of the order to process each date.</param>
        /// <param name="amountIds">How many unique ids there are.</param>
        /// <param name="amountDates">How many unique dates there are.</param>
        /// <param name="fracciones">List of fracciones of libros.</param>
        /// <param name="amountDates">List of minimun of each fraccion of a date and id libro.</param>
        private List<int> calculateOrder(List<int> ordersLibros, List<int> ordersDates, int amountIds, int amountDates, List<string> fracciones, List<string> minListFracciones)
        {
            var response = new List<int>();
            var orderInScale = new List<int>();
            var levelsScale = 0;
            for (int i = 0; i < ordersLibros.Count; i++)
            {
                var level = (ordersLibros.Count() + ordersDates.Count() + fracciones.Count) * levelsScale;
                var IdsOrder = ordersLibros[i];
                var dateOrder = amountIds + ordersDates[i];
                int fraccionOrder = amountIds + amountDates;
                if (fracciones[i] == "0")
                {
                    fraccionOrder += 1;
                }
                else
                {
                    fraccionOrder += Int32.Parse(fracciones[i]) - Int32.Parse(minListFracciones[i]) + 1;
                }
                orderInScale.Add(IdsOrder + dateOrder + fraccionOrder + level);
                levelsScale++;
            }
            var scaleMinToMax = orderInScale.OrderByDescending(order => order).ToList();
            foreach (int oneOrder in orderInScale)
            {
                response.Add(scaleMinToMax.IndexOf(oneOrder) + 1);
            }
            return response;
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
