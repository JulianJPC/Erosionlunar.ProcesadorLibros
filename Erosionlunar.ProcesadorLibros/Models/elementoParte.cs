using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models
{
    public class elementoParte
    {
        public string IdArchivo { get; set; }
        public string IdLibro { get; set; }
        public string IdEmpresa { get; set; }
        public string pageNumberS { get; set; }
        public string pageNumberF { get; set; }
        public string fraccion { get; set; }
        public string hashF { get; set; }
        public string path { get; set; }
        public double size { get; set; }
        public int indexMO {  get; set; }
        public string NameMO { get; set; }
        public DateTime periodoMO { get; set; }
        public bool IsSelected { get; set; }
    }
}
