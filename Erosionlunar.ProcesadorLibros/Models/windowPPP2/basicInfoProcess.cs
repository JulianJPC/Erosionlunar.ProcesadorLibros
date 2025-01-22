using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models.windowPPP2
{
    public class basicInfoProcess
    {   
        public Regex libroRegex {  get; set; }
        public List<string> rawProcessRegex { get; set; }
        public List<string> infosForProcess { get; set; }
    }
}
