using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models.windowF
{
    public class Archivo
    {
        public string id {  get; set; }
        public string idLibro { get; set; }
        public string hashA { get; set; }
        public string pageNumberS { get; set; }
        public string pageNumberE { get; set; }
        public string entryNumberS { get; set; }
        public string entryNumberE { get; set; }
        public string fraccion {  get; set; }
        public string pathArchivo {  get; set; }

    }
}
