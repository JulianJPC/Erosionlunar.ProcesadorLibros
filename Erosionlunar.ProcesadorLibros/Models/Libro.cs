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

        public string getNombreFile(DateTime theDate, string theFraccion, string pathFolder)
        {
            return theManipulator.getNameFile(theDate, theFraccion, nombreArchivoL, pathFolder);
        }
        public DateTime getFecha(string pathFile)
        {
            return theManipulator.getFecha(pathFile);
        }
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
        public Libro(List<string> theInfo, string idE)
        {
            idLibro = theInfo[0];
            nombreL = theInfo[1];
            nombreArchivoL = theInfo[2];
            codificacion = theInfo[3];
            getManipulator(idE);
        }
        private void setEncoding()
        {
            theManipulator.setEncoding(codificacion);
        }
        public void setProcessRegexString(List<string> regexString)
        {
            theManipulator.setProcessRegexFromString(regexString);
        }
        public void setProcessInfo(List<string> infoStrings)
        {
            theManipulator.setProcessInfo(infoStrings);
            setEncoding();
        }
        public void setRegexNombreLibro(Regex elRegex)
        {
            regexNombreLibro = elRegex;
        }
        private void getManipulator(string idE)
        {
            string className = $"Erosionlunar.ProcesadorLibros.Manipulador.Manipulador{idE}_{idLibro}";
            Type type = Type.GetType(className);
            theManipulator = (ABSManipulador)Activator.CreateInstance(type);
        }
        public List<int> proccesLibro(int firstPageNumber, int firstEntry, string thePath, string finalPath)
        {
            Encoding theOpeningEncod = Encoding.GetEncoding(Int32.Parse(codificacion));
            return theManipulator.modFile(firstPageNumber, firstEntry, thePath, finalPath);
        }
    }
}
