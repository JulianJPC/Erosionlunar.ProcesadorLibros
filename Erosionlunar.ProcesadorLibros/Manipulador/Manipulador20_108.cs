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
    public class Manipulador20_108:ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            return getDateTimeGeneric(7, dirA);
        }
    }
}
