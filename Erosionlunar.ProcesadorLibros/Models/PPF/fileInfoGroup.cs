using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models.PPF
{
    public class fileInfoGroup
    {
        public List<string> Paths { get; set; }
        public List<DateTime> Dates { get; set; }
        public List<string> Ids { get; set; }
        public List<string> Fracciones { get; set; }
        public List<int> Orders { get; set; }
        public fileInfoGroup()
        {
            Paths = new List<string>();
            Dates = new List<DateTime>();
            Ids = new List<string>();
            Fracciones = new List<string>();
            Orders = new List<int>();
        }
    }
}
