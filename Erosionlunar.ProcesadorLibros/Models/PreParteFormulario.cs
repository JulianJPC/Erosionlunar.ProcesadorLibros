using Erosionlunar.ProcesadorLibros.Models.PPF;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.RightsManagement;
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
        private fileProcessor theFileProcessor;
        private List<string> Errores;

        private fileInfoGroup startingFiles;
        private fileInfoGroup finalFiles;

        public string numeroP => numeroPreParte;
        public string idEmp => idEmpresa;
        public string inicioEjercicio => inicioE;
        public List<string> pathsFinal => finalFiles.Paths;
        public List<string> theErrors => Errores;
        

        public PreParteFormulario()
        {
            Errores = new List<string>();
            startingFiles = new fileInfoGroup();
            finalFiles = new fileInfoGroup();
            theFileProcessor = new fileProcessor();
        }
        /// <summary>
        /// Adds a new string of error to Errores
        /// </summary>
        /// <param name="theError">String of the error.</param>
        public void addError(string theError)
        {
            Errores.Add(theError);
        }

        /// -------------------------- Used in PreParteProcesos2

        /// <summary>
        /// Creates a list elementoForulario for every file in the formulario. 
        /// It gives a default value to fraccion equal to 0.
        /// </summary>
        /// <remarks>
        /// Used in PreParteProcesos2.
        /// </remarks>
        public List<elementoFormulario> createElementos()
        {
            var theElementos = new List<elementoFormulario>();
            for(int i = 0; i < startingFiles.Paths.Count; i++)
            {
                var oneElemento = new elementoFormulario();
                oneElemento.shortNamesPosible = new List<string>();
                oneElemento.fraccion = 0;
                oneElemento.terminacion = Path.GetExtension(startingFiles.Paths[i]).ToLower();
                oneElemento.month = startingFiles.Dates[i].Month;
                oneElemento.year = startingFiles.Dates[i].Year;
                oneElemento.pathInicial = startingFiles.Paths[i];
                oneElemento.shortName = losLibros[getIndexIdL(startingFiles.Ids[i])].nombreAL;
                foreach(Libro oneLibro in losLibros)
                {
                    oneElemento.shortNamesPosible.Add(oneLibro.nombreAL);
                }
                theElementos.Add(oneElemento);
            }
            return theElementos;
        }
        /// <summary>
        /// Set the fechasFilesFinales, fraccionesFinales and idLibrosFinalse with the giving parameters. Also it gets the order of the files.
        /// </summary>
        /// <param name="theFracciones">List of fracciones of libros</param>
        /// <param name="losIdL">List of Id libros</param>
        /// <param name="theDates">List of dates of libros</param>
        /// <remarks>
        /// Used in PreParteProcesos2.
        /// </remarks>
        public void updateFormulario(List<string> theFracciones, List<string> losIdL, List<DateTime> theDates)
        {
            finalFiles.Dates = theDates;
            finalFiles.Fracciones = theFracciones;
            finalFiles.Ids = losIdL;
            finalFiles.Orders = theFileProcessor.getFinalOrderFiles(finalFiles.Dates, finalFiles.Ids, finalFiles.Fracciones);
        }


        /// <summary>
        /// Giving the name of a libro it returns the Id Libro that correspond.
        /// </summary>
        /// <param name="name">Name of a libro.</param>
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

        /// ---------------------------- SETERS

        /// <summary>
        /// Sets value for numeroPreParte.
        /// </summary>
        /// <param name="numeroP">Number of PreParte.</param>
        public void setIdPreParte(string numeroP)
        {
            numeroPreParte = numeroP;
        }
        /// <summary>
        /// Sets value for IdEmpresa.
        /// </summary>
        /// <param name="idE">Number of Id Empresa.</param>
        public void setIdEmpresa(string idE)
        {
            idEmpresa = idE;
        }
        /// <summary>
        /// Sets value for losLibros.
        /// </summary>
        /// <param name="libros">List of Libro class.</param>
        public void setLibros(List<Libro> libros)
        {
            losLibros = libros;
        }

        /// <summary>
        /// Sets value for startingFilesPaths.
        /// </summary>
        /// <param name="sFiles">List of paths of starting files.</param>
        public void setStartingFiles(List<string> sFiles)
        {
            startingFiles.Paths = sFiles;
        }
        /// <summary>
        /// Sets value for inicioE.
        /// </summary>
        /// <param name="theStartE">Number of starting month of exercise.</param>
        public void setInicioE(string theStartE)
        {
            inicioE = theStartE;
        }
        /// <summary>
        /// Sets the value of regexNombreLibro of a libro giving it's index.
        /// </summary>
        /// <param name="theRegex">Regex of the nomrbe of the libro.</param>
        /// <param name="index">Number of index of the losLibros list.</param>
        public void setRegexUnLibro(Regex theRegex, int index)
        {
            losLibros[index].setRegexNombreLibro(theRegex);
        }
        /// <summary>
        /// Sets the value of the Regex used in the manipulator giving it's index of libro.
        /// </summary>
        /// <param name="listStrings">Strings of the regex used.</param>
        /// <param name="index">Number of index of the losLibros list.</param>
        public void setListRegexString(List<string> listStrings, int index)
        {
            losLibros[index].setProcessRegexString(listStrings);
        }
        /// <summary>
        /// Sets the value of the strings used in the manipulator giving it's index of libro.
        /// </summary>
        /// <param name="listStrings">Strings used in the manipulator.</param>
        /// <param name="index">Number of index of the losLibros list.</param>
        public void setListInfo(List<string> listStrings, int index)
        {
            losLibros[index].setProcessInfo(listStrings);
        }
        /// <summary>
        /// Sets pathsFinales, it uses the libro to make the new name of the file
        /// </summary>
        public void setPathsFinales()
        {
            finalFiles.Paths = new List<string>();
            for (int i = 0; i < finalFiles.Ids.Count; i++)
            {
                foreach (Libro oneLibro in losLibros)
                {
                    if (finalFiles.Ids[i] == oneLibro.idL)
                    {
                        string finalNameFile = oneLibro.getNombreFile(finalFiles.Dates[i], finalFiles.Fracciones[i], startingFiles.Paths[i]);
                        finalFiles.Paths.Add(finalNameFile);
                    }
                }
            }
        }

        /// ------------------ GETERS

        /// <summary>
        /// Gets the libros in losLibros.
        /// </summary>
        public List<Libro> getLibros()
        {
            return losLibros;
        }
        /// <summary>
        /// Gets the Id Libro of a libro giving an index.
        /// </summary>
        /// <param name="index">Number of index of the losLibros list.</param>
        public string getIdLibroFromLibros(int index)
        {
            return losLibros[index].idL;
        }
        /// <summary>
        /// Returns a List o the basic information of a file.
        /// It returns its Id libro, month, year, fraccion and order of process.
        /// </summary>
        public List<List<string>> getAllLibrosInOrder()
        {
            var response = new List<List<string>>();
            //starts to get the order of files
            int quantity = finalFiles.Ids.Count;
            for (int i = 1; i <= quantity; i++) //It loops first for the unique dates
            {
                int j = finalFiles.Orders.IndexOf(i);
                var miniResponse = new List<string>();
                miniResponse.Add(finalFiles.Ids[j]);
                var month = finalFiles.Dates[j].Month.ToString();
                var year = finalFiles.Dates[j].Year.ToString();
                miniResponse.Add(month);
                miniResponse.Add(year);
                miniResponse.Add(finalFiles.Fracciones[j]);
                miniResponse.Add(finalFiles.Orders[j].ToString());
                response.Add(miniResponse);
            }
            return response;
        }

        /// --------------------- Get POSIBLES


        /// <summary>
        /// Sets the fechasFilesPosibles, it searcheas in the startingFilesPaths using the methods of each Libro.
        /// </summary>
        public void getFechasPosibles()
        {
            startingFiles.Dates = new List<DateTime>();
            for(int i = 0; i < startingFiles.Paths.Count; i++)
            {
                Libro libroToUse = losLibros[getIndexIdL(startingFiles.Ids[i])];
                DateTime laFecha = libroToUse.getFecha(startingFiles.Paths[i]);
                startingFiles.Dates.Add(laFecha);
            }
        }
        
        /// <summary>
        /// Sets the idLibrosPosibles, it loops to every path in startingFilesPaths
        /// and then takes the starting lines of the files.
        /// Having the starting lines it loops through the libros to check if
        /// the starting lines correspond to a libro.
        /// If it matches it adds it to startingFiles Ids
        /// </summary>
        public void getIdLPosibles()
        {
            startingFiles.Ids = new List<string>();
            foreach (string startingFile in startingFiles.Paths)
            {
                var firstTenLines = theFileProcessor.getLineas(startingFile, 10);
                foreach (Libro oneLibro in losLibros)
                {
                    var esElLibro = oneLibro.checkLines(firstTenLines);
                    if (esElLibro) { startingFiles.Ids.Add(oneLibro.idL); break; }
                }
            }
        }
        /// <summary>
        /// Gets a list of basic information of only one libro of a giving date and id Libro.
        /// Each list contains the Id libro, month, year and fraccion.
        /// </summary>
        public List<List<string>> getFirstsLibros()
        {
            var theDatesUniques = finalFiles.Dates.Distinct().OrderByDescending(date => date).ToList(); //It gets the Dates of the files and takes out the repetitions and orders by more recent to latest
            var theIdLUniques = finalFiles.Ids.Distinct().OrderByDescending(date => date).ToList(); //It gets the Dates of the files and takes out the repetitions and orders by more recent to latest
            var response = new List<List<string>>();
            //starts to get the order of files
            for (int i = 0; i < theIdLUniques.Count; i++) //It loops first for the unique dates
            {
                var miniResponse = new List<string>();
                miniResponse.Add(theIdLUniques[i]);
                for(int j = 0; j < theDatesUniques.Count; j++)
                {
                    int indexOfMatch = getIndexOfIdLAndDateMinFraccion(theIdLUniques[i], theDatesUniques[j]);
                    if(indexOfMatch != -1)
                    {
                        var month = finalFiles.Dates[indexOfMatch].Month.ToString();
                        var year = finalFiles.Dates[indexOfMatch].Year.ToString();
                        miniResponse.Add(month);
                        miniResponse.Add(year);
                        miniResponse.Add(finalFiles.Fracciones[indexOfMatch]);
                        response.Add(miniResponse);
                        break;
                    }
                }
            }
            return response;
        }
        /// <summary>
        /// Giving a Id Libro and a date it loops by the same index in idLibrosFinales and fechasFilesFinales.
        /// First it gets the minimum number of fraccion of the same index. Then
        /// loops through to get the index of the minimum fraccion, date and id Libro. Finally returns that index.
        /// </summary>
        /// <param name="idL">String of the number of a Id Libro.</param>
        /// <param name="theDate">Date of a file</param>
        private int getIndexOfIdLAndDateMinFraccion(string idL, DateTime theDate)
        {
            var response = -1;
            var minFraccion = 999;
            for (int i = 0; i < finalFiles.Ids.Count; i++)
            {
                if (finalFiles.Ids[i] == idL && finalFiles.Dates[i] == theDate)
                {
                    if (Int32.Parse(finalFiles.Fracciones[i]) < minFraccion)
                    {
                        minFraccion = Int32.Parse(finalFiles.Fracciones[i]);
                    }
                }
            }

            for (int i = 0; i < finalFiles.Ids.Count; i++)
            {
                if (finalFiles.Ids[i] == idL && finalFiles.Dates[i] == theDate && finalFiles.Fracciones[i] == minFraccion.ToString())
                {
                    response = i; break;
                }
            }
            return response;
        }

        /// <summary>
        /// Given the index, first page number and first entry number it process the file and return the last page number and last entry number.
        /// </summary>
        /// <param name="indexToProcess">Number of the index of file to process in the startingFilesPaths list.</param>
        /// <param name="firstPageNumber">First page number to use</param>
        /// <param name="firstEntryNumber">First entry number to use</param>
        public List<int> proccessOneLibro(int indexToProcess, string firstPageNumber, string firstEntryNumber)
        {
            var thefoliosAndEntries = new List<int>();
            foreach (Libro unLibro in losLibros)
            {
                if (finalFiles.Ids[indexToProcess] == unLibro.idL)
                {
                    int pageNumber = Int32.Parse(firstPageNumber);
                    int entryNumber = Int32.Parse(firstEntryNumber);
                    thefoliosAndEntries = unLibro.proccesLibro(pageNumber, entryNumber, startingFiles.Paths[indexToProcess], finalFiles.Paths[indexToProcess]);
                    break;
                }
            }
            return thefoliosAndEntries;
        }
        /// <summary>
        /// Gets the index of the losLibros wich mathes the Id Libro giving.
        /// </summary>
        /// <param name="idL">String of a Id Libro.</param>
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
        
    }
}
