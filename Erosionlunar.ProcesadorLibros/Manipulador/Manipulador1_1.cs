using Erosionlunar.ProcesadorLibros.Abstract;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Manipulador
{
    public class Manipulador1_1 : ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            return getDateTimeGeneric(2, dirA);
        }
        public override List<int> modFile(int startingPageNumber, int startingEntry, string startingPath, string finalPath)
        {
            var lastFolio = modFastGeneric(startingPageNumber, startingPath, finalPath);
            var lastEntry = 0;
            return new List<int> { lastFolio, lastEntry };
        }
    }
}
