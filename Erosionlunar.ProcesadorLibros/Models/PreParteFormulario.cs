using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Erosionlunar.ProcesadorLibros.Models
{
    class PreParteFormulario
    {
        private string numeroPreParte;
        private string idEmpresa;
        private string inicioE;
        private List<Libro> losLibros;
        private List<string> startingFilesPaths;        
        private List<string> Errores;

        private List<DateTime> fechasFilesPosibles;
        private List<string> idLibrosPosibles;

        private List<DateTime> fechasFilesFinales;
        private List<string> idLibrosFinales;
        private List<string> fraccionesFinales;
        private List<string> pathsFinales;
        private List<int> orderOfFiles;

        public string numeroP => numeroPreParte;
        public string idEmp => idEmpresa;
        public string inicioEjercicio => inicioE;
        public List<string> pathsFinal => pathsFinales;
        public List<string> theErrors => Errores;
        

        public PreParteFormulario()
        {
            Errores = new List<string>();
        }
        public void addError(string theError)
        {
            Errores.Add(theError);
        }

        private bool hasDateAllTheSameIdLibros(DateTime theDate)
        {
            var response = true;
            var tempListOfIdLibros = new List<string>();
            for (int i = 0; i < idLibrosFinales.Count; i++)
            {
                if(fechasFilesFinales[i] == theDate)
                {
                    tempListOfIdLibros.Add(idLibrosFinales[i]);
                }
            }
            var lengthStart = tempListOfIdLibros.Count;
            var lengthEnd = tempListOfIdLibros.Distinct().Count();
            if (lengthStart != lengthEnd)
            {
                response = false;
            }
            return response;
        }
        private List<string> getIdLibrosOfDate(DateTime theDate)
        {
            var response = new List<string>();
            var tempListOfIdLibros = new List<string>();
            for (int i = 0; i < idLibrosFinales.Count; i++)
            {
                if (fechasFilesFinales[i] == theDate)
                {
                    tempListOfIdLibros.Add(idLibrosFinales[i]);
                }
            }
            response = tempListOfIdLibros.Distinct().ToList();
            return response;
        }
        
        private int getMinFraccionOfLibroAndDate(string theIdLibro, DateTime theDate)
        {
            var response = 99;
            var tempListOfIdLibros = new List<string>();
            for (int i = 0; i < fraccionesFinales.Count; i++)
            {
                var theFraccion = Int32.Parse(fraccionesFinales[i]);
                if (fechasFilesFinales[i] == theDate && idLibrosFinales[i] == theIdLibro && response > theFraccion)
                {
                    response = theFraccion;
                }
            }
            return response;
        }
        private int getRangeOfOrderOfFilesOfSameIdLAndDate(string elIdLibro, DateTime theDate, int minFraccion, int numberOfOrder)
        {
            var response = numberOfOrder;
            if(minFraccion == 0)
            {
                minFraccion += 1;
            }
            for (int k = 0; k < idLibrosFinales.Count; k++)
            {
                if (idLibrosFinales[k] == elIdLibro && fechasFilesFinales[k] == theDate)
                {
                    orderOfFiles.Add(response - minFraccion + 1);
                    response++;
                }
            }
            return response;
        }
        private void getOrderFiles()
        {
            orderOfFiles = new List<int>();
            var theDatesUniques = fechasFilesFinales.Distinct().OrderByDescending(date => date).ToList(); //It gets the Dates of the files and takes out the repetitions and orders by more recent to latest

            var numberOfOrder = 0;
            //starts to get the order of files
            for (int i = 0; i < theDatesUniques.Count; i++) //It loops first for the unique dates
            {
                    var idLibrosInDate = getIdLibrosOfDate(theDatesUniques[i]);
                    var minFraccionLibro = new List<int>(new int[idLibrosFinales.Count]);
                    //gets the min and max fracciones of each libro
                    for (int j = 0; j < idLibrosInDate.Count; j++)
                    {
                        minFraccionLibro.Add(getMinFraccionOfLibroAndDate(idLibrosInDate[j], theDatesUniques[i]));
                    }
                    for(int j = 0;j < idLibrosInDate.Count; j++)
                    {
                        numberOfOrder = getRangeOfOrderOfFilesOfSameIdLAndDate(idLibrosInDate[j], theDatesUniques[i], minFraccionLibro[j], numberOfOrder);
                    }
            }
        }

        public List<elementoFormulario> createElementos()
        {
            var theElementos = new List<elementoFormulario>();
            for(int i = 0; i < startingFilesPaths.Count; i++)
            {
                var oneElemento = new elementoFormulario();
                oneElemento.shortNamesPosible = new List<string>();
                oneElemento.fraccion = 0;
                oneElemento.terminacion = Path.GetExtension(startingFilesPaths[i]).ToLower();
                oneElemento.month = fechasFilesPosibles[i].Month;
                oneElemento.year = fechasFilesPosibles[i].Year;
                oneElemento.pathInicial = startingFilesPaths[i];
                oneElemento.shortName = losLibros[getIndexIdL(idLibrosPosibles[i])].nombreAL;
                foreach(Libro oneLibro in losLibros)
                {
                    oneElemento.shortNamesPosible.Add(oneLibro.nombreAL);
                }
                theElementos.Add(oneElemento);
            }
            return theElementos;
        }
        public string getLibroFromNameA(string name)
        {
            var response = "";
            foreach (Libro unLibro in losLibros)
            {
                if(unLibro.nombreAL == name)
                {
                    response = unLibro.idL; break;
                }
            }
            return response;
        }
        public void updateFormulario(List<string> theFracciones, List<string> losIdL, List<DateTime> theDates)
        {
            fechasFilesFinales = theDates;
            fraccionesFinales = theFracciones;
            idLibrosFinales = losIdL;
            getOrderFiles();
        }

        public void setIdPreParte(string numeroP)
        {
            numeroPreParte = numeroP;
        }
        public void setIdEmpresa(string idE)
        {
            idEmpresa = idE;
        }
        public void setLibros(List<Libro> libros)
        {
            losLibros = libros;
        }
        public void setStartingFiles(List<string> sFiles)
        {
            startingFilesPaths = sFiles;
        }
        public void setInicioE(string theStartE)
        {
            inicioE = theStartE;
        }
        public List<Libro> getLibros()
        {
            return losLibros;
        }
        public string getIdLibroFromLibros(int index)
        {
            return losLibros[index].idL;
        }
        public void setRegexUnLibro(Regex theRegex, int index)
        {
            losLibros[index].setRegexNombreLibro(theRegex);
        }
        public void setListRegexString(List<string> listStrings, int index)
        {
            losLibros[index].setProcessRegexString(listStrings);
        }
        public void setListInfo(List<string> listStrings, int index)
        {
            losLibros[index].setProcessInfo(listStrings);
        }
        public void getFechasPosibles()
        {
            fechasFilesPosibles = new List<DateTime>();
            for(int i = 0; i < startingFilesPaths.Count; i++)
            {
                DateTime laFecha = losLibros[getIndexIdL(idLibrosPosibles[i])].getFecha(startingFilesPaths[i]);
                fechasFilesPosibles.Add(laFecha);
            }
        }
        private int getIndexIdL(string idL)
        {
            int index = -1;
            for (int i = 0; i < losLibros.Count; i++)
            {
                if (losLibros[i].idL == idL)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        public void getIdLPosibles()
        {
            idLibrosPosibles = new List<string>();
            foreach(string startingFile in startingFilesPaths)
            {
                var extension = Path.GetExtension(startingFile).ToLower();
                int inicialSize = idLibrosPosibles.Count();
                foreach (Libro oneLibro in losLibros)
                {
                    var firstTenLines = new List<string>();    
                    if (extension == ".txt")
                    {
                        firstTenLines = getLineas(startingFile, 10);
                    }
                    else if (extension == ".pdf")
                    {
                        firstTenLines = getLineasPDF(startingFile, 10);
                    }
                    var esElLibro = oneLibro.checkLines(firstTenLines);
                    if (esElLibro) { idLibrosPosibles.Add(oneLibro.idL); break; }
                }
                if(idLibrosPosibles.Count == inicialSize)
                {
                    Errores.Add($"El archivo: {startingFile} no encontro idLibro posible.");
                }
            }
        }
        private int getIndexOfIdLAndDate(string idL, DateTime theDate)
        {
            var response = -1;

            for(int i = 0; i < idLibrosFinales.Count; i++)
            {
                if (idLibrosFinales[i] == idL && fechasFilesFinales[i] == theDate)
                {
                    response = i; break;
                }
            }
            return response;
        }
        public List<List<string>> getAllLibrosInOrder()
        {
            var response = new List<List<string>>();
            //starts to get the order of files
            int quantity = idLibrosFinales.Count;
            for (int i = 0; i < quantity; i++) //It loops first for the unique dates
            {
                int j = orderOfFiles.IndexOf(i);
                var miniResponse = new List<string>();
                miniResponse.Add(idLibrosFinales[j]);
                var month = fechasFilesFinales[j].Month.ToString();
                var year = fechasFilesFinales[j].Year.ToString();
                miniResponse.Add(month);
                miniResponse.Add(year);
                miniResponse.Add(fraccionesFinales[j]);
                miniResponse.Add(orderOfFiles[j].ToString());
                response.Add(miniResponse);
            }
            return response;
        }
        public List<List<string>> getFirstsLibros()
        {
            var theDatesUniques = fechasFilesFinales.Distinct().OrderByDescending(date => date).ToList(); //It gets the Dates of the files and takes out the repetitions and orders by more recent to latest
            var theIdLUniques = idLibrosFinales.Distinct().OrderByDescending(date => date).ToList(); //It gets the Dates of the files and takes out the repetitions and orders by more recent to latest
            var response = new List<List<string>>();
            //starts to get the order of files
            for (int i = 0; i < theIdLUniques.Count; i++) //It loops first for the unique dates
            {
                var miniResponse = new List<string>();
                miniResponse.Add(theIdLUniques[i]);
                for(int j = 0; j < theDatesUniques.Count; j++)
                {
                    int indexOfMatch = getIndexOfIdLAndDate(theIdLUniques[i], theDatesUniques[j]);
                    if(indexOfMatch != -1)
                    {
                        var month = fechasFilesFinales[indexOfMatch].Month.ToString();
                        var year = fechasFilesFinales[indexOfMatch].Year.ToString();
                        miniResponse.Add(month);
                        miniResponse.Add(year);
                        miniResponse.Add(fraccionesFinales[indexOfMatch]);
                        response.Add(miniResponse);
                        break;
                    }
                }
            }
            return response;

        }
        private List<string> getLineas(string direA, int cantidad)
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
        public void setPathsFinales()
        {
            pathsFinales = new List<string>();
            for(int i = 0; i < idLibrosFinales.Count; i++)
            {
                foreach(Libro oneLibro in losLibros)
                {
                    if (idLibrosFinales[i] == oneLibro.idL)
                    {
                        string finalNameFile = oneLibro.getNombreFile(fechasFilesFinales[i], fraccionesFinales[i], startingFilesPaths[i]);
                        pathsFinales.Add(finalNameFile);
                    }
                }
            }
        }
        public List<int> proccessOneLibro(int indexToProcess, string firstPageNumber, string firstEntryNumber)
        {
            var thefoliosAndEntries = new List<int>();
            foreach (Libro unLibro in losLibros)
            {
                if (idLibrosFinales[indexToProcess] == unLibro.idL)
                {
                    int pageNumber = Int32.Parse(firstPageNumber);
                    int entryNumber = Int32.Parse(firstEntryNumber);
                    thefoliosAndEntries = unLibro.proccesLibro(pageNumber, entryNumber, startingFilesPaths[indexToProcess], pathsFinales[indexToProcess]);
                    break;
                }
            }
            return thefoliosAndEntries;
        }
    }
}
