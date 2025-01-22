using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models.PPF
{
    public class fileLibro
    {
        public string thePath { get; set; }
        public string thePathFinal { get; set; }
        public DateTime date { get; set; }
        public string id { get; set; }
        public string fraccion { get; set; }
        public int orderLibro { get; set; }
        public int orderDate { get; set; }
        public int orderScale { get; set; }
        public int pageNumberS { get; set; }
        public int pageNumberE { get; set; }
        public int entryNumberS { get; set; }
        public int entryNumberE { get; set; }
    }
}
