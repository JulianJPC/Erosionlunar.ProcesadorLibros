using Erosionlunar.ProcesadorLibros.Models.PPF;
using Erosionlunar.ProcesadorLibros.Models.windowPPP2;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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

        private List<fileLibro> startingFiles;
        private List<fileLibro> finalFiles;

        public string numeroP => numeroPreParte;
        public string idEmp => idEmpresa;
        public string inicioEjercicio => inicioE;
        public List<string> theErrors => Errores;
        

        public PreParteFormulario()
        {
            Errores = new List<string>();
            startingFiles = new List<fileLibro>();
            finalFiles = new List<fileLibro>();
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
            for(int i = 0; i < startingFiles.Count; i++)
            {
                var oneElemento = new elementoFormulario();
                oneElemento.shortNamesPosible = new List<string>();
                oneElemento.fraccion = 0;
                oneElemento.terminacion = Path.GetExtension(startingFiles[i].thePath).ToLower();
                oneElemento.month = startingFiles[i].date.Month;
                oneElemento.year = startingFiles[i].date.Year;
                oneElemento.pathInicial = startingFiles[i].thePath;
                oneElemento.shortName = losLibros[getIndexIdL(startingFiles[i].id)].nombreAL;
                foreach(Libro oneLibro in losLibros)
                {
                    oneElemento.shortNamesPosible.Add(oneLibro.nombreAL);
                }
                theElementos.Add(oneElemento);
            }
            return theElementos;
        }
        /// <summary>
        /// Takes elementosFormulario and extracts the information in it to create the finalFiles.
        /// </summary>
        /// <param name="elementosFormulario">List of elementoFormularios</param>
        public void updateFormulario(IEnumerable elementosFormulario)
        {
            foreach (elementoFormulario item in elementosFormulario)
            {
                var newFile = new fileLibro();
                var elIdLibro = getLibroFromNameA(item.shortName);
                newFile.id = elIdLibro;
                newFile.date = DateTime.ParseExact(String.Join('/', "01", addCeroMonth(item.month.ToString()), item.year.ToString()), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                newFile.fraccion = item.fraccion.ToString();
                finalFiles.Add(newFile);
            }
            finalFiles = theFileProcessor.getFinalOrderFiles(finalFiles);
            setPathsFinales();
        }
        /// <summary>
        /// Given the index, first page number and first entry number it process the file and return the last page number and last entry number.
        /// </summary>
        /// <param name="indexToProcess">Number of the index of file to process in the startingFilesPaths list.</param>
        /// <param name="firstPageNumber">First page number to use</param>
        /// <param name="firstEntryNumber">First entry number to use</param>
        public List<int> proccessOneLibro(int indexToProcess, int firstPageNumber, int firstEntryNumber)
        {
            var thefoliosAndEntries = new List<int>();
            foreach (Libro unLibro in losLibros)
            {
                if (finalFiles[indexToProcess].id == unLibro.idL)
                {
                    int pageNumber = firstPageNumber;
                    int entryNumber = firstEntryNumber;
                    thefoliosAndEntries = unLibro.processLibro(pageNumber, entryNumber, finalFiles[indexToProcess].thePath, finalFiles[indexToProcess].thePathFinal);
                    break;
                }
            }
            return thefoliosAndEntries;
        }

        /// ---------------------------- SETERS

        /// <summary>
        /// Sets value for losLibros.
        /// </summary>
        /// <param name="libros">List of Libro class.</param>
        public void setLibros(List<Libro> libros)
        {
            losLibros = libros;
        }
        /// <summary>
        /// Extract the information of PreParteBasic and gives it to the PreParteFormulario
        /// </summary>
        /// <param name="onePreParte">One PreParteBasic full of values.</param>
        public void setBasicPreParte(PreParteBasic onePreParte)
        {
            numeroPreParte = onePreParte.idPreParte;
            idEmpresa = onePreParte.idEmpresa;
            inicioE = onePreParte.startExcercise;
        }
        /// <summary>
        /// Sets value for startingFilesPaths.
        /// </summary>
        /// <param name="sFiles">List of paths of starting files.</param>
        public void setStartingFiles(List<string> sFiles)
        {
            foreach(string onePath in sFiles)
            {
                var newFile = new fileLibro();
                newFile.thePath = onePath;
                startingFiles.Add(newFile);
            }
            getIdLPosibles();
            getFechasPosibles();
        }
        /// <summary>
        /// Extract the information of basicInfoProcess and gives it to the libros
        /// </summary>
        /// <param name="theInfo">One basicInfoProcess full of values.</param>
        /// <param name="index">Index of the libro to set values</param>
        public void setBasicInfoProcess(basicInfoProcess theInfo, int index)
        {
            losLibros[index].setRegexNombreLibro(theInfo.libroRegex);
            losLibros[index].setProcessRegexString(theInfo.rawProcessRegex);
            losLibros[index].setProcessInfo(theInfo.infosForProcess);
        }
        /// <summary>
        /// Sets pathsFinales, it uses the libro to make the new name of the file
        /// </summary>
        private void setPathsFinales()
        {
            for (int i = 0; i < finalFiles.Count; i++)
            {
                foreach (Libro oneLibro in losLibros)
                {
                    if (finalFiles[i].id == oneLibro.idL)
                    {
                        string finalNameFile = oneLibro.calculateNombreFile(finalFiles[i].date, finalFiles[i].fraccion, startingFiles[i].thePath);
                        finalFiles[i].thePathFinal = finalNameFile;
                    }
                }
            }
        }
        /// <summary>
        /// Given the index, page number and entry number it gets the finalFile of that index
        /// and set the last entry and page number with those numbers.
        /// </summary>
        /// <param name="index">Index of a finalFile</param>
        /// <param name="lastEntryNumber">Last number of entry of the file</param>
        /// <param name="lastPageNumber">Last number of page number of the file</param>
        public void setLastNumbers(int lastPageNumber, int lastEntryNumber, int index)
        {
            finalFiles[index].pageNumberE = lastPageNumber;
            finalFiles[index].entryNumberE = lastEntryNumber;
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
        public int getNumberFiles()
        {
            return finalFiles.Count();
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
                if (unLibro.nombreAL == name)
                {
                    response = unLibro.idL; break;
                }
            }
            return response;
        }
        /// <summary>
        /// Given the index of a finalFile it it gets the id Libro, and the date and fraccion corresponding to the previous file.
        /// </summary>
        /// <param name="indexFile">Index of a finalFile</param>
        public List<string> getQueryFolio(int indexFile)
        {
            var response = new List<string>();
            var queryFraccion = finalFiles[indexFile].fraccion;
            var queryMonth = finalFiles[indexFile].date.Month.ToString();
            var queryYear = finalFiles[indexFile].date.Year.ToString();

            if ((queryFraccion == "0" || queryFraccion == "1") && queryMonth != inicioEjercicio) // If not start of excercice and fraccion is 0 or 1 it chanches the date to the previous month
            {
                var theDate = DateTime.ParseExact(String.Join('/', "01", addCeroMonth(queryMonth), queryYear), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                theDate = theDate.AddMonths(-1);
                queryMonth = theDate.Month.ToString();
                queryYear = theDate.Year.ToString();
            }
            else if (queryFraccion != "0" && queryFraccion != "1")
            {
                queryFraccion = (Int32.Parse(queryFraccion) - 1).ToString();
            }
            var queryDate = addCeroMonth(queryMonth) + queryYear.Substring(2, 2);

            return new List<string> { finalFiles[indexFile].id, queryDate, queryFraccion };
        }
        /// <summary>
        /// Sets the fechasFilesPosibles, it searcheas in the startingFilesPaths using the methods of each Libro.
        /// </summary>
        public void getFechasPosibles()
        {
            for (int i = 0; i < startingFiles.Count; i++)
            {
                Libro libroToUse = losLibros[getIndexIdL(startingFiles[i].id)];
                DateTime laFecha = libroToUse.getFecha(startingFiles[i].thePath);
                startingFiles[i].date = laFecha;
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
            foreach (fileLibro oneFile in startingFiles)
            {
                var firstTenLines = theFileProcessor.getLineas(oneFile.thePath, 10);
                foreach (Libro oneLibro in losLibros)
                {
                    var esElLibro = oneLibro.checkLines(firstTenLines);
                    if (esElLibro) { oneFile.id = oneLibro.idL; break; }
                }
            }
        }
        /// <summary>
        /// Given the index of a fileFinal it takes from the previous index the last page and entry numbers.
        /// </summary>
        /// <param name="index">Index of a finalFile</param>
        public List<int> getPreviousNumbers(int index)
        {
            var response = new List<int>();
            var previousIndex = index - 1;
            response.Add(finalFiles[previousIndex].pageNumberE);
            response.Add(finalFiles[previousIndex].entryNumberE);
            return response;
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
        /// <summary>
        /// Given the index of a finalFile gets the id libro of that file.
        /// </summary>
        /// <param name="index">Index of a finalFile</param>
        public string getIdLibroFile(int index)
        {
            return finalFiles[index].id;
        }
        /// <summary>
        /// Given the index of a finalFile gets the fraccion of that file.
        /// </summary>
        /// <param name="index">Index of a finalFile</param>
        public string getFraccionFile(int index)
        {
            return finalFiles[index].fraccion;
        }
        /// <summary>
        /// Given the index of a finalFile it calulates it MD5 hash.
        /// </summary>
        /// <param name="index">Index of a finalFile</param>
        public string getHashFile(int index)
        {
            return CalculateMD5(finalFiles[index].thePath);
        }
        /// <summary>
        /// Given the index of a finalFile gets the date of that file.
        /// </summary>
        /// <param name="index">Index of a finalFile</param>
        public DateTime getDateFile(int index)
        {
            return finalFiles[index].date;
        }

        /// <summary>
        /// Given the index of the finalFile it looks if the month of process is the same as inicioEjercicio.
        /// </summary>
        /// <param name="indexFile"></param>
        public bool isFileStartingMonth(int indexFile)
        {
            var response = false;
            var monthFile = finalFiles[indexFile].date.Month.ToString();
            if(monthFile == inicioEjercicio)
            {
                response = true;
            }
            return response;
        }
        
        /// <summary>
        /// Given the index of a fileFinal it looks if the file in the previous index is the last
        /// made of the same Id libro.
        /// </summary>
        /// <param name="index">Index of a finalFile</param>
        public bool isPrevious(int index)
        {
            var response = false;
            if(index != 0)
            {
                var previousIndex = index - 1;
                var fraccionNow = finalFiles[index].fraccion;
                var fraccionPrevious = finalFiles[previousIndex].fraccion;
                var dateNow = finalFiles[index].date;
                var datePrevious = finalFiles[previousIndex].date;
                var idLPrevious = finalFiles[previousIndex].id;
                var idLNow = finalFiles[index].id;
                if((fraccionNow == "0" || fraccionNow == "1") && dateNow.AddMonths(-1) == datePrevious && idLPrevious == idLNow)//If the previous is the month before
                {
                    response = true;
                }
                else if((fraccionNow != "0" && fraccionNow != "1") && dateNow == datePrevious && idLPrevious == idLNow && fraccionPrevious == (Int32.Parse(fraccionNow) - 1).ToString())// If the previous is the fraccion before
                {
                    response = true;
                }
            }
            return response;
        }
        
        /// <summary>
        /// Given a string with the number of the month it adds a 0 in front if its only one digit long.
        /// </summary>
        /// <param name="theMonth">String with the value of the number of a month</param>
        protected string addCeroMonth(string theMonth)
        {
            string response = theMonth;
            if (response.Length == 1)
            {
                response = "0" + response;
            }
            return response;
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
    }
}
