using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.DB
{
    public class QueryDB
    {
        public string query { get; set; }
        public List<string> parameteres { get; set; }
        public List<string> values { get; set; }
    }
}
