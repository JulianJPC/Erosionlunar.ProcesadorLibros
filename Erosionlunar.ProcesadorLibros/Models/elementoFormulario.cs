using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models
{
    public class elementoFormulario
    {
        public List<string> shortNamesPosible { get; set; }
        public string shortName { get; set; }
        public int month { get; set; }
        public int year { get; set; }  
        public int fraccion { get; set; }
        public string terminacion {  get; set; }
        public string pathInicial { get; set; }
    }
}
