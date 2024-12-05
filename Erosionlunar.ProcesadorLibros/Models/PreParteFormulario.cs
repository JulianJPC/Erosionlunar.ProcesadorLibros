using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models
{
    class PreParteFormulario
    {
        private string numeroPreParte;
        private string idEmpresa;
        private List<Libro> losLibros;
        private List<string> startingFilesPaths;

        public string numeroP => numeroPreParte;
        public string idEmp => idEmpresa; 

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
    }
}
