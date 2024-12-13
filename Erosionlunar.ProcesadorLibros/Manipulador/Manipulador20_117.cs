using Erosionlunar.ProcesadorLibros.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Manipulador
{
    public class Manipulador20_117:ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            return getDateTimeGeneric(6, dirA);
        }
    }
}
