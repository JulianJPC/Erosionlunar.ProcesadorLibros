using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models
{
    class Libro
    {
        private string idLibro;
        private string nombreL;
        private string nombreArchivoL;
        private string codificacion;

        public Libro(List<string> theInfo)
        {
            idLibro = theInfo[0];
            nombreL = theInfo[1];
            nombreArchivoL = theInfo[2];
            codificacion = theInfo[3];
        }
    }
}
