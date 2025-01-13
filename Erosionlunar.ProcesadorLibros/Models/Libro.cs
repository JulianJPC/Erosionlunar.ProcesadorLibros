using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using Erosionlunar.ProcesadorLibros.Manipulador;
using Erosionlunar.ProcesadorLibros.Abstract;
using Org.BouncyCastle.X509;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Erosionlunar.ProcesadorLibros.Models
{
    class Libro
    {
        private string idLibro;
        private string nombreL;
        private string nombreArchivoL;
        private string codificacion;
        private ABSManipulador theManipulator;
        private Regex regexNombreLibro;
        public string idL => idLibro;
        public string nombreAL => nombreArchivoL;
        public Libro(List<string> theInfo, string idE)
        {
            idLibro = theInfo[0];
            nombreL = theInfo[1];
            nombreArchivoL = theInfo[2];
            codificacion = theInfo[3];
            getManipulator(idE);
        }
        /// ---------------- USES MANIPULADOR

        /// <summary>
        /// Makes the full path of the file with a different name.
        /// </summary>
        /// <param name="theDate">Period of the Libro.</param>
        /// <param name="theFraccion">Division of the Libro.</param>
        /// <param name="pathFolder">Path to the file.</param>
        /// <remarks>
        /// Uses the Manipulador.
        /// </remarks>
        public string getNombreFile(DateTime theDate, string theFraccion, string pathFolder)
        {
            return theManipulator.makeNameFile(theDate, theFraccion, nombreArchivoL, pathFolder);
        }
        /// <summary>
        /// Gets the date of a libro file.
        /// </summary>
        /// <param name="pathFile">Path to the file.</param>
        /// <remarks>
        /// Uses the Manipulador.
        /// </remarks>
        public DateTime getFecha(string pathFile)
        {
            return theManipulator.getFecha(pathFile);
        }
        /// <summary>
        /// Sets the regex used in the Manipulador.
        /// </summary>
        /// <param name="regexString">Unprocess list of string used as regex.</param>
        /// <remarks>
        /// Uses the Manipulador.
        /// </remarks>
        public void setProcessRegexString(List<string> regexString)
        {
            theManipulator.setProcessRegexFromString(regexString);
        }
        /// <summary>
        /// Sets the information used in the Manipulador.
        /// </summary>
        /// <param name="infoStrings">Unprocess list of string used as information.</param>
        /// <remarks>
        /// Uses the Manipulador.
        /// </remarks>
        public void setProcessInfo(List<string> infoStrings)
        {
            theManipulator.setProcessInfo(infoStrings);
            theManipulator.setEncoding(codificacion);
        }
        /// <summary>
        /// Sets the Manipulador used. 
        /// </summary>
        /// <param name="idE">Id of the empresa.</param>
        private void getManipulator(string idE)
        {
            string className = $"Erosionlunar.ProcesadorLibros.Manipulador.Manipulador{idE}_{idLibro}";
            Type type = Type.GetType(className);
            theManipulator = (ABSManipulador)Activator.CreateInstance(type);
        }
        /// <summary>
        /// Modifys the libro file.
        /// </summary>
        /// <param name="firstPageNumber">Initial page number.</param>
        /// <param name="firstEntry">Initial entry number.</param>
        /// <param name="thePath">Initial path of the file.</param>
        /// <param name="finalPath">Modify path of the file.</param>
        /// <remarks>
        /// Uses the Manipulador.
        /// </remarks>
        public List<int> proccesLibro(int firstPageNumber, int firstEntry, string thePath, string finalPath)
        {
            return theManipulator.modFile(firstPageNumber, firstEntry, thePath, finalPath);
        }

        /// ------------------- Auxiliary Functions

        /// <summary>
        /// Giving a list of lines checks if anyone matches with the regexNombreLibro
        /// </summary>
        /// <param name="lines">List of strings of lines of a libro.</param>
        public bool checkLines(List<string> lines)
        {
            bool response = false;
            foreach (string unaLinea in lines)
            {
                if (regexNombreLibro.IsMatch(unaLinea))
                {
                    response = true; break;
                }
            }
            return response;
        }
        /// <summary>
        /// Sets the regexNombreLibro.
        /// </summary>
        /// <param name="elRegex">Regex used to match with the name of the book.</param>
        /// <remarks>
        public void setRegexNombreLibro(Regex elRegex)
        {
            regexNombreLibro = elRegex;
        }   
    }
}
